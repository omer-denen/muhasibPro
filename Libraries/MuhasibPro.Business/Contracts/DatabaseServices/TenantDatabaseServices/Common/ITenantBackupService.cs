using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common
{
    public interface ITenantBackupService
    {
        Task<ApiDataResponse<TenantDeletingResult>> CleanAllBackupsAsync(string databaseName);
        Task<bool> CleanupBackupFileAsync(string backupFilePath);
        /// <summary>Tek dosya silme + başarısızlık nedeni (VM Danger mesajı için).</summary>
        Task<(bool ok, string neden)> TryDeleteBackupFileAsync(string backupFilePath);
    }
}
