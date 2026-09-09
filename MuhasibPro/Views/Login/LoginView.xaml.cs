using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.ServiceExtensions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.Login;

public sealed partial class LoginView : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginView()
    {
        ViewModel = ServiceLocator.Current.GetService<LoginViewModel>();
        this.InitializeComponent();
        this.InitializeContext();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ShellArgs args)
            await ViewModel.LoadAsync(args);
        else
            await ViewModel.LoadAsync(new ShellArgs { ViewModel = typeof(LoginViewModel) });
        InitializeNavigation();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.Unsubscribe();
    }

    private void InitializeNavigation()
    {
        var nav = ServiceLocator.Current.GetService<INavigationService>();
        if (nav != null && Frame != null)
            nav.Initialize(Frame);
    }

    public void InitializeContext()
    {
        var ctx = ServiceLocator.Current.GetService<IContextService>();
        ctx.InitializeWithContext(DispatcherQueue, this);
    }

    private async void OnTeshisClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            var dlg = new Views.SistemKurulum.Components.QuickSistemDbDiagDialog();
            var result = await MuhasibPro.Helpers.DialogHelper.ShowCenteredAsync(dlg);
            if (result == ContentDialogResult.Secondary)
            {
                // "Detayli Teshis" secildi — SistemKurulumView'a navigate
                ViewModel.NavigationService.Navigate<ViewModels.ViewModels.Sistem.SistemKurulumViewModel>(
                    new ShellArgs { ViewModel = typeof(ViewModels.ViewModels.Sistem.SistemKurulumViewModel) });
            }
            else
            {
                // "Kapat" secildi veya dialog kapatildi — sadece DB durumunu yenile
                await ViewModel.LoadAsync(new ShellArgs { ViewModel = typeof(LoginViewModel) });
            }
        }
        catch
        {
            // Dialog acilamadigindan (baska dialog acik vb.) sessizce devam et
            // SistemKurulum'a yonlendirme YAPMA — kullanici bunu istemedi
        }
    }

    protected override async void OnKeyDown(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.CanLogin)
            await ViewModel.LoginWithPassword();
        base.OnKeyDown(e);
    }
}
