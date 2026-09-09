using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

/// <summary>Derin analiz paneli: DataContext = MaliDonemYonetimViewModel (DerinAnaliz + Çalıştır).</summary>
public sealed partial class DerinAnalizPanel : UserControl
{
    public DerinAnalizPanel() => InitializeComponent();

    private MaliDonemYonetimViewModel Vm => DataContext as MaliDonemYonetimViewModel;

    private async void OnDerinAnalizClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null)
            return;
        await Vm.DerinAnalizYukleAsync();
    }
}
