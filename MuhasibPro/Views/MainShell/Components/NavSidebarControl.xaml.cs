using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.MainShell.Components;

public sealed partial class NavSidebarControl : UserControl
{
    public NavSidebarControl()
    {
        InitializeComponent();
    }

    public MainShellViewModel ViewModel
    {
        get => (MainShellViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(MainShellViewModel),
        typeof(NavSidebarControl),
        new PropertyMetadata(null));

    /// <summary>Gerçek menü öğesine tıklanınca ilgili ekrana gider (Kural 21: yalnız var olan hedefler).</summary>
    private void OnNavClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: NavigationItem item })
            ViewModel?.NavigateTo(item.ViewModel);
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e) => ViewModel?.Logout();
}
