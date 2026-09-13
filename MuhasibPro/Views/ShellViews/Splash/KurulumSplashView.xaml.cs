using MuhasibPro.Domain.Helpers;
using MuhasibPro.Extensions;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.ServiceExtensions;
using MuhasibPro.Services.UIService;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem;
using System.Diagnostics;

namespace MuhasibPro.Views.ShellViews.Splash;

/// <summary>
/// Ilk kurulum splash ekrani: DB yok senaryosunda otomatik DB olusturma.
/// Basarili olunca Login'e, basarisiz olunca hata gosterip tekrar deneme imkani.
/// </summary>
public sealed partial class KurulumSplashView : Page
{
    public KurulumSplashView()
    {
        ViewModel = ServiceLocator.Current.GetService<KurulumSplashViewModel>();
        InitializeComponent();
    }

    public KurulumSplashViewModel ViewModel { get; }

    public string Version => ProcessInfoHelper.Version;

    public string CopyrightText
    {
        get
        {
            int currentYear = DateTime.Now.Year;
            return $"\u00a9 {currentYear} {ProcessInfoHelper.ProductName}";
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try { Helpers.TitleBarHelper.UpdateTitleBar(ElementTheme.Light); } catch { }

        var nav = ServiceLocator.Current.GetService<Business.Contracts.UIServices.CommonServices.INavigationService>();
        if (nav != null && Frame != null)
            nav.Initialize(Frame);

        var contextService = ServiceLocator.Current.GetService<Business.Contracts.UIServices.CommonServices.IContextService>();
        contextService?.InitializeWithContext(dispatcher: DispatcherQueue, viewElement: this);
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        TryAttachCardShadow();
        CardEntranceStoryboard.Begin();
        _ = StatusControl.ShowMessageAsync("Ilk kurulum baslatiliyor...");

        await RunSetupAsync();
    }

    private async Task RunSetupAsync()
    {
        bool success;
        try
        {
            // Durum mesajlarini canli guncelle
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            success = await ViewModel.RunSetupAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"KurulumSplash setup error: {ex.Message}");
            success = false;
        }
        finally
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (success)
        {
            _ = StatusControl.ShowMessageAsync("Giris ekranina yonlendiriliyor...");
            await Task.Delay(800);
            await NavigateToLoginAsync();
        }
        else
        {
            // Hata panelini goster
            ErrorPanel.Visibility = Visibility.Visible;
        }
    }

    private async void OnRetryClick(object sender, RoutedEventArgs e)
    {
        ErrorPanel.Visibility = Visibility.Collapsed;
        _ = StatusControl.ShowMessageAsync("Tekrar deneniyor...");
        await RunSetupAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.StatusMessage))
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await StatusControl.ShowMessageAsync(ViewModel.StatusMessage);
            });
        }
    }

    private async Task NavigateToLoginAsync()
    {
        try
        {
            if (Helpers.WindowHelpers.WindowHelper.MainWindow is not MainWindow mainWindow)
                return;

            await mainWindow.DispatcherQueue.EnqueueAsync(() =>
            {
                var activationInfo = ActivationInfo.CreateDefault();
                var targetArgs = new ShellArgs
                {
                    ViewModel = activationInfo.EntryViewModel,
                    Parameter = activationInfo.EntryArgs,
                    UserInfo = Business.DTOModel.SistemModel.HesapModel.Default
                };
                bool ok = mainWindow.MainFrame.Navigate(typeof(Login.LoginView), targetArgs);
                Debug.WriteLine($"KurulumSplash -> LoginView ok={ok}");

                var themeSelectorService = ServiceLocator.Current.GetService<Contracts.UIService.IThemeSelectorService>();
                if (mainWindow.MainFrame is FrameworkElement element)
                {
                    element.RequestedTheme = themeSelectorService.Theme;
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"KurulumSplash navigate error: {ex.Message}");
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
            Debug.WriteLine($"KurulumSplash shadow: {ex.Message}");
        }
    }
}
