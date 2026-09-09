using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

/// <summary>Bilinmeyen yedek paneli: DataContext = BilinmeyenYedekViewModel, Root = sayfa VM (sayfa atar).</summary>
public sealed partial class BilinmeyenPanel : UserControl
{
    public BilinmeyenPanel() => InitializeComponent();

    public MaliDonemYonetimViewModel Root { get; set; }

    private BilinmeyenYedekViewModel Vm => DataContext as BilinmeyenYedekViewModel;

    private async void OnYenileClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        await Vm.TaraAsync();
        Root?.TazeleSayaclar();
    }

    private async void OnTemizleClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null || sender is not Button { Tag: DatabaseBackupResult yedek }) return;
        Vm.SelectedBilinmeyen = yedek;
        await Vm.TemizleAsync();
        Root?.TazeleSayaclar();
    }

    private void OnBilinmeyenPrevClick(object sender, RoutedEventArgs e)
    {
        if (Vm != null && Vm.BilinmeyenCanPrev) Vm.BilinmeyenCurrentPage--;
    }
    private void OnBilinmeyenNextClick(object sender, RoutedEventArgs e)
    {
        if (Vm != null && Vm.BilinmeyenCanNext) Vm.BilinmeyenCurrentPage++;
    }
}
