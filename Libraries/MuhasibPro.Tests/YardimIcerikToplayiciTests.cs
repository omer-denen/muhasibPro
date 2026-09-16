using FluentAssertions;
using MuhasibPro.ViewModels.Services;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.92 RAG v1: YardimIcerikToplayici — 8 sayfa tek kaynaktan.</summary>
public class YardimIcerikToplayiciTests
{
    [Fact]
    public void Sekiz_Sayfa()
    {
        new YardimIcerikToplayici().TumSayfalariGetir().Should().HaveCount(8);
    }

    [Fact]
    public void Her_Sayfa_EnAz_Bir_Madde()
    {
        var sayfalar = new YardimIcerikToplayici().TumSayfalariGetir();

        foreach (var sayfa in sayfalar)
        {
            sayfa.Maddeler.Should().NotBeEmpty($"sayfa boş olamaz: {sayfa.Anahtar}");
            sayfa.Baslik.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void Anahtarlar_Benzersiz()
    {
        var anahtarlar = new YardimIcerikToplayici().TumSayfalariGetir().Select(s => s.Anahtar);

        anahtarlar.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void SayfaGetir_Bilinmeyen_Bos()
    {
        new YardimIcerikToplayici().SayfaGetir("YokBoyleSayfa").Should().BeEmpty();
    }

    [Fact]
    public void SayfaGetir_Login_Dolu()
    {
        var maddeler = new YardimIcerikToplayici().SayfaGetir("login");

        maddeler.Should().NotBeEmpty();
    }
}
