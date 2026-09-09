using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.SistemServices.Authentication
{
    /// <summary>M2-politika lisans ayar sağlayıcısı — tür + kontrol aralığı tek kaynaktan.</summary>
    public interface ILicenseSettingsProvider
    {
        Task<LicenseSettings> GetAsync();
        Task SaveAsync(LicenseSettings settings);
    }
}
