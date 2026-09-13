using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

public sealed partial class DonemOzetCard : UserControl
{
    public DonemOzetCard() => InitializeComponent();

    public static readonly DependencyProperty RootProperty = DependencyProperty.Register(
        nameof(Root), typeof(MaliDonemYonetimViewModel), typeof(DonemOzetCard), new PropertyMetadata(null));

    public MaliDonemYonetimViewModel Root
    {
        get => (MaliDonemYonetimViewModel)GetValue(RootProperty);
        set => SetValue(RootProperty, value);
    }

    public event RoutedEventHandler HizliYedekleClick;
    public event RoutedEventHandler DonemSilClick;
    public event RoutedEventHandler DurumYenileClick;
    public event RoutedEventHandler GuncelleClick;
    public event RoutedEventHandler ArsivdenCikarClick;

    private void OnHizliYedekle(object sender, RoutedEventArgs e) => HizliYedekleClick?.Invoke(sender, e);
    private void OnDonemSil(object sender, RoutedEventArgs e) => DonemSilClick?.Invoke(sender, e);
    private void OnDurumYenile(object sender, RoutedEventArgs e) => DurumYenileClick?.Invoke(sender, e);
    private void OnGuncelle(object sender, RoutedEventArgs e) => GuncelleClick?.Invoke(sender, e);
    private void OnArsivdenCikar(object sender, RoutedEventArgs e) => ArsivdenCikarClick?.Invoke(sender, e);

    private int _bilgiToken;
    public async Task BilgiGosterAsync(string metin)
    {
        int token = ++_bilgiToken;
        ArsivBilgiMetni.Text = metin;
        ArsivBilgiBar.Opacity = 1;
        ArsivBilgiBar.Visibility = Visibility.Visible;
        await Task.Delay(3500);
        if (token != _bilgiToken)
            return;
        ArsivBilgiKaybol.Begin();
        await Task.Delay(650);
        if (token != _bilgiToken)
            return;
        ArsivBilgiBar.Visibility = Visibility.Collapsed;
    }
}
