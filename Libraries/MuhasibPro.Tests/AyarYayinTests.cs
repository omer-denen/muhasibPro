using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.Shell;
using System.Diagnostics;

namespace MuhasibPro.Tests;

/// <summary>Faz 5 E2: ayar kayıtları AppSettingsChangedEvent yayınlar + Login tek sefer abone olur.</summary>
public class AyarYayinTests
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

    private sealed class KayitliBus : IEventBus
    {
        private readonly object _kilit = new();
        public readonly List<string> Yayinlar = new();
        public void Publish<TEvent>(object sender, TEvent @event) where TEvent : DomainEvent
        {
            if (@event is AppSettingsChangedEvent ayar)
                lock (_kilit) { Yayinlar.Add(ayar.SettingsKey); }
        }
        public void Subscribe<TEvent>(object target, Action<object, TEvent> handler) where TEvent : DomainEvent { }
        public void Unsubscribe(object target) { }
    }

    private static IAuthenticationService Yonetici()
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(1);
        auth.SetupGet(a => a.CurrentAccount).Returns(new HesapModel
        {
            KullaniciId = 1,
            KullaniciModel = new KullaniciModel { Rol = new KullaniciRolModel { RolTip = KullaniciRolTip.Yönetici } }
        });
        return auth.Object;
    }

    private static Mock<ICommonServices> OrtakServisler(
        Mock<IMessageService> mesaj, Mock<IContextService> baglam)
    {
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.MessageService).Returns(mesaj.Object);
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        return ortak;
    }

    private static Mock<IContextService> AnindaBaglam()
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        return baglam;
    }

    private static async Task<string?> IlkYayin(KayitliBus bus, int zamanAsimiMs = 5000)
    {
        var kronometre = Stopwatch.StartNew();
        while (kronometre.ElapsedMilliseconds < zamanAsimiMs)
        {
            lock (bus.Yayinlar)
            {
                if (bus.Yayinlar.Count > 0)
                    return bus.Yayinlar[0];
            }
            await Task.Delay(50);
        }
        return null;
    }

    [Fact]
    public async Task VeritabaniAyar_Kaydi_Yayinlanir()
    {
        var bus = new KayitliBus();
        var ortak = OrtakServisler(new Mock<IMessageService>(), AnindaBaglam());
        var vm = new DatabaseSettingsViewModel(ortak.Object, new BellekAyarlari(), Yonetici(), bus);

        await vm.LoadAsync();
        vm.MaxManuelYedekSayisi = 7;

        (await IlkYayin(bus)).Should().Be(DatabaseSettingsModel.SettingsKey);
    }

    [Fact]
    public async Task GuncellemeAyar_Kaydi_Yayinlanir()
    {
        var guncelleme = new Mock<IUpdateService>();
        guncelleme.Setup(s => s.SaveSettingsAsync(It.IsAny<UpdateSettingsModel>()))
            .Returns(Task.CompletedTask);
        var bus = new KayitliBus();
        var ortak = OrtakServisler(new Mock<IMessageService>(), AnindaBaglam());
        var vm = new UpdateViewModel(guncelleme.Object, ortak.Object, bus);

        vm.AutoCheckEnabled = false;

        (await IlkYayin(bus)).Should().Be(UpdateSettingsModel.SettingsKey);
    }

    [Fact]
    public async Task Login_Ikinci_Load_Tekrar_Abone_Olmaz()
    {
        var mesaj = new Mock<IMessageService>();
        var ortak = OrtakServisler(mesaj, AnindaBaglam());
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync()).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseConnectionAnalysis>(
                new DatabaseConnectionAnalysis
                {
                    DatabaseName = "Sistem.db",
                    IsDatabaseExists = true,
                    CanConnect = true,
                    DatabaseValid = true
                },
                "hazır"));
        var hizliGiris = new QuickLoginAccountsViewModel(ortak.Object, new BellekAyarlari());
        var vm = new LoginViewModel(
            ortak.Object,
            Mock.Of<IAuthenticationService>(),
            Mock.Of<IFirmaService>(),
            hizliGiris,
            sistem.Object);

        await vm.LoadAsync(new ShellArgs { ViewModel = typeof(LoginViewModel) });
        await vm.LoadAsync(new ShellArgs { ViewModel = typeof(LoginViewModel) });

        mesaj.Verify(m => m.Subscribe<QuickLoginAccountsViewModel, RememberedAccount>(
            It.IsAny<object>(), It.IsAny<Action<QuickLoginAccountsViewModel, string, RememberedAccount>>()),
            Times.Once);

        vm.Unsubscribe();
        mesaj.Verify(m => m.Unsubscribe(It.IsAny<object>()), Times.Once);
    }
}
