using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.ViewModels.ViewModels.Settings;

/// <summary>Update ayarları saklama (settings plane).
/// Tek cümle: UpdateSettingsModel'i okur/yazar, kayıtta AppSettingsChangedEvent yayınlar.</summary>
public class UpdateSettingsStore
{
    private readonly IUpdateService _updateService;
    private readonly IEventBus _eventBus;

    public UpdateSettingsStore(IUpdateService updateService, IEventBus eventBus)
    {
        _updateService = updateService;
        _eventBus = eventBus;
    }

    /// <summary>Servis yoksa/bozuksa varsayılan model döner (Denetim gömülü akışı kırılmaz).</summary>
    public async Task<UpdateSettingsModel> LoadAsync()
    {
        if (_updateService == null)
            return new UpdateSettingsModel();
        try
        {
            return await _updateService.GetSettingsAsync() ?? new UpdateSettingsModel();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings yükleme hatası: {ex.Message}");
            return new UpdateSettingsModel();
        }
    }

    public async Task SaveAsync(UpdateSettingsModel settings, object publisher)
    {
        if (_updateService == null || settings == null)
            return;
        try
        {
            System.Diagnostics.Debug.WriteLine("Settings saving");
            await _updateService.SaveSettingsAsync(settings);
            _eventBus?.Publish(publisher, new AppSettingsChangedEvent(UpdateSettingsModel.SettingsKey));
            System.Diagnostics.Debug.WriteLine("Settings saved successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings kaydetme hatası: {ex.Message}");
        }
    }
}
