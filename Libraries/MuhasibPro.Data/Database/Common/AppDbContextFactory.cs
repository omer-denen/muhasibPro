using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Data.Database.Common
{

    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly ITenantSQLiteConnectionStringFactory _connectionStringFactory;
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger<AppDbContextFactory> _logger;

        public AppDbContextFactory(ITenantSQLiteConnectionStringFactory connectionStringFactory, IApplicationPaths applicationPaths, ILogger<AppDbContextFactory> logger)
        {
            _connectionStringFactory = connectionStringFactory;
            _applicationPaths = applicationPaths;
            _logger = logger;
        }

        private string GetTenantDatabaseFilePath(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name required", nameof(databaseName));
            return _applicationPaths.GetTenantDatabaseFilePath(databaseName);
        }
        private bool GetTenantDatabaseValid(string databaseName) => _applicationPaths.IsSqliteDatabaseFileValid(GetTenantDatabaseFilePath(databaseName));

        public AppDbContext CreateDbContext(
            string databaseName,
            int? commandTimeoutSec = null,
            int? busyTimeoutMs = null,
            bool? pooling = null)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Veritabanı adı boş olamaz", nameof(databaseName));

            var options = CreateDbContextOptions(databaseName, commandTimeoutSec, busyTimeoutMs, pooling);
            return new AppDbContext(options);
        }
        private DbContextOptions<AppDbContext> CreateDbContextOptions(
            string databaseName,
            int? commandTimeoutSec = null,
            int? busyTimeoutMs = null,
            bool? pooling = null)
        {
            var connectionString = _connectionStringFactory.CreateConnectionString(databaseName, busyTimeoutMs, pooling);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlite(
                connectionString,
                sqliteOptions =>
                {
                    sqliteOptions.CommandTimeout(TenantSettings.ClampCommandTimeoutSec(commandTimeoutSec));
                });

#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
#endif

            return optionsBuilder.Options;
        }

        public async Task<(bool canConnect, string message)> TestDbContextConnectionAsync(string databaseName)
        {
            // 2. File check
            var dbPath = GetTenantDatabaseFilePath(databaseName);
            if (!File.Exists(dbPath))
            {
                return (false, "Veritabanı dosyası bulunamadı");
            }
            // 2. Dosya boyutu & Sqlite header durumunu kontrol et
            var dbValid = GetTenantDatabaseValid(databaseName);
            if (!dbValid)
            {
                _logger?.LogWarning("Veritabanı dosyası Sqlite için doğrulanamadı: {DatabaseName}", databaseName);
                return (false, $"Veritabanı dosyası Sqlite için doğrulanamadı: {databaseName}");
            }
            try
            {
                var connectionStringResult = await _connectionStringFactory.ValidateConnectionStringAsync(databaseName);

                if (!connectionStringResult.canConnect)
                {
                    return (false, $"Bağlantı hatası: {connectionStringResult.message}");
                }
                using var context = CreateDbContext(databaseName);
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger?.LogWarning("Veritabanı ile bağlantı kurulamadı: {DatabaseName}", databaseName);
                    return (false, "⛓️‍💥 Veritabanı bağlantısı kurululamadı");
                }
                return (true, "🔗 Veritabanı bağlantısı başarılı");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 14)
            {
                return (false, "🔴 Veritabanı dosyası açılamadı!");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 26)
            {
                return (false, "⚠️ Bilinmeyen veritabanı dosyası!");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veritabanı bağlantısı başarısız: {DatabaseName}", databaseName);
                return (false, $"Veritabanı Bağlantı Hatası: {ex.Message}");
            }
        }
    }
}
