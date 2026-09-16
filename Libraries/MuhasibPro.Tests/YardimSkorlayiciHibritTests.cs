using FluentAssertions;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.93: YardimSkorlayici Türkçe normalizasyon + starts-with (mevcut 5 fact aynen yeşil).</summary>
public class YardimSkorlayiciHibritTests
{
    [Fact]
    public void Ascii_Sorgu_Diakritikli_Maddeyi_Bulur()
    {
        var kayitlar = new[]
        {
            ("d#1", "Arşivleme", "dönem arşivlenir", ""),
            ("d#2", "Yedekleme", "yedek alınır", "")
        };

        var sonuc = YardimSkorlayici.KayitlariSirala("arsivleme nasil yapilir", kayitlar);

        sonuc.Should().NotBeEmpty();
        sonuc[0].Anahtar.Should().Be("d#1");
    }

    [Fact]
    public void Baslik_Onden_Gelir()
    {
        var kayitlar = new[]
        {
            ("d#1", "Genel bilgi", "arşivleme buradan yapılır", ""),
            ("d#2", "Arşivleme", "ilgisisiz metin", "")
        };

        var sonuc = YardimSkorlayici.KayitlariSirala("arşivleme", kayitlar);

        sonuc.Should().HaveCount(2);
        sonuc[0].Anahtar.Should().Be("d#2");
    }

    [Fact]
    public void Etiket_Eslesmesi_Skor_Verir()
    {
        var kayitlar = new[]
        {
            ("d#1", "Genel bilgi", "ilgisisiz metin", "arşiv, dönem"),
            ("d#2", "Başka konu", "tamamen farklı içerik", "yedek")
        };

        var sonuc = YardimSkorlayici.KayitlariSirala("arsiv", kayitlar);

        sonuc.Should().HaveCount(1);
        sonuc[0].Anahtar.Should().Be("d#1");
    }
}
