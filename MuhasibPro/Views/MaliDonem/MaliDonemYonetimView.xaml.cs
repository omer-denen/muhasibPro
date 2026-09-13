using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell.Tenant;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using MuhasibPro.Views.MaliDonem.Yonetim.Components.Dialogs;
using MuhasibPro.Views.ShellViews.Shell.Components.Dialogs;

namespace MuhasibPro.Views.MaliDonem;

/// <summary>Firma kartındaki "Mali Dönem İşlemleri" ile açılan yönetim penceresi.
/// Master-detail: solda dönem seçim listesi, sağda konu kartları (hepsi seçili döneme bağlı).</summary>
public sealed partial class MaliDonemYonetimView : Page
{
    public MaliDonemYonetimView()
    {
        InitializeComponent();
        ViewModel = ServiceLocator.Current.GetService<MaliDonemYonetimViewModel>();
        DataContext = ViewModel;
    }
    public MaliDonemYonetimViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        DonemOzet.DataContext = ViewModel;
        DonemOzet.Root = ViewModel;
        TopluIslemler.DataContext = ViewModel;
        DonemKartlar.DataContext = ViewModel;
        DonemYedekler.DataContext = ViewModel.YedeklerVM;
        DonemYedekler.Root = ViewModel;
        DonemParametreler.DataContext = ViewModel;
        DerinAnalizPanel.DataContext = ViewModel;
        ViewModel.Subscribe();
        await ViewModel.LoadAsync(e.Parameter as MaliDonemYonetimArgs);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.Unsubscribe();
    }

    private async void OnTopluYedekleClick(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel.GenelBakisVM;
        if (vm == null) return;
        await vm.TopluYedekleAsync();
        await ViewModel.RefreshAllAsync();
    }

    private async void OnTopluTestClick(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel.GenelBakisVM;
        if (vm == null) return;
        await vm.TopluTestAsync();
        ViewModel.TazeleSayaclar();
    }

    private async void OnTopluBakimClick(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel.GenelBakisVM;
        if (vm == null) return;
        await vm.TopluBakimAsync();
        await ViewModel.RefreshAllAsync();
    }

    private async void OnDurumYenileClick(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel.GenelBakisVM;
        var donem = ViewModel.SelectedDonem;
        if (vm == null || donem == null) return;
        await vm.MaliDonemList.AnalyzeDbStatusAsync(donem);
        await ViewModel.RefreshAllAsync();
    }

    private async void OnHizliYedekleClick(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel.YedeklerVM;
        if (vm == null) return;
        await vm.YedekAlAsync();
        await ViewModel.RefreshAllAsync();
    }

    private async Task BilgiGosterAsync(string metin)
    {
        await DonemOzet.BilgiGosterAsync(metin);
    }

    private void HataBildir(Exception ex, string islem)
    {
        ServiceLocator.Current.GetService<INotificationService>()?.Show(
            $"{islem} Hatası", $"{ex.Message} Yeniden dönem seçin.",
            NotificationType.Danger);
    }

    private async void OnDonemSilClick(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel;
        if (!vm.DonemSeciliMi("Silme"))
            return;
        try
        {
            var dialog = new DeleteGuardDialog { Donem = vm.SelectedDonem };
            await DialogHelper.ShowCenteredAsync(dialog);
            await vm.RefreshAllAsync();
        }
        catch (Exception ex)
        {
            HataBildir(ex, "Silme");
        }
    }

    private async void OnArsivdenCikarClick(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel;
        if (!vm.DonemSeciliMi("Arşivden çıkarma"))
            return;
        try
        {
            int yil = vm.SelectedDonem.MaliYil;
            if (await vm.ArsivVM.ArsivdenCikarAsync(vm.SelectedDonem))
            {
                await vm.RefreshAllAsync();
                await BilgiGosterAsync($"{yil} arşivden çıkarıldı, tekrar kullanıma açık.");
            }
        }
        catch (Exception ex)
        {
            HataBildir(ex, "Arşivden çıkarma");
        }
    }

    private async void OnGuncelleClick(object sender, RoutedEventArgs e)
    {
        var donem = ViewModel.SelectedDonem;
        var firma = ViewModel.SelectedFirma;
        if (donem == null || firma == null) return;
        try
        {
            var nav = ServiceLocator.Current.GetService<INavigationService>();
            var args = new TenantDatabaseUpdateArgs
            {
                DatabaseName = donem.DatabaseName,
                Firma = firma,
                MaliDonem = donem
            };
            await nav.CreateNewViewAsync<TenantDatabaseUpdateViewModel>(new ViewModels.ViewModels.Shell.ShellArgs { Parameter = args }, "Veritabanı Güncelleme");
        }
        catch (Exception ex)
        {
            HataBildir(ex, "Güncelleme");
        }
    }

    /// <summary>Başarı çubuğunu gösterip animasyonla soldurur (üst üste çağrıda son çağrı kazanır).</summary>
    private void OnHamburgerClick(object sender, RoutedEventArgs e)
    {
        MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
    }

    private async void OnAyarlarClick(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel?.AyarlarVM;
        if (vm == null || ViewModel?.SelectedFirma == null)
            return;
        try
        {
            var dialog = new YonetimAyarlarDialog { ViewModel = vm };
            await DialogHelper.ShowCenteredAsync(dialog);
            if (vm.SayfaBoyutuDegisti)
                await ViewModel.AyarSonrasiTazeleAsync();
        }
        catch (Exception ex)
        {
            HataBildir(ex, "Ayarlar");
        }
    }
}
