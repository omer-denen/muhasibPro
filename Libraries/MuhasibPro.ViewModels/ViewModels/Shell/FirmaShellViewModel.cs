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
        private readonly ILocalSettingsService _localSettings;
        private readonly IEventBus _eventBus;

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
            IEventBus eventBus = null) : base(commonServices)
        {
            _selectedService = selectedService;
            _localSettings = localSettingsService;
            _eventBus = eventBus;

            // Önce MaliDonemList oluştur, sonra her iki VM'e paylaştır
            var sharedMaliDonemList = new MaliDonemListViewModel(commonServices, maliDonemService, tenantWorkflowService, eventBus);
            MaliDonemVM = new MaliDonemViewModel(commonServices, maliDonemService, tenantWorkflowService, sharedMaliDonemList);
            FirmalarVM = new FirmalarViewModel(commonServices, firmaService, filePickerService, maliDonemService, sharedMaliDonemList);

            Selection = new TenantSelectionViewModel(commonServices, localSettingsService);
            Progress = new TenantUpdateProgressViewModel();
            UpdateCoordinator = new TenantDatabaseUpdateCoordinator(commonServices, updateService, Progress);

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
                    (DevamEtCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            };
        }

        public ShellArgs ViewModelArgs { get; set; }

        // IMaliDonemListHost + XAML forward'ları (durum Selection'da yaşar)
        public FirmaModel SelectedFirma => Selection.SelectedFirma;
        public bool IsFirmaSelected => Selection.IsFirmaSelected;
        public bool HasSelection => Selection.HasSelection;

        public ICommand DevamEtCommand { get; }

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
            MessageService.Subscribe<TenantDatabaseUpdateViewModel>(this, OnTenantUpdated);
            _eventBus?.Subscribe<TenantUpdateAvailableEvent>(this, OnTenantUpdateAvailable);
            FirmalarVM.Subscribe();
            MaliDonemVM.Subscribe();
        }

        public void BaseUnsubscribe()
        {
            MessageService.Unsubscribe(this);
            _eventBus?.Unsubscribe(this);
            FirmalarVM.Unsubscribe();
            MaliDonemVM.Unsubscribe();
        }

        /// <summary>E1 M1 bacağı: güncelleme saptanınca durum satırı + toast (ShowNotifications kapalıysa sessiz).</summary>
        private async void OnTenantUpdateAvailable(object sender, TenantUpdateAvailableEvent e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.DatabaseName))
                return;
            bool goster = true;
            try
            {
                if (_localSettings != null)
                    goster = (await _localSettings.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey))?.ShowNotifications ?? true;
            }
            catch { /* varsayılan açık korunur */ }
            if (!goster)
                return;
            var mesaj = $"'{e.DatabaseName}' için şema güncellemesi hazır ({e.FromVersion} → {e.ToVersion}).";
            await ContextService.RunAsync(() =>
            {
                StatusActionMessage(mesaj, StatusMessageType.Warning, autoHide: 8);
                NotificationService.Show("Güncelleme Gerekli", mesaj, NotificationType.Warning);
            });
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
                    Selection.SelectMaliDonem(MaliDonemList.SelectedItem);
                    if (Selection.SelectedMaliDonem != null)
                        await UpdateCoordinator.EnsureSwitchedAsync(Selection.SelectedMaliDonem.DatabaseName, Selection.SelectedFirma, Selection.SelectedMaliDonem);
                });
            }
        }

        private async void OnTenantUpdated(TenantDatabaseUpdateViewModel viewModel, string message, object args)
        {
            if (message == TenantEvents.Updated && args is string databaseName
                && Selection.SelectedMaliDonem != null && Selection.SelectedMaliDonem.DatabaseName == databaseName)
            {
                await ContextService.RunAsync(async () => await ExecuteDevamEt());
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
