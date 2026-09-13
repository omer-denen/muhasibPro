using Microsoft.UI.Xaml;
using MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

namespace MuhasibPro.Views.SistemDbYonetim.Components;

public sealed partial class SistemGuncellemePanel : UserControl
{
    public SistemGuncellemeViewModel ViewModel
    {
        get => (SistemGuncellemeViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(SistemGuncellemeViewModel), typeof(SistemGuncellemePanel), new PropertyMetadata(null));

    public SistemGuncellemePanel()
    {
        this.InitializeComponent();
    }
}
