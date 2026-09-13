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
        private readonly IFirmaKullaniciCozucu _cozucu;

        public EntityRegistrySettingsProvider(
            ILocalSettingsService localSettings,
            IAuthenticationService auth = null!,
            IFirmaKullaniciCozucu cozucu = null!)
        {
            _localSettings = localSettings;
            _auth = auth;
            _cozucu = cozucu;
        }

        public async Task<EntityRegistrySettings> GetAsync(long firmaId = 0)
        {
            try
            {
                long kullaniciId = await KullaniciIdAsync(firmaId);
                if (kullaniciId > 0)
                {
                    var ozel = await _localSettings.ReadSettingAsync<EntityRegistrySettings>(
                        KullaniciAyarAnahtari.KeyFor(EntityRegistrySettings.SettingsKey, kullaniciId));
                    if (ozel != null)
                        return Clamp(ozel);
                    if (firmaId > 0 && _cozucu != null)
                    {
                        var tasinan = await TasiAsync(firmaId, kullaniciId);
                        if (tasinan != null)
                            return Clamp(tasinan);
                    }
                }
                else if (firmaId > 0)
                {
                    var eski = await _localSettings.ReadSettingAsync<EntityRegistrySettings>(
                        FirmaAyarAnahtari.KeyFor(EntityRegistrySettings.SettingsKey, firmaId));
                    if (eski != null)
                        return Clamp(eski);
                }
                var stored = await _localSettings.ReadSettingAsync<EntityRegistrySettings>(EntityRegistrySettings.SettingsKey);
                return Clamp(stored ?? new EntityRegistrySettings());
            }
            catch
            {
                return new EntityRegistrySettings();
            }
        }

        public async Task SaveAsync(EntityRegistrySettings settings, long firmaId = 0)
        {
            var clamped = Clamp(settings ?? new EntityRegistrySettings());
            var kayitli = await GetAsync(firmaId);
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            long giris = _cozucu != null ? _cozucu.GirisYapanId() : 0;
            await _localSettings.SaveSettingAsync(
                KullaniciAyarAnahtari.KeyFor(EntityRegistrySettings.SettingsKey, giris), clamped);
        }

        /// <summary>Eski firma anahtarındaki kaydı kullanıcının anahtarına taşır (tek seferlik, best-effort).</summary>
        private async Task<EntityRegistrySettings> TasiAsync(long firmaId, long kullaniciId)
        {
            try
            {
                var eski = await _localSettings.ReadSettingAsync<EntityRegistrySettings>(
                    FirmaAyarAnahtari.KeyFor(EntityRegistrySettings.SettingsKey, firmaId));
                if (eski == null)
                    return null;
                await _localSettings.SaveSettingAsync(
                    KullaniciAyarAnahtari.KeyFor(EntityRegistrySettings.SettingsKey, kullaniciId), eski);
                return eski;
            }
            catch
            {
                return null;
            }
        }

        private async Task<long> KullaniciIdAsync(long firmaId)
        {
            try
            {
                if (_cozucu == null)
                    return 0;
                if (firmaId > 0)
                    return await _cozucu.CozAsync(firmaId);
                return _cozucu.GirisYapanId();
            }
            catch
            {
                return 0;
            }
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
