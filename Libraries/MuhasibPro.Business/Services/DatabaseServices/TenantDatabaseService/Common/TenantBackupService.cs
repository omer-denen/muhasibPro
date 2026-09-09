using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService.Common
{
    public class TenantBackupService : ITenantBackupService
    {
        
        private readonly IApplicationPaths _applicationPaths;
        private readonly ITenantSQLiteDatabaseOperationService _operationService;

        public TenantBackupService(
            IApplicationPaths applicationPaths,
            ITenantSQLiteDatabaseOperationService operationService)
        {
            _applicationPaths = applicationPaths;
            _operationService = operationService;
        }
     
        public async Task<ApiDataResponse<TenantDeletingResult>> CleanAllBackupsAsync(string databaseName)
        {
            var result = new TenantDeletingResult
            {
                DatabaseName = databaseName,
                BackupDeleteCompleted = false,                
                DeletedBackupFiles = new List<string>() // ⭐ Yeni property
            };

            try
            {
                // ⭐ 1. Backup dizini kontrolü
                var backupDir = _applicationPaths.GetTenantBackupFolderPath();
                if (!Directory.Exists(backupDir))
                    return ApiDataExtensions.SuccessResponse(result, "Yedek dizini yok");

                // ⭐ 2. Backup listesini al
                var backupFiles = await _operationService.GetBackupHistoryAsync(databaseName);

                if (!backupFiles.Success || !backupFiles.Data.Any())
                    return ApiDataExtensions.SuccessResponse(result, "Silinecek backup dosyası yok");

                // ⭐ 3. Hepsini sil
                var deletedCount = 0;
                var totalCount = backupFiles.Data.Count;

                foreach (var backup in backupFiles.Data)
                {
                    try
                    {
                        var success = await CleanupBackupFileAsync(backup.BackupFilePath);
                        if (success)
                        {
                            deletedCount++;
                            result.DeletedBackupFiles.Add(backup.BackupFilePath);
                        }
                    }
                    catch
                    {
                        // Bir dosya silinemezse diğerlerine devam et
                        continue;
                    }
                }

                // ⭐ 4. Sonuç
                
                result.BackupDeleteCompleted = deletedCount > 0; // ⭐ En az bir tane silindi mi?
                result.DeletedBackupCount = deletedCount;
                if (deletedCount == 0)
                    return ApiDataExtensions.ErrorResponse(result, "Hiçbir yedek dosyası silinemedi");

                if (deletedCount < totalCount)
                    return ApiDataExtensions.SuccessResponse(result,
                        $"{deletedCount}/{totalCount} yedek dosyası silindi (bazıları silinemedi)");

                return ApiDataExtensions.SuccessResponse(result,
                    $"Tüm yedek dosyaları silindi ({deletedCount} adet)");
            }
            catch (Exception ex)
            {
                return ApiDataExtensions.ErrorResponse(result,
                    $"Backup temizleme hatası: {ex.Message}");
            }
        }

        public async Task<bool>  CleanupBackupFileAsync(string backupFilePath)
        {
            if(string.IsNullOrEmpty(backupFilePath) || !File.Exists(backupFilePath))
            {
                return false;
            }

            try
            {
                // 3 defa deneyelim (file lock olabilir)
                for(int i = 0; i < 3; i++)
                {
                    try
                    {
                        File.Delete(backupFilePath);
                        return true;
                    } catch(IOException) when (i < 2) // Son deneme değilse
                    {
                        await Task.Delay(100 * (i + 1)); // Artan gecikme
                    }
                }
                return false;
            } catch(Exception)
            {
                return false;
            }
        }
    }
}
