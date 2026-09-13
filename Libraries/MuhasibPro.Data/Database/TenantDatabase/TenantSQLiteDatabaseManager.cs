using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Data.Database.Extensions;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Data.Database.TenantDatabase
{
    public class TenantSQLiteDatabaseManager : ITenantSQLiteDatabaseManager
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly ITenantSQLiteMigrationManager _migrationManager;
        private readonly ILogger<TenantSQLiteDatabaseManager> _logger;
        private static readonly SemaphoreSlim _globalDeletionLock = new SemaphoreSlim(1, 1);
        private const int LOCK_TIMEOUT_SECONDS = 30; // Max 30 saniye

        public TenantSQLiteDatabaseManager(IApplicationPaths applicationPaths, ITenantSQLiteMigrationManager migrationManager, ILogger<TenantSQLiteDatabaseManager> logger)
        {
            _applicationPaths = applicationPaths;
            _migrationManager = migrationManager;
            _logger = logger;
        }

        private (bool tenantFileExist, bool tenantDbValid) CheckTenantDatabaseState(string databaseName)
        {
            var tenantFileExist = _applicationPaths.TenantDatabaseFileExists(databaseName);
            var tenantDbValid = _applicationPaths.IsTenantDatabaseValid(databaseName);
            return (tenantFileExist, tenantDbValid);
        }

        public async Task<DatabaseConnectionAnalysis> GetTenantDatabaseStateAsync(
            string databaseName)
        {
            var analysis = new DatabaseConnectionAnalysis();
            try
            {
                var databaseHealty = await _migrationManager.GetTenantDatabaseStateAsync(
                    databaseName)
                    .ConfigureAwait(false);
                if (!databaseHealty.IsHealthy)
                {
                    databaseHealty.HasError = true;
                    databaseHealty.Message = "[Hata] ❌ Veritabanı durum analizi yapılamadı.";
                }
                analysis = databaseHealty;
                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veritabanı analiz hatası: {DatabaseName}", databaseName);
                analysis.HasError = true;
                analysis.Message = "[Hata] ❌ Veritabanı durum analizi yapılamadı.";
                return analysis;
            }
        }

        public async Task<DatabaseCreatingExecutionResult> CreateNewTenantDatabaseAsync(
            string databaseName, int? commandTimeoutSec = null)
        {
            bool isRollbackNeeded = false;
            var result = new DatabaseCreatingExecutionResult
            {
                DatabaseName = databaseName,
                IsCreatedSuccess = false,
                CanConnect = false,
                OperationTime = DateTime.UtcNow,
                HasError = false
            };

            try
            {
                var createResult = await _migrationManager.CreateNewTenantDatabase(databaseName, commandTimeoutSec);

                // Migration başarısızsa onun result'ını döndür
                if (!createResult.IsCreatedSuccess)
                {
                    return createResult;
                }

                // Race condition için bekle
                await Task.Delay(500);

                var tenantdbState = CheckTenantDatabaseState(databaseName);

                // ✅ DÜZELTİLDİ: Migration başarılı ama dosya yoksa HATA
                if (!tenantdbState.tenantFileExist)
                {
                    result.HasError = true;
                    result.Message = "🔴 Veritabanı dosyası oluşturulamadı.";
                    return result;
                }

                // Dosya var ama geçersiz mi?
                isRollbackNeeded = !tenantdbState.tenantDbValid;

                if (isRollbackNeeded)
                {
                    result.HasError = true;
                    var deletingResult = await DeleteTenantDatabase(databaseName);

                    if (deletingResult.IsDeletedSuccess)
                    {
                        result.Message = "❌ Oluşturulan veritabanı geçersiz. İşlem geri alındı";
                    }
                    else
                    {
                        result.Message = deletingResult.Message;
                    }
                    return result;
                }

                // ✅ HER ŞEY BAŞARILI - migration manager'ın result'ını döndür
                return createResult;
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Message = "Tenant database oluşturulurken beklenmeyen bir hata oluştu.";
                _logger.LogError(ex, "Veritabanı oluşturma hatası: {DatabaseName}", databaseName);
                return result;
            }
        }

        public async Task<DatabaseMigrationExecutionResult> InitializeTenantDatabaseAsync(
            string databaseName, int? commandTimeoutSec = null, int? retryCount = null)
        {
            var result = new DatabaseMigrationExecutionResult();
            var validateState = await ValidateTenantDatabaseAsync(databaseName);
            if (!validateState.isValid)
            {
                result.HasError = true;
                result.Message = validateState.Message;
                return result;
            }
            try
            {
                var initializeDatabase = await _migrationManager.InitializeTenantDatabaseAsync(
                    databaseName, commandTimeoutSec, retryCount)
                    .ConfigureAwait(false);
                if (initializeDatabase.HasError)
                {
                    result.HasError = initializeDatabase.HasError;
                    result.Message = initializeDatabase.Message;
                }

                return initializeDatabase;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Veritabanı başlatılamadı:, {databaseName}", databaseName);
                result.HasError = true;
                result.Message = $"[Hata] ❌ Veritabanı başlatılamadı:{ex.Message} ";
                return result;
            }
        }

        public async Task<DatabaseDeletingExecutionResult> DeleteTenantDatabase(
            string databaseName)
        {
            var deletingResult = new DatabaseDeletingExecutionResult
            {
                IsDeletedSuccess = false,
                HasError = true,
                OperationTime = DateTime.UtcNow,
            };

            var result = _applicationPaths.TenantDatabaseFileExists(databaseName);
            if (!result)
            {
                // Dosya zaten yok (elle silinmiş olabilir) — bunu hata sayma; aksi halde
                // Global.db satırı yetim kalır ve kullanıcı kaydı hiç silemez.
                // WAL/SHM artıkları best-effort temizlenir, kayıt silme akışı devam eder.
                _applicationPaths.CleanupSqliteWalFiles(databaseName);
                deletingResult.IsDeletedSuccess = true;
                deletingResult.HasError = false;
                deletingResult.Message = "Veritabanı dosyası zaten yoktu — kayıt temizliğine devam ediliyor.";
                return deletingResult;
            }
            try
            {
                if (!await _globalDeletionLock.WaitAsync(TimeSpan.FromSeconds(LOCK_TIMEOUT_SECONDS)))
                {
                    deletingResult.HasError = true;
                    deletingResult.Message = "🔴 Veritabanı silme işlemi zaman aşımına uğradı. Lütfen tekrar deneyin.";
                    return deletingResult;
                }
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        SqliteConnection.ClearAllPools();

                        await Task.Delay(100 * attempt); // Bağlantıların kapanması için bekleme süresi
                        var dbPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);
                        if (File.Exists(dbPath))
                        {
                            File.Delete(dbPath);
                            if (File.Exists(dbPath))
                            {
                                deletingResult.HasError = true;
                                deletingResult.Message = "�� Veritabanı silme işlemi başarısız oldu.";
                                return deletingResult;
                            }
                        }
                        _applicationPaths.CleanupSqliteWalFiles(databaseName);
                        deletingResult.IsDeletedSuccess = true;
                        deletingResult.Message = "✅ Veritabanı başarıyla silindi.";
                        return deletingResult;
                    }
                    catch (IOException ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Veritabanı silme hatası (Deneme {Attempt}): {DatabaseName}",
                            attempt,
                            databaseName);
                        if (attempt == 3)
                        {
                            deletingResult.HasError = true;
                            deletingResult.Message = $"[Hata] ❌ Veritabanı kullanımda. Silinemedi: {ex.Message}";
                            return deletingResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Veritabanı silme hatası: {DatabaseName}", databaseName);
                        deletingResult.HasError = true;
                        deletingResult.Message = $"[Hata] ❌ Veritabanı silinemedi: {ex.Message}";
                        return deletingResult;
                    }
                }
            }
            finally
            {
                _globalDeletionLock.Release();
            }
            return deletingResult;
        }

        public async Task<(bool isValid, string Message)> ValidateTenantDatabaseAsync(
            string databaseName)
        {
            try
            {
                var quickTest = CheckTenantDatabaseState(databaseName);
                if (!quickTest.tenantFileExist)
                {
                    return (isValid: false, "Veritabanı dosyası bulunamadı");
                }
                if (!quickTest.tenantDbValid)
                {
                    return (isValid: false, "Veritabanı dosya boyutu geçersiz.");
                }
                var result = await GetTenantDatabaseStateAsync(databaseName);
                return result?.ToLegacyResult() ?? (false, "Veritabanı durumu alınamadı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validate failed: {DatabaseName}", databaseName);
                return (false, $"Doğrulama başarısız: {ex.Message}");
            }
        }

        /// <summary>Tenant geçişi/kopuşu öncesi: WAL içeriğini ana dosyaya işler
        /// (<c>wal_checkpoint(TRUNCATE)</c>) ve havuz bağlantılarını bırakır.
        /// busy>0 ise birleştirme tamamlanamaz — ok=false + çerçeve sayıları döner (doğrulama kanıtı).</summary>
        public async Task<(bool ok, string message)> CheckpointAndReleaseAsync(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                return (false, "Veritabanı adı boş.");
            try
            {
                var dbPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);
                if (!File.Exists(dbPath))
                    return (false, $"Veritabanı dosyası bulunamadı: {databaseName}");
                int busy = 0, log = 0, checkpointed = 0;
                using (var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadWrite;Pooling=False;"))
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                    using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        busy = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        log = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        checkpointed = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    }
                }
                SqliteConnection.ClearAllPools();
                if (busy > 0)
                    return (false, $"WAL birleştirilemedi (meşgul {busy}, log {log}, işlenen {checkpointed}). Havuz bırakıldı.");
                return (true, $"WAL birleştirildi (log {log}, işlenen {checkpointed}). Havuz bırakıldı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkpoint başarısız: {DatabaseName}", databaseName);
                return (false, $"Checkpoint başarısız: {ex.Message}");
            }
        }

        /// <summary>Derin salt-okunur analiz: PRAGMA sayfa/bütünlük + tablo satır sayımları.</summary>
        public async Task<TenantDerinAnaliz> GetDerinAnalizAsync(string databaseName)
        {
            var sonuc = new TenantDerinAnaliz { DatabaseName = databaseName };
            try
            {
                var dbPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);
                if (!File.Exists(dbPath))
                {
                    sonuc.Hata = "Veritabanı dosyası bulunamadı";
                    return sonuc;
                }
                sonuc.DosyaVar = true;
                sonuc.DosyaBoyutu = new FileInfo(dbPath).Length;
                var walPath = dbPath + "-wal";
                sonuc.WalBoyutu = File.Exists(walPath) ? new FileInfo(walPath).Length : 0;

                SqliteConnection.ClearAllPools();
                using var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly;");
                await conn.OpenAsync().ConfigureAwait(false);

                async Task<T> PragmaTekilAsync<T>(string sql, T varsayilan)
                {
                    try
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = sql;
                        var deger = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                        if (deger == null || deger == DBNull.Value)
                            return varsayilan;
                        return (T)Convert.ChangeType(deger, typeof(T));
                    }
                    catch { return varsayilan; }
                }

                sonuc.JournalModu = await PragmaTekilAsync("PRAGMA journal_mode;", "?").ConfigureAwait(false);
                sonuc.SqliteSurumu = await PragmaTekilAsync("SELECT sqlite_version();", "?").ConfigureAwait(false);
                sonuc.SayfaBoyutu = await PragmaTekilAsync<long>("PRAGMA page_size;", 0).ConfigureAwait(false);
                sonuc.SayfaSayisi = await PragmaTekilAsync<long>("PRAGMA page_count;", 0).ConfigureAwait(false);
                sonuc.BosSayfaSayisi = await PragmaTekilAsync<long>("PRAGMA freelist_count;", 0).ConfigureAwait(false);

                var tablolar = new List<string>();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
                    using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        tablolar.Add(reader.GetString(0));
                }
                sonuc.TabloSayisi = tablolar.Count;
                foreach (var tablo in tablolar)
                {
                    try
                    {
                        long kayit = 0, boyut = 0;
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = $"SELECT COUNT(*) FROM \"{tablo.Replace("\"", "\"\"")}\";";
                            kayit = Convert.ToInt64(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                        }
                        try
                        {
                            using var bcmd = conn.CreateCommand();
                            bcmd.CommandText = "SELECT COALESCE(SUM(pgsize),0) FROM dbstat WHERE name=@t;";
                            bcmd.Parameters.AddWithValue("@t", tablo);
                            boyut = Convert.ToInt64(await bcmd.ExecuteScalarAsync().ConfigureAwait(false));
                        }
                        catch { /* dbstat derlenmemiş olabilir */ }
                        sonuc.ToplamKayit += kayit;
                        sonuc.Tablolar.Add(new TabloIstatistik { TabloAdi = tablo, KayitSayisi = kayit, BoyutBayt = boyut });
                    }
                    catch { /* tek tablo listeyi kırmaz */ }
                }

                try
                {
                    using var icmd = conn.CreateCommand();
                    icmd.CommandText = "PRAGMA integrity_check;";
                    using var ireader = await icmd.ExecuteReaderAsync().ConfigureAwait(false);
                    int hata = 0;
                    while (await ireader.ReadAsync().ConfigureAwait(false))
                    {
                        if (!string.Equals(ireader.GetString(0), "ok", StringComparison.OrdinalIgnoreCase))
                            hata++;
                    }
                    sonuc.IntegrityHataSayisi = hata;
                }
                catch { /* integrity okunamazsa 0 varsayılır */ }
                return sonuc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Derin analiz hatası: {DatabaseName}", databaseName);
                sonuc.Hata = ex.Message;
                return sonuc;
            }
        }

        /// <summary>Tek komutluk bakım: VACUUM / REINDEX / WAL checkpoint (TRUNCATE).</summary>
        public async Task<DatabaseMaintenanceResult> BakimCalistirAsync(
            string databaseName, string komut, int? timeoutSec = null)
        {
            var sonuc = new DatabaseMaintenanceResult();
            try
            {
                var dbPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);
                if (!File.Exists(dbPath))
                {
                    sonuc.Mesaj = "Veritabanı dosyası bulunamadı.";
                    return sonuc;
                }
                sonuc.OncekiBoyut = new FileInfo(dbPath).Length;
                SqliteConnection.ClearAllPools();
                using var conn = new SqliteConnection($"Data Source={dbPath};");
                await conn.OpenAsync().ConfigureAwait(false);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = komut.ToUpperInvariant() switch
                {
                    "VACUUM" => "VACUUM;",
                    "REINDEX" => "REINDEX;",
                    "WAL" => "PRAGMA wal_checkpoint(TRUNCATE);",
                    _ => throw new ArgumentException($"Bilinmeyen bakım komutu: {komut}")
                };
                cmd.CommandTimeout = TenantSettings.ClampBakimTimeoutSec(timeoutSec);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                SqliteConnection.ClearAllPools();
                sonuc.SonrakiBoyut = new FileInfo(dbPath).Length;
                sonuc.Basarili = true;
                sonuc.Mesaj = $"{komut} tamamlandı.";
                return sonuc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bakım hatası ({Komut}): {DatabaseName}", komut, databaseName);
                sonuc.Mesaj = $"Bakım hatası: {ex.Message}";
                return sonuc;
            }
        }
    }
}
