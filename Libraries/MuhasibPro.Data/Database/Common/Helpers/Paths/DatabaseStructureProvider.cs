using System.Text;
using MuhasibPro.Data.Database.Common.Helpers;

namespace MuhasibPro.Data.Database.Common.Helpers.Paths;

/// <summary>
/// Veritabanı dosya yapısı ve isim sanitizasyonu.
/// Sorumluluk: Databases/ klasörü, Sistem.db ve Tenant db yolları, isim temizleme.
/// </summary>
internal sealed class DatabaseStructureProvider
{
    private readonly BasePathHelper _baseHelper;
    private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly HashSet<string> _reservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON","PRN","AUX","NUL","COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9","LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9"
    };

    public DatabaseStructureProvider(BasePathHelper baseHelper) => _baseHelper = baseHelper;

    public string GetDatabasesFolderPath()
    {
        var path = Path.Combine(_baseHelper.GetRootDataPath(), DatabaseConstants.DATABASE_FOLDER);
        return _baseHelper.SafeCreateDirectory(path);
    }

    public string GetTenantDatabaseFolderPath()
    {
        var path = Path.Combine(GetDatabasesFolderPath(), DatabaseConstants.TENANT_DATABASES_FOLDER);
        return _baseHelper.SafeCreateDirectory(path);
    }

    public string GetSistemDatabaseFilePath() => Path.Combine(GetDatabasesFolderPath(), DatabaseConstants.SISTEM_DB_NAME);

    public string GetTenantDatabaseFilePath(string databaseName)
    {
        var sanitized = SanitizeDatabaseName(databaseName);
        var tenantPath = GetTenantDatabaseFolderPath();
        if (!sanitized.AsSpan().EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            return Path.Combine(tenantPath, sanitized + ".db");
        return Path.Combine(tenantPath, sanitized);
    }

    public string SanitizeDatabaseName(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("Database adı boş olamaz");
        var trimmed = databaseName.Trim();
        if (trimmed.Length > DatabaseConstants.MAX_DATABASE_NAME_LENGTH)
            throw new ArgumentException($"Database adı {DatabaseConstants.MAX_DATABASE_NAME_LENGTH} karakterden uzun olamaz");
        var sanitized = new StringBuilder(trimmed.Length);
        foreach (char c in trimmed)
            if (Array.IndexOf(_invalidFileNameChars, c) == -1) sanitized.Append(c);
        var result = sanitized.ToString().Trim();
        if (string.IsNullOrEmpty(result)) throw new ArgumentException("Database adı geçersiz");
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(result);
        if (_reservedNames.Contains(fileNameWithoutExt)) throw new ArgumentException($"'{result}' rezerve bir dosya adıdır");
        if (!result.EndsWith(".db", StringComparison.OrdinalIgnoreCase)) result += ".db";
        return result;
    }
}
