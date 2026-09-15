namespace MuhasibPro.Business.Contracts.UIServices;

/// <summary>
/// Geliştirme kipi kapısı (Faz 6.82).
/// Tek cümle: derleme-zamanlı DEV yeteneğinin açık olup olmadığını söyler — kullanıcı ayarı DEĞİL.
/// </summary>
public interface IDevModeProvider
{
    /// <summary>Debug derlemesinde true; Release derlemesinde her zaman false.</summary>
    bool IsEnabled { get; }

    /// <summary>Log ve arayüz damgası (ör. "DEV").</summary>
    string Etiket { get; }
}
