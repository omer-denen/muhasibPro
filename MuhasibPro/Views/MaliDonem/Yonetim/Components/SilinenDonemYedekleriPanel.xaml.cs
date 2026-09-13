using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

/// <summary>Silinen dönem yedekleri paneli: DataContext = BilinmeyenYedekViewModel, Root = sayfa VM (sayfa atar).</summary>
public sealed partial class SilinenDonemYedekleriPanel : UserControl
{
    public SilinenDonemYedekleriPanel() => InitializeComponent();

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
        Vm.SelectedSilinen = yedek;
        await Vm.SilinenTemizleAsync();
        Root?.TazeleSayaclar();
    }

    private void OnSilinenPrevClick(object sender, RoutedEventArgs e)
    {
        if (Vm != null && Vm.SilinenCanPrev) Vm.SilinenCurrentPage--;
    }
    private void OnSilinenNextClick(object sender, RoutedEventArgs e)
    {
        if (Vm != null && Vm.SilinenCanNext) Vm.SilinenCurrentPage++;
    }
}
