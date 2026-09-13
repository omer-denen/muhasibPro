using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.UIService;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Tests;

/// <summary>Denetim Masası "Görünüm & Bildirim" bölümü: AppPlatformAyarlarViewModel
/// AppPlatform sağlayıcısına bağlıdır (oku + otomatik kaydet + clamp + yetki-yüzeylemesi).</summary>
public class AppPlatformAyarlarTests
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

    private static IAuthenticationService Kimlik(KullaniciRolTip? rol)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(rol.HasValue);
        auth.SetupGet(a => a.CurrentAccount).Returns(rol.HasValue
            ? new HesapModel
            {
                KullaniciId = 1,
                KullaniciModel = new KullaniciModel { Rol = new KullaniciRolModel { RolTip = rol.Value } }
            }
            : null!);
        return auth.Object;
    }

    private static Mock<ICommonServices> OrtakServisler(Mock<IStatusMessageService> durum = null!)
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        ortak.SetupGet(o => o.StatusMessageService).Returns((durum ?? new Mock<IStatusMessageService>()).Object);
        return ortak;
    }

    private static AppPlatformSettingsProvider Saglayici(
        BellekAyarlari bellek, KullaniciRolTip? rol = KullaniciRolTip.Yönetici) =>
        new(bellek, Mock.Of<IEventBus>(), Kimlik(rol));

    [Fact]
    public async Task Providersiz_Kurulum_Kirilmaz_VarsayilanlaAcilir()
    {
        var vm = new AppPlatformAyarlarViewModel(OrtakServisler().Object, null!);

        await vm.LoadAsync();
        vm.ThemeDefault.Should().Be("Default");
        vm.SplashStepDelayMs.Should().Be(150);
        vm.StatusAutoHideMs.Should().Be(3000);
        vm.NotificationEnabled.Should().BeTrue();

        var eylem = () => vm.SaveAsync();
        await eylem.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Load_KayitliDegerleriTasir()
    {
        var bellek = new BellekAyarlari();
        bellek.Kutu[AppPlatformSettings.SettingsKey] = new AppPlatformSettings
        {
            ThemeDefault = "Dark", SplashStepDelayMs = 400, StatusAutoHideMs = 5000, NotificationEnabled = false
        };
        var vm = new AppPlatformAyarlarViewModel(OrtakServisler().Object, Saglayici(bellek));

        await vm.LoadAsync();

        vm.ThemeDefault.Should().Be("Dark");
        vm.SplashStepDelayMs.Should().Be(400);
        vm.StatusAutoHideMs.Should().Be(5000);
        vm.NotificationEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Degisiklik_OtomatikKaydedilir_BildirimDuser()
    {
        var bellek = new BellekAyarlari();
        var vm = new AppPlatformAyarlarViewModel(OrtakServisler().Object, Saglayici(bellek));
        await vm.LoadAsync();

        vm.NotificationEnabled = false;
        await Task.Delay(300);

        ((AppPlatformSettings)bellek.Kutu[$"{AppPlatformSettings.SettingsKey}:U1"]).NotificationEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task AykiriDeger_Clampelenir()
    {
        var bellek = new BellekAyarlari();
        var vm = new AppPlatformAyarlarViewModel(OrtakServisler().Object, Saglayici(bellek));
        await vm.LoadAsync();

        vm.StatusAutoHideMs = 999999;
        await Task.Delay(300);

        ((AppPlatformSettings)bellek.Kutu[$"{AppPlatformSettings.SettingsKey}:U1"]).StatusAutoHideMs.Should().Be(3000);
    }

    [Fact]
    public async Task Yetkisiz_Kayit_StatusError_Verir()
    {
        var durum = new Mock<IStatusMessageService>();
        var saglayici = new Mock<IAppPlatformSettingsProvider>();
        saglayici.Setup(s => s.GetAsync()).ReturnsAsync(new AppPlatformSettings());
        saglayici.Setup(s => s.SaveAsync(It.IsAny<AppPlatformSettings>())).ThrowsAsync(new UnauthorizedAccessException());
        var vm = new AppPlatformAyarlarViewModel(OrtakServisler(durum).Object, saglayici.Object);
        await vm.LoadAsync();

        vm.NotificationEnabled = false;
        await Task.Delay(300);

        durum.Verify(d => d.ShowMessage(
            It.Is<string>(m => m.Contains("yönetici")), It.IsAny<StatusMessageType>(), It.IsAny<int>()), Times.AtLeastOnce);
    }
}
