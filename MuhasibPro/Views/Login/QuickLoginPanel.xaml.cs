using Microsoft.UI.Xaml.Controls;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.Login;

public sealed partial class QuickLoginPanel : UserControl
{
    public QuickLoginPanel()
    {
        ViewModel = ServiceLocator.Current.GetService<QuickLoginAccountsViewModel>();
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    public QuickLoginAccountsViewModel ViewModel { get; }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UpdateEmptyVisibility();
        ViewModel.RememberedAccounts.CollectionChanged += (s, args) => UpdateEmptyVisibility();
    }

    private void UpdateEmptyVisibility()
    {
        if (emptyText != null && accountsList != null)
        {
            emptyText.Visibility = ViewModel.RememberedAccounts.Count == 0
                ? Microsoft.UI.Xaml.Visibility.Visible
                : Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }

    private void OnAccountClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is RememberedAccount acc)
        {
            // Seçimi LoginViewModel'e bildir
            ViewModel.SelectRememberedAccountCommand.Execute(acc);
        }
        else if (sender is Button b2 && b2.Tag is RememberedAccount acc2)
        {
            ViewModel.SelectRememberedAccountCommand.Execute(acc2);
        }
    }

    private void OnRemoveClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is RememberedAccount acc)
        {
            ViewModel.RemoveRememberedAccountCommand.Execute(acc);
        }
    }
}
