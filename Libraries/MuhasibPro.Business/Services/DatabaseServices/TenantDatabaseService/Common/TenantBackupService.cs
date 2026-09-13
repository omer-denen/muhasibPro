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
                {
                    var kilitliler = backupFiles.Data
                        .Select(b => b?.BackupFileName)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Take(3);
                    return ApiDataExtensions.ErrorResponse(result,
                        $"Hiçbir yedek dosyası silinemedi ({totalCount} dosya kilitli olabilir: {string.Join(", ", kilitliler)})");
                }

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

        public async Task<bool> CleanupBackupFileAsync(string backupFilePath)
            => (await TryDeleteBackupFileAsync(backupFilePath)).ok;

        /// <summary>Tek yedek dosyası siler: salt-okunur bayrağı temizlenir, kilitlere karşı
        /// 5 deneme (100/200/400/800ms) yapılır; sonuç + OS nedeni döner (yutulmaz).</summary>
        public async Task<(bool ok, string neden)> TryDeleteBackupFileAsync(string backupFilePath)
        {
            if (string.IsNullOrWhiteSpace(backupFilePath))
                return (false, "Dosya yolu boş.");
            if (!File.Exists(backupFilePath))
                return (false, "Dosya diskte bulunamadı (başka bir işlem silmiş olabilir).");
            string sonHata = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    try { File.SetAttributes(backupFilePath, FileAttributes.Normal); } catch { }
                    File.Delete(backupFilePath);
                    return (true, string.Empty);
                }
                catch (IOException ex) when (i < 4)
                {
                    sonHata = ex.Message;
                    await Task.Delay(100 * (1 << i));
                }
                catch (UnauthorizedAccessException ex) when (i < 4)
                {
                    sonHata = ex.Message;
                    await Task.Delay(100 * (1 << i));
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
            return (false, string.IsNullOrWhiteSpace(sonHata) ? "Dosya silinemedi." : sonHata);
        }
    }
}
