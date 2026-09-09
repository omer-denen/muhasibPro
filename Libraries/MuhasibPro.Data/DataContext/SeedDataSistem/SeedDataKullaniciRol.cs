using Microsoft.EntityFrameworkCore;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.DataContext.SeedDataSistem
{
    public static class SeedDataKullaniciRol
    {
        public static void SeedKullaniciRoller(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KullaniciRol>().HasData(new KullaniciRol
            {
                Id = 241341,
                RolAdi = "Yönetici",
                Aciklama = "Sistemi yönetme yetkisine sahip kullanıcı rolü",
                RolTip = KullaniciRolTip.Yönetici,
                KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                KayitTarihi = new DateTime(2025, 3, 12),
                AktifMi = true

            },
            new KullaniciRol
            {
                Id = 241342,
                RolAdi = "Kullanıcı",
                Aciklama = "Sistemi sınırlı şekilde kullanma yetkisine sahip kullanıcı rolü",
                RolTip = KullaniciRolTip.Kullanici,
                KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                KayitTarihi = new DateTime(2025, 3, 12),
                AktifMi= true
            });
            
        }
    }
}
