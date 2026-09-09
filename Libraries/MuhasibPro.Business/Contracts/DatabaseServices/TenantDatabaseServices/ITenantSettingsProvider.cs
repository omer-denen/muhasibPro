using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices
{
    /// <summary>M5 tenant ayar sağlayıcısı — limit + eşik değerleri tek kaynaktan.</summary>
    public interface ITenantSettingsProvider
    {
        Task<TenantSettings> GetAsync();
        Task SaveAsync(TenantSettings settings);
    }
}
