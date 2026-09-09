namespace MuhasibPro.Data.Contracts.Database.Common.Helpers;

/// <summary>Tek migration'ın gerçek yapı açıklaması (sınıf kodundan üretilir, hardcoded değil).</summary>
public class TenantMigrationDescription
{
    public string MigrationId { get; set; } = string.Empty;
    public bool IsKnown { get; set; }
    /// <summary>Tür etiketi: "Tablo güncellemesi" / "Sorgu güncellemesi" / "İndeks güncellemesi" / "Şema güncellemesi" / "Şema Değişikliği".</summary>
    public string ChangeKind { get; set; } = "Şema Değişikliği";
    /// <summary>Tek-satır başlık (örn. "'TenantDatabaseVersiyonlar' tablosu güncellemesi").</summary>
    public string Headline { get; set; } = "Şema Değişikliği";
    public string Summary { get; set; } = "Şema Değişikliği";
    public List<string> Details { get; set; } = new();
    public List<TenantTableChange> Tables { get; set; } = new();
}
