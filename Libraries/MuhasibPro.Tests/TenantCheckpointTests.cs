using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Data.Database.TenantDatabase;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.73: `CheckpointAndReleaseAsync` — gerçek dosyada WAL checkpoint (TRUNCATE) + havuz bırakma.</summary>
public class TenantCheckpointTests
{
    private static TenantSQLiteDatabaseManager Yonetici(string dbPath, string databaseName)
    {
        var paths = new Mock<IApplicationPaths>();
        paths.Setup(p => p.GetTenantDatabaseFilePath(databaseName)).Returns(dbPath);
        return new TenantSQLiteDatabaseManager(
            paths.Object, Mock.Of<ITenantSQLiteMigrationManager>(), NullLogger<TenantSQLiteDatabaseManager>.Instance);
    }

    [Fact]
    public async Task Checkpoint_WalModluDosya_Basarili_ve_WalBosaltilir()
    {
        var kok = Path.Combine(Path.GetTempPath(), "muhasib-wal-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(kok);
        var dbPath = Path.Combine(kok, "db-test.db");
        var walPath = dbPath + "-wal";
        try
        {
            using (var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadWriteCreate;Pooling=False;"))
            {
                await conn.OpenAsync();
                using var pragma = conn.CreateCommand();
                pragma.CommandText = "PRAGMA journal_mode=WAL;";
                await pragma.ExecuteNonQueryAsync();
                using var create = conn.CreateCommand();
                create.CommandText = "CREATE TABLE T(Id INTEGER PRIMARY KEY, X TEXT);";
                await create.ExecuteNonQueryAsync();
                using var insert = conn.CreateCommand();
                insert.CommandText = "INSERT INTO T(X) VALUES('a');";
                await insert.ExecuteNonQueryAsync();
            }

            var (ok, mesaj) = await Yonetici(dbPath, "db-test").CheckpointAndReleaseAsync("db-test");

            ok.Should().BeTrue(mesaj);
            mesaj.Should().Contain("Havuz bırakıldı");
            if (File.Exists(walPath))
                new FileInfo(walPath).Length.Should().Be(0, "TRUNCATE checkpoint WAL'i boşaltır");
        }
        finally
        {
            if (Directory.Exists(kok))
                Directory.Delete(kok, true);
        }
    }

    [Fact]
    public async Task Checkpoint_DosyaYok_Basarisiz()
    {
        var yok = Path.Combine(Path.GetTempPath(), "muhasib-yok-" + Guid.NewGuid().ToString("N") + ".db");

        var (ok, mesaj) = await Yonetici(yok, "db-yok").CheckpointAndReleaseAsync("db-yok");

        ok.Should().BeFalse();
        mesaj.Should().Contain("bulunamadı");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Checkpoint_BosAd_Basarisiz(string ad)
    {
        var (ok, _) = await Yonetici("C:\\yok.db", ad).CheckpointAndReleaseAsync(ad);

        ok.Should().BeFalse();
    }
}
