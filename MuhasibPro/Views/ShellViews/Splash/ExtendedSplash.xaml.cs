using MuhasibPro.Contracts.UIService;
using MuhasibPro.Domain.Helpers;
using MuhasibPro.Extensions;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using System.Diagnostics;

namespace MuhasibPro.Views.ShellViews.Splash
{
    /// <summary>Açılış ekranı: başlatma ilerlemesini gösterir, bitince hedefe (Login/Kurulum) geçer.</summary>
    public sealed partial class ExtendedSplash : Page, INotifyPropertyChanged
    {
        private bool _isProcessingComplete;
        private readonly IStartupApplicationService _startupService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ExtendedSplash()
        {
            InitializeComponent();
            _startupService = ServiceLocator.Current.GetService<IStartupApplicationService>();
            _startupService.ProgressChanged += OnStartupProgressChanged;
            Unloaded += OnPageUnloaded;
        }

        public string Version => ProcessInfoHelper.Version;

        public string CopyrightText
        {
            get
            {
                int currentYear = DateTime.Now.Year;
                return $"© {currentYear} {ProcessInfoHelper.ProductName}";
            }
        }

        private double _progressValue;

        public double ProgressValue
        {
            get => _progressValue;
            private set
            {
                if (Math.Abs(_progressValue - value) > 0.01)
                {
                    _progressValue = Math.Clamp(value, 0, 100);
                    NotifyPropertyChanged(nameof(ProgressValue));
                }
            }
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            _startupService.ProgressChanged -= OnStartupProgressChanged;
            Unloaded -= OnPageUnloaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            TryAttachCardShadow();
            CardEntranceStoryboard.Begin();
            _ = StatusControl.ShowMessageAsync("Başlatılıyor…");

            var success = await _startupService.InitializeAsync();

            if (success)
            {
                await CompleteInitializationAsync();
            }
            else
            {
                await ShowNotificationAsync("Uygulama başlatılamadı", "Hata");
                await Task.Delay(3000);
                Application.Current.Exit();
            }
        }

        private void TryAttachCardShadow()
        {
            try
            {
                CardShadow.Receivers.Add(SplashOverlay);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Splash shadow: {ex.Message}");
            }
        }

        private void OnStartupProgressChanged(object sender, StartupProgressEventArgs e)
        {
            DispatcherQueue.TryEnqueue(
                async () =>
                {
                    await StatusControl.ShowMessageAsync(e.Message);
                    ProgressValue = e.Progress;
                });
        }

        private void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        private async Task CompleteInitializationAsync()
        {
            if (_isProcessingComplete)
                return;
            _isProcessingComplete = true;

            try
            {
                // Son progress mesajı
                await _startupService.BeginStepAsync(
                    StartupStep.Complete,
                    "Uygulama başlatılıyor...",
                    CancellationToken.None);

                await SplashNavigator.NavigateToNextAsync();
                await HideSplash();

                await _startupService.CompleteStepAsync("Uygulama hazır");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Complete initialization error: {ex.Message}");
                await _startupService.FailStepAsync($"Başlatma hatası: {ex.Message}", ex);
                await DispatcherQueue.EnqueueAsync(() => SplashOverlay.Visibility = Visibility.Collapsed);
            }
        }

        private async Task HideSplash()
        {
            try
            {
                await DispatcherQueue.EnqueueAsync(
                    () =>
                    {
                        var fadeAnimation = new DoubleAnimation
                        {
                            From = 1,
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(500)
                        };
                        var storyboard = new Storyboard();
                        Storyboard.SetTarget(fadeAnimation, SplashOverlay);
                        Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                        storyboard.Children.Add(fadeAnimation);
                        storyboard.Completed += (s, e) =>
                        {
                            SplashOverlay.Visibility = Visibility.Collapsed;
                        };
                        storyboard.Begin();
                    });
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Hide splash error: {ex.Message}");
                await DispatcherQueue.EnqueueAsync(() => SplashOverlay.Visibility = Visibility.Collapsed);
            }
        }

        private Task ShowNotificationAsync(string message, string title = "Bilgi")
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            DispatcherQueue.TryEnqueue(
                async () =>
                {
                    try
                    {
                        var dialog = new ContentDialog
                        {
                            Title = title,
                            Content = message,
                            PrimaryButtonText = "Tamam"
                        };
                        await DialogHelper.ShowCenteredAsync(dialog);
                        taskCompletionSource.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Notification failed: {ex.Message}");
                        taskCompletionSource.SetResult(false);
                    }
                });
            return taskCompletionSource.Task;
        }
    }
}
