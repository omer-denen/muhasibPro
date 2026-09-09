using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

/// <summary>Dönem işlem kartları (4 kart: bakım + arşiv; yedekler ayrı panelde): DataContext = MaliDonemYonetimViewModel (sayfa atar).</summary>
public sealed partial class DonemIslemKartlariPanel : UserControl
{
    public DonemIslemKartlariPanel() => InitializeComponent();

    private MaliDonemYonetimViewModel Vm => DataContext as MaliDonemYonetimViewModel;

    private void HataBildir(Exception ex, string islem)
    {
        ServiceLocator.Current.GetService<INotificationService>()?.Show(
            $"{islem} Hatası", $"{ex.Message} Yeniden dönem seçin.",
            NotificationType.Danger);
        if (Vm != null)
            Vm.AktifSegment = YonetimSegmenti.Tumu;
    }

    private async void OnBakimClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null || !Vm.DonemSeciliMi("Bakım") || sender is not Button { Tag: string komut })
            return;
        try
        {
            if (await Vm.BakimCalistirAsync(komut))
                await Vm.RefreshAllAsync();
        }
        catch (Exception ex)
        {
            HataBildir(ex, "Bakım");
        }
    }

    private async void OnArsivleClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null || !Vm.DonemSeciliMi("Arşivleme"))
            return;
        try
        {
            if (await Vm.ArsivVM.ArsivleAsync(Vm.SelectedDonem))
                await Vm.RefreshAllAsync();
        }
        catch (Exception ex)
        {
            HataBildir(ex, "Arşivleme");
        }
    }

    private async void OnArsivdenCikarClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null || !Vm.DonemSeciliMi("Arşivden çıkarma"))
            return;
        try
        {
            if (await Vm.ArsivVM.ArsivdenCikarAsync(Vm.SelectedDonem))
                await Vm.RefreshAllAsync();
        }
        catch (Exception ex)
        {
            HataBildir(ex, "Arşivden çıkarma");
        }
    }
}
