using MuhasibPro.Contracts.UIService;
using MuhasibPro.Helpers.WindowHelpers;
using MuhasibPro.HostBuilders;

namespace MuhasibPro.Helpers;

/// <summary>
/// ContentDialog için generik, pencereden boyut almadan ortalı helper.
/// Sabit MinWidth/MaxWidth yok — içerik kendi genişliğini belirler, XamlRoot pencere boyutuna göre ortalanır.
/// AGENTS tema kuralı: XAML'de RequestedTheme yasak; dialog tema garantisi bu metotta (tek kaynak).
/// Ayarlanabilir tema: dialog uygulama temasını miras alır (zorlama yok) — popup katmanı kök eleman
/// temasını izlemeyebildiği için pencere teması buradan yazılır.
/// </summary>
public static class DialogHelper
{
    public static async Task<ContentDialogResult> ShowCenteredAsync(ContentDialog dialog)
    {
        // WindowHelper.CurrentXamlRoot Asla null dönmemeli (ViewModelBase.IsMainWindow ile aynı kaynak)
        // IsMainWindow == true ise yeni pencere, değilse mainWindow içinde — her durumda aktif pencerenin XamlRoot'u
        var xamlRoot = WindowHelper.CurrentXamlRoot;
        dialog.XamlRoot = xamlRoot;

        ApplyAppTheme(dialog);

        // Sabit boyut yok — Content kendi ölçüsünde, dialog pencere ortasında
        return await dialog.ShowAsync();
    }

    public static async Task<ContentDialogResult> ShowCenteredAsync<T>(T dialog) where T : ContentDialog
        => await ShowCenteredAsync((ContentDialog)dialog);

    /// <summary>Dialog'u uygulamanın gerçek temasıyla hizalar (tema-bağımlı fırçaların
    /// beyaz-beyaz/koyu-açık karışmasını önler). XAML'de tema zorlaması yok — runtime değeri yazılır.</summary>
    public static void ApplyAppTheme(ContentDialog dialog)
    {
        if (dialog == null)
            return;

        try
        {
            var themeSelector = ServiceLocator.Current.GetService<IThemeSelectorService>(false);
            if (themeSelector != null)
                dialog.RequestedTheme = themeSelector.Theme;
        }
        catch { /* best-effort: tema servisi yoksa miras davranışı korunur */ }
    }
}
