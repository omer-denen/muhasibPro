using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.ShellViews.Shell.Components;

public sealed partial class FirmalarListControl : UserControl
{
    public FirmalarListControl()
    {
        InitializeComponent();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is ViewModels.ViewModels.Shell.FirmaShellViewModel vm)
        {
            vm.FirmaList.Query = SearchBox.Text;
            _ = vm.FirmaList.RefreshAsync();
        }
    }

    private void OnFirmaRadioChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { DataContext: FirmaModel clicked })
            return;
        if (DataContext is not ViewModels.ViewModels.Shell.FirmaShellViewModel vm)
            return;
        if (!ReferenceEquals(vm.FirmaList.SelectedItem, clicked))
            vm.FirmaList.SelectedItem = clicked;
    }

    private void OnEditFirmaClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FirmaModel model })
            return;
        // TODO: firma düzenleme dialog/pencere — şu an sadece seçim zenginliği için buton
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
