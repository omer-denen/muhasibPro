using MuhasibPro.Domain.Enum.DatabaseEnum;

namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// Bağlı tenant bağlantı bilgisi sözleşmesi — M3 seçim aynası somut Data tipine yaslanmaz.
    /// Implementasyon: <c>MuhasibPro.Data.DataContext.TenantContext</c> (ortak altyapı kural 10).
    /// </summary>
    public interface ITenantConnectionInfo
    {
        string DatabaseName { get; }
        DatabaseType DatabaseType { get; }
        DateTime LoadedAt { get; }
        string ConnectionString { get; }
        bool IsLoaded { get; }
        string Message { get; }
    }
}
