using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components;

public sealed partial class TopluIslemlerPanel : UserControl
{
    public TopluIslemlerPanel() => InitializeComponent();

    public event RoutedEventHandler TopluYedekleClick;
    public event RoutedEventHandler TopluTestClick;
    public event RoutedEventHandler TopluBakimClick;

    private void OnTopluYedekle(object sender, RoutedEventArgs e) => TopluYedekleClick?.Invoke(sender, e);
    private void OnTopluTest(object sender, RoutedEventArgs e) => TopluTestClick?.Invoke(sender, e);
    private void OnTopluBakim(object sender, RoutedEventArgs e) => TopluBakimClick?.Invoke(sender, e);
}
