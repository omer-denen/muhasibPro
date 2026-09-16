using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;

namespace MuhasibPro.Business.Services.UIService
{
    /// <summary>
    /// M1 splash yönlendirme — View'daki EF/DbContext sızıntısının UI'sız karşılığı.
    /// Best-effort: fırlatmaz, arızada güvenli varsayılan (Login) döner.
    /// </summary>
    public class SplashRoutingService : ISplashRoutingService
    {
        private readonly ISistemDatabaseService _sistemDatabaseService;
        private readonly IKurulumKayitService _kurulumKayitService;
        private readonly IMakineKimligiProvider _makineProvider;
        private readonly ITenantSQLiteDatabaseService _tenantService;
        private readonly ITenantVersionReader _versionReader;
        private readonly IEventBus _eventBus;
        private readonly IPostUpdateDogrulamaService _postUpdate;

        public SplashRoutingService(
            ISistemDatabaseService sistemDatabaseService,
            IKurulumKayitService kurulumKayitService,
            IMakineKimligiProvider makineProvider,
            ITenantSQLiteDatabaseService tenantService,
            ITenantVersionReader versionReader,
            IEventBus eventBus,
            IPostUpdateDogrulamaService postUpdate)
        {
            _sistemDatabaseService = sistemDatabaseService;
            _kurulumKayitService = kurulumKayitService;
            _makineProvider = makineProvider;
            _tenantService = tenantService;
            _versionReader = versionReader;
            _eventBus = eventBus;
            _postUpdate = postUpdate;
        }

        public async Task<SplashRouteDecision> DecideRouteAsync(bool? startupDbReady)
        {
            try
            {
                var resp = await _sistemDatabaseService.GetSistemDatabaseStateAsync();
                var state = resp.Data;

                if (state == null)
                    return new SplashRouteDecision { IsDatabaseExists = false, IsDatabaseReady = false };

                bool exists = state.IsDatabaseExists;
                bool ready = exists && state.CanConnect && !state.HasError && state.DatabaseValid;
                bool hasPending = state.PendingMigrations?.Count > 0;

                // startupDbReady override'ı sadece ready durumunu ezer, exists ve pending korunur
                if (startupDbReady.HasValue)
                    ready = startupDbReady.Value;

                // Faz 6.91-D: güncelleme sonrası doğrulama (kalıcı damga) — DB hazırlığından önce yönlendirir.
                bool postUpdate = exists && await _postUpdate.GerekliMiAsync();

                return new SplashRouteDecision
                {
                    IsDatabaseExists = exists,
                    IsDatabaseReady = ready,
                    HasPendingMigrations = hasPending,
                    PendingMigrationCount = state.PendingMigrations?.Count ?? 0,
                    PostUpdateGerekli = postUpdate
                };
            }
            catch
            {
                // Fail-closed: DB sorgusu başarısızsa ilk kuruluma yönlendir (en güvenli yol).
                return new SplashRouteDecision { IsDatabaseExists = false, IsDatabaseReady = false };
            }
        }

        public async Task<TransferCheckResult> CheckTransferAsync()
        {
            var empty = new TransferCheckResult();
            try
            {
                var kayit = await _kurulumKayitService.GetOrCreateAsync();
                string currentKurulum = kayit.KurulumId ?? string.Empty;
                string currentMachine = await _makineProvider.GetMachineIdAsync();

                try { await _tenantService.BackfillMissingTenantIdentitiesAsync(); }
                catch { /* best-effort */ }

                var mismatches = await _versionReader.ScanMismatchesAsync(currentKurulum, currentMachine);

                // Ayrım (Kural 7 — karar veriden): makine farklı → gerçek transfer (veri başka makineden);
                // makine aynı → kurulum kimliği yenilenmiş (yeniden kurulum/temizlik) = kimlik kaybı.
                // Kimlik kaybı kullanıcıya sorulmaz, sessizce onarılır; makine farklıysa bildirim değerlidir.
                var gercekTransfer = mismatches.Where(m => m.MakineFarkli).ToList();
                var kimlikKaybi = mismatches.Where(m => !m.MakineFarkli).ToList();

                int aligned = 0;
                string adopted = string.Empty;
                if (kimlikKaybi.Count > 0)
                {
                    var kimlikler = kimlikKaybi
                        .Select(m => m.VersiyonKurulumId ?? string.Empty)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (kimlikler.Count == 1)
                    {
                        // Tüm dönemler tek eski kimlikte: kimlik damgalardan geri alınır —
                        // o kimlikle alınmış eski yedeklerle uyum korunur.
                        await _kurulumKayitService.UpdateKurulumIdAsync(kimlikler[0]);
                        adopted = kimlikler[0];
                        currentKurulum = adopted;
                        aligned = kimlikKaybi.Count;
                    }
                    else if (kimlikler.Count > 1)
                    {
                        // Karışık kimlik: kaynak belirsiz → dönemler güncel kimliğe eşitlenir.
                        aligned = await _tenantService.ReAlignTenantKurulumIdsAsync(
                            kimlikKaybi.Select(m => m.DatabaseName).ToList());
                    }
                }

                var result = new TransferCheckResult
                {
                    CurrentKurulumId = currentKurulum,
                    CurrentMachineId = currentMachine,
                    Mismatches = gercekTransfer.Select(m => FormatMismatch(m, currentKurulum)).ToList(),
                    AlignedCount = aligned,
                    AdoptedKurulumId = adopted
                };

                if (result.HasMismatches)
                    _eventBus.Publish(this, new TransferDetectedEvent(currentKurulum, currentMachine, result.Mismatches));

                return result;
            }
            catch
            {
                return empty;
            }
        }

        public static string FormatMismatch(TenantMismatchInfo m, string currentKurulum)
        {
            string onceki = string.IsNullOrWhiteSpace(m.VersiyonKurulumId) ? "-"
                : m.VersiyonKurulumId.Substring(0, Math.Min(8, m.VersiyonKurulumId.Length));
            string simdiki = string.IsNullOrWhiteSpace(currentKurulum) ? "-"
                : currentKurulum.Substring(0, Math.Min(8, currentKurulum.Length));
            return $"{m.DatabaseName} • Kurulum:{onceki} → {simdiki} • Makine:{(m.MakineFarkli ? "farklı" : "aynı")}";
        }
    }
}
