using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.92 Adım 5: YapayZekaAyarlarViewModel — yükle/kaydet/hazırla (gerçek provider üstünden).</summary>
public class YapayZekaAyarlarViewModelTests
{
    private sealed class BellekAyarlari : ILocalSettingsService
    {
        public readonly Dictionary<string, object> Kutu = new();
        public Task<T?> ReadSettingAsync<T>(string key)
        {
            if (Kutu.TryGetValue(key, out var raw) && raw is T deger)
                return Task.FromResult<T?>(deger);
            return Task.FromResult<T?>(default);
        }
        public Task SaveSettingAsync<T>(string key, T value)
        {
            Kutu[key] = value!;
            return Task.CompletedTask;
        }
    }

    private static IAuthenticationService Kimlik(long id, KullaniciRolTip? rol)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(rol.HasValue);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(rol.HasValue ? id : -1);
        auth.SetupGet(a => a.CurrentAccount).Returns(rol.HasValue
            ? new HesapModel
            {
                KullaniciId = id,
                KullaniciModel = new KullaniciModel { Rol = new KullaniciRolModel { RolTip = rol.Value } }
            }
            : null!);
        return auth.Object;
    }

    private static Mock<ICommonServices> OrtakServisler(Mock<IStatusMessageService> durum, Mock<IDialogService>? dialog = null)
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.StatusMessageService).Returns(durum.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.DialogService).Returns(dialog?.Object ?? Mock.Of<IDialogService>());
        return ortak;
    }

    private static YapayZekaAyarlarViewModel KurVm(
        BellekAyarlari bellek, IAuthenticationService auth, Mock<IStatusMessageService> durum,
        Mock<ISurumOzellikService>? surum = null, Mock<IAsistanSohbetService>? sohbet = null,
        Mock<IDialogService>? dialog = null)
    {
        var saglayici = new AiAsistanSettingsProvider(bellek, Mock.Of<IEventBus>(), auth);
        return new YapayZekaAyarlarViewModel(
            OrtakServisler(durum, dialog).Object, saglayici,
            surum?.Object, sohbet?.Object);
    }

    private static async Task<bool> BekleAsync(Func<bool> kosul, int tur = 100)
    {
        for (int i = 0; i < tur && !kosul(); i++)
            await Task.Delay(20);
        return kosul();
    }

    [Fact]
    public async Task Load_Doldurur()
    {
        var bellek = new BellekAyarlari();
        bellek.Kutu[AiAsistanSettings.SettingsKey] = new AiAsistanSettings { EtkinMi = false, EnFazlaMadde = 3 };
        var vm = KurVm(bellek, Kimlik(1, KullaniciRolTip.Yönetici), new Mock<IStatusMessageService>());

        await vm.LoadAsync();

        vm.EtkinMi.Should().BeFalse();
        vm.EnFazlaMadde.Should().Be(3);
        vm.IsYukleniyor.Should().BeFalse();
    }

    [Fact]
    public async Task Degisiklik_Otomatik_Kaydeder()
    {
        var bellek = new BellekAyarlari();
        var vm = KurVm(bellek, Kimlik(1, KullaniciRolTip.Yönetici), new Mock<IStatusMessageService>());
        await vm.LoadAsync();

        vm.EtkinMi = false;

        (await BekleAsync(() =>
            bellek.Kutu.TryGetValue(AiAsistanSettings.SettingsKey, out var raw)
            && raw is AiAsistanSettings s && !s.EtkinMi)).Should().BeTrue();
    }

    [Fact]
    public async Task ModelAlias_Sabit_Kalir()
    {
        var bellek = new BellekAyarlari();
        bellek.Kutu[AiAsistanSettings.SettingsKey] = new AiAsistanSettings { ModelAlias = "phi-4-mini" };
        var vm = KurVm(bellek, Kimlik(1, KullaniciRolTip.Yönetici), new Mock<IStatusMessageService>());

        await vm.LoadAsync();

        vm.ModelAlias.Should().Be(AiAsistanSettings.VarsayilanModelAlias);
    }

    [Fact]
    public async Task ModelDurumMetni_Hazirsa_HazirGosterir()
    {
        var sohbet = new Mock<IAsistanSohbetService>();
        sohbet.Setup(s => s.DurumuGetirAsync())
            .ReturnsAsync(new AsistanDurumDto { HazirMi = true, Mesaj = "qwen" });
        var vm = KurVm(new BellekAyarlari(), Kimlik(1, KullaniciRolTip.Yönetici),
            new Mock<IStatusMessageService>(), null, sohbet);

        await vm.LoadAsync();

        vm.ModelDurumMetni.Should().Contain("qwen");
    }

    [Fact]
    public async Task ModelDurumMetni_HazirDegilse_PanelYonlendirir()
    {
        var sohbet = new Mock<IAsistanSohbetService>();
        sohbet.Setup(s => s.DurumuGetirAsync())
            .ReturnsAsync(new AsistanDurumDto { HazirMi = false, Mesaj = string.Empty });
        var vm = KurVm(new BellekAyarlari(), Kimlik(1, KullaniciRolTip.Yönetici),
            new Mock<IStatusMessageService>(), null, sohbet);

        await vm.LoadAsync();

        vm.ModelDurumMetni.Should().Contain("panel");
    }

    [Fact]
    public async Task Surum_Yoksa_Kilit_Metni()
    {
        var surum = new Mock<ISurumOzellikService>();
        surum.Setup(s => s.AiAsistanHakkiAsync()).ReturnsAsync(new SurumHakkiDto
        {
            Tur = LisansTuru.Standart,
            GecerliMi = true,
            HakVarMi = false,
            Gerekce = "Profesyonel gerekir"
        });
        var vm = KurVm(new BellekAyarlari(), Kimlik(1, KullaniciRolTip.Yönetici),
            new Mock<IStatusMessageService>(), surum, null);

        await vm.LoadAsync();

        vm.SurumDurumMetni.Should().Contain("Profesyonel");
    }

    private static Mock<IAsistanSohbetService> SohbetServisi(
        IReadOnlyList<AsistanModelDto>? modeller = null, AsistanDiskKullanimiDto? disk = null)
    {
        var sohbet = new Mock<IAsistanSohbetService>();
        sohbet.Setup(s => s.DurumuGetirAsync()).ReturnsAsync(new AsistanDurumDto());
        sohbet.Setup(s => s.ModelleriGetirAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(modeller ?? new List<AsistanModelDto>());
        sohbet.Setup(s => s.DiskKullanimiAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(disk ?? new AsistanDiskKullanimiDto());
        return sohbet;
    }

    [Fact]
    public async Task Modeller_YalnizIndirilmisler_YukluOnce()
    {
        var sohbet = SohbetServisi(
            new List<AsistanModelDto>
            {
                new() { Alias = "a", GosterimAdi = "A", IndirildiMi = true, YukluMu = false, BoyutBayt = 1024 },
                new() { Alias = "b", GosterimAdi = "B", IndirildiMi = false },
                new() { Alias = "c", GosterimAdi = "C", IndirildiMi = true, YukluMu = true, BoyutBayt = 2048 },
            },
            new AsistanDiskKullanimiDto { ToplamBayt = 3072, ModelSayisi = 2 });
        var vm = KurVm(new BellekAyarlari(), Kimlik(1, KullaniciRolTip.Yönetici),
            new Mock<IStatusMessageService>(), null, sohbet);

        await vm.LoadAsync();

        vm.Modeller.Should().HaveCount(2);
        vm.Modeller[0].Alias.Should().Be("c");
        vm.Modeller[1].Alias.Should().Be("a");
        vm.ModelListesiBosMu.Should().BeFalse();
        vm.DiskMetni.Should().Contain("2 model");
        vm.IsModellerYukleniyor.Should().BeFalse();
    }

    [Fact]
    public async Task ModelSil_Onaylanirsa_Siler()
    {
        var dialog = new Mock<IDialogService>();
        dialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        var sohbet = SohbetServisi(new List<AsistanModelDto>
        {
            new() { Alias = "a", GosterimAdi = "A", IndirildiMi = true, YukluMu = true }
        });
        sohbet.Setup(s => s.ModelSilAsync("a", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AsistanIslemSonucuDto { BasariliMi = true, Mesaj = "Model silindi." });
        var vm = KurVm(new BellekAyarlari(), Kimlik(1, KullaniciRolTip.Yönetici),
            new Mock<IStatusMessageService>(), null, sohbet, dialog);
        await vm.LoadAsync();

        await vm.ModelSilAsync(vm.Modeller[0]);

        sohbet.Verify(s => s.ModelSilAsync("a", It.IsAny<CancellationToken>()), Times.Once);
        dialog.Verify(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        vm.IsModelIslemde.Should().BeFalse();
    }

    [Fact]
    public async Task ModelSil_Onaylanmazsa_Silmez()
    {
        var dialog = new Mock<IDialogService>();
        dialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        var sohbet = SohbetServisi(new List<AsistanModelDto>
        {
            new() { Alias = "a", GosterimAdi = "A", IndirildiMi = true }
        });
        var vm = KurVm(new BellekAyarlari(), Kimlik(1, KullaniciRolTip.Yönetici),
            new Mock<IStatusMessageService>(), null, sohbet, dialog);
        await vm.LoadAsync();

        await vm.ModelSilAsync(vm.Modeller[0]);

        sohbet.Verify(s => s.ModelSilAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "—")]
    [InlineData(0L, "—")]
    [InlineData(512L, "512 B")]
    [InlineData(1024L, "1 KB")]
    public void BaytMetni_Bicimler(long? bayt, string beklenen) =>
        AsistanModelSatiri.BaytMetni(bayt).Should().Be(beklenen);

    [Fact]
    public void BaytMetni_BuyukDeger_BirimBuyur() =>
        AsistanModelSatiri.BaytMetni(1536L * 1024 * 1024).Should().Contain("GB");
}
