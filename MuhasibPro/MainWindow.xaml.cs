using Microsoft.UI.Dispatching;
using MuhasibPro.Helpers;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private readonly DispatcherQueue _dispatcherQueue;
        private readonly UISettings settings;

        public MainWindow()
        {
            this.InitializeComponent();
            WindowSettings();

            _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            settings = new UISettings();
            settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
            App.ThemeSelectorService.ThemeChanged += ThemeService_ThemeChanged;            
        }

    

        private void ThemeService_ThemeChanged(object? sender, ElementTheme e)
        {
            TitleBarHelper.UpdateTitleBar(e);
        }

        void WindowSettings()
        {
            // Backdrop tek kaynak: MainWindow.xaml'de <MicaBackdrop Kind="Base" /> (gerçek Mica).
            // MS önerisi: XAML-only, fallback otomatiktir; code-behind tekrarı Kural 4 ihlalidir.
            ExtendsContentIntoTitleBar = true;

            this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/AppIcon.ico"));
            this.Title = "MuhasibPro";
        }
        private void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            // This calls comes off-thread, hence we will need to dispatch it to current app's thread
            _dispatcherQueue.TryEnqueue(
                () =>
                {
                    TitleBarHelper.ApplySystemThemeToCaptionButtons();
                });
        }
    }
}

