using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Tests;

/// <summary>Denetim Masası "Giriş Güvenliği" bölümü: IdentityAyarlarViewModel
/// global modeli bağlar (oku + otomatik kaydet + yönetici kapısı).</summary>
public class IdentityAyarlarTests
{
    private sealed class BellekAyarlari : MuhasibPro.Business.Contracts.UIServices.ILocalSettingsService
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

    private static IAuthenticationService Kimlik(KullaniciRolTip? rol, long id = 1)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(rol.HasValue);
        auth.SetupGet(a => a.CurrentAccount).Returns(rol.HasValue
            ? new HesapModel
            {
                KullaniciId = id,
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

    private static IdentitySettingsProvider Saglayici(BellekAyarlari bellek)
        => new(bellek);

    [Fact]
    public async Task Providersiz_Kurulum_Kirilmaz()
    {
        var vm = new IdentityAyarlarViewModel(OrtakServisler().Object, null!);

        await vm.LoadAsync();

        vm.MaxFailedAttempts.Should().Be(5);
        vm.IsYonetici.Should().BeFalse();
    }

    [Fact]
    public async Task Yonetici_Belirlenir_KayitClampelenir()
    {
        var bellek = new BellekAyarlari();
        var vm = new IdentityAyarlarViewModel(OrtakServisler().Object, Saglayici(bellek), Kimlik(KullaniciRolTip.Yönetici));
        await vm.LoadAsync();

        vm.IsYonetici.Should().BeTrue();
        vm.MaxFailedAttempts = 99;
        await Task.Delay(300);

        ((IdentitySettings)bellek.Kutu[IdentitySettings.SettingsKey]).MaxFailedAttempts.Should().Be(5);
    }

    [Fact]
    public async Task NormalKullanici_YoneticiDegil()
    {
        var vm = new IdentityAyarlarViewModel(OrtakServisler().Object, Saglayici(new BellekAyarlari()), Kimlik(KullaniciRolTip.Kullanici));
        await vm.LoadAsync();

        vm.IsYonetici.Should().BeFalse();
    }
}
