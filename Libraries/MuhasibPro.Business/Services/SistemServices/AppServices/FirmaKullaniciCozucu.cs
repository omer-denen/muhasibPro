using Microsoft.Extensions.DependencyInjection;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;

namespace MuhasibPro.Business.Services.SistemServices.AppServices
{
    /// <summary>Firma kaydından ayar-sahibi kullanıcı çıkarımı.
    /// IFirmaService ctor'dan alınmaz (FirmaKayit→EntityRegistry→Firma döngüsü olur);
    /// Oturum 140 deseniyle construction-dışı tembel çözülür.</summary>
    public class FirmaKullaniciCozucu : IFirmaKullaniciCozucu
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthenticationService _auth;

        public FirmaKullaniciCozucu(IServiceProvider serviceProvider, IAuthenticationService auth = null!)
        {
            _serviceProvider = serviceProvider;
            _auth = auth;
        }

        public async Task<long> CozAsync(long firmaId)
        {
            try
            {
                if (firmaId <= 0 || _serviceProvider == null)
                    return 0;
                var firmaService = _serviceProvider.GetService<IFirmaService>();
                if (firmaService == null)
                    return 0;
                var sonuc = await firmaService.GetByFirmaIdAsync(firmaId);
                long kaydedenId = sonuc?.Data?.KaydedenId ?? 0;
                return kaydedenId > 0 ? kaydedenId : 0;
            }
            catch
            {
                return 0;
            }
        }

        public long GirisYapanId()
        {
            try
            {
                if (_auth != null && _auth.IsAuthenticated)
                {
                    long id = _auth.CurrentAccount?.KullaniciId ?? 0;
                    if (id > 0)
                        return id;
                }
            }
            catch { /* girişsize düş */ }
            return 0;
        }
    }
}
