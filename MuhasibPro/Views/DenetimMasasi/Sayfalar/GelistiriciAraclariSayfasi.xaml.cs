using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.DenetimMasasi.Sayfalar
{
    /// <summary>Geliştirici Araçları çerçeve sayfası (paneli host eder, VM parametreden gelir).</summary>
    public sealed partial class GelistiriciAraclariSayfasi : Page
    {
        public GelistiriciAraclariSayfasi()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is GelistiriciAraclariViewModel vm)
            {
                Panel.DataContext = vm;
                await vm.LoadAsync();
            }
        }
    }
}
