namespace MuhasibPro.Domain.Models.DatabaseResultModel;

/// <summary>Tek tablo istatistiği (derin analiz).</summary>
public class TabloIstatistik
{
    public string TabloAdi { get; set; }
    public long KayitSayisi { get; set; }
    public long BoyutBayt { get; set; }
    public string BoyutMetni => BoyutBayt < 1024 ? $"{BoyutBayt} B" : $"{BoyutBayt / 1024d:0.#} KB";
}

/// <summary>Tenant veritabanı derin analizi (PRAGMA + tablo sayımları, salt-okunur).</summary>
public class TenantDerinAnaliz
{
    public string DatabaseName { get; set; }
    public bool DosyaVar { get; set; }
    public long DosyaBoyutu { get; set; }
    public long WalBoyutu { get; set; }
    public string JournalModu { get; set; }
    public string SqliteSurumu { get; set; }
    public long SayfaBoyutu { get; set; }
    public long SayfaSayisi { get; set; }
    public long BosSayfaSayisi { get; set; }
    public int TabloSayisi { get; set; }
    public long ToplamKayit { get; set; }
    public List<TabloIstatistik> Tablolar { get; set; } = new();
    public int IntegrityHataSayisi { get; set; }
    public string Hata { get; set; }
    public bool Basarili => string.IsNullOrEmpty(Hata) && DosyaVar;
}

/// <summary>Tek komutluk bakım işlemi sonucu (VACUUM / REINDEX / WAL checkpoint).</summary>
public class DatabaseMaintenanceResult
{
    public bool Basarili { get; set; }
    public string Mesaj { get; set; }
    public long OncekiBoyut { get; set; }
    public long SonrakiBoyut { get; set; }
    public long KazanilanBayt => OncekiBoyut - SonrakiBoyut;
}
