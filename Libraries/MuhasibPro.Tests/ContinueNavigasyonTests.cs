using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Shell.Tenant;
using System.Diagnostics;

namespace MuhasibPro.Tests;

/// <summary>Madde 5 (Continue anomalisi): güncelleme sayfasında Continue sonrası
/// `TenantEvents.Updated` FirmaShell'i MainShell'e geçirmeli — yerel seçim
/// eşleşmese bile (yönetim sayfasından girilen akış), Continue'un yazdığı
/// paylaşılan ayna yedeğiyle. Eşleşen yerel seçimde davranış aynen korunur.</summary>
public class ContinueNavigasyonTests
{
    private sealed class YakalayanMesaj : IMessageService
    {
        public Action<TenantDatabaseUpdateViewModel, string, object>? GuncellemeIsleyici;
        public void Subscribe<TSender>(object target, Action<TSender, string, object> action) where TSender : class
        {
            if (typeof(TSender) == typeof(TenantDatabaseUpdateViewModel))
                GuncellemeIsleyici = (Action<TenantDatabaseUpdateViewModel, string, object>)(object)action;
        }
        public void Subscribe<TSender, TArgs>(object target, Action<TSender, string, TArgs> action) where TSender : class { }
        public void Unsubscribe(object target) { }
        public void Unsubscribe<TSender>(object target) where TSender : class { }
        public void Unsubscribe<TSender, TArgs>(object target) where TSender : class { }
        public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class { }
    }

    private static (Mock<ICommonServices> ortak, Mock<INavigationService> nav, YakalayanMesaj mesaj) OrtakServisler()
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var nav = new Mock<INavigationService>();
        var mesaj = new YakalayanMesaj();
        var log = new Mock<ILogService>();
        log.SetupGet(l => l.SistemLogService).Returns(Mock.Of<ISistemLogService>());
        log.SetupGet(l => l.AppLogService).Returns(Mock.Of<IAppLogService>());
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(mesaj);
        ortak.SetupGet(o => o.NavigationService).Returns(nav.Object);
        ortak.SetupGet(o => o.DialogService).Returns(Mock.Of<IDialogService>());
        ortak.SetupGet(o => o.LogService).Returns(log.Object);
        return (ortak, nav, mesaj);
    }

    private static FirmaModel Firma() => new() { Id = 7, FirmaKodu = "F-0007", KisaUnvani = "Test" };

    private static MaliDonemModel Donem() => new()
    {
        Id = 2, FirmaId = 7, MaliYil = 2027, DatabaseName = "db-F-0001_2027", AktifMi = true
    };

    private static FirmaShellViewModel KabukKur(
        Mock<ICommonServices> ortak, FirmaModel aynaFirma, MaliDonemModel aynaDonem)
    {
        var ayna = new Mock<IFirmaWithMaliDonemSelectedService>();
        ayna.SetupGet(a => a.SelectedFirma).Returns(aynaFirma);
        ayna.SetupGet(a => a.SelectedMaliDonem).Returns(aynaDonem);
        var vm = new FirmaShellViewModel(
            ortak.Object, Mock.Of<IFirmaService>(), Mock.Of<IFilePickerService>(),
            Mock.Of<IMaliDonemService>(), Mock.Of<ILocalSettingsService>(),
            ayna.Object, Mock.Of<ITenantSQLiteDatabaseService>(),
            Mock.Of<ITenantDatabaseUpdateService>(), null!);
        vm.ViewModelArgs = new ShellArgs();
        return vm;
    }

    private static async Task<bool> BekleAsync(Func<bool> kosul, int zamanAsimiMs = 10000)
    {
        var kronometre = Stopwatch.StartNew();
        while (kronometre.ElapsedMilliseconds < zamanAsimiMs)
        {
            if (kosul()) return true;
            await Task.Delay(50);
        }
        return kosul();
    }

    [Fact]
    public async Task Continue_YerelSecimsizken_Aynadan_MainShellE_Gecer()
    {
        // Yönetim sayfasından girilen akış: yerel Selection boş, aynada Continue yazmış.
        var (ortak, nav, mesaj) = OrtakServisler();
        var vm = KabukKur(ortak, Firma(), Donem());
        vm.BaseSubscribe();
        mesaj.GuncellemeIsleyici.Should().NotBeNull();

        mesaj.GuncellemeIsleyici!(null!, TenantEvents.Updated, "db-F-0001_2027");

        (await BekleAsync(() =>
        {
            try { nav.Verify(n => n.Navigate<MainShellViewModel>(It.IsAny<object>()), Times.Once); return true; }
            catch { return false; }
        })).Should().BeTrue("ayna-yedeği yerel seçimi doldurup DevamEt ile MainShell'e geçmeli");
        vm.Selection.SelectedMaliDonem.Should().NotBeNull();
    }

    [Fact]
    public async Task Continue_EslesenYerelSecimde_Aynen_Gecer()
    {
        var (ortak, nav, mesaj) = OrtakServisler();
        var vm = KabukKur(ortak, Firma(), Donem());
        vm.Selection.SelectFirma(Firma());
        vm.Selection.SelectMaliDonem(Donem());
        vm.BaseSubscribe();

        mesaj.GuncellemeIsleyici!(null!, TenantEvents.Updated, "db-F-0001_2027");

        (await BekleAsync(() =>
        {
            try { nav.Verify(n => n.Navigate<MainShellViewModel>(It.IsAny<object>()), Times.Once); return true; }
            catch { return false; }
        })).Should().BeTrue("eşleşen yerel seçimde eski davranış korunmalı");
    }

    [Fact]
    public async Task Continue_AlakasizVeritabaninda_GecisYapmaz()
    {
        var (ortak, nav, mesaj) = OrtakServisler();
        var vm = KabukKur(ortak, Firma(), Donem());
        vm.BaseSubscribe();

        mesaj.GuncellemeIsleyici!(null!, TenantEvents.Updated, "db-YABANCI_2099");
        await Task.Delay(300);

        nav.Verify(n => n.Navigate<MainShellViewModel>(It.IsAny<object>()), Times.Never);
    }
}
