using Microsoft.EntityFrameworkCore;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Cari;

namespace MuhasibPro.Data.DataContext.SeedData;

public static class SeedDataCari
{
    public static void CariGruplar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CariGrup>()
            .HasData(
                new CariGrup
                {
                    Id = 1,
                    GrupAdi = "Genel",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new CariGrup
                {
                    Id = 2,
                    GrupAdi = "Tedarikçi",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new CariGrup
                {
                    Id = 3,
                    GrupAdi = "Müşteri",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                });
    }
}
