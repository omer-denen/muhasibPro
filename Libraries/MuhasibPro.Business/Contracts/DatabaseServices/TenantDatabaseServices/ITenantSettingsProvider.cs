using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices
{
    /// <summary>M5 tenant ayar sağlayıcısı — limit + eşik değerleri tek kaynaktan.
    /// firmaId &gt; 0 ise firma anahtarı (<c>SettingsKey:F{id}</c>), yoksa global şablon okunur/yazılır.</summary>
    public interface ITenantSettingsProvider
    {
        Task<TenantSettings> GetAsync(long firmaId = 0);
        Task SaveAsync(TenantSettings settings, long firmaId = 0);
    }
}
