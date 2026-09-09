using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Data.Contracts.Database.TenantDatabase
{
    public interface ITenantSQLiteMigrationManager
    {
        /// <param name="commandTimeoutSec">Göç komut zaman aşımı sn (null → 5 dk).
        /// TenantSettings'ten gelir (Oturum 127).</param>
        /// <param name="retryCount">Yedek/geri-yükleme deneme sayısı 1-5 (null → 3).</param>
        Task<DatabaseMigrationExecutionResult> InitializeTenantDatabaseAsync(
            string databaseName, int? commandTimeoutSec = null, int? retryCount = null);
        Task<DatabaseCreatingExecutionResult> CreateNewTenantDatabase(
            string databaseName, int? commandTimeoutSec = null);
        Task<DatabaseConnectionAnalysis> GetTenantDatabaseStateAsync(string databaseName);
        Task<List<string>> GetTenantPendingMigrationsAsync(string databaseName);
        Task<string> GetTenantCurrentDatabaseVersionAsync(string databaseName);

    }
}
