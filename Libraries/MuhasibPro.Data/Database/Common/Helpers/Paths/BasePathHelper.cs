using MuhasibPro.Data.Contracts.Database.Common.Helpers;

namespace MuhasibPro.Data.Database.Common.Helpers.Paths;

/// <summary>
/// Uygulama kök klasörleri ve güvenli dosya işlemleri.
/// Sorumluluk: BaseFolder çözümleme, dizin oluşturma, dosya varlık kontrolü.
/// </summary>
internal sealed class BasePathHelper
{
    private readonly IEnvironmentDetector _environmentDetector;
    private readonly string _applicationName;
    private static readonly Lazy<string> _cachedDevProjectPath = new(() =>
    {
        var currentDir = AppContext.BaseDirectory;
        var dirInfo = new DirectoryInfo(currentDir);
        for (int i = 0; i < 6 && dirInfo?.Parent != null; i++)
        {
            if (dirInfo.GetFiles("*.csproj", SearchOption.TopDirectoryOnly).Length > 0 ||
                dirInfo.GetFiles("*.sln", SearchOption.TopDirectoryOnly).Length > 0)
                return dirInfo.FullName;
            dirInfo = dirInfo.Parent;
        }
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MuhasibPro");
    });

    public BasePathHelper(IEnvironmentDetector environmentDetector, string applicationName = "MuhasibPro")
    {
        _environmentDetector = environmentDetector;
        _applicationName = applicationName ?? "MuhasibPro";
    }

    public string GetAppDataFolderPath()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _applicationName);
        return SafeCreateDirectory(path);
    }

    public string GetDevelopmentProjectFolderPath() => _cachedDevProjectPath.Value;

    public string GetRootDataPath() => _environmentDetector.IsDevelopment() ? GetDevelopmentProjectFolderPath() : GetAppDataFolderPath();

    public bool SafeFileExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;
        try
        {
            if (filePath.Length > 260) return false;
            return File.Exists(filePath);
        }
        catch (PathTooLongException) { return false; }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException) { return false; }
    }

    public string SafeCreateDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentException("Dizin yolu boş olamaz");
        try { Directory.CreateDirectory(directoryPath); return directoryPath; }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
        { throw new InvalidOperationException($"Dizin oluşturulamadı: {directoryPath}", ex); }
    }

    public long GetFileSizeSafe(string filePath)
    {
        if (!SafeFileExists(filePath)) return 0L;
        try { return new FileInfo(filePath).Length; }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { return 0L; }
    }

    public FileInfo? GetFileInfoSafe(string filePath)
    {
        try { return new FileInfo(filePath); }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { return null; }
    }

    public void SafeDeleteFile(string filePath)
    {
        try { if (File.Exists(filePath)) File.Delete(filePath); }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { }
    }
}
