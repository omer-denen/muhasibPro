using System.Diagnostics;
using MuhasibPro.Extensions;
using MuhasibPro.Helpers.WindowHelpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.UIService;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.Views.ShellViews.Splash;

/// <summary>
/// DEV-ONLY: açılışta bekleyen Sistem.db şema göçlerini otomatik uygulayan ekran (splash ailesi).
/// Başarıda Login'e (yeniden yönlendirme kararı ile), hatada kullanıcıya Tekrar Dene / Veritabanı Yönetimi sunar.
/// </summary>
public sealed partial class SistemMigrationView : Page
{
    private bool _yonlendirildi;

    public SistemMigrationView()
    {
        ViewModel = ServiceLocator.Current.GetService<SistemMigrationViewModel>();
        InitializeComponent();
    }

    public SistemMigrationViewModel ViewModel { get; }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        try { AnaBorderShadow.Receivers.Add(RootLayout); } catch { /* gölge best-effort */ }
        await CalistirAsync();
    }

    private async Task CalistirAsync()
    {
        await ViewModel.CalistirAsync();
        if (!ViewModel.Basarili)
            return;

        await Task.Delay(700);
        await SplashNavigator.NavigateToNextAsync();
    }

    private async void OnRetryClick(object sender, RoutedEventArgs e) => await CalistirAsync();

    private async void OnManageClick(object sender, RoutedEventArgs e)
    {
        if (_yonlendirildi)
            return;
        _yonlendirildi = true;

        try
        {
            if (WindowHelper.MainWindow is not MainWindow mainWindow)
                return;

            await mainWindow.DispatcherQueue.EnqueueAsync(() =>
            {
                bool ok = mainWindow.MainFrame.Navigate(typeof(Views.SistemDbYonetim.SistemDbYonetimView), new ShellArgs
                {
                    ViewModel = typeof(SistemDbYonetimViewModel),
                    Parameter = ViewModel.HataMetni
                });
                Debug.WriteLine($"SistemMigration -> SistemDbYonetim ok={ok}");
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SistemMigration navigate error: {ex.Message}");
        }
    }
}
