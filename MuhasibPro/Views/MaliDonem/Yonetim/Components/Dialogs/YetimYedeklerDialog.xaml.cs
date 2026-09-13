using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components.Dialogs;

/// <summary>Kayıt-dışı yedekler sekmeli dialogu: DataContext = BilinmeyenYedekViewModel (açan verir).
/// Sekme 0 = silinen dönem yedekleri, Sekme 1 = bilinmeyen yedekler. Tek işlem: silme.</summary>
public sealed partial class YetimYedeklerDialog : ContentDialog
{
    public BilinmeyenYedekViewModel Vm { get; set; }

    public MaliDonemYonetimViewModel Root { get; set; }

    /// <summary>Açılış sekmesi (0 = silinen, 1 = bilinmeyen).</summary>
    public int BaslangicSekmesi { get; set; }

    public YetimYedeklerDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Vm == null)
            return;
        DataContext = Vm;
        SilinenPanel.DataContext = Vm;
        SilinenPanel.Root = Root;
        BilinmeyenPanel.DataContext = Vm;
        BilinmeyenPanel.Root = Root;
        SekmeSec(BaslangicSekmesi == 1 ? 1 : 0);
        await Vm.TaraAsync();
    }

    private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        Root?.TazeleSayaclar();
    }

    private void OnSekmeSilinenClick(object sender, RoutedEventArgs e) => SekmeSec(0);

    private void OnSekmeBilinmeyenClick(object sender, RoutedEventArgs e) => SekmeSec(1);

    private void SekmeSec(int sekme)
    {
        bool silinen = sekme != 1;
        SilinenKaydirici.Visibility = silinen ? Visibility.Visible : Visibility.Collapsed;
        BilinmeyenKaydirici.Visibility = silinen ? Visibility.Collapsed : Visibility.Visible;
        SekmeSilinenButton.Opacity = silinen ? 1 : 0.55;
        SekmeBilinmeyenButton.Opacity = silinen ? 0.55 : 1;
    }
}
