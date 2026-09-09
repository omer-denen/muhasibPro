using System.Diagnostics;

namespace MuhasibPro.Views.ShellViews.Splash;

/// <summary>Splash durum satırı: tek mesaj, sıralı sönümle/değiştir/belir (çakışan çağrılarda sonuncu kazanır).</summary>
public sealed partial class SplashStatusControl : UserControl
{
    private string _current = "";
    private int _sequence;

    public SplashStatusControl()
    {
        InitializeComponent();
    }

    public async Task ShowMessageAsync(string message)
    {
        var id = ++_sequence;
        try
        {
            if (MessageText == null || message == _current)
                return;

            FadeOutStoryboard.Stop();
            FadeInStoryboard.Stop();

            if (!string.IsNullOrEmpty(_current))
            {
                FadeOutStoryboard.Begin();
                await Task.Delay(120);
                if (id != _sequence)
                    return;
            }

            _current = message;
            MessageText.Text = message;

            FadeInStoryboard.Begin();
            await Task.Delay(180);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Splash status: {ex.Message}");
        }
    }
}
