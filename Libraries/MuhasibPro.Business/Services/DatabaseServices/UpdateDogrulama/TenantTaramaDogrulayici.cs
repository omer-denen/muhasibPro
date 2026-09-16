using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Business.Services.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D: güncelleme sonrası dönem (tenant) taraması.
    /// Bozuk → son doğrulanmış yedekten restore + verify; bekleyen → yalnız rapor (erişimdeki göç saga'sı uygular);
    /// gelecek şema → dokunma + rapor (fail-closed).</summary>
    public class TenantTaramaDogrulayici : ITenantTaramaDogrulayici
    {
        private readonly IMaliDonemService _maliDonemService;
        private readonly ITenantSQLiteDatabaseLifecycleService _tenantLifecycle;
        private readonly ITenantSQLiteDatabaseOperationService _tenantOperation;
        private readonly IApplicationPaths _yollar;
        private readonly ISistemLogService _logService;

        public TenantTaramaDogrulayici(
            IMaliDonemService maliDonemService,
            ITenantSQLiteDatabaseLifecycleService tenantLifecycle,
            ITenantSQLiteDatabaseOperationService tenantOperation,
            IApplicationPaths yollar,
            ISistemLogService logService)
        {
            _maliDonemService = maliDonemService;
            _tenantLifecycle = tenantLifecycle;
            _tenantOperation = tenantOperation;
            _yollar = yollar;
            _logService = logService;
        }

        public async Task<TenantTaramaSonucu> TaraAsync(IProgress<double>? ilerleme = null, CancellationToken cancellationToken = default)
        {
            var sonuc = new TenantTaramaSonucu();

            List<string> donemAdlari;
            try
            {
                var resp = await _maliDonemService.GetMaliDonemlerPageAsync(
                    0, int.MaxValue, new DataRequest<MaliDonem> { OrderBy = m => m.MaliYil });
                donemAdlari = resp?.Data?
                    .Select(d => d.DatabaseName)
                    .Where(ad => !string.IsNullOrWhiteSpace(ad) && GuvenliDosyaVarMi(ad))
                    .Select(ad => ad!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                sonuc.Taranamadi = true;
                sonuc.Mesaj = $"Dönem listesi okunamadı: {ex.Message}";
                ilerleme?.Report(100);
                return sonuc;
            }

            if (donemAdlari.Count == 0)
            {
                sonuc.Mesaj = "Doğrulanacak dönem veritabanı yok.";
                ilerleme?.Report(100);
                return sonuc;
            }

            int islenen = 0;
            foreach (var ad in donemAdlari)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await DegerlendirAsync(ad, sonuc);
                islenen++;
                ilerleme?.Report((double)islenen / donemAdlari.Count * 100);
            }

            sonuc.Mesaj = OzetMetni(sonuc);
            return sonuc;
        }

        private async Task DegerlendirAsync(string dbAd, TenantTaramaSonucu sonuc)
        {
            sonuc.Taranan++;

            var state = await DurumAsync(dbAd);
            if (state == null)
            {
                sonuc.Bozuk++;
                sonuc.RaporSatirlari.Add($"{dbAd} • durum okunamadı");
                return;
            }

            if (state.IsFutureSchema)
            {
                sonuc.GelecekSema++;
                sonuc.RaporSatirlari.Add($"{dbAd} • daha yeni sürümle oluşturulmuş ({state.CurrentVersion}) — uygulamayı güncelleyin");
                return;
            }

            if (state.HasError || !state.CanConnect || !state.DatabaseValid)
            {
                bool kurtarildi = await KurtarAsync(dbAd);
                if (kurtarildi)
                {
                    sonuc.Kurtarilan++;
                    sonuc.RaporSatirlari.Add($"{dbAd} • bozuktu, son yedekten geri alındı");
                }
                else
                {
                    sonuc.Bozuk++;
                    sonuc.RaporSatirlari.Add($"{dbAd} • bozuk, kurtarılamadı (yedek yok/başarısız)");
                }
                return;
            }

            if (state.IsUpdateRequired)
            {
                sonuc.Bekleyen++;
                sonuc.RaporSatirlari.Add($"{dbAd} • güncelleme bekliyor ({state.PendingMigrations.Count} göç) — döneme girişte uygulanacak");
            }
        }

        private async Task<bool> KurtarAsync(string dbAd)
        {
            try
            {
                var restore = await _tenantOperation.RestoreFromLatestBackupAsync(dbAd);
                if (restore?.Data != true)
                    return false;

                var verify = await DurumAsync(dbAd);
                return verify != null
                       && verify.IsDatabaseExists
                       && verify.CanConnect
                       && !verify.HasError
                       && verify.DatabaseValid;
            }
            catch (Exception ex)
            {
                await _logService.SistemLogExceptionAsync("Güncelleme Sonrası Doğrulama", $"Dönem kurtarma: {dbAd}", ex);
                return false;
            }
        }

        private async Task<DatabaseConnectionAnalysis?> DurumAsync(string dbAd)
        {
            try
            {
                var resp = await _tenantLifecycle.GetTenantDatabaseStateAsync(dbAd);
                return resp?.Data;
            }
            catch (Exception ex)
            {
                await _logService.SistemLogExceptionAsync("Güncelleme Sonrası Doğrulama", $"Dönem durumu: {dbAd}", ex);
                return null;
            }
        }

        private bool GuvenliDosyaVarMi(string dbAd)
        {
            try { return _yollar.TenantDatabaseFileExists(dbAd); }
            catch { return false; }
        }

        private static string OzetMetni(TenantTaramaSonucu s)
        {
            var parcalar = new List<string> { $"{s.Taranan} dönem tarandı" };
            if (s.Kurtarilan > 0) parcalar.Add($"{s.Kurtarilan} kurtarıldı");
            if (s.Bekleyen > 0) parcalar.Add($"{s.Bekleyen} güncelleme bekliyor");
            if (s.GelecekSema > 0) parcalar.Add($"{s.GelecekSema} daha yeni sürüm");
            if (s.Bozuk > 0) parcalar.Add($"{s.Bozuk} bozuk");
            return string.Join(" • ", parcalar);
        }
    }
}
