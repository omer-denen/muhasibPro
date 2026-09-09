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
}
