using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService.Common;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.71/1: tenant silme hattı davranış mührü (kod değiştirmeden).
/// Seviye 1 — TenantSQLiteDatabaseService.DeleteTenantDatabaseAsync varyantları;
/// Seviye 2 — TenantDatabaseSagaStep yedek-koruma/kısmi-silme;
/// Seviye 3 — TenantBackupService gerçek-dosya temizliği + TryDelete retry yolu.</summary>
public class TenantSilmeServisTests
{
    private sealed class SilmeKurulum
    {
        public Mock<IMaliDonemService> Donemler = new();
        public Mock<ITenantDatabaseSagaStep> SagaAdimi = new();
        public Mock<IMaliDonemSagaStep> DonemSaga = new();
        public Mock<ILogService> Log = new();

        public SilmeKurulum()
        {
            Log.SetupGet(l => l.SistemLogService).Returns(Mock.Of<ISistemLogService>());
        }

        public TenantSQLiteDatabaseService Servis() => new(
            Log.Object,
            Donemler.Object,
            Mock.Of<IFirmaService>(),
            Mock.Of<ILogger<TenantSQLiteDatabaseService>>(),
            Mock.Of<ILocalSettingsService>(),
            Mock.Of<IApplicationPaths>(),
            Mock.Of<ITenantSQLiteDatabaseLifecycleService>(),
            Mock.Of<ITenantSQLiteSelectionService>(),
            DonemSaga.Object,
            SagaAdimi.Object,
            Mock.Of<IAuthenticationService>(),
            Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<IKurulumKayitService>(),
            Mock.Of<IAppDbContextFactory>());

        public void Satir(string db, long id = 5) =>
            Donemler.Setup(s => s.GetByMaliDonemIdAsync(id)).ReturnsAsync(
                new SuccessApiDataResponse<MaliDonemModel>(
                    new MaliDonemModel { Id = id, FirmaId = 7, MaliYil = 2026, DatabaseName = db }, "ok"));
    }

    private static TenantDeletingRequest Istek(
        long id = 5,
        string db = "db-F7_2026",
        bool dosyayiSil = true,
        bool kaydiSil = false,
        bool yedekleriSil = false,
        bool onceYedekle = false) => new()
        {
            MaliDonemId = id,
            DatabaseName = db,
            IsDeleteDatabase = dosyayiSil,
            IsDeleteMaliDonem = kaydiSil,
            DeleteAllTenantBackup = yedekleriSil,
            IsCurrentTenantDeletingBeforeBackup = onceYedekle
        };

    private static TenantDeletingResult SagaSonucu(
        bool dosyaSilindi = true,
        bool yedekKuruldu = false,
        bool yedekSilindi = false) => new()
        {
            DatabaseName = "db-F7_2026",
            DatabaseDeleted = dosyaSilindi,
            DeleteCompleted = dosyaSilindi,
            BackupCreateCompleted = yedekKuruldu,
            IsCurrentTenantDeletingBeforeBackup = yedekKuruldu,
            BackupFilePath = yedekKuruldu ? "C:/yedek.backup" : null,
            BackupDeleteCompleted = yedekSilindi,
            DeletedBackupCount = 0,
            DeletedBackupFiles = new List<string>()
        };

    #region Seviye 1 — servis guard'ları

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task IdSifirVeyaNegatif_HataVerirSatiraBakmaz(long id)
    {
        var kur = new SilmeKurulum();

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek(id: id));

        sonuc.Success.Should().BeFalse();
        kur.Donemler.Verify(s => s.GetByMaliDonemIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task AdNull_HataVerir()
    {
        var kur = new SilmeKurulum();

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek(db: null!));

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SatirBulunamazsa_HataVerir()
    {
        var kur = new SilmeKurulum();
        kur.Donemler.Setup(s => s.GetByMaliDonemIdAsync(5)).ReturnsAsync(
            new ErrorApiDataResponse<MaliDonemModel>(data: null, message: "kayıt yok"));

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek());

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("kayıt yok");
    }

    [Fact]
    public async Task SatirAdUyusmazsa_HataVerir()
    {
        var kur = new SilmeKurulum();
        kur.Satir("db-GERCEK_2026");

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek(db: "db-YANLIS_2026"));

        sonuc.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CaseFarkliAd_KabulEdilir()
    {
        var kur = new SilmeKurulum();
        kur.Satir("DB-F7_2026");
        kur.SagaAdimi.Setup(s => s.DeleteTenantDatabaseAsync(
                It.IsAny<TenantOperationSaga>(), It.IsAny<TenantDeletingRequest>()))
            .ReturnsAsync(new SuccessApiDataResponse<TenantDeletingResult>(SagaSonucu(), "ok"));

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek(db: "db-f7_2026"));

        sonuc.Success.Should().BeTrue("ad eşleşmesi OrdinalIgnoreCase");
        sonuc.Data.DatabaseDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DosyaSilmeKapaliysa_SagaCagrilmazKayitYoluCalisir()
    {
        var kur = new SilmeKurulum();
        kur.Satir("db-F7_2026");

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek(dosyayiSil: false));

        sonuc.Success.Should().BeTrue();
        sonuc.Data.DatabaseDeleted.Should().BeFalse();
        kur.SagaAdimi.Verify(s => s.DeleteTenantDatabaseAsync(
            It.IsAny<TenantOperationSaga>(), It.IsAny<TenantDeletingRequest>()), Times.Never);
    }

    [Fact]
    public async Task SagaBasarisiz_HataVerir()
    {
        var kur = new SilmeKurulum();
        kur.Satir("db-F7_2026");
        kur.SagaAdimi.Setup(s => s.DeleteTenantDatabaseAsync(
                It.IsAny<TenantOperationSaga>(), It.IsAny<TenantDeletingRequest>()))
            .ReturnsAsync(new ErrorApiDataResponse<TenantDeletingResult>(
                data: new TenantDeletingResult { DatabaseDeleted = false }, message: "dosya kilitli"));

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek());

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("dosya kilitli");
    }

    [Fact]
    public async Task YedekKorulu_UyariylaDevamEderDosyaSilinir()
    {
        var kur = new SilmeKurulum();
        kur.Satir("db-F7_2026");
        kur.SagaAdimi.Setup(s => s.DeleteTenantDatabaseAsync(
                It.IsAny<TenantOperationSaga>(), It.IsAny<TenantDeletingRequest>()))
            .ReturnsAsync(new SuccessApiDataResponse<TenantDeletingResult>(SagaSonucu(), "ok"));

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek(yedekleriSil: false));

        sonuc.Success.Should().BeTrue();
        sonuc.Data.DatabaseDeleted.Should().BeTrue();
        sonuc.Data.BackupDeleteCompleted.Should().BeFalse("yedek silme kullanıcı tarafından atlandı");
    }

    [Fact]
    public async Task OnceYedekle_Yedeklenmisse_BayraklarTasinir()
    {
        var kur = new SilmeKurulum();
        kur.Satir("db-F7_2026");
        kur.SagaAdimi.Setup(s => s.DeleteTenantDatabaseAsync(
                It.IsAny<TenantOperationSaga>(), It.IsAny<TenantDeletingRequest>()))
            .ReturnsAsync(new SuccessApiDataResponse<TenantDeletingResult>(SagaSonucu(yedekKuruldu: true), "ok"));

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek(onceYedekle: true));

        sonuc.Success.Should().BeTrue();
        sonuc.Data.BackupCreateCompleted.Should().BeTrue();
        sonuc.Data.BackupFilePath.Should().Be("C:/yedek.backup");
    }

    [Fact]
    public async Task OnceYedekle_Yedeklenmemisse_HataIsaretlenirAmaAkisBiter()
    {
        // Mevcut davranış mührü (Oturum 208 "sahte Geri Al" ailesi): yedek kurulamayınca
        // MarkAsError işlenir ama akış Success ile biter — bug-kapanış fazı (6.71/2) netleştirir.
        var kur = new SilmeKurulum();
        kur.Satir("db-F7_2026");
        kur.SagaAdimi.Setup(s => s.DeleteTenantDatabaseAsync(
                It.IsAny<TenantOperationSaga>(), It.IsAny<TenantDeletingRequest>()))
            .ReturnsAsync(new SuccessApiDataResponse<TenantDeletingResult>(SagaSonucu(), "ok"));

        var sonuc = await kur.Servis().DeleteTenantDatabaseAsync(Istek(onceYedekle: true));

        sonuc.Success.Should().BeTrue("mevcut akış return etmeden devam eder");
        sonuc.Data.HasError.Should().BeTrue("yedek kurulamadı işareti korunur");
        sonuc.Data.BackupCreateCompleted.Should().BeFalse();
    }

    #endregion

    #region Seviye 2 — saga adımı yedek dalları

    private sealed class SagaKurulum
    {
        public Mock<ITenantSQLiteDatabaseLifecycleService> Yasam = new();
        public Mock<ITenantBackupService> Yedek = new();
        public Mock<IApplicationPaths> Yollar = new();
        private readonly string _klasor = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public SagaKurulum()
        {
            Directory.CreateDirectory(_klasor);
            Yollar.Setup(y => y.GetTenantBackupFolderPath()).Returns(_klasor);
            Yollar.Setup(y => y.GetTenantDatabaseFilePath(It.IsAny<string>()))
                .Returns((string ad) => Path.Combine(_klasor, ad + ".db"));
            Yasam.Setup(y => y.DeleteTenantDatabase(It.IsAny<string>())).ReturnsAsync(
                new SuccessApiDataResponse<DatabaseDeletingExecutionResult>(
                    new DatabaseDeletingExecutionResult { IsDeletedSuccess = true }, "silindi"));
        }

        public TenantDatabaseSagaStep Adim() => new(
            Yasam.Object,
            Yedek.Object,
            Yollar.Object,
            Mock.Of<ITenantSQLiteDatabaseOperationService>(),
            SecimYuksuz(),
            Mock.Of<IDatabaseBackupManager>());

        private static ITenantSQLiteSelectionService SecimYuksuz()
        {
            var secim = new Mock<ITenantSQLiteSelectionService>();
            secim.SetupGet(s => s.IsTenantLoaded).Returns(false);
            return secim.Object;
        }

        public void Temizle()
        {
            try { Directory.Delete(_klasor, true); } catch { }
        }
    }

    private static TenantOperationSaga Saga() => new(Mock.Of<ILogger>());

    [Fact]
    public async Task YedekKorulu_TemizligeDokunmazDosyaSilinir()
    {
        var kur = new SagaKurulum();
        try
        {
            var sonuc = await kur.Adim().DeleteTenantDatabaseAsync(Saga(), Istek(yedekleriSil: false));

            sonuc.Success.Should().BeTrue();
            sonuc.Data.DatabaseDeleted.Should().BeTrue();
            kur.Yedek.Verify(y => y.CleanAllBackupsAsync(It.IsAny<string>()), Times.Never);
        }
        finally { kur.Temizle(); }
    }

    [Fact]
    public async Task KismiSilme_AkisaDevamEder()
    {
        var kur = new SagaKurulum();
        try
        {
            kur.Yedek.Setup(y => y.CleanAllBackupsAsync("db-F7_2026")).ReturnsAsync(
                new SuccessApiDataResponse<TenantDeletingResult>(
                    new TenantDeletingResult
                    {
                        DatabaseName = "db-F7_2026",
                        BackupDeleteCompleted = true,
                        DeletedBackupCount = 1,
                        DeletedBackupFiles = new List<string> { "C:/y1.backup" }
                    }, "1/2 silindi"));

            var sonuc = await kur.Adim().DeleteTenantDatabaseAsync(Saga(), Istek(yedekleriSil: true));

            sonuc.Success.Should().BeTrue("kısmi başarı abort koşulunu (Success=false && tamamlanmadı) tutmaz");
            sonuc.Data.DatabaseDeleted.Should().BeTrue();
        }
        finally { kur.Temizle(); }
    }

    [Fact]
    public async Task HicbiriSilinemezse_AbortEder()
    {
        var kur = new SagaKurulum();
        try
        {
            kur.Yedek.Setup(y => y.CleanAllBackupsAsync("db-F7_2026")).ReturnsAsync(
                new ErrorApiDataResponse<TenantDeletingResult>(
                    data: new TenantDeletingResult { DatabaseName = "db-F7_2026" }, message: "kilitli"));

            var sonuc = await kur.Adim().DeleteTenantDatabaseAsync(Saga(), Istek(yedekleriSil: true));

            sonuc.Success.Should().BeFalse();
            sonuc.Message.Should().Contain("yedekleri silinemedi");
            kur.Yasam.Verify(y => y.DeleteTenantDatabase(It.IsAny<string>()), Times.Never);
        }
        finally { kur.Temizle(); }
    }

    #endregion

    #region Seviye 3 — gerçek-dosya temizliği + TryDelete

    private static Mock<IApplicationPaths> KlasorYolu(string klasor)
    {
        var yollar = new Mock<IApplicationPaths>();
        yollar.Setup(y => y.GetTenantBackupFolderPath()).Returns(klasor);
        return yollar;
    }

    [Fact]
    public async Task DizinYoksa_BasariliBosDoner()
    {
        var servis = new TenantBackupService(
            KlasorYolu(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).Object,
            Mock.Of<ITenantSQLiteDatabaseOperationService>());

        var sonuc = await servis.CleanAllBackupsAsync("db-X");

        sonuc.Success.Should().BeTrue();
        sonuc.Data.BackupDeleteCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task ListeBosken_BasariliBosDoner()
    {
        var klasor = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(klasor);
        try
        {
            var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
            operasyon.Setup(o => o.GetBackupHistoryAsync("db-X")).ReturnsAsync(
                new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
            var servis = new TenantBackupService(KlasorYolu(klasor).Object, operasyon.Object);

            var sonuc = await servis.CleanAllBackupsAsync("db-X");

            sonuc.Success.Should().BeTrue();
            sonuc.Data.DeletedBackupCount.Should().Be(0);
        }
        finally { try { Directory.Delete(klasor, true); } catch { } }
    }

    [Fact]
    public async Task GercekDosyalar_TumuSilinir()
    {
        var klasor = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(klasor);
        try
        {
            var yollar = new[] { Path.Combine(klasor, "y1.backup"), Path.Combine(klasor, "y2.backup") };
            foreach (var yol in yollar) await File.WriteAllTextAsync(yol, "yedek");
            var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
            operasyon.Setup(o => o.GetBackupHistoryAsync("db-X")).ReturnsAsync(
                new SuccessApiDataResponse<List<DatabaseBackupResult>>(yollar.Select(y =>
                    new DatabaseBackupResult { DatabaseName = "db-X", BackupFileName = Path.GetFileName(y), BackupFilePath = y }).ToList(), "ok"));
            var servis = new TenantBackupService(KlasorYolu(klasor).Object, operasyon.Object);

            var sonuc = await servis.CleanAllBackupsAsync("db-X");

            sonuc.Success.Should().BeTrue();
            sonuc.Data.BackupDeleteCompleted.Should().BeTrue();
            sonuc.Data.DeletedBackupCount.Should().Be(2);
            yollar.Should().OnlyContain(y => !File.Exists(y));
        }
        finally { try { Directory.Delete(klasor, true); } catch { } }
    }

    [Fact]
    public async Task OlmayanDosyalar_HataDoner()
    {
        var klasor = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(klasor);
        try
        {
            var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
            operasyon.Setup(o => o.GetBackupHistoryAsync("db-X")).ReturnsAsync(
                new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>
                {
                    new() { DatabaseName = "db-X", BackupFileName = "yok.backup", BackupFilePath = Path.Combine(klasor, "yok.backup") }
                }, "ok"));
            var servis = new TenantBackupService(KlasorYolu(klasor).Object, operasyon.Object);

            var sonuc = await servis.CleanAllBackupsAsync("db-X");

            sonuc.Success.Should().BeFalse();
            sonuc.Message.Should().Contain("Hiçbir yedek dosyası silinemedi");
        }
        finally { try { Directory.Delete(klasor, true); } catch { } }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TryDelete_BosYol_Basarisiz(string yol)
    {
        var servis = new TenantBackupService(
            Mock.Of<IApplicationPaths>(), Mock.Of<ITenantSQLiteDatabaseOperationService>());

        var (ok, neden) = await servis.TryDeleteBackupFileAsync(yol);

        ok.Should().BeFalse();
        neden.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TryDelete_OlmayanDosya_Basarisiz()
    {
        var servis = new TenantBackupService(
            Mock.Of<IApplicationPaths>(), Mock.Of<ITenantSQLiteDatabaseOperationService>());

        var (ok, neden) = await servis.TryDeleteBackupFileAsync(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".backup"));

        ok.Should().BeFalse();
        neden.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task TryDelete_GercekDosya_Siler()
    {
        var yol = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".backup");
        await File.WriteAllTextAsync(yol, "yedek");
        var servis = new TenantBackupService(
            Mock.Of<IApplicationPaths>(), Mock.Of<ITenantSQLiteDatabaseOperationService>());

        var (ok, neden) = await servis.TryDeleteBackupFileAsync(yol);

        ok.Should().BeTrue();
        neden.Should().BeEmpty();
        File.Exists(yol).Should().BeFalse();
    }

    [Fact]
    public async Task TryDelete_DizinYolu_RetrySonrasiBasarisiz()
    {
        // Dizin silinemez → retry döngüsü (5 deneme ~1,5sn) işletilir, neden yutulmadan döner.
        var klasor = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(klasor);
        try
        {
            var servis = new TenantBackupService(
                Mock.Of<IApplicationPaths>(), Mock.Of<ITenantSQLiteDatabaseOperationService>());

            var (ok, neden) = await servis.TryDeleteBackupFileAsync(klasor);

            ok.Should().BeFalse();
            neden.Should().NotBeNullOrEmpty("tek dosya silme nedeni VM Danger mesajına taşınır");
            Directory.Exists(klasor).Should().BeTrue("dizin silinmemiş olmalı");
        }
        finally { try { Directory.Delete(klasor, true); } catch { } }
    }

    #endregion
}
