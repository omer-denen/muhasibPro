using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.MainShell
{
    public sealed partial class MainShellView : Page
    {
        public MainShellView()
        {
            InitializeComponent();
        }

        public MainShellViewModel ViewModel => (MainShellViewModel)DataContext;
    }
}
