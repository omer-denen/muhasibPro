using Microsoft.EntityFrameworkCore;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Banka;

namespace MuhasibPro.Data.DataContext.SeedData;

public static class SeedDataBanka
{
    public static void Bankalar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankaListesi>()
            .HasData(
                new BankaListesi
                {
                    Id = 1,
                    BankaAdi = "Ziraat Bankası",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 2,
                    BankaAdi = "Vakıfbank",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 3,
                    BankaAdi = "Halkbank",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 4,
                    BankaAdi = "Garanti Bankası",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 5,
                    BankaAdi = "Akbank",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 6,
                    BankaAdi = "İş Bankası",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 7,
                    BankaAdi = "Yapı Kredi",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 8,
                    BankaAdi = "QNB Finansbank",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 9,
                    BankaAdi = "Denizbank",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 10,
                    BankaAdi = "TEB",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                });
    }
}
