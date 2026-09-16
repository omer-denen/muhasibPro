namespace MuhasibPro.Domain.Models
{
    public class UpdateSettingsModel
    {
        public const string SettingsKey = "UpdateSettings";

        /// <summary>Varsayılan güncelleme kaynağı — derlemede gömülü git remote (repo adresi).
        /// Kaynak adresi boşsa bu değer kullanılır; böylece kullanıcı adres girmek zorunda kalmaz.</summary>
        public static string VarsayilanFeedUrl => AppGuncellemeBilgisi.VarsayilanFeedUrl;

        public bool AutoCheckOnStartup { get; set; } = true;
        public bool ShowNotifications { get; set; } = true;
        public bool IncludeBetaVersions { get; set; } = false;
        public DateTime? LastCheckTime { get; set; }
        /// <summary>Manuel güncelleme kaynağı (GitHub repo URL veya Velopack feed URL). Boşsa kontrol yapılmaz.</summary>
        public string FeedUrl { get; set; } = string.Empty;

        // ---- Faz 6.91-B: güncelleme öncesi hazırlık kaydı ----
        /// <summary>Güncelleme öncesi çalışan sürüm (velo current → fallback assembly).</summary>
        public string? LastUpdateFromVersion { get; set; }
        /// <summary>Hedeflenen yeni sürüm (Velopack feed).</summary>
        public string? LastUpdateToVersion { get; set; }
        /// <summary>Güncelleme öncesi alınan doğrulanmış Sistem.db yedeğinin tam yolu.</summary>
        public string? LastUpdateBackupPath { get; set; }
        /// <summary>Güncelleme hazırlığının başladığı an.</summary>
        public DateTime? LastUpdateStartTime { get; set; }

        // ---- Faz 6.91-D: güncelleme sonrası doğrulama damgası ----
        /// <summary>Uygulama yeni sürüme geçtikten sonra post-update doğrulaması başarıyla tamamlandığı an.</summary>
        public DateTime? LastUpdateVerifiedAt { get; set; }

        /// <summary>Faz 6.91-D Revizyon 3 (B2): güncelleme ön-yedeği başarıyla alındıktan sonra yazılan tetik bayrağı.
        /// Açılışta doğrulama sagası yalnız bu bayrak true iken çalışır; iptal/başarısız hazırlıkta false'a döner,
        /// böylece iptal edilen bir güncelleme sonraki açılışta yanlış doğrulama ekranı açmaz.</summary>
        public bool PostUpdatePending { get; set; }
    }
}
