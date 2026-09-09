using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// M2-politika lisans ayarları — lisans türü + kontrol aralığı modelden okunur.
    /// Kalıcılık: ILocalSettingsService, anahtar <see cref="SettingsKey"/>.
    /// </summary>
    public class LicenseSettings
    {
        public const string SettingsKey = "LicenseSettings";

        /// <summary>Etkin lisans türü. Varsayılan Deneme. Kritik — yalnızca yönetici değiştirir.</summary>
        [YoneticiAyari]
        public LisansTuru Tur { get; set; } = LisansTuru.Deneme;

        /// <summary>Lisans kontrol aralığı (gün). Varsayılan 7.</summary>
        public int CheckIntervalDays { get; set; } = 7;

        /// <summary>Aralık dışındaysa varsayılana çeker (1-90).</summary>
        public int GetCheckIntervalDays() => Math.Clamp(CheckIntervalDays, 1, 90);
    }
}
