using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.DataContext;

namespace MuhasibPro.Tests;

public class TenantIdentityMigrationTests
{
    private static string CreateOldTenantDb()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"muhasib_test_{Guid.NewGuid():N}.db");
        var cs = $"Data Source={tempFile}";
        using var conn = new SqliteConnection(cs);
        conn.Open();
        using var cmd = conn.CreateCommand();
        // Eski şema: sadece 4 kolon (InitialApp hali)
        cmd.CommandText = @"
            CREATE TABLE TenantDatabaseVersiyonlar (
                DatabaseName TEXT NOT NULL PRIMARY KEY,
                CurrentTenantDbVersion TEXT NOT NULL,
                CurrentTenantDbLastUpdate TEXT NOT NULL,
                PreviousTenantDbVersiyon TEXT
            );
            CREATE TABLE ""__EFMigrationsHistory"" (
                MigrationId TEXT NOT NULL PRIMARY KEY,
                ProductVersion TEXT NOT NULL
            );
            INSERT INTO ""__EFMigrationsHistory"" (MigrationId, ProductVersion) VALUES ('20260828170927_InitialApp', '9.0.11');
            INSERT INTO TenantDatabaseVersiyonlar (DatabaseName, CurrentTenantDbVersion, CurrentTenantDbLastUpdate, PreviousTenantDbVersiyon)
            VALUES ('db-TEST_2027', '1.0.0.0', '2024-01-01 00:00:00', NULL);
            CREATE TABLE AppLogs (
                Id INTEGER NOT NULL PRIMARY KEY,
                IsRead INTEGER NOT NULL,
                User TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Source TEXT NOT NULL,
                Action TEXT NOT NULL,
                Message TEXT NOT NULL,
                Description TEXT,
                KaydedenId INTEGER NOT NULL,
                KayitTarihi TEXT NOT NULL,
                GuncellemeTarihi TEXT,
                GuncelleyenId INTEGER,
                AktifMi INTEGER NOT NULL,
                ArananTerim TEXT
            );
        ";
        cmd.ExecuteNonQuery();
        conn.Close();
        return tempFile;
    }

    private static AppDbContext CreateAppDbContext(string filePath)
    {
        var cs = $"Data Source={filePath}";
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(cs).Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task EskiTenantDb_Migrate_KimlikKolonlariniEkler()
    {
        var file = CreateOldTenantDb();
        try
        {
            using var ctx = CreateAppDbContext(file);
            // Bekleyen migration olmalı (AddTenantIdentity)
            var pending = await ctx.Database.GetPendingMigrationsAsync();
            pending.Should().Contain("20260908_AddTenantIdentity");

            // Migrate et (güncelleme motorunun yaptığı)
            await ctx.Database.MigrateAsync();

            // Pending kalmamalı
            var pendingAfter = await ctx.Database.GetPendingMigrationsAsync();
            pendingAfter.Should().BeEmpty();

            // PRAGMA ile kolonlar var mı?
            using var conn = new SqliteConnection($"Data Source={file}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='TenantDatabaseVersiyonlar';";
            var sql = (await cmd.ExecuteScalarAsync())?.ToString();
            sql.Should().Contain("FirmaId");
            sql.Should().Contain("MaliDonemId");
            sql.Should().Contain("OlusturanKullaniciId");
            sql.Should().Contain("MakineId");
            sql.Should().Contain("KurulumId");

            // Eski veri korunmuş mu?
            using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = "SELECT DatabaseName, CurrentTenantDbVersion FROM TenantDatabaseVersiyonlar WHERE DatabaseName='db-TEST_2027';";
                using var reader = await cmd2.ExecuteReaderAsync();
                (await reader.ReadAsync()).Should().BeTrue();
                reader.GetString(0).Should().Be("db-TEST_2027");
                reader.GetString(1).Should().Be("1.0.0.0");
            }

            // Yeni kolonlar NULL geliyor mu? (kimliksiz eski veri)
            using (var cmd3 = conn.CreateCommand())
            {
                cmd3.CommandText = "SELECT FirmaId, MaliDonemId, OlusturanKullaniciId, MakineId, KurulumId FROM TenantDatabaseVersiyonlar WHERE DatabaseName='db-TEST_2027';";
                using var reader2 = await cmd3.ExecuteReaderAsync();
                await reader2.ReadAsync();
                reader2.IsDBNull(0).Should().BeTrue();
                reader2.IsDBNull(1).Should().BeTrue();
                reader2.IsDBNull(2).Should().BeTrue();
            }
        }
        finally
        {
            try { File.Delete(file); } catch { }
            try { File.Delete(file + "-wal"); } catch { }
            try { File.Delete(file + "-shm"); } catch { }
        }
    }

    [Fact]
    public async Task EskiTenantDb_Migrate_Sonrasi_KimlikYazilabilir()
    {
        var file = CreateOldTenantDb();
        try
        {
            using var ctx = CreateAppDbContext(file);
            await ctx.Database.MigrateAsync();

            // Yeni kolonlara damga yaz (B damgası simülasyonu)
            var ver = await ctx.TenantDatabaseVersiyonlar.FirstAsync(v => v.DatabaseName == "db-TEST_2027");
            ver.FirmaId.Should().BeNull(); // önce null
            ver.FirmaId = 1;
            ver.MaliDonemId = 10;
            ver.OlusturanKullaniciId = MuhasibPro.Domain.Entities.SistemEntity.KullaniciRolTip.Yönetici;
            ver.MakineId = "MACHINE-GUID-TEST";
            ver.KurulumId = "KURULUM-TEST";
            await ctx.SaveChangesAsync();

            // Tekrar oku
            using var ctx2 = CreateAppDbContext(file);
            var ver2 = await ctx2.TenantDatabaseVersiyonlar.AsNoTracking().FirstAsync(v => v.DatabaseName == "db-TEST_2027");
            ver2.FirmaId.Should().Be(1);
            ver2.MaliDonemId.Should().Be(10);
            ver2.OlusturanKullaniciId.Should().Be(MuhasibPro.Domain.Entities.SistemEntity.KullaniciRolTip.Yönetici);
            ver2.MakineId.Should().Be("MACHINE-GUID-TEST");
            ver2.KurulumId.Should().Be("KURULUM-TEST");
        }
        finally
        {
            try { File.Delete(file); } catch { }
            try { File.Delete(file + "-wal"); } catch { }
            try { File.Delete(file + "-shm"); } catch { }
        }
    }

    [Fact]
    public async Task GuncelTenantDb_ZatenMigrated_PendingBos()
    {
        var file = Path.Combine(Path.GetTempPath(), $"muhasib_test_{Guid.NewGuid():N}.db");
        try
        {
            var cs = $"Data Source={file}";
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(cs).Options;
            using var ctx = new AppDbContext(options);
            await ctx.Database.MigrateAsync();
            var pending = await ctx.Database.GetPendingMigrationsAsync();
            pending.Should().BeEmpty();
            // Tablo zaten yeni kolonlara sahip
            using var conn = new SqliteConnection(cs);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='TenantDatabaseVersiyonlar';";
            var sql = (await cmd.ExecuteScalarAsync())?.ToString();
            sql.Should().Contain("FirmaId");
        }
        finally
        {
            try { File.Delete(file); } catch { }
            try { File.Delete(file + "-wal"); } catch { }
            try { File.Delete(file + "-shm"); } catch { }
        }
    }
}
