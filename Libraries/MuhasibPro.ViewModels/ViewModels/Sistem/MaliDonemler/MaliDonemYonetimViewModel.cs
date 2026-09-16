using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

public class MaliDonemYonetimArgs
{
    public long FirmaId { get; set; }
    public string FirmaKodu { get; set; }
    public string KisaUnvani { get; set; }
}

/// <summary>
/// Mali Dönem yönetim penceresi orkestratörü: firma bağlamı + dönem seçimi + bölüm geçişi.
/// Ağır işler alt VM'lerdedir (Yedekler/Arsiv/Bilinmeyen); kendisi seçim ve tazeleme dışında iş yapmaz.
/// </summary>
public class MaliDonemYonetimViewModel : ViewModelBase, IMaliDonemListHost
{
    public MaliDonemYonetimViewModel(
        ICommonServices commonServices,
        IMaliDonemService maliDonemService,
        ITenantSQLiteDatabaseService tenantDatabaseService,
        ITenantSQLiteDatabaseOperationService operationService,
        ITenantBackupService backupService,
        ILocalSettingsService localSettingsService = null,
        IEventBus eventBus = null,
        ITenantSettingsProvider tenantAyarlari = null,
        IEntityRegistrySettingsProvider entityAyarlari = null,
        IAuthenticationService auth = null,
        IFirmaService firmaService = null,
        IKullaniciService kullaniciService = null) : base(commonServices)
    {
        LocalSettingsService = localSettingsService;
        TenantAyarlari = tenantAyarlari;
        EntityAyarlari = entityAyarlari;
        MaliDonemList = new MaliDonemListViewModel(commonServices, maliDonemService, tenantDatabaseService);
        YedeklerVM = new DonemYedeklerViewModel(commonServices, operationService, backupService, maliDonemService, localSettingsService, eventBus, tenantAyarlari);
        ArsivVM = new ArsivDonemlerViewModel(commonServices, maliDonemService, localSettingsService, entityAyarlari);
        BilinmeyenVM = new BilinmeyenYedekViewModel(commonServices, operationService, backupService, maliDonemService, localSettingsService, eventBus, tenantAyarlari);
        GenelBakisVM = new DonemGenelBakisViewModel(commonServices, MaliDonemList, operationService);
        AyarlarVM = new YonetimAyarlarViewModel(commonServices, localSettingsService, tenantAyarlari, entityAyarlari, auth, eventBus, firmaService, kullaniciService);
        MaliDonemList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MaliDonemListViewModel.IsListeYukleniyor))
                NotifyPropertyChanged(nameof(IsDonemListesiBos));
        };
        // Toplu analiz bittiğinde KPI'ı tazele (analiz bitmeden %0 Sağlıklı yalanını önler).
        MaliDonemList.TopluAnalizTamamlandi += () =>
        {
            try { GenelBakisVM.YenileAsync().ConfigureAwait(false); }
            catch { /* KPI tazeleme görevsel, listeyi kırmaz */ }
        };
    }

    public ILocalSettingsService LocalSettingsService { get; }
    public ITenantSettingsProvider TenantAyarlari { get; }
    public IEntityRegistrySettingsProvider EntityAyarlari { get; }

    public MaliDonemListViewModel MaliDonemList { get; }
    public DonemYedeklerViewModel YedeklerVM { get; }
    public ArsivDonemlerViewModel ArsivVM { get; }
    public BilinmeyenYedekViewModel BilinmeyenVM { get; }
    public DonemGenelBakisViewModel GenelBakisVM { get; }
    public YonetimAyarlarViewModel AyarlarVM { get; }

    public FirmaModel SelectedFirma { get; private set; }

    private bool _isSayfaYukleniyor;
    /// <summary>Sayfa ilk veriyi çekerken üst şerit ring gösterir (Kural 11).</summary>
    public bool IsSayfaYukleniyor
    {
        get => _isSayfaYukleniyor;
        private set => Set(ref _isSayfaYukleniyor, value);
    }

    public string FirmaBaslik => SelectedFirma == null
        ? "Mali Dönem Yönetimi"
        : $"{SelectedFirma.KisaUnvani} — Mali Dönem Yönetimi";

    public string FirmaOzet => SelectedFirma == null
        ? string.Empty
        : $"{SelectedFirma.FirmaKodu} • {MaliDonemList.ItemsSource?.Count ?? 0} dönem";

    /// <summary>Yönetim penceresinde firma bağlamı sabittir.</summary>
    public bool IsFirmaSelected => SelectedFirma != null;

    /// <summary>Yönetim penceresinde çalışma alanına geçiş yok.</summary>
    public bool HasSelection => false;

    /// <summary>Yenileme sırasında ComboBox ara-reset null'larını yoksay (seçim koruma).</summary>
    private bool _secimKoruma;

    private MaliDonemModel _selectedDonem;
    public MaliDonemModel SelectedDonem
    {
        get => _selectedDonem;
        set
        {
            // ComboBox TwoWay ara-reset'i: liste yenilenirken ItemsSource.Clear() anında
            // (o anda sayı 0 görünür!) Combo seçimi düşürüp null yazar. Koruma sırasında
            // veya liste doluyken null'ı yoksay; liste gerçekten boşken kabul et.
            if (value == null && (_secimKoruma || (MaliDonemList?.ItemsSource?.Count ?? 0) > 0))
                return;
            if (Set(ref _selectedDonem, value))
            {
                NotifyPropertyChanged(nameof(DonemBaslik));
                NotifyPropertyChanged(nameof(IsDonemSeciliDegil));
                // Seçili dönem açık listede ise pagination'ı o sayfaya taşı
                if (value != null && AcikDonemler != null)
                {
                    var idx = AcikDonemler.FindIndex(m => m.Id == value.Id);
                    if (idx >= 0)
                    {
                        var targetPage = (idx / AcikPageSize) + 1;
                        if (targetPage != AcikCurrentPage)
                            AcikCurrentPage = targetPage;
                    }
                }
                _ = DonemDegistiAsync();
            }
        }
    }

    /// <summary>Sol seçim listesi: arşivli olmayan dönemler (Arşivli grup ayrı kaynaktan).</summary>
    public List<MaliDonemModel> AcikDonemler
    {
        get => _acikDonemler;
        private set
        {
            if (Set(ref _acikDonemler, value))
            {
                AcikCurrentPage = 1;
                UpdatePagedAcik();
            }
        }
    }
    private List<MaliDonemModel> _acikDonemler = new();

    // ── Pagination: AcikDonemler (50 item → dikey uzamayı önler; boyut EntityRegistrySettings'ten) ──
    private int _acikPageSize = new EntityRegistrySettings().GetAcikPageSize();
    public int AcikPageSize
    {
        get => _acikPageSize;
        private set
        {
            if (Set(ref _acikPageSize, value))
            {
                NotifyPropertyChanged(nameof(AcikTotalPages));
                NotifyPropertyChanged(nameof(AcikHasPagination));
                NotifyPropertyChanged(nameof(AcikPageInfo));
                UpdatePagedAcik();
            }
        }
    }

    private async Task AcikSayfaBoyutunuYukleAsync()
    {
        try
        {
            if (EntityAyarlari != null)
            {
                var ayar = await EntityAyarlari.GetAsync(SelectedFirma?.Id ?? 0);
                AcikPageSize = ayar.GetAcikPageSize();
            }
            else if (LocalSettingsService != null)
            {
                var ayar = await LocalSettingsService.ReadSettingAsync<EntityRegistrySettings>(EntityRegistrySettings.SettingsKey);
                if (ayar != null)
                    AcikPageSize = ayar.GetAcikPageSize();
            }
        }
        catch { /* model varsayılanı korunur */ }
    }
    private int _acikCurrentPage = 1;
    public int AcikCurrentPage
    {
        get => _acikCurrentPage;
        set
        {
            var clamped = Math.Clamp(value, 1, Math.Max(1, AcikTotalPages));
            if (Set(ref _acikCurrentPage, clamped))
            {
                UpdatePagedAcik();
                NotifyPropertyChanged(nameof(AcikCanPrev));
                NotifyPropertyChanged(nameof(AcikCanNext));
                NotifyPropertyChanged(nameof(AcikPageInfo));
            }
        }
    }
    public int AcikTotalPages => Math.Max(1, (int)Math.Ceiling((AcikDonemler?.Count ?? 0) / (double)AcikPageSize));
    public bool AcikHasPagination => (AcikDonemler?.Count ?? 0) > AcikPageSize;
    public bool AcikCanPrev => AcikCurrentPage > 1;
    public bool AcikCanNext => AcikCurrentPage < AcikTotalPages;
    public string AcikPageInfo => $"{AcikCurrentPage} / {AcikTotalPages}";
    private List<MaliDonemModel> _pagedAcikDonemler = new();
    public List<MaliDonemModel> PagedAcikDonemler
    {
        get => _pagedAcikDonemler;
        private set => Set(ref _pagedAcikDonemler, value);
    }
    public System.Windows.Input.ICommand AcikPrevCommand => new RelayCommand(() => AcikCurrentPage--, () => AcikCanPrev);
    public System.Windows.Input.ICommand AcikNextCommand => new RelayCommand(() => AcikCurrentPage++, () => AcikCanNext);
    private void UpdatePagedAcik()
    {
        // Liste kısalınca sayfa taşmasın (öz. son sayfada silme/arşivleme sonrası boş liste bug'ı).
        var total = AcikTotalPages;
        if (_acikCurrentPage > total)
            _acikCurrentPage = total;
        if (_acikCurrentPage < 1)
            _acikCurrentPage = 1;
        var src = AcikDonemler ?? new List<MaliDonemModel>();
        PagedAcikDonemler = src.Skip((AcikCurrentPage - 1) * AcikPageSize).Take(AcikPageSize).ToList();
        NotifyPropertyChanged(nameof(AcikCurrentPage));
        NotifyPropertyChanged(nameof(AcikTotalPages));
        NotifyPropertyChanged(nameof(AcikHasPagination));
        NotifyPropertyChanged(nameof(AcikPageInfo));
        NotifyPropertyChanged(nameof(AcikCanPrev));
        NotifyPropertyChanged(nameof(AcikCanNext));
    }

    /// <summary>Sol listede ARŞİVLİ grubu görünsün mü?</summary>
    public bool ArsivGrubuVarMi => (ArsivVM.ArsivliDonemler?.Count ?? 0) > 0;

    /// <summary>Sol liste boş-durumu: yükleme bitti + 0 açık dönem (Kural 11: ring ile aynı anda görünmez).</summary>
    public bool IsDonemListesiBos => !MaliDonemList.IsListeYukleniyor && (AcikDonemler?.Count ?? 0) == 0;

    /// <summary>Null-güvenlik: seçim yoksa detay içerik yerine uyarı barı gösterilir.</summary>
    public bool IsDonemSeciliDegil => SelectedDonem == null;

    /// <summary>Seçim gerektiren işlemler için guard: yoksa uyarır.</summary>
    public bool DonemSeciliMi(string islemAdi)
    {
        if (SelectedDonem != null)
            return true;
        NotificationService.Show("Dönem Seçilmedi",
            $"{islemAdi} için önce listeden bir dönem seçin.",
            NotificationType.Warning);
        return false;
    }

    public string DonemBaslik => SelectedDonem == null
        ? "Dönem seçilmedi"
        : $"{SelectedDonem.MaliYil} — {SelectedDonem.DatabaseName}";

    private int _tenantDbSayisi;
    public int TenantDbSayisi
    {
        get => _tenantDbSayisi;
        private set
        {
            if (Set(ref _tenantDbSayisi, value))
                NotifyPropertyChanged(nameof(TumuSegmentBasligi));
        }
    }

    private int _arsivSekmeSayisi;
    public int ArsivSekmeSayisi
    {
        get => _arsivSekmeSayisi;
        private set
        {
            if (Set(ref _arsivSekmeSayisi, value))
                NotifyPropertyChanged(nameof(ArsivSekmeBasligi));
        }
    }

    public string TumuSegmentBasligi => $"Tüm Tenant DB'leri ({TenantDbSayisi})";
    public string ArsivSekmeBasligi => $"Arşiv ({ArsivSekmeSayisi})";

    private TenantDerinAnaliz _derinAnaliz;
    /// <summary>Seçili dönemin isteğe bağlı derin analizi ("Derin Analiz Çalıştır").</summary>
    public TenantDerinAnaliz DerinAnaliz
    {
        get => _derinAnaliz;
        private set => Set(ref _derinAnaliz, value);
    }

    private bool _isDerinAnalizYukleniyor;
    public bool IsDerinAnalizYukleniyor
    {
        get => _isDerinAnalizYukleniyor;
        private set => Set(ref _isDerinAnalizYukleniyor, value);
    }

    private bool _isBakimCalisiyor;
    public bool IsBakimCalisiyor
    {
        get => _isBakimCalisiyor;
        private set => Set(ref _isBakimCalisiyor, value);
    }

    private string _sonBakimMesaji = string.Empty;
    /// <summary>Son tek-dönem bakım sonucu (panel satır içi bilgi).</summary>
    public string SonBakimMesaji
    {
        get => _sonBakimMesaji;
        private set => Set(ref _sonBakimMesaji, value);
    }

    public async Task DerinAnalizYukleAsync()
    {
        var donem = SelectedDonem;
        if (donem == null || string.IsNullOrWhiteSpace(donem.DatabaseName) || IsDerinAnalizYukleniyor)
            return;
        IsDerinAnalizYukleniyor = true;
        try
        {
            var response = await YedeklerVM.OperationService.GetDerinAnalizAsync(donem.DatabaseName);
            DerinAnaliz = response?.Data;
            NotificationService.ShowTagged("Derin Analiz Tamamlandı",
                $"{donem.MaliYil} dönemi analiz edildi.",
                NotificationType.Info,
                "DerinAnaliz", NotificationGroups.Analiz);
        }
        catch (Exception ex)
        {
            DerinAnaliz = null;
            NotificationService.ShowTagged("Derin Analiz Hatası", ex.Message, NotificationType.Danger,
                "DerinAnaliz", NotificationGroups.Analiz);
        }
        finally
        {
            IsDerinAnalizYukleniyor = false;
        }
    }

    /// <summary>Seçili döneme tek bakım komutu (VACUUM / REINDEX / WAL) veya TUMU (üçü sırayla).</summary>
    public async Task<bool> BakimCalistirAsync(string komut)
    {
        var donem = SelectedDonem;
        if (donem == null || string.IsNullOrWhiteSpace(donem.DatabaseName) || IsBakimCalisiyor)
            return false;
        IsBakimCalisiyor = true;
        try
        {
            var komutlar = string.Equals(komut, "TUMU", StringComparison.OrdinalIgnoreCase)
                ? new[] { "VACUUM", "REINDEX", "WAL" }
                : new[] { (komut ?? string.Empty).ToUpperInvariant() };
            int ok = 0;
            string sonMesaj = string.Empty;
            foreach (var k in komutlar)
            {
                try
                {
                    var response = await YedeklerVM.OperationService.BakimCalistirAsync(donem.DatabaseName, k);
                    if (response.Success && response.Data != null && response.Data.Basarili)
                    {
                        ok++;
                        sonMesaj = response.Data.Mesaj;
                    }
                    else
                    {
                        sonMesaj = response.Message ?? response.Data?.Mesaj ?? "Bakım başarısız.";
                    }
                }
                catch { /* tek komut batch'i durdurmaz */ }
            }
            bool tumOk = ok == komutlar.Length;
            SonBakimMesaji = tumOk ? $"{komut} tamamlandı." : sonMesaj;
            NotificationService.ShowTagged(tumOk ? "Bakım Tamamlandı" : "Bakım Eksik",
                tumOk ? $"{donem.MaliYil} dönemi: {komut} tamamlandı." : sonMesaj,
                tumOk ? NotificationType.Success : NotificationType.Warning,
                "BakimCalistir", NotificationGroups.Bakim);
            return tumOk;
        }
        catch (Exception ex)
        {
            SonBakimMesaji = ex.Message;
            NotificationService.ShowTagged("Bakım Hatası", ex.Message, NotificationType.Danger,
                "BakimCalistir", NotificationGroups.Bakim);
            return false;
        }
        finally
        {
            IsBakimCalisiyor = false;
        }
    }

    /// <summary>Sayfa yükleme tavanı: bu sürede bitmezse son-snapshot'ta kalınır (Kural 11).</summary>
    private static readonly TimeSpan YuklemeTimeout = TimeSpan.FromSeconds(30);

    public async Task LoadAsync(MaliDonemYonetimArgs args)
    {
        _secimKoruma = true;
        IsSayfaYukleniyor = true;
        try
        {
            var cts = new CancellationTokenSource(YuklemeTimeout);
            await YukleCoreAsync(args, cts.Token);
        }
        catch (OperationCanceledException)
        {
            NotificationService.ShowTagged("Yükleme Zaman Aşımı",
                "Dönem listesi zamanında yüklenemedi; mevcut veriler gösterilmektedir.",
                NotificationType.Warning,
                "Yukleme", NotificationGroups.Yonetim);
        }
        finally
        {
            _secimKoruma = false;
            IsSayfaYukleniyor = false;
        }
    }

    private async Task YukleCoreAsync(MaliDonemYonetimArgs args, CancellationToken ct)
    {
        await AcikSayfaBoyutunuYukleAsync();
        ct.ThrowIfCancellationRequested();

        if (args != null && args.FirmaId > 0)
        {
            SelectedFirma = new FirmaModel
            {
                Id = args.FirmaId,
                FirmaKodu = args.FirmaKodu,
                KisaUnvani = args.KisaUnvani
            };
        }
        NotifyPropertyChanged(nameof(SelectedFirma));
        NotifyPropertyChanged(nameof(FirmaBaslik));
        NotifyPropertyChanged(nameof(FirmaOzet));
        NotifyPropertyChanged(nameof(IsFirmaSelected));

        // Firma bağlamı alt VM'lere iner (sayfa boyutu + saklama firma anahtarından okunur).
        long firmaId = SelectedFirma?.Id ?? 0;
        ArsivVM.FirmaId = firmaId;
        BilinmeyenVM.FirmaId = firmaId;
        AyarlarVM.FirmaId = firmaId;
        AyarlarVM.FirmaBasligi = SelectedFirma == null
            ? string.Empty
            : $"{SelectedFirma.KisaUnvani} ({SelectedFirma.FirmaKodu})";
        // İlk çağrı firmasızdı (global); firma belli olunca firma anahtarıyla tekrar oku.
        await AcikSayfaBoyutunuYukleAsync();
        ct.ThrowIfCancellationRequested();

        if (SelectedFirma != null)
            await MaliDonemList.LoadAsync(new MaliDonemListArgs { FirmaId = SelectedFirma.Id }, silent: true);
        else
            await MaliDonemList.LoadAsync(MaliDonemListArgs.CreateEmpty(), silent: true);
        AyarlarVM.Genel.DonemSayisi = MaliDonemList.ItemsSource?.Count ?? 0;
        ct.ThrowIfCancellationRequested();

        await ArsivVM.RefreshAsync(MaliDonemList.ItemsSource);
        SelectedDonem = MaliDonemList.ItemsSource?.FirstOrDefault(m => m != null && !m.KapaliMi)
            ?? MaliDonemList.ItemsSource?.FirstOrDefault();
        await GenelBakisVM.YenileAsync();
        TazeleSayaclar();
        EsitleListeSecimi();
        // Yetim taraması açılışı bloklamaz: kart kendi ringini gösterir, bitince sayaçlar tazelenir.
        // Not: ConfigureAwait yok — TazeleSayaclar UI thread'inde koşmalı (RPC_E_WRONG_THREAD).
        _ = YetimTaramayiBitirAsync();
    }

    /// <summary>Açılışı bloklamayan yetim taraması: bitince sayaçlar tazelenir.</summary>
    private async Task YetimTaramayiBitirAsync()
    {
        await BilinmeyenVM.TaraAsync();
        TazeleSayaclar();
    }

    /// <summary>Silme/kurtarma/arşiv sonrası tüm panelleri tazele.</summary>
    public async Task RefreshAllAsync()
    {
        long korunanId = SelectedDonem?.Id ?? 0;
        _secimKoruma = true;
        try
        {
            var cts = new CancellationTokenSource(YuklemeTimeout);
            await RefreshCoreAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            NotificationService.ShowTagged("Tazeleme Zaman Aşımı",
                "Dönem listesi zamanında tazelenemedi; mevcut veriler gösterilmektedir.",
                NotificationType.Warning,
                "Tazeleme", NotificationGroups.Yonetim);
        }
        finally
        {
            _secimKoruma = false;
        }
        // Koruma sonrası son söz: kuyruktaki geç null yazımı seçimi düşürdüyse geri al.
        if (SelectedDonem == null && korunanId > 0)
        {
            SelectedDonem = MaliDonemList.ItemsSource?.FirstOrDefault(m => m != null && m.Id == korunanId)
                ?? MaliDonemList.ItemsSource?.FirstOrDefault();
        }
    }

    private async Task RefreshCoreAsync(CancellationToken ct)
    {
        await AcikSayfaBoyutunuYukleAsync();
        await MaliDonemList.RefreshAsync();
        AyarlarVM.Genel.DonemSayisi = MaliDonemList.ItemsSource?.Count ?? 0;
        ct.ThrowIfCancellationRequested();

        await ArsivVM.RefreshAsync(MaliDonemList.ItemsSource);
        await GenelBakisVM.YenileAsync();
        ct.ThrowIfCancellationRequested();

        if (SelectedDonem != null)
        {
            var guncel = MaliDonemList.ItemsSource?.FirstOrDefault(m => m != null && m.Id == SelectedDonem.Id);
            if (guncel == null)
            {
                SelectedDonem = MaliDonemList.ItemsSource?.FirstOrDefault();
            }
            else if (!ReferenceEquals(guncel, SelectedDonem))
            {
                SelectedDonem = guncel;
            }
            else
            {
                await DonemDegistiAsync();
            }
        }
        // Bilinmeyen paneli kalıcı görünür (sekmeler Oturum 106'da kalktı) — her tazede tara.
        await BilinmeyenVM.TaraAsync();
        TazeleSayaclar();
        EsitleListeSecimi();
    }

    /// <summary>Ayar dialogu kapandıktan sonra hafif tazeleme: sayfa boyutları
    /// sağlayıcılardan yeniden okunur (RefreshAll + seçili dönem yedek listesi).</summary>
    public async Task AyarSonrasiTazeleAsync()
    {
        await RefreshAllAsync();
        if (SelectedDonem != null)
        {
            YedeklerVM.BagliDonem = SelectedDonem;
            await YedeklerVM.YukleAsync(SelectedDonem.DatabaseName);
        }
        TazeleSayaclar();
    }

    /// <summary>
    /// Paylaşılan listenin SelectedItem'ını sayfa seçimiyle eşitler (tablo yıl rozeti
    /// desync olmasın). Liste yenilemesi ilk açık dönemi seçer; sayfa başka dönemdeyse geri alınır.
    /// </summary>
    private void EsitleListeSecimi()
    {
        if (SelectedDonem == null || MaliDonemList?.ItemsSource == null)
            return;
        var pick = MaliDonemList.ItemsSource.FirstOrDefault(m => m != null && m.Id == SelectedDonem.Id);
        if (pick != null && !ReferenceEquals(MaliDonemList.SelectedItem, pick))
            MaliDonemList.SelectedItem = pick;
    }

    /// <summary>Sekme/segment sayaç başlıkları (Tümü + Arşiv).</summary>
    public void TazeleSayaclar()
    {
        TenantDbSayisi = MaliDonemList.ItemsSource?.Count ?? 0;
        ArsivSekmeSayisi = (ArsivVM.ArsivliDonemler?.Count ?? 0) + (BilinmeyenVM.BilinmeyenYedekler?.Count ?? 0);
        AcikDonemler = MaliDonemList.ItemsSource?.Where(m => m != null && !m.ArsivliMi).OrderByDescending(m => m.MaliYil).ToList()
            ?? new List<MaliDonemModel>();
        NotifyPropertyChanged(nameof(ArsivGrubuVarMi));
        NotifyPropertyChanged(nameof(IsDonemListesiBos));
        NotifyPropertyChanged(nameof(FirmaOzet));
        SecimiListeyeYenidenDuyur();
    }

    /// <summary>Sol liste ItemsSource'u yenilendiğinde ListView iç seçimi düşer; TwoWay
    /// null-yazımı setter guard'ı ile engellenir ama kontrolde görsel seçim kaybolur.
    /// Seçimi yeni listedeki aynı Id'li örneğe eşitleyip binding'e tekrar duyurur.</summary>
    private void SecimiListeyeYenidenDuyur()
    {
        if (_selectedDonem == null)
            return;
        var pick = AcikDonemler?.FirstOrDefault(m => m != null && m.Id == _selectedDonem.Id)
            ?? ArsivVM?.ArsivliDonemler?.FirstOrDefault(m => m != null && m.Id == _selectedDonem.Id);
        if (pick == null)
            return;
        if (!ReferenceEquals(pick, _selectedDonem))
            SelectedDonem = pick;
        else
            NotifyPropertyChanged(nameof(SelectedDonem));
    }

    private System.Windows.Input.ICommand _yardimCommand;

    /// <summary>Kural 13: sayfa yardımı (içerik ViewModel'de, dialog chrome'u App'te).</summary>
    public System.Windows.Input.ICommand YardimCommand => _yardimCommand ??= new AsyncRelayCommand(YardimGoster);

    internal const string YardimAnahtari = "MaliDonemYonetim";
    internal const string YardimBasligi = "Mali Dönem Yönetimi — Yardım";

    /// <summary>Kural 13 içeriği (? yardım dialogu — AI bilgi tabanı artık `docs/yardim/*.md`).</summary>
    internal static List<YardimMaddesiDto> YardimMaddeleri() => new()
    {
        new() { Baslik = "Bu pencere ne işe yarar?", Aciklama = "Seçili firmanın mali dönemlerini yönetir: yedekleme, bakım, analiz, arşivleme ve silme. Soldan dönem seçin; sağdaki kartlar ve yedek listesi o döneme bağlanır." },
        new() { Baslik = "Sol liste — dönem seçimi", Aciklama = "Açık dönemler listelenir; arşivli dönemler varsa ayrı bölümde görünür. Bir döneme tıklamak sağ içeriği tamamen o döneme geçirir." },
        new() { Baslik = "Özet ve toplu işlemler", Aciklama = "'Şimdi Yedekle' seçili dönemin yedeğini alır. Toplu Yedekle/Test/Bakım butonları tüm dönemlere sırayla uygulanır; sonuç özet bildirimle gelir." },
        new() { Baslik = "Yedekler", Aciklama = "Yedek listesi en yeniden eskiye sıralanır. 'Geri Yükle' tek kapıdan geçer: yedek önce analiz edilir (bozuk dosya / yeni sürüm / kayıp kayıt) ve kayıp varsa 6 haneli onay kodu istenir. Geri yükleme sonrası mevcut veri değişir." },
        new() { Baslik = "Bakım ve analiz", Aciklama = "VACUUM/REINDEX/WAL seçili döneme uygulanır. 'Derin Analiz Çalıştır' tablo bazlı sağlık ve anomali raporu üretir." },
        new() { Baslik = "Arşiv ve silme", Aciklama = "Arşivle dönemi kapatır (girişe kapanır, veri korunur; Arşivden Çıkar ile geri açılır). Silme kalıcıdır ve yazılı onay ister — yedek alınmadan yapılmaz." },
        new() { Baslik = "Ayarlar", Aciklama = "Bu firmaya özel liste sayfası boyutu ve yedek saklama sayısı buradan değiştirilir." },
    };

    private async Task YardimGoster()
    {
        await DialogService.ShowYardimAsync(YardimBasligi, YardimMaddeleri());
    }

    public void Subscribe()
    {
        MaliDonemList.Subscribe();
        YedeklerVM.Subscribe();
        BilinmeyenVM.Subscribe();
    }

    public void Unsubscribe()
    {
        MaliDonemList.Unsubscribe();
        YedeklerVM.Unsubscribe();
        BilinmeyenVM.Unsubscribe();
    }

    private async Task DonemDegistiAsync()
    {
        var donem = SelectedDonem;
        if (donem == null)
            return;
        DerinAnaliz = null;
        SonBakimMesaji = string.Empty;
        await MaliDonemList.AnalyzeDbStatusAsync(donem);
        YedeklerVM.BagliDonem = donem;
        await YedeklerVM.YukleAsync(donem.DatabaseName);
        donem.TenantDetails ??= new Business.ResultModels.TenantResultModels.TenantDetailsModel();
        donem.DbYedekSayisi = YedeklerVM.Yedekler.Count;
        donem.DbYedekVarMi = donem.DbYedekSayisi > 0;
        donem.RefreshVitrin();
        TazeleSayaclar();
    }
}
