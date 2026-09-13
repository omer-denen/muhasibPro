using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
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

        public SplashRoutingService(
            ISistemDatabaseService sistemDatabaseService,
            IKurulumKayitService kurulumKayitService,
            IMakineKimligiProvider makineProvider,
            ITenantSQLiteDatabaseService tenantService,
            ITenantVersionReader versionReader,
            IEventBus eventBus)
        {
            _sistemDatabaseService = sistemDatabaseService;
            _kurulumKayitService = kurulumKayitService;
            _makineProvider = makineProvider;
            _tenantService = tenantService;
            _versionReader = versionReader;
            _eventBus = eventBus;
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

                return new SplashRouteDecision
                {
                    IsDatabaseExists = exists,
                    IsDatabaseReady = ready,
                    HasPendingMigrations = hasPending,
                    PendingMigrationCount = state.PendingMigrations?.Count ?? 0
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
                var lines = mismatches.Select(m => FormatMismatch(m, currentKurulum)).ToList();

                var result = new TransferCheckResult
                {
                    CurrentKurulumId = currentKurulum,
                    CurrentMachineId = currentMachine,
                    Mismatches = lines
                };

                if (result.HasMismatches)
                    _eventBus.Publish(this, new TransferDetectedEvent(currentKurulum, currentMachine, lines));

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
