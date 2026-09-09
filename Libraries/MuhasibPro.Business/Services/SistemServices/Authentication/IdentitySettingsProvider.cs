using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.SistemServices.Authentication
{
    public class IdentitySettingsProvider : IIdentitySettingsProvider
    {
        private readonly ILocalSettingsService _localSettings;
        private readonly IAuthenticationService _auth;

        public IdentitySettingsProvider(ILocalSettingsService localSettings, IAuthenticationService auth = null!)
        {
            _localSettings = localSettings;
            _auth = auth;
        }

        public async Task<IdentitySettings> GetAsync()
        {
            try
            {
                var stored = await _localSettings.ReadSettingAsync<IdentitySettings>(IdentitySettings.SettingsKey);
                return Clamp(stored ?? new IdentitySettings());
            }
            catch
            {
                return new IdentitySettings();
            }
        }

        public async Task SaveAsync(IdentitySettings settings)
        {
            var clamped = Clamp(settings ?? new IdentitySettings());
            var kayitli = await GetAsync();
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            await _localSettings.SaveSettingAsync(IdentitySettings.SettingsKey, clamped);
        }

        internal static IdentitySettings Clamp(IdentitySettings s)
        {
            if (s.MaxFailedAttempts < 1 || s.MaxFailedAttempts > 20)
                s.MaxFailedAttempts = 5;
            if (s.LockoutMinutes < 1 || s.LockoutMinutes > 120)
                s.LockoutMinutes = 5;
            if (s.AttemptWindowMinutes < 1 || s.AttemptWindowMinutes > 120)
                s.AttemptWindowMinutes = 5;
            if (s.Pbkdf2Iterations < 10000 || s.Pbkdf2Iterations > 1000000)
                s.Pbkdf2Iterations = 100000;
            if (s.MinPasswordLength < 4 || s.MinPasswordLength > 32)
                s.MinPasswordLength = 6;
            return s;
        }
    }
}
