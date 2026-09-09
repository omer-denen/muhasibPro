using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Enum.DatabaseEnum;

namespace MuhasibPro.Domain.Entities.SistemEntity;

[Table("MaliDonemler")]
public class MaliDonem : BaseEntity
{
    [Required]
    public long FirmaId { get; set; }

    [Required]
    public int MaliYil { get; set; }

    [Required]
    public string DatabaseName { get; set; } = string.Empty;

    public DatabaseType DatabaseType { get; set; }

    public DonemDurum Durum { get; set; } = DonemDurum.Acik;

    public long? DosyaBoyutu { get; set; }

    public DateTime? SonYedekTarihi { get; set; }

    public bool ArsivlendiMi { get; set; } = false;

    public Firma Firma { get; set; } = null!;

    public string BuildSearchTerms()
    {
        return $"{Id} {FirmaId} {MaliYil}".ToLower();
    }
}
