using Microsoft.UI.Dispatching;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Domain.Enum;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.ShellViews.Shell
{
    public sealed partial class ShellStatusBar : UserControl, IDisposable, INotifyPropertyChanged
    {
        private readonly IStatusMessageService _messageService;
        private readonly IStatusBarService _statusBarService;
        private DispatcherTimer _timer;

        public ShellStatusBar()
        {
            InitializeComponent();

            _messageService = ServiceLocator.Current.GetService<IStatusMessageService>();
            _statusBarService = ServiceLocator.Current.GetService<IStatusBarService>();

            _statusBarService.PropertyChanged += OnServicePropertyChanged;
            _messageService.PropertyChanged += OnServicePropertyChanged;

            DataContext = this;
            if (AsistanPaneli != null)
                AsistanPaneli.GizleIstek += OnAsistanGizleIstek;
            InitializeTimer();
            Unloaded += (_, _) => Dispose();
        }

        #region Status message projections
        public string StatusMessage => _messageService.StatusMessage;
        public bool IsProgressVisible => _messageService.IsProgressVisible;
        public bool IsProgressIndeterminate => _messageService.IsProgressIndeterminate;
        public double ProgressValue => _messageService.ProgressValue;
        public bool ShowProgressBar => _messageService.ShowProgressBar;
        public string ProgressText => _messageService.ProgressText;

        public bool IsInfo => _messageService.MessageType == StatusMessageType.Info;
        public bool IsSuccess => _messageService.MessageType == StatusMessageType.Success;
        public bool IsWarning => _messageService.MessageType is StatusMessageType.Warning or StatusMessageType.Deleting;
        public bool IsError => _messageService.MessageType == StatusMessageType.Error;
        #endregion

        #region Status bar service projections
        public string UserName => _statusBarService.UserName;
        public string DatabaseConnectionMessage => _statusBarService.DatabaseConnectionMessage;
        public bool IsDatabaseConnection => _statusBarService.IsSistemDatabaseConnection;

        // Bağlam (firma/dönem/tenant) yalnız workspace host'unda (MainShell) gösterilir;
        // Ayarlar/Yeni Firma gibi yönetim pencereleri (ShellView) göstermez.
        private bool _baglamGoster;
        public bool BaglamGoster
        {
            get => _baglamGoster;
            set
            {
                if (_baglamGoster == value) return;
                _baglamGoster = value;
                NotifyPropertyChanged(nameof(FirmaAdi));
                NotifyPropertyChanged(nameof(MaliDonemAdi));
                NotifyPropertyChanged(nameof(IsTenantDatabaseConnection));
            }
        }

        public string FirmaAdi => BaglamGoster ? _statusBarService.FirmaAdi : string.Empty;
        public string MaliDonemAdi => BaglamGoster ? _statusBarService.MaliDonemAdi : string.Empty;
        public bool IsTenantDatabaseConnection => BaglamGoster && _statusBarService.IsTenantDatabaseConnection;
        public string TenantDatabaseMessage => _statusBarService.TenantDatabaseMessage;
        #endregion

        #region Faz 6.92: AI asistanı göster/gizle (drawer)
        /// <summary>AI toggle yalnız workspace host'unda (MainShell) görünür.</summary>
        private bool _asistanGorunur;
        public bool AsistanGorunur
        {
            get => _asistanGorunur;
            set
            {
                if (_asistanGorunur == value) return;
                _asistanGorunur = value;
                NotifyPropertyChanged(nameof(AsistanGorunur));
            }
        }

        /// <summary>Çekmece açık mı — toggle görsel durumu (accent/temel).</summary>
        private bool _asistanAcik;
        public bool AsistanAcik
        {
            get => _asistanAcik;
            set
            {
                if (_asistanAcik == value) return;
                _asistanAcik = value;
                NotifyPropertyChanged(nameof(AsistanAcik));
            }
        }

        /// <summary>Asistan VM'i (MainShellView atar) — Flyout içeriğinin DataContext'i.</summary>
        private AsistanSohbetViewModel? _asistanVm;
        public AsistanSohbetViewModel? AsistanVm
        {
            get => _asistanVm;
            set
            {
                _asistanVm = value;
                if (AsistanPaneli != null)
                    AsistanPaneli.DataContext = value;
            }
        }

        private async void OnAsistanFlyoutOpened(object sender, object e)
        {
            AsistanAcik = true;
            // Model durumu "hazır" ⇔ cevaplayabilir olsun: panel açılışında model yüklüyse tazelenir,
            // indirilmişse ön-yüklenir (PanelAcildiAsync fırlatmaz; hata durumu kendi içinde yönetir).
            if (AsistanVm != null)
            {
                try { await AsistanVm.PanelAcildiAsync(); }
                catch { /* durum metni VM içinde yönetilir */ }
            }
        }

        private void OnAsistanFlyoutClosed(object sender, object e) => AsistanAcik = false;

        /// <summary>Panel başlığındaki "gizle" düğmesi flyout'u kapatır.</summary>
        private void OnAsistanGizleIstek(object sender, System.EventArgs e)
        {
            try { AsistanFlyout?.Hide(); } catch { }
        }

        /// <summary>F1 / statü çubuğu "Asistan": AI yardım panelini açar (tek yardım yüzeyi — Kural 13).</summary>
        public void AsistanPaneliAc()
        {
            if (!AsistanGorunur || AsistanFlyout == null || AsistanButton == null)
                return;
            try { AsistanFlyout.ShowAt(AsistanButton); } catch { }
        }
        #endregion

        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                NotifyPropertyChanged(e.PropertyName);

                switch (e.PropertyName)
                {
                    case nameof(IStatusMessageService.MessageType):
                        NotifyPropertyChanged(nameof(IsInfo));
                        NotifyPropertyChanged(nameof(IsSuccess));
                        NotifyPropertyChanged(nameof(IsWarning));
                        NotifyPropertyChanged(nameof(IsError));
                        break;

                    case nameof(IStatusMessageService.IsProgressVisible):
                    case nameof(IStatusMessageService.IsProgressIndeterminate):
                    case nameof(IStatusMessageService.ProgressValue):
                        NotifyPropertyChanged(nameof(ShowProgressBar));
                        NotifyPropertyChanged(nameof(ProgressText));
                        break;

                    case nameof(IStatusBarService.IsSistemDatabaseConnection):
                        NotifyPropertyChanged(nameof(IsDatabaseConnection));
                        break;
                }
            });
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) => TimeDisplay.Text = DateTime.Now.ToString("HH:mm:ss");
            _timer.Start();
            TimeDisplay.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        public void Dispose()
        {
            _statusBarService.PropertyChanged -= OnServicePropertyChanged;
            _messageService.PropertyChanged -= OnServicePropertyChanged;
            _timer?.Stop();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
