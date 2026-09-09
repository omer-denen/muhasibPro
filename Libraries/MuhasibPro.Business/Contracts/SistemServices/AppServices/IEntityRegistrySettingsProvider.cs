using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices
{
    /// <summary>M3 kayıt ayar sağlayıcısı — kod deseni + varsayılan durum + sayfa boyutları tek kaynaktan.</summary>
    public interface IEntityRegistrySettingsProvider
    {
        Task<EntityRegistrySettings> GetAsync();
        Task SaveAsync(EntityRegistrySettings settings);
    }
}
