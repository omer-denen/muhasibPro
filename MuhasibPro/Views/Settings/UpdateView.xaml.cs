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

        /// <summary>Denetim Masası içi gömülü mod: başlık satırı gizlenir (kabukta zaten
        /// 28px başlık var) + zemin saydamlaşır (çift başlık/çift zemin olmaz).</summary>
        public void GomuluUygula()
        {
            BaslikSatiri.Visibility = Visibility.Collapsed;
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
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
