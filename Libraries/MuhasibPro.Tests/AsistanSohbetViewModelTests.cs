using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.92 Adım 5: AsistanSohbetViewModel — kapı + gönderim akışı.</summary>
public class AsistanSohbetViewModelTests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        return ortak;
    }

    private sealed class Kurulum
    {
        public Mock<IAsistanSohbetService> Sohbet = new();
        public Mock<IAiAsistanSettingsProvider> Ayarlar = new();
        public Mock<ISurumOzellikService> Surum = new();
        public Mock<IPermissionService> Yetki = new();
        public Mock<IFirmaWithMaliDonemSelectedService> Secim = new();

        public Kurulum()
        {
            Ayarlar.Setup(a => a.GetAsync()).ReturnsAsync(new AiAsistanSettings());
            Surum.Setup(s => s.AiAsistanHakkiAsync()).ReturnsAsync(new SurumHakkiDto
            {
                Tur = LisansTuru.Profesyonel,
                GecerliMi = true,
                HakVarMi = true
            });
            Yetki.Setup(y => y.HasPermissionAsync(Permission.AiAsistan_Kullan)).ReturnsAsync(true);
            Sohbet.Setup(s => s.DurumuGetirAsync()).ReturnsAsync(new AsistanDurumDto { HazirMi = true, Mesaj = "m" });
        }

        public AsistanSohbetViewModel Vm() => new(
            OrtakServisler().Object, Sohbet.Object, Ayarlar.Object, Surum.Object, Yetki.Object, Secim.Object);
    }

    private static async IAsyncEnumerable<string> ParcaUret(params string[] parcalar)
    {
        foreach (var p in parcalar)
        {
            yield return p;
            await Task.Yield();
        }
    }

    [Fact]
    public async Task Kilitli_LisansYok_Gondermez()
    {
        var kur = new Kurulum();
        kur.Surum.Setup(s => s.AiAsistanHakkiAsync()).ReturnsAsync(new SurumHakkiDto
        {
            Tur = LisansTuru.Standart,
            GecerliMi = true,
            HakVarMi = false,
            Gerekce = "Profesyonel gerekir"
        });
        var vm = kur.Vm();
        await vm.LoadAsync();

        vm.KilitliMi.Should().BeTrue();
        vm.KilitMetni.Should().Contain("Profesyonel");
        vm.SoruMetni = "selam";
        await vm.GonderAsync();

        vm.Mesajlar.Should().HaveCount(1);
        kur.Sohbet.Verify(s => s.SorStreamingAsync(It.IsAny<AsistanSoruDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Yetkisiz_Kilitli()
    {
        var kur = new Kurulum();
        kur.Yetki.Setup(y => y.HasPermissionAsync(Permission.AiAsistan_Kullan)).ReturnsAsync(false);
        var vm = kur.Vm();

        await vm.LoadAsync();

        vm.KilitliMi.Should().BeTrue();
        vm.KilitMetni.Should().Contain("yetki");
    }

    [Fact]
    public async Task EtkinDegilse_Kilitli()
    {
        var kur = new Kurulum();
        kur.Ayarlar.Setup(a => a.GetAsync()).ReturnsAsync(new AiAsistanSettings { EtkinMi = false });
        var vm = kur.Vm();

        await vm.LoadAsync();

        vm.KilitliMi.Should().BeTrue();
        vm.KilitMetni.Should().Contain("kapalı");
    }

    [Fact]
    public async Task Gonder_Akim_Ekler()
    {
        var kur = new Kurulum();
        kur.Sohbet.Setup(s => s.SorStreamingAsync(It.IsAny<AsistanSoruDto>(), It.IsAny<CancellationToken>()))
            .Returns(ParcaUret("Mer", "haba"));
        var vm = kur.Vm();
        await vm.LoadAsync();

        vm.SoruMetni = "selam";
        await vm.GonderAsync();

        vm.Mesajlar.Should().HaveCount(3);
        vm.Mesajlar[1].KullaniciMi.Should().BeTrue();
        vm.Mesajlar[1].Icerik.Should().Be("selam");
        vm.Mesajlar[2].AsistanMi.Should().BeTrue();
        vm.Mesajlar[2].Icerik.Should().Be("Merhaba");
        vm.IsGonderiliyor.Should().BeFalse();
        vm.SoruMetni.Should().BeEmpty();
        kur.Sohbet.Verify(s => s.SorStreamingAsync(
            It.Is<AsistanSoruDto>(q => q.Soru == "selam" && q.SayfaAnahtari == "MainShell"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Bos_Soru_Gondermez()
    {
        var kur = new Kurulum();
        var vm = kur.Vm();
        await vm.LoadAsync();

        vm.SoruMetni = "  ";
        await vm.GonderAsync();

        vm.Mesajlar.Should().HaveCount(1);
        kur.Sohbet.Verify(s => s.SorStreamingAsync(It.IsAny<AsistanSoruDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HazirDegilse_Once_Hazirlar()
    {
        var kur = new Kurulum();
        kur.Sohbet.Setup(s => s.DurumuGetirAsync()).ReturnsAsync(new AsistanDurumDto { HazirMi = false });
        kur.Sohbet.Setup(s => s.SorStreamingAsync(It.IsAny<AsistanSoruDto>(), It.IsAny<CancellationToken>()))
            .Returns(ParcaUret("ok"));
        var vm = kur.Vm();
        await vm.LoadAsync();

        vm.SoruMetni = "selam";
        await vm.GonderAsync();

        kur.Sohbet.Verify(s => s.HazirlaAsync(It.IsAny<IProgress<AsistanDurumDto>>(), It.IsAny<CancellationToken>()), Times.Once);
        kur.Sohbet.Verify(s => s.SorStreamingAsync(It.IsAny<AsistanSoruDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
