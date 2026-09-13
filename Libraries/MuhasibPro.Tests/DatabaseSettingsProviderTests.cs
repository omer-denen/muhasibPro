using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

/// <summary>M4 SystemDb sağlayıcısı: kullanıcı-bazlı kayıt + clamp + yetki (Entity/Tenant deseni).</summary>
public class DatabaseSettingsProviderTests
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

    private static IFirmaKullaniciCozucu Cozucu(long girisYapanId)
    {
        var cozucu = new Mock<IFirmaKullaniciCozucu>();
        cozucu.Setup(c => c.GirisYapanId()).Returns(girisYapanId);
        cozucu.Setup(c => c.CozAsync(It.IsAny<long>())).ReturnsAsync(girisYapanId);
        return cozucu.Object;
    }

    [Fact]
    public void Varsayilanlar_Plan_Degerleridir()
    {
        var ayar = new DatabaseSettingsModel();

        ayar.MaxManuelYedekSayisi.Should().Be(5);
        ayar.SistemKeepLast.Should().Be(3);
        ayar.BusyTimeoutMs.Should().Be(5000);
        ayar.JournalMode.Should().Be("WAL");
        ayar.Synchronous.Should().Be("NORMAL");
        ayar.WeeklyBackupDays.Should().Be(7);
        ayar.GetManuelKeep().Should().Be(5);
        ayar.GetSistemKeep().Should().Be(3);
    }

    [Fact]
    public async Task Saglayici_Bozuk_Degerleri_Duzenler()
    {
        var saglayici = new DatabaseSettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new DatabaseSettingsModel
        {
            MaxManuelYedekSayisi = 0,
            SistemKeepLast = 999,
            BusyTimeoutMs = 100,
            WeeklyBackupDays = 99,
            JournalMode = "bozuk",
            Synchronous = ""
        });
        var ayar = await saglayici.GetAsync();

        ayar.MaxManuelYedekSayisi.Should().Be(5);
        ayar.SistemKeepLast.Should().Be(3);
        ayar.BusyTimeoutMs.Should().Be(5000);
        ayar.WeeklyBackupDays.Should().Be(7);
        ayar.JournalMode.Should().Be("WAL");
        ayar.Synchronous.Should().Be("NORMAL");
    }

    [Fact]
    public async Task NormalKullanici_Saklama_Limitini_Degistiremez()
    {
        var saglayici = new DatabaseSettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Kullanici));

        var eylem = () => saglayici.SaveAsync(new DatabaseSettingsModel { MaxManuelYedekSayisi = 9 });

        await eylem.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task NormalKullanici_Normal_Ayarlari_Degistirebilir()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new DatabaseSettingsProvider(bellek, Kimlik(KullaniciRolTip.Kullanici));

        await saglayici.SaveAsync(new DatabaseSettingsModel { KapanistaOtomatikYedek = true, WeeklyBackupDays = 14 });

        var okunan = await saglayici.GetAsync();
        okunan.KapanistaOtomatikYedek.Should().BeTrue();
        okunan.WeeklyBackupDays.Should().Be(14);
    }

    [Fact]
    public async Task Yonetici_Tumunu_Kaydeder()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new DatabaseSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new DatabaseSettingsModel
        {
            MaxManuelYedekSayisi = 7,
            JournalMode = "DELETE",
            Synchronous = "FULL",
            VacuumOnBackup = false
        });

        var okunan = await saglayici.GetAsync();
        okunan.MaxManuelYedekSayisi.Should().Be(7);
        okunan.JournalMode.Should().Be("DELETE");
        okunan.Synchronous.Should().Be("FULL");
        okunan.VacuumOnBackup.Should().BeFalse();
    }

    [Fact]
    public async Task Kullanicilar_Birbirinden_Yalitilir()
    {
        var bellek = new BellekAyarlari();
        var birinci = new DatabaseSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici, 1), Cozucu(1));
        var ikinci = new DatabaseSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici, 2), Cozucu(2));

        await birinci.SaveAsync(new DatabaseSettingsModel { MaxManuelYedekSayisi = 7 });
        await ikinci.SaveAsync(new DatabaseSettingsModel { MaxManuelYedekSayisi = 12 });

        (await birinci.GetAsync()).MaxManuelYedekSayisi.Should().Be(7);
        (await ikinci.GetAsync()).MaxManuelYedekSayisi.Should().Be(12);
    }

    [Fact]
    public async Task Eski_Firma_Kaydi_Kullaniciya_Tasinir()
    {
        var bellek = new BellekAyarlari();
        await bellek.SaveSettingAsync(
            FirmaAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, 9),
            new DatabaseSettingsModel { MaxManuelYedekSayisi = 11 });
        var saglayici = new DatabaseSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici, 4), Cozucu(4));

        var okunan = await saglayici.GetAsync(firmaId: 9);

        okunan.MaxManuelYedekSayisi.Should().Be(11);
        bellek.Kutu.Should().ContainKey(KullaniciAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, 4));
    }
}
