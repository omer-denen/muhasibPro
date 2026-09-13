using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.DenetimMasasi.Sayfalar
{
    /// <summary>Giriş dashboard çerçeve sayfası (VM parametreden gelir; RichButton/bölüm linkleri VM'e iletilir).</summary>
    public sealed partial class GirisDashboardSayfasi : Page
    {
        public GirisDashboardSayfasi()
        {
            InitializeComponent();
        }

        private GirisDashboardViewModel Vm => DataContext as GirisDashboardViewModel;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is GirisDashboardViewModel vm)
            {
                DataContext = vm;
                UygulamaRichButton.SubTitle = $"v{Helpers.AppSurumBilgisi.Surum} • Çok kullanıcılı • Yerel SQLite";
                await vm.YukleAsync();
            }
        }

        private async void OnYonetimClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: FirmaDashboardKarti kart } && Vm != null)
                await Vm.YonetimiAcAsync(kart);
        }

        private async void OnYeniFirmaClick(object sender, RoutedEventArgs e)
        {
            if (Vm != null)
                await Vm.YeniFirmaAcAsync();
        }

        private void OnVeritabaniClick(object sender, RoutedEventArgs e)
            => Vm?.VeritabaniBolumunuAc();

        private void OnGuncellemeClick(object sender, RoutedEventArgs e)
            => Vm?.GuncellemeBolumunuAc();
    }
}
