using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Arşivlenen dönemler: listele / kapat / geri aç. Tek sorumluluk: arşiv paneli.</summary>
public class ArsivDonemlerViewModel : ViewModelBase
{
    public ArsivDonemlerViewModel(
        ICommonServices commonServices,
        IMaliDonemService maliDonemService,
        ILocalSettingsService localSettingsService = null,
        IEntityRegistrySettingsProvider entityAyarlari = null) : base(commonServices)
    {
        MaliDonemService = maliDonemService;
        LocalSettingsService = localSettingsService;
        EntityAyarlari = entityAyarlari;
    }

    public IMaliDonemService MaliDonemService { get; }
    public ILocalSettingsService LocalSettingsService { get; }
    public IEntityRegistrySettingsProvider EntityAyarlari { get; }

    /// <summary>Sayfa VM'inin açtığı firma (sayfa boyutu firma anahtarından okunur).</summary>
    public long FirmaId { get; set; }

    private bool _isArsivYukleniyor;
    /// <summary>Arşiv süzülürken panel ring gösterir (Kural 11: bellek-içi hızlı işlem).</summary>
    public bool IsArsivYukleniyor
    {
        get => _isArsivYukleniyor;
        private set => Set(ref _isArsivYukleniyor, value);
    }

    private List<MaliDonemModel> _arsivliDonemler = new();
    /// <summary>Yönetim VM'inin listesinden süzülür (Refresh ile tazelenir).</summary>
    public List<MaliDonemModel> ArsivliDonemler
    {
        get => _arsivliDonemler;
        private set
        {
            if (Set(ref _arsivliDonemler, value))
            {
                ArsivCurrentPage = 1;
                UpdatePagedArsiv();
            }
        }
    }

    // ── Pagination: ArsivliDonemler (boyut EntityRegistrySettings'ten) ──
    private int _arsivPageSize = new EntityRegistrySettings().GetArsivPageSize();
    public int ArsivPageSize
    {
        get => _arsivPageSize;
        private set
        {
            if (Set(ref _arsivPageSize, value))
            {
                NotifyPropertyChanged(nameof(ArsivTotalPages));
                NotifyPropertyChanged(nameof(ArsivHasPagination));
                UpdatePagedArsiv();
            }
        }
    }
    private int _arsivCurrentPage = 1;
    public int ArsivCurrentPage
    {
        get => _arsivCurrentPage;
        set
        {
            var clamped = Math.Clamp(value, 1, Math.Max(1, ArsivTotalPages));
            if (Set(ref _arsivCurrentPage, clamped))
            {
                UpdatePagedArsiv();
                NotifyPropertyChanged(nameof(ArsivCanPrev));
                NotifyPropertyChanged(nameof(ArsivCanNext));
                NotifyPropertyChanged(nameof(ArsivPageInfo));
            }
        }
    }
    public int ArsivTotalPages => Math.Max(1, (int)Math.Ceiling((ArsivliDonemler?.Count ?? 0) / (double)ArsivPageSize));
    public bool ArsivHasPagination => (ArsivliDonemler?.Count ?? 0) > ArsivPageSize;
    public bool ArsivCanPrev => ArsivCurrentPage > 1;
    public bool ArsivCanNext => ArsivCurrentPage < ArsivTotalPages;
    public string ArsivPageInfo => $"{ArsivCurrentPage} / {ArsivTotalPages}";
    private List<MaliDonemModel> _pagedArsivliDonemler = new();
    public List<MaliDonemModel> PagedArsivliDonemler
    {
        get => _pagedArsivliDonemler;
        private set => Set(ref _pagedArsivliDonemler, value);
    }
    public System.Windows.Input.ICommand ArsivPrevCommand => new RelayCommand(() => ArsivCurrentPage--, () => ArsivCanPrev);
    public System.Windows.Input.ICommand ArsivNextCommand => new RelayCommand(() => ArsivCurrentPage++, () => ArsivCanNext);
    private void UpdatePagedArsiv()
    {
        // Liste kısalınca sayfa taşmasın (öz. son sayfada arşivden çıkarma sonrası boş liste bug'ı).
        var total = ArsivTotalPages;
        if (_arsivCurrentPage > total)
            _arsivCurrentPage = total;
        if (_arsivCurrentPage < 1)
            _arsivCurrentPage = 1;
        var src = ArsivliDonemler ?? new List<MaliDonemModel>();
        PagedArsivliDonemler = src.Skip((ArsivCurrentPage - 1) * ArsivPageSize).Take(ArsivPageSize).ToList();
        NotifyPropertyChanged(nameof(ArsivCurrentPage));
        NotifyPropertyChanged(nameof(ArsivTotalPages));
        NotifyPropertyChanged(nameof(ArsivHasPagination));
        NotifyPropertyChanged(nameof(ArsivPageInfo));
        NotifyPropertyChanged(nameof(ArsivCanPrev));
        NotifyPropertyChanged(nameof(ArsivCanNext));
    }

    public void Refresh(IEnumerable<MaliDonemModel> tumDonemler)
    {
        ArsivliDonemler = tumDonemler?.Where(m => m != null && m.ArsivliMi).OrderByDescending(m => m.MaliYil).ToList()
            ?? new List<MaliDonemModel>();
    }

    public async Task RefreshAsync(IEnumerable<MaliDonemModel> tumDonemler)
    {
        IsArsivYukleniyor = true;
        try
        {
            try
            {
                if (EntityAyarlari != null)
                {
                    var ayar = await EntityAyarlari.GetAsync(FirmaId);
                    ArsivPageSize = ayar.GetArsivPageSize();
                }
                else if (LocalSettingsService != null)
                {
                    var ayar = await LocalSettingsService.ReadSettingAsync<EntityRegistrySettings>(EntityRegistrySettings.SettingsKey);
                    if (ayar != null)
                        ArsivPageSize = ayar.GetArsivPageSize();
                }
            }
            catch { /* model varsayılanı korunur */ }
            Refresh(tumDonemler);
        }
        finally
        {
            IsArsivYukleniyor = false;
        }
    }

    public async Task<bool> ArsivleAsync(MaliDonemModel model)
    {
        if (model == null)
            return false;
        bool onay = await DialogService.ShowConfirmationAsync(
            "Mali Dönemi Kapat",
            $"{model.MaliYil} dönemi kapatılacak. Giriş yapılamaz, kayıtlar korunur; daha sonra geri açılabilir.\n\nDevam edilsin mi?",
            "Kapat", "Vazgeç");
        if (!onay)
            return false;
        return await DurumGuncelleAsync(model, DonemDurum.Arsivlenmis, true, "kapatıldı");
    }

    public async Task<bool> ArsivdenCikarAsync(MaliDonemModel model)
    {
        if (model == null)
            return false;
        bool onay = await DialogService.ShowConfirmationAsync(
            "Arşivden Çıkar",
            $"{model.MaliYil} dönemi yeniden açılacak.\n\nDevam edilsin mi?",
            "Aç", "Vazgeç");
        if (!onay)
            return false;
        return await DurumGuncelleAsync(model, DonemDurum.Acik, false, "yeniden açıldı");
    }

    private async Task<bool> DurumGuncelleAsync(MaliDonemModel model, DonemDurum durum, bool arsivlendiMi, string eylem)
    {
        try
        {
            model.TenantDetails ??= new Business.ResultModels.TenantResultModels.TenantDetailsModel();
            model.TenantDetails.Durum = durum;
            model.TenantDetails.ArsivlendiMi = arsivlendiMi;
            var response = await MaliDonemService.UpdateMaliDonemAsync(model);
            if (response.Success)
            {
                NotificationService.ShowTagged("Arşiv", $"{model.MaliYil} dönemi {eylem}.", NotificationType.Success,
                    "ArsivDurum", NotificationGroups.DonemIslemleri);
                return true;
            }
            NotificationService.ShowTagged("Arşiv", response.Message ?? "İşlem başarısız.", NotificationType.Warning,
                "ArsivDurum", NotificationGroups.DonemIslemleri);
            return false;
        }
        catch (Exception ex)
        {
            NotificationService.ShowTagged("Arşiv Hatası", ex.Message, NotificationType.Danger,
                "ArsivDurum", NotificationGroups.DonemIslemleri);
            return false;
        }
    }
}
