using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Data.Contracts.Database.TenantDatabase
{
    public interface ITenantSQLiteDatabaseManager
    {
        Task<DatabaseConnectionAnalysis> GetTenantDatabaseStateAsync(
             string databaseName);
        Task<DatabaseCreatingExecutionResult> CreateNewTenantDatabaseAsync(
            string databaseName, int? commandTimeoutSec = null);
        Task<DatabaseMigrationExecutionResult> InitializeTenantDatabaseAsync(
            string databaseName, int? commandTimeoutSec = null, int? retryCount = null);
        Task<DatabaseDeletingExecutionResult> DeleteTenantDatabase(
            string databaseName);
        Task<(bool isValid, string Message)> ValidateTenantDatabaseAsync(
            string databaseName);
        Task<TenantDerinAnaliz> GetDerinAnalizAsync(string databaseName);
        /// <summary>Tenant geçişi/kopuşu öncesi: WAL içeriğini ana dosyaya işler (checkpoint)
        /// ve havuz bağlantılarını bırakır. Dönen bilgi loglanır (doğrulama kanıtı).</summary>
        Task<(bool ok, string message)> CheckpointAndReleaseAsync(string databaseName);
        /// <param name="komut">"VACUUM" / "REINDEX" / "WAL"</param>
        /// <param name="timeoutSec">Bakım komut zaman aşımı sn 30-600 (null → 120).
        /// TenantSettings'ten gelir (Oturum 127).</param>
        Task<DatabaseMaintenanceResult> BakimCalistirAsync(
            string databaseName, string komut, int? timeoutSec = null);


    }
}
