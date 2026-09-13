using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.DenetimMasasi.Sayfalar
{
    /// <summary>Giriş Güvenliği çerçeve sayfası (paneli host eder, VM parametreden gelir).</summary>
    public sealed partial class GirisAyarSayfasi : Page
    {
        public GirisAyarSayfasi()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is IdentityAyarlarViewModel vm)
            {
                Panel.DataContext = vm;
                await vm.LoadAsync();
            }
        }
    }
}
