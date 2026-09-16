using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.84 regresyonu: CTA seçim özeti + neden-tooltip + 0-dönem başlığı
/// (VM'den, hardcoded yok) + güncelleme bildirimi özeti + son-çalışılan işareti.</summary>
public class FirmaShellSecimDeneyimiTests
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

    private static FirmaModel Firma() => new() { Id = 7, FirmaKodu = "F-0007", KisaUnvani = "Korkut Mermer" };

    private static MaliDonemModel Donem(long id, int yil) => new()
    {
        Id = id,
        FirmaId = 7,
        MaliYil = yil,
        DatabaseName = $"db-F-0007_{yil}",
        AktifMi = true
    };

    private static FirmaShellViewModel KabukKur(Mock<ICommonServices> ortak)
    {
        var vm = new FirmaShellViewModel(
            ortak.Object, Mock.Of<IFirmaService>(), Mock.Of<IFilePickerService>(),
            Mock.Of<IMaliDonemService>(), Mock.Of<ILocalSettingsService>(),
            Mock.Of<IFirmaWithMaliDonemSelectedService>(), Mock.Of<ITenantSQLiteDatabaseService>(),
            Mock.Of<ITenantDatabaseUpdateService>(), null!);
        vm.ViewModelArgs = new ShellArgs();
        return vm;
    }

    private static MaliDonemListViewModel ListeKur(Mock<ICommonServices> ortak, Mock<IMaliDonemService> donemServisi) =>
        new(ortak.Object, donemServisi.Object, null!, null!);

    private static Mock<IMaliDonemService> DonemServisi(List<MaliDonemModel> liste)
    {
        var svc = new Mock<IMaliDonemService>();
        svc.Setup(s => s.GetMaliDonemlerCountAsync(It.IsAny<DataRequest<MaliDonem>>()))
            .ReturnsAsync(new SuccessApiDataResponse<int>(liste.Count, "ok"));
        svc.Setup(s => s.GetMaliDonemlerWithFirmaId(It.IsAny<DataRequest<MaliDonem>>(), It.IsAny<long>()))
            .ReturnsAsync(new SuccessApiDataResponse<IList<MaliDonemModel>>(liste, "ok"));
        return svc;
    }

    [Fact]
    public void SecimOzeti_FirmaYoksa_SecilmediMesaji()
    {
        var vm = KabukKur(OrtakServisler());

        vm.SecimOzeti.Should().Be("Firma seçilmedi");
        vm.DevamEtNedenTooltip.Should().Be("Önce firma ve mali dönem seçin");
        vm.BosDonemBaslik.Should().Be("Bu firmada mali dönem yok");
    }

    [Fact]
    public void SecimOzeti_FirmaVarDonemYoksa_FirmaAdiyla()
    {
        var vm = KabukKur(OrtakServisler());
        vm.Selection.SelectFirma(Firma());

        vm.SecimOzeti.Should().Be("Korkut Mermer • Dönem seçilmedi");
        vm.BosDonemBaslik.Should().Be("Korkut Mermer firmasında mali dönem yok");
    }

    [Fact]
    public void SecimOzeti_DonemDurumunu_Yansitir()
    {
        var vm = KabukKur(OrtakServisler());
        vm.Selection.SelectFirma(Firma());

        var acik = Donem(1, 2027);
        vm.Selection.SelectMaliDonem(acik);
        vm.SecimOzeti.Should().Be("Korkut Mermer • 2027 • Açık");
        vm.DevamEtNedenTooltip.Should().Be("Seçili firma ve dönemle çalışma alanına geç");

        var kapali = Donem(2, 2026);
        kapali.AktifMi = false;
        vm.Selection.SelectMaliDonem(kapali);
        vm.SecimOzeti.Should().Be("Korkut Mermer • 2026 • Kapalı");
        vm.DevamEtNedenTooltip.Should().Be("Kapalı döneme girilemez — açık bir dönem seçin");

        var guncelleme = Donem(3, 2025);
        guncelleme.DbAnalizYapildi = true;
        guncelleme.DbDurum = DatabaseStatusResult.RequiredUpdating;
        vm.Selection.SelectMaliDonem(guncelleme);
        vm.SecimOzeti.Should().Be("Korkut Mermer • 2025 • Güncelleme gerekli");

        var dosyaYok = Donem(4, 2024);
        dosyaYok.DbAnalizYapildi = true;
        dosyaYok.DbDurum = DatabaseStatusResult.DatabaseNotFound;
        vm.Selection.SelectMaliDonem(dosyaYok);
        vm.SecimOzeti.Should().Be("Korkut Mermer • 2024 • DB yok");
        vm.DevamEtNedenTooltip.Should().Be("Veritabanı dosyası yok — bu döneme girilemez");
    }

    [Fact]
    public async Task BosDurumGoster_FirmaSeciliListeBossa_Acik()
    {
        var liste = ListeKur(OrtakServisler(), DonemServisi(new List<MaliDonemModel>()));

        await liste.LoadAsync(new MaliDonemListArgs { FirmaId = 7 }, silent: true);

        liste.BosDurumGoster.Should().BeTrue();
    }

    [Fact]
    public async Task BosDurumGoster_ListeDoluyken_Kapali()
    {
        var liste = ListeKur(OrtakServisler(), DonemServisi(new List<MaliDonemModel> { Donem(1, 2027) }));

        await liste.LoadAsync(new MaliDonemListArgs { FirmaId = 7 }, silent: true);

        liste.BosDurumGoster.Should().BeFalse();
    }

    [Fact]
    public async Task GuncellemeBildirimi_BekleyenYoksa_Kapali()
    {
        var d = Donem(1, 2027);
        d.DbAnalizYapildi = true;
        d.DbDurum = DatabaseStatusResult.Healty;
        var liste = ListeKur(OrtakServisler(), DonemServisi(new List<MaliDonemModel> { d }));
        await liste.LoadAsync(new MaliDonemListArgs { FirmaId = 7 }, silent: true);

        liste.TazeleGuncellemeBildirimi();

        liste.GuncellemeVarMi.Should().BeFalse();
        liste.GuncellemeOzeti.Should().BeEmpty();
        liste.GuncellemeAksiyonMetni.Should().BeEmpty();
    }

    [Fact]
    public async Task GuncellemeBildirimi_TekDonemse_DogrudanGuncelle()
    {
        var eski = Donem(1, 2026);
        eski.DbAnalizYapildi = true;
        eski.DbDurum = DatabaseStatusResult.RequiredUpdating;
        var guncel = Donem(2, 2027);
        guncel.DbAnalizYapildi = true;
        guncel.DbDurum = DatabaseStatusResult.Healty;
        var liste = ListeKur(OrtakServisler(), DonemServisi(new List<MaliDonemModel> { eski, guncel }));
        await liste.LoadAsync(new MaliDonemListArgs { FirmaId = 7 }, silent: true);

        liste.TazeleGuncellemeBildirimi();

        liste.GuncellemeVarMi.Should().BeTrue();
        liste.GuncellemeOzeti.Should().Be("2026 dönemi için şema güncellemesi hazır. Dönemi seçtiğinizde güncelleme önerilir.");
        liste.GuncellemeAksiyonMetni.Should().Be("İncele");
        liste.GuncellemeBekleyenDonemler.Should().ContainSingle().Which.MaliYil.Should().Be(2026);
    }

    [Fact]
    public async Task GuncellemeBildirimi_CokDonemse_InceleListesi()
    {
        var d1 = Donem(1, 2025);
        d1.DbAnalizYapildi = true;
        d1.DbDurum = DatabaseStatusResult.RequiredUpdating;
        var d2 = Donem(2, 2027);
        d2.DbAnalizYapildi = true;
        d2.DbDurum = DatabaseStatusResult.RequiredUpdating;
        var liste = ListeKur(OrtakServisler(), DonemServisi(new List<MaliDonemModel> { d1, d2 }));
        await liste.LoadAsync(new MaliDonemListArgs { FirmaId = 7 }, silent: true);

        liste.TazeleGuncellemeBildirimi();

        liste.GuncellemeVarMi.Should().BeTrue();
        liste.GuncellemeOzeti.Should().Be("2 dönemde şema güncellemesi hazır: 2025, 2027.");
        liste.GuncellemeAksiyonMetni.Should().Be("İncele (2)");
        liste.GuncellemeBekleyenDonemler.Should().HaveCount(2);
    }

    [Fact]
    public async Task SonCalisilanIsareti_KayitliIdyeUyanSatirda()
    {
        var liste = ListeKur(OrtakServisler(), DonemServisi(
            new List<MaliDonemModel> { Donem(1, 2025), Donem(2, 2026), Donem(3, 2027) }));
        await liste.LoadAsync(new MaliDonemListArgs { FirmaId = 7 }, silent: true);

        liste.SonCalisilanDonemId = 2;
        liste.UygulaSonCalisilanIsareti();

        liste.ItemsSource.Should().HaveCount(3);
        liste.ItemsSource![0].SonCalisilanMi.Should().BeFalse();
        liste.ItemsSource![1].SonCalisilanMi.Should().BeTrue();
        liste.ItemsSource![2].SonCalisilanMi.Should().BeFalse();

        liste.SonCalisilanDonemId = 0;
        liste.UygulaSonCalisilanIsareti();

        liste.ItemsSource.Should().OnlyContain(m => !m.SonCalisilanMi);
    }
}
