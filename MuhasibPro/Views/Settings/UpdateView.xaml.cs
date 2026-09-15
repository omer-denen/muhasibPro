using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Settings;

namespace MuhasibPro.Views.Settings
{
    /// <summary>Uygulama güncelleme sayfası (durum + ayarlar sekmeleri).</summary>
    public sealed partial class UpdateView : Page
    {
        public UpdateView()
        {
            ViewModel = ServiceLocator.Current.GetService<UpdateViewModel>();
            InitializeComponent();
        }

        public UpdateViewModel ViewModel { get; }

        /// <summary>Denetim Masası içi gömülü mod: başlık satırı + zemin katmanı gizlenir
        /// (kabukta zaten 28px başlık var; çift başlık/çift zemin olmaz). Panel ve içerik kalır.</summary>
        public void GomuluUygula()
        {
            BaslikSatiri.Visibility = Visibility.Collapsed;
            ZeminGorseli.Visibility = Visibility.Collapsed;
            ZeminPerdesi.Visibility = Visibility.Collapsed;
            PanelWrap.Margin = new Thickness(0, 8, 0, 0);
            YardimButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
        private void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                AnaBorderShadow.Receivers.Add(RootGrid);
            }
            catch { }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.InitializeAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.Unsubscribe();
        }
    }
}
