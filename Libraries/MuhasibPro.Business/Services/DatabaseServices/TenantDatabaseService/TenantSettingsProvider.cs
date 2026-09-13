using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
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
        private readonly IFirmaKullaniciCozucu _cozucu;

        public TenantSettingsProvider(
            ILocalSettingsService localSettings,
            IAuthenticationService auth = null!,
            IFirmaKullaniciCozucu cozucu = null!)
        {
            _localSettings = localSettings;
            _auth = auth;
            _cozucu = cozucu;
        }

        public async Task<TenantSettings> GetAsync(long firmaId = 0)
        {
            try
            {
                long kullaniciId = await KullaniciIdAsync(firmaId);
                if (kullaniciId > 0)
                {
                    var ozel = await _localSettings.ReadSettingAsync<TenantSettings>(
                        KullaniciAyarAnahtari.KeyFor(TenantSettings.SettingsKey, kullaniciId));
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
                    var eski = await _localSettings.ReadSettingAsync<TenantSettings>(
                        FirmaAyarAnahtari.KeyFor(TenantSettings.SettingsKey, firmaId));
                    if (eski != null)
                        return Clamp(eski);
                }
                var stored = await _localSettings.ReadSettingAsync<TenantSettings>(TenantSettings.SettingsKey);
                return Clamp(stored ?? new TenantSettings());
            }
            catch
            {
                return new TenantSettings();
            }
        }

        public async Task SaveAsync(TenantSettings settings, long firmaId = 0)
        {
            var clamped = Clamp(settings ?? new TenantSettings());
            var kayitli = await GetAsync(firmaId);
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            long giris = _cozucu != null ? _cozucu.GirisYapanId() : 0;
            await _localSettings.SaveSettingAsync(
                KullaniciAyarAnahtari.KeyFor(TenantSettings.SettingsKey, giris), clamped);
        }

        /// <summary>Eski firma anahtarındaki kaydı kullanıcının anahtarına taşır (tek seferlik, best-effort).</summary>
        private async Task<TenantSettings> TasiAsync(long firmaId, long kullaniciId)
        {
            try
            {
                var eski = await _localSettings.ReadSettingAsync<TenantSettings>(
                    FirmaAyarAnahtari.KeyFor(TenantSettings.SettingsKey, firmaId));
                if (eski == null)
                    return null;
                await _localSettings.SaveSettingAsync(
                    KullaniciAyarAnahtari.KeyFor(TenantSettings.SettingsKey, kullaniciId), eski);
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
