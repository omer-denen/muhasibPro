using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.ServiceExtensions;
using MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

namespace MuhasibPro.Views.KullaniciYonetimi;

/// <summary>Faz 6.85 K2-REDESIGN: Kullanıcı Yönetimi sayfası.
/// Pivot: "Kullanıcılar" (master-detail liste + detay) ve "Roller &amp; İzinler" (K3 matris).</summary>
public sealed partial class KullaniciYonetimiView : Page
{
    public KullaniciYonetimiView()
    {
        ViewModel = ServiceLocator.Current.GetService<KullaniciYonetimiViewModel>();
        InitializeComponent();
    }

    public KullaniciYonetimiViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        // Pencere ContextService dispatcher'ını bağla (ilk seçim/detay güncellemesi UI'a insin).
        try
        {
            ServiceLocator.Current.GetService<IContextService>()?
                .InitializeWithContext(dispatcher: DispatcherQueue, viewElement: this);
        }
        catch { /* context best-effort */ }

        ViewModel.Subscribe();
        await ViewModel.LoadAsync();

        // Açılışta ilk satır otomatik seçilsin (liste boş değilse).
        if (ViewModel.List.SelectedItem == null && ViewModel.List.ItemsSource?.Count > 0)
            ViewModel.List.SelectedItem = ViewModel.List.ItemsSource[0];
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

    /// <summary>Çoklu seçimde liste tüm yüksekliği kaplar (detay kartı gizlenir).</summary>
    public int GetRowSpan(bool isMultipleSelection)
    {
        return isMultipleSelection ? 2 : 1;
    }

    private bool _detayTamEkran;

    /// <summary>Detay panelini büyüt/küçült (yukarı açılan expander): liste kapanır, detay tüm alanı kaplar.</summary>
    private void OnDetayBuyutClick(object sender, RoutedEventArgs e)
    {
        _detayTamEkran = !_detayTamEkran;
        ListeSatiri.Height = _detayTamEkran ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        ListeSatiri.MinHeight = _detayTamEkran ? 0 : 200;
        ListeKarti.Visibility = _detayTamEkran ? Visibility.Collapsed : Visibility.Visible;
        DetayBuyutIcon.Glyph = _detayTamEkran ? "\uE70D" : "\uE70E";
        ToolTipService.SetToolTip(DetayBuyutButton, _detayTamEkran ? "Detayı küçült (listeyi göster)" : "Detayı büyüt (listeyi kapat)");
    }
}
