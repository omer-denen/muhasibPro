using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Seçili dönemin yedekleri: listele / al / geri yükle / sil. Tek sorumluluk: yedek paneli.</summary>
public class DonemYedeklerViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;

    public DonemYedeklerViewModel(
        ICommonServices commonServices,
        ITenantSQLiteDatabaseOperationService operationService,
        ITenantBackupService backupService,
        IMaliDonemService maliDonemService,
        ILocalSettingsService localSettingsService = null,
        IEventBus eventBus = null) : base(commonServices)
    {
        OperationService = operationService;
        BackupService = backupService;
        MaliDonemService = maliDonemService;
        LocalSettingsService = localSettingsService;
        _eventBus = eventBus;
    }

    public ITenantSQLiteDatabaseOperationService OperationService { get; }
    public ITenantBackupService BackupService { get; }
    public IMaliDonemService MaliDonemService { get; }
    public ILocalSettingsService LocalSettingsService { get; }

    /// <summary>Yedeği alınan dönem (sayfa DonemDegistiAsync'te atar — satır yazımı için).</summary>
    public MaliDonemModel BagliDonem { get; set; }

    private string _databaseName;
    private List<DatabaseBackupResult> _yedekler = new();
    public List<DatabaseBackupResult> Yedekler
    {
        get => _yedekler;
        private set
        {
            if (Set(ref _yedekler, value))
            {
                NotifyPropertyChanged(nameof(HasYedek));
                YedekCurrentPage = 1;
                UpdatePagedYedek();
            }
        }
    }

    public bool HasYedek => Yedekler.Count > 0;

    // ── Pagination: Yedekler (boyut TenantSettings'ten) ──
    private int _yedekPageSize = new TenantSettings().GetYedekPageSize();
    public int YedekPageSize
    {
        get => _yedekPageSize;
        private set
        {
            if (Set(ref _yedekPageSize, value))
            {
                NotifyPropertyChanged(nameof(YedekTotalPages));
                NotifyPropertyChanged(nameof(YedekHasPagination));
                UpdatePagedYedek();
            }
        }
    }
    private int _yedekCurrentPage = 1;
    public int YedekCurrentPage
    {
        get => _yedekCurrentPage;
        set
        {
            var clamped = Math.Clamp(value, 1, Math.Max(1, YedekTotalPages));
            if (Set(ref _yedekCurrentPage, clamped))
            {
                UpdatePagedYedek();
                NotifyPropertyChanged(nameof(YedekCanPrev));
                NotifyPropertyChanged(nameof(YedekCanNext));
                NotifyPropertyChanged(nameof(YedekPageInfo));
            }
        }
    }
    public int YedekTotalPages => Math.Max(1, (int)Math.Ceiling((Yedekler?.Count ?? 0) / (double)YedekPageSize));
    public bool YedekHasPagination => (Yedekler?.Count ?? 0) > YedekPageSize;
    public bool YedekCanPrev => YedekCurrentPage > 1;
    public bool YedekCanNext => YedekCurrentPage < YedekTotalPages;
    public string YedekPageInfo => $"{YedekCurrentPage} / {YedekTotalPages}";
    private List<DatabaseBackupResult> _pagedYedekler = new();
    public List<DatabaseBackupResult> PagedYedekler
    {
        get => _pagedYedekler;
        private set => Set(ref _pagedYedekler, value);
    }
    public System.Windows.Input.ICommand YedekPrevCommand => new RelayCommand(() => YedekCurrentPage--, () => YedekCanPrev);
    public System.Windows.Input.ICommand YedekNextCommand => new RelayCommand(() => YedekCurrentPage++, () => YedekCanNext);
    private void UpdatePagedYedek()
    {
        var src = Yedekler ?? new List<DatabaseBackupResult>();
        PagedYedekler = src.Skip((YedekCurrentPage - 1) * YedekPageSize).Take(YedekPageSize).ToList();
        NotifyPropertyChanged(nameof(YedekTotalPages));
        NotifyPropertyChanged(nameof(YedekHasPagination));
        NotifyPropertyChanged(nameof(YedekPageInfo));
    }

    private DatabaseBackupResult _selectedYedek;
    public DatabaseBackupResult SelectedYedek
    {
        get => _selectedYedek;
        set => Set(ref _selectedYedek, value);
    }

    public async Task YukleAsync(string databaseName)
    {
        _databaseName = databaseName;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            Yedekler = new List<DatabaseBackupResult>();
            return;
        }
        try
        {
            if (LocalSettingsService != null)
            {
                var ayar = await LocalSettingsService.ReadSettingAsync<TenantSettings>(TenantSettings.SettingsKey);
                if (ayar != null)
                    YedekPageSize = ayar.GetYedekPageSize();
            }
        }
        catch { /* model varsayılanı korunur */ }
        try
        {
            var response = await OperationService.GetBackupHistoryAsync(databaseName);
            Yedekler = response?.Data ?? new List<DatabaseBackupResult>();
            SelectedYedek = Yedekler.FirstOrDefault();
        }
        catch
        {
            Yedekler = new List<DatabaseBackupResult>();
        }
    }

    public async Task YedekAlAsync()
    {
        if (string.IsNullOrWhiteSpace(_databaseName))
            return;
        try
        {
            var response = await OperationService.CreateBackupAsync(_databaseName, Domain.Enum.DatabaseEnum.DatabaseBackupType.Manual);
            if (response.Success && response.Data != null && response.Data.IsBackupComleted)
            {
                await SatirVitrininiYazAsync(response.Data);
                NotificationService.Show("Yedek Alındı", $"{_databaseName} yedeklendi.", NotificationType.Success);
                // FIFO: fazlalık en eski otomatik silinir (modelden gelen gerçek değer)
                try
                {
                    if (LocalSettingsService != null)
                    {
                        var dbSettings = await LocalSettingsService.ReadSettingAsync<DatabaseSettingsModel>(DatabaseSettingsModel.SettingsKey)
                            ?? new DatabaseSettingsModel();
                        if (dbSettings.OtomatikTemizlemeAcik)
                            await OperationService.CleanOldBackupsAsync(_databaseName, dbSettings.GetManuelKeep());
                    }
                }
                catch { }
            }
            else
                NotificationService.Show("Yedek Alınamadı", response.Message, NotificationType.Warning);
            await YukleAsync(_databaseName);
        }
        catch (Exception ex)
        {
            NotificationService.Show("Yedek Hatası", ex.Message, NotificationType.Danger);
        }
    }

    public async Task GeriYukleAsync()
    {
        var yedek = SelectedYedek;
        if (yedek == null || string.IsNullOrWhiteSpace(_databaseName))
            return;
        try
        {
            bool onay = await DialogService.ShowConfirmationAsync(
                "Yedekten Geri Yükle",
                $"'{yedek.BackupFileName}' yedeği geri yüklenecek. Mevcut dosya üzerine yazılır.\n\nDevam edilsin mi?",
                "Geri Yükle", "Vazgeç");
            if (!onay)
                return;
            var response = await OperationService.RestoreBackupAsync(_databaseName, yedek.BackupFileName);
            if (response.Success && response.Data != null && response.Data.IsRestoreSuccess)
                NotificationService.Show("Geri Yüklendi", $"'{yedek.BackupFileName}' geri yüklendi.", NotificationType.Success);
            else
                NotificationService.Show("Geri Yüklenemedi", response.Message ?? "İşlem başarısız.", NotificationType.Danger);
        }
        catch (Exception ex)
        {
            NotificationService.Show("Geri Yükleme Hatası", ex.Message, NotificationType.Danger);
        }
    }

    /// <summary>Başarılı yedek sonrası Global.db satırına vitrin verisini işler (Son Yedek + Boyut).</summary>
    private async Task SatirVitrininiYazAsync(DatabaseBackupResult sonuc)
    {
        try
        {
            var donem = BagliDonem;
            if (donem == null || MaliDonemService == null)
                return;
            donem.TenantDetails ??= new TenantDetailsModel();
            donem.TenantDetails.SonYedekTarihi = DateTime.Now;
            if (sonuc.BackupFileSizeBytes > 0)
                donem.TenantDetails.DosyaBoyutu = sonuc.BackupFileSizeBytes;
            await MaliDonemService.UpdateMaliDonemAsync(donem);
        }
        catch { /* vitrin yazımı yedek başarısını gölgelemez */ }
    }

    public async Task YedekSilAsync()    {        var yedek = SelectedYedek;
        if (yedek == null)
            return;
        try
        {
            bool onay = await DialogService.ShowConfirmationAsync(
                "Yedeği Sil",
                $"'{yedek.BackupFileName}' kalıcı olarak silinecek.\n\nDevam edilsin mi?",
                "Sil", "Vazgeç");
            if (!onay)
                return;
            await BackupService.CleanupBackupFileAsync(yedek.BackupFilePath);
            NotificationService.Show("Yedek Silindi", $"'{yedek.BackupFileName}' silindi.", NotificationType.Success);
            await YukleAsync(_databaseName);
        }
        catch (Exception ex)
        {
            NotificationService.Show("Silme Hatası", ex.Message, NotificationType.Danger);
        }
    }

    public void Subscribe()
    {
        _eventBus?.Subscribe<TenantBackupCompletedEvent>(this, (s, e) => OnYedekListesiDegisti(e));
        _eventBus?.Subscribe<TenantRestoreCompletedEvent>(this, (s, e) => OnYedekListesiDegisti(e));
    }

    public void Unsubscribe()
    {
        _eventBus?.Unsubscribe(this);
    }

    /// <summary>E2: başka pencerede alınan yedek/geri-yükleme bu panelin listesini tazeler.</summary>
    private async void OnYedekListesiDegisti(DomainEvent e)
    {
        string db = e switch
        {
            TenantBackupCompletedEvent y => y.DatabaseName,
            TenantRestoreCompletedEvent g => g.DatabaseName,
            _ => string.Empty
        };
        if (string.IsNullOrWhiteSpace(db) || string.IsNullOrWhiteSpace(_databaseName))
            return;
        if (!string.Equals(db, _databaseName, StringComparison.OrdinalIgnoreCase))
            return;
        await ContextService.RunAsync(() => { _ = YukleAsync(_databaseName); });
    }
}
