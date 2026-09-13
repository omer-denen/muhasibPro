using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.UIService
{
    public class AppPlatformSettingsProvider : IAppPlatformSettingsProvider
    {
        private readonly ILocalSettingsService _localSettings;
        private readonly IEventBus _eventBus;
        private readonly IAuthenticationService _auth;

        public AppPlatformSettingsProvider(
            ILocalSettingsService localSettings,
            IEventBus eventBus,
            IAuthenticationService auth = null!)
        {
            _localSettings = localSettings;
            _eventBus = eventBus;
            _auth = auth;
        }

        public async Task<AppPlatformSettings> GetAsync()
        {
            try
            {
                var anahtar = Anahtar();
                var stored = await _localSettings.ReadSettingAsync<AppPlatformSettings>(anahtar);
                if (stored == null && anahtar != AppPlatformSettings.SettingsKey)
                    stored = await _localSettings.ReadSettingAsync<AppPlatformSettings>(AppPlatformSettings.SettingsKey);
                return Clamp(stored ?? new AppPlatformSettings());
            }
            catch
            {
                return new AppPlatformSettings();
            }
        }

        public async Task SaveAsync(AppPlatformSettings settings)
        {
            var clamped = Clamp(settings ?? new AppPlatformSettings());
            var kayitli = await GetAsync();
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            await _localSettings.SaveSettingAsync(Anahtar(), clamped);
            _eventBus.Publish(this, new AppSettingsChangedEvent(AppPlatformSettings.SettingsKey));
        }

        /// <summary>Kullanıcı bazlı anahtar (Denetim Masası kararı): giriş yapmışken
        /// `Anahtar:U{id}`, değilse global anahtar. Olay yine baz anahtarla yayınlanır.</summary>
        internal string Anahtar()
        {
            try
            {
                if (_auth != null && _auth.IsAuthenticated)
                {
                    long id = _auth.CurrentAccount?.KullaniciId ?? 0;
                    if (id > 0)
                        return $"{AppPlatformSettings.SettingsKey}:U{id}";
                }
            }
            catch { /* modelsiz moda düş */ }
            return AppPlatformSettings.SettingsKey;
        }

        internal static AppPlatformSettings Clamp(AppPlatformSettings s)
        {
            if (string.IsNullOrWhiteSpace(s.ThemeDefault)
                || (!s.ThemeDefault.Equals("Light", StringComparison.OrdinalIgnoreCase)
                    && !s.ThemeDefault.Equals("Dark", StringComparison.OrdinalIgnoreCase)
                    && !s.ThemeDefault.Equals("Default", StringComparison.OrdinalIgnoreCase)))
                s.ThemeDefault = "Default";
            if (s.SplashStepDelayMs < 0 || s.SplashStepDelayMs > 5000)
                s.SplashStepDelayMs = 150;
            if (s.StatusAutoHideMs < 1000 || s.StatusAutoHideMs > 30000)
                s.StatusAutoHideMs = 3000;
            if (string.IsNullOrWhiteSpace(s.ApplicationDataFolder))
                s.ApplicationDataFolder = "MuhasibPro/ApplicationData";
            if (string.IsNullOrWhiteSpace(s.LocalSettingsFile))
                s.LocalSettingsFile = "LocalSettings.json";
            return s;
        }
    }
}
