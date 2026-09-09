using MuhasibPro.Business.Contracts.SistemServices.Authentication;
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
        public static bool KullaniciYoneticiMi(IAuthenticationService? auth)
            => auth != null
            && auth.IsAuthenticated
            && auth.CurrentAccount?.KullaniciModel?.Rol?.RolTip == KullaniciRolTip.Yönetici;

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
