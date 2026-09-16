using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.DevServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.92 Adım 6: dev-mode AI Bağlantı Öz-testi.</summary>
public class GelistiriciAiOzTestTests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        ortak.SetupGet(o => o.StatusMessageService).Returns(Mock.Of<IStatusMessageService>());
        return ortak;
    }

    private static GelistiriciAraclariViewModel KurVm(
        Mock<ISurumOzellikService>? surum = null, Mock<IAsistanSohbetService>? sohbet = null) =>
        new(OrtakServisler().Object, Mock.Of<IDevModeProvider>(), Mock.Of<IDevAraclariService>(),
            Mock.Of<IYolAciciService>(), surumService: surum?.Object, asistanSohbet: sohbet?.Object);

    [Fact]
    public async Task ServislerYoksa_EksikSatirlari()
    {
        var vm = KurVm();

        await vm.AiOzTestAsync();

        vm.AiOzTestVar.Should().BeTrue();
        vm.AiOzTestDetayi.Should().Contain("çözülemedi");
        vm.AiOzTestDetayi.Should().Contain("Yardım dizini:");
    }

    [Fact]
    public async Task HakYoksa_Gerekce_Satirda()
    {
        var surum = new Mock<ISurumOzellikService>();
        surum.Setup(s => s.AiAsistanHakkiAsync()).ReturnsAsync(new SurumHakkiDto
        {
            Tur = LisansTuru.Standart, GecerliMi = true, HakVarMi = false, Gerekce = "Profesyonel gerekir"
        });
        var vm = KurVm(surum);

        await vm.AiOzTestAsync();

        vm.AiOzTestDetayi.Should().Contain("Standart — kapalı");
        vm.AiOzTestDetayi.Should().Contain("Profesyonel gerekir");
    }

    [Fact]
    public async Task Hazirsa_Detay_Gecti()
    {
        var surum = new Mock<ISurumOzellikService>();
        surum.Setup(s => s.AiAsistanHakkiAsync()).ReturnsAsync(new SurumHakkiDto
        {
            Tur = LisansTuru.Profesyonel, GecerliMi = true, HakVarMi = true
        });
        var sohbet = new Mock<IAsistanSohbetService>();
        sohbet.Setup(s => s.DurumuGetirAsync())
            .ReturnsAsync(new AsistanDurumDto { HazirMi = true, Mesaj = "qwen" });
        var vm = KurVm(surum, sohbet);

        await vm.AiOzTestAsync();

        vm.AiOzTestDetayi.Should().Contain("Profesyonel — AI açık");
        vm.AiOzTestDetayi.Should().Contain("Model durumu: Hazır (qwen).");
        vm.SonucMesaji.Should().Contain("geçti");
    }
}
