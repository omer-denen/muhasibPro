using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.DenetimMasasi.Sayfalar
{
    /// <summary>Veritabanı çerçeve sayfası: Sistem + Dönem gruplarını host eder.
    /// DataContext, navigasyon parametresindeki DenetimMasasiViewModel'dir (çocuk VM'ler oradan bağlanır).</summary>
    public sealed partial class VeritabaniAyarSayfasi : Page
    {
        public VeritabaniAyarSayfasi()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is DenetimMasasiViewModel denetim)
            {
                Panel.DataContext = denetim;
                await denetim.SistemVeritabani.LoadAsync();
                await denetim.YedekSaklama.LoadAsync();
            }
        }
    }
}
