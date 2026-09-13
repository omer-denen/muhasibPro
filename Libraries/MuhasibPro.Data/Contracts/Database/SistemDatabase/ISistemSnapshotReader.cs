using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Data.Contracts.Database.SistemDatabase
{
    /// <summary>
    /// Faz 6.78 Adım 3: bir Sistem.db dosyasını (mevcut veya yedek) salt-okunur açıp
    /// restore farkı için gereken satır/kimlik bilgisini okur. Yazma yapmaz, fırlatmaz.
    /// </summary>
    public interface ISistemSnapshotReader
    {
        Task<SistemSnapshot> ReadAsync(string databaseFilePath);
    }
}
