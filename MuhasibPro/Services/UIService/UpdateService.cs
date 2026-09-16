using Microsoft.Extensions.Logging;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Contracts.UIService;
using MuhasibPro.Domain.Models;
using System.Reflection;
using Velopack;

namespace MuhasibPro.Services.UIService;

public class UpdateService : IUpdateService
{
    private readonly ILocalSettingsService _localSettings;
    private readonly ISistemYasamDongusuService _yasamDongusu;
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(
        ILocalSettingsService localSettings,
        ISistemYasamDongusuService yasamDongusu,
        ILogger<UpdateService> logger)
    {
        _localSettings = localSettings;
        _yasamDongusu = yasamDongusu;
        _logger = logger;
    }

    private Velopack.UpdateManager _manager;
    private Velopack.UpdateInfo _lastUpdate;

    /// <summary>UI thread'den await edilir — içeride bloklayan çağrı yok (deadlock'suz).</summary>
    public async Task<bool> IsUpdatePendingRestartAsync()
    {
        try
        {
            if (_manager != null)
                return _manager.UpdatePendingRestart != null;
            var feed = (await GetSettingsAsync())?.FeedUrl;
            if (string.IsNullOrWhiteSpace(feed))
                return false;
            var manager = new Velopack.UpdateManager(BuildSource(feed, false));
            return manager.UpdatePendingRestart != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IsUpdatePendingRestart failed");
            return false;
        }
    }

    public async Task<UpdateSettingsModel> GetSettingsAsync()
    {
        try
        {
            var s = await _localSettings.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey);
            s ??= new UpdateSettingsModel { AutoCheckOnStartup = true, ShowNotifications = true, IncludeBetaVersions = false, LastCheckTime = null };
            // Kaynak adresi girilmemişse derlemede gömülü varsayılan (mevcut git repo adresi) kullanılır.
            if (string.IsNullOrWhiteSpace(s.FeedUrl))
                s.FeedUrl = UpdateSettingsModel.VarsayilanFeedUrl;
            return s;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetSettingsAsync failed");
            return new UpdateSettingsModel { FeedUrl = UpdateSettingsModel.VarsayilanFeedUrl };
        }
    }

    public async Task SaveSettingsAsync(UpdateSettingsModel settings)
    {
        try
        {
            // C2 fix: LastCheckTime yalnız CheckForUpdatesAsync'te set edilir;
            // ayar panelinden "Kaydet" yapılınca son kontrol zamanı ezilmez.
            await _localSettings.SaveSettingAsync(UpdateSettingsModel.SettingsKey, settings);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "SaveSettingsAsync failed"); }
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync(bool includePrereleases = false)
    {
        var settings = await GetSettingsAsync();
        settings.LastCheckTime = DateTime.Now;
        await SaveSettingsAsync(settings);

        if (string.IsNullOrWhiteSpace(settings.FeedUrl))
        {
            _logger.LogInformation("CheckForUpdatesAsync: FeedUrl ayarlanmamış — kontrol atlandı");
            return null;
        }

        try
        {
            _manager = new Velopack.UpdateManager(BuildSource(settings.FeedUrl, includePrereleases));
            _lastUpdate = await _manager.CheckForUpdatesAsync();
            _logger.LogInformation("CheckForUpdatesAsync: {Result}", _lastUpdate == null ? "güncel" : $"güncelleme var {_lastUpdate.TargetFullRelease.Version}");
            return _lastUpdate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckForUpdatesAsync failed");
            throw new InvalidOperationException($"Güncelleme kaynağına ulaşılamadı ({settings.FeedUrl}): {ex.Message}", ex);
        }
    }

    public async Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null)
    {
        if (_manager == null || _lastUpdate == null)
            return false;
        try
        {
            await _manager.DownloadUpdatesAsync(_lastUpdate, p => progress?.Report(p));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DownloadUpdatesAsync failed");
            throw new InvalidOperationException($"Güncelleme indirilemedi: {ex.Message}", ex);
        }
    }

    internal static Velopack.Sources.IUpdateSource BuildSource(string feedUrl, bool includePrereleases)
        => MuhasibPro.Business.Services.UIService.UpdateFeedSourceFactory.Create(feedUrl, includePrereleases);

    public void ApplyUpdatesAndRestart(params string[] restartArgs)
    {
        try
        {
            _logger.LogInformation("ApplyUpdatesAndRestart called with {Args}", string.Join(",", restartArgs));
            if (_manager == null || _lastUpdate == null)
            {
                _logger.LogWarning("ApplyUpdatesAndRestart: manager veya lastUpdate null — güncelleme uygulanamıyor");
                return;
            }
            _manager.ApplyUpdatesAndRestart(_lastUpdate, restartArgs);
        }
        catch (Exception ex) { _logger.LogError(ex, "ApplyUpdatesAndRestart failed"); }
    }

    public void ApplyUpdatesAndRestartWithDatabaseSync(params string[] restartArgs)
    {
        // Database sync is handled in OnRestarted / PostUpdateDatabaseSyncAsync — just restart
        ApplyUpdatesAndRestart(restartArgs);
    }

    public async Task<bool> PrepareForUpdateAsync()
    {
        try
        {
            var settings = await GetSettingsAsync();
            var fromVersion = TryGetCurrentVersion();
            var toVersion = _lastUpdate?.TargetFullRelease?.Version?.ToString();

            // Revizyon 3 (B1/B2): yeni tur — önceki damga/bekleyen bayrak temizlenir; yalnız
            // başarılı ön-yedek sonrası PostUpdatePending yazılır (iptal edilen güncelleme ekran açmaz).
            settings.LastUpdateFromVersion = fromVersion;
            settings.LastUpdateToVersion = toVersion;
            settings.LastUpdateStartTime = DateTime.Now;
            settings.LastUpdateBackupPath = null;
            settings.LastUpdateVerifiedAt = null;
            settings.PostUpdatePending = false;

            _logger.LogInformation("PrepareForUpdate: {From} -> {To} — güncelleme öncesi yedek alınıyor", fromVersion, toVersion);

            // Faz 6.91-B: zorunlu, doğrulanmış Sistem.db yedeği (fail-closed — alınamazsa güncelleme başlamaz).
            var (basarili, yedekYolu, mesaj) = await _yasamDongusu.EnsureUpdateSafetyAsync();
            if (!basarili)
            {
                _logger.LogError("PrepareForUpdate durduruldu: {Mesaj}", mesaj);
                settings.LastUpdateToVersion = null;   // iptal: sonraki açılışta yanlış doğrulama tetiklenmez
                settings.PostUpdatePending = false;
                await SaveSettingsAsync(settings);
                return false;
            }

            settings.LastUpdateBackupPath = yedekYolu;
            settings.PostUpdatePending = true;       // başarılı hazırlık → yeniden başlatma sonrası doğrulama tetiklenir
            await SaveSettingsAsync(settings);
            _logger.LogInformation("PrepareForUpdate tamam: {From} -> {To}, yedek={Yedek}", fromVersion, toVersion, yedekYolu ?? "(gerekmedi)");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PrepareForUpdateAsync failed");
            return false;
        }
    }

    /// <summary>Çalışan sürüm: Velopack current → yoksa assembly sürümü.</summary>
    private string? TryGetCurrentVersion()
    {
        try
        {
            var velo = _manager?.CurrentVersion?.ToString();
            if (!string.IsNullOrWhiteSpace(velo))
                return velo;
        }
        catch { /* Velopack durumu okunamadı — assembly'ye düş */ }

        try
        {
            var asm = Assembly.GetEntryAssembly()?.GetName()?.Version;
            return asm?.ToString(3);
        }
        catch { return null; }
    }

    // Kaynak: Ayarlar sayfasındaki manuel FeedUrl (GitHub repo URL veya Velopack feed URL).
    // CI tarafında `velopack pack/upload` ile üretilmiş feed gerekir.
}
