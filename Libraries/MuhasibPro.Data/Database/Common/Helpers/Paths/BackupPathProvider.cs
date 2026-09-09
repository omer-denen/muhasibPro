using MuhasibPro.Data.Database.Common.Helpers;

namespace MuhasibPro.Data.Database.Common.Helpers.Paths;

/// <summary>
/// Yedek klasör yapısı.
/// </summary>
internal sealed class BackupPathProvider
{
    private readonly DatabaseStructureProvider _structure;

    public BackupPathProvider(DatabaseStructureProvider structure) => _structure = structure;

    public string GetBackupFolderPath()
    {
        var path = Path.Combine(_structure.GetDatabasesFolderPath(), DatabaseConstants.BACKUP_FOLDER);
        return Directory.CreateDirectory(path).FullName;
    }

    public string GetTenantBackupFolderPath()
    {
        var path = Path.Combine(GetBackupFolderPath(), DatabaseConstants.TENANT_BACKUPS_FOLDER);
        return Directory.CreateDirectory(path).FullName;
    }
}
