using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Shell.Tenant;
using MuhasibPro.ViewModels.ViewModels.Sistem;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using MuhasibPro.Views.Firma;
using MuhasibPro.Views.Firmalar;
using MuhasibPro.Views.Login;
using MuhasibPro.Views.MainShell;
using MuhasibPro.Views.MaliDonem;
using MuhasibPro.Views.Settings;
using MuhasibPro.Views.ShellViews.Shell;
using MuhasibPro.Views.SistemKurulum;

namespace MuhasibPro.Configurations
{
    public class Startup
    {
        private static readonly Lazy<Startup> _instance = new Lazy<Startup>(() => new Startup());

        public static Startup Instance => _instance.Value;

       
        private Startup()
        {
        }

        public Task ConfigureAsync()
        {
            ConfigureNavigation();
            // InitializeSistemDatabase otomatik çağrılmıyor — Splash dbReady kontrolü ve SistemKurulumView Kurulumu Başlat manuel akışı kullanılacak
            return Task.CompletedTask;
        }

        public void ConfigureNavigation()
        {
            NavigationService.Register<SistemKurulumViewModel, SistemKurulumView>();
            NavigationService.Register<LoginViewModel, LoginView>();
            NavigationService.Register<ShellViewModel, ShellView>();
            NavigationService.Register<MainShellViewModel, MainShellView>();
            NavigationService.Register<FirmaShellViewModel,FirmaShellView>();
            NavigationService.Register<TenantDatabaseUpdateViewModel,TenantDatabaseUpdateView>();
            
            

            //NavigationService.Register<DashboardViewModel, DashboardView>();
            //NavigationService.Register<SettingsViewModel, SettingsView>();
            NavigationService.Register<UpdateViewModel, UpdateView>();
            NavigationService.Register<DatabaseSettingsViewModel, DatabaseSettingsView>();
            //NavigationService.Register<SistemLogsViewModel, SistemLogsView>();

            NavigationService.Register<FirmaDetailsViewModel, FirmaView>();
            NavigationService.Register<FirmalarViewModel, FirmalarView>();

            NavigationService.Register<MaliDonemDetailsViewModel, MaliDonemView>();
            NavigationService.Register<MaliDonemYonetimViewModel, MaliDonemYonetimView>();
        }
        
        public async Task<(bool isValid, string message)> InitializeSistemDatabase()
        {
            try
            {
                var sistemDbService = ServiceLocator.Current.GetService<ISistemMigrationManager>();
                var statusBarService = ServiceLocator.Current.GetService<IStatusBarService>();

                var initilize = await sistemDbService.InitializeSistemDatabaseAsync();

                if (!initilize.initializeState)
                {
                    return (false, initilize.message);
                }                
                statusBarService.DatabaseConnectionMessage = initilize.message;
                
                return (true, initilize.message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }


}
