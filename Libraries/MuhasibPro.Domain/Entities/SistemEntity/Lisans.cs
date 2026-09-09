using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasibPro.Domain.Entities.SistemEntity;

public enum LisansTuru
{
    Deneme = 1,
    Standart = 2,
    Profesyonel = 3,
    Kurumsal = 4
}

[Table("Lisanslar")]
public class Lisans : BaseEntity
{
    public LisansTuru Tur { get; set; }
    public string LisansAnahtari { get; set; } = string.Empty;
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string? Aciklama { get; set; }
}
