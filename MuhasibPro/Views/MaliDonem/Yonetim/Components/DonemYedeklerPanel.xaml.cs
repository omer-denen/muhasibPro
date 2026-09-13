using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Helpers;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

/// <summary>Dönem yedek listesi: DataContext = DonemYedeklerViewModel, Root = sayfa VM (tazeleme için, sayfa atar).</summary>
public sealed partial class DonemYedeklerPanel : UserControl
{
    public DonemYedeklerPanel() => InitializeComponent();

    /// <summary>Sayfa VM'i (DP — XAML ElementName binding'leri güncellensin diye).</summary>
    public static readonly DependencyProperty RootProperty = DependencyProperty.Register(
        nameof(Root), typeof(MaliDonemYonetimViewModel), typeof(DonemYedeklerPanel), new PropertyMetadata(null));

    public MaliDonemYonetimViewModel Root
    {
        get => (MaliDonemYonetimViewModel)GetValue(RootProperty);
        set => SetValue(RootProperty, value);
    }

    private DonemYedeklerViewModel Vm => DataContext as DonemYedeklerViewModel;

    private async void OnYedekAlClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        await Vm.YedekAlAsync();
        Root?.TazeleSayaclar();
    }

    private async void OnGeriYukleClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null || sender is not Button { Tag: DatabaseBackupResult yedek }) return;
        Vm.SelectedYedek = yedek;
        var target = Root?.SelectedDonem ?? Vm.BagliDonem;
        if (target == null)
        {
            await Vm.GeriYukleAsync();
            if (Root != null) await Root.RefreshAllAsync();
            return;
        }
        var dialog = new Views.ShellViews.Shell.Components.Dialogs.RestoreVerifyDialog
        {
            Backup = yedek,
            TargetDonem = target
        };
        await DialogHelper.ShowCenteredAsync(dialog);
        if (dialog.RestoreSucceeded)
        {
            await Vm.YukleAsync(Vm.BagliDonem?.DatabaseName ?? target.DatabaseName);
            if (Root != null) await Root.RefreshAllAsync();
        }
        else
        {
            // RestoreVerifyDialog kendi bildirimini yaptı, sadece listeyi tazele
            if (dialog.RestoreSucceeded == false)
            {
                // kullanıcı vazgeçtiyse de listeyi yenilemeye gerek yok, ama yedek sonrası için yenile
            }
        }
    }

    private async void OnYedekSilClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null || sender is not Button { Tag: DatabaseBackupResult yedek }) return;
        Vm.SelectedYedek = yedek;
        await Vm.YedekSilAsync();
        Root?.TazeleSayaclar();
    }

    private void OnYedekPrevClick(object sender, RoutedEventArgs e)
    {
        if (Vm != null && Vm.YedekCanPrev) Vm.YedekCurrentPage--;
    }
    private void OnYedekNextClick(object sender, RoutedEventArgs e)
    {
        if (Vm != null && Vm.YedekCanNext) Vm.YedekCurrentPage++;
    }
}
