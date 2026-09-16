using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.92: SurumOzellikService — 4 tür × geçerli/geçersiz kapı matrisi (fail-closed).</summary>
public class SurumOzellikServiceTests
{
    private static SurumOzellikService KurServis(LisansTuru tur, bool gecerliMi)
    {
        var lisans = new Mock<ILisansService>();
        lisans.Setup(s => s.GetLisansDurumuAsync()).ReturnsAsync(
            new SuccessApiDataResponse<LisansDurumDto>(
                new LisansDurumDto { Tur = tur, GecerliMi = gecerliMi }, string.Empty));
        return new SurumOzellikService(lisans.Object);
    }

    [Fact]
    public async Task Deneme_Gecerli_Hak_Var()
    {
        var hak = await KurServis(LisansTuru.Deneme, true).AiAsistanHakkiAsync();

        hak.HakVarMi.Should().BeTrue();
        hak.Tur.Should().Be(LisansTuru.Deneme);
    }

    [Fact]
    public async Task Standart_Gecerli_Hak_Yok()
    {
        var hak = await KurServis(LisansTuru.Standart, true).AiAsistanHakkiAsync();

        hak.HakVarMi.Should().BeFalse();
        hak.Gerekce.Should().Contain("Profesyonel");
    }

    [Fact]
    public async Task Profesyonel_Gecerli_Hak_Var()
    {
        var hak = await KurServis(LisansTuru.Profesyonel, true).AiAsistanHakkiAsync();

        hak.HakVarMi.Should().BeTrue();
    }

    [Fact]
    public async Task Kurumsal_Gecerli_Hak_Var()
    {
        var hak = await KurServis(LisansTuru.Kurumsal, true).AiAsistanHakkiAsync();

        hak.HakVarMi.Should().BeTrue();
    }

    [Fact]
    public async Task Gecersiz_Lisans_Hak_Yok()
    {
        var hak = await KurServis(LisansTuru.Profesyonel, false).AiAsistanHakkiAsync();

        hak.HakVarMi.Should().BeFalse();
        hak.GecerliMi.Should().BeFalse();
    }

    [Fact]
    public async Task Servis_Hatasi_FailClosed()
    {
        var lisans = new Mock<ILisansService>();
        lisans.Setup(s => s.GetLisansDurumuAsync()).ThrowsAsync(new InvalidOperationException("db yok"));
        var svc = new SurumOzellikService(lisans.Object);

        var hak = await svc.AiAsistanHakkiAsync();

        hak.HakVarMi.Should().BeFalse();
    }
}
