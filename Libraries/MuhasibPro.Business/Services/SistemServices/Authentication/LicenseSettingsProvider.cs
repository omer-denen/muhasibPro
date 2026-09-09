using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.SistemServices.Authentication
{
    public class LicenseSettingsProvider : ILicenseSettingsProvider
    {
        private readonly ILocalSettingsService _localSettings;
        private readonly IAuthenticationService _auth;

        public LicenseSettingsProvider(ILocalSettingsService localSettings, IAuthenticationService auth = null!)
        {
            _localSettings = localSettings;
            _auth = auth;
        }

        public async Task<LicenseSettings> GetAsync()
        {
            try
            {
                var stored = await _localSettings.ReadSettingAsync<LicenseSettings>(LicenseSettings.SettingsKey);
                return Clamp(stored ?? new LicenseSettings());
            }
            catch
            {
                return new LicenseSettings();
            }
        }

        public async Task SaveAsync(LicenseSettings settings)
        {
            var clamped = Clamp(settings ?? new LicenseSettings());
            var kayitli = await GetAsync();
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            await _localSettings.SaveSettingAsync(LicenseSettings.SettingsKey, clamped);
        }

        internal static LicenseSettings Clamp(LicenseSettings s)
        {
            if (!Enum.IsDefined(typeof(LisansTuru), s.Tur))
                s.Tur = LisansTuru.Deneme;
            s.CheckIntervalDays = Math.Clamp(s.CheckIntervalDays, 1, 90);
            return s;
        }
    }
}
