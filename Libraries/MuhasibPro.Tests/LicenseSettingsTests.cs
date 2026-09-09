using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

/// <summary>Faz 5 M2-politika: LicenseSettings (Tür + kontrol aralığı) + sağlayıcı + DI.</summary>
public class LicenseSettingsTests
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
        var ayar = new LicenseSettings();

        ayar.Tur.Should().Be(LisansTuru.Deneme);
        ayar.GetCheckIntervalDays().Should().Be(7);
    }

    [Fact]
    public void Aralık_Modelden_Gelir()
    {
        new LicenseSettings { CheckIntervalDays = 0 }.GetCheckIntervalDays().Should().Be(1);
        new LicenseSettings { CheckIntervalDays = 999 }.GetCheckIntervalDays().Should().Be(90);
        new LicenseSettings { CheckIntervalDays = 30 }.GetCheckIntervalDays().Should().Be(30);
    }

    [Fact]
    public async Task Saglayici_Bozuk_Degerleri_Duzenler()
    {
        var saglayici = new LicenseSettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new LicenseSettings
        {
            Tur = (LisansTuru)99,
            CheckIntervalDays = 0
        });
        var ayar = await saglayici.GetAsync();

        ayar.Tur.Should().Be(LisansTuru.Deneme);
        ayar.GetCheckIntervalDays().Should().Be(1);
    }

    [Fact]
    public async Task NormalKullanici_Turu_Degistiremez()
    {
        var saglayici = new LicenseSettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Kullanici));

        var eylem = () => saglayici.SaveAsync(new LicenseSettings { Tur = LisansTuru.Kurumsal });

        await eylem.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task NormalKullanici_Araligi_Degistirebilir()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new LicenseSettingsProvider(bellek, Kimlik(KullaniciRolTip.Kullanici));

        await saglayici.SaveAsync(new LicenseSettings { CheckIntervalDays = 30 });

        var okunan = await saglayici.GetAsync();
        okunan.GetCheckIntervalDays().Should().Be(30);
    }

    [Fact]
    public async Task Yonetici_Turu_Kaydeder()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new LicenseSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new LicenseSettings { Tur = LisansTuru.Profesyonel });

        var okunan = await saglayici.GetAsync();
        okunan.Tur.Should().Be(LisansTuru.Profesyonel);
    }

    [Fact]
    public void Saglayici_DI_Singleton_Kayitli()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (File.Exists(Path.Combine(dir, "MuhasibPro.slnx"))) break;
            dir = Path.GetDirectoryName(dir)!;
        }
        var src = File.ReadAllText(Path.Combine(dir,
            "Libraries/MuhasibPro.Business/HostBuilder/AddServicesHostBuilderExtensions.cs"));
        src.Should().Contain("AddSingleton<ILicenseSettingsProvider, LicenseSettingsProvider>");
    }
}
