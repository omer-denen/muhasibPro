using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Bilinmeyen yedekler (kaydı olmayan .backup dosyaları): tara / temizle. Tek sorumluluk: silinenler paneli.</summary>
public class BilinmeyenYedekViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;

    public BilinmeyenYedekViewModel(
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

    private List<DatabaseBackupResult> _bilinmeyenYedekler = new();
    public List<DatabaseBackupResult> BilinmeyenYedekler
    {
        get => _bilinmeyenYedekler;
        private set
        {
            if (Set(ref _bilinmeyenYedekler, value))
            {
                BilinmeyenCurrentPage = 1;
                UpdatePagedBilinmeyen();
            }
        }
    }

    // ── Pagination: BilinmeyenYedekler (boyut TenantSettings'ten) ──
    private int _bilinmeyenPageSize = new TenantSettings().GetBilinmeyenPageSize();
    public int BilinmeyenPageSize
    {
        get => _bilinmeyenPageSize;
        private set
        {
            if (Set(ref _bilinmeyenPageSize, value))
            {
                NotifyPropertyChanged(nameof(BilinmeyenTotalPages));
                NotifyPropertyChanged(nameof(BilinmeyenHasPagination));
                UpdatePagedBilinmeyen();
            }
        }
    }
    private int _bilinmeyenCurrentPage = 1;
    public int BilinmeyenCurrentPage
    {
        get => _bilinmeyenCurrentPage;
        set
        {
            var clamped = Math.Clamp(value, 1, Math.Max(1, BilinmeyenTotalPages));
            if (Set(ref _bilinmeyenCurrentPage, clamped))
            {
                UpdatePagedBilinmeyen();
                NotifyPropertyChanged(nameof(BilinmeyenCanPrev));
                NotifyPropertyChanged(nameof(BilinmeyenCanNext));
                NotifyPropertyChanged(nameof(BilinmeyenPageInfo));
            }
        }
    }
    public int BilinmeyenTotalPages => Math.Max(1, (int)Math.Ceiling((BilinmeyenYedekler?.Count ?? 0) / (double)BilinmeyenPageSize));
    public bool BilinmeyenHasPagination => (BilinmeyenYedekler?.Count ?? 0) > BilinmeyenPageSize;
    public bool BilinmeyenCanPrev => BilinmeyenCurrentPage > 1;
    public bool BilinmeyenCanNext => BilinmeyenCurrentPage < BilinmeyenTotalPages;
    public string BilinmeyenPageInfo => $"{BilinmeyenCurrentPage} / {BilinmeyenTotalPages}";
    private List<DatabaseBackupResult> _pagedBilinmeyenYedekler = new();
    public List<DatabaseBackupResult> PagedBilinmeyenYedekler
    {
        get => _pagedBilinmeyenYedekler;
        private set => Set(ref _pagedBilinmeyenYedekler, value);
    }
    public System.Windows.Input.ICommand BilinmeyenPrevCommand => new RelayCommand(() => BilinmeyenCurrentPage--, () => BilinmeyenCanPrev);
    public System.Windows.Input.ICommand BilinmeyenNextCommand => new RelayCommand(() => BilinmeyenCurrentPage++, () => BilinmeyenCanNext);
    private void UpdatePagedBilinmeyen()
    {
        var src = BilinmeyenYedekler ?? new List<DatabaseBackupResult>();
        PagedBilinmeyenYedekler = src.Skip((BilinmeyenCurrentPage - 1) * BilinmeyenPageSize).Take(BilinmeyenPageSize).ToList();
        NotifyPropertyChanged(nameof(BilinmeyenTotalPages));
        NotifyPropertyChanged(nameof(BilinmeyenHasPagination));
        NotifyPropertyChanged(nameof(BilinmeyenPageInfo));
    }

    private DatabaseBackupResult _selectedBilinmeyen;
    public DatabaseBackupResult SelectedBilinmeyen
    {
        get => _selectedBilinmeyen;
        set => Set(ref _selectedBilinmeyen, value);
    }

    public async Task TaraAsync()
    {
        try
        {
            if (LocalSettingsService != null)
            {
                try
                {
                    var ayar = await LocalSettingsService.ReadSettingAsync<TenantSettings>(TenantSettings.SettingsKey);
                    if (ayar != null)
                        BilinmeyenPageSize = ayar.GetBilinmeyenPageSize();
                }
                catch { /* model varsayılanı korunur */ }
            }
            var tumResponse = await OperationService.GetAllBackupsAsync();
            var tum = tumResponse?.Data ?? new List<DatabaseBackupResult>();
            var kayitliAdlar = await TumDonemAdlariniYukleAsync();
            BilinmeyenYedekler = tum
                .Where(b => b != null && (string.IsNullOrWhiteSpace(b.DatabaseName) || !kayitliAdlar.Contains(TrimDbSuffixLocal(b.DatabaseName))))
                .ToList();
            SelectedBilinmeyen = BilinmeyenYedekler.FirstOrDefault();
        }
        catch
        {
            BilinmeyenYedekler = new List<DatabaseBackupResult>();
        }
    }

    public async Task TemizleAsync()
    {
        var yedek = SelectedBilinmeyen;
        if (yedek == null)
            return;
        bool onay = await DialogService.ShowConfirmationAsync(
            "Bilinmeyen Yedeği Sil",
            $"'{yedek.BackupFileName}' kalıcı olarak silinecek (ait olduğu dönem kaydı yok).\n\nDevam edilsin mi?",
            "Sil", "Vazgeç");
        if (!onay)
            return;
        try
        {
            await BackupService.CleanupBackupFileAsync(yedek.BackupFilePath);
            NotificationService.Show("Temizlendi", $"'{yedek.BackupFileName}' silindi.", NotificationType.Success);
            await TaraAsync();
        }
        catch (Exception ex)
        {
            NotificationService.Show("Silme Hatası", ex.Message, NotificationType.Danger);
        }
    }

    private static string NormalizeDb(string name) => (name ?? string.Empty).Trim().TrimEnd('.').Trim();
    private static string TrimDbSuffixLocal(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (name.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            name = name.Substring(0, name.Length - 3);
        return name;
    }

    private async Task<HashSet<string>> TumDonemAdlariniYukleAsync()
    {
        try
        {
            var sayfa = await MaliDonemService.GetMaliDonemlerPageAsync(0, int.MaxValue, new DataRequest<MaliDonem> { OrderBy = m => m.MaliYil });
            var adlar = sayfa?.Data?.Where(m => m != null).Select(m => TrimDbSuffixLocal(m.DatabaseName));
            return new HashSet<string>(adlar ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public void Subscribe()
    {
        _eventBus?.Subscribe<TenantBackupCompletedEvent>(this, (s, e) => OnYedekHattiDegisti());
        _eventBus?.Subscribe<TenantRestoreCompletedEvent>(this, (s, e) => OnYedekHattiDegisti());
    }

    public void Unsubscribe()
    {
        _eventBus?.Unsubscribe(this);
    }

    /// <summary>E2: yedek/geri-yükleme sonrası yetim taraması tazelenir (dosya adı değişmiş olabilir).</summary>
    private async void OnYedekHattiDegisti()
    {
        await ContextService.RunAsync(() => { _ = TaraAsync(); });
    }
}
