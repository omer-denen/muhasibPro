using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel;

namespace MuhasibPro.Data.Database.Extensions
{
    /// <summary>
    /// Tanılama extension — DbContextAnalysisExtensions.AnalyzeDatabaseCoreAsync ince sarmalayıcısıdır.
    /// Tüm analiz/tablo/bütünlük mantığı merkez sınıfta yaşar; burada sadece tip seçilir.
    /// </summary>
    public static class DbContextDiagnosticsExtensions
    {
        public static Task<DatabaseHealtyDiagReport> GetDatabaseFullDiagStateAsync(
            this DbContext context,
            string databaseName,
            bool isDatabaseExists,
            bool databaseValid,
            string[] tablesToCheck,
            IProgress<AnalysisProgress> progressReporter = null,
            AnalysisOptions options = null,
            ILogger logger = null)
        {
            return context.AnalyzeDatabaseCoreAsync(
                databaseName,
                isDatabaseExists,
                databaseValid,
                tablesToCheck,
                progressReporter,
                options,
                logger,
                () => new DatabaseHealtyDiagReport { AnalysisProgress = progressReporter });
        }
    }
}