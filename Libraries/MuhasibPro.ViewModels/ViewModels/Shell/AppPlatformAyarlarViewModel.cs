using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası "Görünüm & Bildirim" bölümünün VM'i (AppPlatform, global).
/// Tek cümle: AppPlatform ayar modelini panele bağlar (oku + değişiklikte otomatik kaydet).</summary>
public class AppPlatformAyarlarViewModel : ViewModelBase
{
    private readonly IAppPlatformSettingsProvider _saglayici;
    private AppPlatformSettings _ayarlar = new();
    private bool _yuklendi;

    public AppPlatformAyarlarViewModel(
        ICommonServices commonServices,
        IAppPlatformSettingsProvider saglayici = null) : base(commonServices)
    {
        _saglayici = saglayici;
    }

    public async Task LoadAsync()
    {
        if (_saglayici != null)
            _ayarlar = await _saglayici.GetAsync() ?? new AppPlatformSettings();
        _yuklendi = true;
        NotifyPropertyChanged(nameof(ThemeDefault));
        NotifyPropertyChanged(nameof(SplashStepDelayMs));
        NotifyPropertyChanged(nameof(StatusAutoHideMs));
        NotifyPropertyChanged(nameof(NotificationEnabled));
    }

    public string ThemeDefault
    {
        get => _ayarlar.ThemeDefault;
        set
        {
            if (_ayarlar.ThemeDefault == value) return;
            _ayarlar.ThemeDefault = value;
            NotifyPropertyChanged(nameof(ThemeDefault));
            _ = SaveAsync();
        }
    }

    public int SplashStepDelayMs
    {
        get => _ayarlar.SplashStepDelayMs;
        set
        {
            if (_ayarlar.SplashStepDelayMs == value) return;
            _ayarlar.SplashStepDelayMs = value;
            NotifyPropertyChanged(nameof(SplashStepDelayMs));
            _ = SaveAsync();
        }
    }

    public int StatusAutoHideMs
    {
        get => _ayarlar.StatusAutoHideMs;
        set
        {
            if (_ayarlar.StatusAutoHideMs == value) return;
            _ayarlar.StatusAutoHideMs = value;
            NotifyPropertyChanged(nameof(StatusAutoHideMs));
            _ = SaveAsync();
        }
    }

    public bool NotificationEnabled
    {
        get => _ayarlar.NotificationEnabled;
        set
        {
            if (_ayarlar.NotificationEnabled == value) return;
            _ayarlar.NotificationEnabled = value;
            NotifyPropertyChanged(nameof(NotificationEnabled));
            _ = SaveAsync();
        }
    }

    public async Task SaveAsync()
    {
        if (!_yuklendi || _saglayici == null)
            return;
        try
        {
            await _saglayici.SaveAsync(_ayarlar);
        }
        catch (UnauthorizedAccessException)
        {
            StatusError("Bu ayar yalnızca yönetici tarafından değiştirilebilir.");
            await LoadAsync();
        }
    }
}
