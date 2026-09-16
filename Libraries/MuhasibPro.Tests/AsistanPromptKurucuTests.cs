using FluentAssertions;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.92 RAG v1: AsistanPromptKurucu — sistem/bağlam/madde/geçmiş.</summary>
public class AsistanPromptKurucuTests
{
    private static YardimSkorlayici.Eslesme Eslesme(string sayfa, string baslik) =>
        new(new YardimliSayfaDto { Anahtar = sayfa, Baslik = sayfa },
            new YardimMaddesiDto { Baslik = baslik, Aciklama = "açıklama" }, 3);

    [Fact]
    public void Sistem_Mesaji_Madde_Ve_Kural_Icerir()
    {
        var soru = new AsistanSoruDto { Soru = "nasıl arşivlerim?" };

        var mesajlar = AsistanPromptKurucu.MesajlariKur(new[] { Eslesme("Donem", "Arşivleme") }, soru, 4);

        mesajlar[0].Rol.Should().Be("system");
        mesajlar[0].Icerik.Should().Contain("Arşivleme");
        mesajlar[0].Icerik.Should().Contain("uydurma");
        mesajlar[^1].Rol.Should().Be("user");
        mesajlar[^1].Icerik.Should().Be("nasıl arşivlerim?");
    }

    [Fact]
    public void Baglam_Yazilir()
    {
        var soru = new AsistanSoruDto
        {
            Soru = "soru",
            SayfaAnahtari = "FirmaShell",
            FirmaAdi = "ABC Ltd",
            DonemAdi = "2026"
        };

        var mesajlar = AsistanPromptKurucu.MesajlariKur([], soru, 4);

        mesajlar[0].Icerik.Should().Contain("FirmaShell");
        mesajlar[0].Icerik.Should().Contain("ABC Ltd");
        mesajlar[0].Icerik.Should().Contain("2026");
    }

    [Fact]
    public void Gecmis_SonNTur_Kirpilir()
    {
        var soru = new AsistanSoruDto
        {
            Soru = "üçüncü soru",
            Gecmis = new[]
            {
                new AsistanMesajDto { Rol = "kullanici", Icerik = "ilk soru" },
                new AsistanMesajDto { Rol = "asistan", Icerik = "ilk cevap" },
                new AsistanMesajDto { Rol = "kullanici", Icerik = "ikinci soru" },
                new AsistanMesajDto { Rol = "asistan", Icerik = "ikinci cevap" }
            }
        };

        var mesajlar = AsistanPromptKurucu.MesajlariKur([], soru, 1);

        mesajlar.Should().HaveCount(4); // system + son tur (2) + soru
        string.Join(" ", mesajlar.Select(m => m.Icerik)).Should().NotContain("ilk soru");
        mesajlar[1].Rol.Should().Be("user");
        mesajlar[2].Rol.Should().Be("assistant");
    }

    [Fact]
    public void Bos_Soru_Hata()
    {
        var eylem = () => AsistanPromptKurucu.MesajlariKur([], new AsistanSoruDto { Soru = "  " }, 4);

        eylem.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Madde_Yoksa_Bos_Durum()
    {
        var mesajlar = AsistanPromptKurucu.MesajlariKur([], new AsistanSoruDto { Soru = "soru" }, 0);

        mesajlar.Should().HaveCount(2);
        mesajlar[0].Rol.Should().Be("system");
    }
}
