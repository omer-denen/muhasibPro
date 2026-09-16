using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Shell.Tenant;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Shell
{
    /// <summary>FirmaShell orkestratörü: composition + navigation. Seçim Selection'da, güncelleme Coordinator'da, ilerleme Progress'te.</summary>
    public class FirmaShellViewModel : ViewModelBase, IMaliDonemListHost
    {
        private readonly IFirmaWithMaliDonemSelectedService _selectedService;

        public FirmalarViewModel FirmalarVM { get; }
        public MaliDonemViewModel MaliDonemVM { get; }
        public TenantSelectionViewModel Selection { get; }
        public TenantUpdateProgressViewModel Progress { get; }
        public TenantDatabaseUpdateCoordinator UpdateCoordinator { get; }

        public FirmaListViewModel FirmaList => FirmalarVM.FirmaList;
        public FirmaDetailsViewModel FirmaDetails => FirmalarVM.FirmaDetails;
        public MaliDonemListViewModel MaliDonemList => MaliDonemVM.MaliDonemList;

        public FirmaShellViewModel(
            ICommonServices commonServices,
            IFirmaService firmaService,
            IFilePickerService filePickerService,
            IMaliDonemService maliDonemService,
            ILocalSettingsService localSettingsService,
            IFirmaWithMaliDonemSelectedService selectedService,
            ITenantSQLiteDatabaseService tenantWorkflowService,
            ITenantDatabaseUpdateService updateService,
            ITenantSQLiteDatabaseOperationService operations,
            IEventBus eventBus = null) : base(commonServices)
        {
            _selectedService = selectedService;

            // Önce MaliDonemList oluştur, sonra her iki VM'e paylaştır
            var sharedMaliDonemList = new MaliDonemListViewModel(commonServices, maliDonemService, tenantWorkflowService, eventBus);
            MaliDonemVM = new MaliDonemViewModel(commonServices, maliDonemService, tenantWorkflowService, sharedMaliDonemList);
            FirmalarVM = new FirmalarViewModel(commonServices, firmaService, filePickerService, maliDonemService, sharedMaliDonemList);

            Selection = new TenantSelectionViewModel(commonServices, localSettingsService);
            Progress = new TenantUpdateProgressViewModel();
            UpdateCoordinator = new TenantDatabaseUpdateCoordinator(commonServices, updateService, operations, Progress);

            DevamEtCommand = new AsyncRelayCommand(ExecuteDevamEt, CanExecuteDevamEt);
            Selection.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TenantSelectionViewModel.HasSelection)
                    || e.PropertyName == nameof(TenantSelectionViewModel.SelectedMaliDonem)
                    || e.PropertyName == nameof(TenantSelectionViewModel.SelectedFirma))
                {
                    NotifyPropertyChanged(nameof(SelectedFirma));
                    NotifyPropertyChanged(nameof(IsFirmaSelected));
                    NotifyPropertyChanged(nameof(HasSelection));
                    NotifyPropertyChanged(nameof(SecimOzeti));
                    NotifyPropertyChanged(nameof(DevamEtNedenTooltip));
                    NotifyPropertyChanged(nameof(BosDonemBaslik));
                    (DevamEtCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            };
            // Toplu analiz bittiğinde dönem durumu (DB yok/güncelleme gerekli) değişir — özeti tazele.
            sharedMaliDonemList.TopluAnalizTamamlandi += () =>
            {
                _ = ContextService.RunAsync(() =>
                {
                    NotifyPropertyChanged(nameof(SecimOzeti));
                    NotifyPropertyChanged(nameof(DevamEtNedenTooltip));
                });
            };
        }

        /// <summary>0-dönem boş-durum kartı başlığı — firma adıyla (Kural 7: hardcoded metin yok).</summary>
        public string BosDonemBaslik
        {
            get
            {
                var firma = Selection.SelectedFirma;
                if (firma == null)
                    return "Bu firmada mali dönem yok";
                return $"{firma.KisaUnvani} firmasında mali dönem yok";
            }
        }

        /// <summary>CTA alt barı solundaki seçim özeti: "Firma • Yıl • durum".</summary>
        public string SecimOzeti
        {
            get
            {
                var firma = Selection.SelectedFirma;
                if (firma == null)
                    return "Firma seçilmedi";
                var donem = Selection.SelectedMaliDonem;
                if (donem == null)
                    return $"{firma.KisaUnvani} • Dönem seçilmedi";
                var durum = donem.DbDosyaYokMu ? "DB yok"
                    : donem.KapaliMi ? "Kapalı"
                    : donem.DbGuncellemeGerekliMi ? "Güncelleme gerekli"
                    : "Açık";
                return $"{firma.KisaUnvani} • {donem.MaliYil} • {durum}";
            }
        }

        /// <summary>"Çalışma Alanına Geç" butonu pasifken nedenini söyleyen tooltip.</summary>
        public string DevamEtNedenTooltip
        {
            get
            {
                if (!Selection.HasSelection)
                    return "Önce firma ve mali dönem seçin";
                var donem = Selection.SelectedMaliDonem;
                if (donem?.DbDosyaYokMu == true)
                    return "Veritabanı dosyası yok — bu döneme girilemez";
                if (donem?.KapaliMi == true)
                    return "Kapalı döneme girilemez — açık bir dönem seçin";
                return "Seçili firma ve dönemle çalışma alanına geç";
            }
        }

        public ShellArgs ViewModelArgs { get; set; }

        // IMaliDonemListHost + XAML forward'ları (durum Selection'da yaşar)
        public FirmaModel SelectedFirma => Selection.SelectedFirma;
        public bool IsFirmaSelected => Selection.IsFirmaSelected;
        public bool HasSelection => Selection.HasSelection;

        public ICommand DevamEtCommand { get; }

        private ICommand _yardimCommand;

        /// <summary>Kural 13: sayfa yardımı (içerik ViewModel'de, dialog chrome'u App'te).</summary>
        public ICommand YardimCommand => _yardimCommand ??= new AsyncRelayCommand(YardimGoster);

        internal const string YardimAnahtari = "FirmaShell";
        internal const string YardimBasligi = "Firma & Mali Dönem Seçimi — Yardım";

        /// <summary>Kural 13 içeriği + Faz 6.92 RAG derlemi (tek kaynak burası; toplayıcı buradan okur).</summary>
        internal static List<YardimMaddesiDto> YardimMaddeleri() => new()
        {
            new() { Baslik = "Firma nasıl seçerim?", Aciklama = "Yukarıdaki firma seçiciye tıklayın; açılan listede firma kodu, ünvan veya şehir yazarak arayabilirsiniz. Bir firmaya tıkladığınızda seçilir ve liste kapanır; firma bilgileri alttaki kartta görünür." },
            new() { Baslik = "Mali dönem nasıl seçerim?", Aciklama = "Sağdaki mali dönem listesinden bir satıra tıklayın; seçilen satır genişleyip dönemin veritabanı, boyut, durum ve son yedek bilgilerini gösterir. Seçim sonrası 'Çalışma Alanına Geç' butonu etkinleşir." },
            new() { Baslik = "Yeni firma eklemek", Aciklama = "FİRMA başlığının sağındaki 'Yeni Firma' butonu firma tanımlama penceresini açar; kayıt sonrası liste tazelenir. Seçili firma kartındaki 'Düzenle' ile mevcut firma bilgileri açılır." },
            new() { Baslik = "Yeni mali dönem açmak", Aciklama = "Mali dönem listesi başlığındaki 'Yeni Mali Dönem Aç' butonu yeni dönem penceresini açar; veritabanı oluşturma adımları sırayla gösterilir. Dönem yoksa boş-durum kartındaki 'Yeni mali dönem aç' bağlantısı da aynı pencereyi açar." },
            new() { Baslik = "Mali Dönem İşlemleri", Aciklama = "Seçili firma kartındaki 'Mali Dönem İşlemleri' butonu yedekleme, arşivleme ve kurtarma işlemlerini içeren yönetim penceresini açar." },
            new() { Baslik = "Güncelleme bildirimi", Aciklama = "Bir dönemin veritabanı şema güncellemesi bekliyorsa listenin üstünde sarı bilgi çubuğu görünür. Tek dönemde 'Güncelle' doğrudan güncelleme sayfasını açar; birden çok dönemde 'İncele' listesinden istediğiniz dönemi seçin." },
            new() { Baslik = "Son çalışılan rozeti", Aciklama = "En son giriş yapılan dönemin satırında 'Son çalışılan' rozeti görünür; liste sırası değişmez. Açılışta bu dönem otomatik seçilir (kapalıysa ilk açık döneme düşülür)." },
            new() { Baslik = "Çalışma alanına geçiş", Aciklama = "Firma ve açık bir dönem seçiliyken alttaki 'Çalışma Alanına Geç' ile ana panele geçilir. Buton pasifse nedenini üzerine gelerek görebilirsiniz (dönem kapalı ya da veritabanı dosyası yok)." },
            new() { Baslik = "Ayarlar", Aciklama = "Sağ üstteki 'Ayarlar' butonu tüm uygulama ayarlarını ayrı bir pencerede açar." },
        };

        private async Task YardimGoster()
        {
            await DialogService.ShowYardimAsync(YardimBasligi, YardimMaddeleri());
        }

        private bool CanExecuteDevamEt()
        {
            var donem = Selection.SelectedMaliDonem;
            if (!Selection.HasSelection) return false;
            if (donem != null && donem.KapaliMi) return false;
            if (donem != null && donem.DbDosyaYokMu) return false;
            return true;
        }

        public async Task LoadAsync(ShellArgs args)
        {
            ViewModelArgs = args;
            try
            {
                await FirmalarVM.LoadAsync(new FirmaListArgs());
                if (FirmaList.Items == null || FirmaList.Items.Count == 0)
                {
                    await Selection.EnsureFirmaExistsAsync();
                    return;
                }
                await Selection.LoadLastSelectionAsync(FirmaList, MaliDonemList);
                MaliDonemList.SonCalisilanDonemId = Selection.SonKayitliDonemId;
                MaliDonemList.UygulaSonCalisilanIsareti();

                // LoadDataAsync otomatik ilk öğeyi seçer ama dispatch henüz çalışmamış olabilir → garantile
                if (Selection.SelectedFirma == null && FirmaList.SelectedItem != null)
                    Selection.SelectFirma(FirmaList.SelectedItem);
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync("Hata", $"Veriler yüklenirken hata: {ex.Message}");
                await LogSistemExceptionAsync("FirmaDonemSelect", "LoadAsync", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void BaseSubscribe()
        {
            MessageService.Subscribe<FirmaListViewModel>(this, OnMessage);
            MessageService.Subscribe<MaliDonemListViewModel>(this, OnMaliDonemMessage);
            FirmalarVM.Subscribe();
            MaliDonemVM.Subscribe();
        }

        public void BaseUnsubscribe()
        {
            MessageService.Unsubscribe(this);
            FirmalarVM.Unsubscribe();
            MaliDonemVM.Unsubscribe();
        }

        private async void OnMessage(FirmaListViewModel viewModel, string message, object args)
        {
            if (viewModel == FirmaList && message == EntityEvents.ItemSelected)
            {
                // FirmalarVM kendi subscriber'ıyla OnItemSelected'i zaten çağırıyor; burada sadece seçim aynalanır.
                await ContextService.RunAsync(() => Selection.SelectFirma(FirmaList.SelectedItem));
            }
        }

        private async void OnMaliDonemMessage(MaliDonemListViewModel viewModel, string message, object args)
        {
            if (viewModel == MaliDonemList && message == EntityEvents.ItemSelected)
            {
                await ContextService.RunAsync(async () =>
                {
                    MaliDonemVM.OnItemSelected();
                    // Aynı dönemin yeniden seçilmesi (liste tazelemesi / flyout açılışı) tenant
                    // geçişini tekrar tetiklemez — koordinatör yalnız dönem gerçekten değişince çalışır.
                    var secili = MaliDonemList.SelectedItem;
                    var onceki = Selection.SelectedMaliDonem;
                    bool ayniDonem = secili != null && onceki != null && secili.Id == onceki.Id;
                    Selection.SelectMaliDonem(secili);
                    if (!ayniDonem && Selection.SelectedMaliDonem != null)
                        await UpdateCoordinator.EnsureSwitchedAsync(Selection.SelectedMaliDonem.DatabaseName, Selection.SelectedFirma, Selection.SelectedMaliDonem);
                });
            }
        }

        private async Task ExecuteDevamEt()
        {
            if (!Selection.HasSelection)
                return;
            var guard = Selection.CheckWorkspaceEntry();
            if (!guard.IsAllowed)
            {
                await DialogService.ShowAsync(guard.Title, guard.Message, "Tamam");
                return;
            }
            try
            {
                await Selection.SaveLastSelectionAsync();
                await LogSistemInformationAsync("FirmaDonemSelect", "Firma ve dönem seçildi", "Seçim",
                    $"{Selection.SelectedFirma.KisaUnvani} - {Selection.SelectedMaliDonem.MaliYil}");

                _selectedService.SelectedFirma = Selection.SelectedFirma;
                _selectedService.SelectedMaliDonem = Selection.SelectedMaliDonem;

                ViewModelArgs.ViewModel = typeof(DashboardViewModel);
                NavigationService.Navigate<MainShellViewModel>(ViewModelArgs);
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync("Hata", $"İşlem sırasında hata: {ex.Message}");
                await LogSistemExceptionAsync("FirmaDonemSelect", "ExecuteDevamEt", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
