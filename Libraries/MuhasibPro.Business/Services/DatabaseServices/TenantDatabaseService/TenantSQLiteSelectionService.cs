using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Data.Database.Extensions;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService
{
    /// <summary>
    /// public class MainViewModel : ObservableObject
    ///{
    ///    public MainViewModel(IMessageService messageService)
    ///    {
    ///        // Tenant değişikliklerini dinle
    ///        messageService.Subscribe<TenantContext>(
    ///            this,
    ///            "TenantChanged",
    ///            OnTenantChanged);
    ///    }

    ///    private void OnTenantChanged(TenantContext tenant)
    ///    {
    ///        // UI thread'de çalıştır (WinUI3)
    ///        _ = DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
    ///        {
    ///            CurrentDatabase = tenant.DatabaseName;
    ///            IsConnected = tenant.IsLoaded;
    ///
    ///            /// İsteğe bağlı: Mesaj göster
    ///            if (!tenant.IsLoaded && !string.IsNullOrEmpty(tenant.Message))
    ///            {
    ///                ShowToast(tenant.Message);
    ///            }
    ///        });
    ///    }

    ///    [ObservableProperty]
    ///    private string _currentDatabase;

    ///    [ObservableProperty]
    ///    private bool _isConnected;
    ///}
    /// </summary>
    public class TenantSQLiteSelectionService : ITenantSQLiteSelectionService
    {
        private readonly ITenantSQLiteSelectionManager _selectionManager;
        private readonly ITenantSQLiteDatabaseManager _databaseManager;
        private readonly ITenantSQLiteConnectionStringFactory _connectionStringFactory;
        private readonly IMessageService _messageService;
        private readonly ILogService _logService;
        private readonly ITenantSettingsProvider _settingsProvider;

        public TenantSQLiteSelectionService(
            ITenantSQLiteSelectionManager selectionManager,
            IMessageService messageService,
            ILogService logService,
            ITenantSQLiteDatabaseManager databaseManager,
            ITenantSQLiteConnectionStringFactory connectionStringFactory,
            ITenantSettingsProvider settingsProvider = null)
        {
            _selectionManager = selectionManager;
            _messageService = messageService;
            _logService = logService;

            // Manager'daki değişiklikleri MessageService ile yayınla
            _selectionManager.TenantChanged += OnManagerTenantChanged;
            _databaseManager = databaseManager;
            _connectionStringFactory = connectionStringFactory;
            _settingsProvider = settingsProvider;
        }

        private void OnManagerTenantChanged(TenantContext tenant)
        {
            // Manager'daki değişikliği MessageService ile UI'ya ilet
            _messageService.Send(this, "TenantChanged", tenant);
        }

        public bool IsTenantLoaded => _selectionManager.IsTenantLoaded;

        public void ClearCurrentTenantAsync()
            => _selectionManager.ClearCurrentTenant();

        public async Task<ApiDataResponse<bool>> DisconnectCurrentTenantAsync()
        {
            try
            {
                if (!IsTenantLoaded)
                {
                    return new ErrorApiDataResponse<bool>(
                        data: false,
                        message: "🟢 Zaten aktif bir bağlantı bulunamadı");
                }

                var eskiAd = CurrentTenant?.DatabaseName ?? string.Empty;
                await EskiTenantıKapatAsync(eskiAd, "Bağlantı kesme");
                ClearCurrentTenantAsync();

                await _logService.SistemLogService.SistemLogInformationAsync(
                    "Mali Dönem Seçimi",
                    "Veritabanı İşlemleri",
                    "Seçili veritabanı bağlantısı kesildi",
                    "Veritabanı bağlantısı kullanıcı tarafından kesildi");

                // Kısa bekle ve kontrol et
                await Task.Delay(100);

                if (IsTenantLoaded)
                {
                    return new ErrorApiDataResponse<bool>(
                        data: false,
                        message: "⚠️ [UYARI] Veritabanı bağlantısı kesilemedi");
                }

                return new SuccessApiDataResponse<bool>(
                    data: true,
                    message: "⛓️‍💥 Aktif veritabanı bağlantısı başarıyla kesildi");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService.SistemLogExceptionAsync(
                    "Mali Dönem Seçimi", "Veritabanı İşlemleri", ex);
                return new ErrorApiDataResponse<bool>(
                    false,
                    message: $"[HATA] Bağlantı kesilemedi : {ex.Message}");
            }
        }

        public TenantContext CurrentTenant => _selectionManager.GetCurrentTenant();

        public async Task<ApiDataResponse<TenantContext>> SwitchTenantAsync(
            string databaseName)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return new ErrorApiDataResponse<TenantContext>(
                    data: null,
                    message: "Veritabanı adı boş olamaz");
            }

            // Aynı tenant kontrolü (case-insensitive) — idempotent: zaten bağlıysa bu bir hata değil,
            // mevcut bağlantı korunur (liste tazeleme / yeniden seçim akışları buradan geçer).
            if (IsTenantLoaded &&
                string.Equals(CurrentTenant.DatabaseName, databaseName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return new SuccessApiDataResponse<TenantContext>(
                    data: CurrentTenant,
                    message: $"Zaten bu mali dönem bağlı: {databaseName}");
            }
            // Eski tenant: WAL birleştir + bağlantıyı bırak + Sistem.db'ye logla.
            // Başarısızlık geçişi engellemez (WAL dosyası diskte durur, veri kaybolmaz) — uyarı loglanır.
            if (IsTenantLoaded && !string.IsNullOrWhiteSpace(CurrentTenant?.DatabaseName))
                await EskiTenantıKapatAsync(CurrentTenant.DatabaseName, "Tenant geçişi");
            // Bağlantı/göç eşikleri kullanıcı ayarından (Oturum 127 derin bağlantı).
            // Sağlayıcı yoksa veya okunamazsa null taşınır → Data varsayılanları korunur.
            var ayar = await BaglantiAyariniOkuAsync();
            var validateConnection = await _connectionStringFactory.ValidateConnectionStringAsync(
                databaseName, busyTimeoutMs: ayar.BusyTimeoutMs, pooling: ayar.Pooling);
            if(!validateConnection.canConnect)
            {
                return new ErrorApiDataResponse<TenantContext>(
                    data: null,
                    message: $"Veritabanı bağlantı dizesi oluşturulamadı: {databaseName}");
            }
            var tenantContext = new TenantContext
            {
                DatabaseName = databaseName,
                DatabaseType = Domain.Enum.DatabaseEnum.DatabaseType.SQLite,
                ConnectionString = validateConnection.connectionString,
                 LoadedAt = DateTime.UtcNow

            };
            var initilizeDatabase = await _databaseManager.InitializeTenantDatabaseAsync(
                databaseName, commandTimeoutSec: ayar.CommandTimeoutSec, retryCount: ayar.RetryCount);
            if(!initilizeDatabase.IsHealthy)
            {
                return ApiDataExtensions.ErrorResponse<TenantContext>(CurrentTenant, initilizeDatabase.ToUIFullMessage());
            }
            try
            {
                var newTenant = _selectionManager.SwitchToTenantAsync(
                    tenantContext);

                // Business logging - sadece başarılıysa
                if (newTenant.IsLoaded)
                {
                    await _logService.SistemLogService.SistemLogInformationAsync(
                        "Mali Dönem Seçimi",
                        "Veritabanı İşlemleri",
                        "Seçili dönem değiştirildi.",
                        $"Yeni dönem: {databaseName}");
                }

                // Response mesajı
                var responseMessage = newTenant.IsLoaded
                    ? $"✅ {databaseName} dönemine geçildi"
                    : newTenant.Message ?? "❌ Bağlantı kurulamadı";

                return newTenant.IsLoaded
                    ? new SuccessApiDataResponse<TenantContext>(newTenant, responseMessage)
                    : new ErrorApiDataResponse<TenantContext>(newTenant, responseMessage);
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                   .SistemLogExceptionAsync("Mali Dönem Seçimi",
                       $"Veritabanı İşlemleri - {databaseName}", ex);
                return new ErrorApiDataResponse<TenantContext>(
                    data: null,
                    message: $"[HATA] İşlem başarısız: {ex.Message}");
            }
        }

        /// <summary>Eski tenant'ı güvenli bırakır: WAL checkpoint + havuz boşaltma (Data),
        /// sonuç Sistem.db'ye yazılır (doğrulama kanıtı). Hata fırlatmaz.</summary>
        private async Task EskiTenantıKapatAsync(string eskiDatabaseName, string neden)
        {
            if (string.IsNullOrWhiteSpace(eskiDatabaseName))
                return;
            try
            {
                var (ok, mesaj) = await _databaseManager.CheckpointAndReleaseAsync(eskiDatabaseName);
                if (ok)
                {
                    await _logService.SistemLogService.SistemLogInformationAsync(
                        "Mali Dönem Seçimi",
                        "Tenant Bağlantı Kapatma",
                        $"{neden}: '{eskiDatabaseName}' güvenli bırakıldı.",
                        mesaj);
                }
                else
                {
                    await _logService.SistemLogService.SistemLogErrorAsync(
                        "Mali Dönem Seçimi",
                        "Tenant Bağlantı Kapatma",
                        $"{neden}: '{eskiDatabaseName}' WAL birleştirilemedi.",
                        mesaj);
                }
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService.SistemLogExceptionAsync(
                    "Mali Dönem Seçimi", "Tenant Bağlantı Kapatma", ex);
            }
        }

        /// <summary>Kullanıcı ayarını okur; sağlayıcı yoksa/okunamazsa hepsi null
        /// (Data varsayılanları korunur — providersız eski kurulumlar kırılmaz).</summary>
        private async Task<(int? BusyTimeoutMs, bool? Pooling, int? CommandTimeoutSec, int? RetryCount)> BaglantiAyariniOkuAsync()
        {
            try
            {
                if (_settingsProvider == null)
                    return (null, null, null, null);
                TenantSettings ayar = await _settingsProvider.GetAsync();
                if (ayar == null)
                    return (null, null, null, null);
                return (ayar.GetBusyTimeoutMs(), ayar.Pooling, ayar.GetCommandTimeoutSec(), ayar.GetMigrationRetry());
            }
            catch
            {
                return (null, null, null, null);
            }
        }

        public void Dispose()
        {
            _selectionManager.TenantChanged -= OnManagerTenantChanged;
        }
    }
}