using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.Views.MaliDonem.Yonetim.Components.Dialogs;

/// <summary>Saklama alt sınırı yazılı silme onayı: kod = yedek dosya adı (code-behind, DeleteGuard deseni).</summary>
public sealed partial class YedekSilOnayDialog : ContentDialog
{
    public YedekSilOnayDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    public string YedekDosyaAdi { get; set; } = string.Empty;

    public int KalanSayi { get; set; }

    public int AltSinir { get; set; }

    public bool SilmeOnaylandi { get; private set; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DosyaAdiText.Text = YedekDosyaAdi ?? string.Empty;
        KalanSayiText.Text = KalanSayi.ToString();
        AltSinirText.Text = AltSinir.ToString();
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        args.Cancel = true;

        if (string.Equals(OnayKutusu.Text.Trim(), (YedekDosyaAdi ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase))
        {
            SilmeOnaylandi = true;
            Hide();
        }
        else
        {
            ErrorText.Text = "Onay kodu hatalı. Dosya adını aynen yazınız.";
            ErrorBorder.Visibility = Visibility.Visible;
        }
        deferral.Complete();
    }
}
