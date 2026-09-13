using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices
{
    /// <summary>M4 SystemDb ayar sağlayıcısı — yedek saklama + SQLite bağlantı değerleri tek kaynaktan.
    /// firmaId &gt; 0 ise eski firma anahtarı taşınır; kayıt giriş yapanın kullanıcı anahtarına
    /// (<c>SettingsKey:U{id}</c>), yoksa global şablona yazılır (Entity/Tenant deseni).</summary>
    public interface IDatabaseSettingsProvider
    {
        Task<DatabaseSettingsModel> GetAsync(long firmaId = 0);
        Task SaveAsync(DatabaseSettingsModel settings, long firmaId = 0);
    }
}
