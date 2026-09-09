using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

/// <summary>Arşiv paneli: DataContext = ArsivDonemlerViewModel, Root = sayfa VM (sayfa atar).</summary>
public sealed partial class ArsivPanel : UserControl
{
    public ArsivPanel() => InitializeComponent();

    public MaliDonemYonetimViewModel Root { get; set; }

    private ArsivDonemlerViewModel Vm => DataContext as ArsivDonemlerViewModel;

    private async void OnGeriAcClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null || sender is not Button { Tag: MaliDonemModel model }) return;
        if (await Vm.ArsivdenCikarAsync(model) && Root != null)
            await Root.RefreshAllAsync();
    }

    private void OnArsivPrevClick(object sender, RoutedEventArgs e)
    {
        if (Vm != null && Vm.ArsivCanPrev) Vm.ArsivCurrentPage--;
    }
    private void OnArsivNextClick(object sender, RoutedEventArgs e)
    {
        if (Vm != null && Vm.ArsivCanNext) Vm.ArsivCurrentPage++;
    }
}
