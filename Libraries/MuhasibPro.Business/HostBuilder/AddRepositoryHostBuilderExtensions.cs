using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MuhasibPro.Data.Contracts.Repository.Common;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.Contracts.Repository.SistemRepos.Authentication;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Data.Repository.Common;
using MuhasibPro.Data.Repository.Common.BaseRepo;
using MuhasibPro.Data.Repository.SistemRepos;
using MuhasibPro.Data.Repository.SistemRepos.Authentication;
using MuhasibPro.Domain.Entities.SistemEntity;


namespace MuhasibPro.Business.HostBuilder
{
    public static class AddRepositoryHostBuilderExtensions
    {
        public static IHostBuilder AddRepositories(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddScoped<IUnitOfWork<SistemDbContext>, UnitOfWork<SistemDbContext>>();
                services.AddScoped<IUnitOfWork<AppDbContext>, UnitOfWork<AppDbContext>>();
                services.AddScoped<ITransaction, EfTransaction>();
                services.AddSingleton<IPasswordHasher<Kullanici>>(provider => new PasswordHasher<Kullanici>());
                // Yeni parolaların hash gücü modelden (IdentitySettings.Pbkdf2Iterations varsayılanı).
                services.Configure<PasswordHasherOptions>(o => o.IterationCount = new MuhasibPro.Domain.Models.IdentitySettings().Pbkdf2Iterations);
                services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

                ////Repository
                //services.AddScoped<IGenericRepository<SistemLog>, GenericRepository<SistemLog>>();
                //services.AddScoped<IGenericRepository<AppLog>, GenericRepository<AppLog>>();
                //services.AddScoped<IGenericRepository<Firma>, GenericRepository<Firma>>();
                //services.AddScoped<IGenericRepository<Kullanici>, GenericRepository<Kullanici>>();
                services.AddSingleton<ISistemLogRepository, SistemLogRepository>();
                services.AddSingleton<IAppLogRepository, AppLogRepository>();
                services.AddSingleton<IAuthenticationRepository, AuthenticationRepository>();
                services.AddSingleton<IAccountStore, AccountStore>();
                services.AddSingleton<IAuthenticator, Authenticator>();
                services.AddSingleton<IFirmaRepository, FirmaRepository>();
                services.AddSingleton<IMaliDonemRepository, MaliDonemRepository>();
                // Faz 5 M2-politika: DbContext tutan repolar Scoped olur (Singleton = captive).
                // Faz 6.85 K2: IUserRepository de DbContext tutar ve ServiceLocator pencere-başına scope
                // açar; Singleton kalırsa başka pencerenin context'ine yazar (KFR FK ihlali) → Scoped.
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IKullaniciFirmaRolRepository, KullaniciFirmaRolRepository>();
                services.AddScoped<IRolPermissionRepository, RolPermissionRepository>();
                // Faz 6.85 K2: rol kataloğu (rol seçici/listeleme) — DbContext tutar, Scoped.
                services.AddScoped<IKullaniciRolRepository, KullaniciRolRepository>();



            });
            return host;
        }
    }
}
