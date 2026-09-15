using System.Diagnostics;
using MuhasibPro.Business.Contracts.UIServices;

namespace MuhasibPro.Services.UIService;

/// <summary>
/// Yol açıcı (Faz 6.82): verilen klasörü/dosyayı varsayılan dosya gezgininde açar.
/// Tek cümle: OS kabuğuna devreder (App katmanı — WinUI/OS bağımlılığı burada kalır).
/// </summary>
public class YolAciciService : IYolAciciService
{
    public bool KlasoruAc(string yol)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(yol))
                return false;

            if (Directory.Exists(yol))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{yol}\"",
                    UseShellExecute = true
                });
                return true;
            }

            if (File.Exists(yol))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{yol}\"",
                    UseShellExecute = true
                });
                return true;
            }
        }
        catch { /* OS reddi/eksik yol → false */ }
        return false;
    }
}
