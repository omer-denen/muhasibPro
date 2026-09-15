using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>Güncelleme sayfasına taşınan bağlam (firma + mali dönem + tenant veritabanı).</summary>
    public class TenantDatabaseUpdateArgs
    {
        public string DatabaseName { get; set; } = string.Empty;
        public FirmaModel Firma { get; set; }
        public MaliDonemModel MaliDonem { get; set; }
    }
}
