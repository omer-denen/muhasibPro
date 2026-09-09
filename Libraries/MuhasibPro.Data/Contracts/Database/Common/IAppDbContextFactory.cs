using MuhasibPro.Data.DataContext;

namespace MuhasibPro.Data.Contracts.Database.Common
{
    public interface IAppDbContextFactory
    {
        /// <param name="commandTimeoutSec">EF komut zaman aşımı sn (5-300; null → 30).
        /// TenantSettings'ten gelir (Oturum 127).</param>
        /// <param name="busyTimeoutMs">SQLite busy timeout ms (null → 5000).</param>
        /// <param name="pooling">Bağlantı havuzu (null → true).</param>
        AppDbContext CreateDbContext(
            string databaseName,
            int? commandTimeoutSec = null,
            int? busyTimeoutMs = null,
            bool? pooling = null);
        Task<(bool canConnect, string message)> TestDbContextConnectionAsync(string databaseName);
    }
}
