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
    }
}
