using FluentAssertions;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.92 RAG v1: YardimSkorlayici eşleştirme matrisi.</summary>
public class YardimSkorlayiciTests
{
    private static YardimliSayfaDto Sayfa(string anahtar, params (string Baslik, string Aciklama)[] maddeler) =>
        new()
        {
            Anahtar = anahtar,
            Baslik = anahtar,
            Maddeler = maddeler.Select(m => new YardimMaddesiDto { Baslik = m.Baslik, Aciklama = m.Aciklama }).ToList()
        };

    [Fact]
    public void Baslik_Eslesmesi_Aciklama_Onunde()
    {
        var sayfalar = new[]
        {
            Sayfa("A", ("Arşivleme", "ilgisisiz metin")),
            Sayfa("B", ("Genel bilgi", "arşivleme buradan yapılır"))
        };

        var sonuc = YardimSkorlayici.IlgiliMaddeleriBul("arşivleme", sayfalar, 6);

        sonuc.Should().HaveCount(2);
        sonuc[0].Sayfa.Anahtar.Should().Be("A");
        sonuc[0].Skor.Should().BeGreaterThan(sonuc[1].Skor);
    }

    [Fact]
    public void Eslesme_Yoksa_Bos()
    {
        var sayfalar = new[] { Sayfa("A", ("Yedekleme", "yedek alınır")) };

        var sonuc = YardimSkorlayici.IlgiliMaddeleriBul("bordro tahakkuku", sayfalar, 6);

        sonuc.Should().BeEmpty();
    }

    [Fact]
    public void EnFazla_Limiti()
    {
        var sayfalar = new[]
        {
            Sayfa("A", ("Yedek al", "yedek"), ("Yedek sil", "yedek"), ("Yedek liste", "yedek"))
        };

        var sonuc = YardimSkorlayici.IlgiliMaddeleriBul("yedek", sayfalar, 2);

        sonuc.Should().HaveCount(2);
    }

    [Fact]
    public void BuyukKucukHarf_Duyarsiz()
    {
        var sayfalar = new[] { Sayfa("A", ("Yedekleme", "yedek alınır")) };

        var sonuc = YardimSkorlayici.IlgiliMaddeleriBul("YEDEK", sayfalar, 6);

        sonuc.Should().HaveCount(1);
    }

    [Fact]
    public void Bos_Soru_Bos_Doner()
    {
        var sayfalar = new[] { Sayfa("A", ("Yedekleme", "yedek alınır")) };

        YardimSkorlayici.IlgiliMaddeleriBul("  ", sayfalar, 6).Should().BeEmpty();
        YardimSkorlayici.IlgiliMaddeleriBul("yedek", sayfalar, 0).Should().BeEmpty();
    }
}
