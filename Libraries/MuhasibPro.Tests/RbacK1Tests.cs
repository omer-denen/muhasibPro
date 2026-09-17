using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Data.Database.SistemDatabase;
using MuhasibPro.Data.Repository.Common.BaseRepo;
using MuhasibPro.Data.Repository.SistemRepos;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.85 K1 — RBAC seed/atama temeli: RolPermission HasData, KFR yazımı,
/// idempotent backfill + PermissionService Yönetici bypass'ı.</summary>
public class RbacK1Tests
{
    private static (SistemDbContext ctx, SqliteConnection keepAlive) CreateContext()
    {
        var cs = $"Data Source=rbac{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var keepAlive = new SqliteConnection(cs);
        keepAlive.Open();
        var options = new DbContextOptionsBuilder<SistemDbContext>().UseSqlite(cs).Options;
        return (new SistemDbContext(options), keepAlive);
    }

    private static Firma YeniFirma(long id, long kaydedenId) => new()
    {
        Id = id,
        FirmaKodu = $"F-{id % 10000:0000}",
        KisaUnvani = "Test Firma",
        TamUnvani = "Test Firma Ltd.",
        YetkiliKisi = "Yetkili",
        PBu1 = "TL",
        KaydedenId = kaydedenId,
        KayitTarihi = DateTime.UtcNow,
        AktifMi = true
    };

    private static PermissionService KurPermissionService(
        SistemDbContext ctx, long kullaniciId, long firmaId)
    {
        var kfrRepo = new KullaniciFirmaRolRepository(ctx);
        var rolPermRepo = new RolPermissionRepository(ctx);
        var secim = new Mock<IFirmaWithMaliDonemSelectedService>();
        secim.SetupGet(s => s.SelectedFirma).Returns(new FirmaModel { Id = firmaId });
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(kullaniciId);
        return new PermissionService(kfrRepo, rolPermRepo, secim.Object, auth.Object);
    }

    [Fact]
    public async Task Seed_Yonetici_TumIzinler_Kullanici_VarsayilanSet()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();

            var yonetici = await ctx.RolPermissionlar
                .Where(x => x.RolId == KullaniciRolSabitleri.YoneticiRolId)
                .Select(x => x.PermissionId).ToListAsync();
            var kullanici = await ctx.RolPermissionlar
                .Where(x => x.RolId == KullaniciRolSabitleri.KullaniciRolId)
                .Select(x => x.PermissionId).ToListAsync();

            yonetici.Should().BeEquivalentTo(PermissionVarsayilanlari.TumIzinler);
            kullanici.Should().BeEquivalentTo(PermissionVarsayilanlari.Kullanici);
            kullanici.Should().NotContain(Permission.Cari_Ekle);
            kullanici.Should().NotContain(Permission.Veritabani_Goruntule);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task PermissionService_YoneticiKfr_TumIzinleri_Gecerli()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            const long firmaId = 7001;
            ctx.Firmalar.Add(YeniFirma(firmaId, KullaniciSabitleri.SeedYoneticiId));
            ctx.KullaniciFirmaRoller.Add(new KullaniciFirmaRol
            {
                KullaniciId = KullaniciSabitleri.SeedYoneticiId,
                FirmaId = firmaId,
                RolId = KullaniciRolSabitleri.YoneticiRolId
            });
            await ctx.SaveChangesAsync();

            var svc = KurPermissionService(ctx, KullaniciSabitleri.SeedYoneticiId, firmaId);

            (await svc.HasPermissionAsync(Permission.Cari_Goruntule)).Should().BeTrue();
            (await svc.HasPermissionAsync(Permission.Veritabani_KaliciSil)).Should().BeTrue();
            (await svc.HasPermissionAsync(Permission.Kullanici_Yonet)).Should().BeTrue();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task PermissionService_YoneticiRolu_Bypass_RolPermissionSorgulamaz()
    {
        var kfr = new Mock<IKullaniciFirmaRolRepository>();
        kfr.Setup(r => r.GetByKullaniciIdAsync(5)).ReturnsAsync(new List<KullaniciFirmaRol>
        {
            new()
            {
                KullaniciId = 5,
                FirmaId = 7,
                RolId = KullaniciRolSabitleri.YoneticiRolId,
                Rol = new KullaniciRol { Id = KullaniciRolSabitleri.YoneticiRolId, RolTip = KullaniciRolTip.Yönetici }
            }
        });
        var rp = new Mock<IRolPermissionRepository>();
        rp.Setup(r => r.GetByRolIdAsync(It.IsAny<long>())).ReturnsAsync(new List<RolPermission>());
        var secim = new Mock<IFirmaWithMaliDonemSelectedService>();
        secim.SetupGet(s => s.SelectedFirma).Returns(new FirmaModel { Id = 7 });
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(5);

        var svc = new PermissionService(kfr.Object, rp.Object, secim.Object, auth.Object);

        (await svc.HasPermissionAsync(5, 7, Permission.Veritabani_Sil)).Should().BeTrue();
        rp.Verify(r => r.GetByRolIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task PermissionService_KullaniciKfr_Sadece_VarsayilanSet()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            const long firmaId = 7002;
            const long kullaniciId = 7003;
            ctx.Kullanicilar.Add(new Kullanici
            {
                Id = kullaniciId, KullaniciAdi = "testkullanici", ParolaHash = "x",
                Adi = "Test", Soyadi = "Kullanici", AktifMi = true,
                KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = DateTime.UtcNow
            });
            ctx.Firmalar.Add(YeniFirma(firmaId, KullaniciSabitleri.SeedYoneticiId));
            ctx.KullaniciFirmaRoller.Add(new KullaniciFirmaRol
            {
                KullaniciId = kullaniciId, FirmaId = firmaId, RolId = KullaniciRolSabitleri.KullaniciRolId
            });
            await ctx.SaveChangesAsync();

            var svc = KurPermissionService(ctx, kullaniciId, firmaId);

            (await svc.HasPermissionAsync(Permission.Cari_Goruntule)).Should().BeTrue();
            (await svc.HasPermissionAsync(Permission.AiAsistan_Kullan)).Should().BeTrue();
            (await svc.HasPermissionAsync(Permission.Cari_Ekle)).Should().BeFalse();
            (await svc.HasPermissionAsync(Permission.Kullanici_Yonet)).Should().BeFalse();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task Backfill_KfrsizFirmaya_YoneticiKfr_Ekler_Idempotent()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            ctx.Firmalar.Add(YeniFirma(7101, KullaniciSabitleri.SeedYoneticiId));
            ctx.Firmalar.Add(YeniFirma(7102, KullaniciSabitleri.SeedYoneticiId));
            await ctx.SaveChangesAsync();

            var eklenen = await SistemRbacBackfill.EnsureAsync(ctx);
            eklenen.Should().Be(2);

            var kfrler = await ctx.KullaniciFirmaRoller.ToListAsync();
            kfrler.Should().HaveCount(2);
            kfrler.Should().OnlyContain(k =>
                k.KullaniciId == KullaniciSabitleri.SeedYoneticiId
                && k.RolId == KullaniciRolSabitleri.YoneticiRolId);

            (await SistemRbacBackfill.EnsureAsync(ctx)).Should().Be(0);
            (await ctx.KullaniciFirmaRoller.CountAsync()).Should().Be(2);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task FirmaKayit_Firmayi_Olusturana_YoneticiKfr_Yazar()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            const long olusturanId = 7301;
            ctx.Kullanicilar.Add(new Kullanici
            {
                Id = olusturanId, KullaniciAdi = "olusturan", ParolaHash = "x",
                Adi = "Olusturan", Soyadi = "Kullanici", AktifMi = true,
                KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();

            var auth = new Mock<IAuthenticationService>();
            auth.SetupGet(a => a.IsAuthenticated).Returns(true);
            auth.SetupGet(a => a.GetCurrentUserId).Returns(olusturanId);

            var log = new Mock<ILogService>();
            log.SetupGet(l => l.SistemLogService).Returns(new Mock<ISistemLogService>().Object);

            var ayarlar = new Mock<IEntityRegistrySettingsProvider>();
            ayarlar.Setup(a => a.GetAsync(It.IsAny<long>()))
                   .ReturnsAsync(new EntityRegistrySettings { ValidationStrict = false });

            var svc = new FirmaKayitService(
                new FirmaRepository(ctx),
                new KullaniciFirmaRolRepository(ctx),
                new UnitOfWork<SistemDbContext>(ctx),
                log.Object,
                auth.Object,
                new Mock<IBitmapToolsService>().Object,
                ayarlar.Object);

            var sonuc = await svc.UpdateFirmaAsync(new FirmaModel
            {
                Id = 0,
                KisaUnvani = "Yeni Firma",
                TamUnvani = "Yeni Firma Ltd.",
                YetkiliKisi = "Yetkili",
                PBu1 = "TL"
            });

            sonuc.Success.Should().BeTrue();
            var firma = await ctx.Firmalar.SingleAsync();
            var kfr = await ctx.KullaniciFirmaRoller.SingleAsync();
            kfr.KullaniciId.Should().Be(olusturanId);
            kfr.FirmaId.Should().Be(firma.Id);
            kfr.RolId.Should().Be(KullaniciRolSabitleri.YoneticiRolId);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }
}
