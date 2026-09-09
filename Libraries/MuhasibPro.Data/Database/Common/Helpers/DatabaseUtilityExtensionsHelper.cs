using MuhasibPro.Data.Contracts.Database.Common.Helpers;

namespace MuhasibPro.Data.Database.Common.Helpers
{
    public static class DatabaseUtilityExtensionsHelper
    {
        public static string GenerateTenantDatabaseName(this IApplicationPaths paths, string prefix, string firmaKodu, int maliYil)
        {
            // Tüm SQL adlarını büyük harf kullanmak ve özel karakterden kaçınmak iyi bir pratiktir.
            return paths.SanitizeDatabaseName($"{prefix}{firmaKodu.ToUpperInvariant()}_{maliYil}");
        }

        public static string GenerateBackupFileName(string databaseName, string suffix = null)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var guidPart = Guid.NewGuid().ToString("N")[..4];

            var fileName = $"{databaseName}_{timestamp}_{guidPart}";
            if (!string.IsNullOrEmpty(suffix))
                fileName += $"_{suffix}";

            return fileName + ".backup";
        }
    }
}
