namespace MuhasibPro.Data.Contracts.Database.Common.Helpers;

/// <summary>Tek tablodaki gerçek değişiklik (kolon listeleri migration sınıfından).</summary>
public class TenantTableChange
{
    public string Table { get; set; } = string.Empty;
    public bool IsCreated { get; set; }
    public List<string> AddedColumns { get; set; } = new();
    public List<string> AlteredColumns { get; set; } = new();
    public List<string> RemovedColumns { get; set; } = new();
}
