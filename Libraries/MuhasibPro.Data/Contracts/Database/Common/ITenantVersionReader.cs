namespace MuhasibPro.Data.Contracts.Database.Common
{
    /// <summary>Tenant damga satırının taşınmış-veri karşılaştırması için ham okuması (Data'ya ait).</summary>
    public class TenantMismatchInfo
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string? VersiyonKurulumId { get; set; }
        public string? VersiyonMakineId { get; set; }
        public bool KurulumFarkli { get; set; }
        public bool MakineFarkli { get; set; }
    }

    public interface ITenantVersionReader
    {
        /// <param name="currentKurulumId">Bu kurulumun id'si.</param>
        /// <param name="currentMachineId">Bu makinenin id'si.</param>
        /// <returns>Dosyası var + damgası farklı dönemler. Best-effort: arızada boş liste.</returns>
        Task<IReadOnlyList<TenantMismatchInfo>> ScanMismatchesAsync(string currentKurulumId, string currentMachineId);

        /// <summary>Tüm tenant damga satırlarının ham okuması (geliştirici araçları — şema damgası görünümü).</summary>
        /// <returns>Dosyası var + damga satırı olan dönemler. Best-effort: arızada boş liste.</returns>
        Task<IReadOnlyList<TenantDamgaInfo>> GetStampsAsync();
    }

    /// <summary>Tenant damga satırının tam okuması (geliştirici araçları görünümü).</summary>
    public class TenantDamgaInfo
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string? KurulumId { get; set; }
        public string? MakineId { get; set; }
        public string? SemVer { get; set; }
    }
}
