using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Data.Database.TenantDatabase
{
    /// <summary>
    /// SADECE SQLite için connection string oluşturur
    /// </summary>
    public class TenantSQLiteConnectionStringFactory : ITenantSQLiteConnectionStringFactory
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger<TenantSQLiteConnectionStringFactory> _logger;

        // SQLite için optimal ayarlar (TenantSettings varsayılanlarıyla aynı)
        private const SqliteOpenMode DefaultOpenMode = SqliteOpenMode.ReadWriteCreate;
        private const SqliteCacheMode DefaultCacheMode = SqliteCacheMode.Shared;

        public TenantSQLiteConnectionStringFactory(
            IApplicationPaths applicationPaths,
            ILogger<TenantSQLiteConnectionStringFactory> logger)
        {
            _applicationPaths = applicationPaths ?? throw new ArgumentNullException(nameof(applicationPaths));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// SQLite için connection string oluşturur. busyTimeoutMs/pooling verilmezse
        /// TenantSettings varsayılanları kullanılır (Data Business bilmez — değerler
        /// Business'taki ITenantSettingsProvider'dan okunup parametreyle taşınır).
        /// </summary>
        public string CreateConnectionString(string databaseName, int? busyTimeoutMs = null, bool? pooling = null)
        {
            try
            {
                // ApplicationPaths zaten validasyon yapıyor
                var dbPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);

                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    Mode = DefaultOpenMode,
                    Cache = DefaultCacheMode,
                    Pooling = pooling ?? true,
                    ForeignKeys = true,
                    // Builder saniye ister; ayar ms tutar (en az 1 sn)
                    DefaultTimeout = TenantSettings.BusyTimeoutMsToSeconds(busyTimeoutMs)
                };
                var connectionString = builder.ToString();

                _logger.LogDebug(
                    "SQLite connection string oluşturuldu - Database: {DatabaseName}, Path: {DbPath}",
                    databaseName,
                    dbPath);

                return connectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection string oluşturulamadı: {DatabaseName}", databaseName);
                throw;
            }
        }

        /// <summary>
        /// SQLite bağlantısını test eder
        /// </summary>
        public async Task<(bool canConnect, string message, string connectionString)> ValidateConnectionStringAsync(
            string databaseName, int? busyTimeoutMs = null, bool? pooling = null)
        {
            // 2. File check
            var dbPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);
            if (!File.Exists(dbPath))
            {
                return (false, "Veritabanı dosyası bulunamadı", string.Empty);
            }
            try
            {
                var connectionString = CreateConnectionString(databaseName, busyTimeoutMs, pooling);

                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                // Basit bir test sorgusu
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                var result = await command.ExecuteScalarAsync();
                if (result?.ToString() != "1")
                {
                    return (false, "🔴 Bağlantı yanıt vermiyor!", string.Empty);
                }
                var isSuccess = Convert.ToInt32(result) == 1;

                _logger.LogInformation(
                    "SQLite bağlantı testi {Sonuç} {DatabaseName} için",
                    isSuccess ? "Başarılı" : "Başarısız",
                    databaseName);

                return (isSuccess, "🔗 Bağlantı başarılı", connectionString);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 14)
            {
                return (false, "⚠️ Veritabanı dosyası açılamadı!", string.Empty);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 26)
            {
                return (false, "⚠️ Bilinmeyen veritabanı dosyası!", string.Empty);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bağlantı testi başarısız: {DatabaseName}", databaseName);
                return (false, $"Bağlantı hatası: {ex.Message}", string.Empty);
            }
        }
    }
}