using FluentAssertions;
using MuhasibPro.Domain.Utilities;

namespace MuhasibPro.Tests;

/// <summary>SemVer tek kaynağı: normalleştirme + sayısal karşılaştırma + eski takvim şeması uyumu.</summary>
public class SemanticVersionTests
{
    [Theory]
    [InlineData("1.1.0", "1.1.0")]
    [InlineData("1.1", "1.1.0")]
    [InlineData("1", "1.0.0")]
    [InlineData("1.1.15.1430", "1.1.15")]
    [InlineData(" 1.2.3 ", "1.2.3")]
    public void Normalize_SemVer_UcParcayaIner(string giris, string beklenen)
    {
        SemanticVersion.Normalize(giris).Should().Be(beklenen);
    }

    [Theory]
    [InlineData("1.2026.0908.0", "0.908.0")]
    [InlineData("1.2026.0828.1709", "0.828.1709")]
    public void Normalize_EskiTakvimSemasi_SifirBandinaIner(string giris, string beklenen)
    {
        SemanticVersion.Normalize(giris).Should().Be(beklenen);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1.0.0.04DD0C")]
    [InlineData("sürüm-yok")]
    public void Normalize_Cop_SifirBandinaDuser(string? giris)
    {
        SemanticVersion.Normalize(giris).Should().Be("0.0.0");
    }

    [Theory]
    [InlineData("1.1.4-beta", "1.1.4")]
    [InlineData("1.1.4+build.2", "1.1.4")]
    [InlineData("1.1.4-rc.1+exp.sha.5114f85", "1.1.4")]
    public void Normalize_OnSurumVeMetadata_YokSayilir(string giris, string beklenen)
    {
        SemanticVersion.Normalize(giris).Should().Be(beklenen);
    }

    [Fact]
    public void Karsilastirma_OnSurumEki_YokSayilir()
    {
        SemanticVersion.IsEqual("1.1.4-beta", "1.1.4").Should().BeTrue();
        SemanticVersion.IsEqual("1.1.4+build", "1.1.4").Should().BeTrue();
        SemanticVersion.IsValid("1.1.4-beta").Should().BeTrue();
    }

    [Fact]
    public void Karsilastirma_Sayisaldir_StringDegil()
    {
        SemanticVersion.IsGreater("1.10.0", "1.9.0").Should().BeTrue();
        SemanticVersion.IsLess("1.9.0", "1.10.0").Should().BeTrue();
        SemanticVersion.IsEqual("1.1.0", "1.1.0").Should().BeTrue();
    }

    [Fact]
    public void EskiTakvimDamgasi_HerSemVerdenKucuktur()
    {
        SemanticVersion.IsLess("1.2026.0908.0", "1.0.0").Should().BeTrue();
        SemanticVersion.IsLess("1.2026.0828.1709", "1.2026.0908.0").Should().BeTrue();
    }

    [Fact]
    public void Gecerlilik_UcParcaSayisal()
    {
        SemanticVersion.IsValid("1.1.0").Should().BeTrue();
        SemanticVersion.IsValid("1.1").Should().BeFalse();
        SemanticVersion.IsValid("1.1.0.0").Should().BeFalse();
        SemanticVersion.IsValid(null).Should().BeFalse();
    }
}
