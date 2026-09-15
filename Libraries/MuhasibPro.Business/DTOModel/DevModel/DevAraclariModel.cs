namespace MuhasibPro.Business.DTOModel.DevModel;

/// <summary>Geliştirici araçları — kurulum kimliği + şema damga durumu (salt-okunur anlık görüntü).</summary>
public class DevAracDurumuModel
{
    public string KurulumId { get; set; } = string.Empty;
    public string MakineId { get; set; } = string.Empty;
    public string MachineGuid { get; set; } = string.Empty;

    /// <summary>Bu kodun dağıttığı uygulama şema sürümü (SemVer).</summary>
    public string UygulamaSemVer { get; set; } = string.Empty;

    public string LogKlasoru { get; set; } = string.Empty;
    public string VeriKlasoru { get; set; } = string.Empty;
    public string SistemDbYolu { get; set; } = string.Empty;

    public bool AyrintiliLog { get; set; }

    public List<DevTenantDamgaModel> TenantDamgalari { get; set; } = new();

    public string KurulumIdKisa => Kisalt(KurulumId);
    public string MakineIdKisa => Kisalt(MakineId);
    public string DamgaOzeti => $"{TenantDamgalari.Count} dönem damgası";

    internal static string Kisalt(string deger)
        => string.IsNullOrWhiteSpace(deger) ? "-" : (deger.Length > 14 ? deger[..14] + "…" : deger);
}

/// <summary>Tek bir tenant veritabanının kimlik/şema damgası (Faz 6.82 görünüm satırı).</summary>
public class DevTenantDamgaModel
{
    public string DatabaseName { get; set; } = string.Empty;
    public string SemVer { get; set; } = string.Empty;
    public string KurulumId { get; set; } = string.Empty;
    public string MakineId { get; set; } = string.Empty;

    public bool KurulumEslesiyor { get; set; }
    public bool MakineEslesiyor { get; set; }

    public string KimlikMetni => KurulumEslesiyor && MakineEslesiyor
        ? "eşleşiyor"
        : (!MakineEslesiyor ? "makine farklı" : "kurulum farklı");
}

/// <summary>Geliştirici aracı işlem sonucu (Kural 11: her işlem kullanıcıya sonuç verir).</summary>
public class DevAracSonucuModel
{
    public bool Basarili { get; set; }
    public string Mesaj { get; set; } = string.Empty;
}
