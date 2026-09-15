namespace MuhasibPro.Business.Services.SistemServices.LogServices;

/// <summary>
/// Dosya log yolu tek kaynağı — host kurulumu (FileLoggerProvider) ve geliştirici araçları aynı yeri okur.
/// Tek cümle: çalışma dizini altındaki günlük log dosyasının yolunu verir.
/// </summary>
public static class LogDosyaYolu
{
    public static string Klasor => Path.Combine(Directory.GetCurrentDirectory(), "logs");

    public static string BugununDosyasi => Path.Combine(Klasor, $"muhasib-{DateTime.Now:yyyyMMdd}.txt");
}
