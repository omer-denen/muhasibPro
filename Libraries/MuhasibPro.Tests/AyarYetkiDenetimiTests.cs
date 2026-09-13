using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

/// <summary>Yönetici kapısı tek kaynak: seed fallback + rol satırı + kritik-alan denetimi.</summary>
public class AyarYetkiDenetimiTests
{
    private static IAuthenticationService Kimlik(bool girisYapti, long id, KullaniciRolTip? rolTip)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(girisYapti);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(id);
        auth.SetupGet(a => a.CurrentAccount).Returns(girisYapti
            ? new HesapModel
            {
                KullaniciId = id,
                KullaniciModel = new KullaniciModel
                {
                    Rol = rolTip.HasValue
                        ? new KullaniciRolModel { RolTip = rolTip.Value }
                        : null!
                }
            }
            : null!);
        return auth.Object;
    }

    [Fact]
    public void SeedYonetici_RolSatiri_Olmasa_Da_Yoneticidir()
    {
        AyarYetkiDenetimi.KullaniciYoneticiMi(
            Kimlik(true, KullaniciSabitleri.SeedYoneticiId, null)).Should().BeTrue();
    }

    [Fact]
    public void RolSatiri_Yonetici_Olan_Yoneticidir()
    {
        AyarYetkiDenetimi.KullaniciYoneticiMi(
            Kimlik(true, 7, KullaniciRolTip.Yönetici)).Should().BeTrue();
    }

    [Fact]
    public void NormalKullanici_Yonetici_Degildir()
    {
        AyarYetkiDenetimi.KullaniciYoneticiMi(
            Kimlik(true, 7, KullaniciRolTip.Kullanici)).Should().BeFalse();
    }

    [Fact]
    public void GirisYapmamis_Yonetici_Degildir()
    {
        AyarYetkiDenetimi.KullaniciYoneticiMi(
            Kimlik(false, 0, null)).Should().BeFalse();
        AyarYetkiDenetimi.KullaniciYoneticiMi(null).Should().BeFalse();
    }

    [Fact]
    public void SeedYonetici_Kritik_Ayari_Degistirebilir()
    {
        var auth = Kimlik(true, KullaniciSabitleri.SeedYoneticiId, null);
        var kayitli = new DatabaseSettingsModel();
        var gelen = new DatabaseSettingsModel { MaxManuelYedekSayisi = 9 };

        var eylem = () => AyarYetkiDenetimi.KritikDegisiklikleriDogrula(gelen, kayitli, auth);

        eylem.Should().NotThrow();
    }

    [Fact]
    public void NormalKullanici_Kritik_Ayari_Degistiremez()
    {
        var auth = Kimlik(true, 7, KullaniciRolTip.Kullanici);
        var kayitli = new DatabaseSettingsModel();
        var gelen = new DatabaseSettingsModel { MaxManuelYedekSayisi = 9 };

        var eylem = () => AyarYetkiDenetimi.KritikDegisiklikleriDogrula(gelen, kayitli, auth);

        eylem.Should().Throw<UnauthorizedAccessException>();
    }
}
