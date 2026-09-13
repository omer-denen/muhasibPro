using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Data.Database.Common.Helpers;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Data.Database.SistemDatabase
{
    /// <summary>
    /// Faz 6.78 Adım 3: Sistem.db anlık-görüntü okuyucu. Mevcut dosya veya yedek dosya
    /// aynı yolla okunur; bağlantı <c>Mode=ReadOnly;Pooling=False</c> ile açılır (canlı DB'ye dokunmaz).
    /// </summary>
    public class SistemSnapshotReader : ISistemSnapshotReader
    {
        private readonly ILogger<SistemSnapshotReader> _logger;

        public SistemSnapshotReader(ILogger<SistemSnapshotReader> logger)
        {
            _logger = logger;
        }

        public async Task<SistemSnapshot> ReadAsync(string databaseFilePath)
        {
            var snapshot = new SistemSnapshot();
            if (string.IsNullOrWhiteSpace(databaseFilePath) || !File.Exists(databaseFilePath))
                return snapshot;

            try
            {
                await using var connection = new SqliteConnection($"Data Source={databaseFilePath};Mode=ReadOnly;Pooling=False");
                await connection.OpenAsync();

                snapshot.SonMigration = await ScalarAsync(connection, "SELECT MigrationId FROM \"__EFMigrationsHistory\" ORDER BY MigrationId DESC LIMIT 1;");
                snapshot.Surum = DbSchemaVersions.ForMigration(snapshot.SonMigration);

                await foreach (var row in MaliDonemSatirlariAsync(connection))
                    snapshot.MaliDonemler.Add(row);

                snapshot.FirmaIdler = await IdListesiAsync(connection, "SELECT Id FROM Firmalar;");
                snapshot.KullaniciIdler = await IdListesiAsync(connection, "SELECT Id FROM Kullanicilar;");

                var ayarlar = await AyarlarAsync(connection);
                snapshot.KurulumId = ayarlar.TryGetValue("KurulumId", out var kid) ? kid : null;
                snapshot.MachineGuid = ayarlar.TryGetValue("KurulumMachineGuid", out var mg) ? mg : null;

                snapshot.OkunabildiMi = true;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Sistem snapshot okunamadı: {Path}", databaseFilePath);
            }

            return snapshot;
        }

        private static async Task<string?> ScalarAsync(SqliteConnection connection, string sql)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            return (await cmd.ExecuteScalarAsync())?.ToString();
        }

        private static async Task<List<long>> IdListesiAsync(SqliteConnection connection, string sql)
        {
            var liste = new List<long>();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(0))
                    liste.Add(reader.GetInt64(0));
            }
            return liste;
        }

        private static async IAsyncEnumerable<SistemMaliDonemSatiri> MaliDonemSatirlariAsync(SqliteConnection connection)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, FirmaId, MaliYil, DatabaseName FROM MaliDonemler;";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return new SistemMaliDonemSatiri
                {
                    Id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0),
                    FirmaId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
                    MaliYil = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    DatabaseName = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
            }
        }

        private static async Task<Dictionary<string, string>> AyarlarAsync(SqliteConnection connection)
        {
            var ayarlar = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Anahtar, Deger FROM GlobalAyarlar;";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (reader.IsDBNull(0)) continue;
                var anahtar = reader.GetString(0);
                ayarlar[anahtar] = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            }
            return ayarlar;
        }
    }
}
