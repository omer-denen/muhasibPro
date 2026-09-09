using MuhasibPro.Data.Database.Common.Helpers;

namespace MuhasibPro.Data.Database.Common.Helpers.Paths;

/// <summary>
/// SQLite dosya doğrulama ve boyut kontrolleri.
/// </summary>
internal sealed class DatabaseValidationProvider
{
    private readonly BasePathHelper _baseHelper;
    private readonly DatabaseStructureProvider _structure;

    private static readonly byte[] _sqliteMagicBytes = new byte[]
    {
        0x53,0x51,0x4C,0x69,0x74,0x65,0x20,0x66,0x6F,0x72,0x6D,0x61,0x74,0x20,0x33,0x00
    };

    public DatabaseValidationProvider(BasePathHelper baseHelper, DatabaseStructureProvider structure)
    {
        _baseHelper = baseHelper;
        _structure = structure;
    }

    public bool IsSqliteDatabaseFileValid(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;
        if (!_baseHelper.SafeFileExists(filePath)) return false;
        var fi = _baseHelper.GetFileInfoSafe(filePath);
        if (fi == null || fi.Length < DatabaseConstants.MIN_SQLITE_FILE_SIZE) return false;
        return ValidateSqliteHeader(filePath);
    }

    public bool IsSistemDatabaseValid() => IsSqliteDatabaseFileValid(_structure.GetSistemDatabaseFilePath());
    public bool IsTenantDatabaseValid(string databaseName) => IsSqliteDatabaseFileValid(_structure.GetTenantDatabaseFilePath(databaseName));

    public bool IsDatabaseSizeValid(string filePath) => _baseHelper.GetFileSizeSafe(filePath) >= DatabaseConstants.MIN_SQLITE_FILE_SIZE;
    public bool IsSistemDatabaseSizeValid() => IsDatabaseSizeValid(_structure.GetSistemDatabaseFilePath());
    public bool IsTenantDatabaseSizeValid(string databaseName) => IsDatabaseSizeValid(_structure.GetTenantDatabaseFilePath(databaseName));

    public void CleanupSqliteWalFiles(string databaseName)
    {
        try
        {
            var dbPath = _structure.GetTenantDatabaseFilePath(databaseName);
            var dir = Path.GetDirectoryName(dbPath);
            var name = Path.GetFileNameWithoutExtension(dbPath);
            foreach (var suffix in new[] { $"{name}-wal", $"{name}-shm" })
            {
                var p = Path.Combine(dir!, suffix);
                _baseHelper.SafeDeleteFile(p);
            }
        }
        catch { }
    }

    private bool ValidateSqliteHeader(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Span<byte> header = stackalloc byte[16];
            int read = fs.Read(header);
            if (read < 16) return false;
            return header.SequenceEqual(_sqliteMagicBytes.AsSpan());
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException) { return false; }
    }
}
