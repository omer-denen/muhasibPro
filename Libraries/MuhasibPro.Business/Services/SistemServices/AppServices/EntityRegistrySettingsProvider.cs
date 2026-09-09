using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.SistemServices.AppServices
{
    public class EntityRegistrySettingsProvider : IEntityRegistrySettingsProvider
    {
        private readonly ILocalSettingsService _localSettings;
        private readonly IAuthenticationService _auth;

        public EntityRegistrySettingsProvider(ILocalSettingsService localSettings, IAuthenticationService auth = null!)
        {
            _localSettings = localSettings;
            _auth = auth;
        }

        public async Task<EntityRegistrySettings> GetAsync()
        {
            try
            {
                var stored = await _localSettings.ReadSettingAsync<EntityRegistrySettings>(EntityRegistrySettings.SettingsKey);
                return Clamp(stored ?? new EntityRegistrySettings());
            }
            catch
            {
                return new EntityRegistrySettings();
            }
        }

        public async Task SaveAsync(EntityRegistrySettings settings)
        {
            var clamped = Clamp(settings ?? new EntityRegistrySettings());
            var kayitli = await GetAsync();
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            await _localSettings.SaveSettingAsync(EntityRegistrySettings.SettingsKey, clamped);
        }

        internal static EntityRegistrySettings Clamp(EntityRegistrySettings s)
        {
            if (s.AcikPageSize < 1 || s.AcikPageSize > 50)
                s.AcikPageSize = 8;
            if (s.ArsivPageSize < 1 || s.ArsivPageSize > 50)
                s.ArsivPageSize = 5;
            if (string.IsNullOrWhiteSpace(s.FirmaKodPattern))
                s.FirmaKodPattern = "F-XXXX";
            else
                s.FirmaKodPattern = s.FirmaKodPattern.Trim();
            if (!Enum.IsDefined(typeof(DonemDurum), s.DefaultDurum))
                s.DefaultDurum = DonemDurum.Acik;
            return s;
        }
    }
}
