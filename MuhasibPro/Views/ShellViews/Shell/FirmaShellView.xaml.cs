using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.ShellViews.Shell
{
    /// <summary>Firma ve mali dönem seçim ekranı.</summary>
    public sealed partial class FirmaShellView : Page
    {
        public FirmaShellView()
        {
            ViewModel = ServiceLocator.Current.GetService<FirmaShellViewModel>();
            InitializeComponent();
            SurumFooter.Text = Helpers.AppSurumBilgisi.Metin;
        }

        public FirmaShellViewModel ViewModel { get;}

        /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
        private void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                AnaBorderShadow.Receivers.Add(RootGrid);
            }
            catch { }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var args = e.Parameter as ShellArgs ?? new ShellArgs();
            ViewModel.BaseSubscribe();
            await ViewModel.LoadAsync(args);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.BaseUnsubscribe();
            ViewModel.FirmalarVM.Unload();
        }

        /// <summary>Denetim Masası'nı ayrı modal pencerede açar (Oturum 244).</summary>
        private async void OnDenetimMasasiClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.NavigationService.CreateNewViewAsync<DenetimMasasiViewModel>(null, "Denetim Masası");
        }

        private async void OnYeniFirmaClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)        {
            if (ViewModel.IsMainWindow)
                await ViewModel.NavigationService.CreateNewViewAsync<ViewModels.ViewModels.Sistem.Firmalar.FirmaDetailsViewModel>(
                    new ViewModels.ViewModels.Sistem.Firmalar.FirmaDetailsArgs());
            else
                ViewModel.NavigationService.Navigate<ViewModels.ViewModels.Sistem.Firmalar.FirmaDetailsViewModel>(
                    new ViewModels.ViewModels.Sistem.Firmalar.FirmaDetailsArgs());
        }
    }
}
