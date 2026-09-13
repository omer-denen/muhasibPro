using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.71/1: tenant operasyon servisi davranış mührü (kod değiştirmeden).
/// Gerçek TenantSQLiteDatabaseOperationService + mock Data-manager'lar:
/// FIFO keep dalları, yedek/geri-yükleme hata dalları, derin-analiz Hata-dolu dalı,
/// bakım, tarih, liste ve passthrough çağrılar.</summary>
public class TenantOperationTests
{
    private sealed class OperasyonKurulum
    {
        public Mock<ITenantSQLiteBackupManager> Yedek = new();
        public Mock<ITenantSQLiteDatabaseManager> Veritabani = new();

        public TenantSQLiteDatabaseOperationService Servis() => new(
            Yedek.Object, Veritabani.Object, Mock.Of<ISistemLogService>());

        public DatabaseBackupResult YedekSatiri(string ad, bool gecerli = true) => new()
        {
            DatabaseName = "db-F7_2026",
            BackupFileName = ad,
            BackupFilePath = "C:/y/" + ad,
            IsBackupComleted = gecerli,
            Message = gecerli ? "ok" : "bozuk"
        };
    }

    #region CleanOldBackupsAsync — FIFO

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task KeepGecersiz_HataVerir(int keep)
    {
        var kur = new OperasyonKurulum();

        var sonuc = await kur.Servis().CleanOldBackupsAsync("db-F7_2026", keep);

        sonuc.Success.Should().BeFalse();
        sonuc.Data.Should().Be(-1);
        sonuc.Message.Should().Contain("En az 1 yedek");
    }

    [Fact]
    public async Task ListeBosken_SilmedenDoner()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.GetBackupsAsync("db-F7_2026")).ReturnsAsync(new List<DatabaseBackupResult>());

        var sonuc = await kur.Servis().CleanOldBackupsAsync("db-F7_2026", 5);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().Be(0);
        kur.Yedek.Verify(y => y.CleanOldBackupsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task YeterliYedek_SilmedenDoner()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.GetBackupsAsync("db-F7_2026")).ReturnsAsync(
            new List<DatabaseBackupResult> { kur.YedekSatiri("y1"), kur.YedekSatiri("y2") });

        var sonuc = await kur.Servis().CleanOldBackupsAsync("db-F7_2026", 5);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().Be(0);
        kur.Yedek.Verify(y => y.CleanOldBackupsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task FazlaYedek_SilerKalanlaDoner()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.SetupSequence(y => y.GetBackupsAsync("db-F7_2026"))
            .ReturnsAsync(new List<DatabaseBackupResult>
                { kur.YedekSatiri("y1"), kur.YedekSatiri("y2"), kur.YedekSatiri("y3"), kur.YedekSatiri("y4") })
            .ReturnsAsync(new List<DatabaseBackupResult> { kur.YedekSatiri("y3"), kur.YedekSatiri("y4") });
        kur.Yedek.Setup(y => y.CleanOldBackupsAsync("db-F7_2026", 2)).ReturnsAsync(2);

        var sonuc = await kur.Servis().CleanOldBackupsAsync("db-F7_2026", 2);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().Be(2);
        sonuc.Message.Should().Contain("2 adet eski backup");
    }

    [Fact]
    public async Task Silinemedi_HataVerir()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.GetBackupsAsync("db-F7_2026")).ReturnsAsync(
            new List<DatabaseBackupResult>
                { kur.YedekSatiri("y1"), kur.YedekSatiri("y2"), kur.YedekSatiri("y3") });
        kur.Yedek.Setup(y => y.CleanOldBackupsAsync("db-F7_2026", 1)).ReturnsAsync(0);

        var sonuc = await kur.Servis().CleanOldBackupsAsync("db-F7_2026", 1);

        sonuc.Success.Should().BeFalse();
        sonuc.Data.Should().Be(0);
        sonuc.Message.Should().Contain("silinirken bir hata");
    }

    #endregion

    #region CreateBackupAsync

    [Fact]
    public async Task YedekBasarili_SuccessDoner()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.CreateBackupAsync("db-F7_2026", DatabaseBackupType.Manual)).ReturnsAsync(
            new DatabaseBackupResult
            {
                DatabaseName = "db-F7_2026",
                BackupFileName = "f1.backup",
                IsBackupComleted = true,
                Message = "alındı"
            });

        var sonuc = await kur.Servis().CreateBackupAsync("db-F7_2026", DatabaseBackupType.Manual);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.IsBackupComleted.Should().BeTrue();
    }

    [Fact]
    public async Task YedekBasarisiz_HataDoner()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.CreateBackupAsync("db-F7_2026", DatabaseBackupType.Manual)).ReturnsAsync(
            new DatabaseBackupResult
            {
                DatabaseName = "db-F7_2026",
                IsBackupComleted = false,
                Message = "disk dolu"
            });

        var sonuc = await kur.Servis().CreateBackupAsync("db-F7_2026", DatabaseBackupType.Manual);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("disk dolu");
    }

    [Fact]
    public async Task YedekException_TeknikHataDoner()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.CreateBackupAsync(It.IsAny<string>(), It.IsAny<DatabaseBackupType>()))
            .ThrowsAsync(new InvalidOperationException("kilit"));

        var sonuc = await kur.Servis().CreateBackupAsync("db-F7_2026", DatabaseBackupType.Manual);

        sonuc.Success.Should().BeFalse();
        sonuc.Data.Should().BeNull();
        sonuc.Message.Should().Contain("teknik bir hata");
    }

    #endregion

    #region Liste + tarih + geri-yükleme

    [Fact]
    public async Task GecmisBosken_BasariliBosListe()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.GetBackupsAsync("db-F7_2026")).ReturnsAsync(new List<DatabaseBackupResult>());

        var sonuc = await kur.Servis().GetBackupHistoryAsync("db-F7_2026");

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GecmisKarisikken_GecerliSayisiniBildirir()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.GetBackupsAsync("db-F7_2026")).ReturnsAsync(
            new List<DatabaseBackupResult>
                { kur.YedekSatiri("y1"), kur.YedekSatiri("y2"), kur.YedekSatiri("bozuk", false) });

        var sonuc = await kur.Servis().GetBackupHistoryAsync("db-F7_2026");

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().HaveCount(3, "ham liste aynen döner");
        sonuc.Message.Should().Contain("2 adet geçerli");
    }

    [Fact]
    public async Task TumYedekler_Basarili()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.GetAllBackupsAsync()).ReturnsAsync(
            new List<DatabaseBackupResult> { kur.YedekSatiri("y1") });

        var sonuc = await kur.Servis().GetAllBackupsAsync();

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().ContainSingle();
    }

    [Fact]
    public async Task TumYedekler_HataDali()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.GetAllBackupsAsync()).ThrowsAsync(new InvalidOperationException("disk yok"));

        var sonuc = await kur.Servis().GetAllBackupsAsync();

        sonuc.Success.Should().BeFalse();
        sonuc.Data.Should().BeNull();
    }

    [Fact]
    public void SonYedekTarihi_DegerDoner()
    {
        var kur = new OperasyonKurulum();
        var gun = new DateTime(2026, 9, 1, 10, 0, 0);
        kur.Yedek.Setup(y => y.GetLastBackupDate("db-F7_2026")).Returns(gun);

        kur.Servis().GetLastBackupDate("db-F7_2026").Should().Be(gun);
    }

    [Fact]
    public void SonYedekTarihi_HataDaliNullDoner()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.GetLastBackupDate(It.IsAny<string>())).Throws(new InvalidOperationException("x"));

        kur.Servis().GetLastBackupDate("db-F7_2026").Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GeriYukle_BosAd_HataVerirManageraGitmez(string ad)
    {
        var kur = new OperasyonKurulum();

        var sonuc = await kur.Servis().RestoreBackupAsync("db-F7_2026", ad);

        sonuc.Success.Should().BeFalse();
        kur.Yedek.Verify(y => y.RestoreBackupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GeriYukle_Basarili()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.RestoreBackupAsync("db-F7_2026", "f1.backup")).ReturnsAsync(
            new DatabaseRestoreExecutionResult { IsRestoreSuccess = true, Message = "ok" });

        var sonuc = await kur.Servis().RestoreBackupAsync("db-F7_2026", "f1.backup");

        sonuc.Success.Should().BeTrue();
        sonuc.Data.IsRestoreSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GeriYukle_Basarisiz_HataVerir()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.RestoreBackupAsync("db-F7_2026", "f1.backup")).ReturnsAsync(
            new DatabaseRestoreExecutionResult { IsRestoreSuccess = false, Message = "dosya bozuk" });

        var sonuc = await kur.Servis().RestoreBackupAsync("db-F7_2026", "f1.backup");

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("dosya bozuk");
    }

    [Fact]
    public async Task SondanGeriYukle_Basarili()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.RestoreFromLatestBackupAsync("db-F7_2026")).ReturnsAsync(true);

        var sonuc = await kur.Servis().RestoreFromLatestBackupAsync("db-F7_2026");

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().BeTrue();
    }

    [Fact]
    public async Task SondanGeriYukle_Basarisiz()
    {
        var kur = new OperasyonKurulum();
        kur.Yedek.Setup(y => y.RestoreFromLatestBackupAsync("db-F7_2026")).ReturnsAsync(false);

        var sonuc = await kur.Servis().RestoreFromLatestBackupAsync("db-F7_2026");

        sonuc.Success.Should().BeFalse();
        sonuc.Data.Should().BeFalse();
    }

    #endregion

    #region Derin analiz + bakım

    [Fact]
    public async Task DerinAnaliz_Basarili()
    {
        var kur = new OperasyonKurulum();
        kur.Veritabani.Setup(v => v.GetDerinAnalizAsync("db-F7_2026")).ReturnsAsync(
            new TenantDerinAnaliz { DatabaseName = "db-F7_2026", DosyaVar = true, Hata = string.Empty });

        var sonuc = await kur.Servis().GetDerinAnalizAsync("db-F7_2026");

        sonuc.Success.Should().BeTrue();
        sonuc.Data.DosyaVar.Should().BeTrue();
    }

    [Fact]
    public async Task DerinAnaliz_HataDolu_HataDonerVeriKorunur()
    {
        var kur = new OperasyonKurulum();
        kur.Veritabani.Setup(v => v.GetDerinAnalizAsync("db-F7_2026")).ReturnsAsync(
            new TenantDerinAnaliz { DatabaseName = "db-F7_2026", Hata = "PRAGMA bozuk" });

        var sonuc = await kur.Servis().GetDerinAnalizAsync("db-F7_2026");

        sonuc.Success.Should().BeFalse();
        sonuc.Data.Should().NotBeNull("Hata-dolu dal veriyi taşır");
        sonuc.Message.Should().Contain("PRAGMA bozuk");
    }

    [Fact]
    public async Task DerinAnaliz_Exception_TeknikHata()
    {
        var kur = new OperasyonKurulum();
        kur.Veritabani.Setup(v => v.GetDerinAnalizAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("kilit"));

        var sonuc = await kur.Servis().GetDerinAnalizAsync("db-F7_2026");

        sonuc.Success.Should().BeFalse();
        sonuc.Data.Should().BeNull();
    }

    [Fact]
    public async Task Bakim_Basarili()
    {
        var kur = new OperasyonKurulum();
        kur.Veritabani.Setup(v => v.BakimCalistirAsync("db-F7_2026", "VACUUM", null)).ReturnsAsync(
            new DatabaseMaintenanceResult { Basarili = true, Mesaj = "temiz" });

        var sonuc = await kur.Servis().BakimCalistirAsync("db-F7_2026", "VACUUM");

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Basarili.Should().BeTrue();
    }

    [Fact]
    public async Task Bakim_Basarisiz_HataDoner()
    {
        var kur = new OperasyonKurulum();
        kur.Veritabani.Setup(v => v.BakimCalistirAsync("db-F7_2026", "WAL", null)).ReturnsAsync(
            new DatabaseMaintenanceResult { Basarili = false, Mesaj = "kilitli" });

        var sonuc = await kur.Servis().BakimCalistirAsync("db-F7_2026", "WAL");

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("kilitli");
    }

    #endregion
}
