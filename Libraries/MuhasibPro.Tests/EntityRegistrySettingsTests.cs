using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

public class EntityRegistrySettingsTests
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
        var ayar = new EntityRegistrySettings();

        ayar.FirmaKodPattern.Should().Be("F-XXXX");
        ayar.DefaultDurum.Should().Be(DonemDurum.Acik);
        ayar.GetAcikPageSize().Should().Be(8);
        ayar.GetArsivPageSize().Should().Be(5);
        ayar.ValidationStrict.Should().BeTrue();
    }

    [Fact]
    public void Sayfa_Boyutlari_Modelden_Gelir()
    {
        new EntityRegistrySettings { AcikPageSize = 99 }.GetAcikPageSize().Should().Be(50);
        new EntityRegistrySettings { AcikPageSize = -4 }.GetAcikPageSize().Should().Be(1);
        new EntityRegistrySettings { ArsivPageSize = 0 }.GetArsivPageSize().Should().Be(1);
        new EntityRegistrySettings { ArsivPageSize = 12 }.GetArsivPageSize().Should().Be(12);
        new EntityRegistrySettings().GetAcikPageSize().Should().Be(8);
        new EntityRegistrySettings().GetArsivPageSize().Should().Be(5);
    }

    [Fact]
    public void Bos_Desen_Varsayilana_Duser()
    {
        new EntityRegistrySettings { FirmaKodPattern = "  " }.GetFirmaKodPattern().Should().Be("F-XXXX");
        new EntityRegistrySettings { FirmaKodPattern = " G-XXX " }.GetFirmaKodPattern().Should().Be("G-XXX");
    }

    [Fact]
    public async Task Saglayici_Bozuk_Degerleri_Duzenler()
    {
        var saglayici = new EntityRegistrySettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new EntityRegistrySettings
        {
            AcikPageSize = 0,
            ArsivPageSize = 999,
            FirmaKodPattern = "",
            DefaultDurum = (DonemDurum)99
        });
        var ayar = await saglayici.GetAsync();

        ayar.GetAcikPageSize().Should().Be(8);
        ayar.GetArsivPageSize().Should().Be(5);
        ayar.GetFirmaKodPattern().Should().Be("F-XXXX");
        ayar.DefaultDurum.Should().Be(DonemDurum.Acik);
    }

    [Fact]
    public async Task NormalKullanici_Deseni_Degistiremez()
    {
        var saglayici = new EntityRegistrySettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Kullanici));

        var eylem = () => saglayici.SaveAsync(new EntityRegistrySettings { FirmaKodPattern = "G-XXX" });

        await eylem.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task NormalKullanici_Strict_Kapatamaz()
    {
        var saglayici = new EntityRegistrySettingsProvider(new BellekAyarlari(), Kimlik(KullaniciRolTip.Kullanici));

        var eylem = () => saglayici.SaveAsync(new EntityRegistrySettings { ValidationStrict = false });

        await eylem.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task NormalKullanici_Normal_Ayarlari_Degistirebilir()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new EntityRegistrySettingsProvider(bellek, Kimlik(KullaniciRolTip.Kullanici));

        await saglayici.SaveAsync(new EntityRegistrySettings { DefaultDurum = DonemDurum.Acik, AcikPageSize = 10 });

        var okunan = await saglayici.GetAsync();
        okunan.GetAcikPageSize().Should().Be(10);
    }

    [Fact]
    public async Task Yonetici_Tumunu_Kaydeder()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new EntityRegistrySettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new EntityRegistrySettings { FirmaKodPattern = "G-XXX", ValidationStrict = false });

        var okunan = await saglayici.GetAsync();
        okunan.GetFirmaKodPattern().Should().Be("G-XXX");
        okunan.ValidationStrict.Should().BeFalse();
    }

    [Theory]
    [InlineData("F-0001", "F-XXXX", true)]
    [InlineData("F-12", "F-XXXX", false)]
    [InlineData("G-0001", "F-XXXX", false)]
    [InlineData(null, "F-XXXX", false)]
    [InlineData("F-0001", "", false)]
    public void Kod_Deseni_Dogrular(string? kod, string desen, bool beklenen)
    {
        FirmaKodHelper.IsValid(kod, desen).Should().Be(beklenen);
    }

    [Theory]
    [InlineData("F-XXXX", 7, "F-0007")]
    [InlineData("F-XXXX", 12345, "F-2345")]
    [InlineData("G-XXX", 1, "G-001")]
    public void Kod_Desenden_Uretilir(string desen, int sira, string beklenen)
    {
        FirmaKodHelper.Generate(desen, sira).Should().Be(beklenen);
    }
}
