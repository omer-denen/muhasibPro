using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Contracts.UIService;
using MuhasibPro.Extensions;
using MuhasibPro.Helpers;
using MuhasibPro.Helpers.WindowHelpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.UIService;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.Views.Login;
using System.Diagnostics;

namespace MuhasibPro.Views.ShellViews.Splash;

/// <summary>
/// Splash sonrası hedef: 3 yol — FirstSetup → KurulumSplash, MigrationRequired → SistemDbYonetim, Login → LoginView.
/// İnce yönlendirici — karar + veri Business'ta (ISplashRoutingService), EF/DbContext bilmez.
/// </summary>
public static class SplashNavigator
{
    public static async Task NavigateToNextAsync()
    {
        try
        {
            var routing = ServiceLocator.Current.GetService<ISplashRoutingService>();
            var startupService = ServiceLocator.Current.GetService<IStartupApplicationService>();

            bool? startupReady = (startupService as StartupApplicationService)?.IsDatabaseReady;
            var decision = routing != null
                ? await routing.DecideRouteAsync(startupReady)
                : new SplashRouteDecision { IsDatabaseReady = startupReady ?? true };

            Type targetView;
            ShellArgs targetArgs;
            if (decision.Target == SplashTarget.FirstSetup)
            {
                // DB yok — otomatik kurulum splash'i
                targetView = typeof(Views.ShellViews.Splash.KurulumSplashView);
                targetArgs = new ShellArgs { ViewModel = typeof(ViewModels.ViewModels.Sistem.KurulumSplashViewModel) };
            }
            else if (decision.Target == SplashTarget.MigrationRequired)
            {
                // DB var ama migration/onarim gerekiyor — karar izi sayfa günlüğüne düşer (Adım 0 tanısı)
                Debug.WriteLine($"Splash karar: {decision.KararOzeti}");
                targetView = typeof(Views.SistemDbYonetim.SistemDbYonetimView);
                targetArgs = new ShellArgs { ViewModel = typeof(ViewModels.ViewModels.Sistem.SistemDbYonetimViewModel), Parameter = decision.KararOzeti };
            }
            else if (decision.Target == SplashTarget.DevMigration)
            {
                // DEV-ONLY: bekleyen göçler açılışta otomatik uygulanır (SistemMigrationView) —
                // manuel Veritabanı Yönetimi ekranına gitmeden. Release'de bu hedef seçilmez.
                Debug.WriteLine($"Splash karar: {decision.KararOzeti}");
                targetView = typeof(Views.ShellViews.Splash.SistemMigrationView);
                targetArgs = new ShellArgs { ViewModel = typeof(ViewModels.ViewModels.Sistem.SistemMigrationViewModel) };
            }
            else if (decision.Target == SplashTarget.PostUpdateVerification)
            {
                // Faz 6.91-D: uygulama güncellendi — açılışta doğrulama sagası (view kendi akışını yürütür).
                Debug.WriteLine($"Splash karar: {decision.KararOzeti}");
                targetView = typeof(Views.ShellViews.Splash.GuncellemeSonrasiView);
                targetArgs = new ShellArgs { ViewModel = typeof(ViewModels.ViewModels.Sistem.GuncellemeSonrasiViewModel) };
            }
            else
            {
                // D1: taşınmış veri kontrolü (best-effort, engellemez)
                try { await ShowTransferIfNeededAsync(routing); } catch (Exception ex) { Debug.WriteLine($"Transfer check: {ex.Message}"); }

                var activationInfo = ActivationInfo.CreateDefault();
                targetView = typeof(LoginView);
                targetArgs = new ShellArgs { ViewModel = activationInfo.EntryViewModel, Parameter = activationInfo.EntryArgs, UserInfo = Business.DTOModel.SistemModel.HesapModel.Default };
            }

            if (WindowHelper.MainWindow is not MainWindow mainWindow)
                return;

            await mainWindow.DispatcherQueue.EnqueueAsync(() =>
            {
                bool ok = mainWindow.MainFrame.Navigate(targetView, targetArgs);
                Debug.WriteLine($"Navigate {targetView.Name} ok={ok}");
                var themeSelectorService = ServiceLocator.Current.GetService<IThemeSelectorService>();
                if (mainWindow.MainFrame is FrameworkElement element)
                {
                    element.RequestedTheme = themeSelectorService.Theme;
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Splash navigation error: {ex.Message}");
        }
    }

    private static async Task ShowTransferIfNeededAsync(ISplashRoutingService routing)
    {
        if (routing == null)
            return;

        // Kimlik kaybı (makine aynı) CheckTransferAsync içinde onarılır; burada yalnız gerçek
        // transfer (başka makine) kalır. Bildirim yalnız geliştirme derlemesinde gösterilir;
        // Release'de gerçek transfer bilgisi geri-yükleme hükmünde (bloklayan akış) çıkar (Kural 18 dev kapısı).
        var check = await routing.CheckTransferAsync();

        if (check.AlignedCount > 0)
            Debug.WriteLine($"Kurulum kimligi onarildi: {check.AlignedCount} donem • benimsenen={check.AdoptedKurulumId}");

#if DEBUG
        if (!check.HasMismatches)
            return;

        var dialog = new Views.ShellViews.Shell.Components.Dialogs.TransferDialog
        {
            CurrentKurulumId = check.CurrentKurulumId,
            CurrentMachineId = check.CurrentMachineId
        };
        dialog.SetMismatches(check.Mismatches);
        await DialogHelper.ShowCenteredAsync(dialog);
#else
        if (check.HasMismatches)
            Debug.WriteLine($"Tasinmis veri: {check.Mismatches.Count} donem (bildirim yalniz DEBUG derlemesinde)");
#endif
    }
}
