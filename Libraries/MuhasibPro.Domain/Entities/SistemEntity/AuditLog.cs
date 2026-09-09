using System.ComponentModel.DataAnnotations.Schema;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Domain.Entities.SistemEntity;

[Table("AuditLoglar")]
public class AuditLog : BaseEntity
{
    public long KullaniciId { get; set; }
    public AuditIslemTipi IslemTipi { get; set; }
    public string HedefTablo { get; set; } = string.Empty;
    public string HedefId { get; set; } = string.Empty;
    public string? EskiDeger { get; set; }
    public string? YeniDeger { get; set; }
    public string? Aciklama { get; set; }
    public string? IpAdresi { get; set; }
    public DateTime Tarih { get; set; } = DateTime.UtcNow;

    public Kullanici? Kullanici { get; set; }
}
