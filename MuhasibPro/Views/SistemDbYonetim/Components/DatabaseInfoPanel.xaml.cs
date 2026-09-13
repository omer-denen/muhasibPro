using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.Views.SistemDbYonetim.Components
{
    public sealed partial class DatabaseInfoPanel : UserControl
    {
        public DatabaseInfoPanel()
        {
            InitializeComponent();
        }

        public SistemDbYonetimViewModel ViewModel
        {
            get => (SistemDbYonetimViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(SistemDbYonetimViewModel), typeof(DatabaseInfoPanel), new PropertyMetadata(null));
    }
}
