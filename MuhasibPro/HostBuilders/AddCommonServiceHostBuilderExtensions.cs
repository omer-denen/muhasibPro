using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Contracts.UIService;
using MuhasibPro.Services.CommonServices;
using MuhasibPro.Services.UIService;

namespace MuhasibPro.HostBuilders
{
    public static class AddCommonServiceHostBuilderExtensions
    {
        public static IHostBuilder AddCommonServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                // ✅ GLOBAL & STATEFUL servisler - SINGLETON:
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IUpdateService, UpdateService>();
                // ISettingsService kayıtsız: implementasyonu yok (SettingsService sınıfı mevcut değil).
                //services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<ILogService, LogService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IFilePickerService, FilePickerService>();
                services.AddSingleton<ISistemLogService, SistemLogService>();
                services.AddSingleton<IAppLogService, AppLogService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<IMessageService, MessageService>();
                services.AddSingleton<MuhasibPro.Business.Contracts.UIServices.CommonServices.Events.IEventBus, MuhasibPro.Business.Services.CommonServices.EventBus>();
                services.AddSingleton<INotificationService, NotificationService>();
                // Faz 6.86: uygulama içi bildirim merkezi — SCOPED (pencere başına ayrı; bildirim yalnız
                // tetikleyen/aktif pencerede görünür, diğer pencerelere yansımaz).
                services.AddScoped<IInAppMessageService, InAppMessageService>();
                // Faz 6.82: yol açıcı (OS kabuğu — App katmanı implementasyonu).
                services.AddSingleton<IYolAciciService, YolAciciService>();



                // ✅ VIEW/OPERATION başına - SCOPED:
                services.AddScoped<IStatusBarService, StatusBarService>();
                services.AddScoped<IStatusMessageService, StatusMessageService>();
                services.AddScoped<IBitmapToolsService, BitmapToolsService>();
                //services.AddScoped<IWebViewService, WebViewService>();
                services.AddSingleton<IStartupApplicationService, StartupApplicationService>();


                services.AddScoped<ICommonServices, CommonServices>();
                services.AddScoped<IContextService, ContextService>();
                services.AddScoped<INavigationService, NavigationService>();





            });
            return host;
        }
    }
}
