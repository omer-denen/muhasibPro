using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using Velopack;

namespace MuhasibPro.ViewModels.ViewModels.Settings;

/// <summary>Update servis çağrıları + bulunan sürüm bilgisi (data plane).
/// Tek cümle: IUpdateService'i çağırır, UpdateInfo'yu saklar, durum kararını orkestratöre bırakır.</summary>
public class UpdateCheckCoordinator
{
    private readonly IUpdateService _updateService;

    public UpdateCheckCoordinator(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    public UpdateInfo? CurrentUpdateInfo { get; private set; }

    public Task<bool> IsRestartPendingAsync()
        => _updateService?.IsUpdatePendingRestartAsync() ?? Task.FromResult(false);

    /// <summary>Kaynakta sürüm arar; bulunursa saklar, bulunamazsa eski bilgi korunur.</summary>
    public async Task<UpdateInfo?> CheckAsync(bool includeBeta)
    {
        if (_updateService == null)
            return null;
        var info = await _updateService.CheckForUpdatesAsync(includeBeta);
        if (info != null)
            CurrentUpdateInfo = info;
        return info;
    }

    public async Task<bool> DownloadAsync(IProgress<int> progress)
    {
        if (_updateService == null || CurrentUpdateInfo == null)
            return false;
        return await _updateService.DownloadUpdatesAsync(progress);
    }

    public void Apply()
    {
        if (_updateService == null || CurrentUpdateInfo == null)
            return;
        _updateService.ApplyUpdatesAndRestart();
    }
}
