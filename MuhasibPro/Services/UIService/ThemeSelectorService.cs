using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Contracts.UIService;
using MuhasibPro.Helpers;
using MuhasibPro.Helpers.WindowHelpers;

namespace MuhasibPro.Services.UIService;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";
    public event EventHandler<ElementTheme> ThemeChanged;
    private List<WeakReference<Window>> _subscribedWindows = new();


    private ElementTheme _theme = ElementTheme.Default;
    public ElementTheme Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            ThemeChanged?.Invoke(this, value);
        }
    }

    private readonly ILocalSettingsService _localSettingsService;
    private readonly IAppPlatformSettingsProvider _platformSettings;

    public ThemeSelectorService(
        ILocalSettingsService localSettingsService,
        IAppPlatformSettingsProvider platformSettings)
    {
        _localSettingsService = localSettingsService;
        _platformSettings = platformSettings;
    }

    public async Task InitializeAsync()
    {
        Theme = await LoadThemeFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;

        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

    public async Task SetRequestedThemeAsync()
    {
        foreach (var window in WindowHelper.GetAllWindows())
        {
            if (window.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = Theme;

                TitleBarHelper.UpdateTitleBar(Theme);
            }
        }


        await Task.CompletedTask;
    }

    private async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            if (cacheTheme == ElementTheme.Default)
                return await LoadPlatformDefaultAsync();
            return cacheTheme;
        }

        // Kayıt yoksa platform ayarı (modelden gelen gerçek değer)
        return await LoadPlatformDefaultAsync();
    }

    private async Task<ElementTheme> LoadPlatformDefaultAsync()
    {
        try
        {
            var platform = await _platformSettings.GetAsync();
            if (Enum.TryParse(platform.ThemeDefault, out ElementTheme platformTheme)
                && platformTheme != ElementTheme.Default)
                return platformTheme;
        }
        catch { /* best-effort */ }

        // OOBE: ilk açılışta Light — Windows 11 kurulum beyazı, splash dahil
        return ElementTheme.Light;
    }

    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());
    }
}
