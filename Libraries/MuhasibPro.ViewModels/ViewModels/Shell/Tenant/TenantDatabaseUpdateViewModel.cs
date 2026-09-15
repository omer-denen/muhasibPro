using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Shell;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>
    /// Veritabanı güncelleme sayfasının yüzü: bağlam/yükleme durumu, komutlar, navigasyon ve yardım.
    /// Adım motoru <see cref="TenantUpdateAkisYoneticisi"/>'ndedir; bu sınıf yalnız ekranı yönetir.
    /// </summary>
    public class TenantDatabaseUpdateViewModel : ViewModelBase
    {
        private readonly ITenantDatabaseUpdateService _updateService;
        private readonly ILocalSettingsService _settings;
        private readonly IFirmaWithMaliDonemSelectedService _selectedService;

        public TenantDatabaseUpdateViewModel(
            ICommonServices commonServices,
            ITenantDatabaseUpdateService updateService,
            ITenantSQLiteDatabaseOperationService operations,
            ILocalSettingsService settings,
            IFirmaWithMaliDonemSelectedService selectedService) : base(commonServices)
        {
            _updateService = updateService;
            _settings = settings;
            _selectedService = selectedService;
            Akis = new TenantUpdateAkisYoneticisi(updateService, operations);
            Akis.PropertyChanged += OnAkisPropertyChanged;

            StartUpdateCommand = new AsyncRelayCommand(ExecuteStartAsync, () => CanStart);
            ContinueCommand = new AsyncRelayCommand(ExecuteContinueAsync, () => ShowContinue);
            GoBackCommand = new RelayCommand(ExecuteGoBack);
        }

        public TenantDatabaseUpdateArgs Args { get; private set; } = new();
        public TenantUpdateCheckResult Check { get; private set; } = new();

        /// <summary>Adım motoru (Yedek→Göç→Doğrulama + oto geri alma + determinate ilerleme).</summary>
        public TenantUpdateAkisYoneticisi Akis { get; }

        public string FirmaUnvani => Args.Firma?.KisaUnvani ?? "—";
        public string DonemYili => Args.MaliDonem != null ? Args.MaliDonem.MaliYil.ToString() : "—";

        private List<TenantTableDisplay> _tableDisplays = new();
        public List<TenantTableDisplay> TableDisplays
        {
            get => _tableDisplays;
            private set
            {
                if (Set(ref _tableDisplays, value))
                    NotifyPropertyChanged(nameof(HasTableDisplays));
            }
        }

        public bool HasTableDisplays => _tableDisplays.Count > 0;

        private string _headline = string.Empty;
        public string Headline { get => _headline; private set => Set(ref _headline, value); }

        private bool _isChecking;
        public bool IsChecking { get => _isChecking; private set => Set(ref _isChecking, value); }

        private bool _checkYuklendi;
        public bool CheckYuklendi
        {
            get => _checkYuklendi;
            private set
            {
                if (Set(ref _checkYuklendi, value))
                {
                    NotifyPropertyChanged(nameof(IsUpToDate));
                    NotifyPropertyChanged(nameof(IsCheckFailed));
                }
            }
        }

        private bool _checkLoaded;
        public bool CheckLoaded
        {
            get => _checkLoaded;
            private set { if (Set(ref _checkLoaded, value)) RefreshCommands(); }
        }

        private string _checkMessage = string.Empty;
        public string CheckMessage { get => _checkMessage; private set => Set(ref _checkMessage, value); }

        public bool IsUpToDate => CheckYuklendi && !IsChecking && Check.CheckSucceeded && !Check.NeedsUpdate;
        public bool IsCheckFailed => CheckYuklendi && !IsChecking && !Check.CheckSucceeded;

        public bool CanStart => CheckLoaded && !Akis.IsRunning && !Akis.IsCompleted;
        public bool ShowContinue => Akis.IsCompleted && !Akis.HasError;
        public bool ShowSuccess => ShowContinue && !Akis.RestoredFromBackup;
        public bool ShowRestored => ShowContinue && Akis.RestoredFromBackup;

        // Adım motorunun ekrana yansıyan durumu (test/komut erişimi için delege).
        public ObservableCollection<TenantUpdateStep> Steps => Akis.Steps;
        public int ProgressYuzde => Akis.ProgressYuzde;
        public string AktifAdim => Akis.AktifAdim;
        public bool IsRunning => Akis.IsRunning;
        public bool IsCompleted => Akis.IsCompleted;
        public bool RestoredFromBackup => Akis.RestoredFromBackup;
        public string ErrorMessage => Akis.ErrorMessage;
        public bool HasError => Akis.HasError;
        public string ResultMessage => Akis.ResultMessage;

        public ICommand StartUpdateCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand GoBackCommand { get; }

        private ICommand _yardimCommand;

        /// <summary>Kural 13: sayfa yardımı (içerik ViewModel'de, dialog chrome'u App'te).</summary>
        public ICommand YardimCommand => _yardimCommand ??= new AsyncRelayCommand(YardimGoster);

        private async Task YardimGoster()
            => await DialogService.ShowYardimAsync("Veritabanı Güncelleme — Yardım", TenantDatabaseUpdateYardim.Maddeler());

        public async Task LoadAsync(TenantDatabaseUpdateArgs args)
        {
            Args = args ?? new TenantDatabaseUpdateArgs();
            NotifyPropertyChanged(nameof(FirmaUnvani));
            NotifyPropertyChanged(nameof(DonemYili));
            CheckYuklendi = false;
            CheckLoaded = false;
            CheckMessage = string.Empty;
            Akis.Reset();

            IsChecking = true;
            try
            {
                Check = await _updateService.CheckUpdateRequiredAsync(Args.DatabaseName);
            }
            finally
            {
                IsChecking = false;
                CheckYuklendi = true;
            }
            NotifyPropertyChanged(nameof(Check));

            if (!Check.CheckSucceeded)
            {
                CheckMessage = "Veritabanı durumu okunamadı — bağlantıyı kontrol edip tekrar deneyin.";
                RefreshCommands();
                return;
            }
            if (!Check.NeedsUpdate)
            {
                CheckMessage = "Bu dönem güncel — işlem gerekmiyor.";
                RefreshCommands();
                return;
            }

            Headline = Check.Headline;
            TableDisplays = Check.TableChanges.Select(t => new TenantTableDisplay(t)).ToList();
            NotifyPropertyChanged(nameof(TableDisplays));
            CheckLoaded = true;
        }

        private void OnAkisPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.PropertyName))
                NotifyPropertyChanged(e.PropertyName);
            RefreshCommands();
        }

        private void RefreshCommands()
        {
            NotifyPropertyChanged(nameof(CanStart));
            NotifyPropertyChanged(nameof(ShowContinue));
            NotifyPropertyChanged(nameof(ShowSuccess));
            NotifyPropertyChanged(nameof(ShowRestored));
            (StartUpdateCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (ContinueCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task ExecuteStartAsync()
        {
            if (!CanStart)
                return;
            await Akis.RunAsync(Args, Check);
        }

        private async Task ExecuteContinueAsync()
        {
            if (!ShowContinue)
                return;
            try
            {
                await _settings.SaveSettingAsync("LastSelectedFirmaId", Args.Firma.Id);
                await _settings.SaveSettingAsync("LastSelectedDonemId", Args.MaliDonem.Id);
                await LogSistemInformationAsync("FirmaDonemSelect", "Firma ve dönem seçildi", "Seçim",
                    $"{Args.Firma.KisaUnvani} - {Args.MaliDonem.MaliYil}");

                _selectedService.SelectedFirma = Args.Firma;
                _selectedService.SelectedMaliDonem = Args.MaliDonem;

                // Ana pencereye haber ver (FirmaShell kaldığı yerden DevamEt ile MainShell'e geçer), bu pencereyi kapat
                MessageService.Send(this, TenantEvents.Updated, Args.DatabaseName);
                await NavigationService.CloseViewAsync();
            }
            catch (Exception ex)
            {
                await LogSistemExceptionAsync("FirmaDonemSelect", "UpdateContinue", ex);
            }
        }

        private void ExecuteGoBack()
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate<FirmaShellViewModel>();
        }
    }
}
