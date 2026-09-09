using Microsoft.EntityFrameworkCore;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace MuhasibPro.Data.DataContext.SeedData;

public static class SeedDatailler
{
    public static void IlListesi(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Iller>()
            .HasData(
                new Iller { Id = 1, IlAdi = "Adana", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller
                {
                    Id = 2,
                    IlAdi = "Adıyaman",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller
                {
                    Id = 3,
                    IlAdi = "Afyonkarahisar",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller { Id = 4, IlAdi = "Ağrı", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 5, IlAdi = "Amasya", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 6, IlAdi = "Ankara", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller
                {
                    Id = 7,
                    IlAdi = "Antalya",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller { Id = 8, IlAdi = "Artvin", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 9, IlAdi = "Aydın", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller
                {
                    Id = 10,
                    IlAdi = "Balıkesir",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller
                {
                    Id = 11,
                    IlAdi = "Bilecik",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller
                {
                    Id = 12,
                    IlAdi = "Bingöl",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller
                {
                    Id = 13,
                    IlAdi = "Bitlis",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller { Id = 14, IlAdi = "Bolu", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller
                {
                    Id = 15,
                    IlAdi = "Burdur",
                    KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller { Id = 16, IlAdi = "Bursa", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 17, IlAdi = "Çanakkale", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 18, IlAdi = "Çankırı", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 19, IlAdi = "Çorum", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 20, IlAdi = "Denizli", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 21, IlAdi = "Diyarbakır", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 22, IlAdi = "Edirne", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 23, IlAdi = "Elazığ", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 24, IlAdi = "Erzincan", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 25, IlAdi = "Erzurum", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 26, IlAdi = "Eskişehir", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 27, IlAdi = "Gaziantep", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 28, IlAdi = "Giresun", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 29, IlAdi = "Gümüşhane", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 30, IlAdi = "Hakkari", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 31, IlAdi = "Hatay", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 32, IlAdi = "Isparta", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 33, IlAdi = "Mersin", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 34, IlAdi = "İstanbul", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 35, IlAdi = "İzmir", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 36, IlAdi = "Kars", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 37, IlAdi = "Kastamonu", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 38, IlAdi = "Kayseri", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 39, IlAdi = "Kırklareli", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 40, IlAdi = "Kırşehir", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 41, IlAdi = "Kocaeli", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 42, IlAdi = "Konya", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 43, IlAdi = "Kütahya", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 44, IlAdi = "Malatya", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 45, IlAdi = "Manisa", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 46, IlAdi = "Kahramanmaraş", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 47, IlAdi = "Mardin", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 48, IlAdi = "Muğla", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 49, IlAdi = "Muş", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 50, IlAdi = "Nevşehir", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 51, IlAdi = "Niğde", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 52, IlAdi = "Ordu", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 53, IlAdi = "Rize", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 54, IlAdi = "Sakarya", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 55, IlAdi = "Samsun", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 56, IlAdi = "Siirt", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 57, IlAdi = "Sinop", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 58, IlAdi = "Sivas", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 59, IlAdi = "Tekirdağ", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 60, IlAdi = "Tokat", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 61, IlAdi = "Trabzon", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 62, IlAdi = "Tunceli", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 63, IlAdi = "Şanlıurfa", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 64, IlAdi = "Uşak", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 65, IlAdi = "Van", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 66, IlAdi = "Yozgat", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 67, IlAdi = "Zonguldak", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 68, IlAdi = "Aksaray", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 69, IlAdi = "Bayburt", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 70, IlAdi = "Karaman", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 71, IlAdi = "Kırıkkale", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 72, IlAdi = "Batman", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 73, IlAdi = "Şırnak", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 74, IlAdi = "Bartın", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 75, IlAdi = "Ardahan", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 76, IlAdi = "Iğdır", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 77, IlAdi = "Yalova", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 78, IlAdi = "Karabük", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 79, IlAdi = "Kilis", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 80, IlAdi = "Osmaniye", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 81, IlAdi = "Düzce", KaydedenId = KullaniciSabitleri.SeedYoneticiId, KayitTarihi = new DateTime(2025, 3, 2) });
    }
}
