using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Windows.Input;


namespace MuhasibPro.ViewModels.ViewModels.Settings
{
    /// <summary>Uygulama güncelleme orkestratörü (Denetim Güncelleme bölümü bunu host'lar).
    /// Tek cümle: durum makinesi + komutlar + bildirimleri yönetir, servis/ayar/metin işini composition'a devreder.</summary>
    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly UpdateCheckCoordinator _koordinator;
        private readonly UpdateSettingsStore _magaza;
        private readonly IEventBus _eventBus;
        private UpdateSettingsModel _settings = new();

        #region Constructor

        public UpdateViewModel(IUpdateService updateService, ICommonServices commonServices, IEventBus eventBus = null!) : base(commonServices)
        {
            _koordinator = new UpdateCheckCoordinator(updateService);
            _magaza = new UpdateSettingsStore(updateService, eventBus);
            _eventBus = eventBus;
        }

        #endregion

        #region Properties

        private UpdateState _currentState = UpdateState.Idle;
        public UpdateState CurrentState
        {
            get => _currentState;
            set
            {
                if (Set(ref _currentState, value))
                {
                    _ = Task.Run(async () => await UpdateUIProperties());
                }
            }
        }


        private int _progressPercentage;
        public int ProgressPercentage
        {
            get => _progressPercentage;
            set => Set(ref _progressPercentage, value);
        }

        private string _progressText = string.Empty;
        public string ProgressText
        {
            get => _progressText;
            set => Set(ref _progressText, value);
        }

        private string _progressDetails = string.Empty;
        public string ProgressDetails
        {
            get => _progressDetails;
            set => Set(ref _progressDetails, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (Set(ref _errorMessage, value))
                    NotifyPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private string _lastCheckText = "Son denetleme: Hiçbir zaman";
        public string LastCheckText
        {
            get => _lastCheckText;
            set => Set(ref _lastCheckText, value);
        }

        public bool AutoCheckEnabled
        {
            get => Settings?.AutoCheckOnStartup == true;
            set
            {
                if (Settings != null && Settings.AutoCheckOnStartup != value)
                {
                    Settings.AutoCheckOnStartup = value;
                    NotifyPropertyChanged(nameof(AutoCheckEnabled));
                    _ = SaveSettingsAsync();
                }
            }
        }

        public bool NotificationsEnabled
        {
            get => Settings?.ShowNotifications == true;
            set
            {
                if (Settings != null && Settings.ShowNotifications != value)
                {
                    Settings.ShowNotifications = value;
                    NotifyPropertyChanged(nameof(NotificationsEnabled));
                    _ = SaveSettingsAsync();
                }
            }
        }

        public bool IncludeBetaVersionsEnabled
        {
            get => Settings?.IncludeBetaVersions == true;
            set
            {
                if (Settings != null && Settings.IncludeBetaVersions != value)
                {
                    Settings.IncludeBetaVersions = value;
                    NotifyPropertyChanged(nameof(IncludeBetaVersionsEnabled));
                    _ = SaveSettingsAsync();
                }
            }
        }

        public string FeedUrl
        {
            get => Settings?.FeedUrl ?? string.Empty;
            set
            {
                if (Settings != null && Settings.FeedUrl != value?.Trim())
                {
                    Settings.FeedUrl = value?.Trim() ?? string.Empty;
                    NotifyPropertyChanged(nameof(FeedUrl));
                    _ = SaveSettingsAsync();
                }
            }
        }

        private bool _progressVisible;
        public bool ProgressVisible { get => _progressVisible; set => Set(ref _progressVisible, value); }

        private bool _detailsVisible;
        public bool DetailsVisible { get => _detailsVisible; set => Set(ref _detailsVisible, value); }

        private bool _updateCardVisible;
        public bool UpdateCardVisible { get => _updateCardVisible; set => Set(ref _updateCardVisible, value); }

        public UpdateSettingsModel Settings
        {
            get => _settings;
            set
            {
                if (Set(ref _settings, value))
                {
                    NotifyPropertyChanged(nameof(AutoCheckEnabled));
                    NotifyPropertyChanged(nameof(NotificationsEnabled));
                    NotifyPropertyChanged(nameof(IncludeBetaVersionsEnabled));
                    NotifyPropertyChanged(nameof(FeedUrl));
                }
            }
        }

        public string StatusText => UpdateUiMetinleri.GetStatusText(CurrentState);
        public string VersionText => UpdateUiMetinleri.GetVersionText(CurrentState, SurumMetni());
        public string UpdateButtonText => UpdateUiMetinleri.GetButtonText(CurrentState);
        public bool IsUpdateButtonEnabled => UpdateUiMetinleri.GetButtonEnabled(CurrentState);
        public bool IsCheckButtonEnabled => CurrentState != UpdateState.Checking;

        public string StatusIconGlyph => UpdateUiMetinleri.GetStatusIcon(CurrentState);

        public string UpdateSize => _koordinator.CurrentUpdateInfo?.TargetFullRelease.Size.ToString("N0") + " bytes" ?? string.Empty;
        public string ReleaseDate => _koordinator.CurrentUpdateInfo?.TargetFullRelease.Version.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")) ?? string.Empty;

        public string ChangelogUrl
            => UpdateUiMetinleri.GetChangelogUrl(Settings?.FeedUrl, SurumMetni());

        private string SurumMetni()
            => _koordinator.CurrentUpdateInfo?.TargetFullRelease.Version.ToString();

        #endregion



        #region Initialization

        public async Task InitializeAsync()
        {
            await LoadSettingsAsync();

            await ContextService.RunAsync(() =>
            {
                NotifyPropertyChanged(nameof(AutoCheckEnabled));
                NotifyPropertyChanged(nameof(NotificationsEnabled));
                NotifyPropertyChanged(nameof(IncludeBetaVersionsEnabled));
            });

            await CheckInitialStateAsync();
        }

        private async Task LoadSettingsAsync()
        {
            Settings = await _magaza.LoadAsync();
            UpdateLastCheckText();
        }

        private async Task CheckInitialStateAsync()
        {
            try
            {
                if (await _koordinator.IsRestartPendingAsync())
                {
                    CurrentState = UpdateState.RestartRequired;
                    ProgressText = "Yeniden başlatma bekleniyor";
                    return;
                }

                if (AutoCheckEnabled && !string.IsNullOrWhiteSpace(Settings?.FeedUrl))
                {
                    await CheckForUpdatesAsync();
                }
                else
                {
                    CurrentState = UpdateState.Idle;
                    if (string.IsNullOrWhiteSpace(Settings?.FeedUrl))
                        ProgressText = "Güncelleme kaynağı ayarlanmadı — Ayarlar sekmesinden adres girin";
                }
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
            }
        }



        #endregion

        #region Commands

        public ICommand CheckNowCommand => new AsyncRelayCommand(CheckNowAsync, () => IsCheckButtonEnabled);
        public ICommand UpdateActionCommand => new AsyncRelayCommand(UpdateActionAsync);




        #endregion

        #region Command Implementations

        private async Task CheckNowAsync()
        {
            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Settings?.FeedUrl))
                {
                    CurrentState = UpdateState.Idle;
                    ProgressText = "Güncelleme kaynağı ayarlanmadı — Ayarlar sekmesinden adres girin";
                    UpdateLastCheckText();
                    return;
                }

                CurrentState = UpdateState.Checking;
                ProgressText = "Güncelleştirmeler kontrol ediliyor...";
                ErrorMessage = string.Empty;

                var updateInfo = await _koordinator.CheckAsync(IncludeBetaVersionsEnabled);

                if (updateInfo != null)
                {
                    CurrentState = UpdateState.UpdateAvailable;
                    ProgressText = "Güncelleme indirilebilir";
                }
                else
                {
                    CurrentState = UpdateState.Idle;
                    ProgressText = string.Empty;
                }

                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
                ProgressText = "Kontrol başarısız";
            }
        }

        private async Task UpdateActionAsync()
        {
            try
            {
                switch (CurrentState)
                {
                    case UpdateState.Idle:
                    case UpdateState.Error:
                        await CheckForUpdatesAsync();
                        break;

                    case UpdateState.UpdateAvailable:
                        await DownloadUpdateAsync();
                        break;

                    case UpdateState.Downloaded:
                        InstallUpdate();
                        break;
                }
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
            }
        }

        private async Task DownloadUpdateAsync()
        {
            try
            {
                if (_koordinator.CurrentUpdateInfo == null) return;

                CurrentState = UpdateState.Downloading;
                ProgressText = "İndiriliyor...";
                ProgressPercentage = 0;

                var progress = new Progress<int>(async percentage =>
                {
                    await ContextService.RunAsync(() =>
                    {
                        ProgressPercentage = percentage;
                        ProgressDetails = $"{percentage}% tamamlandı";
                    });
                });

                await _koordinator.DownloadAsync(progress);

                CurrentState = UpdateState.Downloaded;
                ProgressText = "Güncelleme kurulmaya hazır";
                ProgressPercentage = 100;
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
                ProgressText = "İndirme başarısız";
            }
        }

        private void InstallUpdate()
        {
            try
            {
                if (_koordinator.CurrentUpdateInfo == null) return;

                CurrentState = UpdateState.Installing;
                ProgressText = "Uygulama yeniden başlatılıyor...";

                _koordinator.Apply();
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
                ProgressText = "Kurulum başarısız";
            }
        }



        #endregion

        #region Settings


        private async Task SaveSettingsAsync()
        {
            await _magaza.SaveAsync(Settings, this);
        }

        #endregion

        #region UI Helpers

        private async Task UpdateUIProperties()
        {
            await ContextService.RunAsync(() =>
            {
                ProgressVisible = UpdateUiMetinleri.IsProgressVisible(CurrentState);
                DetailsVisible = UpdateUiMetinleri.ShouldShowDetails(CurrentState);
                UpdateCardVisible = UpdateUiMetinleri.ShouldShowUpdateCard(CurrentState);
                NotifyPropertyChanged(nameof(StatusText));
                NotifyPropertyChanged(nameof(VersionText));
                NotifyPropertyChanged(nameof(UpdateButtonText));
                NotifyPropertyChanged(nameof(IsUpdateButtonEnabled));
                NotifyPropertyChanged(nameof(IsCheckButtonEnabled));
                NotifyPropertyChanged(nameof(StatusIconGlyph));
                NotifyPropertyChanged(nameof(UpdateSize));
                NotifyPropertyChanged(nameof(ReleaseDate));
                NotifyPropertyChanged(nameof(ChangelogUrl));
                NotifyPropertyChanged(nameof(FeedUrl));
                NotifyPropertyChanged(nameof(HasError));
                NotifyPropertyChanged(nameof(ProgressVisible));
                NotifyPropertyChanged(nameof(DetailsVisible));
                NotifyPropertyChanged(nameof(UpdateCardVisible));
            });
        }

        private void UpdateLastCheckText()
        {
            LastCheckText = UpdateUiMetinleri.FormatLastCheckText(Settings?.LastCheckTime);
        }

        #endregion

        #region Cleanup

        public void Unsubscribe()
        {
            _eventBus?.Unsubscribe(this);
        }

        #endregion
    }
}
