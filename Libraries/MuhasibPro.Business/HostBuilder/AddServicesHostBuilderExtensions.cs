using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Business.Services.Installation;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Business.Services.UIService;

namespace MuhasibPro.Business.HostBuilder
{
    public static class AddServicesHostBuilderExtensions
    {
        public static IHostBuilder AddBusinessServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddSingleton<ModelFactory>();
                services.AddSingleton<MuhasibPro.Business.Contracts.SistemServices.Authentication.IIdentitySettingsProvider, IdentitySettingsProvider>();
                services.AddSingleton<ILicenseSettingsProvider, LicenseSettingsProvider>();
                services.AddSingleton<IEntityRegistrySettingsProvider, EntityRegistrySettingsProvider>();
                services.AddSingleton<ITenantSettingsProvider, TenantSettingsProvider>();
                services.AddSingleton<IDatabaseSettingsProvider, DatabaseSettingsProvider>();
                services.AddSingleton<IFirmaKullaniciCozucu, FirmaKullaniciCozucu>();
                services.AddSingleton<IAuthenticationService, AuthenticationService>();
                services.AddSingleton<IFirmaListelemeService, FirmaListelemeService>();
                services.AddSingleton<IFirmaKayitService, FirmaKayitService>();
                services.AddSingleton<IFirmaSilmeService, FirmaSilmeService>();
                services.AddSingleton<IFirmaService, FirmaService>();
                services.AddSingleton<IMaliDonemService, MaliDonemService>();
                services.AddSingleton<IFirmaWithMaliDonemSelectedService, FirmaWithMaliDonemSelectedService>();
                                
                services.AddSingleton<ILogService, LogService>();
                services.AddSingleton<IAppLogService, AppLogService>();
                services.AddSingleton<ISistemLogService, SistemLogService>();

                services.AddScoped<ISistemDiagnosticsService, SistemDiagnosticsService>();
                services.AddScoped<ISplashRoutingService, SplashRoutingService>();
                // Faz 5 M2-politika: yetki/lisans servisleri Scoped (Scoped DbContext/repo'ya yaslanır).
                services.AddScoped<IPermissionService, PermissionService>();
                services.AddScoped<IModuleLicenseService, ModuleLicenseService>();
                services.AddScoped<IKullaniciService, KullaniciService>();
                services.AddScoped<ILisansService, LisansService>();
                // Singleton bağımlılarına (ThemeSelector) sızmasın diye Singleton: sadece Singleton'lara yaslanır.
                services.AddSingleton<IAppPlatformSettingsProvider, AppPlatformSettingsProvider>();
                // Faz 6.79: yaşam-döngüsü servisi Singleton — yalnız Singleton'lara yaslanır (manager/operasyon/ayarlar/log).
                services.AddSingleton<ISistemYasamDongusuService, SistemYasamDongusuService>();
                // Faz 6.78 Adım 3: sistem restore tek-kapı analizi (Singleton — manager/snapshot/yol Singleton'larına yaslanır).
                services.AddSingleton<ISistemRestoreAnalizService, SistemRestoreAnalizService>();
                services.AddScoped<ITenantDatabaseUpdateService, TenantDatabaseUpdateService>();
                services.AddScoped<IGlobalAyarlarService, GlobalAyarlarService>();
                services.AddScoped<IKurulumKayitService, KurulumKayitService>();
                
            });
            return host;
        }
    }
}
