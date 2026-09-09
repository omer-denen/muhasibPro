using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices
{
    public interface ITenantSQLiteDatabaseOperationService
    {
        Task<ApiDataResponse<DatabaseBackupResult>> CreateBackupAsync(string databaseName, DatabaseBackupType backupType);
        Task<ApiDataResponse<DatabaseRestoreExecutionResult>> RestoreBackupAsync(string databaseName, string backupFilePath);
        Task<ApiDataResponse<List<DatabaseBackupResult>>> GetBackupHistoryAsync(string databaseName);
        /// <summary>Yedek klasöründeki TÜM tenant yedekleri (öksüz/yetim taraması için).</summary>
        Task<ApiDataResponse<List<DatabaseBackupResult>>> GetAllBackupsAsync();
        Task<ApiDataResponse<bool>> RestoreFromLatestBackupAsync(string databaseName);
        DateTime? GetLastBackupDate(string databaseName);
        Task<ApiDataResponse<int>> CleanOldBackupsAsync(string databaseName, int keepLast);
        Task<DatabaseDeletingExecutionResult> DeleteBackupDatabaseAsync(
        string databaseName);
        /// <summary>Derin salt-okunur analiz (PRAGMA + tablo sayımları).</summary>
        Task<ApiDataResponse<TenantDerinAnaliz>> GetDerinAnalizAsync(string databaseName);
        /// <param name="komut">"VACUUM" / "REINDEX" / "WAL"</param>
        Task<ApiDataResponse<DatabaseMaintenanceResult>> BakimCalistirAsync(string databaseName, string komut);
    }
}
