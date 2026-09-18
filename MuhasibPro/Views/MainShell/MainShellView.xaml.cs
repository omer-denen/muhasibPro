using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.MainShell
{
    public sealed partial class MainShellView : Page
    {
        public MainShellView()
        {
            ViewModel = ServiceLocator.Current.GetService<MainShellViewModel>();
            AsistanVm = ServiceLocator.Current.GetService<AsistanSohbetViewModel>();
            InitializeComponent();
            DataContext = ViewModel;
            // AI asistanı statü çubuğundaki 🤖 flyout'unda host'lanır (MainShell yerleşiminde değil).
            ShellStatusBarControl.AsistanVm = AsistanVm;
        }

        public MainShellViewModel ViewModel { get; }

        public AsistanSohbetViewModel AsistanVm { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadAsync(e.Parameter as ShellArgs);
            await AsistanVm.LoadAsync();
        }

        /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
        private void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Firma/dönem bağlamı yalnız workspace host'unda gösterilir (Ayarlar/Yeni Firma göstermez).
            ShellStatusBarControl.BaglamGoster = true;
            // AI asistanı giriş noktası (🤖) yalnız workspace host'unda görünür.
            ShellStatusBarControl.AsistanGorunur = true;
            try
            {
                AnaBorderShadow.Receivers.Add(RootGrid);
            }
            catch { }
        }

        /// <summary>Kural 13: F1 → AI yardım paneli (statü çubuğu "Asistan" ile aynı yol).</summary>
        private void OnF1Yardim(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            ShellStatusBarControl.AsistanPaneliAc();
        }
    }
}
