using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.ShellViews.Shell.Components;

public sealed partial class FirmalarListControl : UserControl
{
    public FirmalarListControl()
    {
        InitializeComponent();
        // Flyout Popup katmaninda ayri namescope'ta render edilir — ElementName binding
        // çalışmadığı için boş-durum görünürlüğü kod tarafında izlenir (Kural 11 ayrımı).
        Loaded += (s, e) =>
        {
            FirmaFlyoutListesi.Items.VectorChanged += (_, __) => BosDurumGuncelle();
            BosDurumGuncelle();
        };
    }

    private void BosDurumGuncelle()
    {
        // x:Name alanları InitializeComponent sonrası null olamaz; sessiz catch yok (Kural 12).
        BosDurumText.Visibility = FirmaFlyoutListesi.Items.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    /// <summary>Seçici butonu flyout'u açar (AttachedFlyout deseni — projede tek kaynak).</summary>
    private void OnFirmaSeciciClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
            FlyoutBase.ShowAttachedFlyout(element);
    }

    /// <summary>Flyout her açıldığında arama temizlenir ve odak arama kutusuna verilir (klavye akışı);
    /// liste yalnız filtre uygulanmışsa tazelenir.</summary>
    private void OnFirmaFlyoutOpened(object sender, object e)
    {
        if (DataContext is not ViewModels.ViewModels.Shell.FirmaShellViewModel vm)
            return;
        if (FirmaAramaBox == null)
            return;
        if (FirmaAramaBox.Text.Length > 0)
            FirmaAramaBox.Text = string.Empty;
        FirmaAramaBox.Focus(FocusState.Programmatic);
        if (!string.IsNullOrEmpty(vm.FirmaList.Query))
        {
            vm.FirmaList.Query = string.Empty;
            _ = vm.FirmaList.RefreshAsync();
        }
    }

    private void OnFirmaAramaChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is not ViewModels.ViewModels.Shell.FirmaShellViewModel vm)
            return;
        var query = FirmaAramaBox.Text ?? string.Empty;
        if (vm.FirmaList.Query == query)
            return;
        vm.FirmaList.Query = query;
        _ = vm.FirmaList.RefreshAsync();
    }

    /// <summary>Liste seçimi (tıklama/klavye) VM'e yazılır; programatik temizleme (arama filtresi) yazılmaz.</summary>
    private void OnFirmaSecimDegisti(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ViewModels.ViewModels.Shell.FirmaShellViewModel vm)
            return;
        if (e.AddedItems.Count == 0 || e.AddedItems[0] is not FirmaModel clicked)
            return;
        if (!ReferenceEquals(vm.FirmaList.SelectedItem, clicked))
            vm.FirmaList.SelectedItem = clicked;
    }

    /// <summary>Kullanıcı satıra tıklayınca flyout kapanır (programatik seçim flyout'u kapatmaz).</summary>
    private void OnFirmaItemClick(object sender, ItemClickEventArgs e)
    {
        // Hide() kapalı flyout'ta no-op'tur; sessiz catch yok (Kural 12).
        FirmaSeciciFlyout?.Hide();
    }

    private async void OnEditFirmaClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FirmaModel model })
            return;
        if (DataContext is not ViewModels.ViewModels.Shell.FirmaShellViewModel vm)
            return;
        var args = new FirmaDetailsArgs { FirmaId = model.Id };
        if (vm.IsMainWindow)
            await vm.NavigationService.CreateNewViewAsync<FirmaDetailsViewModel>(args, $"{model.KisaUnvani} — Düzenle");
        else
            vm.NavigationService.Navigate<FirmaDetailsViewModel>(args);
    }

    /// <summary>Yeni firma tanımlama (Sage "Add Company" deseni — secici flyout'unda).</summary>
    private async void OnYeniFirmaClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ViewModels.ViewModels.Shell.FirmaShellViewModel vm)
            return;
        FirmaSeciciFlyout?.Hide();
        var args = new FirmaDetailsArgs();
        if (vm.IsMainWindow)
            await vm.NavigationService.CreateNewViewAsync<FirmaDetailsViewModel>(args, "Yeni Firma");
        else
            vm.NavigationService.Navigate<FirmaDetailsViewModel>(args);
    }

    private async void OnMaliDonemYonetimClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FirmaModel model })
            return;
        var nav = ServiceLocator.Current.GetService<INavigationService>();
        if (nav == null)
            return;
        await nav.CreateNewViewAsync<MaliDonemYonetimViewModel>(
            new MaliDonemYonetimArgs
            {
                FirmaId = model.Id,
                FirmaKodu = model.FirmaKodu,
                KisaUnvani = model.KisaUnvani
            },
            $"{model.KisaUnvani} — Mali Dönem Yönetimi");
    }
}
