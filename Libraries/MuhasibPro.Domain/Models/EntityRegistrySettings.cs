using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// M3 EntityRegistry ayarları — firma/dönem kayıt kuralları + liste limitleri kullanıcı tarafından yönetilir.
    /// Kalıcılık: ILocalSettingsService, anahtar <see cref="SettingsKey"/>.
    /// </summary>
    public class EntityRegistrySettings
    {
        public const string SettingsKey = "EntityRegistrySettings";

        /// <summary>Firma kodu deseni ("X" = rakam, diğerleri aynen). Varsayılan F-XXXX.</summary>
        [YoneticiAyari]
        public string FirmaKodPattern { get; set; } = "F-XXXX";

        /// <summary>Yeni mali dönem varsayılan durumu. Varsayılan Acik.</summary>
        public DonemDurum DefaultDurum { get; set; } = DonemDurum.Acik;

        /// <summary>Açık dönemler sayfa boyutu. 1-50 arası, varsayılan 8.</summary>
        public int AcikPageSize { get; set; } = 8;

        /// <summary>Arşivli dönemler sayfa boyutu. 1-50 arası, varsayılan 5.</summary>
        public int ArsivPageSize { get; set; } = 5;

        /// <summary>Sıkı doğrulama: firma kodu desene uymazsa kayıt reddedilir. Varsayılan true.</summary>
        [YoneticiAyari]
        public bool ValidationStrict { get; set; } = true;

        public int GetAcikPageSize() => Math.Clamp(AcikPageSize, 1, 50);
        public int GetArsivPageSize() => Math.Clamp(ArsivPageSize, 1, 50);

        /// <summary>Boş/geçersiz desen yerine varsayılan desen.</summary>
        public string GetFirmaKodPattern()
            => string.IsNullOrWhiteSpace(FirmaKodPattern) ? "F-XXXX" : FirmaKodPattern.Trim();
    }
}
