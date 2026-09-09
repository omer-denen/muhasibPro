using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.Views.ShellViews.Shell.Components.Dialogs;

public sealed partial class YeniDonemDialog : ContentDialog
{
    public YeniDonemDialog()
    {
        InitializeComponent();
        // DefaultButton yok (WinUI default butona accent basip PrimaryButtonStyle'i ezer — Oturum 96).
        // Enter ile onay davranisi burada korunur.
        DonemYiliBox.KeyDown += OnEnterOnay;
        AciklamaBox.KeyDown += OnEnterOnay;
    }

    private void OnEnterOnay(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            EnterOnay = true;
            Hide();
        }
    }

    /// <summary>Enter ile onaylandı mı — DefaultButton yokken ShowAsync None döner, çağırıcı bunu da kabul eder.</summary>
    public bool EnterOnay { get; private set; }

    public long FirmaId { get; set; }
    public string? FirmaKodu { get; set; }

    private string? _firmaUnvan;
    public string? FirmaUnvan
    {
        get => _firmaUnvan;
        set
        {
            _firmaUnvan = value;
            if (FirmaUnvanText != null)
                FirmaUnvanText.Text = value ?? string.Empty;
        }
    }

    public int DonemYili => (int)DonemYiliBox.Value;
    public string Aciklama => AciklamaBox.Text;
}
