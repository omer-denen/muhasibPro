using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Data.Contracts.Repository.SistemRepos.Authentication;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

/// <summary>Faz 5 M2-politika: KullaniciService (repo sarmalar) + DI.</summary>
public class KullaniciServiceTests
{
    private sealed class BellekAyarlari : ILocalSettingsService
    {
        public readonly Dictionary<string, object> Kutu = new();
        public Task<T?> ReadSettingAsync<T>(string key)
        {
            if (Kutu.TryGetValue(key, out var raw) && raw is T deger)
                return Task.FromResult<T?>(deger);
            return Task.FromResult<T?>(default);
        }
        public Task SaveSettingAsync<T>(string key, T value)
        {
            Kutu[key] = value!;
            return Task.CompletedTask;
        }
    }

    private static IAuthenticationService Kimlik(long id, KullaniciRolTip? rol)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(rol.HasValue);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(rol.HasValue ? id : -1);
        auth.SetupGet(a => a.CurrentAccount).Returns(rol.HasValue
            ? new HesapModel
            {
                KullaniciId = id,
                KullaniciModel = new KullaniciModel { Rol = new KullaniciRolModel { RolTip = rol.Value } }
            }
            : null!);
        return auth.Object;
    }

    private static Kullanici Kayit(long id = 10) => new()
    {
        Id = id,
        KullaniciAdi = "testci",
        Adi = "Test",
        Soyadi = "Kullanici",
        Eposta = "test@ornek.com",
        ParolaHash = "eski-hash",
        AktifMi = true,
        KayitTarihi = DateTime.Now
    };

    private static KullaniciService KurServis(
        Mock<IUserRepository> repo,
        IAuthenticationService auth,
        IIdentitySettingsProvider kimlikAyar = null!,
        Mock<IKullaniciFirmaRolRepository> kfr = null!,
        Mock<IKullaniciRolRepository> rol = null!)
    {
        var uow = new Mock<IUnitOfWork<SistemDbContext>>();
        uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        return new KullaniciService(
            repo.Object,
            (kfr ?? new Mock<IKullaniciFirmaRolRepository>()).Object,
            (rol ?? new Mock<IKullaniciRolRepository>()).Object,
            uow.Object, auth,
            new PasswordHasher<Kullanici>(), kimlikAyar);
    }

    [Fact]
    public async Task GetKullanici_Bulundu_Bulunamadi()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(Kayit());
        repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Kullanici)null!);
        var svc = KurServis(repo, Kimlik(1, KullaniciRolTip.Yönetici));

        (await svc.GetKullaniciAsync(10)).Success.Should().BeTrue();
        (await svc.GetKullaniciAsync(99)).Success.Should().BeFalse();
    }

    [Fact]
    public async Task Update_Girissiz_Reddedilir()
    {
        var repo = new Mock<IUserRepository>();
        var svc = KurServis(repo, Kimlik(0, null));

        var sonuc = await svc.UpdateKullaniciAsync(new KullaniciModel { Id = 10 });

        sonuc.Success.Should().BeFalse();
        repo.Verify(r => r.UpdateAsync(It.IsAny<Kullanici>()), Times.Never);
    }

    [Fact]
    public async Task Update_Alanlari_Isler_Guncelleyeni_Yazar()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(Kayit());
        var svc = KurServis(repo, Kimlik(5, KullaniciRolTip.Yönetici));

        var sonuc = await svc.UpdateKullaniciAsync(new KullaniciModel { Id = 10, Adi = "Yeni", AktifMi = false });

        sonuc.Success.Should().BeTrue();
        repo.Verify(r => r.UpdateAsync(It.Is<Kullanici>(k =>
            k.Adi == "Yeni" && !k.AktifMi && k.GuncelleyenId == 5)), Times.Once);
    }

    [Fact]
    public async Task SetAktif_SeedYonetici_PasifeAlinamaz()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(MuhasibPro.Domain.Entities.KullaniciSabitleri.SeedYoneticiId))
            .ReturnsAsync(Kayit(MuhasibPro.Domain.Entities.KullaniciSabitleri.SeedYoneticiId));
        var svc = KurServis(repo, Kimlik(5, KullaniciRolTip.Yönetici));

        var sonuc = await svc.SetAktifAsync(MuhasibPro.Domain.Entities.KullaniciSabitleri.SeedYoneticiId, false);

        sonuc.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Sifre_Kisa_Reddedilir_Uzun_Hashlenir()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(Kayit());
        var svc = KurServis(repo, Kimlik(10, KullaniciRolTip.Kullanici));

        (await svc.SifreBelirleAsync(10, "123")).Success.Should().BeFalse();

        var sonuc = await svc.SifreBelirleAsync(10, "yeni-gizli-123");
        sonuc.Success.Should().BeTrue();
        var dogrulayici = new PasswordHasher<Kullanici>();
        repo.Verify(r => r.UpdateAsync(It.Is<Kullanici>(k =>
            dogrulayici.VerifyHashedPassword(k, k.ParolaHash, "yeni-gizli-123")
                == PasswordVerificationResult.Success)), Times.Once);
    }

    [Fact]
    public async Task Sifre_Baskasinin_NormalKullanici_Reddedilir_Yonetici_Ok()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(Kayit());
        var normal = KurServis(repo, Kimlik(11, KullaniciRolTip.Kullanici));
        var yonetici = KurServis(repo, Kimlik(11, KullaniciRolTip.Yönetici));

        (await normal.SifreBelirleAsync(10, "yeni-gizli-123")).Success.Should().BeFalse();
        (await yonetici.SifreBelirleAsync(10, "yeni-gizli-123")).Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Kendi_Ve_Seed_Korumali()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(Kayit());
        var svc = KurServis(repo, Kimlik(10, KullaniciRolTip.Yönetici));

        (await svc.DeleteKullaniciAsync(10)).Success.Should().BeFalse();
        (await svc.DeleteKullaniciAsync(MuhasibPro.Domain.Entities.KullaniciSabitleri.SeedYoneticiId)).Success.Should().BeFalse();
        repo.Verify(r => r.DeleteAsync(It.IsAny<Kullanici>()), Times.Never);
    }

    [Fact]
    public async Task Create_NormalKullanici_Reddedilir_Yonetici_Olusturur()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByUsernameAsync("yeni")).ReturnsAsync((Kullanici)null!);
        var kfr = new Mock<IKullaniciFirmaRolRepository>();
        kfr.Setup(r => r.FindAsync(It.IsAny<long>(), 7)).ReturnsAsync((KullaniciFirmaRol)null!);
        var rol = new Mock<IKullaniciRolRepository>();
        rol.Setup(r => r.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new KullaniciRol { Id = KullaniciRolSabitleri.YoneticiRolId, RolTip = KullaniciRolTip.Yönetici });

        var normal = KurServis(repo, Kimlik(11, KullaniciRolTip.Kullanici), null!, kfr, rol);
        (await normal.CreateKullaniciAsync(new KullaniciModel { KullaniciAdi = "yeni", Adi = "Y" },
            "gizli-123", 7, KullaniciRolSabitleri.KullaniciRolId)).Success.Should().BeFalse();
        repo.Verify(r => r.AddAsync(It.IsAny<Kullanici>()), Times.Never);

        var yonetici = KurServis(repo, Kimlik(5, KullaniciRolTip.Yönetici), null!, kfr, rol);
        var sonuc = await yonetici.CreateKullaniciAsync(
            new KullaniciModel { KullaniciAdi = "yeni", Adi = "Y", Soyadi = "K" },
            "gizli-123", 7, KullaniciRolSabitleri.YoneticiRolId);

        sonuc.Success.Should().BeTrue();
        var dogrulayici = new PasswordHasher<Kullanici>();
        repo.Verify(r => r.AddAsync(It.Is<Kullanici>(k =>
            k.KullaniciAdi == "yeni" && k.Adi == "Y" && k.KaydedenId == 5 && k.AktifMi
            && dogrulayici.VerifyHashedPassword(k, k.ParolaHash, "gizli-123") == PasswordVerificationResult.Success)), Times.Once);
        kfr.Verify(r => r.AddAsync(It.Is<KullaniciFirmaRol>(x =>
            x.FirmaId == 7 && x.RolId == KullaniciRolSabitleri.YoneticiRolId)), Times.Once);
    }

    [Fact]
    public async Task Create_Duplicate_KullaniciAdi_Ve_KisaSifre_Reddedilir()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByUsernameAsync("testci")).ReturnsAsync(Kayit());
        var svc = KurServis(repo, Kimlik(5, KullaniciRolTip.Yönetici));

        (await svc.CreateKullaniciAsync(new KullaniciModel { KullaniciAdi = "testci", Adi = "A" },
            "gizli-123", 7, KullaniciRolSabitleri.KullaniciRolId)).Success.Should().BeFalse();

        var repo2 = new Mock<IUserRepository>();
        repo2.Setup(r => r.GetByUsernameAsync("yeni")).ReturnsAsync((Kullanici)null!);
        var svc2 = KurServis(repo2, Kimlik(5, KullaniciRolTip.Yönetici));
        (await svc2.CreateKullaniciAsync(new KullaniciModel { KullaniciAdi = "yeni", Adi = "A" },
            "123", 7, KullaniciRolSabitleri.KullaniciRolId)).Success.Should().BeFalse();
    }

    [Fact]
    public async Task RolAta_Guard_Ve_Kfr_Gunceller()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(Kayit());
        var rol = new Mock<IKullaniciRolRepository>();
        rol.Setup(r => r.GetByIdAsync(KullaniciRolSabitleri.YoneticiRolId))
            .ReturnsAsync(new KullaniciRol { Id = KullaniciRolSabitleri.YoneticiRolId, RolTip = KullaniciRolTip.Yönetici });
        var mevcutKfr = new KullaniciFirmaRol { KullaniciId = 10, FirmaId = 7, RolId = KullaniciRolSabitleri.KullaniciRolId };
        var kfr = new Mock<IKullaniciFirmaRolRepository>();
        kfr.Setup(r => r.FindAsync(10, 7)).ReturnsAsync(mevcutKfr);

        var normal = KurServis(repo, Kimlik(11, KullaniciRolTip.Kullanici), null!, kfr, rol);
        (await normal.RolAtaAsync(10, 7, KullaniciRolSabitleri.YoneticiRolId)).Success.Should().BeFalse();

        var yonetici = KurServis(repo, Kimlik(5, KullaniciRolTip.Yönetici), null!, kfr, rol);
        (await yonetici.RolAtaAsync(10, 7, KullaniciRolSabitleri.YoneticiRolId)).Success.Should().BeTrue();
        mevcutKfr.RolId.Should().Be(KullaniciRolSabitleri.YoneticiRolId);
        kfr.Verify(r => r.AddAsync(It.IsAny<KullaniciFirmaRol>()), Times.Never);

        (await yonetici.RolAtaAsync(10, 7, 999999)).Success.Should().BeFalse();
    }

    [Fact]
    public void Servisler_DI_Scoped_Kayitli()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (File.Exists(Path.Combine(dir, "MuhasibPro.slnx")) || File.Exists(Path.Combine(dir, "MuhasibPro.sln"))) break;
            dir = Path.GetDirectoryName(dir)!;
        }
        var src = File.ReadAllText(Path.Combine(dir,
            "Libraries/MuhasibPro.Business/HostBuilder/AddServicesHostBuilderExtensions.cs"));
        src.Should().Contain("AddScoped<IKullaniciService, KullaniciService>");
        src.Should().Contain("AddScoped<ILisansService, LisansService>");
    }
}
