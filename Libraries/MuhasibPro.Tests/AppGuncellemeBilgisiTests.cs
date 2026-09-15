using FluentAssertions;
using MuhasibPro.Domain;

namespace MuhasibPro.Tests;

/// <summary>Gömülü güncelleme kaynağının (git remote) normalize edilmesi — repo adresi sabit değil.
/// Örnekler Domain'deki <see cref="AppGuncellemeBilgisi.NormalizeOrnekleri"/> ile ORTAK kaynaktan gelir;
/// aynı kaynak dev-mode "Kaynağı Doğrula" öz-testini de besler.</summary>
public class AppGuncellemeBilgisiTests
{
    public static IEnumerable<object[]> Ornekler =>
        AppGuncellemeBilgisi.NormalizeOrnekleri.Select(o => new object[] { o.Ham, o.Beklenen });

    [Theory]
    [MemberData(nameof(Ornekler))]
    public void Normalize_Ornekler_BeklenenSonucuVerir(string ham, string beklenen)
        => AppGuncellemeBilgisi.Normalize(ham).Should().Be(beklenen);

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Normalize_BosBosluk_BosDoner(string ham)
        => AppGuncellemeBilgisi.Normalize(ham).Should().BeEmpty();

    [Fact]
    public void OzTest_TumOrneklerGecer()
    {
        var sonuc = AppGuncellemeBilgisi.OzTest();

        sonuc.Basarili.Should().BeTrue();
        sonuc.Gecen.Should().Be(sonuc.Toplam);
        sonuc.Toplam.Should().Be(AppGuncellemeBilgisi.NormalizeOrnekleri.Count);
        sonuc.Detaylar.Should().HaveCount(sonuc.Toplam);
    }
}
