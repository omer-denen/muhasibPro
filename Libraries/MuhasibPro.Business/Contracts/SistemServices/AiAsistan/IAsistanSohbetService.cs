namespace MuhasibPro.Business.Contracts.SistemServices.AiAsistan
{
    /// <summary>Konuşma geçmişi satırı. Rol: "kullanici" veya "asistan".</summary>
    public class AsistanMesajDto
    {
        public string Rol { get; set; } = "kullanici";
        public string Icerik { get; set; } = string.Empty;
    }

    /// <summary>Asistan sorusu + ekran bağlamı (sayfa/firma/dönem adı).</summary>
    public class AsistanSoruDto
    {
        public string Soru { get; set; } = string.Empty;
        public string? SayfaAnahtari { get; set; }
        public string? FirmaAdi { get; set; }
        public string? DonemAdi { get; set; }
        public IReadOnlyList<AsistanMesajDto> Gecmis { get; set; } = [];
    }

    /// <summary>Model hazırlık/çalışma durumu (determinate ilerleme + Kural 11 sonucu).</summary>
    public class AsistanDurumDto
    {
        public bool HazirMi { get; set; }
        public string Asama { get; set; } = string.Empty;
        public double? IlerlemeYuzde { get; set; }
        public string Mesaj { get; set; } = string.Empty;
    }

    /// <summary>Model satırı (yönetim listesi). BoyutBayt yalnız indirilmiş modelde doludur.</summary>
    public class AsistanModelDto
    {
        public string Alias { get; set; } = string.Empty;
        public string GosterimAdi { get; set; } = string.Empty;
        public bool IndirildiMi { get; set; }
        public bool YukluMu { get; set; }
        public long? BoyutBayt { get; set; }
    }

    /// <summary>Model önbelleği disk kullanımı.</summary>
    public class AsistanDiskKullanimiDto
    {
        public long ToplamBayt { get; set; }
        public int ModelSayisi { get; set; }
    }

    /// <summary>Model işlemi sonucu (silme fırlatmaz, sonucu döndürür — fail-closed).</summary>
    public class AsistanIslemSonucuDto
    {
        public bool BasariliMi { get; set; }
        public string Mesaj { get; set; } = string.Empty;
    }

    /// <summary>
    /// Faz 6.92: asistan sohbet sözleşmesi — provider soyutlaması budur (Open-Closed: yeni provider = yeni impl).
    /// Sürüm kapısı (ISurumOzellikService) + rol kapısı (Permission) çağırandadır, burada değil.
    /// </summary>
    public interface IAsistanSohbetService
    {
        Task<AsistanDurumDto> DurumuGetirAsync();
        Task HazirlaAsync(IProgress<AsistanDurumDto>? ilerleme = null, CancellationToken ct = default);
        IAsyncEnumerable<string> SorStreamingAsync(AsistanSoruDto soru, CancellationToken ct = default);
        /// <summary>Yüklü modeli bellekten bırakır (unload). Diskten silmez; boşta çağrı güvenli (no-op).</summary>
        Task KapatAsync();
        /// <summary>Katalog modelleri + indirme/yükleme durumu (yönetim listesi). EP indirmez.</summary>
        Task<IReadOnlyList<AsistanModelDto>> ModelleriGetirAsync(CancellationToken ct = default);
        /// <summary>İndirilmiş modellerin toplam disk kullanımı.</summary>
        Task<AsistanDiskKullanimiDto> DiskKullanimiAsync(CancellationToken ct = default);
        /// <summary>Modeli önbellekten siler (yüklüyse önce bırakır). İptal hariç fırlatmaz; sonucu döndürür.</summary>
        Task<AsistanIslemSonucuDto> ModelSilAsync(string alias, CancellationToken ct = default);
        /// <summary>Aktif model aliasını değiştirir: eskiyi bırakır + yeniyi hazırlar (ilerleme HazirlaAsync fazlarıyla).
        /// ModelAlias ayarının güncel olduğu varsayılır; ayar yazılmaz.
        /// S1 tam kilit (Oturum 292): sabit alias dışındaki hedef fırlatır (InvalidOperationException).</summary>
        Task AliasDegisiminiUygulaAsync(string yeniAlias, IProgress<AsistanDurumDto>? ilerleme = null, CancellationToken ct = default);
    }
}
