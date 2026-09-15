using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MuhasibPro.Helpers;

/// <summary>Tek örnek koruması: ikinci çalıştırma "zaten açık" uyarısıyla kapanır.
/// Kilit oturum kapsamındadır; süreç ömrü boyunca tutulur (Dispose gerekmez, OS bırakır).</summary>
public static class SingleInstanceGuard
{
    private const string MutexName = "MuhasibPro.App.SingleInstance";
    private const string PencereBasligi = "MuhasibPro";

    private static Mutex? _mutex;

    /// <summary>İlk örnekse true döner ve kilidi tutar. Güncelleme sonrası hızlı yeniden
    /// başlatmalarda eski süreç kapanışını tolere etmek için kısa bekleme tanınır.</summary>
    public static bool TryAcquire(TimeSpan? bekleme = null)
    {
        try
        {
            _mutex = new Mutex(initiallyOwned: false, MutexName, out _);
            try
            {
                return _mutex.WaitOne(bekleme ?? TimeSpan.FromSeconds(3));
            }
            catch (AbandonedMutexException)
            {
                // Önceki örnek çökmüş: kilidi devral (kilit sahipsiz, veri riski yok).
                return true;
            }
        }
        catch
        {
            // Kilit altyapısı kurulamazsa uygulama engellenmez.
            return true;
        }
    }

    /// <summary>İkinci örneğe "zaten açık" uyarısını gösterir; açık pencereyi öne getirmeyi dener.</summary>
    public static void ShowAlreadyRunningWarning()
    {
        try
        {
            var hwnd = PInvoke.FindWindow(null, PencereBasligi);
            if (hwnd != HWND.Null)
            {
                PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_RESTORE);
                PInvoke.SetForegroundWindow(hwnd);
            }
        }
        catch { /* best-effort: pencere bulunamazsa yalnız uyarı gösterilir */ }

        try
        {
            PInvoke.MessageBox(
                HWND.Null,
                "MuhasibPro zaten açık.\n\nAynı anda yalnızca bir uygulama penceresi çalıştırılabilir. Açık olan pencereyi kullanmaya devam edin.",
                PencereBasligi,
                MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONINFORMATION | MESSAGEBOX_STYLE.MB_TOPMOST);
        }
        catch { /* best-effort */ }
    }
}
