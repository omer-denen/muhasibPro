using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

namespace MuhasibPro.Views.KullaniciYonetimi;

/// <summary>Faz 6.85 K2-REDESIGN: yeni/düzenle kullanıcı sayfası (liste araç çubuğundan açılır).</summary>
public sealed partial class KullaniciDetailsView : Page
{
    public KullaniciDetailsView()
    {
        ViewModel = ServiceLocator.Current.GetService<KullaniciDetailsViewModel>();
        InitializeComponent();
    }

    public KullaniciDetailsViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel.Subscribe();
        await ViewModel.LoadAsync(e.Parameter as KullaniciDetailsArgs);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.Unload();
        ViewModel.Unsubscribe();
    }

    /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        try { AnaBorderShadow.Receivers.Add(RootLayout); } catch { }
    }
}
