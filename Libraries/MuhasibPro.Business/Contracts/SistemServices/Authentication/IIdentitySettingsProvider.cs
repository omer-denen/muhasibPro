using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.SistemServices.Authentication
{
    /// <summary>M2 kimlik ayar sağlayıcısı — kilit eşikleri tek kaynaktan.</summary>
    public interface IIdentitySettingsProvider
    {
        Task<IdentitySettings> GetAsync();
        Task SaveAsync(IdentitySettings settings);
    }
}
