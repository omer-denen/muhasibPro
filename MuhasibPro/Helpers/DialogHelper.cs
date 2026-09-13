using MuhasibPro.Helpers.WindowHelpers;

namespace MuhasibPro.Helpers;

/// <summary>
/// ContentDialog için generik, pencereden boyut almadan ortalı helper.
/// Sabit MinWidth/MaxWidth yok — içerik kendi genişliğini belirler, XamlRoot pencere boyutuna göre ortalanır.
/// AGENTS tema kuralı: XAML'de RequestedTheme yasak; dialog tema garantisi bu metotta (tek kaynak).
/// Ayarlanabilir tema: dialog uygulama temasını miras alır (zorlama yok).
/// </summary>
public static class DialogHelper
{
    public static async Task<ContentDialogResult> ShowCenteredAsync(ContentDialog dialog)
    {
        // WindowHelper.CurrentXamlRoot Asla null dönmemeli (ViewModelBase.IsMainWindow ile aynı kaynak)
        // IsMainWindow == true ise yeni pencere, değilse mainWindow içinde — her durumda aktif pencerenin XamlRoot'u
        var xamlRoot = WindowHelper.CurrentXamlRoot;
        dialog.XamlRoot = xamlRoot;

        // Ayarlanabilir tema: dialog ana temayı miras alır — açık/koyu zorlanmaz.

        // Sabit boyut yok — Content kendi ölçüsünde, dialog pencere ortasında
        return await dialog.ShowAsync();
    }

    public static async Task<ContentDialogResult> ShowCenteredAsync<T>(T dialog) where T : ContentDialog
        => await ShowCenteredAsync((ContentDialog)dialog);
}
