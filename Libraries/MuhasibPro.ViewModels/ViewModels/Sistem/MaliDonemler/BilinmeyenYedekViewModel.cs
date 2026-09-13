using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
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
        IEventBus eventBus = null,
        ITenantSettingsProvider tenantAyarlari = null) : base(commonServices)
    {
        OperationService = operationService;
        BackupService = backupService;
        MaliDonemService = maliDonemService;
        LocalSettingsService = localSettingsService;
        TenantAyarlari = tenantAyarlari;
        _eventBus = eventBus;
    }

    public ITenantSQLiteDatabaseOperationService OperationService { get; }
    public ITenantBackupService BackupService { get; }
    public IMaliDonemService MaliDonemService { get; }
    public ILocalSettingsService LocalSettingsService { get; }
    public ITenantSettingsProvider TenantAyarlari { get; }

    /// <summary>Sayfa VM'inin açtığı firma (sayfa boyutu firma anahtarından okunur).</summary>
    public long FirmaId { get; set; }

    private List<DatabaseBackupResult> _bilinmeyenYedekler = new();
    public List<DatabaseBackupResult> BilinmeyenYedekler
    {
        get => _bilinmeyenYedekler;
        private set
        {
            if (Set(ref _bilinmeyenYedekler, value))
            {
                NotifyPropertyChanged(nameof(IsBilinmeyenBos));
                NotifyPropertyChanged(nameof(BilinmeyenSayisi));
                NotifyPropertyChanged(nameof(ToplamYetimSayisi));
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
        // Liste kısalınca sayfa taşmasın (öz. son sayfada temizleme sonrası boş liste bug'ı).
        var total = BilinmeyenTotalPages;
        if (_bilinmeyenCurrentPage > total)
            _bilinmeyenCurrentPage = total;
        if (_bilinmeyenCurrentPage < 1)
            _bilinmeyenCurrentPage = 1;
        var src = BilinmeyenYedekler ?? new List<DatabaseBackupResult>();
        PagedBilinmeyenYedekler = src.Skip((BilinmeyenCurrentPage - 1) * BilinmeyenPageSize).Take(BilinmeyenPageSize).ToList();
        NotifyPropertyChanged(nameof(BilinmeyenCurrentPage));
        NotifyPropertyChanged(nameof(BilinmeyenTotalPages));
        NotifyPropertyChanged(nameof(BilinmeyenHasPagination));
        NotifyPropertyChanged(nameof(BilinmeyenPageInfo));
        NotifyPropertyChanged(nameof(BilinmeyenCanPrev));
        NotifyPropertyChanged(nameof(BilinmeyenCanNext));
    }

    private DatabaseBackupResult _selectedBilinmeyen;
    public DatabaseBackupResult SelectedBilinmeyen
    {
        get => _selectedBilinmeyen;
        set => Set(ref _selectedBilinmeyen, value);
    }

    private List<DatabaseBackupResult> _silinenDonemYedekleri = new();
    /// <summary>Dönemi silinmiş ama yedeği korunmuş dosyalar (kimlikli, dönem kaydı yok) — ayrı liste.</summary>
    public List<DatabaseBackupResult> SilinenDonemYedekleri
    {
        get => _silinenDonemYedekleri;
        private set
        {
            if (Set(ref _silinenDonemYedekleri, value))
            {
                NotifyPropertyChanged(nameof(IsSilinenBos));
                NotifyPropertyChanged(nameof(SilinenSayisi));
                NotifyPropertyChanged(nameof(ToplamYetimSayisi));
                SilinenCurrentPage = 1;
                UpdatePagedSilinen();
            }
        }
    }

    // ── Pagination: SilinenDonemYedekleri (boyut Bilinmeyen ile aynı) ──
    private int _silinenCurrentPage = 1;
    public int SilinenCurrentPage
    {
        get => _silinenCurrentPage;
        set
        {
            var clamped = Math.Clamp(value, 1, Math.Max(1, SilinenTotalPages));
            if (Set(ref _silinenCurrentPage, clamped))
            {
                UpdatePagedSilinen();
                NotifyPropertyChanged(nameof(SilinenCanPrev));
                NotifyPropertyChanged(nameof(SilinenCanNext));
                NotifyPropertyChanged(nameof(SilinenPageInfo));
            }
        }
    }
    public int SilinenTotalPages => Math.Max(1, (int)Math.Ceiling((SilinenDonemYedekleri?.Count ?? 0) / (double)BilinmeyenPageSize));
    public bool SilinenHasPagination => (SilinenDonemYedekleri?.Count ?? 0) > BilinmeyenPageSize;
    public bool SilinenCanPrev => SilinenCurrentPage > 1;
    public bool SilinenCanNext => SilinenCurrentPage < SilinenTotalPages;
    public string SilinenPageInfo => $"{SilinenCurrentPage} / {SilinenTotalPages}";
    private List<DatabaseBackupResult> _pagedSilinenDonemYedekleri = new();
    public List<DatabaseBackupResult> PagedSilinenDonemYedekleri
    {
        get => _pagedSilinenDonemYedekleri;
        private set => Set(ref _pagedSilinenDonemYedekleri, value);
    }
    public System.Windows.Input.ICommand SilinenPrevCommand => new RelayCommand(() => SilinenCurrentPage--, () => SilinenCanPrev);
    public System.Windows.Input.ICommand SilinenNextCommand => new RelayCommand(() => SilinenCurrentPage++, () => SilinenCanNext);
    private void UpdatePagedSilinen()
    {
        var total = SilinenTotalPages;
        if (_silinenCurrentPage > total)
            _silinenCurrentPage = total;
        if (_silinenCurrentPage < 1)
            _silinenCurrentPage = 1;
        var src = SilinenDonemYedekleri ?? new List<DatabaseBackupResult>();
        PagedSilinenDonemYedekleri = src.Skip((SilinenCurrentPage - 1) * BilinmeyenPageSize).Take(BilinmeyenPageSize).ToList();
        NotifyPropertyChanged(nameof(SilinenCurrentPage));
        NotifyPropertyChanged(nameof(SilinenTotalPages));
        NotifyPropertyChanged(nameof(SilinenHasPagination));
        NotifyPropertyChanged(nameof(SilinenPageInfo));
        NotifyPropertyChanged(nameof(SilinenCanPrev));
        NotifyPropertyChanged(nameof(SilinenCanNext));
    }

    private DatabaseBackupResult _selectedSilinen;
    public DatabaseBackupResult SelectedSilinen
    {
        get => _selectedSilinen;
        set => Set(ref _selectedSilinen, value);
    }

    private bool _isTaraniyor;
    /// <summary>Tarama/temizleme sürerken panel ring gösterir (Kural 11: try/finally ile kapanır).</summary>
    public bool IsTaraniyor
    {
        get => _isTaraniyor;
        private set
        {
            if (Set(ref _isTaraniyor, value))
            {
                NotifyPropertyChanged(nameof(IsBilinmeyenBos));
                NotifyPropertyChanged(nameof(IsSilinenBos));
            }
        }
    }

    /// <summary>Boş-durumlar: tarama bitti + 0 kayıt (Kural 11).</summary>
    public bool IsBilinmeyenBos => !IsTaraniyor && (BilinmeyenYedekler?.Count ?? 0) == 0;
    public bool IsSilinenBos => !IsTaraniyor && (SilinenDonemYedekleri?.Count ?? 0) == 0;

    /// <summary>Sekme/navbar rozet sayaçları (liste Count yolu bildirim üretmez).</summary>
    public int BilinmeyenSayisi => BilinmeyenYedekler?.Count ?? 0;
    public int SilinenSayisi => SilinenDonemYedekleri?.Count ?? 0;
    public int ToplamYetimSayisi => BilinmeyenSayisi + SilinenSayisi;

    public async Task TaraAsync()
    {
        try
        {
            if (TenantAyarlari != null)
            {
                var ayar = await TenantAyarlari.GetAsync(FirmaId);
                BilinmeyenPageSize = ayar.GetBilinmeyenPageSize();
            }
            else if (LocalSettingsService != null)
            {
                try
                {
                    var ayar = await LocalSettingsService.ReadSettingAsync<TenantSettings>(TenantSettings.SettingsKey);
                    if (ayar != null)
                        BilinmeyenPageSize = ayar.GetBilinmeyenPageSize();
                }
                catch { /* model varsayılanı korunur */ }
            }
        }
        catch { /* model varsayılanı korunur */ }
        IsTaraniyor = true;
        try
        {
            var taramaGorevi = OperationService.GetAllBackupsAsync();
            var tamamlanan = await Task.WhenAny(taramaGorevi, Task.Delay(TimeSpan.FromSeconds(30)));
            if (tamamlanan != taramaGorevi)
            {
                StatusError("Yedek taraması zaman aşımına uğradı (30sn). Disk yanıt vermiyor olabilir.");
                return;
            }
            var tumResponse = await taramaGorevi;
            var tum = tumResponse?.Data ?? new List<DatabaseBackupResult>();
            var (kayitliAdlar, kayitliIdler) = await KayitliDonemKumesiniYukleAsync();
            var silinen = new List<DatabaseBackupResult>();
            var bilinmeyen = new List<DatabaseBackupResult>();
            foreach (var b in tum)
            {
                if (b == null)
                    continue;
                if (!string.IsNullOrWhiteSpace(b.DatabaseName) && kayitliAdlar.Contains(TrimDbSuffixLocal(b.DatabaseName)))
                    continue;
                if (SilinenDonemYedegiMi(b, kayitliIdler))
                    silinen.Add(b);
                else
                    bilinmeyen.Add(b);
            }
            SilinenDonemYedekleri = silinen;
            BilinmeyenYedekler = bilinmeyen;
            SelectedBilinmeyen = BilinmeyenYedekler.FirstOrDefault();
            SelectedSilinen = SilinenDonemYedekleri.FirstOrDefault();
        }
        catch (Exception ex)
        {
            StatusError($"Yedek taraması başarısız: {ex.Message}");
            BilinmeyenYedekler = new List<DatabaseBackupResult>();
            SilinenDonemYedekleri = new List<DatabaseBackupResult>();
        }
        finally
        {
            IsTaraniyor = false;
        }
    }

    /// <summary>Kimlikli yedek ama dönem kaydı yok (+firma uyumu) → silinen dönem yedeği.</summary>
    private bool SilinenDonemYedegiMi(DatabaseBackupResult b, HashSet<long> kayitliIdler)
    {
        if (b.KimlikMaliDonemId == null || b.KimlikMaliDonemId.Value <= 0)
            return false;
        if (b.KimlikFirmaId != null && FirmaId > 0 && b.KimlikFirmaId.Value != FirmaId)
            return false;
        return !kayitliIdler.Contains(b.KimlikMaliDonemId.Value);
    }

    public async Task TemizleAsync()
    {
        await SeciliYedegiTemizleAsync(SelectedBilinmeyen,
            "Bilinmeyen Yedeği Sil",
            $"'{SelectedBilinmeyen?.BackupFileName}' kalıcı olarak silinecek (ait olduğu dönem kaydı yok).\n\nDevam edilsin mi?");
    }

    public async Task SilinenTemizleAsync()
    {
        await SeciliYedegiTemizleAsync(SelectedSilinen,
            "Silinen Dönem Yedeğini Sil",
            $"'{SelectedSilinen?.BackupFileName}' kalıcı olarak silinecek (ait olduğu dönem silinmiş).\n\nDevam edilsin mi?");
    }

    private async Task SeciliYedegiTemizleAsync(DatabaseBackupResult yedek, string baslik, string mesaj)
    {
        if (yedek == null)
            return;
        bool onay = await DialogService.ShowConfirmationAsync(baslik, mesaj, "Sil", "Vazgeç");
        if (!onay)
            return;
        IsTaraniyor = true;
        try
        {
            var (silindi, neden) = await BackupService.TryDeleteBackupFileAsync(yedek.BackupFilePath);
            if (!silindi)
            {
                NotificationService.ShowTagged("Temizlenemedi",
                    $"'{yedek.BackupFileName}' silinemedi. Neden: {neden}",
                    NotificationType.Danger,
                    "BilinmeyenTemizle", NotificationGroups.Yedek);
            }
            else
            {
                NotificationService.ShowTagged("Temizlendi", $"'{yedek.BackupFileName}' silindi.", NotificationType.Success,
                    "BilinmeyenTemizle", NotificationGroups.Yedek);
            }
            await TaraAsync();
        }
        catch (Exception ex)
        {
            NotificationService.ShowTagged("Silme Hatası", ex.Message, NotificationType.Danger,
                "BilinmeyenTemizle", NotificationGroups.Yedek);
        }
        finally
        {
            IsTaraniyor = false;
        }
    }

    private static string TrimDbSuffixLocal(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (name.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            name = name.Substring(0, name.Length - 3);
        return name;
    }

    /// <summary>Açık firmanın kayıtlı dönem adları + Id'leri (ad-eşleşme + silinen tespiti için).
    /// Firma yoksa eski global yol korunur (davranış değişmez).</summary>
    private async Task<(HashSet<string> adlar, HashSet<long> idler)> KayitliDonemKumesiniYukleAsync()
    {
        var bos = (new HashSet<string>(StringComparer.OrdinalIgnoreCase), new HashSet<long>());
        try
        {
            IList<MaliDonemModel> donemler = null;
            if (FirmaId > 0)
            {
                var firmaSayfasi = await MaliDonemService.GetMaliDonemlerWithFirmaId(
                    new DataRequest<MaliDonem> { OrderBy = m => m.MaliYil }, FirmaId);
                donemler = firmaSayfasi?.Data;
            }
            else
            {
                var sayfa = await MaliDonemService.GetMaliDonemlerPageAsync(0, int.MaxValue, new DataRequest<MaliDonem> { OrderBy = m => m.MaliYil });
                donemler = sayfa?.Data;
            }
            var gecerli = donemler?.Where(m => m != null).ToList() ?? new List<MaliDonemModel>();
            return (
                new HashSet<string>(gecerli.Select(m => TrimDbSuffixLocal(m.DatabaseName)), StringComparer.OrdinalIgnoreCase),
                new HashSet<long>(gecerli.Select(m => m.Id)));
        }
        catch
        {
            return bos;
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
