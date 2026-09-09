using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasibPro.Data.Database.Common.Helpers;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel;
using System.Collections.Concurrent;

namespace MuhasibPro.Data.Database.Extensions
{
    /// <summary>
    /// Merkezi analiz extension — TÜM DbContext analizleri tek yerden geçer (single source of truth).
    /// DbContextOperationsExtensions ve DbContextDiagnosticsExtensions bu sınıfın ince sarmalayıcılarıdır.
    /// DatabaseValid burada GERÇEKTEN hesaplanır (dosya ipucu + PRAGMA integrity_check) — asla bayat taşınmaz.
    /// </summary>
    public static class DbContextAnalysisExtensions
    {
        private static readonly ConcurrentDictionary<string, List<string>> _allTablesCache = new();

        public static void ClearCache() => _allTablesCache.Clear();

        /// <summary>
        /// Dosya varlığı, bağlantı, bütünlük (PRAGMA), tablo ve migration analizini tek geçişte yapar.
        /// </summary>
        /// <param name="fileLevelValid">ApplicationPaths dosya seviyesi ön kontrolü (boyut/geçerli SQLite dosyası).</param>
        public static async Task<T> AnalyzeDatabaseCoreAsync<T>(
            this DbContext context,
            string databaseName,
            bool isDatabaseExists,
            bool fileLevelValid,
            string[] tablesToCheck,
            IProgress<AnalysisProgress> progress = null,
            AnalysisOptions options = null,
            ILogger logger = null,
            Func<T> factory = null)
            where T : DatabaseAnalysisResult, new()
        {
            options ??= AnalysisOptions.Default;
            var result = factory != null ? factory() : new T();
            result.DatabaseName = databaseName;
            result.OperationTime = DateTime.UtcNow;

            result.IsDatabaseExists = isDatabaseExists;
            result.DatabaseValid = false;
            result.IsEmptyDatabase = false;

            try
            {
                Report(progress, "Analiz başlatılıyor...", ProgressType.Info, 0);

                if (!isDatabaseExists)
                {
                    result.Message = "Veritabanı dosyası bulunamadı.";
                    Report(progress, result.Message, ProgressType.Warning, 100);
                    return result;
                }

                // 1. BAĞLANTI TESTİ
                Report(progress, "Bağlantı test ediliyor...", ProgressType.Info, 20);
                result.CanConnect = await context.Database.CanConnectAsync().ConfigureAwait(false);
                if (!result.CanConnect)
                {
                    result.HasError = true;
                    result.Message = "Veritabanına bağlanılamadı!";
                    Report(progress, result.Message, ProgressType.Error, 100);
                    return result;
                }
                Report(progress, "✓ Bağlantı başarılı", ProgressType.Success, 30);

                // 2. BÜTÜNLÜK — DatabaseValid gerçekten hesaplanır (PRAGMA ham SQLite komutu ile,
                //    EF SqlQueryRaw sarıcıları "Value" kolonu istediği için PRAGMA'lar ham connection ile çalıştırılır)
                var integrityOk = true;
                if (options.CheckIntegrity || !fileLevelValid)
                {
                    Report(progress, "Bütünlük kontrolü yapılıyor...", ProgressType.Info, 40);
                    integrityOk = await CheckIntegrityAsync(context, logger).ConfigureAwait(false);
                    if (!integrityOk)
                    {
                        result.DatabaseValid = false;
                        result.HasError = true;
                        result.Message = "Veritabanı bütünlük kontrolü başarısız.";
                        Report(progress, result.Message, ProgressType.Error, 100);
                        return result;
                    }
                    Report(progress, "✓ Bütünlük kontrolü başarılı", ProgressType.Success, 50);
                }

                var isValid = fileLevelValid && integrityOk;
                result.DatabaseValid = isValid;
                if (!isValid)
                {
                    result.HasError = true;
                    result.Message = "Veritabanı dosyası geçersiz (geçersiz boyut veya bozuk yapı).";
                    Report(progress, result.Message, ProgressType.Error, 100);
                    return result;
                }

                // 3. TABLO ANALİZİ (sıralı — DbContext thread-safe değildir, Task.WhenAll YASAK)
                if (tablesToCheck != null && tablesToCheck.Length > 0)
                {
                    var (existingCount, hasRows) = await AnalyzeTablesAsync(
                        context, tablesToCheck, progress, options, logger).ConfigureAwait(false);
                    result.TableCount = existingCount;
                    result.IsEmptyDatabase = existingCount == 0 || !hasRows;
                    Report(progress,
                        $"✓ Tablo analizi: {existingCount} tablo, {(hasRows ? "veri var" : "boş")}",
                        ProgressType.Success, 90);
                }

                // 4. MIGRATION ANALİZİ
                if (options.CheckMigrations)
                {
                    Report(progress, "Migration kontrolü yapılıyor...", ProgressType.Info, 92);
                    var applied = (await context.Database.GetAppliedMigrationsAsync().ConfigureAwait(false)).ToList();
                    var pending = (await context.Database.GetPendingMigrationsAsync().ConfigureAwait(false)).ToList();
                    result.PendingMigrations = pending;
                    result.AppliedMigrationsCount = applied.Count;
                    result.CurrentVersion =
                        DbSchemaVersions.ForMigration(applied.LastOrDefault());
                    Report(progress, $"✓ Migration: {applied.Count} uygulanmış, {pending.Count} bekleyen",
                        ProgressType.Success, 95);
                }

                Report(progress, "✓ Analiz tamamlandı!", ProgressType.Success, 100);
                logger?.LogDebug("Database analizi tamamlandı: {Database}", databaseName);
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Message = $"{databaseName} analiz edilemedi: {ex.Message}";
                Report(progress, $"✗ HATA: {ex.Message}", ProgressType.Error, 100);
                logger?.LogError(ex, "Database analizi başarısız: {Db}", databaseName);
            }

            return result;
        }

        /// <summary>
        /// PRAGMA integrity_check — ham SqliteConnection ile (EF SqlQueryRaw "Value" kolon adı bekler, PRAGMA uyumsuz).
        /// Çalıştırma hatası (kilit/busy) bozulma kanıtı sayılmaz → geçerli varsayılır (sahte rollback önlenir), uyarı loglanır.
        /// </summary>
        public static async Task<bool> CheckIntegrityAsync(DbContext context, ILogger logger = null)
        {
            string connectionString = null;
            try
            {
                connectionString = context.Database.GetConnectionString();
                await using var conn = new SqliteConnection(connectionString);
                await conn.OpenAsync().ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA integrity_check;";
                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                return string.Equals(res?.ToString(), "ok", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex,
                    "integrity_check çalıştırılamadı ({Database}) — bozulma kanıtı yok, geçerli varsayıldı",
                    connectionString);
                return true;
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  Tablo analizi — sıralı, cache'li, connection-string bazlı
        // ────────────────────────────────────────────────────────────────
        private static async Task<(int existingCount, bool hasRows)> AnalyzeTablesAsync(
            DbContext context,
            string[] tablesToCheck,
            IProgress<AnalysisProgress> progress,
            AnalysisOptions options,
            ILogger logger)
        {
            var validTables = tablesToCheck.Where(IsValidTableName).ToList();
            if (validTables.Count == 0)
                return (0, false);

            var allTables = await GetAllTablesCachedAsync(context).ConfigureAwait(false);
            int existingCount = 0;
            bool hasRows = false;
            var batchSize = Math.Max(1, options.BatchSize);
            int processed = 0;

            foreach (var batch in validTables.Batch(batchSize))
            {
                foreach (var tableName in batch)
                {
                    processed++;
                    if (!allTables.Contains(tableName, StringComparer.OrdinalIgnoreCase))
                        continue;

                    existingCount++;
                    if (!hasRows)
                    {
                        try
                        {
                            hasRows = await TableHasRowsSafeAsync(context, tableName).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogWarning(ex, "Tablo analizi hatası: {Table}", tableName);
                        }
                    }
                }

                var pct = 50 + (int)((processed / (float)validTables.Count) * 40);
                Report(progress, $"{processed}/{validTables.Count} tablo kontrol edildi...", ProgressType.Info, pct);

                if (options.DelayBetweenBatches > TimeSpan.Zero)
                    await Task.Delay(options.DelayBetweenBatches).ConfigureAwait(false);
            }

            return (existingCount, hasRows);
        }

        // ────────────────────────────────────────────────────────────────
        //  Yardımcılar
        // ────────────────────────────────────────────────────────────────
        private static bool IsValidTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName) || tableName.Length > 64) return false;
            if (tableName.Contains("--") || tableName.Contains(";")) return false;
            if (tableName[0] != '_' && !char.IsLetter(tableName[0])) return false;
            foreach (char c in tableName)
                if (!char.IsLetterOrDigit(c) && c != '_') return false;
            return true;
        }

        private static async Task<bool> TableExistsAsync(DbContext context, string tableName)
        {
            if (!IsValidTableName(tableName)) return false;
            try
            {
                // EF SqlQueryRaw sarıcısı "Value" kolon adını ister → AS Value zorunlu
                var sql = "SELECT 1 AS Value FROM sqlite_master WHERE type = 'table' AND name = @name LIMIT 1";
                return await context.Database
                    .SqlQueryRaw<int>(sql, new SqliteParameter("@name", tableName))
                    .AnyAsync()
                    .ConfigureAwait(false);
            }
            catch { return false; }
        }

        private static async Task<bool> TableHasRowsSafeAsync(DbContext context, string tableName)
        {
            if (!IsValidTableName(tableName)) return false;
            try
            {
                if (!await TableExistsAsync(context, tableName).ConfigureAwait(false)) return false;
                var sql = $"SELECT 1 AS Value FROM \"{tableName.Replace("\"", "\"\"")}\" LIMIT 1";
                return await context.Database
                    .SqlQueryRaw<int>(sql)
                    .AnyAsync()
                    .ConfigureAwait(false);
            }
            catch { return false; }
        }

        /// <summary>
        /// Tablo listesi cache'i DB DOSYASI bazlıdır (connection string) — DbContext tipi bazlı DEĞİL,
        /// aksi halde aynı context tipinin farklı tenant dosyalarında bayat liste dönerdi.
        /// </summary>
        private static async Task<List<string>> GetAllTablesCachedAsync(DbContext context)
        {
            string cacheKey = null;
            try { cacheKey = context.Database.GetConnectionString(); } catch { }

            if (cacheKey != null && _allTablesCache.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {
                var list = await context.Database.SqlQueryRaw<string>(
                        "SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'")
                    .ToListAsync().ConfigureAwait(false);

                if (cacheKey != null)
                    _allTablesCache[cacheKey] = list;

                return list;
            }
            catch { return new List<string>(); }
        }

        private static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize) { yield return batch; batch = new List<T>(batchSize); }
            }
            if (batch.Count > 0) yield return batch;
        }

        private static void Report(IProgress<AnalysisProgress> progress, string msg, ProgressType type, int pct)
        {
            progress?.Report(new AnalysisProgress { Message = msg, Type = type, Percentage = pct, Timestamp = DateTime.UtcNow });
        }
    }
}