using Microsoft.EntityFrameworkCore;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Stok;

namespace MuhasibPro.Data.DataContext.SeedData;

public static class SeedDataStok
{
    public static void StokBirim(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StokBirimler>()
            .HasData(
                new StokBirimler
                {
                    Id = 1,
                    BirimAdi = "Mt",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 2,
                    BirimAdi = "M2",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 3,
                    BirimAdi = "M3",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 4,
                    BirimAdi = "MTül",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 5,
                    BirimAdi = "Adet",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 6,
                    BirimAdi = "Paket",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 7,
                    BirimAdi = "Top",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 8,
                    BirimAdi = "Gram",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 9,
                    BirimAdi = "Litre",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokBirimler
                {
                    Id = 10,
                    BirimAdi = "Kg",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                });
    }
    public static void StokGrup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StokGruplar>()
            .HasData(
                new StokGruplar
                {
                    Id = 1,
                    GrupAdi = "Genel",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokGruplar
                {
                    Id = 2,
                    GrupAdi = "A Grubu",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                },
                new StokGruplar
                {
                    Id = 3,
                    GrupAdi = "B Grubu",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 3)
                });
    }
}

