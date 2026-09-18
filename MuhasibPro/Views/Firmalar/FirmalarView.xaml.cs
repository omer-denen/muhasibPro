// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.ServiceExtensions;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;
using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.Views.Firmalar
{
    /// <summary>
    /// Firma Yönetimi sayfası: master-detail (liste + seçili firma detayı/mali dönemleri).
    /// Liste üzerindeki yeni/düzenle/sil işlemleri <see cref="FirmaListViewModel"/> tarafından yürütülür.
    /// </summary>
    public sealed partial class FirmalarView : Page
    {
        public FirmalarView()
        {
            ViewModel = ServiceLocator.Current.GetService<FirmalarViewModel>();
            InitializeComponent();
        }
        public FirmalarViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Yeni pencerede ContextService dispatcher'ı bağlanmazsa VM'in dispatch'i UI'a inmez
            // (ilk seçim/detay güncellenmez). Pencere context'ini bağla.
            try
            {
                ServiceLocator.Current.GetService<IContextService>()?
                    .InitializeWithContext(dispatcher: DispatcherQueue, viewElement: this);
            }
            catch { /* context best-effort */ }

            ViewModel.Subscribe();
            await ViewModel.LoadAsync(e.Parameter as FirmaListArgs);

            // Açılışta ilk satır otomatik seçilsin (liste boş değilse).
            if (ViewModel.FirmaList.SelectedItem == null && ViewModel.FirmaList.ItemsSource?.Count > 0)
                ViewModel.FirmaList.SelectedItem = ViewModel.FirmaList.ItemsSource[0];
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Unload();
            ViewModel.Unsubscribe();
        }

        /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AnaBorderShadow.Receivers.Add(RootLayout);
            }
            catch { }
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
}
