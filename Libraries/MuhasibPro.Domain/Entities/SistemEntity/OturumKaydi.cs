using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasibPro.Domain.Entities.SistemEntity;

[Table("OturumKayitlari")]
public class OturumKaydi : BaseEntity
{
    public long KullaniciId { get; set; }
    public long FirmaId { get; set; }
    public long MaliDonemId { get; set; }
    public string OturumToken { get; set; } = string.Empty;
    public DateTime GirisZamani { get; set; } = DateTime.UtcNow;
    public DateTime SonHeartbeat { get; set; } = DateTime.UtcNow;
    public DateTime? CikisZamani { get; set; }
    public string? IpAdresi { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
    public Firma Firma { get; set; } = null!;
}
