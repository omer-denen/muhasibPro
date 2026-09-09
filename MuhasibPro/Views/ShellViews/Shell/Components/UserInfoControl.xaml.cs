using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.ShellViews.Shell.Components;

public sealed partial class UserInfoControl : UserControl
{
    public UserInfoControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => UpdateUserInfo();

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) => UpdateUserInfo();

    private void UpdateUserInfo()
    {
        if (DataContext is FirmaShellViewModel vm)
        {
            var name = vm.ViewModelArgs?.UserInfo?.KullaniciModel?.AdiSoyadi;
            if (!string.IsNullOrWhiteSpace(name))
            {
                NameText.Text = name;
                InitialsText.Text = GetInitials(name);
            }
        }
    }

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            var s = parts[0].Trim();
            if (s.Length >= 2) return $"{s[0]}{s[1]}".ToUpperInvariant();
            return $"{s[0]}".ToUpperInvariant();
        }
        return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is FirmaShellViewModel vm)
        {
            vm.NavigationService.Navigate<LoginViewModel>(new ShellArgs { ViewModel = typeof(LoginViewModel) });
        }
        else
        {
            // Fallback: try ServiceLocator
            var nav = MuhasibPro.HostBuilders.ServiceLocator.Current.GetService<MuhasibPro.Business.Contracts.UIServices.CommonServices.INavigationService>();
            nav?.Navigate<LoginViewModel>(new ShellArgs { ViewModel = typeof(LoginViewModel) });
        }
    }
}
