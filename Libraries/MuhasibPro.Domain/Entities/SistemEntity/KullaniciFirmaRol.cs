using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasibPro.Domain.Entities.SistemEntity;

[Table("KullaniciFirmaRol")]
public class KullaniciFirmaRol
{
    public long KullaniciId { get; set; }
    public long FirmaId { get; set; }
    public long RolId { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
    public Firma Firma { get; set; } = null!;
    public KullaniciRol Rol { get; set; } = null!;
}
