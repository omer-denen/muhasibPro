using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.Views.SistemKurulum.Components;

public sealed partial class SystemTestsPanel : UserControl
{
    public SystemTestsPanel()
    {
        InitializeComponent();
    }

    public SistemKurulumViewModel ViewModel
    {
        get => (SistemKurulumViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(SistemKurulumViewModel), typeof(SystemTestsPanel), new PropertyMetadata(null));
}