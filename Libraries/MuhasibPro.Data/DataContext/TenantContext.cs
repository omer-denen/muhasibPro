using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Data.DataContext
{
    /// <summary>
    /// Immutable tenant context - Minimal implementation for TenantSQLiteSelectionManager
    /// </summary>
    public class TenantContext : ITenantConnectionInfo
    {
        public string DatabaseName { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public DateTime LoadedAt { get; set; }
        public string ConnectionString { get; set; }
        public bool IsLoaded => !string.IsNullOrEmpty(DatabaseName);
        public string Message { get; set; }
        public static TenantContext Empty => new TenantContext();
    }
}