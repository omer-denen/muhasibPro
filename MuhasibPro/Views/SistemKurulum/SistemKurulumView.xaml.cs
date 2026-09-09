using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.ServiceExtensions;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.Views.SistemKurulum
{
    public sealed partial class SistemKurulumView : Page
    {
        public SistemKurulumView()
        {
            ViewModel = ServiceLocator.Current.GetService<SistemKurulumViewModel>();
            InitializeContext();
            InitializeComponent();
        }

        public SistemKurulumViewModel ViewModel { get; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Splash yönlendirmesi: DB yoksa "ilk kurulum" modu netleştirilir
            if (e.Parameter is ShellArgs sa && sa.Parameter is bool firstSetup && firstSetup)
                ViewModel.SetFirstSetupMode();

            // Açık tema — pencere kontrolleri koyu çizilsin (viewpackage açık zemin)
            try { Helpers.TitleBarHelper.UpdateTitleBar(Microsoft.UI.Xaml.ElementTheme.Light); } catch { }

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

    }
}
