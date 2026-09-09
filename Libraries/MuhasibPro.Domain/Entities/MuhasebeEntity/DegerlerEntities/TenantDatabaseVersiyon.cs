using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasibPro.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    [Table("TenantDatabaseVersiyonlar")]
    public class TenantDatabaseVersiyon
    {
        [Key]
        public string DatabaseName { get; set; } = string.Empty;
        public string CurrentTenantDbVersion { get; set; } = string.Empty;
        public DateTime CurrentTenantDbLastUpdate { get; set; }
        public string? PreviousTenantDbVersiyon { get; set; }

        public long? FirmaId { get; set; }
        public long? MaliDonemId { get; set; }
        public Domain.Entities.SistemEntity.KullaniciRolTip? OlusturanKullaniciId { get; set; }
        public string? MakineId { get; set; }
        public string? KurulumId { get; set; }

        // Geriye uyum — eski kolon adı için alias (EF aynı kolonu kullanır, migration sonrası veri korunur)
        // Not: OlusturanKullaniciId artık yok, yerine OlusturanRol (enum Admin=Yönetici) kullanılıyor.
    }
}

