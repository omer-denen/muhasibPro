using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum.DatabaseEnum;

namespace MuhasibPro.Domain.Models.DatabaseResultModel
{
    public class DatabaseBackupResult
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string BackupFilePath { get; set; } = string.Empty;
        public string BackupDirectory { get; set; } = string.Empty;
        public string BackupFileName { get; set; } = string.Empty;
        public string BackupPath { get; set; } = string.Empty;
        public long BackupFileSizeBytes { get; set; }
        public DatabaseBackupType BackupType { get; set; } // ⭐ ENUM!
        public bool IsBackupComleted { get; set; }
        public DateTime LastBackupDate { get; set; }
        public string Message { get; set; } = string.Empty;

        // Kimlik (salt-okunur SELECT — yedek DB içindeki TenantDatabaseVersiyon'dan)
        public long? KimlikFirmaId { get; set; }
        public long? KimlikMaliDonemId { get; set; }
        public KullaniciRolTip? KimlikRol { get; set; }
        public string? KimlikMakineId { get; set; }
        public string? KimlikKurulumId { get; set; }
        public string? KimlikVersion { get; set; }
        public bool IsKimlikli => KimlikFirmaId != null || KimlikMaliDonemId != null || KimlikRol != null || !string.IsNullOrWhiteSpace(KimlikMakineId) || !string.IsNullOrWhiteSpace(KimlikKurulumId);
        public string KimlikRozeti => IsKimlikli ? "Kimlikli" : "Kimliksiz (eski)";

        public string BackupFileSizeDisplay => FormatFileSize(BackupFileSizeBytes);
        public string BackupDisplayName => GetBackupTypeDisplay(BackupType); // ⭐ Display property



        public string GetStatusMessage()
        {
            return IsBackupComleted
                ? "✅ Yedekleme işlemi başarıyla tamamlandı."
                : "🔴 Yedekleme başarısız: Lütfen veritabanın kullanılabilir olduğuna dikkat edin!";
        }

        private string GetBackupTypeDisplay(DatabaseBackupType type)
        {
            return type switch
            {
                DatabaseBackupType.Manual => "Manuel Yedek",
                DatabaseBackupType.Automatic => "Otomatik Yedek",
                DatabaseBackupType.Safety => "Güvenlik Yedeği",
                DatabaseBackupType.Migration => "Güncelleme Öncesi Yedek",
                DatabaseBackupType.System => "Sistem Yedeği",
                _ => "Bilinmeyen"
            };
        }
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}