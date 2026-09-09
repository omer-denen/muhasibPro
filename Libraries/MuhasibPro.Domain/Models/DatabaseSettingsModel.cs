namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// M4 SystemDb ayarları — tüm saklama/timeout/journal değerleri modelden okunur.
    /// Kalıcılık: ILocalSettingsService, anahtar "DatabaseSettings".
    /// </summary>
    public class DatabaseSettingsModel
    {
        public const string SettingsKey = "DatabaseSettings";

        /// <summary>Manuel yedek saklama limiti — dolunca en eski silinir (FIFO). 1-20 arası, varsayılan 5.</summary>
        [YoneticiAyari]
        public int MaxManuelYedekSayisi { get; set; } = 5;

        /// <summary>Limit aşılınca en eski yedek otomatik silinsin mi? Varsayılan true.</summary>
        [YoneticiAyari]
        public bool OtomatikTemizlemeAcik { get; set; } = true;

        /// <summary>Uygulama kapanışında açık dönemin yedeği otomatik alınsın mı? Varsayılan false.</summary>
        public bool KapanistaOtomatikYedek { get; set; } = false;

        /// <summary>Haftalık bütünlük kontrolü hatırlatması. Varsayılan true.</summary>
        public bool HaftalikButunlukKontrolu { get; set; } = true;

        /// <summary>Sistem.db hızlı tanı yedek saklama adedi. Varsayılan 3.</summary>
        [YoneticiAyari]
        public int SistemKeepLast { get; set; } = 3;

        /// <summary>SQLite busy timeout (ms). Varsayılan 5000.</summary>
        [YoneticiAyari]
        public int BusyTimeoutMs { get; set; } = 5000;

        /// <summary>SQLite journal modu. Varsayılan WAL.</summary>
        [YoneticiAyari]
        public string JournalMode { get; set; } = "WAL";

        /// <summary>SQLite synchronous ayarı. Varsayılan NORMAL.</summary>
        [YoneticiAyari]
        public string Synchronous { get; set; } = "NORMAL";

        /// <summary>Otomatik yedek aralığı (gün). Varsayılan 7.</summary>
        public int WeeklyBackupDays { get; set; } = 7;

        /// <summary>Yedek öncesi VACUUM çalışsın mı? Varsayılan true.</summary>
        [YoneticiAyari]
        public bool VacuumOnBackup { get; set; } = true;

        /// <summary>Manuel keep, 1-20 aralığına sabitlenmiş.</summary>
        public int GetManuelKeep() => Math.Clamp(MaxManuelYedekSayisi, 1, 20);

        /// <summary>Sistem keep, 1-20 aralığına sabitlenmiş.</summary>
        public int GetSistemKeep() => Math.Clamp(SistemKeepLast, 1, 20);
    }
}
