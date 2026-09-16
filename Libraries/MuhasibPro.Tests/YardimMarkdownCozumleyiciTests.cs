using FluentAssertions;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.93: YardimMarkdownCozumleyici matrizi (kendi fixture'ıyla; docs/yardim okunmaz).</summary>
public class YardimMarkdownCozumleyiciTests
{
    private const string Ornek = """
        # Deneme Sayfası

        ## İlk madde nasıl yapılır?
        Önce seçim yapılır, sonra onaylanır.
        Etiket: seçim, onay

        ## İkinci madde nedir?
        Yalnız bilgi verir.
        """;

    [Fact]
    public void Sayfa_Madde_Etiket_Cozulur()
    {
        var sonuc = YardimMarkdownCozumleyici.Cozumle("deneme.md", Ornek);

        sonuc.Should().HaveCount(2);
        sonuc[0].Kayit.Anahtar.Should().Be("deneme#1");
        sonuc[0].Kayit.Sayfa.Should().Be("Deneme Sayfası");
        sonuc[0].Kayit.Baslik.Should().Be("İlk madde nasıl yapılır?");
        sonuc[0].Kayit.Icerik.Should().Contain("Önce seçim yapılır");
        sonuc[0].Kayit.Icerik.Should().NotContain("Etiket:");
        sonuc[0].Kayit.Etiketler.Should().BeEquivalentTo("seçim", "onay");
        sonuc[1].Kayit.Anahtar.Should().Be("deneme#2");
        sonuc[1].Kayit.Etiketler.Should().BeEmpty();
    }

    [Fact]
    public void Anahtar_DosyaAdindan_Stabil()
    {
        var a = YardimMarkdownCozumleyici.Cozumle("01-giris.md", Ornek);
        var b = YardimMarkdownCozumleyici.Cozumle("01-giris.md", Ornek);

        a.Select(m => m.Kayit.Anahtar).Should().BeEquivalentTo(b.Select(m => m.Kayit.Anahtar));
        a.Select(m => m.IcerikHash).Should().BeEquivalentTo(b.Select(m => m.IcerikHash));
    }

    [Fact]
    public void Icerik_Degisince_Hash_Degisir()
    {
        var a = YardimMarkdownCozumleyici.Cozumle("d.md", Ornek);
        var b = YardimMarkdownCozumleyici.Cozumle("d.md", Ornek.Replace("onaylanır", "vazgeçilir"));

        a[0].IcerikHash.Should().NotBe(b[0].IcerikHash);
        a[1].IcerikHash.Should().Be(b[1].IcerikHash);
    }

    [Fact]
    public void Basliksiz_Metin_Madde_Uretmez()
    {
        var sonuc = YardimMarkdownCozumleyici.Cozumle("bos.md", "# Yalnız Başlık\n\nDüz metin.");

        sonuc.Should().BeEmpty();
    }

    [Fact]
    public void Bos_Girdi_Bos_Doner()
    {
        YardimMarkdownCozumleyici.Cozumle("b.md", "").Should().BeEmpty();
        YardimMarkdownCozumleyici.Cozumle("b.md", "   ").Should().BeEmpty();
    }

    [Fact]
    public void Hash_Baslik_Icerik_Birlesimidir()
    {
        var beklenen = YardimMarkdownCozumleyici.IcerikHashHesapla("B", "C");
        var sonuc = YardimMarkdownCozumleyici.Cozumle("h.md", "# S\n\n## B\nC");

        sonuc.Should().HaveCount(1);
        sonuc[0].IcerikHash.Should().Be(beklenen);
    }
}
