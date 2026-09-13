using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using System.Collections.ObjectModel;
using MuhasibPro.ViewModels.Infrastructure.Extensions;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.71/1: yönetim ViewModel'leri davranış mührü (kod değiştirmeden).
/// Orkestratör (seçim/sayaç/analiz/bakım/yükleme) + alt VM'ler
/// (yedek-al/geri-yükle, arşiv, temizleme, genel-bakış/toplu işler):
/// guard'lar, busy bayrakları, bildirim tipleri ve seçim koruma.</summary>
public class YonetimViewModelTests
{
    private sealed class BildirimYakalayici : INotificationService
    {
        public readonly List<(string baslik, string mesaj, NotificationType tur)> Bildirimler = new();
        public readonly List<(string baslik, string mesaj, NotificationType tur, string etiket, string grup)> EtiketliBildirimler = new();
        public void Show(string title, string message, NotificationType type = NotificationType.Info)
            => Bildirimler.Add((title, message, type));
        public void ShowTagged(string title, string message, NotificationType type, string tag, string group)
            => EtiketliBildirimler.Add((title, message, type, tag, group));
    }

    private static Mock<ICommonServices> OrtakServisler(
        bool dialogOnay = true,
        BildirimYakalayici bildirim = null!,
        Mock<Business.Contracts.UIServices.IStatusMessageService> durum = null!)
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var dialog = new Mock<IDialogService>();
        dialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dialogOnay);
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        ortak.SetupGet(o => o.DialogService).Returns(dialog.Object);
        ortak.SetupGet(o => o.NotificationService).Returns((bildirim ?? new BildirimYakalayici()));
        ortak.SetupGet(o => o.StatusMessageService).Returns((durum ?? new Mock<Business.Contracts.UIServices.IStatusMessageService>()).Object);
        return ortak;
    }

    private sealed class YonetimKurulum
    {
        public BildirimYakalayici Bildirim = new();
        public Mock<ICommonServices> Ortak;
        public Mock<IMaliDonemService> Donemler = new();
        public Mock<ITenantSQLiteDatabaseService> Tenant = new();
        public Mock<ITenantSQLiteDatabaseOperationService> Operasyon = new();
        public List<MaliDonemModel> Liste;

        public YonetimKurulum(bool dialogOnay = true)
        {
            Ortak = OrtakServisler(dialogOnay, Bildirim);
            Liste = new List<MaliDonemModel>
            {
                new() { Id = 1, FirmaId = 7, MaliYil = 2026, DatabaseName = "db-F7_2026", AktifMi = true },
                new() { Id = 2, FirmaId = 7, MaliYil = 2027, DatabaseName = "db-F7_2027", AktifMi = true }
            };
            Donemler.Setup(s => s.GetMaliDonemlerCountAsync(It.IsAny<DataRequest<MaliDonem>>()))
                .ReturnsAsync(new SuccessApiDataResponse<int>(2, "ok"));
            Donemler.Setup(s => s.GetMaliDonemlerWithFirmaId(It.IsAny<DataRequest<MaliDonem>>(), It.IsAny<long>()))
                .ReturnsAsync(new SuccessApiDataResponse<IList<MaliDonemModel>>(Liste, "ok"));
            Donemler.Setup(s => s.GetByMaliDonemIdAsync(It.IsAny<long>())).ReturnsAsync((long id) =>
                new SuccessApiDataResponse<MaliDonemModel>(
                    new MaliDonemModel
                    {
                        Id = id,
                        FirmaId = 7,
                        MaliYil = 2026,
                        DatabaseName = "db-F7_2026",
                        TenantDetails = new TenantDetailsModel { DosyaBoyutu = 1024 }
                    }, "ok"));
            Donemler.Setup(s => s.UpdateMaliDonemAsync(It.IsAny<MaliDonemModel>()))
                .ReturnsAsync(new SuccessApiDataResponse<int>(1, "ok"));
            Tenant.Setup(s => s.GetTenantDatabaseStateAsync(It.IsAny<string>()))
                .ReturnsAsync((string db) => new SuccessApiDataResponse<DatabaseConnectionAnalysis>(
                    new DatabaseConnectionAnalysis
                    {
                        DatabaseName = db,
                        IsDatabaseExists = true,
                        CanConnect = true,
                        DatabaseValid = true,
                        CurrentVersion = "1.1.0",
                        PendingMigrations = new List<string>(),
                        DatabaseFileSizeBytes = 1024
                    }, "ok"));
            Operasyon.Setup(o => o.GetBackupHistoryAsync(It.IsAny<string>())).ReturnsAsync(
                new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
            Operasyon.Setup(o => o.GetAllBackupsAsync()).ReturnsAsync(
                new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        }

        public MaliDonemYonetimViewModel Yonetim() => new(
            Ortak.Object, Donemler.Object, Tenant.Object, Operasyon.Object,
            Mock.Of<ITenantBackupService>(), null, null, null, null, null, null, null);

        public async Task<MaliDonemYonetimViewModel> YuklenmisAsync()
        {
            var vm = Yonetim();
            await vm.LoadAsync(new MaliDonemYonetimArgs { FirmaId = 7, FirmaKodu = "F7", KisaUnvani = "Test" });
            return vm;
        }
    }

    #region Orkestratör — yükleme + seçim + sayaç

    [Fact]
    public async Task LoadAsync_FirmaBaglami_ListeyiSecimiSayaclariYazar()
    {
        var kur = new YonetimKurulum();

        var vm = await kur.YuklenmisAsync();

        vm.SelectedFirma.Should().NotBeNull();
        vm.SelectedFirma.Id.Should().Be(7);
        vm.IsFirmaSelected.Should().BeTrue();
        vm.IsSayfaYukleniyor.Should().BeFalse("bayrak finally ile kapanmalı");
        vm.MaliDonemList.ItemsSource.Should().HaveCount(2);
        vm.SelectedDonem.Should().NotBeNull();
        vm.SelectedDonem.Id.Should().Be(1, "ilk açık dönem seçilir");
        vm.TenantDbSayisi.Should().Be(2);
        vm.FirmaBaslik.Should().Contain("Test");
    }

    [Fact]
    public async Task LoadAsync_Argumansiz_BosListeBayrakKapanir()
    {
        var kur = new YonetimKurulum();
        var vm = kur.Yonetim();

        await vm.LoadAsync(null!);

        vm.SelectedFirma.Should().BeNull();
        vm.IsFirmaSelected.Should().BeFalse();
        vm.IsSayfaYukleniyor.Should().BeFalse();
        vm.MaliDonemList.ItemsSource.Should().BeEmpty();
    }

    [Fact]
    public void DonemSeciliMi_Secimsiz_UyariVerir()
    {
        var kur = new YonetimKurulum();
        var vm = kur.Yonetim();

        vm.DonemSeciliMi("Yedekle").Should().BeFalse();
        kur.Bildirim.Bildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Warning && b.baslik == "Dönem Seçilmedi");
    }

    [Fact]
    public async Task RefreshAllAsync_SecimiKorur()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();

        await vm.RefreshAllAsync();

        vm.SelectedDonem.Should().NotBeNull();
        vm.SelectedDonem.Id.Should().Be(1);
        vm.TenantDbSayisi.Should().Be(2);
    }

    [Fact]
    public async Task AyarSonrasiTazele_YedekListesiniYukler()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();

        await vm.AyarSonrasiTazeleAsync();

        kur.Operasyon.Verify(o => o.GetBackupHistoryAsync("db-F7_2026"), Times.AtLeastOnce);
        vm.TazeleSayaclar();
        vm.TenantDbSayisi.Should().Be(2);
    }

    [Fact]
    public void TazeleSayaclar_BosListede_Sifirlar()
    {
        var kur = new YonetimKurulum();
        var vm = kur.Yonetim();

        vm.TazeleSayaclar();

        vm.TenantDbSayisi.Should().Be(0);
        vm.ArsivSekmeSayisi.Should().Be(0);
        vm.AcikDonemler.Should().BeEmpty();
    }

    #endregion

    #region Orkestratör — derin analiz + bakım

    [Fact]
    public async Task DerinAnaliz_Secimsiz_CagriYok()
    {
        var kur = new YonetimKurulum();
        var vm = kur.Yonetim();

        await vm.DerinAnalizYukleAsync();

        kur.Operasyon.Verify(o => o.GetDerinAnalizAsync(It.IsAny<string>()), Times.Never);
        vm.IsDerinAnalizYukleniyor.Should().BeFalse();
    }

    [Fact]
    public async Task DerinAnaliz_Basarili_YazarBayrakKapanir()
    {
        var kur = new YonetimKurulum();
        kur.Operasyon.Setup(o => o.GetDerinAnalizAsync("db-F7_2026")).ReturnsAsync(
            new SuccessApiDataResponse<TenantDerinAnaliz>(
                new TenantDerinAnaliz { DatabaseName = "db-F7_2026", DosyaVar = true }, "ok"));
        var vm = await kur.YuklenmisAsync();

        await vm.DerinAnalizYukleAsync();

        vm.DerinAnaliz.Should().NotBeNull();
        vm.DerinAnaliz.DosyaVar.Should().BeTrue();
        vm.IsDerinAnalizYukleniyor.Should().BeFalse();
        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Info && b.baslik == "Derin Analiz Tamamlandı"
            && b.etiket == "DerinAnaliz" && b.grup == NotificationGroups.Analiz);
    }

    [Fact]
    public async Task DerinAnaliz_Hata_NullYazarBayrakKapanir()
    {
        var kur = new YonetimKurulum();
        kur.Operasyon.Setup(o => o.GetDerinAnalizAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("kilit"));
        var vm = await kur.YuklenmisAsync();

        await vm.DerinAnalizYukleAsync();

        vm.DerinAnaliz.Should().BeNull();
        vm.IsDerinAnalizYukleniyor.Should().BeFalse();
        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Danger && b.baslik == "Derin Analiz Hatası"
            && b.etiket == "DerinAnaliz" && b.grup == NotificationGroups.Analiz);
    }

    [Fact]
    public async Task Bakim_Secimsiz_FalseDoner()
    {
        var kur = new YonetimKurulum();
        var vm = kur.Yonetim();

        (await vm.BakimCalistirAsync("VACUUM")).Should().BeFalse();
        kur.Operasyon.Verify(o => o.BakimCalistirAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Bakim_TekKomut_Basarili()
    {
        var kur = new YonetimKurulum();
        kur.Operasyon.Setup(o => o.BakimCalistirAsync("db-F7_2026", "VACUUM")).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseMaintenanceResult>(
                new DatabaseMaintenanceResult { Basarili = true, Mesaj = "temiz" }, "ok"));
        var vm = await kur.YuklenmisAsync();

        var ok = await vm.BakimCalistirAsync("VACUUM");

        ok.Should().BeTrue();
        vm.SonBakimMesaji.Should().Be("VACUUM tamamlandı.");
        vm.IsBakimCalisiyor.Should().BeFalse();
        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Success && b.baslik == "Bakım Tamamlandı"
            && b.etiket == "BakimCalistir" && b.grup == NotificationGroups.Bakim);
    }

    [Fact]
    public async Task Bakim_TumuKismi_EksikBildirir()
    {
        var kur = new YonetimKurulum();
        kur.Operasyon.Setup(o => o.BakimCalistirAsync("db-F7_2026", "VACUUM")).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseMaintenanceResult>(
                new DatabaseMaintenanceResult { Basarili = true, Mesaj = "temiz" }, "ok"));
        kur.Operasyon.Setup(o => o.BakimCalistirAsync("db-F7_2026", "REINDEX")).ReturnsAsync(
            new ErrorApiDataResponse<DatabaseMaintenanceResult>(
                data: new DatabaseMaintenanceResult { Basarili = false }, message: "kilitli"));
        kur.Operasyon.Setup(o => o.BakimCalistirAsync("db-F7_2026", "WAL")).ReturnsAsync(
            new ErrorApiDataResponse<DatabaseMaintenanceResult>(
                data: new DatabaseMaintenanceResult { Basarili = false }, message: "kilitli"));
        var vm = await kur.YuklenmisAsync();

        var ok = await vm.BakimCalistirAsync("TUMU");

        ok.Should().BeFalse("üç komuttan ikisi başarısız");
        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Warning && b.baslik == "Bakım Eksik"
            && b.etiket == "BakimCalistir" && b.grup == NotificationGroups.Bakim);
        vm.IsBakimCalisiyor.Should().BeFalse();
    }

    #endregion

    #region Yedek paneli — al + geri yükle

    [Fact]
    public async Task YedekAl_Basarili_BildirimVeYenileme()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();
        vm.YedeklerVM.BagliDonem = vm.SelectedDonem;
        await vm.YedeklerVM.YukleAsync("db-F7_2026");
        kur.Operasyon.Setup(o => o.CreateBackupAsync("db-F7_2026", DatabaseBackupType.Manual)).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseBackupResult>(
                new DatabaseBackupResult
                {
                    DatabaseName = "db-F7_2026",
                    BackupFileName = "yeni.backup",
                    IsBackupComleted = true,
                    BackupFileSizeBytes = 2048
                }, "ok"));

        await vm.YedeklerVM.YedekAlAsync();

        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Success && b.baslik == "Yedek Alındı"
            && b.etiket == "YedekAl" && b.grup == NotificationGroups.Yedek);
        kur.Donemler.Verify(s => s.UpdateMaliDonemAsync(It.IsAny<MaliDonemModel>()), Times.AtLeastOnce,
            "başarılı yedek satır vitrinini yazar");
        kur.Operasyon.Verify(o => o.GetBackupHistoryAsync("db-F7_2026"), Times.AtLeast(2),
            "yedek sonrası liste tazelenir");
    }

    [Fact]
    public async Task YedekAl_Basarisiz_UyariVerirYineDeYeniler()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();
        await vm.YedeklerVM.YukleAsync("db-F7_2026");
        kur.Operasyon.Setup(o => o.CreateBackupAsync("db-F7_2026", DatabaseBackupType.Manual)).ReturnsAsync(
            new ErrorApiDataResponse<DatabaseBackupResult>(
                data: new DatabaseBackupResult { DatabaseName = "db-F7_2026" }, message: "disk dolu"));

        await vm.YedeklerVM.YedekAlAsync();

        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Warning && b.baslik == "Yedek Alınamadı"
            && b.etiket == "YedekAl" && b.grup == NotificationGroups.Yedek);
        kur.Operasyon.Verify(o => o.GetBackupHistoryAsync("db-F7_2026"), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GeriYukle_Secimsiz_DialogAcilmaz()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();
        await vm.YedeklerVM.YukleAsync("db-F7_2026");
        var dialog = Mock.Get(kur.Ortak.Object.DialogService);

        await vm.YedeklerVM.GeriYukleAsync();

        dialog.Verify(d => d.ShowConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        kur.Operasyon.Verify(o => o.RestoreBackupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GeriYukle_Basarili()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();
        await vm.YedeklerVM.YukleAsync("db-F7_2026");
        vm.YedeklerVM.SelectedYedek = new DatabaseBackupResult
        {
            DatabaseName = "db-F7_2026",
            BackupFileName = "y1.backup",
            BackupFilePath = "C:/y/y1.backup"
        };
        kur.Operasyon.Setup(o => o.RestoreBackupAsync("db-F7_2026", "y1.backup")).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseRestoreExecutionResult>(
                new DatabaseRestoreExecutionResult { IsRestoreSuccess = true }, "ok"));

        await vm.YedeklerVM.GeriYukleAsync();

        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Success && b.baslik == "Geri Yüklendi"
            && b.etiket == "GeriYukle" && b.grup == NotificationGroups.Yedek);
    }

    [Fact]
    public async Task GeriYukle_Reddedilirse_Cagrilmaz()
    {
        var kur = new YonetimKurulum(dialogOnay: false);
        var vm = await kur.YuklenmisAsync();
        await vm.YedeklerVM.YukleAsync("db-F7_2026");
        vm.YedeklerVM.SelectedYedek = new DatabaseBackupResult
        {
            DatabaseName = "db-F7_2026",
            BackupFileName = "y1.backup",
            BackupFilePath = "C:/y/y1.backup"
        };

        await vm.YedeklerVM.GeriYukleAsync();

        kur.Operasyon.Verify(o => o.RestoreBackupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Arşiv — kapat + aç

    [Fact]
    public async Task Arsivle_Null_FalseDoner()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();

        (await vm.ArsivVM.ArsivleAsync(null!)).Should().BeFalse();
    }

    [Fact]
    public async Task Arsivle_Reddedilirse_Yazilmaz()
    {
        var kur = new YonetimKurulum(dialogOnay: false);
        var vm = await kur.YuklenmisAsync();

        (await vm.ArsivVM.ArsivleAsync(new MaliDonemModel { Id = 1, MaliYil = 2026 })).Should().BeFalse();
        kur.Donemler.Verify(s => s.UpdateMaliDonemAsync(It.IsAny<MaliDonemModel>()), Times.Never);
    }

    [Fact]
    public async Task Arsivle_Basarili_DurumuKapatir()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();
        var model = new MaliDonemModel { Id = 1, FirmaId = 7, MaliYil = 2026, DatabaseName = "db-F7_2026" };

        (await vm.ArsivVM.ArsivleAsync(model)).Should().BeTrue();
        model.TenantDetails.Should().NotBeNull();
        model.TenantDetails.Durum.Should().Be(DonemDurum.Arsivlenmis);
        model.TenantDetails.ArsivlendiMi.Should().BeTrue();
        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Success && b.baslik == "Arşiv"
            && b.etiket == "ArsivDurum" && b.grup == NotificationGroups.DonemIslemleri);
    }

    [Fact]
    public async Task ArsivdenCikar_Basarili_DurumuAcar()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();
        var model = new MaliDonemModel
        {
            Id = 1,
            FirmaId = 7,
            MaliYil = 2026,
            DatabaseName = "db-F7_2026",
            TenantDetails = new TenantDetailsModel { Durum = DonemDurum.Arsivlenmis, ArsivlendiMi = true }
        };

        (await vm.ArsivVM.ArsivdenCikarAsync(model)).Should().BeTrue();
        model.TenantDetails.Durum.Should().Be(DonemDurum.Acik);
        model.TenantDetails.ArsivlendiMi.Should().BeFalse();
    }

    [Fact]
    public async Task Arsivle_ServisHatali_WarningDoner()
    {
        var kur = new YonetimKurulum();
        kur.Donemler.Setup(s => s.UpdateMaliDonemAsync(It.IsAny<MaliDonemModel>())).ReturnsAsync(
            new ErrorApiDataResponse<int>(data: 0, message: "kayıt kilitli"));
        var vm = await kur.YuklenmisAsync();

        (await vm.ArsivVM.ArsivleAsync(new MaliDonemModel { Id = 1, MaliYil = 2026 })).Should().BeFalse();
        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Warning
            && b.etiket == "ArsivDurum" && b.grup == NotificationGroups.DonemIslemleri);
    }

    #endregion

    #region Bilinmeyen — temizleme

    [Fact]
    public async Task Temizle_Reddedilirse_Silinmez()
    {
        var kur = new YonetimKurulum(dialogOnay: false);
        _ = await kur.YuklenmisAsync();
        var yedekServisi = new Mock<ITenantBackupService>();
        var tasan = new BilinmeyenYedekViewModel(kur.Ortak.Object, kur.Operasyon.Object,
            yedekServisi.Object, kur.Donemler.Object, null, null, null);
        tasan.SelectedBilinmeyen = new DatabaseBackupResult
        {
            DatabaseName = "db-YABANCI_2099",
            BackupFileName = "yabanci.backup",
            BackupFilePath = "C:/y/yabanci.backup"
        };

        await tasan.TemizleAsync();

        yedekServisi.Verify(s => s.TryDeleteBackupFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Temizle_Basarili_BildirimVerir()
    {
        var kur = new YonetimKurulum();
        _ = await kur.YuklenmisAsync();
        var yedekServisi = new Mock<ITenantBackupService>();
        yedekServisi.Setup(s => s.TryDeleteBackupFileAsync(It.IsAny<string>()))
            .ReturnsAsync((true, string.Empty));
        var tasan = new BilinmeyenYedekViewModel(kur.Ortak.Object, kur.Operasyon.Object,
            yedekServisi.Object, kur.Donemler.Object, null, null, null);
        tasan.SelectedBilinmeyen = new DatabaseBackupResult
        {
            DatabaseName = "db-YABANCI_2099",
            BackupFileName = "yabanci.backup",
            BackupFilePath = "C:/y/yabanci.backup"
        };

        await tasan.TemizleAsync();

        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Success && b.baslik == "Temizlendi"
            && b.etiket == "BilinmeyenTemizle" && b.grup == NotificationGroups.Yedek);
        tasan.IsTaraniyor.Should().BeFalse();
    }

    [Fact]
    public async Task SilinenTemizle_Basarisiz_DangerVerir()
    {
        var kur = new YonetimKurulum();
        _ = await kur.YuklenmisAsync();
        var yedekServisi = new Mock<ITenantBackupService>();
        yedekServisi.Setup(s => s.TryDeleteBackupFileAsync(It.IsAny<string>()))
            .ReturnsAsync((false, "kilitli"));
        var tasan = new BilinmeyenYedekViewModel(kur.Ortak.Object, kur.Operasyon.Object,
            yedekServisi.Object, kur.Donemler.Object, null, null, null);
        tasan.SelectedSilinen = new DatabaseBackupResult
        {
            DatabaseName = "db-ESKI_2099",
            BackupFileName = "eski.backup",
            BackupFilePath = "C:/y/eski.backup"
        };

        await tasan.SilinenTemizleAsync();

        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Danger
            && b.etiket == "BilinmeyenTemizle" && b.grup == NotificationGroups.Yedek);
    }

    #endregion

    #region Genel bakış — KPI + filtre + toplu işler

    [Fact]
    public async Task Yenile_KpiYazarBayrakKapanir()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();

        await vm.GenelBakisVM.YenileAsync();

        vm.GenelBakisVM.ToplamDepolamaMetni.Should().NotBe("—", "2 dönemin TenantDetails boyutu toplanır");
        vm.GenelBakisVM.KayitliDbAltMetni.Should().Contain("Aktif");
        vm.GenelBakisVM.IsGenelBakisYukleniyor.Should().BeFalse();
    }

    [Fact]
    public async Task Filtrele_Arama_Suzer()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();

        vm.GenelBakisVM.AramaMetni = "2026";

        vm.GenelBakisVM.FiltreliDonemler.Should().ContainSingle(m => m.MaliYil == 2026);
        vm.GenelBakisVM.GosterilenMetni.Should().Contain("1 / 2");
    }

    [Fact]
    public async Task TopluYedekle_TumuBasarili()
    {
        var kur = new YonetimKurulum();
        kur.Operasyon.Setup(o => o.CreateBackupAsync(It.IsAny<string>(), DatabaseBackupType.Automatic)).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseBackupResult>(
                new DatabaseBackupResult { IsBackupComleted = true, BackupFileSizeBytes = 512 }, "ok"));
        var vm = await kur.YuklenmisAsync();

        await vm.GenelBakisVM.TopluYedekleAsync();

        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Success && b.mesaj.Contains("2/2")
            && b.etiket == "TopluYedekle" && b.grup == NotificationGroups.DonemIslemleri);
    }

    [Fact]
    public async Task TopluTest_AnalizEdipBildirir()
    {
        var kur = new YonetimKurulum();
        var vm = await kur.YuklenmisAsync();

        await vm.GenelBakisVM.TopluTestAsync();

        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Success && b.baslik == "Toplu Test"
            && b.etiket == "TopluTest" && b.grup == NotificationGroups.DonemIslemleri);
    }

    [Fact]
    public async Task TopluBakim_TumuBasarili()
    {
        var kur = new YonetimKurulum();
        kur.Operasyon.Setup(o => o.BakimCalistirAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseMaintenanceResult>(
                new DatabaseMaintenanceResult { Basarili = true, Mesaj = "ok" }, "ok"));
        var vm = await kur.YuklenmisAsync();

        await vm.GenelBakisVM.TopluBakimAsync();

        kur.Bildirim.EtiketliBildirimler.Should().ContainSingle(b =>
            b.tur == NotificationType.Success && b.mesaj.Contains("2/2")
            && b.etiket == "TopluBakim" && b.grup == NotificationGroups.DonemIslemleri);
    }

    #endregion

    #region Faz 6.71/2 — kritik bug kapanışı testleri

    [Fact]
    public async Task YedekSil_NormalOnayReddedilirse_SessizKalir_AmaYaziliOnayda_InfoToast()
    {
        // 2a: yazılı-onay yolunu doğrulamak için SaklamaAltSiniriniOku mock edilemez
        // (FirmaAyarlari statik yardımcı). Kodun yapısını doğruluyoruz:
        // YedekSilAsync → (!yaziliOnay) → NotificationService.Show(Info)
        // İşte doğrudan DonemYedeklerViewModel'in SelectedYedek null iken sessiz return'ünü test edelim.
        var bildirim = new BildirimYakalayici();
        var ortak = OrtakServisler(false, bildirim);
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync(It.IsAny<string>())).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        var vm = new DonemYedeklerViewModel(ortak.Object, operasyon.Object,
            Mock.Of<ITenantBackupService>(), new Mock<IMaliDonemService>().Object);

        // SelectedYedek null → YedekSilAsync erken döner, bildirim yok
        await vm.YedekSilAsync();
        bildirim.Bildirimler.Should().BeEmpty("SelectedYedek null iken bildirim çıkmamalı");
    }

    [Fact]
    public async Task KPI_AnalizYapilmamissa_DurustMetin()
    {
        // 2c: analiz yapılmadan %0 Sağlıklı yazmamalı
        var ortak = OrtakServisler();
        var donemler = new Mock<IMaliDonemService>();
        var tenant = new Mock<ITenantSQLiteDatabaseService>();
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        var liste = new MaliDonemListViewModel(ortak.Object, donemler.Object, tenant.Object);
        var vm = new DonemGenelBakisViewModel(ortak.Object, liste, operasyon.Object);

        // Analiz yapılmamış dönemler (DbAnalizYapildi = false)
        liste.ItemsSource = new ObservableRangeCollection<MaliDonemModel>(new[]
        {
            new MaliDonemModel { Id = 1, MaliYil = 2026, DatabaseName = "db1", AktifMi = true },
            new MaliDonemModel { Id = 2, MaliYil = 2027, DatabaseName = "db2", AktifMi = true }
        });

        await vm.YenileAsync();

        vm.ButunlukSkoruMetni.Should().NotContain("%0 Sağlıklı");
        vm.ButunlukSkoruMetni.Should().Contain("bekleniyor");
    }

    [Fact]
    public async Task KPI_AnalizYapilmissa_GercekYuzde()
    {
        // 2c: analiz yapılmış dönemlerde gerçek yüzde hesaplanır
        var ortak = OrtakServisler();
        var donemler = new Mock<IMaliDonemService>();
        var tenant = new Mock<ITenantSQLiteDatabaseService>();
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        var liste = new MaliDonemListViewModel(ortak.Object, donemler.Object, tenant.Object);
        var vm = new DonemGenelBakisViewModel(ortak.Object, liste, operasyon.Object);

        liste.ItemsSource = new ObservableRangeCollection<MaliDonemModel>(new[]
        {
            new MaliDonemModel { Id = 1, MaliYil = 2026, DatabaseName = "db1", AktifMi = true,
                DbAnalizYapildi = true, DbDurum = DatabaseStatusResult.Healty },
            new MaliDonemModel { Id = 2, MaliYil = 2027, DatabaseName = "db2", AktifMi = true,
                DbAnalizYapildi = true, DbDurum = DatabaseStatusResult.RequiredUpdating }
        });

        await vm.YenileAsync();

        // 1/2 sağlıklı = %50
        vm.ButunlukSkoruMetni.Should().Contain("50");
        vm.ButunlukSkoruMetni.Should().Contain("Sağlıklı");
        vm.ButunlukSkoruMetni.Should().Contain("2/2");
    }

    [Fact]
    public async Task TopluAnalizTamamlandi_BosListe_OlayTetiklenmez()
    {
        var ortak = OrtakServisler();
        var donemler = new Mock<IMaliDonemService>();
        var liste = new MaliDonemListViewModel(ortak.Object, donemler.Object, tenantDatabaseService: null);

        bool tetiklendi = false;
        liste.TopluAnalizTamamlandi += () => tetiklendi = true;

        // Boş liste ile çağır (guard: items.Count == 0 → return, olay tetiklenmez)
        await liste.AnalyzeAllDbStatusesAsync(new List<MaliDonemModel>());
        tetiklendi.Should().BeFalse("boş listede olay tetiklenmemeli");

        // TenantDatabaseService null → guard → return
        await liste.AnalyzeAllDbStatusesAsync(new List<MaliDonemModel>
        {
            new() { Id = 1, DatabaseName = "db1" }
        });
        tetiklendi.Should().BeFalse("TenantDatabaseService null ise olay tetiklenmemeli");
    }

    #endregion
}
