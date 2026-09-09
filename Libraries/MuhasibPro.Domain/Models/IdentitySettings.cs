namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// M2 kimlik ayarları — kilit eşikleri + hash gücü modelden okunur.
    /// Kalıcılık: ILocalSettingsService, anahtar <see cref="SettingsKey"/>.
    /// </summary>
    public class IdentitySettings
    {
        public const string SettingsKey = "IdentitySettings";

        /// <summary>Kilit öncesi izin verilen hatalı deneme. Varsayılan 5.</summary>
        [YoneticiAyari]
        public int MaxFailedAttempts { get; set; } = 5;

        /// <summary>Kilit süresi (dakika). Varsayılan 5.</summary>
        [YoneticiAyari]
        public int LockoutMinutes { get; set; } = 5;

        /// <summary>Deneme sayacının sıfırlandığı pencere (dakika). Varsayılan 5.</summary>
        [YoneticiAyari]
        public int AttemptWindowMinutes { get; set; } = 5;

        /// <summary>Yeni parolalarda PBKDF2 iterasyon sayısı. Varsayılan 100000.</summary>
        [YoneticiAyari]
        public int Pbkdf2Iterations { get; set; } = 100000;

        /// <summary>Yeni parola en az uzunluğu. Varsayılan 6.</summary>
        public int MinPasswordLength { get; set; } = 6;
    }
}
