using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Domain.Enum.DatabaseEnum;

namespace MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService
{
    /// <summary>Faz 6.79: tek sorumluluk — Sistem.db açılış/kapanış güvenlik paketi (Kural 1).
    /// Faz 6.91-B: güncelleme öncesi zorunlu yedek paketi eklendi.</summary>
    public class SistemYasamDongusuService : ISistemYasamDongusuService
    {
        private readonly ISistemBackupManager _yedekYoneticisi;
        private readonly ISistemDatabaseOperationService _operasyon;
        private readonly IDatabaseSettingsProvider _ayarlar;
        private readonly ISistemLogService _logServisi;
        private readonly IApplicationPaths _yollar;

        public SistemYasamDongusuService(
            ISistemBackupManager yedekYoneticisi,
            ISistemDatabaseOperationService operasyon,
            IDatabaseSettingsProvider ayarlar,
            ISistemLogService logServisi,
            IApplicationPaths yollar)
        {
            _yedekYoneticisi = yedekYoneticisi;
            _operasyon = operasyon;
            _ayarlar = ayarlar;
            _logServisi = logServisi;
            _yollar = yollar;
        }

        public async Task<(bool basarili, string? yedekYolu, string mesaj)> EnsureUpdateSafetyAsync()
        {
            try { await _yedekYoneticisi.CheckpointWalAsync(); }
            catch { /* best-effort */ }

            // Sistem.db yok/sağlıksız (ilk kurulum) → yedek gerekmez, güncellemeyi engelleme.
            try
            {
                if (!_yollar.SistemDatabaseFileExists() || !_yollar.IsSqliteDatabaseFileValid(_yollar.GetSistemDatabaseFilePath()))
                    return (true, null, "Sistem.db yok — güncelleme öncesi yedek gerekmedi.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Sistem.db durumu okunamadı: {ex.Message}");
            }

            try
            {
                var yedek = await _operasyon.CreateBackupAsync(DatabaseBackupType.Migration);
                if (yedek == null || !yedek.Success || yedek.Data == null || !yedek.Data.IsBackupComleted
                    || string.IsNullOrWhiteSpace(yedek.Data.BackupFilePath))
                {
                    await _logServisi.SistemLogErrorAsync("Sistem Veritabanı", "Güncelleme Öncesi Yedek",
                        "Yedek alınamadı — güncelleme durduruldu", yedek?.Message ?? "bilinmeyen hata");
                    return (false, null, "Güncelleme öncesi Sistem.db yedeği alınamadı — güncelleme durduruldu.");
                }

                var yol = yedek.Data.BackupFilePath;
                var ayar = await _ayarlar.GetAsync();
                await _operasyon.CleanOldBackupsAsync(ayar.GetSistemKeep());
                await _logServisi.SistemLogInformationAsync("Sistem Veritabanı", "Güncelleme Öncesi Yedek",
                    "Doğrulanmış yedek alındı", yol);
                return (true, yol, "Güncelleme öncesi Sistem.db yedeği alındı.");
            }
            catch (Exception ex)
            {
                await _logServisi.SistemLogExceptionAsync("Sistem Veritabanı", "Güncelleme Öncesi Yedek", ex);
                return (false, null, $"Güncelleme öncesi yedek hatası: {ex.Message}");
            }
        }

        public async Task<(bool checkpoint, bool yedekAlindi, string mesaj)> EnsureStartupSafetyAsync()
        {
            bool checkpoint = false;
            try { checkpoint = await _yedekYoneticisi.CheckpointWalAsync(); }
            catch { /* best-effort */ }

            try
            {
                var ayar = await _ayarlar.GetAsync();
                int esikGun = ayar.WeeklyBackupDays;
                if (esikGun < 1 || esikGun > 30) esikGun = 7;
                int keep = ayar.GetSistemKeep();
                var son = _operasyon.GetLastBackupDate();
                double gun = son.HasValue ? (DateTime.Now - son.Value).TotalDays : double.MaxValue;
                if (gun < esikGun)
                    return (checkpoint, false, checkpoint ? "WAL aktarıldı; son yedek güncel." : "Son yedek güncel.");

                var yedek = await _operasyon.CreateBackupAsync(DatabaseBackupType.Automatic);
                if (yedek == null || !yedek.Success)
                    return (checkpoint, false, "Otomatik yedek alınamadı — mevcut veriyle devam ediliyor.");
                await _operasyon.CleanOldBackupsAsync(keep);
                await _logServisi.SistemLogInformationAsync("Sistem Veritabanı", "Açılış Güvenliği",
                    "Otomatik yedek alındı", $"Son yedek {gun:0} gündür alınmıyordu (eşik {esikGun} gün).");
                return (checkpoint, true, $"Otomatik yedek alındı (son yedek {gun:0} günlüktü).");
            }
            catch
            {
                return (checkpoint, false, "Açılış güvenlik paketi tamamlanamadı — mevcut veriyle devam ediliyor.");
            }
        }

        public async Task<string> EnsureShutdownSafetyAsync(bool kapanisYedegiAcik)
        {
            try { await _yedekYoneticisi.CheckpointWalAsync(); }
            catch { /* best-effort */ }

            if (!kapanisYedegiAcik)
                return "WAL aktarıldı.";

            try
            {
                var ayar = await _ayarlar.GetAsync();
                int keep = ayar.GetSistemKeep();
                var yedek = await _operasyon.CreateBackupAsync(DatabaseBackupType.Automatic);
                if (yedek == null || !yedek.Success)
                    return "WAL aktarıldı; kapanış yedeği alınamadı.";
                await _operasyon.CleanOldBackupsAsync(keep);
                return "WAL aktarıldı, kapanış yedeği alındı.";
            }
            catch
            {
                return "WAL aktarıldı; kapanış yedeği tamamlanamadı.";
            }
        }
    }
}
