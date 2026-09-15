using MuhasibPro.Business.Contracts.UIServices;

namespace MuhasibPro.Business.Services.UIService;

/// <summary>
/// Derleme-zamanlı geliştirme kipi (Faz 6.82). Debug'da açık, Release'de kapalı.
/// Tek sınıf — ayrı bir NullProvider sınıfı yerine derleme sabiti (Kural 4 tekilleştirme).
/// </summary>
public class DevModeProvider : IDevModeProvider
{
#if DEBUG
    public bool IsEnabled => true;
#else
    public bool IsEnabled => false;
#endif

    public string Etiket => "DEV";
}
