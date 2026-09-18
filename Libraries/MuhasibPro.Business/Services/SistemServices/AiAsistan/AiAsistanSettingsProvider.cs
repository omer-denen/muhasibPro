using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.SistemServices.AiAsistan
{
    /// <summary>Faz 6.92: AI ayar sağlayıcı — LicenseSettings emsali (makine geneli anahtar) + ayar-değişim olayı.</summary>
    public class AiAsistanSettingsProvider : IAiAsistanSettingsProvider
    {
        private readonly ILocalSettingsService _localSettings;
        private readonly IEventBus _eventBus;
        private readonly IAuthenticationService _auth;

        public AiAsistanSettingsProvider(
            ILocalSettingsService localSettings,
            IEventBus eventBus,
            IAuthenticationService auth = null!)
        {
            _localSettings = localSettings;
            _eventBus = eventBus;
            _auth = auth;
        }

        public async Task<AiAsistanSettings> GetAsync()
        {
            try
            {
                var stored = await _localSettings.ReadSettingAsync<AiAsistanSettings>(AiAsistanSettings.SettingsKey);
                return Clamp(stored ?? new AiAsistanSettings());
            }
            catch
            {
                return new AiAsistanSettings();
            }
        }

        public async Task SaveAsync(AiAsistanSettings settings)
        {
            var clamped = Clamp(settings ?? new AiAsistanSettings());
            var kayitli = await GetAsync();
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            await _localSettings.SaveSettingAsync(AiAsistanSettings.SettingsKey, clamped);
            _eventBus.Publish(this, new AppSettingsChangedEvent(AiAsistanSettings.SettingsKey));
        }

        internal static AiAsistanSettings Clamp(AiAsistanSettings s)
        {
            // S1 tam kilit (Oturum 292): sohbet modeli sabittir — yönetici dahil kimse değiştiremez.
            s.ModelAlias = AiAsistanSettings.VarsayilanModelAlias;
            s.MaksGecmisTur = Math.Clamp(s.MaksGecmisTur, 0, 20);
            s.EnFazlaMadde = Math.Clamp(s.EnFazlaMadde, 1, 12);
            s.SoruZamanAsimiSn = Math.Clamp(s.SoruZamanAsimiSn, 10, 300);
            return s;
        }
    }
}
