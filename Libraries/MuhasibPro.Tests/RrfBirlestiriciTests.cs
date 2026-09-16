using FluentAssertions;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.93: RrfBirlestirici füzyon matriksi.</summary>
public class RrfBirlestiriciTests
{
    [Fact]
    public void Iki_Liste_Birlesir_Ust_Sira_Kazanir()
    {
        var sonuc = RrfBirlestirici.Birlestir(["a", "b"], ["b", "a"]);

        sonuc.Should().HaveCount(2);
        sonuc[0].Anahtar.Should().Be("b");
    }

    [Fact]
    public void Vektor_Yoksa_Lexical_Tek_Basina()
    {
        var sonuc = RrfBirlestirici.Birlestir(["a", "b"], null, 1, 0);

        sonuc.Select(e => e.Anahtar).Should().ContainInOrder("a", "b");
    }

    [Fact]
    public void Agirlik_Sifirsa_Liste_Yok_Sayilir()
    {
        var sonuc = RrfBirlestirici.Birlestir(["a"], ["b"], 1, 0);

        sonuc.Select(e => e.Anahtar).Should().Equal("a");
    }

    [Fact]
    public void Bos_Girdi_Bos_Doner()
    {
        RrfBirlestirici.Birlestir(null, null).Should().BeEmpty();
        RrfBirlestirici.Birlestir([], []).Should().BeEmpty();
    }

    [Fact]
    public void Skor_Rrf_Formulune_Uyar()
    {
        var sonuc = RrfBirlestirici.Birlestir(["a"], ["a"], 0.4, 0.6);

        var beklenen = 0.4 / (RrfBirlestirici.K + 1) + 0.6 / (RrfBirlestirici.K + 1);
        sonuc.Should().HaveCount(1);
        sonuc[0].Skor.Should().BeApproximately(beklenen, 1e-9);
    }
}
