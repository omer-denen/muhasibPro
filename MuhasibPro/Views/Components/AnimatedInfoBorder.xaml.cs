using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.Views.Components;

public sealed partial class AnimatedInfoBorder : UserControl
{
    private CancellationTokenSource? _cts;

    public AnimatedInfoBorder()
    {
        InitializeComponent();
    }

    public async void ShowAsync(string text, int visibleMs = 2200)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        InfoText.Text = text;
        BorderRoot.Opacity = 0;
        BorderTransform.TranslateY = 8;
        BorderRoot.Visibility = Visibility.Visible;
        ShowStoryboard.Begin();

        try
        {
            await Task.Delay(visibleMs, token);
            HideStoryboard.Begin();
            await Task.Delay(200, token);
            BorderRoot.Visibility = Visibility.Collapsed;
        }
        catch (TaskCanceledException) { }
    }
}
