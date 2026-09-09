using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Data.Contracts.Repository.SistemRepos.Authentication;
using MuhasibPro.Domain.Exceptions;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

public class IdentitySettingsTests
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

    private static AuthenticationService KurServis(IdentitySettings ayar)
    {
        var dogrulayici = new Mock<IAuthenticator>();
        dogrulayici.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidPasswordException("u", "p"));
        var kullanicilar = new Mock<IUserRepository>();
        kullanicilar.Setup(k => k.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((MuhasibPro.Domain.Entities.SistemEntity.Kullanici?)null);
        var saglayici = new Mock<IIdentitySettingsProvider>();
        saglayici.Setup(s => s.GetAsync()).ReturnsAsync(ayar);
        return new AuthenticationService(
            dogrulayici.Object,
            Mock.Of<IBitmapToolsService>(),
            Mock.Of<IMessageService>(),
            new ModelFactory(),
            kullanicilar.Object,
            saglayici.Object);
    }

    [Fact]
    public async Task Kilit_Esigi_Modelden_Gelir()
    {
        var svc = KurServis(new IdentitySettings { MaxFailedAttempts = 2, LockoutMinutes = 5, AttemptWindowMinutes = 5 });

        await Assert.ThrowsAsync<InvalidPasswordException>(() => svc.Login("u", "x"));
        await Assert.ThrowsAsync<InvalidPasswordException>(() => svc.Login("u", "x"));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.Login("u", "x"));
        hata.Message.Should().Contain("hatalı deneme");
    }

    [Fact]
    public async Task Varsayilan_Esik_Bes_Denemedir()
    {
        var svc = KurServis(new IdentitySettings());

        for (int i = 0; i < 5; i++)
            await Assert.ThrowsAsync<InvalidPasswordException>(() => svc.Login("u", "x"));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.Login("u", "x"));
        hata.Message.Should().Contain("hatalı deneme");
    }

    [Fact]
    public async Task Saglayici_Bozuk_Degerleri_Duzenler()
    {
        var saglayici = new IdentitySettingsProvider(new BellekAyarlari());

        await saglayici.SaveAsync(new IdentitySettings
        {
            MaxFailedAttempts = 0,
            LockoutMinutes = 9999,
            AttemptWindowMinutes = -3,
            Pbkdf2Iterations = 5
        });
        var ayar = await saglayici.GetAsync();

        ayar.MaxFailedAttempts.Should().Be(5);
        ayar.LockoutMinutes.Should().Be(5);
        ayar.AttemptWindowMinutes.Should().Be(5);
        ayar.Pbkdf2Iterations.Should().Be(100000);
    }

    [Fact]
    public void VeritabaniAyarlari_Keep_Araligi_Modelden_Gelir()
    {
        new DatabaseSettingsModel { MaxManuelYedekSayisi = 99 }.GetManuelKeep().Should().Be(20);
        new DatabaseSettingsModel { MaxManuelYedekSayisi = -4 }.GetManuelKeep().Should().Be(1);
        new DatabaseSettingsModel { SistemKeepLast = 0 }.GetSistemKeep().Should().Be(1);
        new DatabaseSettingsModel().GetManuelKeep().Should().Be(5);
        new DatabaseSettingsModel().GetSistemKeep().Should().Be(3);
    }
}
