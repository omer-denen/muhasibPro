using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.DenetimMasasi.Sayfalar
{
    /// <summary>Görünüm & Bildirim çerçeve sayfası (paneli host eder, VM parametreden gelir).</summary>
    public sealed partial class GorunumAyarSayfasi : Page
    {
        public GorunumAyarSayfasi()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is AppPlatformAyarlarViewModel vm)
            {
                Panel.DataContext = vm;
                await vm.LoadAsync();
            }
        }
    }
}
