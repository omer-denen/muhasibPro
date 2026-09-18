using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.85 K4 — izin kapıları: firma-bağımsız kullanıcı izni,
/// modül menüsü pasif+gerekçe, Denetim Masası bölüm gizleme.</summary>
public class RbacK4Tests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        ortak.SetupGet(o => o.NotificationService).Returns(Mock.Of<INotificationService>());
        return ortak;
    }

    private static IAuthenticationService Kimlik(long kullaniciId)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(kullaniciId);
        return auth.Object;
    }

    private static KullaniciFirmaRol Kfr(KullaniciRolTip tip, long firmaId = 7)
        => new()
        {
            KullaniciId = 5,
            FirmaId = firmaId,
            RolId = tip == KullaniciRolTip.Yönetici
                ? KullaniciRolSabitleri.YoneticiRolId
                : KullaniciRolSabitleri.KullaniciRolId,
            Rol = new KullaniciRol
            {
                Id = tip == KullaniciRolTip.Yönetici
                    ? KullaniciRolSabitleri.YoneticiRolId
                    : KullaniciRolSabitleri.KullaniciRolId,
                RolTip = tip
            }
        };

    private static PermissionService Kur(
        IKullaniciFirmaRolRepository kfr,
        IRolPermissionRepository rp,
        IFirmaWithMaliDonemSelectedService secim = null)
        => new(kfr, rp, secim ?? new Mock<IFirmaWithMaliDonemSelectedService>().Object, Kimlik(5));

    [Fact]
    public async Task KullaniciYetkisiVarMi_Yonetici_Bypass()
    {
        var kfr = new Mock<IKullaniciFirmaRolRepository>();
        kfr.Setup(r => r.GetByKullaniciIdAsync(5)).ReturnsAsync(new List<KullaniciFirmaRol> { Kfr(KullaniciRolTip.Yönetici) });
        var rp = new Mock<IRolPermissionRepository>();
        rp.Setup(r => r.GetByRolIdAsync(It.IsAny<long>())).ReturnsAsync(new List<RolPermission>());

        var svc = Kur(kfr.Object, rp.Object);

        // Firma seçilmemiş olsa bile yönetici her izne sahiptir.
        (await svc.KullaniciYetkisiVarMiAsync(Permission.Firma_Yonet)).Should().BeTrue();
        (await svc.KullaniciYetkisiVarMiAsync(Permission.Veritabani_KaliciSil)).Should().BeTrue();
        rp.Verify(r => r.GetByRolIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task KullaniciYetkisiVarMi_Kullanici_SadeceRolKumesi()
    {
        var kfr = new Mock<IKullaniciFirmaRolRepository>();
        kfr.Setup(r => r.GetByKullaniciIdAsync(5)).ReturnsAsync(new List<KullaniciFirmaRol> { Kfr(KullaniciRolTip.Kullanici) });
        var rp = new Mock<IRolPermissionRepository>();
        rp.Setup(r => r.GetByRolIdAsync(KullaniciRolSabitleri.KullaniciRolId)).ReturnsAsync(new List<RolPermission>
        {
            new() { RolId = KullaniciRolSabitleri.KullaniciRolId, PermissionId = Permission.Cari_Goruntule },
            new() { RolId = KullaniciRolSabitleri.KullaniciRolId, PermissionId = Permission.AiAsistan_Kullan }
        });

        var svc = Kur(kfr.Object, rp.Object);

        (await svc.KullaniciYetkisiVarMiAsync(Permission.Cari_Goruntule)).Should().BeTrue();
        (await svc.KullaniciYetkisiVarMiAsync(Permission.AiAsistan_Kullan)).Should().BeTrue();
        (await svc.KullaniciYetkisiVarMiAsync(Permission.Firma_Yonet)).Should().BeFalse();
        (await svc.KullaniciYetkisiVarMiAsync(Permission.Log_Goruntule)).Should().BeFalse();
    }

    [Fact]
    public async Task KullaniciYetkisiVarMi_KfrYok_False()
    {
        var kfr = new Mock<IKullaniciFirmaRolRepository>();
        kfr.Setup(r => r.GetByKullaniciIdAsync(5)).ReturnsAsync(new List<KullaniciFirmaRol>());
        var rp = new Mock<IRolPermissionRepository>();

        var svc = Kur(kfr.Object, rp.Object);

        (await svc.KullaniciYetkisiVarMiAsync(Permission.Cari_Goruntule)).Should().BeFalse();
    }

    [Fact]
    public async Task MainMenu_YetkisizOge_Gizli()
    {
        var yetki = new Mock<IPermissionService>();
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(It.IsAny<Permission>())).ReturnsAsync(true);
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(Permission.Firma_Yonet)).ReturnsAsync(false);
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(Permission.Log_Goruntule)).ReturnsAsync(false);

        var auth = new Mock<IAuthenticationService>();
        var vm = new MainMenuViewModel(auth.Object, Mock.Of<ISistemDatabaseService>(), OrtakServisler().Object, yetki.Object);

        vm.InitializeNavigationItems();
        await vm.YetkileriUygulaAsync();

        // Katalog tam kalır; görünür liste süzülür (yetkisiz öğe gizlenir).
        vm.NavigationItems.Should().HaveCount(3);
        vm.GorunurNavigationItems.Select(i => i.Label).Should().BeEquivalentTo(new[] { "Genel Bakış" });
    }

    [Fact]
    public async Task MainMenu_YetkiliKullanici_TumGercekOgeGorunur()
    {
        var yetki = new Mock<IPermissionService>();
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(It.IsAny<Permission>())).ReturnsAsync(true);

        var vm = new MainMenuViewModel(new Mock<IAuthenticationService>().Object, Mock.Of<ISistemDatabaseService>(), OrtakServisler().Object, yetki.Object);
        vm.InitializeNavigationItems();
        await vm.YetkileriUygulaAsync();

        vm.GorunurNavigationItems.Select(i => i.Label).Should().BeEquivalentTo(
            new[] { "Genel Bakış", "Firma Yönetimi", "Sistem Kayıtları" });
    }

    [Fact]
    public void MainMenu_SahteModulYok_YalnizGercekHedefler()
    {
        var yetki = new Mock<IPermissionService>();
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(It.IsAny<Permission>())).ReturnsAsync(true);

        var vm = new MainMenuViewModel(new Mock<IAuthenticationService>().Object, Mock.Of<ISistemDatabaseService>(), OrtakServisler().Object, yetki.Object);
        vm.InitializeNavigationItems();

        vm.NavigationItems.Select(i => i.Label).Should().BeEquivalentTo(
            new[] { "Genel Bakış", "Firma Yönetimi", "Sistem Kayıtları" });
        vm.NavigationItems.Should().OnlyContain(i => i.ViewModel != null && i.ViewModel != typeof(Nullable));
    }

    [Fact]
    public async Task FirmaSilme_Yetkisiz_Reddedilir_RepoDokunulmaz()
    {
        var repo = new Mock<IFirmaRepository>();
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.GetCurrentUserId).Returns(1);
        var yetki = new Mock<IPermissionService>();
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(Permission.Firma_Yonet)).ReturnsAsync(false);

        var svc = new FirmaSilmeService(
            repo.Object,
            new Mock<IUnitOfWork<SistemDbContext>>().Object,
            new Mock<ILogService>().Object,
            auth.Object,
            yetki.Object);

        var sonuc = await svc.DeleteFirmaAsync(42);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("yetkiniz yok");
        repo.Verify(r => r.GetByFirmaIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task MaliDonemSilme_Yetkisiz_Reddedilir_RepoDokunulmaz()
    {
        var repo = new Mock<IMaliDonemRepository>();
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.GetCurrentUserId).Returns(1);
        var yetki = new Mock<IPermissionService>();
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(Permission.MaliDonem_Yonet)).ReturnsAsync(false);

        var svc = new MaliDonemService(
            repo.Object,
            new Mock<ILogService>().Object,
            new Mock<IUnitOfWork<SistemDbContext>>().Object,
            auth.Object,
            new Mock<IFirmaService>().Object,
            new Mock<IBitmapToolsService>().Object,
            null,
            yetki.Object);

        var sonuc = await svc.DeleteMaliDonemAsync(42);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("yetkiniz yok");
        repo.Verify(r => r.GetByMaliDonemIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Arsiv_DonemKapatma_Yetkisiz_Engellenir()
    {
        var donem = new Mock<IMaliDonemService>();
        var yetki = new Mock<IPermissionService>();
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(Permission.MaliDonem_Yonet)).ReturnsAsync(false);

        var vm = new ArsivDonemlerViewModel(OrtakServisler().Object, donem.Object, null, null, yetki.Object);

        var sonuc = await vm.ArsivleAsync(new MaliDonemModel { Id = 5, MaliYil = 2026 });

        sonuc.Should().BeFalse();
        donem.Verify(d => d.UpdateMaliDonemAsync(It.IsAny<MaliDonemModel>()), Times.Never);
    }

    [Fact]
    public async Task Denetim_KisitliKullanici_YetkisizBolumlerGizli()
    {
        var firma = new Mock<IFirmaService>();
        firma.Setup(s => s.GetFirmalarWithUserId(It.IsAny<DataRequest<Firma>>(), It.IsAny<long>()))
            .ReturnsAsync(new MuhasibPro.Domain.Utilities.Responses.SuccessApiDataResponse<IList<FirmaModel>>(
                new List<FirmaModel>(), "ok"));

        var yetki = new Mock<IPermissionService>();
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(It.IsAny<Permission>())).ReturnsAsync(false);
        yetki.Setup(p => p.KullaniciYetkisiVarMiAsync(Permission.AiAsistan_Kullan)).ReturnsAsync(true);

        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, firma.Object, permissionService: yetki.Object);
        await vm.LoadAsync();

        var bolumler = vm.GorunurMenuler.Select(m => m.Bolum).ToList();
        bolumler.Should().Contain(AyarBolumu.GirisPaneli);
        bolumler.Should().Contain(AyarBolumu.Gorunum);
        bolumler.Should().Contain(AyarBolumu.YapayZeka);
        bolumler.Should().NotContain(AyarBolumu.Firma);
        bolumler.Should().NotContain(AyarBolumu.Veritabani);
        bolumler.Should().NotContain(AyarBolumu.Donem);
        bolumler.Should().NotContain(AyarBolumu.Giris);
    }
}
