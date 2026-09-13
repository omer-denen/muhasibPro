using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Data.Contracts.Database.SistemDatabase
{
    public interface ISistemBackupManager
    {
        Task<DatabaseBackupResult> CreateBackupAsync(DatabaseBackupType backupType);

        /// <summary>
        /// Backup'tan geri yükler
        /// </summary>
        Task<DatabaseRestoreExecutionResult> RestoreBackupAsync(string backupFileName);

        /// <summary>
        /// Mevcut backup'ları listeler
        /// </summary>
        Task<List<DatabaseBackupResult>> GetBackupsAsync();
        DateTime? GetLastBackupDate();
        Task<int> CleanOldBackupsAsync(int keepLast);

        /// <summary>Faz 6.79: WAL içeriğini ana dosyaya aktarır (TRUNCATE, best-effort). Fırlatmaz.</summary>
        Task<bool> CheckpointWalAsync();
        
        Task<bool> RestoreFromLatestBackupAsync();
    }
}
