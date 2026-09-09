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
/// Splash sonrası hedef: DB hazırsa Login, değilse SistemKurulum (frame + tema).
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
            if (!decision.IsDatabaseReady)
            {
                targetView = typeof(Views.SistemKurulum.SistemKurulumView);
                targetArgs = new ShellArgs { ViewModel = typeof(ViewModels.ViewModels.Sistem.SistemKurulumViewModel), Parameter = true };
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

        var check = await routing.CheckTransferAsync();
        if (!check.HasMismatches)
            return;

        var dialog = new Views.ShellViews.Shell.Components.Dialogs.TransferDialog
        {
            CurrentKurulumId = check.CurrentKurulumId,
            CurrentMachineId = check.CurrentMachineId
        };
        dialog.SetMismatches(check.Mismatches);
        await DialogHelper.ShowCenteredAsync(dialog);
    }
}
