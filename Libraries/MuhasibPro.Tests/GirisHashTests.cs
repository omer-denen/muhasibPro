using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MuhasibPro.Data.Contracts.Repository.SistemRepos.Authentication;
using MuhasibPro.Data.Repository.SistemRepos.Authentication;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Exceptions;

namespace MuhasibPro.Tests;

/// <summary>Login hash dayanıklılığı: bozuk/legacy hash hasher'ı patlatmaz,
/// InvalidPasswordException ile yedek yola düşer (Oturum 199: seed hash 78 karakterdi).</summary>
public class GirisHashTests
{
    private const string LegacyTestHash =
        "PBKDF2$100000$MDEyMzQ1Njc4OUFCQ0RFRg==$Hm85ty7CnXdG1XpSL8pqDeUjV/fjF2+vXA9BTTeEQYA=";

    private static Kullanici Kayit(string hash) => new()
    {
        Id = 10,
        KullaniciAdi = "testci",
        Adi = "Test",
        Soyadi = "Kullanici",
        Eposta = "test@ornek.com",
        ParolaHash = hash,
        AktifMi = true,
        KayitTarihi = DateTime.Now
    };

    private static AuthenticationRepository KurServis(string hash)
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByUsernameAsync("testci")).ReturnsAsync(Kayit(hash));
        return new AuthenticationRepository(repo.Object, new PasswordHasher<Kullanici>());
    }

    [Fact]
    public async Task BozukHash_FormatFirlatmadan_Reddedilir()
    {
        var svc = KurServis("BOZUK-HASH");

        var eylem = () => svc.Login("testci", "Test123!");

        await eylem.Should().ThrowAsync<InvalidPasswordException>();
    }

    [Fact]
    public async Task LegacyHash_YedekYola_Duser_FormatDegil()
    {
        var svc = KurServis(LegacyTestHash);

        var eylem = () => svc.Login("testci", "Test123!");

        // Identity hasher legacy biçimi reddeder; FormatException kaçmaz, parola-reddi döner
        // (AuthenticationService bunu yakalayıp LegacyPbkdf2Verifier'a geçirir).
        await eylem.Should().ThrowAsync<InvalidPasswordException>();
    }

    [Fact]
    public async Task GecerliHash_DogruParola_GirisYapar()
    {
        var dogru = new PasswordHasher<Kullanici>().HashPassword(null!, "Test123!");
        var svc = KurServis(dogru);

        var sonuc = await svc.Login("testci", "Test123!");

        sonuc.Should().NotBeNull();
        sonuc.KullaniciAdi.Should().Be("testci");
    }
}
