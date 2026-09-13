namespace MuhasibPro.Domain.Models.DatabaseResultModel
{
    /// <summary>Faz 6.78 Adım 1: yedek-dosyanın salt-okunur analizi (1. katman).</summary>
    public class RestoreDosyaAnalizi
    {
        public string DosyaAdi { get; set; } = string.Empty;
        public bool DosyaVarMi { get; set; }
        public bool IntegrityTamamMi { get; set; }
        public string IntegrityMesaji { get; set; } = string.Empty;
        public int TabloSayisi { get; set; }
        public string? GocGecmisiSurumu { get; set; }
        public long DosyaBoyutu { get; set; }
        public DateTime YedekTarihi { get; set; }
    }

    /// <summary>Faz 6.78 Adım 1: mevcut DB ↔ yedek farkı (2. katman, ki taraf da satır-karşılaştırma yapar).</summary>
    public class RestoreFarkOzeti
    {
        /// <summary>Yedekten sonra açılan/eklenen kayıtların görünen satırları (örn. "Mali dönem 2028").</summary>
        public List<string> KayipKayitlar { get; set; } = new();
        /// <summary>Yedekte karşılığı olmayan tenant DB dosyaları.</summary>
        public List<string> KayipTenantDosyalari { get; set; } = new();
        public bool KayipVarMi => KayipKayitlar.Count > 0 || KayipTenantDosyalari.Count > 0;
        public int ToplamKayip => KayipKayitlar.Count + KayipTenantDosyalari.Count;
    }

    /// <summary>Faz 6.78 Adım 1: hüküm motoru girdisi (iki katman + kimlik).</summary>
    public class RestoreAnalizGirdisi
    {
        public RestoreDosyaAnalizi Dosya { get; set; } = new();
        public RestoreFarkOzeti Fark { get; set; } = new();
        public bool KimlikliMi { get; set; }
        public string? YedekVersion { get; set; }
        public string? MevcutVersion { get; set; }
        public bool KurulumFarkli { get; set; }
        public bool MakineFarkli { get; set; }

        /// <summary>
        /// Faz 6.78 Adım 2: kimlik uyuşmazlığı (firma/dönem/ad) başlığı — doluysa RequireCode üretir.
        /// Tenant hattı kendi kimlik kıyasını buraya yazar; ortak çekirdek hükmü verir.
        /// </summary>
        public string? KimlikUyusmazlikBaslik { get; set; }

        /// <summary>Kimlik uyuşmazlığı açıklaması (çağırandan gelir).</summary>
        public string? KimlikUyusmazlikAciklama { get; set; }

        public bool KimlikUyusmazMi => !string.IsNullOrWhiteSpace(KimlikUyusmazlikBaslik);
    }
}
