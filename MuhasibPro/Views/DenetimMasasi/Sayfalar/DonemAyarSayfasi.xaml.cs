using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.DenetimMasasi.Sayfalar
{
    /// <summary>Dönem çerçeve sayfası (paneli host eder, VM parametreden gelir).</summary>
    public sealed partial class DonemAyarSayfasi : Page
    {
        public DonemAyarSayfasi()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is DonemAyarlarViewModel vm)
            {
                Panel.DataContext = vm;
                await vm.LoadAsync();
            }
        }
    }
}
