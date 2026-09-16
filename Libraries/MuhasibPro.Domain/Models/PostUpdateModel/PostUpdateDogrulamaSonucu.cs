namespace MuhasibPro.Domain.Models.PostUpdateModel
{
    /// <summary>Faz 6.91-D: güncelleme sonrası doğrulama sagasının nihai sonucu (Kural 7 — gerçek veri).
    /// Hero (sonuç) paneli <see cref="SonucTuru"/> + <see cref="Baslik"/> + <see cref="Ozet"/>'ten beslenir.</summary>
    public class PostUpdateDogrulamaSonucu
    {
        /// <summary>Saga tamamlandı mı (sert blok yoksa true).</summary>
        public bool Basarili { get; set; }

        /// <summary>Uygulama açılışını bloklar mı (yalnız Sistem.db gelecek şema / göç+restore başarısız).</summary>
        public bool Bloklayici { get; set; }

        public PostUpdateSonucTuru SonucTuru { get; set; } = PostUpdateSonucTuru.Temiz;

        /// <summary>Hero başlığı (kullanıcıya gösterilir; View bunu aynen basar).</summary>
        public string Baslik { get; set; } = string.Empty;

        /// <summary>Hero açıklaması — tek cümlelik özet/sebep (Kural 12 — tek özet).</summary>
        public string Ozet { get; set; } = string.Empty;

        /// <summary>Adım izleri (rozetli akış planı). Her üç adım da her zaman yer alır (başarılı/uyarı/hata/atlandı).</summary>
        public List<PostUpdateAdimSonucu> Adimlar { get; set; } = new();

        /// <summary>Dönem-bazlı ayrıntı satırları (bozuk / güncelleme bekleyen / daha yeni sürüm).</summary>
        public List<string> RaporSatirlari { get; set; } = new();

        public long SureMs { get; set; }

        // ---- Dönem tarama sayaçları (Kural 7 — karar veriden) ----
        public int TarananDonemSayisi { get; set; }
        public int BozukDonemSayisi { get; set; }
        public int BekleyenDonemSayisi { get; set; }
        public int GelecekSemaDonemSayisi { get; set; }

        /// <summary>Kısa tek-satır özet (sayaçlar + eklenen ayrıntı yok).</summary>
        public string SatirOzeti()
        {
            var parcalar = new List<string>();
            if (TarananDonemSayisi > 0) parcalar.Add($"{TarananDonemSayisi} dönem tarandı");
            if (BozukDonemSayisi > 0) parcalar.Add($"{BozukDonemSayisi} bozuk");
            if (BekleyenDonemSayisi > 0) parcalar.Add($"{BekleyenDonemSayisi} güncelleme bekliyor");
            if (GelecekSemaDonemSayisi > 0) parcalar.Add($"{GelecekSemaDonemSayisi} daha yeni sürüm");

            const string temel = "Güncelleme sonrası doğrulama tamamlandı";
            return parcalar.Count > 0 ? $"{temel} • {string.Join(" • ", parcalar)}." : $"{temel}.";
        }
    }
}
