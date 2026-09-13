using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Contracts.UIService;
using MuhasibPro.Domain.Models;
using MuhasibPro.Helpers;
using MuhasibPro.Helpers.WindowHelpers;

namespace MuhasibPro.Services.UIService;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";
    public event EventHandler<ElementTheme> ThemeChanged;
    private List<WeakReference<Window>> _subscribedWindows = new();

    private readonly IEventBus _eventBus;

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
        IAppPlatformSettingsProvider platformSettings,
        IEventBus eventBus = null)
    {
        _localSettingsService = localSettingsService;
        _platformSettings = platformSettings;
        _eventBus = eventBus;

        // Tema ayarı (Denetim Masası → Görünüm) kaydedilince canlı uygula + açılış anahtarına yaz.
        // Provider per-user kaydeder ve AppSettingsChangedEvent yayınlar; tema okunurken global
        // anahtar sorunu bu abonelikle kapanır (HATALAR: "Tema ayarı sahte").
        _eventBus?.Subscribe<AppSettingsChangedEvent>(this, OnAppSettingsChanged);
    }

    private void OnAppSettingsChanged(object sender, AppSettingsChangedEvent e)
    {
        if (!string.Equals(e?.SettingsKey, AppPlatformSettings.SettingsKey, StringComparison.Ordinal))
            return;
        _ = ApplyPlatformThemeAsync();
    }

    /// <summary>Kaydedilmiş platform temasını okuyup canlı uygular (açılış anahtarına da yazar).</summary>
    private async Task ApplyPlatformThemeAsync()
    {
        try
        {
            var platform = await _platformSettings.GetAsync();
            var theme = ElementTheme.Default;
            if (platform != null && Enum.TryParse(platform.ThemeDefault, out ElementTheme parsed))
                theme = parsed;
            if (theme == Theme)
                return;
            await SetThemeAsync(theme);
        }
        catch { /* best-effort: tema uygulanamazsa mevcut tema korunur */ }
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
            if (Enum.TryParse(platform.ThemeDefault, out ElementTheme platformTheme))
                return platformTheme;
        }
        catch { /* best-effort */ }

        // Kayıt yoksa sistem teması takip edilir (Default) — ayarlanabilir tema politikası.
        return ElementTheme.Default;
    }

    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());
    }
}
