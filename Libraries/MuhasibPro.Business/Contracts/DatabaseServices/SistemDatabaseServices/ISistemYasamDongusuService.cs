namespace MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices
{
    /// <summary>
    /// Faz 6.79: Sistem.db yaşam-döngüsü kritik işlemleri (açılış + kapanış).
    /// Sektör deseni (Sage kapanışta-gün-ilk, QuickBooks kapanışta-otomatik + saklama-limiti).
    /// Tüm metotlar best-effort: fırlatmaz, açılışı/kapanışı engellemez.
    /// </summary>
    public interface ISistemYasamDongusuService
    {
        /// <summary>Açılış paketi: WAL checkpoint + eşiği aşanlarda otomatik yedek (WeeklyBackupDays modelden).</summary>
        Task<(bool checkpoint, bool yedekAlindi, string mesaj)> EnsureStartupSafetyAsync();
        /// <summary>Kapanış paketi: WAL checkpoint (her zaman) + ayar açıksa otomatik yedek.</summary>
        Task<string> EnsureShutdownSafetyAsync(bool kapanisYedegiAcik);
    }
}
