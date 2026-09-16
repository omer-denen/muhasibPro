namespace MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D: dönem (tenant) tarama özeti — Kural 7 (karar veriden).</summary>
    public class TenantTaramaSonucu
    {
        /// <summary>Tarama hiç yapılamadı (liste okunamadı / hata).</summary>
        public bool Taranamadi { get; set; }

        public string Mesaj { get; set; } = string.Empty;

        /// <summary>Kullanıcıya gösterilecek dönem-bazlı rapor satırları (bozuk/gelecek şema/bekleyen).</summary>
        public List<string> RaporSatirlari { get; set; } = new();

        public int Taranan { get; set; }
        public int Bozuk { get; set; }
        public int Kurtarilan { get; set; }
        public int Bekleyen { get; set; }
        public int GelecekSema { get; set; }
    }

    /// <summary>Faz 6.91-D: post-update dönem taraması — her tenant state (connect/valid/pending/future):
    /// bozuk → son doğrulanmış yedekten restore; pending → yalnız rapor (erişimdeki göç saga'sı uygular);
    /// gelecek şema → dokunma + rapor (fail-closed).</summary>
    public interface ITenantTaramaDogrulayici
    {
        /// <param name="ilerleme">0-100 arası tarama ilerlemesi (opsiyonel).</param>
        Task<TenantTaramaSonucu> TaraAsync(IProgress<double>? ilerleme = null, CancellationToken cancellationToken = default);
    }
}
