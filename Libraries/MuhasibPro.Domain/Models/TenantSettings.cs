namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// M5 Tenant ayarları — liste limitleri, toplu işlem boyutu ve bağlantı/bakım/göç eşikleri
    /// kullanıcı tarafından yönetilir.
    /// Kalıcılık: ILocalSettingsService, anahtar <see cref="SettingsKey"/>.
    /// Derin bağlantı (Oturum 127): connection-string (BusyTimeoutMs/Pooling),
    /// bakım (BakimTimeoutSec) ve göç (MigrationRetry/CommandTimeoutSec) değerleri
    /// Data katmanına opsiyonel parametrelerle taşınır; Data Business bilmez,
    /// clamp mantığı buradaki statiklerde tek kaynaktır.
    /// Not: Microsoft.Data.Sqlite "Default Timeout"u SANİYE ister; BusyTimeoutMs
    /// saniyeye çevrilir (en az 1 sn): <see cref="BusyTimeoutMsToSeconds"/>.
    /// </summary>
    public class TenantSettings
    {
        public const string SettingsKey = "TenantSettings";

        public const int DefaultBusyTimeoutMs = 5000;
        public const int DefaultCommandTimeoutSec = 30;
        public const int DefaultBakimTimeoutSec = 120;
        public const int DefaultMigrationRetry = 2;

        /// <summary>Dönem yedekleri sayfa boyutu. 1-50 arası, varsayılan 4.</summary>
        public int YedekPageSize { get; set; } = 4;

        /// <summary>Bilinmeyen yedekler sayfa boyutu. 1-50 arası, varsayılan 4.</summary>
        public int BilinmeyenPageSize { get; set; } = 4;

        /// <summary>Toplu tarama sayfa boyutu (backfill). 10-1000 arası, varsayılan 100.</summary>
        [YoneticiAyari]
        public int BackfillSayfaBoyutu { get; set; } = 100;

        /// <summary>Göç yedek/geri-yükleme deneme sayısı. 1-5 arası, varsayılan 2.</summary>
        [YoneticiAyari]
        public int MigrationRetry { get; set; } = 2;

        /// <summary>Bakım komutu zaman aşımı (sn). 30-600 arası, varsayılan 120.</summary>
        [YoneticiAyari]
        public int BakimTimeoutSec { get; set; } = 120;

        /// <summary>Komut zaman aşımı (sn). 5-300 arası, varsayılan 30.</summary>
        [YoneticiAyari]
        public int CommandTimeoutSec { get; set; } = 30;

        /// <summary>SQLite busy timeout (ms). 1000-30000 arası, varsayılan 5000.</summary>
        [YoneticiAyari]
        public int BusyTimeoutMs { get; set; } = 5000;

        /// <summary>Bağlantı havuzu kullanılsın mı? Varsayılan true.</summary>
        [YoneticiAyari]
        public bool Pooling { get; set; } = true;

        /// <summary>Son değiştiren (görünen ad). Denetim satırı için; yetki denetimine girmez.</summary>
        public string SonDegistiren { get; set; } = string.Empty;

        /// <summary>Son değişiklik tarihi (yerel saat). Hiç kaydedilmediyse default.</summary>
        public DateTime SonDegisiklikTarihi { get; set; }

        public int GetYedekPageSize() => Math.Clamp(YedekPageSize, 1, 50);
        public int GetBilinmeyenPageSize() => Math.Clamp(BilinmeyenPageSize, 1, 50);
        public int GetBackfillSayfaBoyutu() => Math.Clamp(BackfillSayfaBoyutu, 10, 1000);
        public int GetMigrationRetry() => ClampMigrationRetry(MigrationRetry);
        public int GetBakimTimeoutSec() => ClampBakimTimeoutSec(BakimTimeoutSec);
        public int GetCommandTimeoutSec() => ClampCommandTimeoutSec(CommandTimeoutSec);
        public int GetBusyTimeoutMs() => ClampBusyTimeoutMs(BusyTimeoutMs);

        /// <summary>Null bozuk aralık dışı → varsayılan (Data katmanı da buradan clamp'ler).</summary>
        public static int ClampMigrationRetry(int? v)
            => v == null ? DefaultMigrationRetry : Math.Clamp(v.Value, 1, 5);
        public static int ClampBakimTimeoutSec(int? v)
            => v == null ? DefaultBakimTimeoutSec : Math.Clamp(v.Value, 30, 600);
        public static int ClampCommandTimeoutSec(int? v)
            => v == null ? DefaultCommandTimeoutSec : Math.Clamp(v.Value, 5, 300);
        public static int ClampBusyTimeoutMs(int? v)
            => v == null ? DefaultBusyTimeoutMs : Math.Clamp(v.Value, 1000, 30000);

        /// <summary>BusyTimeoutMs → connection-string saniyesi (en az 1).</summary>
        public static int BusyTimeoutMsToSeconds(int? busyTimeoutMs)
            => Math.Max(1, ClampBusyTimeoutMs(busyTimeoutMs) / 1000);
    }
}
