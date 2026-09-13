using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Views.Settings;

namespace MuhasibPro.Views.DenetimMasasi.Sayfalar
{
    /// <summary>Güncelleme çerçeve sayfası (mevcut UpdateView'i gömülü modda host eder;
    /// UpdateView VM'ini kendisi çözer — parametre taşınmaz; çıkışta abonelik bırakılmaz).</summary>
    public sealed partial class GuncellemeAyarSayfasi : Page
    {
        public GuncellemeAyarSayfasi()
        {
            InitializeComponent();
            IcerikFrame.Navigated += OnIcerikGezildi;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            IcerikFrame.Navigate(typeof(UpdateView));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            IcerikFrame.Navigated -= OnIcerikGezildi;
            if (IcerikFrame.Content is UpdateView govde)
                govde.ViewModel.Unsubscribe();
        }

        private void OnIcerikGezildi(object sender, NavigationEventArgs e)
        {
            if (IcerikFrame.Content is UpdateView govde)
                govde.GomuluUygula();
        }
    }
}
