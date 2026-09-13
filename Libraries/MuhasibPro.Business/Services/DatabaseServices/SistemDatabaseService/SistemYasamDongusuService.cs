using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Domain.Enum.DatabaseEnum;

namespace MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService
{
    /// <summary>Faz 6.79: tek sorumluluk — Sistem.db açılış/kapanış güvenlik paketi (Kural 1).</summary>
    public class SistemYasamDongusuService : ISistemYasamDongusuService
    {
        private readonly ISistemBackupManager _yedekYoneticisi;
        private readonly ISistemDatabaseOperationService _operasyon;
        private readonly IDatabaseSettingsProvider _ayarlar;
        private readonly ISistemLogService _logServisi;

        public SistemYasamDongusuService(
            ISistemBackupManager yedekYoneticisi,
            ISistemDatabaseOperationService operasyon,
            IDatabaseSettingsProvider ayarlar,
            ISistemLogService logServisi)
        {
            _yedekYoneticisi = yedekYoneticisi;
            _operasyon = operasyon;
            _ayarlar = ayarlar;
            _logServisi = logServisi;
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
