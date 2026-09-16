namespace MuhasibPro.Domain.Models.PostUpdateModel
{
    /// <summary>Faz 6.91-D: güncelleme sonrası doğrulama sagasının tek adım izi.
    /// Hem canlı ilerleme bildirimi hem nihai sonuç listesinde aynı sözleşme kullanılır (Kural 3).</summary>
    public class PostUpdateAdimSonucu
    {
        /// <summary>Adımın sabit kimliği — VM bu adla ilgili rozeti eşler.</summary>
        public string Ad { get; set; } = string.Empty;

        public PostUpdateAdimDurumu Durum { get; set; } = PostUpdateAdimDurumu.Bekliyor;

        public string Mesaj { get; set; } = string.Empty;

        /// <summary>Adım sonu genel ilerleme yüzdesi (0-100).</summary>
        public double Yuzde { get; set; }
    }
}
