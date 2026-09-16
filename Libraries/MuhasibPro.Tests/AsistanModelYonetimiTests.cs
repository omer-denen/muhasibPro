using FluentAssertions;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;

namespace MuhasibPro.Tests;

/// <summary>6.92 servis: model-yönetimi DTO şekli + klasör ölçümü (277 isteği; impl canlıda).</summary>
public class AsistanModelYonetimiTests
{
    [Fact]
    public void KlasorOlcer_Dosyalari_Toplar()
    {
        var kok = Path.Combine(Path.GetTempPath(), "muhasib-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(kok);
            Directory.CreateDirectory(Path.Combine(kok, "alt"));
            File.WriteAllBytes(Path.Combine(kok, "a.bin"), new byte[10]);
            File.WriteAllBytes(Path.Combine(kok, "alt", "b.bin"), new byte[25]);

            ModelKlasorOlcer.BaytToplaminiHesapla(kok).Should().Be(35);
        }
        finally
        {
            if (Directory.Exists(kok))
                Directory.Delete(kok, true);
        }
    }

    [Fact]
    public void KlasorOlcer_Yoksa_Sifir()
    {
        ModelKlasorOlcer.BaytToplaminiHesapla(null).Should().Be(0);
        ModelKlasorOlcer.BaytToplaminiHesapla("  ").Should().Be(0);
        ModelKlasorOlcer.BaytToplaminiHesapla(Path.Combine(Path.GetTempPath(), "muhasib-yok-" + Guid.NewGuid().ToString("N"))).Should().Be(0);
    }

    [Fact]
    public void IslemSonucu_Varsayilan_Basarisiz()
    {
        var sonuc = new AsistanIslemSonucuDto();

        sonuc.BasariliMi.Should().BeFalse();
        sonuc.Mesaj.Should().BeEmpty();
    }

    [Fact]
    public void ModelDto_Varsayilan()
    {
        var model = new AsistanModelDto();

        model.Alias.Should().BeEmpty();
        model.GosterimAdi.Should().BeEmpty();
        model.IndirildiMi.Should().BeFalse();
        model.YukluMu.Should().BeFalse();
        model.BoyutBayt.Should().BeNull();
    }
}
