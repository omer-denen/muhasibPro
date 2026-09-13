using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Data.Database.TenantDatabase;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Tests;

/// <summary>Oturum 127 derin bağlantı: TenantSettings değerleri (busy/pooling/bakım/retry)
/// Data katmanına parametreyle taşınır. Bekçi: Data Business bilmez.</summary>
public class TenantDerinBaglantiTests
{
    private static TenantSQLiteConnectionStringFactory KurFabrika(string dbYolu)
    {
        var paths = new Mock<IApplicationPaths>();
        paths.Setup(p => p.GetTenantDatabaseFilePath(It.IsAny<string>())).Returns(dbYolu);
        return new TenantSQLiteConnectionStringFactory(
            paths.Object,
            new Mock<ILogger<TenantSQLiteConnectionStringFactory>>().Object);
    }

    [Fact]
    public void ConnectionString_Varsayilan_PoolingAcik_5Sn()
    {
        var cs = KurFabrika("C:\\tmp\\x.db").CreateConnectionString("db-X");
        var b = new SqliteConnectionStringBuilder(cs);
        b.Pooling.Should().BeTrue();
        b.DefaultTimeout.Should().Be(5, "BusyTimeoutMs=5000 varsayılanı saniyeye çevrilir");
    }

    [Fact]
    public void ConnectionString_AyarDegerlerini_Tasir()
    {
        var cs = KurFabrika("C:\\tmp\\x.db").CreateConnectionString("db-X", busyTimeoutMs: 10000, pooling: false);
        var b = new SqliteConnectionStringBuilder(cs);
        b.Pooling.Should().BeFalse();
        b.DefaultTimeout.Should().Be(10);
    }

    [Fact]
    public void ConnectionString_BusyTimeout_Clamplenir()
    {
        var f = KurFabrika("C:\\tmp\\x.db");
        new SqliteConnectionStringBuilder(f.CreateConnectionString("db-X", busyTimeoutMs: 100)).DefaultTimeout.Should().Be(1);
        new SqliteConnectionStringBuilder(f.CreateConnectionString("db-X", busyTimeoutMs: 999999)).DefaultTimeout.Should().Be(30);
    }

    [Fact]
    public void StatikClamp_Null_Varsayilan_Disi_Clamp()
    {
        TenantSettings.ClampMigrationRetry(null).Should().Be(2);
        TenantSettings.ClampMigrationRetry(99).Should().Be(5);
        TenantSettings.ClampBusyTimeoutMs(null).Should().Be(5000);
        TenantSettings.BusyTimeoutMsToSeconds(null).Should().Be(5);
        TenantSettings.BusyTimeoutMsToSeconds(1500).Should().Be(1, "saniye çözünürlüğü aşağı yuvarlar, en az 1");
        TenantSettings.ClampBakimTimeoutSec(5).Should().Be(30);
        TenantSettings.ClampCommandTimeoutSec(9999).Should().Be(300);
    }

    [Fact]
    public async Task Bakim_AyarTimeout_Ile_Calisir()
    {
        var yol = Path.Combine(Path.GetTempPath(), $"bakim{Guid.NewGuid():N}.db");
        try
        {
            await using (var ac = new SqliteConnection($"Data Source={yol};"))
                await ac.OpenAsync();

            var paths = new Mock<IApplicationPaths>();
            paths.Setup(p => p.GetTenantDatabaseFilePath(It.IsAny<string>())).Returns(yol);
            var yonetici = new TenantSQLiteDatabaseManager(
                paths.Object,
                new Mock<ITenantSQLiteMigrationManager>().Object,
                new Mock<ILogger<TenantSQLiteDatabaseManager>>().Object);

            var sonuc = await yonetici.BakimCalistirAsync("db-X", "VACUUM", timeoutSec: 30);
            sonuc.Basarili.Should().BeTrue();

            var kotu = await yonetici.BakimCalistirAsync("db-X", "BOZUK-KOMUT", timeoutSec: 30);
            kotu.Basarili.Should().BeFalse();
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(yol)) File.Delete(yol);
        }
    }

    [Fact]
    public async Task SwitchTenant_AyarDegerlerini_Asagi_Tasir()
    {
        var manager = new Mock<ITenantSQLiteSelectionManager>();
        manager.SetupGet(m => m.IsTenantLoaded).Returns(false);
        manager.Setup(m => m.SwitchToTenantAsync(It.IsAny<TenantContext>()))
            .Returns(new TenantContext { DatabaseName = "db-yeni" });

        var dbManager = new Mock<ITenantSQLiteDatabaseManager>();
        dbManager.Setup(m => m.InitializeTenantDatabaseAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync(new DatabaseMigrationExecutionResult
            {
                DatabaseName = "db-yeni",
                HasError = false,
                CanConnect = true,
                DatabaseValid = true,
                Message = "ok"
            });

        var connFactory = new Mock<ITenantSQLiteConnectionStringFactory>();
        connFactory.Setup(m => m.ValidateConnectionStringAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync((true, "test", "Data Source=x"));

        var sistemLog = new Mock<ISistemLogService>();
        var logSvc = new Mock<ILogService>();
        logSvc.SetupGet(l => l.SistemLogService).Returns(sistemLog.Object);

        var ayar = new Mock<ITenantSettingsProvider>();
        ayar.Setup(a => a.GetAsync(It.IsAny<long>())).ReturnsAsync(new TenantSettings
        {
            BusyTimeoutMs = 10000,
            Pooling = false,
            CommandTimeoutSec = 45,
            MigrationRetry = 4
        });

        var service = new TenantSQLiteSelectionService(
            manager.Object,
            new Mock<IMessageService>().Object,
            logSvc.Object,
            dbManager.Object,
            connFactory.Object,
            ayar.Object);

        var sonuc = await service.SwitchTenantAsync("db-yeni");

        sonuc.Success.Should().BeTrue();
        connFactory.Verify(m => m.ValidateConnectionStringAsync("db-yeni", 10000, false), Times.Once);
        dbManager.Verify(m => m.InitializeTenantDatabaseAsync("db-yeni", 45, 4), Times.Once);
    }
}
