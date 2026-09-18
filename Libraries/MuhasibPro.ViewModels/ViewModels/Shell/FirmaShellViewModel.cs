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
        private readonly IPermissionService _yetki;

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
            IEventBus eventBus = null,
            IPermissionService permissionService = null) : base(commonServices)
        {
            _selectedService = selectedService;
            _yetki = permissionService;

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

        private ICommand _firmaYonetimiCommand;

        private bool _firmaYonetimiYetkisi = true;

        /// <summary>K4: 'Firma_Yonet' izni (firma-bağımsız, kullanıcı düzeyinde). Yetkisizse buton pasif + gerekçe.</summary>
        public bool FirmaYonetimiYetkisi
        {
            get => _firmaYonetimiYetkisi;
            private set
            {
                if (Set(ref _firmaYonetimiYetkisi, value))
                    NotifyPropertyChanged(nameof(FirmaYonetimiNedenTooltip));
            }
        }

        public string FirmaYonetimiNedenTooltip => _firmaYonetimiYetkisi
            ? "Firma Yönetimi — firma tanımlama, düzenleme ve mali dönemler (ayrı pencere)"
            : "Firma Yönetimi yetkiniz yok — yöneticinizden 'Firma Yönetimi' izni isteyin.";

        /// <summary>Firma Yönetimi sayfasını açar (tam CRUD: lista + detay + mali dönemler).</summary>
        public ICommand FirmaYonetimiCommand => _firmaYonetimiCommand ??= new AsyncRelayCommand(FirmaYonetimiAc, () => FirmaYonetimiYetkisi);

        private async Task FirmaYonetimiAc()
        {
            if (!FirmaYonetimiYetkisi)
            {
                StatusError("Firma Yönetimi yetkiniz yok.");
                return;
            }

            StatusReady();
            var args = new FirmaListArgs();
            if (IsMainWindow)
                await NavigationService.CreateNewViewAsync<FirmalarViewModel>(args, "Firma Yönetimi");
            else
                NavigationService.Navigate<FirmalarViewModel>(args);
        }

        private async Task FirmaYonetimiYetkisiniYukleAsync()
        {
            if (_yetki == null)
                return;
            try
            {
                FirmaYonetimiYetkisi = await _yetki.KullaniciYetkisiVarMiAsync(Permission.Firma_Yonet);
            }
            catch (Exception ex)
            {
                await LogSistemExceptionAsync("FirmaShell", "FirmaYonetimiYetki", ex);
            }
            (_firmaYonetimiCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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
