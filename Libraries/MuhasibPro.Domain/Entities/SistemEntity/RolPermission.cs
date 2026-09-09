using System.ComponentModel.DataAnnotations.Schema;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Domain.Entities.SistemEntity;

[Table("RolPermission")]
public class RolPermission
{
    public long RolId { get; set; }
    public Permission PermissionId { get; set; }

    public KullaniciRol Rol { get; set; } = null!;
}
