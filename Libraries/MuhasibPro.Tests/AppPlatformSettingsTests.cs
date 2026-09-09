using FluentAssertions;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Services.UIService;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

public class AppPlatformSettingsTests
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

    private sealed class YakalayanBus : IEventBus
    {
        public readonly List<DomainEvent> Olaylar = new();
        public void Publish<TEvent>(object sender, TEvent @event) where TEvent : DomainEvent
            => Olaylar.Add(@event);
        public void Subscribe<TEvent>(object target, Action<object, TEvent> handler) where TEvent : DomainEvent { }
        public void Unsubscribe(object target) { }
    }

    [Fact]
    public async Task Get_KayitYoksa_VarsayilanlariDoner()
    {
        var saglayici = new AppPlatformSettingsProvider(new BellekAyarlari(), new YakalayanBus());

        var ayar = await saglayici.GetAsync();

        ayar.ThemeDefault.Should().Be("Light");
        ayar.SplashStepDelayMs.Should().Be(150);
        ayar.StatusAutoHideMs.Should().Be(3000);
        ayar.NotificationEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Get_BozukDegerleri_Duzenler()
    {
        var bellek = new BellekAyarlari();
        bellek.Kutu[AppPlatformSettings.SettingsKey] = new AppPlatformSettings
        {
            ThemeDefault = "Mor",
            SplashStepDelayMs = -5,
            StatusAutoHideMs = 999999,
            ApplicationDataFolder = "",
            LocalSettingsFile = ""
        };
        var saglayici = new AppPlatformSettingsProvider(bellek, new YakalayanBus());

        var ayar = await saglayici.GetAsync();

        ayar.ThemeDefault.Should().Be("Light");
        ayar.SplashStepDelayMs.Should().Be(150);
        ayar.StatusAutoHideMs.Should().Be(3000);
        ayar.ApplicationDataFolder.Should().Be("MuhasibPro/ApplicationData");
        ayar.LocalSettingsFile.Should().Be("LocalSettings.json");
    }

    [Fact]
    public async Task Save_Yazar_Ve_OlayYayinlar()
    {
        var bellek = new BellekAyarlari();
        var bus = new YakalayanBus();
        var saglayici = new AppPlatformSettingsProvider(bellek, bus);

        await saglayici.SaveAsync(new AppPlatformSettings { ThemeDefault = "Dark", SplashStepDelayMs = 50 });

        var okunan = await saglayici.GetAsync();
        okunan.ThemeDefault.Should().Be("Dark");
        okunan.SplashStepDelayMs.Should().Be(50);
        bus.Olaylar.Should().ContainSingle()
            .Which.Should().BeOfType<AppSettingsChangedEvent>()
            .Which.SettingsKey.Should().Be(AppPlatformSettings.SettingsKey);
    }
}
