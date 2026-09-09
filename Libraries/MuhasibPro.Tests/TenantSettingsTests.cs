using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

public class TenantSettingsTests
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

    [Fact]
    public void Varsayilanlar_Plan_Degerleridir()
    {
        var ayar = new TenantSettings();

        ayar.GetYedekPageSize().Should().Be(4);
        ayar.GetBilinmeyenPageSize().Should().Be(4);
        ayar.GetBackfillSayfaBoyutu().Should().Be(100);
        ayar.GetMigrationRetry().Should().Be(2);
        ayar.GetBakimTimeoutSec().Should().Be(120);
        ayar.GetCommandTimeoutSec().Should().Be(30);
        ayar.GetBusyTimeoutMs().Should().Be(5000);
        ayar.Pooling.Should().BeTrue();
    }

    [Fact]
    public void Esikler_Modelden_Gelir()
    {
        new TenantSettings { MigrationRetry = 99 }.GetMigrationRetry().Should().Be(5);
        new TenantSettings { MigrationRetry = 0 }.GetMigrationRetry().Should().Be(1);
        new TenantSettings { BakimTimeoutSec = 5 }.GetBakimTimeoutSec().Should().Be(30);
        new TenantSettings { BakimTimeoutSec = 9999 }.GetBakimTimeoutSec().Should().Be(600);
        new TenantSettings { CommandTimeoutSec = 0 }.GetCommandTimeoutSec().Should().Be(5);
        new TenantSettings { BusyTimeoutMs = 100 }.GetBusyTimeoutMs().Should().Be(1000);
        new TenantSettings { BusyTimeoutMs = 999999 }.GetBusyTimeoutMs().Should().Be(30000);
        new TenantSettings { YedekPageSize = 99 }.GetYedekPageSize().Should().Be(50);
        new TenantSettings { BilinmeyenPageSize = -1 }.GetBilinmeyenPageSize().Should().Be(1);
        new TenantSettings { BackfillSayfaBoyutu = 5 }.GetBackfillSayfaBoyutu().Should().Be(10);
    }

    [Fact]
    public async Task Saglayici_Bozuk_Degerleri_Duzenler()
    {
        var saglayici = new TenantSettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new TenantSettings
        {
            MigrationRetry = 0,
            BakimTimeoutSec = -5,
            CommandTimeoutSec = 9999,
            BusyTimeoutMs = 0,
            YedekPageSize = 0,
            BackfillSayfaBoyutu = 5
        });
        var ayar = await saglayici.GetAsync();

        ayar.GetMigrationRetry().Should().Be(2);
        ayar.GetBakimTimeoutSec().Should().Be(120);
        ayar.GetCommandTimeoutSec().Should().Be(30);
        ayar.GetBusyTimeoutMs().Should().Be(5000);
        ayar.GetYedekPageSize().Should().Be(4);
        ayar.GetBackfillSayfaBoyutu().Should().Be(100);
    }

    [Fact]
    public async Task NormalKullanici_Kritik_Esigi_Degistiremez()
    {
        var saglayici = new TenantSettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Kullanici));

        var eylem = () => saglayici.SaveAsync(new TenantSettings { MigrationRetry = 4 });

        await eylem.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task NormalKullanici_Pooling_Kapatamaz()
    {
        var saglayici = new TenantSettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Kullanici));

        var eylem = () => saglayici.SaveAsync(new TenantSettings { Pooling = false });

        await eylem.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task NormalKullanici_Sayfa_Boyutunu_Degistirebilir()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new TenantSettingsProvider(bellek, Kimlik(KullaniciRolTip.Kullanici));

        await saglayici.SaveAsync(new TenantSettings { YedekPageSize = 10 });

        var okunan = await saglayici.GetAsync();
        okunan.GetYedekPageSize().Should().Be(10);
    }

    [Fact]
    public async Task Yonetici_Esik_Kaydeder()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new TenantSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new TenantSettings { MigrationRetry = 4, BakimTimeoutSec = 300 });

        var okunan = await saglayici.GetAsync();
        okunan.GetMigrationRetry().Should().Be(4);
        okunan.GetBakimTimeoutSec().Should().Be(300);
    }
}
