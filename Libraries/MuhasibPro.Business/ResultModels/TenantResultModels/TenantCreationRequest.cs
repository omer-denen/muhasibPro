using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Business.ResultModels.TenantResultModels
{
    public class TenantCreationRequest
    {
        public long FirmaId { get; set; }
        public long MaliDonemId { get; set; }
        public string DatabaseName { get; set; } = string.Empty;
        public int MaliYil { get; set; }
        public bool AutoCreateDatabase { get; set; } = true;
        public KullaniciRolTip? OlusturanRol { get; set; }
    }
}
