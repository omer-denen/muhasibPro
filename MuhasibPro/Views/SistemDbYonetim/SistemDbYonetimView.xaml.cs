using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.ServiceExtensions;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.Views.SistemDbYonetim
{
    public sealed partial class SistemDbYonetimView : Page
    {
        public SistemDbYonetimView()
        {
            ViewModel = ServiceLocator.Current.GetService<SistemDbYonetimViewModel>();
            InitializeContext();
            InitializeComponent();
            SurumFooter.Text = Helpers.AppSurumBilgisi.Metin;
        }

        public SistemDbYonetimViewModel ViewModel { get; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Splash yönlendirmesi: karar izi (string) veya eski migration bayrağı (bool)
            if (e.Parameter is ShellArgs sa && sa.Parameter is string kararOzeti && !string.IsNullOrWhiteSpace(kararOzeti))
                ViewModel.SetMigrationMode(kararOzeti);
            else if (e.Parameter is ShellArgs sa2 && sa2.Parameter is bool migrationMode && migrationMode)
                ViewModel.SetMigrationMode();

            // Uygulama teması — pencere kontrolleri aktif temaya göre çizilir (ayarlanabilir tema).
            try { Helpers.TitleBarHelper.UpdateTitleBar(App.ThemeSelectorService.Theme); } catch { }

            var nav = ServiceLocator.Current.GetService<INavigationService>();
            if (nav != null && Frame != null)
                nav.Initialize(Frame);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try { Helpers.TitleBarHelper.UpdateTitleBar(App.ThemeSelectorService.Theme); } catch { }
        }

        public void InitializeContext()
        {
            var contextService = ServiceLocator.Current.GetService<IContextService>();
            contextService.InitializeWithContext(dispatcher: DispatcherQueue, viewElement: this);
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


    }
}
