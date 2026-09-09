using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Data.Database.Common.Helpers;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService
{
    public class TenantSQLiteDatabaseLifecycleService : ITenantSQLiteDatabaseLifecycleService
    {
        private readonly ITenantSQLiteDatabaseManager _databaseManager;
        private readonly ILogService _logService;

        public TenantSQLiteDatabaseLifecycleService(
            ITenantSQLiteDatabaseManager databaseManager,
            ILogService logService)
        {
            _databaseManager = databaseManager;
            _logService = logService;
        }

        public async Task<ApiDataResponse<DatabaseCreatingExecutionResult>> CreateNewTenantDatabaseAsync(
            string databaseName)
        {
            var checkTenantDb = await ValidateTenantDatabaseAsync(databaseName);
            if(checkTenantDb.isValid)
            {
                var result = new DatabaseCreatingExecutionResult { DatabaseName = databaseName, };
                return new SuccessApiDataResponse<DatabaseCreatingExecutionResult>(
                    data: result,
                    message: "✅ Veritabanı zaten mevcut ve sağlıklı");
            }
            try
            {
                var createTenant = await _databaseManager.CreateNewTenantDatabaseAsync(databaseName);
                if(!createTenant.IsCreatedSuccess)
                {
                    await _logService.SistemLogService
                        .SistemLogInformationAsync(
                            "Mali Dönem Veritabanı İşlemleri",
                            "Veritabanı Oluşturma İşlemi",
                            "Veritabanı oluşturma işleminde hata",
                            $"Mali Döneme ait {databaseName} veritabanı oluşturulamadı");
                    return new ErrorApiDataResponse<DatabaseCreatingExecutionResult>(
                        data: createTenant,
                        message: createTenant.Message);
                }
                await _logService.SistemLogService
                    .SistemLogInformationAsync(
                        "Mali Dönem Veritabanı İşlemleri",
                        "Veritabanı Oluşturma İşlemi",
                        "Veritabanı başarıyla oluşturuldu",
                        $"Mali Döneme ait {databaseName} veritabanı başarıyla oluşturuldu");
                return new SuccessApiDataResponse<DatabaseCreatingExecutionResult>(
                    data: createTenant,
                    message: createTenant.Message);
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogExceptionAsync("Mali Dönem Veritabanı işlemleri", "Yeni Mali Dönem Veritabanı Oluştur", ex);
                return new ErrorApiDataResponse<DatabaseCreatingExecutionResult>(
                    null,
                    message: $"[HATA] Mali Dönem'e ait veritabanı oluşturalamadı : {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<DatabaseDeletingExecutionResult>> DeleteTenantDatabase(
            string databaseName)
        {
            var tenantFileExist = await ValidateTenantDatabaseAsync(databaseName);
            if(!tenantFileExist.isValid)
            {
                // Dosya zaten yok (elle silinmiş olabilir) — bunu hata sayma; aksi halde
                // Global.db satırı yetim kalır ve kullanıcı kaydı hiç silemez.
                await _logService.SistemLogService
                    .SistemLogInformationAsync(
                        "Mali Dönem Veritabanı İşlemleri",
                        "Veritabanı Silme İşlemi",
                        "Veritabanı dosyası zaten yoktu",
                        $"Mali Döneme ait {databaseName} dosyası diskte bulunamadı — kayıt temizliğine devam ediliyor");
                return new SuccessApiDataResponse<DatabaseDeletingExecutionResult>(
                    data: new DatabaseDeletingExecutionResult
                    {
                        IsDeletedSuccess = true,
                        HasError = false,
                        OperationTime = DateTime.UtcNow,
                        Message = "Veritabanı dosyası zaten yoktu — kayıt temizliğine devam ediliyor."
                    },
                    message: "Veritabanı dosyası zaten yoktu — kayıt temizliğine devam ediliyor.");
            }
            try
            {
                var deleteTenant = await _databaseManager.DeleteTenantDatabase(databaseName);
                if(!deleteTenant.IsDeletedSuccess)
                {
                    await _logService.SistemLogService
                        .SistemLogErrorAsync(
                            "Mali Dönem Veritabanı İşlemleri",
                            "Veritabanı Silme İşlemi",
                            "Veritabanı silme işleminde hata",
                            $"Mali Döneme ait {databaseName} veritabanı silinemedi");
                    return new ErrorApiDataResponse<DatabaseDeletingExecutionResult>(
                        data: deleteTenant,
                        message: deleteTenant.Message);
                }
                await _logService.SistemLogService
                    .SistemLogInformationAsync(
                        "Mali Dönem Veritabanı İşlemleri",
                        "Veritabanı Silme İşlemi",
                        "Veritabanı başarıyla silindi",
                        $"Mali Döneme ait {databaseName} veritabanı silindi");
                return new SuccessApiDataResponse<DatabaseDeletingExecutionResult>(
                    data: deleteTenant,
                    deleteTenant.Message);
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogExceptionAsync("Mali Dönem Veritabanı işlemleri", "Mali Dönem Veritabanı Silme", ex);
                return new ErrorApiDataResponse<DatabaseDeletingExecutionResult>(
                    null,
                    message: $"[HATA] Mali Dönem'e ait veritabanı silenemedi : {ex.Message}");
                
            }
        }

        public async Task<ApiDataResponse<DatabaseConnectionAnalysis>> GetTenantDatabaseStateAsync(
            string databaseName)
        {
            var analysis = new DatabaseConnectionAnalysis();
            if (string.IsNullOrEmpty(databaseName))
                return new ErrorApiDataResponse<DatabaseConnectionAnalysis>(data: analysis, message: "⚠️ Veritabanı adı boş olamaz");
            try
            {
                var analysisResult = await _databaseManager.GetTenantDatabaseStateAsync(databaseName);
                if(analysisResult == null)
                {
                    return new ErrorApiDataResponse<DatabaseConnectionAnalysis>(data: analysis, message: $"{databaseName}, Veritabanı analiz edilemedi");
                }
                if (analysisResult.HasError)
                {
                    await _logService.SistemLogService.SistemLogErrorAsync(
                        "Mali Dönem Veritabanı İşlemleri", 
                        "Veritabanı Analizi", 
                        "Analiz işleminde hata oluştu", 
                        analysis.Message);
                }
                return new SuccessApiDataResponse<DatabaseConnectionAnalysis>(data: analysisResult, message: analysisResult.Message);
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService.SistemLogExceptionAsync("Mali Dönem Veritabanı işlemleri", "Veritabanı Analizi", ex);
                return new ErrorApiDataResponse<DatabaseConnectionAnalysis>(data: analysis, message: $"[HATA] {ex.Message}");
            }
        }

     

        public async Task<(bool isValid, string Message)> ValidateTenantDatabaseAsync(
            string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                return (false, "⚠️ Veritabanı adı boş olamaz!");            
            return await _databaseManager.ValidateTenantDatabaseAsync(databaseName);
        }
      
    }
}
