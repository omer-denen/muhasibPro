using MuhasibPro.Data.Database.Extensions;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices
{
    public interface ISistemDatabaseService
    {
        Task<(bool isValid, string Message)> ValidateSistemDatabaseAsync();
        Task<ApiDataResponse<DatabaseConnectionAnalysis>> GetSistemDatabaseStateAsync();
        Task<(bool initializeState, string message)> InitializeSistemDatabaseAsync();
        Task<List<string>> GetPendingMigrationsAsync();
        /// <summary>Bekleyen sistem göçlerini yedek-önce-göç ile uygular (Sage deseni: yedek → göç → doğrula). Bekleyen yoksa (false, bilgi).</summary>
        Task<(bool success, string message)> ApplyPendingSistemMigrationsAsync();
        Task<DatabaseHealtyDiagReport> GetSistemDatabaseFullDiagStateAsync(IProgress<AnalysisProgress> progressReporter = null, AnalysisOptions options = null);
    }
}
