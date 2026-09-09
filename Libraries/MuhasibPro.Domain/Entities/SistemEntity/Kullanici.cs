using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasibPro.Domain.Entities.SistemEntity;

[Table("Kullanicilar")]
public class Kullanici : BaseEntity
{
    [MaxLength(50)]
    [Required]
    public string KullaniciAdi { get; set; } = string.Empty;

    [MaxLength(400)]
    [Required]
    public string ParolaHash { get; set; } = string.Empty;

    [MaxLength(50)]
    [Required]
    public string Adi { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Soyadi { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Eposta { get; set; } = string.Empty;

    [MaxLength(17)]
    public string Telefon { get; set; } = string.Empty;

    public byte[]? Resim { get; set; }

    public byte[]? ResimOnizleme { get; set; }

    public ICollection<KullaniciFirmaRol> KullaniciFirmaRoller { get; set; } = new List<KullaniciFirmaRol>();
}
