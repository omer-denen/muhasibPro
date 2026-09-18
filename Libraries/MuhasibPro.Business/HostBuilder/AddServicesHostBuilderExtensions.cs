using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.DevServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Business.Services.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Business.Services.Installation;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Business.Services.SistemServices.DevServices;
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
                services.AddScoped<IRolYetkiService, RolYetkiService>();
                services.AddScoped<ILisansService, LisansService>();
                // Faz 6.92: sürüm→özellik kapısı (Scoped — ILisansService Scoped'a yaslanır).
                services.AddScoped<ISurumOzellikService, SurumOzellikService>();
                // Singleton bağımlılarına (ThemeSelector) sızmasın diye Singleton: sadece Singleton'lara yaslanır.
                services.AddSingleton<IAppPlatformSettingsProvider, AppPlatformSettingsProvider>();
                // Faz 6.92: AI asistan ayarları (Singleton — yalnız Singleton'lara yaslanır).
                services.AddSingleton<IAiAsistanSettingsProvider, AiAsistanSettingsProvider>();
                // Faz 6.79: yaşam-döngüsü servisi Singleton — yalnız Singleton'lara yaslanır (manager/operasyon/ayarlar/log).
                services.AddSingleton<ISistemYasamDongusuService, SistemYasamDongusuService>();
                // Faz 6.78 Adım 3: sistem restore tek-kapı analizi (Singleton — manager/snapshot/yol Singleton'larına yaslanır).
                services.AddSingleton<ISistemRestoreAnalizService, SistemRestoreAnalizService>();
                // Faz 6.91-D: güncelleme sonrası doğrulama sagası (Singleton — yalnız Singleton yönetici/servislere yaslanır).
                services.AddSingleton<IUygulamaDosyaDogrulayici, UygulamaDosyaDogrulayici>();
                services.AddSingleton<ISistemDbGocDogrulayici, SistemDbGocDogrulayici>();
                services.AddSingleton<ITenantTaramaDogrulayici, TenantTaramaDogrulayici>();
                services.AddSingleton<IPostUpdateDogrulamaService, PostUpdateDogrulamaService>();
                services.AddScoped<ITenantDatabaseUpdateService, TenantDatabaseUpdateService>();
                services.AddScoped<IGlobalAyarlarService, GlobalAyarlarService>();
                services.AddScoped<IKurulumKayitService, KurulumKayitService>();

                // Faz 6.82: geliştirme kipi kapısı (derleme sabiti) + çalışma-zamanı log eşiği (Singleton).
                services.AddSingleton<IDevModeProvider, DevModeProvider>();
                services.AddSingleton<ILogSeviyesiYoneticisi, LogSeviyesiYoneticisi>();
                // Geliştirici araçları Scoped — Scoped tenant/kurulum servislerine yaslanır.
                services.AddScoped<IDevAraclariService, DevAraclariService>();
                // Modül entegrasyon testleri (donanım POST) — DI çözümüne yaslanır (Scoped).
                services.AddScoped<IModulTestCalistirici, ModulTestCalistirici>();

            });
            return host;
        }
    }
}
