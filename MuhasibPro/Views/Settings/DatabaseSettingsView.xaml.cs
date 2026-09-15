using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;

namespace MuhasibPro.Views.Settings
{
    public sealed partial class DatabaseSettingsView : Page
    {
        public DatabaseSettingsView()
        {
            InitializeComponent();
            ViewModel = ServiceLocator.Current.GetService<ViewModels.ViewModels.Settings.DatabaseSettingsViewModel>();
            DataContext = ViewModel;
        }

        public ViewModels.ViewModels.Settings.DatabaseSettingsViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.LoadAsync();
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
