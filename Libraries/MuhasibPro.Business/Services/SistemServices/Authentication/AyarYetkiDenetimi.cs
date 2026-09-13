using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;
using System.Reflection;

namespace MuhasibPro.Business.Services.SistemServices.Authentication
{
    /// <summary>
    /// Ayar yazma yetkisi — kritik ([YoneticiAyari]) değer değiştiyse yönetici şart.
    /// </summary>
    public static class AyarYetkiDenetimi
    {
        /// <summary>Seed yönetici kayıtsız-yönetici sayılır (bootstrap fallback — rol satırı
        /// firma-bağımlı olduğu ve seed'de KFR üretilmediği için; Oturum 129/130 presedanı).</summary>
        public static bool KullaniciYoneticiMi(IAuthenticationService? auth)
        {
            if (auth == null || !auth.IsAuthenticated)
                return false;
            try
            {
                if (auth.GetCurrentUserId == KullaniciSabitleri.SeedYoneticiId)
                    return true;
            }
            catch { /* rol kontrolüne düş */ }
            return auth.CurrentAccount?.KullaniciModel?.Rol?.RolTip == KullaniciRolTip.Yönetici;
        }

        public static void KritikDegisiklikleriDogrula<T>(T gelen, T kayitli, IAuthenticationService? auth) where T : class
        {
            if (gelen == null || kayitli == null)
                return;
            if (KullaniciYoneticiMi(auth))
                return;

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute<YoneticiAyariAttribute>() == null)
                    continue;
                var yeni = prop.GetValue(gelen);
                var eski = prop.GetValue(kayitli);
                if (!Equals(yeni, eski))
                    throw new UnauthorizedAccessException(
                        $"'{prop.Name}' kritik bir ayardır, yalnızca yönetici değiştirebilir.");
            }
        }
    }
}
