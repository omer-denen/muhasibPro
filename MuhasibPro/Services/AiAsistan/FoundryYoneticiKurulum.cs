using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
using FoundryLogLevel = Microsoft.AI.Foundry.Local.LogLevel;

namespace MuhasibPro.Services.AiAsistan;

/// <summary>
/// Faz 6.93 fix (Oturum 281): Foundry yöneticisi süreç-başına tek kez kurulur.
/// İki Singleton servis (sohbet + vektör üretici) ayrı bayrakla `CreateAsync` çağırıyordu;
/// ikinci çağrının SDK'da düşme riskine karşı tek kapı (Kural 4: aynı iş tek sınıfta).
/// </summary>
internal static class FoundryYoneticiKurulum
{
    private const string UygulamaAdi = "MuhasibPro";

    private static readonly SemaphoreSlim Kapi = new(1, 1);
    private static bool _kuruldu;

    internal static bool Kuruldu => _kuruldu;

    internal static async Task KurAsync()
    {
        if (_kuruldu)
            return;
        await Kapi.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_kuruldu)
                return;
            await FoundryLocalManager.CreateAsync(
                new Configuration { AppName = UygulamaAdi, LogLevel = FoundryLogLevel.Information },
                NullLogger.Instance).ConfigureAwait(false);
            _kuruldu = true;
        }
        finally
        {
            Kapi.Release();
        }
    }
}
