namespace MuhasibPro.Helpers;

/// <summary>Uygulama sürüm alt-bilgisi tek kaynağı (Kural 7: hardcoded "v1.0" yasak).
/// Paketteyse paket sürümü, dev'de derleme sürümü (3 hane) okunur.</summary>
public static class AppSurumBilgisi
{
    public static string Surum
    {
        get
        {
            try
            {
                if (PackageHelper.IsPackaged)
                {
                    var v = PackageHelper.GetPackageVersion();
                    return $"{v.Major}.{v.Minor}.{v.Build}";
                }
            }
            catch { }
            try
            {
                var v = typeof(AppSurumBilgisi).Assembly.GetName().Version;
                if (v != null)
                    return $"{v.Major}.{v.Minor}.{v.Build}";
            }
            catch { }
            return "0.0.0";
        }
    }

    /// <summary>Dev modunda assembly derleme zamanı: bayat-exe tuzağını yakalar.</summary>
    public static string DerlemeZamani
    {
        get
        {
            try
            {
                // Single-file ve normal WinUI3 için: çalışan exe'nin yazılma tarihini oku.
                var dir = AppContext.BaseDirectory;
                var exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
                var exePath = System.IO.Path.Combine(dir, exeName);
                if (System.IO.File.Exists(exePath))
                    return System.IO.File.GetLastWriteTime(exePath).ToString("dd.MM HH:mm");
            }
            catch { }
            return string.Empty;
        }
    }

    public static string Metin
    {
        get
        {
            var zaman = DerlemeZamani;
            return string.IsNullOrEmpty(zaman)
                ? $"v{Surum} Pro Multi-Tenant"
                : $"v{Surum} Pro Multi-Tenant • {zaman}";
        }
    }
}
