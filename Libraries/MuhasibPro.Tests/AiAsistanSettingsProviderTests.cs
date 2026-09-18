using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.92: AiAsistanSettingsProvider — varsayılan/clamp/yetki/olay.</summary>
public class AiAsistanSettingsProviderTests
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

    [Fact]
    public async Task Kayit_Yoksa_Varsayilan()
    {
        var svc = new AiAsistanSettingsProvider(new BellekAyarlari(), new Mock<IEventBus>().Object);

        var ayar = await svc.GetAsync();

        ayar.GetModelAlias().Should().Be(AiAsistanSettings.VarsayilanModelAlias);
        ayar.EtkinMi.Should().BeTrue();
        ayar.GetEnFazlaMadde().Should().Be(6);
        ayar.GetMaksGecmisTur().Should().Be(4);
        ayar.GetSoruZamanAsimiSn().Should().Be(60);
    }

    [Fact]
    public async Task Clamp_Aralik_Disi()
    {
        var svc = new AiAsistanSettingsProvider(
            new BellekAyarlari(), new Mock<IEventBus>().Object, Kimlik(1, KullaniciRolTip.Yönetici));

        await svc.SaveAsync(new AiAsistanSettings
        {
            ModelAlias = "  ",
            MaksGecmisTur = 99,
            EnFazlaMadde = 0,
            SoruZamanAsimiSn = 5
        });
        var ayar = await svc.GetAsync();

        ayar.GetModelAlias().Should().Be(AiAsistanSettings.VarsayilanModelAlias);
        ayar.GetMaksGecmisTur().Should().Be(20);
        ayar.GetEnFazlaMadde().Should().Be(1);
        ayar.GetSoruZamanAsimiSn().Should().Be(10);
    }

    [Fact]
    public async Task Yonetici_Bile_ModelAlias_Degistiremez_SabitKalir()
    {
        var svc = new AiAsistanSettingsProvider(
            new BellekAyarlari(), new Mock<IEventBus>().Object, Kimlik(1, KullaniciRolTip.Yönetici));

        await svc.SaveAsync(new AiAsistanSettings { ModelAlias = "phi-4-mini" });
        var ayar = await svc.GetAsync();

        ayar.GetModelAlias().Should().Be(AiAsistanSettings.VarsayilanModelAlias);
    }

    [Fact]
    public async Task Yonetici_Olmayan_Kaydetse_Bile_SabitKalir()
    {
        var svc = new AiAsistanSettingsProvider(
            new BellekAyarlari(), new Mock<IEventBus>().Object, Kimlik(2, KullaniciRolTip.Kullanici));

        await svc.SaveAsync(new AiAsistanSettings { ModelAlias = "phi-4-mini", EtkinMi = false });
        var ayar = await svc.GetAsync();

        ayar.GetModelAlias().Should().Be(AiAsistanSettings.VarsayilanModelAlias);
        ayar.EtkinMi.Should().BeFalse();
    }

    [Fact]
    public async Task Normal_Alan_Herkese_Acik()
    {
        var svc = new AiAsistanSettingsProvider(
            new BellekAyarlari(), new Mock<IEventBus>().Object, Kimlik(2, KullaniciRolTip.Kullanici));

        await svc.SaveAsync(new AiAsistanSettings { EtkinMi = false });
        var ayar = await svc.GetAsync();

        ayar.EtkinMi.Should().BeFalse();
    }

    [Fact]
    public async Task Kaydetme_Olay_Yayinlar()
    {
        var bus = new Mock<IEventBus>();
        var svc = new AiAsistanSettingsProvider(new BellekAyarlari(), bus.Object, Kimlik(1, KullaniciRolTip.Yönetici));

        await svc.SaveAsync(new AiAsistanSettings { EtkinMi = false });

        bus.Verify(b => b.Publish(It.IsAny<object>(), It.IsAny<AppSettingsChangedEvent>()), Times.Once);
    }
}
