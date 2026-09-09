using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;

namespace MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices
{
    /// <summary>
    /// Tenant güncelleme akışının UI'sız adımları: durum kontrolü → göç açıklaması → geçiş+yayın.
    /// Onay/arıza dialogları ViewModels katmanındaki Coordinator'dadır, burada dialog yok.
    /// </summary>
    public interface ITenantDatabaseUpdateService
    {
        Task<TenantUpdateCheckResult> CheckUpdateRequiredAsync(string databaseName);
        Task<TenantUpdateSwitchResult> SwitchAndPublishAsync(string databaseName, FirmaModel firma, MaliDonemModel maliDonem);
        /// <summary>Göç sonrası sağlık doğrulaması (bağlantı + şema geçerli + bekleyen göç yok).</summary>
        Task<bool> ValidateAsync(string databaseName);
        string DescribeMigration(string migrationId);
    }
}
