using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;

namespace MuhasibPro.Services.CommonServices;
public class CommonServices : ICommonServices
{
    public CommonServices(
        IContextService contextService,
        INavigationService navigationService,
        IMessageService messageService,
        IDialogService dialogService,
        ILogService logService,
        IStatusBarService statusBarService,
        IStatusMessageService statusMessageService,
        INotificationService notificationService)
    {
        ContextService = contextService;
        NavigationService = navigationService;
        MessageService = messageService;
        DialogService = dialogService;
        LogService = logService;
        StatusBarService = statusBarService;
        StatusMessageService = statusMessageService;
        NotificationService = notificationService;
    }
    public IContextService ContextService { get; }
    public INavigationService NavigationService { get; }
    public IMessageService MessageService { get; }
    public IDialogService DialogService { get; }
    public ILogService LogService { get; }
    public ISettingsService SettingsService { get; }
    public IStatusBarService StatusBarService { get; }
    public IStatusMessageService StatusMessageService { get; }
    public INotificationService NotificationService { get; }
}
