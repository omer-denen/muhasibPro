using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel;

namespace MuhasibPro.Data.Database.Extensions
{
    public static class DbContextOperationsExtensions
    {
        #region Connection Analysis (Read-Only)

        /// <summary>
        /// Database bağlantısını, migration durumunu ve yapısını analiz eder (READ ONLY).
        /// Merkezi DbContextAnalysisExtensions.AnalyzeDatabaseCoreAsync sarmalayıcısıdır.
        /// </summary>
        public static Task<DatabaseConnectionAnalysis> GetConnectionFullStateAsync(
            this DbContext context,
            string databaseName,
            bool isDatabaseExists,
            bool databaseValid,
            string[] tablesToCheck,
            ILogger logger = null)
        {
            return context.AnalyzeDatabaseCoreAsync<DatabaseConnectionAnalysis>(
                databaseName,
                isDatabaseExists,
                databaseValid,
                tablesToCheck,
                progress: null,
                options: AnalysisOptions.Default,
                logger: logger);
        }
        #endregion

        #region Migration Execution (Write Operation)

        /// <summary>
        /// Orijinal metod - geriye uyumluluk için
        /// </summary>
        public static async Task<DatabaseMigrationExecutionResult> ExecuteTenantMigrationsWithBackupCheckAsync(
            this DbContext context,
            string databaseName,
            bool isDatabaseExists,
            bool databaseValid,
            string[] tablesToCheck,
            Func<Task<bool>> restoreAction = null,
            Func<Task<bool>> backupAction = null,
            ILogger logger = null,
            int commandTimeoutMinutes = 5,
            int? commandTimeoutSec = null,
            int maxAttempts = 3
            )
        {
            // İç analiz yap ve ana metoda yönlendir
            var analysis = await context.GetConnectionFullStateAsync(
                databaseName, isDatabaseExists, databaseValid, tablesToCheck, logger)
                .ConfigureAwait(false);

            return await context.ExecuteMigrationsWithBackupCheckAsync(
                analysis, restoreAction, backupAction, logger, commandTimeoutMinutes, commandTimeoutSec, maxAttempts)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Analiz parametreli ana metod (TEMEL METOD).
        /// Akış: invalid→restore→re-analyze → backup → pending kontrol → migrate → son analiz → doğrulama.
        /// Tüm "geri yükleme" kararları Sonucun GERÇEKTEN yeniden hesaplanan DatabaseValid alanına dayanır.
        /// commandTimeoutSec verilirse dakika parametresini ezer (Oturum 127: TenantSettings saniyesi).
        /// maxAttempts yedek/geri-yükleme deneme sayısıdır (varsayılan 3; ayar MigrationRetry 1-5).
        /// </summary>
        public static async Task<DatabaseMigrationExecutionResult> ExecuteMigrationsWithBackupCheckAsync(
            this DbContext context,
            DatabaseConnectionAnalysis analysis,
            Func<Task<bool>> restoreAction = null,
            Func<Task<bool>> backupAction = null,
            ILogger logger = null,
            int commandTimeoutMinutes = 5,
            int? commandTimeoutSec = null,
            int maxAttempts = 3)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(analysis);

            var result = new DatabaseMigrationExecutionResult
            {
                DatabaseName = analysis.DatabaseName,
                OperationTime = DateTime.UtcNow,
                DatabaseValid = analysis.DatabaseValid,
                IsRolledBack = false,
                CanConnect = analysis.CanConnect,
                PendingMigrations = analysis.PendingMigrations,
                AppliedMigrationsCount = analysis.AppliedMigrationsCount
            };

            try
            {
                // TEMEL KONTROLLER
                if (!analysis.IsDatabaseExists || !analysis.CanConnect)
                {
                    result.HasError = true;
                    result.Message = analysis.ToUIFullMessage();
                    logger?.LogWarning(result.Message);
                    return result;
                }

                var currentAnalysis = analysis;

                // 1. GEÇERSİZ VERİTABANI → yedekten geri yükle ve YENİDEN ANALİZ ET (bayat alan taşınmaz)
                if (!currentAnalysis.DatabaseValid)
                {
                    logger?.LogWarning("Veritabanı geçersiz, yedekten geri yükleniyor: {Database}", analysis.DatabaseName);

                    if (restoreAction == null)
                    {
                        result.HasError = true;
                        result.Message = "Veritabanı geçersiz ve yedekten geri yükleme tanımlı değil, işlem durduruldu.";
                        return result;
                    }

                    var restored = await TryWithRetryAsync("Restore", restoreAction, logger, maxAttempts).ConfigureAwait(false);
                    result.IsRolledBack = restored;
                    if (!restored)
                    {
                        result.HasError = true;
                        result.Message = "Veritabanı meşgul veya kilitli, geri yüklenemediği için işlem durduruldu.";
                        return result;
                    }

                    currentAnalysis = await context.GetConnectionFullStateAsync(
                        analysis.DatabaseName,
                        isDatabaseExists: true,
                        databaseValid: true,
                        tablesToCheck: Array.Empty<string>(),
                        logger).ConfigureAwait(false);

                    result.CanConnect = currentAnalysis.CanConnect;

                    if (!currentAnalysis.CanConnect || !currentAnalysis.DatabaseValid)
                    {
                        result.DatabaseValid = currentAnalysis.DatabaseValid;
                        result.HasError = true;
                        result.Message = "Yedekten geri yükleme sonrasında bile sağlıklı veritabanı elde edilemedi.";
                        return result;
                    }
                }

                // 2. BACKUP (migration'dan ÖNCE!)
                if (currentAnalysis.ShouldTakeBackupBeforeMigration && backupAction != null)
                {
                    logger?.LogInformation("Backup başlatılıyor: {Database}", analysis.DatabaseName);

                    var backupSuccess = await TryWithRetryAsync("Backup", backupAction, logger, maxAttempts).ConfigureAwait(false);
                    if (!backupSuccess)
                    {
                        result.DatabaseValid = currentAnalysis.DatabaseValid;
                        result.HasError = true;
                        result.Message = "Yedek alınamadığı için işlem durduruldu.";
                        return result;
                    }

                    result.BackupTaken = true;
                    logger?.LogInformation("Backup başarılı: {Database}", analysis.DatabaseName);
                }

                // 3. MIGRATION GEREKMİYORSA: erken dön (backup alınmış olabilir)
                if (!currentAnalysis.IsUpdateRequired)
                {
                    result.DatabaseValid = currentAnalysis.DatabaseValid;
                    result.AppliedMigrationsCount = currentAnalysis.AppliedMigrationsCount;
                    result.PendingMigrations = currentAnalysis.PendingMigrations;
                    result.Message = result.ToUIFullMessage();
                    return result;
                }

                // 4. MIGRATION UYGULA + tablo cache'ini geçersiz kıl
                context.Database.SetCommandTimeout(ResolveCommandTimeout(commandTimeoutMinutes, commandTimeoutSec));
                await context.Database.MigrateAsync().ConfigureAwait(false);
                DbContextAnalysisExtensions.ClearCache();

                // 5. SON DURUM — state GERÇEKTEN yeniden hesaplanır (bütünlük + tablo + migration)
                var finalAnalysis = await context.GetConnectionFullStateAsync(
                    analysis.DatabaseName,
                    isDatabaseExists: true,
                    databaseValid: true,
                    tablesToCheck: Array.Empty<string>(),
                    logger).ConfigureAwait(false);

                result.DatabaseValid = finalAnalysis.DatabaseValid;
                result.CanConnect = finalAnalysis.CanConnect;
                result.AppliedMigrationsCount = finalAnalysis.AppliedMigrationsCount;
                result.PendingMigrations = finalAnalysis.PendingMigrations;

                // 6. MIGRATION SONRASI DOĞRULAMA — sadece gerçekten geçersizse geri yükle
                if (!finalAnalysis.DatabaseValid)
                {
                    logger?.LogError("Migration sonrası veritabanı doğrulanamadı: {Database}", analysis.DatabaseName);

                    if (restoreAction != null)
                    {
                        result.IsRolledBack = await TryWithRetryAsync("Restore", restoreAction, logger, maxAttempts).ConfigureAwait(false);
                        if (!result.IsRolledBack)
                        {
                            result.HasError = true;
                            result.Message = "Migration sonrası veritabanı bozuldu ve yedekten de geri yüklenemedi.";
                            return result;
                        }
                    }
                    else
                    {
                        result.HasError = true;
                        result.Message = "Migration sonrası veritabanı bozuldu ve geri yükleme tanımlı değil.";
                        return result;
                    }
                }

                result.Message = result.ToUIFullMessage();

                logger?.LogInformation(
                    "Migration tamamlandı: {Database} ({Count} migration, Backup: {Backup}, Rollback: {Rollback})",
                    analysis.DatabaseName,
                    result.AppliedMigrationsCount,
                    result.BackupTaken ? "Evet" : "Hayır",
                    result.IsRolledBack ? "Evet" : "Hayır");
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Message = $"Migration başarısız: {ex.Message}";
                logger?.LogError(ex, "Migration başarısız: {Database}", analysis.DatabaseName);

                if (restoreAction != null)
                {
                    result.IsRolledBack = await TryWithRetryAsync("Restore", restoreAction, logger, maxAttempts).ConfigureAwait(false);
                    if (result.IsRolledBack)
                    {
                        result.Message += " Hata nedeniyle yedekten geri yüklendi.";
                    }
                    else
                    {
                        logger?.LogError("Hata durumunda restore de başarısız oldu.");
                    }
                }
            }

            return result;
        }
        #endregion

        #region Private Helpers
        /// <summary>Saniye verilmişse onu (5-1800 clamp), yoksa dakika parametresini kullanır.</summary>
        private static TimeSpan ResolveCommandTimeout(int commandTimeoutMinutes, int? commandTimeoutSec)
        {
            if (commandTimeoutSec.HasValue)
                return TimeSpan.FromSeconds(Math.Clamp(commandTimeoutSec.Value, 5, 1800));
            return TimeSpan.FromMinutes(commandTimeoutMinutes);
        }

        private static async Task<bool> TryWithRetryAsync(
            string operationName,
            Func<Task<bool>> action,
            ILogger logger,
            int maxAttempts = 3)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (await action().ConfigureAwait(false))
                        return true;

                    logger?.LogWarning("{Operation} başarısız (deneme {Attempt}/{Max})",
                        operationName, attempt, maxAttempts);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "{Operation} hata fırlattı (deneme {Attempt}/{Max})",
                        operationName, attempt, maxAttempts);
                }

                if (attempt < maxAttempts)
                    await Task.Delay(1000).ConfigureAwait(false);
            }

            return false;
        }
        #endregion
    }
}