using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Data.Database.Extensions
{
    public static class DbContextDatabaseCreateExtensions
    {
        public static async Task<DatabaseCreatingExecutionResult> ExecuteCreatingDatabaseAsync(
          this DbContext context,
          string databaseName,
          ILogger logger = null,
          int commandTimeoutMinutes = 5,
          int? commandTimeoutSec = null)
        {
            var result = new DatabaseCreatingExecutionResult
            {
                DatabaseName = databaseName,
                OperationTime = DateTime.UtcNow
            };

            try
            {
                // MIGRATION UYGULA (saniye verilmişse onu kullan — Oturum 127)
                var timeout = commandTimeoutSec.HasValue
                    ? TimeSpan.FromSeconds(Math.Clamp(commandTimeoutSec.Value, 5, 1800))
                    : TimeSpan.FromMinutes(commandTimeoutMinutes);
                context.Database.SetCommandTimeout(timeout);
                await context.Database.MigrateAsync().ConfigureAwait(false);
                DbContextAnalysisExtensions.ClearCache(); // Şema değişti → tablo cache'i geçersiz kıl

                var canConnect = await context.Database.CanConnectAsync().ConfigureAwait(false);
                if (canConnect)
                {
                    result.IsCreatedSuccess = true;
                    result.CanConnect = true;
                    result.Message = $"✅ Veritabanı başarıyla oluşturuldu";

                    logger?.LogInformation("Veritabanı oluşturma işlemi tamamlandı {DatabaseName})", databaseName);
                }
                else
                {
                    result.CanConnect = false;
                    result.HasError = true;
                    result.Message = "Veritabanı oluşturuldu ancak bağlantı doğrulanamadı.";
                    logger?.LogError("Veritabanı oluşturma sonrası bağlantı doğrulanamadı: {Database}", databaseName);
                }
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Message = $"❌ Veritabanı oluşturma işlemi başarısız: {ex.Message}";
                logger?.LogError(ex, "Veritabanı oluşturma işlemi başarısız: {Database}", databaseName);
            }

            return result;
        }
    }
}