using FluentAssertions;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;

namespace MuhasibPro.Tests;

/// <summary>D2-D3 donanım hükmü (Oturum 292): eşik sınırları + null/ölçülemeyen durumu.</summary>
public class DonanimUygunlukTests
{
    private static readonly DonanimUygunlukService Svc = new();

    [Theory]
    [InlineData(8192, DonanimHukmu.Uygun)]
    [InlineData(16384, DonanimHukmu.Uygun)]
    [InlineData(8191, DonanimHukmu.Sinirda)]
    [InlineData(6144, DonanimHukmu.Sinirda)]
    [InlineData(6143, DonanimHukmu.Yetersiz)]
    [InlineData(0, DonanimHukmu.Yetersiz)]
    [InlineData(-1, DonanimHukmu.Yetersiz)]
    public void Degerlendir_Esikler(long ramMb, DonanimHukmu beklenen)
    {
        Svc.Degerlendir(new DonanimBilgisiDto { KullanilabilirRamMb = ramMb })
            .Should().Be(beklenen);
    }

    [Fact]
    public void Degerlendir_Null_Firlatir()
    {
        var eylem = () => Svc.Degerlendir(null!);

        eylem.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Olc_Deger_Dondurur()
    {
        var bilgi = Svc.Olc();

        bilgi.IslemciCekirdek.Should().BeGreaterThan(0);
    }
}
