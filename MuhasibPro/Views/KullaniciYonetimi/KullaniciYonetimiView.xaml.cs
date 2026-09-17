using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

namespace MuhasibPro.Views.KullaniciYonetimi;

/// <summary>Faz 6.85 K2: Kullanıcı Yönetimi penceresi (FirmaShell kullanıcı menüsünden açılır).
/// Master-detail: solda kullanıcı listesi, sağda ekle/düzenle formu.</summary>
public sealed partial class KullaniciYonetimiView : Page
{
    public KullaniciYonetimiView()
    {
        InitializeComponent();
        ViewModel = ServiceLocator.Current.GetService<KullaniciYonetimiViewModel>();
        DataContext = ViewModel;
    }

    public KullaniciYonetimiViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await ViewModel.LoadAsync();
    }

    /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        try { AnaBorderShadow.Receivers.Add(RootLayout); } catch { }
    }

    private void OnDuzenleClick(object sender, RoutedEventArgs e) => KomutCalistir(ViewModel.DuzenleCommand, sender);
    private void OnAktifClick(object sender, RoutedEventArgs e) => KomutCalistir(ViewModel.AktifDegistirCommand, sender);
    private void OnSifreClick(object sender, RoutedEventArgs e) => KomutCalistir(ViewModel.SifreBelirleCommand, sender);
    private void OnSilClick(object sender, RoutedEventArgs e) => KomutCalistir(ViewModel.SilCommand, sender);

    private static void KomutCalistir(ICommand komut, object sender)
    {
        if (komut == null) return;
        if ((sender as FrameworkElement)?.DataContext is KullaniciModel model)
            komut.Execute(model);
    }
}
