using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

namespace MuhasibPro.Views.SistemDbYonetim.Components;

public sealed partial class KurulumKayitPanel : UserControl
{
    public KurulumKayitPanel() => InitializeComponent();

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(KurulumKayitViewModel), typeof(KurulumKayitPanel), new PropertyMetadata(null));

    public KurulumKayitViewModel ViewModel
    {
        get => (KurulumKayitViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null) await ViewModel.YukleAsync();
    }
}
