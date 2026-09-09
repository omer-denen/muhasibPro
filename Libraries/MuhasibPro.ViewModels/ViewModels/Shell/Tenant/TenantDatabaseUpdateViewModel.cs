using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Shell;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    public class TenantDatabaseUpdateArgs
    {
        public string DatabaseName { get; set; } = string.Empty;
        public FirmaModel Firma { get; set; }
        public MaliDonemModel MaliDonem { get; set; }
    }

    /// <summary>Tek güncelleme adımı (Yedek/Göç/Doğrulama/Geri alma) — saf durum taşıyıcı.</summary>
    public class TenantUpdateStep : ObservableObject
    {
        public TenantUpdateStep(string number, string title)
        {
            Number = number;
            Title = title;
        }

        public string Number { get; }
        public string Title { get; }

        private string _detail = "Bekliyor";
        public string Detail { get => _detail; private set => Set(ref _detail, value); }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; private set => Set(ref _isRunning, value); }

        private bool _isDone;
        public bool IsDone { get => _isDone; private set => Set(ref _isDone, value); }

        private bool _isFaulted;
        public bool IsFaulted { get => _isFaulted; private set => Set(ref _isFaulted, value); }

        public bool IsPending => !IsRunning && !IsDone && !IsFaulted;

        public void MarkRunning(string detail)
        {
            Detail = detail;
            IsFaulted = false;
            IsDone = false;
            IsRunning = true;
            NotifyPropertyChanged(nameof(IsPending));
        }

        public void MarkDone(string detail)
        {
            Detail = detail;
            IsRunning = false;
            IsDone = true;
            NotifyPropertyChanged(nameof(IsPending));
        }

        public void MarkFault(string detail)
        {
            Detail = detail;
            IsRunning = false;
            IsFaulted = true;
            NotifyPropertyChanged(nameof(IsPending));
        }
    }

    /// <summary>Tablo değişikliğinin ekrana hazır hali (Expander başlık + kolon grupları).</summary>
    public class TenantTableDisplay : ObservableObject
    {
        public TenantTableDisplay(MuhasibPro.Data.Contracts.Database.Common.Helpers.TenantTableChange change)
        {
            Table = change.Table;
            TitleLine = change.IsCreated ? $"'{change.Table}' tablosu oluşturuldu" : $"'{change.Table}' tablosu güncellemesi";
            Added = new List<string>(change.AddedColumns);
            Updated = new List<string>(change.AlteredColumns);
            Removed = new List<string>(change.RemovedColumns);
        }

        public string Table { get; }
        public string TitleLine { get; }
        public List<string> Added { get; }
        public List<string> Updated { get; }
        public List<string> Removed { get; }
        public bool HasAdded => Added.Count > 0;
        public bool HasUpdated => Updated.Count > 0;
        public bool HasRemoved => Removed.Count > 0;
    }

    /// <summary>
    /// Veritabanı güncelleme sayfası: bilgi + Yedek→Göç→Doğrulama (+otomatik geri alma) + sonuç.
    /// Tüm iş Business servislerinde; burada yalnız akış + ekran durumu.
    /// </summary>
    public class TenantDatabaseUpdateViewModel : ViewModelBase
    {
        private readonly ITenantDatabaseUpdateService _updateService;
        private readonly ITenantSQLiteDatabaseOperationService _operations;
        private readonly ILocalSettingsService _settings;
        private readonly IFirmaWithMaliDonemSelectedService _selectedService;

        private TenantUpdateStep _backupStep;
        private TenantUpdateStep _migrateStep;
        private TenantUpdateStep _validateStep;
        private TenantUpdateStep _restoreStep;

        public TenantDatabaseUpdateViewModel(
            ICommonServices commonServices,
            ITenantDatabaseUpdateService updateService,
            ITenantSQLiteDatabaseOperationService operations,
            ILocalSettingsService settings,
            IFirmaWithMaliDonemSelectedService selectedService) : base(commonServices)
        {
            _updateService = updateService;
            _operations = operations;
            _settings = settings;
            _selectedService = selectedService;

            Steps = new ObservableCollection<TenantUpdateStep>();
            StartUpdateCommand = new AsyncRelayCommand(ExecuteStartAsync, () => CanStart);
            ContinueCommand = new AsyncRelayCommand(ExecuteContinueAsync, () => ShowContinue);
            GoBackCommand = new RelayCommand(ExecuteGoBack);
        }

        public TenantDatabaseUpdateArgs Args { get; private set; } = new();
        public TenantUpdateCheckResult Check { get; private set; } = new();

        public string FirmaUnvani => Args.Firma?.KisaUnvani ?? "—";
        public string DonemYili => Args.MaliDonem != null ? Args.MaliDonem.MaliYil.ToString() : "—";

        public ObservableCollection<TenantUpdateStep> Steps { get; }

        private List<TenantTableDisplay> _tableDisplays = new();
        public List<TenantTableDisplay> TableDisplays
        {
            get => _tableDisplays;
            private set => Set(ref _tableDisplays, value);
        }

        private string _headline = string.Empty;
        public string Headline { get => _headline; private set => Set(ref _headline, value); }

        private bool _checkLoaded;
        public bool CheckLoaded { get => _checkLoaded; private set => Set(ref _checkLoaded, value); }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (Set(ref _isRunning, value))
                    RefreshStartCommand();
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            private set
            {
                if (Set(ref _isCompleted, value))
                {
                    RefreshStartCommand();
                    NotifyPropertyChanged(nameof(ShowContinue));
                    (ContinueCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _restoredFromBackup;
        public bool RestoredFromBackup { get => _restoredFromBackup; private set => Set(ref _restoredFromBackup, value); }

        private string _backupPath = string.Empty;
        public string BackupPath { get => _backupPath; private set => Set(ref _backupPath, value); }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (Set(ref _errorMessage, value))
                    NotifyPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private string _resultMessage = string.Empty;
        public string ResultMessage { get => _resultMessage; private set => Set(ref _resultMessage, value); }

        public bool CanStart => CheckLoaded && !IsRunning && !IsCompleted;
        public bool ShowContinue => IsCompleted && !HasError;

        public ICommand StartUpdateCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand GoBackCommand { get; }

        public async Task LoadAsync(TenantDatabaseUpdateArgs args)
        {
            Args = args ?? new TenantDatabaseUpdateArgs();
            NotifyPropertyChanged(nameof(FirmaUnvani));
            NotifyPropertyChanged(nameof(DonemYili));
            ResetSteps();

            Check = await _updateService.CheckUpdateRequiredAsync(Args.DatabaseName);
            NotifyPropertyChanged(nameof(Check));
            if (!Check.CheckSucceeded || !Check.NeedsUpdate)
            {
                ErrorMessage = "Bu dönem güncel — işlem gerekmiyor.";
                RefreshStartCommand();
                return;
            }

            Headline = Check.Headline;
            TableDisplays = Check.TableChanges.Select(t => new TenantTableDisplay(t)).ToList();
            NotifyPropertyChanged(nameof(TableDisplays));
            CheckLoaded = true;
            RefreshStartCommand();
        }

        private void RefreshStartCommand()
        {
            NotifyPropertyChanged(nameof(CanStart));
            (StartUpdateCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private void ResetSteps()
        {
            Steps.Clear();
            _restoreStep = null;
            _backupStep = new TenantUpdateStep("1", "Yedek");
            _migrateStep = new TenantUpdateStep("2", "Göç");
            _validateStep = new TenantUpdateStep("3", "Doğrulama");
            Steps.Add(_backupStep);
            Steps.Add(_migrateStep);
            Steps.Add(_validateStep);
            ErrorMessage = string.Empty;
            ResultMessage = string.Empty;
            RestoredFromBackup = false;
            BackupPath = string.Empty;
            IsCompleted = false;
        }

        private async Task ExecuteStartAsync()
        {
            if (!CanStart)
                return;
            IsRunning = true;
            ErrorMessage = string.Empty;
            ResultMessage = string.Empty;

            // 1. Güncelleme öncesi yedek (bilinen geri dönüş noktası)
            _backupStep.MarkRunning("VACUUM INTO ile güvenlik yedeği alınıyor...");
            var backup = await _operations.CreateBackupAsync(Args.DatabaseName, DatabaseBackupType.Migration);
            if (backup?.Success != true || backup.Data == null || !backup.Data.IsBackupComleted || string.IsNullOrEmpty(backup.Data.BackupFilePath))
            {
                Fail(_backupStep, "Yedek alınamadı: " + (backup?.Message ?? "bilinmeyen hata"));
                return;
            }
            BackupPath = backup.Data.BackupFilePath;
            _backupStep.MarkDone("Yedek hazır: " + backup.Data.BackupFileName);

            // 2. Göç (yedek-önce-göç + durum yayını servis içinde)
            _migrateStep.MarkRunning("Göçler uygulanıyor...");
            var switched = await _updateService.SwitchAndPublishAsync(Args.DatabaseName, Args.Firma, Args.MaliDonem);
            if (!switched.Success)
            {
                _migrateStep.MarkFault(switched.ErrorMessage);
                await AutoRestoreAsync("Geçiş başarısız: " + switched.ErrorMessage);
                return;
            }
            _migrateStep.MarkDone("Sürüm " + Check.TargetVersion + " uygulandı.");

            // 3. Doğrulama
            _validateStep.MarkRunning("Bağlantı + şema + bekleyen göç kontrolü...");
            if (await _updateService.ValidateAsync(Args.DatabaseName))
            {
                _validateStep.MarkDone("Veritabanı sağlıklı ve güncel.");
                FinishSuccess(Args.MaliDonem.MaliYil + " dönemi güncellendi ve doğrulandı.");
                return;
            }

            _validateStep.MarkFault("Doğrulama geçilemedi.");
            await AutoRestoreAsync("Doğrulama geçilemedi.");
        }

        private async Task AutoRestoreAsync(string reason)
        {
            if (_restoreStep == null)
            {
                _restoreStep = new TenantUpdateStep("4", "Geri alma");
                Steps.Add(_restoreStep);
            }
            _restoreStep.MarkRunning("Güncelleme öncesi yedeğe dönülüyor...");

            var restored = await _operations.RestoreBackupAsync(Args.DatabaseName, BackupPath);
            if (restored?.Success == true && restored.Data != null && restored.Data.IsRestoreSuccess
                && await _updateService.ValidateAsync(Args.DatabaseName))
            {
                _restoreStep.MarkDone("Yedekten geri alındı ve doğrulandı.");
                RestoredFromBackup = true;
                FinishSuccess(reason + " Güncelleme öncesi yedekten geri alındı — veriler korunuyor.");
                return;
            }

            _restoreStep.MarkFault("Geri alma başarısız.");
            Fail(_restoreStep, reason + " Otomatik geri alma da başarısız — Mali Dönem Yönetim → Dönem Yedekleri → Geri Yükle ile manuel alın: " + BackupPath);
        }

        private void FinishSuccess(string message)
        {
            ResultMessage = message;
            IsRunning = false;
            IsCompleted = true;
        }

        private void Fail(TenantUpdateStep step, string message)
        {
            if (step != null && !step.IsFaulted)
                step.MarkFault(message);
            ErrorMessage = message;
            IsRunning = false;
            NotifyPropertyChanged(nameof(CanStart));
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
                ErrorMessage = "İşlem sırasında hata: " + ex.Message;
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
