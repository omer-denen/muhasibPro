using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.UIServices
{
    /// <summary>M1 AppPlatform ayar sağlayıcısı — okuma/yazma + aralık doğrulama (clamp) tek kaynaktan.</summary>
    public interface IAppPlatformSettingsProvider
    {
        Task<AppPlatformSettings> GetAsync();
        Task SaveAsync(AppPlatformSettings settings);
    }
}
