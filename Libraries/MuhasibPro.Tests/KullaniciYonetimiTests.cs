using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.85 K2: kullanıcı yönetimi formu + pencere orkestratörü.</summary>
public class KullaniciYonetimiTests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(Mock.Of<IContextService>());
        ortak.SetupGet(o => o.DialogService).Returns(Mock.Of<IDialogService>());
        return ortak;
    }

    private static IAuthenticationService Kimlik(KullaniciRolTip? rolTip, long id = 5)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(rolTip.HasValue);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(rolTip.HasValue ? id : -1);
        auth.SetupGet(a => a.CurrentAccount).Returns(rolTip.HasValue
            ? new HesapModel
            {
                KullaniciId = id,
                KullaniciModel = new KullaniciModel
                {
                    Rol = new KullaniciRolModel { RolTip = rolTip.Value }
                }
            }
            : null!);
        return auth.Object;
    }

    private static KullaniciRolModel Rol(KullaniciRolTip tip) => new()
    {
        Id = tip == KullaniciRolTip.Yönetici ? KullaniciRolSabitleri.YoneticiRolId : KullaniciRolSabitleri.KullaniciRolId,
        RolAdi = tip == KullaniciRolTip.Yönetici ? "Yönetici" : "Kullanıcı",
        RolTip = tip
    };

    private static Mock<IFirmaWithMaliDonemSelectedService> Secim(long firmaId = 7)
    {
        var secim = new Mock<IFirmaWithMaliDonemSelectedService>();
        secim.SetupGet(s => s.SelectedFirma).Returns(new FirmaModel { Id = firmaId, KisaUnvani = "Test Firma" });
        return secim;
    }

    // ── Form VM ─────────────────────────────────────────────────────────────

    private static KullaniciDuzenleViewModel Form(out Mock<IKullaniciService> servis)
    {
        servis = new Mock<IKullaniciService>();
        var vm = new KullaniciDuzenleViewModel(servis.Object);
        vm.RolleriYukle(new[] { Rol(KullaniciRolTip.Yönetici), Rol(KullaniciRolTip.Kullanici) });
        return vm;
    }

    [Fact]
    public void Yeni_VarsayilanRol_Kullanici()
    {
        var vm = Form(out _);
        vm.YeniIcinHazirla();
        vm.YeniMi.Should().BeTrue();
        vm.Gorunur.Should().BeTrue();
        vm.SeciliRol.RolTip.Should().Be(KullaniciRolTip.Kullanici);
    }

    [Fact]
    public async Task Kaydet_Yeni_MissingAd_Ve_SifreUyusmazligi_Reddedilir()
    {
        var vm = Form(out var servis);
        vm.YeniIcinHazirla();
        vm.KullaniciAdi = "yeni";
        vm.Adi = string.Empty;

        (await vm.KaydetAsync(7)).Should().BeFalse();
        vm.HataMetni.Should().Contain("zorunlu");
        servis.Verify(s => s.CreateKullaniciAsync(It.IsAny<KullaniciModel>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);

        vm.Adi = "Ad";
        vm.Sifre = "bir-iki-uc";
        vm.SifreTekrar = "farkli";
        (await vm.KaydetAsync(7)).Should().BeFalse();
        vm.HataMetni.Should().Contain("eşleşmiyor");
    }

    [Fact]
    public async Task Kaydet_Yeni_CreateCagrilir_FirmaVeRolIle()
    {
        var vm = Form(out var servis);
        servis.Setup(s => s.CreateKullaniciAsync(It.IsAny<KullaniciModel>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(new SuccessApiDataResponse<int>(1, "ok"));

        vm.YeniIcinHazirla();
        vm.KullaniciAdi = "yeni";
        vm.Adi = "Ad";
        vm.Soyadi = "Soyad";
        vm.Sifre = "gizli-123";
        vm.SifreTekrar = "gizli-123";
        vm.SeciliRol = vm.Roller.First(r => r.RolTip == KullaniciRolTip.Yönetici);

        (await vm.KaydetAsync(7)).Should().BeTrue();
        servis.Verify(s => s.CreateKullaniciAsync(
            It.Is<KullaniciModel>(m => m.KullaniciAdi == "yeni" && m.Adi == "Ad"),
            "gizli-123", 7, KullaniciRolSabitleri.YoneticiRolId), Times.Once);
    }

    [Fact]
    public async Task Kaydet_Duzenle_UpdateVeRolAta_Cagrilir()
    {
        var vm = Form(out var servis);
        servis.Setup(s => s.UpdateKullaniciAsync(It.IsAny<KullaniciModel>()))
            .ReturnsAsync(new SuccessApiDataResponse<int>(1, "ok"));
        servis.Setup(s => s.RolAtaAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(new SuccessApiDataResponse<int>(1, "ok"));

        vm.DuzenleIcinHazirla(new KullaniciModel
        {
            Id = 44,
            KullaniciAdi = "mevcut",
            Adi = "Ad",
            Soyadi = "Soyad",
            AktifMi = true,
            RolId = KullaniciRolSabitleri.KullaniciRolId
        });

        vm.YeniMi.Should().BeFalse();
        vm.SeciliRol.RolTip.Should().Be(KullaniciRolTip.Kullanici);

        (await vm.KaydetAsync(7)).Should().BeTrue();
        servis.Verify(s => s.UpdateKullaniciAsync(It.Is<KullaniciModel>(m => m.Id == 44)), Times.Once);
        servis.Verify(s => s.RolAtaAsync(44, 7, KullaniciRolSabitleri.KullaniciRolId), Times.Once);
    }

    // ── Orkestratör VM ──────────────────────────────────────────────────────

    private static KullaniciYonetimiViewModel Pencere(
        out Mock<IKullaniciService> servis,
        KullaniciRolTip? rolTip = KullaniciRolTip.Yönetici)
    {
        servis = new Mock<IKullaniciService>();
        return new KullaniciYonetimiViewModel(
            OrtakServisler().Object, servis.Object, Secim().Object, Kimlik(rolTip));
    }

    [Fact]
    public async Task Load_YoneticiDegil_HataVerir_ServisCagrilmaz()
    {
        var vm = Pencere(out var servis, KullaniciRolTip.Kullanici);

        await vm.LoadAsync();

        vm.HataVar.Should().BeTrue();
        servis.Verify(s => s.GetKullanicilarWithRolAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Load_ListeVeRolleri_FirmaIleDoldurur()
    {
        var vm = Pencere(out var servis);
        servis.Setup(s => s.GetRollerAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciRolModel>>(new List<KullaniciRolModel>
            {
                Rol(KullaniciRolTip.Yönetici), Rol(KullaniciRolTip.Kullanici)
            }, "ok"));
        servis.Setup(s => s.GetKullanicilarWithRolAsync(7)).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciModel>>(new List<KullaniciModel>
            {
                new() { Id = 1, KullaniciAdi = "a", Adi = "A", Soyadi = "B", RolId = KullaniciRolSabitleri.YoneticiRolId }
            }, "ok"));

        await vm.LoadAsync();

        vm.HataVar.Should().BeFalse();
        vm.FirmaAdi.Should().Be("Test Firma");
        vm.Kullanicilar.Should().HaveCount(1);
        vm.Duzenle.Roller.Should().HaveCount(2);
        vm.IsYukleniyor.Should().BeFalse();
        vm.ListeBos.Should().BeFalse();
    }
}
