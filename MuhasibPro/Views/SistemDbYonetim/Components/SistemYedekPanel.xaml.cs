using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

namespace MuhasibPro.Views.SistemDbYonetim.Components;

public sealed partial class SistemYedekPanel : UserControl
{
    public SistemYedekViewModel ViewModel
    {
        get => (SistemYedekViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(SistemYedekViewModel), typeof(SistemYedekPanel), new PropertyMetadata(null));

    public SistemYedekPanel()
    {
        this.InitializeComponent();
    }

    private async void OnGeriYukleClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;
        var yedek = (sender as Button)?.Tag as DatabaseBackupResult;
        await ViewModel.GeriYukleAsync(yedek);
    }
}
