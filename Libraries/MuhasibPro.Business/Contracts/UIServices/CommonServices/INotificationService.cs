namespace MuhasibPro.Business.Contracts.UIServices.CommonServices;

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Danger
}

public interface INotificationService
{
    void Show(string title, string message, NotificationType type = NotificationType.Info);

    /// <summary>Etiketli bildirim: aynı tag+group yeni toast eskisini değiştirir (üst üste dizilme kapanır).</summary>
    void ShowTagged(string title, string message, NotificationType type, string tag, string group);
}
