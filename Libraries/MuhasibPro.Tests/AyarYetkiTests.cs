using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Business.Services.UIService;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

public class AyarYetkiTests
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

    private static IServiceProvider ServiceProviderWithAuth(IAuthenticationService auth)
    {
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(auth);
        return sp.Object;
    }

    [Fact]
    public void Yonetici_Kritik_Degistirebilir()
    {
        var eski = new IdentitySettings();
        var yeni = new IdentitySettings { MaxFailedAttempts = 3 };

        var eylem = () => AyarYetkiDenetimi.KritikDegisiklikleriDogrula(yeni, eski, Kimlik(KullaniciRolTip.Yönetici));

        eylem.Should().NotThrow();
    }

    [Fact]
    public void NormalKullanici_Kritik_Degistiremez()
    {
        var eski = new IdentitySettings();
        var yeni = new IdentitySettings { LockoutMinutes = 30 };

        var eylem = () => AyarYetkiDenetimi.KritikDegisiklikleriDogrula(yeni, eski, Kimlik(KullaniciRolTip.Kullanici));

        eylem.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void GirisYapmamis_Kritik_Degistiremez()
    {
        var eski = new IdentitySettings();
        var yeni = new IdentitySettings { Pbkdf2Iterations = 200000 };

        var eylem = () => AyarYetkiDenetimi.KritikDegisiklikleriDogrula(yeni, eski, Kimlik(null));

        eylem.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void NormalKullanici_Normal_Ayari_Degistirebilir()
    {
        var eski = new AppPlatformSettings();
        var yeni = new AppPlatformSettings { NotificationEnabled = false, StatusAutoHideMs = 5000 };

        var eylem = () => AyarYetkiDenetimi.KritikDegisiklikleriDogrula(yeni, eski, Kimlik(KullaniciRolTip.Kullanici));

        eylem.Should().NotThrow();
    }

    [Fact]
    public async Task Saglayici_Kritigi_Kullaniciya_Kaydetmez()
    {
        var saglayici = new IdentitySettingsProvider(new BellekAyarlari(), ServiceProviderWithAuth(Kimlik(KullaniciRolTip.Kullanici)));

        var eylem = () => saglayici.SaveAsync(new IdentitySettings { MaxFailedAttempts = 3 });

        await eylem.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Saglayici_Kritigi_Yoneticiye_Kaydeder()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new IdentitySettingsProvider(bellek, ServiceProviderWithAuth(Kimlik(KullaniciRolTip.Yönetici)));

        await saglayici.SaveAsync(new IdentitySettings { MaxFailedAttempts = 3 });

        var okunan = await saglayici.GetAsync();
        okunan.MaxFailedAttempts.Should().Be(3);
    }
}
