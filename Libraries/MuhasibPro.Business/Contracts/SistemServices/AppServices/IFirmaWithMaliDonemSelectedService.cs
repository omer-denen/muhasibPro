using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices
{
    public interface IFirmaWithMaliDonemSelectedService
    {
        FirmaModel SelectedFirma { get; set; }
        MaliDonemModel SelectedMaliDonem { get; set; }
        ITenantConnectionInfo ConnectedTenantDb { get; set; }
        event Action StateChanged;
    }
}
