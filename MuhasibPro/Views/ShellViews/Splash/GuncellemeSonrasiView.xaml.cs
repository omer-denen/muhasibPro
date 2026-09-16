using System.Diagnostics;
using MuhasibPro.Domain.Helpers;
using MuhasibPro.Extensions;
using MuhasibPro.Helpers;
using MuhasibPro.Helpers.WindowHelpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.ServiceExtensions;
using MuhasibPro.Services.UIService;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.Views.ShellViews.Splash;

/// <summary>Faz 6.91-D: Uygulama güncellendikten sonra açılışta çalışan doğrulama ekranı (splash ailesi).
/// Doğrulama başarılıysa Login'e; bloklayıcı hatada SistemDbYonetim'e yönlendirir (fail-closed).</summary>
public sealed partial class GuncellemeSonrasiView : Page
{
    private bool _yonlendirildi;

    public GuncellemeSonrasiView()
    {
        ViewModel = ServiceLocator.Current.GetService<GuncellemeSonrasiViewModel>();
        InitializeComponent();
    }

    public GuncellemeSonrasiViewModel ViewModel { get; }

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
        try { TitleBarHelper.UpdateTitleBar(ElementTheme.Light); } catch { }

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
        _ = StatusControl.ShowMessageAsync("Güncelleme sonrası doğrulama başlatılıyor...");

        await RunVerificationAsync();
    }

    private async Task RunVerificationAsync()
    {
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        try
        {
            await ViewModel.CalistirAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GuncellemeSonrasi verification error: {ex.Message}");
        }
        finally
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        // Yalnız temiz sonuçta otomatik Login'e geçilir; uyarıda "Devam Et", uygulama hatasında
        // "Kapat", Sistem.db hatasında "Veritabanı Yönetimi" beklenir (Revizyon 3).
        if (!ViewModel.OtomatikGecis)
            return;

        await Task.Delay(900);
        await NavigateToLoginAsync();
    }

    private async void OnContinueClick(object sender, RoutedEventArgs e) => await NavigateToLoginAsync();

    private async void OnManageClick(object sender, RoutedEventArgs e) => await NavigateToSystemDbManagementAsync();

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewModel.StatusMessage))
            return;

        DispatcherQueue.TryEnqueue(async () => { await StatusControl.ShowMessageAsync(ViewModel.StatusMessage); });
    }

    private async Task NavigateToLoginAsync()
    {
        if (_yonlendirildi) return;
        _yonlendirildi = true;
        await NavAsync(typeof(Login.LoginView), () =>
        {
            var activationInfo = ActivationInfo.CreateDefault();
            return new ShellArgs
            {
                ViewModel = activationInfo.EntryViewModel,
                Parameter = activationInfo.EntryArgs,
                UserInfo = Business.DTOModel.SistemModel.HesapModel.Default
            };
        });
    }

    private async Task NavigateToSystemDbManagementAsync()
    {
        if (_yonlendirildi) return;
        _yonlendirildi = true;
        await NavAsync(typeof(Views.SistemDbYonetim.SistemDbYonetimView), () => new ShellArgs
        {
            ViewModel = typeof(SistemDbYonetimViewModel),
            Parameter = ViewModel.SonucAciklama
        });
    }

    private static async Task NavAsync(Type targetView, Func<ShellArgs> argsFactory)
    {
        try
        {
            if (WindowHelper.MainWindow is not MainWindow mainWindow)
                return;

            await mainWindow.DispatcherQueue.EnqueueAsync(() =>
            {
                bool ok = mainWindow.MainFrame.Navigate(targetView, argsFactory());
                Debug.WriteLine($"GuncellemeSonrasi -> {targetView.Name} ok={ok}");

                var themeSelectorService = ServiceLocator.Current.GetService<Contracts.UIService.IThemeSelectorService>();
                if (mainWindow.MainFrame is FrameworkElement element)
                    element.RequestedTheme = themeSelectorService.Theme;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GuncellemeSonrasi navigate error: {ex.Message}");
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
            Debug.WriteLine($"GuncellemeSonrasi shadow: {ex.Message}");
        }
    }
}
