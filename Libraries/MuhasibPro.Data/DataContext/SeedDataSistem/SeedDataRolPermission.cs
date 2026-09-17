using Microsoft.EntityFrameworkCore;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Data.DataContext.SeedDataSistem
{
    /// <summary>
    /// Faz 6.85 K1: rol → izin matrisi statik seed'i (migration <c>HasData</c>).
    /// Varsayılan setler <see cref="PermissionVarsayilanlari"/> tek kaynağından gelir.
    /// </summary>
    public static class SeedDataRolPermission
    {
        public static void SeedRolPermissionlar(ModelBuilder modelBuilder)
        {
            var satirlar = new List<RolPermission>();

            foreach (var izin in PermissionVarsayilanlari.Yonetici)
                satirlar.Add(new RolPermission { RolId = KullaniciRolSabitleri.YoneticiRolId, PermissionId = izin });

            foreach (var izin in PermissionVarsayilanlari.Kullanici)
                satirlar.Add(new RolPermission { RolId = KullaniciRolSabitleri.KullaniciRolId, PermissionId = izin });

            modelBuilder.Entity<RolPermission>().HasData(satirlar);
        }
    }
}
