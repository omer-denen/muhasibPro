using Microsoft.EntityFrameworkCore;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace MuhasibPro.Data.DataContext.SeedData
{
    public static class SeedDataAppComboBoxData
    {
        public static void HatirlatmaTur(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HatirlatmaTurler>()
                .HasData(
                    new HatirlatmaTurler
                    {
                        Id = 1,
                        TurAdi = "Genel",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new HatirlatmaTurler
                    {
                        Id = 2,
                        TurAdi = "Borç",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new HatirlatmaTurler
                    {
                        Id = 3,
                        TurAdi = "Alacak",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    });
        }
        public static void OdemeSekilleri(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OdemeTurler>()
                .HasData(
                    new OdemeTurler
                    {
                        Id = 1,
                        OdemeTuru = "Nakit",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new OdemeTurler
                    {
                        Id = 2,
                        OdemeTuru = "Havale",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new OdemeTurler
                    {
                        Id = 3,
                        OdemeTuru = "Kredi Kartı",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    });
        }
        public static void ParaBirimi(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParaBirimler>()
                .HasData(
                    new ParaBirimler
                    {
                        Id = 1,
                        Kisaltmasi = "TL",
                        PB = "Türk Lirası",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new ParaBirimler
                    {
                        Id = 2,
                        Kisaltmasi = "USD",
                        PB = "Amerikan Doları",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new ParaBirimler
                    {
                        Id = 3,
                        Kisaltmasi = "EUR",
                        PB = "Euro",
                        KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    });
        }
    }
}
