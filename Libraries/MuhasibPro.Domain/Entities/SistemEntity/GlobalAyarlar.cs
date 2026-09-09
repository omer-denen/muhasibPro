using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasibPro.Domain.Entities.SistemEntity;

[Table("GlobalAyarlar")]
public class GlobalAyarlar : BaseEntity
{
    public string Anahtar { get; set; } = string.Empty;
    public string Deger { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
}
