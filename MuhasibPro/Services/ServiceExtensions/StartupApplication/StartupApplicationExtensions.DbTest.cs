using MuhasibPro.Configurations;
using MuhasibPro.Contracts.UIService;

namespace MuhasibPro.Services.ServiceExtensions.StartupApplication
{
    public static partial class StartupApplicationExtensions
    {
        public static async Task ExecuteDatabaseValidationAsync(
            this IStartupApplicationService startupService,           
            CancellationToken cancellationToken = default)
        {
            await startupService.ExecuteStepWithProgressAsync(
                StartupStep.DatabaseValidation,
                "Veritabanı doğrulama",
                async (service, ct) =>
                {
                    // 1. Başlangıç
                    await service.ReportSubProgressAsync("Veritabanı bağlantısı kontrol ediliyor...", 10, ct);

                    // 2. DB Test — SADECE ANALİZ, OLUŞTURMA YOK (Oluşturma SistemKurulumView'da)
                    // Startup.Instance.InitializeSistemDatabase() silindi — o metot yoksa otomatik oluşturuyordu, splash SistemKurulum'a gidemiyordu
                    var svc = MuhasibPro.HostBuilders.ServiceLocator.Current.GetService<MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices.ISistemDatabaseService>();
                    var resp = svc != null ? await svc.GetSistemDatabaseStateAsync() : null;
                    var state = resp?.Data;
                    bool isValid = state != null && state.IsDatabaseExists && state.CanConnect && !state.HasError && state.DatabaseValid;
                    string msg = state?.Message ?? resp?.Message ?? "Durum alınamadı";

                    // Detaylı durum — kullanıcı Splash'ta ne olduğunu görsün
                    string detailMsg;
                    if (state == null)
                        detailMsg = "Veritabanı durumu alınamadı";
                    else if (!state.IsDatabaseExists)
                        detailMsg = "Veritabanı dosyası bulunamadı — ilk kurulum gerekiyor";
                    else if (!state.CanConnect)
                        detailMsg = "Veritabanı dosyası mevcut ancak bağlantı kurulamadı";
                    else if (state.HasError)
                        detailMsg = $"Veritabanı hasarlı: {msg}";
                    else if (!state.DatabaseValid)
                        detailMsg = "Veritabanı yapısı geçersiz — onarım veya yeniden kurulum gerekiyor";
                    else
                        detailMsg = msg;

                    // Tek kaynak: StartupService'e yaz — Splash buradan okur, DB'ye tekrar bakmaz
                    if (service is Services.UIService.StartupApplicationService concrete)
                        concrete.IsDatabaseReady = isValid;

                    // 3. Sonuç — her durumda success, Splash DB durumuna göre yönlendirir
                    if (isValid)
                    {
                        await service.ReportSubProgressAsync($"{detailMsg}", 50, ct);
                        await Task.Delay(300, ct);

                        await service.ReportSubProgressAsync("Veritabanı hazırlanıyor...", 80, ct);
                        await Task.Delay(200, ct);
                    }
                    else
                    {
                        await service.ReportSubProgressAsync($"{detailMsg}", 50, ct);
                        await Task.Delay(300, ct);
                        await service.ReportSubProgressAsync("Kurulum ekranı hazırlanıyor...", 80, ct);
                        await Task.Delay(200, ct);
                    }
                },
                cancellationToken);
        }
    }
}
