using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Data.Database.Common.Helpers;

namespace MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService
{
    /// <summary>
    /// Tenant güncelleme akışının UI'sız adımları. Sürüm/göç metinleri burada üretilir
    /// (eskiden FirmaShellViewModel içindeydi ve Data katmanına uzanıyordu).
    /// </summary>
    public class TenantDatabaseUpdateService : ITenantDatabaseUpdateService
    {
        private readonly ITenantSQLiteDatabaseService _tenantService;
        private readonly IFirmaWithMaliDonemSelectedService _selectedService;
        private readonly MuhasibPro.Data.Contracts.Database.Common.Helpers.ITenantMigrationDescriber _describer;
        private readonly IEventBus _eventBus;

        public TenantDatabaseUpdateService(
            ITenantSQLiteDatabaseService tenantService,
            IFirmaWithMaliDonemSelectedService selectedService,
            MuhasibPro.Data.Contracts.Database.Common.Helpers.ITenantMigrationDescriber describer,
            IEventBus eventBus = null)
        {
            _tenantService = tenantService;
            _selectedService = selectedService;
            _describer = describer;
            _eventBus = eventBus;
        }

        public async Task<TenantUpdateCheckResult> CheckUpdateRequiredAsync(string databaseName)
        {
            var result = new TenantUpdateCheckResult { DatabaseName = databaseName ?? string.Empty };
            try
            {
                var stateResp = await _tenantService.GetTenantDatabaseStateAsync(databaseName);
                var state = stateResp?.Data;
                if (state == null)
                    return result;

                result.CheckSucceeded = true;
                result.CurrentVersion = state.CurrentVersion;
                result.StatusMessage = state.Message;

                // Faz 6.91-C: dönem daha yeni sürümle yazılmışsa güncelleme/göç sunulmaz — fail-closed bilgi.
                if (state.IsFutureSchema)
                {
                    result.CheckSucceeded = false;
                    result.NeedsUpdate = false;
                    result.StatusMessage = state.Message;
                    return result;
                }

                if (state.IsUpdateRequired || !state.DatabaseValid)
                {
                    var pendingIds = state.PendingMigrations ?? new List<string>();
                    result.NeedsUpdate = true;
                    result.PendingCount = pendingIds.Count;
                    result.TargetVersion = pendingIds.Count > 0
                        ? DbSchemaVersions.ForMigration(pendingIds.Last())
                        : state.CurrentVersion ?? "-";
                    result.PendingLines = pendingIds.Count > 0
                        ? pendingIds.Select(id => $"• {id} — {DescribeMigration(id)}").ToList()
                        : new List<string> { "—" };
                    result.PendingSummaries = pendingIds
                        .Select(id => _describer.Describe(id).Summary)
                        .ToList();
                    var described = pendingIds.Select(id => _describer.Describe(id)).ToList();
                    result.Headline = described.Count == 0
                        ? "Şema güncellemesi"
                        : described.Count == 1
                            ? described[0].Headline
                            : described[0].Headline + $" (+{described.Count - 1} göç daha)";
                    result.TableChanges = described.SelectMany(d => d.Tables).ToList();
                    result.UpdateDetailLines = pendingIds
                        .SelectMany(id => _describer.Describe(id).Details)
                        .ToList();
                    result.ConfirmMessage = BuildConfirmMessage(result);
                }
                if (result.NeedsUpdate)
                    _eventBus?.Publish(this, new TenantUpdateAvailableEvent(
                        result.DatabaseName,
                        result.CurrentVersion ?? "-",
                        result.TargetVersion ?? "-"));
                return result;
            }
            catch (Exception ex)
            {
                // C4 fix: hata yutmak yerine result'a yaz (caller görebilsin).
                result.StatusMessage = $"Durum kontrol hatası: {ex.Message}";
                return result;
            }
        }

        public async Task<TenantUpdateSwitchResult> SwitchAndPublishAsync(string databaseName, FirmaModel firma, MaliDonemModel maliDonem)
        {
            // Tenant'a bağlan (yedek-önce-göç içeride) ve uygulama durumunu yayınla.
            var result = await _tenantService.SwitchTenantAsync(databaseName);
            if (result.Success && result.Data != null && result.Data.IsLoaded)
            {
                _selectedService.SelectedFirma = firma;
                _selectedService.SelectedMaliDonem = maliDonem;
                _selectedService.ConnectedTenantDb = result.Data;
                return new TenantUpdateSwitchResult { Success = true, ConnectionMessage = result.Message ?? string.Empty };
            }
            return new TenantUpdateSwitchResult { Success = false, ErrorMessage = result.Message ?? "Bağlantı kurulamadı" };
        }

        public async Task<bool> ValidateAsync(string databaseName)
        {
            try
            {
                var resp = await _tenantService.GetTenantDatabaseStateAsync(databaseName);
                var state = resp?.Data;
                return state != null && state.CanConnect && state.DatabaseValid && !state.IsUpdateRequired;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TenantUpdate] ValidateAsync hatası: {ex.Message}");
                return false;
            }
        }

        public string DescribeMigration(string migrationId)
        {
            if (string.IsNullOrWhiteSpace(migrationId))
                return "Şema Değişikliği";
            return _describer.Describe(migrationId).Summary;
        }

        private static string BuildConfirmMessage(TenantUpdateCheckResult check)
        {
            var pendingAcik = check.PendingCount > 0
                ? string.Join("\n", check.PendingLines)
                : "—";
            var gercekYapi = check.UpdateDetailLines.Count > 0
                ? string.Join("\n", check.UpdateDetailLines.Select(l => $"• {l}")) + "\n"
                : string.Empty;
            return $"Veritabanı '{check.DatabaseName}' için güncelleme gerekli.\n\n" +
                   $"Mevcut sürüm: {check.CurrentVersion ?? "-"}\n" +
                   $"Hedef sürüm: {check.TargetVersion}\n" +
                   $"Bekleyen göç: {check.PendingCount}\n{pendingAcik}\n" +
                   $"Durum: {check.StatusMessage}\n\n" +
                   $"Ne güncellenecek?\n" +
                   gercekYapi +
                   $"• Mevcut dönem verileri, yedekler ve ayarlar korunacak.\n\n" +
                   $"Ne olacak?\n" +
                   $"1) TenantBackups klasörüne VACUUM INTO yedek alınacak.\n" +
                   $"2) Göçler uygulanacak ve sürüm {check.CurrentVersion ?? "-"} → {check.TargetVersion} yükseltilecek.\n" +
                   $"3) Hata oluşursa otomatik yedekten geri alınacak (VACUUM dosyası silinmez).\n" +
                   $"4) Başarıda Mali Dönem Yönetim → Durumu Yenile ile sağlık kontrolü yapılabilir.\n\n" +
                   $"Güncellemek istiyor musunuz?";
        }
    }
}
