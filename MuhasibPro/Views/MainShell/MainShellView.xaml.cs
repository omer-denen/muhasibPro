using Microsoft.UI.Xaml.Controls;
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
            InitializeComponent();
            DataContext = ViewModel;
        }

        public MainShellViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadAsync(e.Parameter as ShellArgs);
        }

        /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
        private void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Firma/dönem bağlamı yalnız workspace host'unda gösterilir (Ayarlar/Yeni Firma göstermez).
            ShellStatusBarControl.BaglamGoster = true;
            try
            {
                AnaBorderShadow.Receivers.Add(RootGrid);
            }
            catch { }
        }
    }
}
