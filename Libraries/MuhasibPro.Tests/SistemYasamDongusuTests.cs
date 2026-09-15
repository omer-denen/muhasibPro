using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;
using System.Collections.Generic;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.79: Sistem.db yaşam-döngüsü paketi — açılış/kapanış best-effort zincir testleri.</summary>
public class SistemYasamDongusuTests
{
    private readonly Mock<ISistemBackupManager> _yedek;
    private readonly Mock<ISistemDatabaseOperationService> _operasyon;
    private readonly Mock<IDatabaseSettingsProvider> _ayarlar;
    private readonly Mock<ISistemLogService> _log;
    private readonly Mock<IApplicationPaths> _yollar;

    public SistemYasamDongusuTests()
    {
        _yedek = new Mock<ISistemBackupManager>();
        _yedek.Setup(m => m.CheckpointWalAsync()).ReturnsAsync(true);
        _operasyon = new Mock<ISistemDatabaseOperationService>();
        _operasyon.Setup(o => o.GetBackupHistoryAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        _ayarlar = new Mock<IDatabaseSettingsProvider>();
        _ayarlar.Setup(s => s.GetAsync(It.IsAny<long>())).ReturnsAsync(
            new DatabaseSettingsModel { WeeklyBackupDays = 7, SistemKeepLast = 3 });
        _log = new Mock<ISistemLogService>();
        _yollar = new Mock<IApplicationPaths>();
        _yollar.Setup(p => p.SistemDatabaseFileExists()).Returns(true);
        _yollar.Setup(p => p.GetSistemDatabaseFilePath()).Returns("C:\\test\\Sistem.db");
        _yollar.Setup(p => p.IsSqliteDatabaseFileValid(It.IsAny<string>())).Returns(true);
    }

    private SistemYasamDongusuService Servis() =>
        new(_yedek.Object, _operasyon.Object, _ayarlar.Object, _log.Object, _yollar.Object);

    // ---- Faz 6.91-B: güncelleme öncesi zorunlu yedek ----

    [Fact]
    public async Task Guncelleme_Oncesi_Yedek_Alir()
    {
        _operasyon.Setup(o => o.CreateBackupAsync(DatabaseBackupType.Migration)).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseBackupResult>(
                new DatabaseBackupResult { BackupFilePath = "C:\\bk\\sistem_pre.backup", IsBackupComleted = true }, "ok"));
        _operasyon.Setup(o => o.CleanOldBackupsAsync(3)).ReturnsAsync(
            new SuccessApiDataResponse<int>(0, "temiz"));

        var (basarili, yol, _) = await Servis().EnsureUpdateSafetyAsync();

        basarili.Should().BeTrue();
        yol.Should().Be("C:\\bk\\sistem_pre.backup");
        _yedek.Verify(m => m.CheckpointWalAsync(), Times.Once);
        _operasyon.Verify(o => o.CreateBackupAsync(DatabaseBackupType.Migration), Times.Once);
        _operasyon.Verify(o => o.CleanOldBackupsAsync(3), Times.Once);
    }

    [Fact]
    public async Task Guncelleme_Yedek_Alinamazsa_Durdurur()
    {
        _operasyon.Setup(o => o.CreateBackupAsync(DatabaseBackupType.Migration)).ReturnsAsync(
            new ErrorApiDataResponse<DatabaseBackupResult>(null!, "disk hatası"));

        var (basarili, yol, mesaj) = await Servis().EnsureUpdateSafetyAsync();

        basarili.Should().BeFalse();
        yol.Should().BeNull();
        mesaj.Should().Contain("durduruldu");
    }

    [Fact]
    public async Task Guncelleme_SistemDb_Yoksa_Engellemez()
    {
        _yollar.Setup(p => p.SistemDatabaseFileExists()).Returns(false);

        var (basarili, yol, _) = await Servis().EnsureUpdateSafetyAsync();

        basarili.Should().BeTrue();
        yol.Should().BeNull();
        _operasyon.Verify(o => o.CreateBackupAsync(It.IsAny<DatabaseBackupType>()), Times.Never);
    }

    [Fact]
    public async Task Guncelleme_SistemDb_Gecersizse_Engellemez()
    {
        _yollar.Setup(p => p.IsSqliteDatabaseFileValid(It.IsAny<string>())).Returns(false);

        var (basarili, yol, _) = await Servis().EnsureUpdateSafetyAsync();

        basarili.Should().BeTrue();
        yol.Should().BeNull();
        _operasyon.Verify(o => o.CreateBackupAsync(It.IsAny<DatabaseBackupType>()), Times.Never);
    }

    [Fact]
    public async Task Acilis_BayatYedekse_OtomatikAlir()
    {
        _operasyon.Setup(o => o.GetLastBackupDate()).Returns(DateTime.Now.AddDays(-10));
        _operasyon.Setup(o => o.CreateBackupAsync(DatabaseBackupType.Automatic)).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseBackupResult>(
                new DatabaseBackupResult { BackupFileName = "oto.backup", IsBackupComleted = true }, "ok"));
        _operasyon.Setup(o => o.CleanOldBackupsAsync(3)).ReturnsAsync(
            new SuccessApiDataResponse<int>(0, "temiz"));

        var (checkpoint, yedekAlindi, mesaj) = await Servis().EnsureStartupSafetyAsync();

        checkpoint.Should().BeTrue();
        yedekAlindi.Should().BeTrue();
        _operasyon.Verify(o => o.CleanOldBackupsAsync(3), Times.Once);
        mesaj.Should().Contain("Otomatik yedek alındı");
    }

    [Fact]
    public async Task Acilis_TazeYedekse_Almaz()
    {
        _operasyon.Setup(o => o.GetLastBackupDate()).Returns(DateTime.Now.AddDays(-1));

        var (checkpoint, yedekAlindi, mesaj) = await Servis().EnsureStartupSafetyAsync();

        checkpoint.Should().BeTrue();
        yedekAlindi.Should().BeFalse();
        _operasyon.Verify(o => o.CreateBackupAsync(It.IsAny<DatabaseBackupType>()), Times.Never);
        mesaj.Should().Contain("güncel");
    }

    [Fact]
    public async Task Acilis_CheckpointBasarisizsa_DevamEder()
    {
        _yedek.Setup(m => m.CheckpointWalAsync()).ReturnsAsync(false);
        _operasyon.Setup(o => o.GetLastBackupDate()).Returns(DateTime.Now);

        var (checkpoint, yedekAlindi, _) = await Servis().EnsureStartupSafetyAsync();

        checkpoint.Should().BeFalse();
        yedekAlindi.Should().BeFalse();
    }

    [Fact]
    public async Task Acilis_YedekPatlarsa_BilgiDoner_Firlatmaz()
    {
        _operasyon.Setup(o => o.GetLastBackupDate()).Returns(DateTime.Now.AddDays(-30));
        _operasyon.Setup(o => o.CreateBackupAsync(DatabaseBackupType.Automatic)).ReturnsAsync(
            new ErrorApiDataResponse<DatabaseBackupResult>(null!, "disk hatası"));

        var (checkpoint, yedekAlindi, mesaj) = await Servis().EnsureStartupSafetyAsync();

        checkpoint.Should().BeTrue();
        yedekAlindi.Should().BeFalse();
        mesaj.Should().Contain("alınamadı");
    }

    [Fact]
    public async Task Kapanis_Kapaliysa_YalnizCheckpoint()
    {
        string mesaj = await Servis().EnsureShutdownSafetyAsync(false);

        _yedek.Verify(m => m.CheckpointWalAsync(), Times.Once);
        _operasyon.Verify(o => o.CreateBackupAsync(It.IsAny<DatabaseBackupType>()), Times.Never);
        mesaj.Should().Contain("WAL");
    }

    [Fact]
    public async Task Kapanis_Aciksa_YedekAlir()
    {
        _operasyon.Setup(o => o.CreateBackupAsync(DatabaseBackupType.Automatic)).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseBackupResult>(
                new DatabaseBackupResult { BackupFileName = "kapanis.backup", IsBackupComleted = true }, "ok"));
        _operasyon.Setup(o => o.CleanOldBackupsAsync(3)).ReturnsAsync(
            new SuccessApiDataResponse<int>(0, "temiz"));

        string mesaj = await Servis().EnsureShutdownSafetyAsync(true);

        _operasyon.Verify(o => o.CreateBackupAsync(DatabaseBackupType.Automatic), Times.Once);
        mesaj.Should().Contain("kapanış yedeği alındı");
    }
}
