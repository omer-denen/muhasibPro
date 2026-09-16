namespace MuhasibPro.Business.Contracts.SistemServices.AiAsistan
{
    /// <summary>Faz 6.93: yardım bilgi tabanı kaydı (tek madde).</summary>
    public class YardimKaydi
    {
        public string Anahtar { get; set; } = string.Empty;
        public string Sayfa { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public IReadOnlyList<string> Etiketler { get; set; } = [];
    }

    /// <summary>Faz 6.93: hibrit arama sonucu.</summary>
    public class YardimAramaSonucu
    {
        public string Anahtar { get; set; } = string.Empty;
        public string Sayfa { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public double Skor { get; set; }
        /// <summary>"lexical" | "vektor" | "hibrit".</summary>
        public string Yontem { get; set; } = string.Empty;
    }

    /// <summary>Faz 6.93: dizin hazırlık/durum bilgisi (ilerleme + Kural 11 sonucu).</summary>
    public class YardimIndexDurumu
    {
        public bool HazirMi { get; set; }
        public int MaddeSayisi { get; set; }
        public bool VektorVarMi { get; set; }
        public string Asama { get; set; } = string.Empty;
        public double? IlerlemeYuzde { get; set; }
        public string Mesaj { get; set; } = string.Empty;
    }

    /// <summary>AI yardım bilgi tabanı: içerik + (varsa) semantik indeks üzerinden hibrit arama.</summary>
    public interface IYardimBilgiTabani
    {
        Task<YardimIndexDurumu> DurumGetirAsync(CancellationToken ct = default);
        /// <summary>İçeriği okur, DB'yi tazeler, eksik vektörleri üretir. Embedding yoksa lexical'e düşer (fırlatmaz).
        /// <paramref name="modelIndirmeyeIzin"/> false ise embedding modeli İNDİRİLMEZ; yalnız önbellekte varsa kullanılır (güncelleme adımı 6.91-G).</summary>
        Task HazirlaAsync(bool modelIndirmeyeIzin = true, IProgress<YardimIndexDurumu>? ilerleme = null, CancellationToken ct = default);
        Task<IReadOnlyList<YardimAramaSonucu>> AraAsync(string soru, int enFazla, CancellationToken ct = default);
        /// <summary>Yüklü tüm kayıtlar (dev-mode öz-testi sayımı için).</summary>
        IReadOnlyList<YardimKaydi> TumKayitlar();
    }

    /// <summary>Metni vektöre çevirir (Foundry embedding). Model indirilemezse FIRLATIR — çağıran lexical'e düşer.</summary>
    public interface IYardimVektorUretici
    {
        string ModelAlias { get; }
        Task<int> VektorBoyutuAsync(CancellationToken ct = default);
        /// <summary>Embedding modeli önbellekte/yüklenebilir mi (İNDİRME YAPMAZ). Güncelleme adımı bunu kontrol eder.</summary>
        Task<bool> OnbellekteMiAsync(CancellationToken ct = default);
        Task<IReadOnlyList<float[]>> UretAsync(IReadOnlyList<string> metinler, IProgress<double>? ilerleme = null, CancellationToken ct = default);
    }

    /// <summary>Gömülü yardım içeriği ham kaynağı (tek Markdown dosyası).</summary>
    public class YardimHamKaynak
    {
        public string DosyaAdi { get; set; } = string.Empty;
        public string HamMetin { get; set; } = string.Empty;
    }

    /// <summary>Gömülü yardım içeriği kaynağı (Markdown).</summary>
    public interface IYardimIcerikKaynagi
    {
        IReadOnlyList<YardimHamKaynak> KaynaklariGetir();
    }
}
