using MuhasibPro.Data.DataContext;

namespace MuhasibPro.Data.Contracts.Database.Common;

public interface IAccountingDbContextFactory
{
    AppDbContext CreateDbContext(
        string databaseName,
        int? commandTimeoutSec = null,
        int? busyTimeoutMs = null,
        bool? pooling = null);
    AppDbContext CreateForDb(string dbPath);
    Task<(bool canConnect, string message)> TestConnectionAsync(string databaseName);
}
