using Microsoft.UI.Xaml.Controls;
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
