using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.ViewModels.ViewModels.Loggings.SistemLogs;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Shell.Tenant;
using MuhasibPro.ViewModels.ViewModels.Sistem;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.HostBuilders;
public static class AddAppViewModelHostBuilderExtensions
{
    public static IHostBuilder AddAppViewModel(this IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<AppPlatformAyarlarViewModel>();
            services.AddSingleton<QuickLoginAccountsViewModel>();
            services.AddTransient<FirmaShellViewModel>();
            services.AddTransient<DenetimMasasiViewModel>();
            services.AddTransient<TenantDatabaseUpdateViewModel>();
            services.AddTransient<ShellViewModel>();

            services.AddTransient<MainShellViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<UpdateViewModel>();
            services.AddTransient<DatabaseSettingsViewModel>();


            services.AddTransient<FirmalarViewModel>();
            services.AddTransient<FirmaDetailsViewModel>();
            services.AddTransient<FirmaDetailsWithMaliDonemlerViewModel>();

            services.AddTransient<MaliDonemViewModel>();
            services.AddTransient<MaliDonemDetailsViewModel>();
            services.AddTransient<MaliDonemCreationViewModel>();
            services.AddTransient<MaliDonemYonetimViewModel>();
            services.AddTransient<YonetimAyarlarViewModel>();

            services.AddTransient<SistemDbYonetimViewModel>();
            services.AddTransient<KurulumSplashViewModel>();

            services.AddTransient<SistemLogsViewModel>();

        });
        return host;
    }

}
