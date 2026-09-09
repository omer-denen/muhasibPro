using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

/// <summary>Sol seçim listesi: AÇIK (üst) + ARŞİVLİ (alt). Seçim TwoWay binding ile sayfa VM'ine
/// yazılır; iki liste aynı seçimi göstermesin diye diğer taraf temizlenir.</summary>
public sealed partial class DonemSecimListesi : UserControl
{
    public DonemSecimListesi() => InitializeComponent();

    private bool _esitleme;

    private void OnAcikSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_esitleme || AcikListe.SelectedItem == null)
            return;
        _esitleme = true;
        try { ArsivListe.SelectedItem = null; }
        finally { _esitleme = false; }
    }

    private void OnArsivSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_esitleme || ArsivListe.SelectedItem == null)
            return;
        _esitleme = true;
        try { AcikListe.SelectedItem = null; }
        finally { _esitleme = false; }
    }

    private void OnAcikPrevClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ViewModels.Sistem.MaliDonemler.MaliDonemYonetimViewModel vm && vm.AcikCanPrev)
            vm.AcikCurrentPage--;
    }
    private void OnAcikNextClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ViewModels.Sistem.MaliDonemler.MaliDonemYonetimViewModel vm && vm.AcikCanNext)
            vm.AcikCurrentPage++;
    }
    private void OnArsivPrevClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ViewModels.Sistem.MaliDonemler.MaliDonemYonetimViewModel vm && vm.ArsivVM.ArsivCanPrev)
            vm.ArsivVM.ArsivCurrentPage--;
    }
    private void OnArsivNextClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ViewModels.Sistem.MaliDonemler.MaliDonemYonetimViewModel vm && vm.ArsivVM.ArsivCanNext)
            vm.ArsivVM.ArsivCurrentPage++;
    }
}
