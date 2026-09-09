using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasibPro.Domain.Entities.SistemEntity
{
    [Table("KullaniciRoller")]
    public class KullaniciRol : BaseEntity
    {
        public string RolAdi { get; set; } = string.Empty;

        public string Aciklama { get; set; } = string.Empty;

        public KullaniciRolTip RolTip { get; set; }

        public ICollection<RolPermission> Yetkiler { get; set; } = new List<RolPermission>();
    }
    public enum KullaniciRolTip
    {
        Yönetici = 1,
        Kullanici = 2,
    }
}
