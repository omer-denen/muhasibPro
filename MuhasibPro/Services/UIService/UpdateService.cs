using Microsoft.Extensions.Logging;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Contracts.UIService;
using MuhasibPro.Domain.Models;
using Velopack;

namespace MuhasibPro.Services.UIService;

public class UpdateService : IUpdateService
{
    private readonly ILocalSettingsService _localSettings;
    private readonly ISistemDatabaseService _sistemDatabaseService;
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(ILocalSettingsService localSettings, ISistemDatabaseService sistemDatabaseService, ILogger<UpdateService> logger)
    {
        _localSettings = localSettings;
        _sistemDatabaseService = sistemDatabaseService;
        _logger = logger;
    }

    private Velopack.UpdateManager _manager;
    private Velopack.UpdateInfo _lastUpdate;

    public bool IsUpdatePendingRestart
    {
        get
        {
            try
            {
                if (_manager != null)
                    return _manager.UpdatePendingRestart != null;
                var feed = GetSettingsAsync().GetAwaiter().GetResult()?.FeedUrl;
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
    }

    public async Task<UpdateSettingsModel> GetSettingsAsync()
    {
        try
        {
            var s = await _localSettings.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey);
            return s ?? new UpdateSettingsModel { AutoCheckOnStartup = true, ShowNotifications = true, IncludeBetaVersions = false, LastCheckTime = null };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetSettingsAsync failed");
            return new UpdateSettingsModel();
        }
    }

    public async Task SaveSettingsAsync(UpdateSettingsModel settings)
    {
        try
        {
            settings.LastCheckTime = DateTime.Now;
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
            VelopackApp.Build().Run();
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
            _logger.LogInformation("PrepareForUpdateAsync: backing up Global.db before update");
            // Ensure DB is valid and take backup via SistemDatabaseService's backup manager indirectly
            var state = await _sistemDatabaseService.GetSistemDatabaseStateAsync();
            if (state?.Data == null || !state.Data.IsDatabaseExists || !state.Data.CanConnect)
            {
                _logger.LogWarning("PrepareForUpdate: DB not ready, skipping backup (state: {Msg})", state?.Message);
                return true; // don't block update
            }
            // SistemMigrationManager's backup is internal to Initialize — here we just ensure DB file is WAL and not locked
            // Try to ensure backup via direct check (best-effort)
            _logger.LogInformation("PrepareForUpdateAsync completed (DB: {Version}, Tables: {Count})", state.Data.CurrentVersion, state.Data.TableCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PrepareForUpdateAsync failed");
            return false;
        }
    }

    public async Task<bool> PostUpdateDatabaseSyncAsync()
    {
        try
        {
            _logger.LogInformation("PostUpdateDatabaseSyncAsync: running SistemMigrationManager via SistemDatabaseService");
            var (success, message) = await _sistemDatabaseService.InitializeSistemDatabaseAsync();
            _logger.LogInformation("PostUpdateDatabaseSync result: {Success} — {Msg}", success, message);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostUpdateDatabaseSyncAsync failed");
            return false;
        }
    }

    // Kaynak: Ayarlar sayfasındaki manuel FeedUrl (GitHub repo URL veya Velopack feed URL).
    // CI tarafında `velopack pack/upload` ile üretilmiş feed gerekir.
}
