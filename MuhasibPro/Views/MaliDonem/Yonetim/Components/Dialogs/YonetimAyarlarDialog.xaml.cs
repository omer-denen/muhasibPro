using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components.Dialogs;

/// <summary>Firma ayar dialogu: DataContext = YonetimAyarlarViewModel (sayfa verir),
/// kayıtlar anında ve satır-içi hatalı; sayfa kapanışta SayfaBoyutuDegisti'ne bakar.</summary>
public sealed partial class YonetimAyarlarDialog : ContentDialog
{
    public YonetimAyarlarViewModel ViewModel { get; set; }

    public YonetimAyarlarDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
            return;
        DataContext = ViewModel;
        GenelPaneli.DataContext = ViewModel.Genel;
        ListePaneli.DataContext = ViewModel.Liste;
        SaklamaPaneli.DataContext = ViewModel.Saklama;
        await ViewModel.LoadAsync();
    }

    private void OnHamburgerClick(object sender, RoutedEventArgs e)
    {
        MenuSplit.IsPaneOpen = !MenuSplit.IsPaneOpen;
    }

    /// <summary>Menü seçimi = Tag adındaki paneli göster, diğerlerini gizle.
    /// Yeni grup: panele x:Name ver + menüye aynı Tag'li satır ekle, buraya dokunma.</summary>
    private void OnBolumSecildi(object sender, SelectionChangedEventArgs e)
    {
        if (IcerikGrid == null)
            return;
        string hedef = (BolumListe.SelectedItem as ListViewItem)?.Tag as string ?? string.Empty;
        foreach (var cocuk in IcerikGrid.Children)
        {
            if (cocuk is FrameworkElement panel && !string.IsNullOrEmpty(panel.Name))
                panel.Visibility = panel.Name == hedef ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private async void OnSifirlaClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
            await ViewModel.SifirlaAsync();
    }

    private void OnKapatClick(object sender, RoutedEventArgs e) => Hide();

    private async void OnConflictOverwriteClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
            await ViewModel.OnConflictOverwriteAsync();
    }

    private async void OnConflictVazgecClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
            await ViewModel.OnConflictVazgecAsync();
    }
}
