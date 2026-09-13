using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices
{
    /// <summary>M3 kayıt ayar sağlayıcısı — kod deseni + varsayılan durum + sayfa boyutları tek kaynaktan.
    /// firmaId &gt; 0 ise firma anahtarı (<c>SettingsKey:F{id}</c>), yoksa global şablon okunur/yazılır.</summary>
    public interface IEntityRegistrySettingsProvider
    {
        Task<EntityRegistrySettings> GetAsync(long firmaId = 0);
        Task SaveAsync(EntityRegistrySettings settings, long firmaId = 0);
    }
}
