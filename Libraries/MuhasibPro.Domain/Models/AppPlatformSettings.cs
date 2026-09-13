namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// M1 AppPlatform modül ayarları — tüm değerler modelden okunur, hardcoded fallback yasaktır.
    /// Kalıcılık: ILocalSettingsService, anahtar <see cref="SettingsKey"/>.
    /// </summary>
    public class AppPlatformSettings
    {
        public const string SettingsKey = "AppPlatformSettings";

        /// <summary>Kayıtlı tema yoksa / Default ise kullanılacak tema adı. Geçerli: Default (sistemi takip et), Light, Dark.</summary>
        public string ThemeDefault { get; set; } = "Default";

        /// <summary>Splash adımları arası yapay bekleme (ms). 0-5000 arası, varsayılan 150.</summary>
        public int SplashStepDelayMs { get; set; } = 150;

        /// <summary>Durum çubuğu mesajı otomatik gizleme süresi (ms). 1000-30000 arası, varsayılan 3000.</summary>
        public int StatusAutoHideMs { get; set; } = 3000;

        /// <summary>OS bildirimleri gösterilsin mi? Varsayılan true.</summary>
        public bool NotificationEnabled { get; set; } = true;

        /// <summary>LocalSettings dosya kökü. Boşsa LocalSettingsService default'u kullanılır.</summary>
        [YoneticiAyari]
        public string ApplicationDataFolder { get; set; } = "MuhasibPro/ApplicationData";

        /// <summary>LocalSettings dosya adı. Boşsa LocalSettingsService default'u kullanılır.</summary>
        [YoneticiAyari]
        public string LocalSettingsFile { get; set; } = "LocalSettings.json";
    }
}
