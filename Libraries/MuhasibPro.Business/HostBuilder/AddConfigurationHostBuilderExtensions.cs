using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MuhasibPro.Business.Services.SistemServices.LogServices;

namespace MuhasibPro.Business.HostBuilder
{
    public static class AddConfigurationHostBuilderExtensions
    {
        public static IHostBuilder AddConfiguration(this IHostBuilder host)
        {
            host.ConfigureHostConfiguration(
                c =>
                {
                    c.AddJsonFile("appsettings.json");
                });
            host.ConfigureLogging((context, logging) =>
            {
                // Console ve Debug logger'ları
                logging.AddConsole();
                logging.AddDebug();

                // Basit file logging (yol tek kaynak: LogDosyaYolu — geliştirici araçları da aynı yeri okur)
                Directory.CreateDirectory(LogDosyaYolu.Klasor);

                logging.AddProvider(new FileLoggerProvider(LogDosyaYolu.BugununDosyasi));
            });

            return host;
        }
    }
}
