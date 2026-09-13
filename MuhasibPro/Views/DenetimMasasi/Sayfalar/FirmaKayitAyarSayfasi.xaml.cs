using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.DenetimMasasi.Sayfalar
{
    /// <summary>Firma Kayıt çerçeve sayfası (paneli host eder, VM parametreden gelir).</summary>
    public sealed partial class FirmaKayitAyarSayfasi : Page
    {
        public FirmaKayitAyarSayfasi()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FirmaKayitAyarlarViewModel vm)
            {
                Panel.DataContext = vm;
                await vm.LoadAsync();
            }
        }
    }
}
