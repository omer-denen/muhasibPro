using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService
{
    public class TenantSettingsProvider : ITenantSettingsProvider
    {
        private readonly ILocalSettingsService _localSettings;
        private readonly IAuthenticationService _auth;

        public TenantSettingsProvider(ILocalSettingsService localSettings, IAuthenticationService auth = null!)
        {
            _localSettings = localSettings;
            _auth = auth;
        }

        public async Task<TenantSettings> GetAsync()
        {
            try
            {
                var stored = await _localSettings.ReadSettingAsync<TenantSettings>(TenantSettings.SettingsKey);
                return Clamp(stored ?? new TenantSettings());
            }
            catch
            {
                return new TenantSettings();
            }
        }

        public async Task SaveAsync(TenantSettings settings)
        {
            var clamped = Clamp(settings ?? new TenantSettings());
            var kayitli = await GetAsync();
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            await _localSettings.SaveSettingAsync(TenantSettings.SettingsKey, clamped);
        }

        internal static TenantSettings Clamp(TenantSettings s)
        {
            if (s.YedekPageSize < 1 || s.YedekPageSize > 50)
                s.YedekPageSize = 4;
            if (s.BilinmeyenPageSize < 1 || s.BilinmeyenPageSize > 50)
                s.BilinmeyenPageSize = 4;
            if (s.BackfillSayfaBoyutu < 10 || s.BackfillSayfaBoyutu > 1000)
                s.BackfillSayfaBoyutu = 100;
            if (s.MigrationRetry < 1 || s.MigrationRetry > 5)
                s.MigrationRetry = 2;
            if (s.BakimTimeoutSec < 30 || s.BakimTimeoutSec > 600)
                s.BakimTimeoutSec = 120;
            if (s.CommandTimeoutSec < 5 || s.CommandTimeoutSec > 300)
                s.CommandTimeoutSec = 30;
            if (s.BusyTimeoutMs < 1000 || s.BusyTimeoutMs > 30000)
                s.BusyTimeoutMs = 5000;
            return s;
        }
    }
}
