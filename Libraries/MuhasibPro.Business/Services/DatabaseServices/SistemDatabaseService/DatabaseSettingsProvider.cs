using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService
{
    /// <summary>M4 SystemDb ayar sağlayıcısı (kullanıcı-bazlı; Entity/Tenant deseni).
    /// Tek cümle: DatabaseSettingsModel'i oku/kaydet + clamp + yetki uygular.</summary>
    public class DatabaseSettingsProvider : IDatabaseSettingsProvider
    {
        private static readonly string[] JournalModlari = ["DELETE", "TRUNCATE", "PERSIST", "MEMORY", "WAL", "OFF"];
        private static readonly string[] SynchronousModlari = ["OFF", "NORMAL", "FULL", "EXTRA"];

        private readonly ILocalSettingsService _localSettings;
        private readonly IAuthenticationService _auth;
        private readonly IFirmaKullaniciCozucu _cozucu;

        public DatabaseSettingsProvider(
            ILocalSettingsService localSettings,
            IAuthenticationService auth = null!,
            IFirmaKullaniciCozucu cozucu = null!)
        {
            _localSettings = localSettings;
            _auth = auth;
            _cozucu = cozucu;
        }

        public async Task<DatabaseSettingsModel> GetAsync(long firmaId = 0)
        {
            try
            {
                long kullaniciId = await KullaniciIdAsync(firmaId);
                if (kullaniciId > 0)
                {
                    var ozel = await _localSettings.ReadSettingAsync<DatabaseSettingsModel>(
                        KullaniciAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, kullaniciId));
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
                    var eski = await _localSettings.ReadSettingAsync<DatabaseSettingsModel>(
                        FirmaAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, firmaId));
                    if (eski != null)
                        return Clamp(eski);
                }
                var stored = await _localSettings.ReadSettingAsync<DatabaseSettingsModel>(DatabaseSettingsModel.SettingsKey);
                return Clamp(stored ?? new DatabaseSettingsModel());
            }
            catch
            {
                return new DatabaseSettingsModel();
            }
        }

        public async Task SaveAsync(DatabaseSettingsModel settings, long firmaId = 0)
        {
            var clamped = Clamp(settings ?? new DatabaseSettingsModel());
            var kayitli = await GetAsync(firmaId);
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(clamped, kayitli, _auth);
            long giris = _cozucu != null ? _cozucu.GirisYapanId() : 0;
            await _localSettings.SaveSettingAsync(
                KullaniciAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, giris), clamped);
        }

        /// <summary>Eski firma anahtarındaki kaydı kullanıcının anahtarına taşır (tek seferlik, best-effort).</summary>
        private async Task<DatabaseSettingsModel> TasiAsync(long firmaId, long kullaniciId)
        {
            try
            {
                var eski = await _localSettings.ReadSettingAsync<DatabaseSettingsModel>(
                    FirmaAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, firmaId));
                if (eski == null)
                    return null;
                await _localSettings.SaveSettingAsync(
                    KullaniciAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, kullaniciId), eski);
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

        internal static DatabaseSettingsModel Clamp(DatabaseSettingsModel s)
        {
            if (s.MaxManuelYedekSayisi < 1 || s.MaxManuelYedekSayisi > 20)
                s.MaxManuelYedekSayisi = 5;
            if (s.SistemKeepLast < 1 || s.SistemKeepLast > 20)
                s.SistemKeepLast = 3;
            if (s.BusyTimeoutMs < 1000 || s.BusyTimeoutMs > 30000)
                s.BusyTimeoutMs = 5000;
            if (s.WeeklyBackupDays < 1 || s.WeeklyBackupDays > 30)
                s.WeeklyBackupDays = 7;
            var journal = (s.JournalMode ?? string.Empty).Trim().ToUpperInvariant();
            s.JournalMode = Array.IndexOf(JournalModlari, journal) >= 0 ? journal : "WAL";
            var sync = (s.Synchronous ?? string.Empty).Trim().ToUpperInvariant();
            s.Synchronous = Array.IndexOf(SynchronousModlari, sync) >= 0 ? sync : "NORMAL";
            return s;
        }
    }
}
