namespace MuhasibPro.Data.Contracts.Database.TenantDatabase
{
    public interface ITenantSQLiteConnectionStringFactory
    {
        /// <summary>
        /// Database adı ve tip'e göre connection string oluşturur
        /// </summary>
        /// <param name="databaseName">Database adı (örn: FIRMA001_2024)</param>
        /// <param name="dbType">Veritabanı tipi (SQLite, SqlServer)</param>
        /// <param name="busyTimeoutMs">SQLite busy timeout (ms, 1000-30000; null → 5000).
        /// TenantSettings'ten gelir (Oturum 127 derin bağlantı).</param>
        /// <param name="pooling">Bağlantı havuzu (null → true). TenantSettings'ten gelir.</param>
        /// <returns>Connection string</returns>
        string CreateConnectionString(string databaseName, int? busyTimeoutMs = null, bool? pooling = null);
        Task<(bool canConnect, string message, string connectionString)> ValidateConnectionStringAsync(
            string databaseName, int? busyTimeoutMs = null, bool? pooling = null);


    }
}
