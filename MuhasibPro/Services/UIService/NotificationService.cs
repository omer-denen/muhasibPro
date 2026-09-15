using System.Diagnostics;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.HostBuilders;

namespace MuhasibPro.Services.UIService;

/// <summary>
/// Uygulama içi bildirim servisi. OS toast (ToastNotificationManager + CommunityToolkit +
/// AUMID/kısayol hack'i) tamamen kaldırıldı (Oturum 270 kararı); bildirimler aktif pencerenin
/// <see cref="IInAppMessageService"/> kanalına yayılır (pencere başına ayrı — yansımaz).
/// <c>NotificationEnabled</c> kapalıysa sessizdir.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IAppPlatformSettingsProvider _ayarlar;

    public NotificationService(IAppPlatformSettingsProvider ayarlar = null!)
    {
        _ayarlar = ayarlar;
    }

    public void Show(string title, string message, NotificationType type = NotificationType.Info)
        => ShowTagged(title, message, type, string.Empty, string.Empty);

    public void ShowTagged(string title, string message, NotificationType type, string tag, string group)
        => _ = YayinlaAsync(title, message, type, tag, group);

    private async Task YayinlaAsync(string title, string message, NotificationType type, string tag, string group)
    {
        try
        {
            if (_ayarlar != null)
            {
                var ayar = await _ayarlar.GetAsync();
                if (ayar != null && !ayar.NotificationEnabled)
                    return;
            }

            // Aktif pencerenin kanalı (Scoped) — bildirim yalnız o pencerede görünür.
            var inApp = ServiceLocator.Current.GetService<IInAppMessageService>();
            inApp?.Yayinla(new InAppBildirim
            {
                Baslik = title ?? string.Empty,
                Mesaj = message ?? string.Empty,
                Tur = type,
                Etiket = tag ?? string.Empty,
                Grup = group ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            // Bildirim hatası uygulama akışını kesmez.
            Debug.WriteLine($"NotificationService hatası: {ex.Message}");
        }
    }
}
