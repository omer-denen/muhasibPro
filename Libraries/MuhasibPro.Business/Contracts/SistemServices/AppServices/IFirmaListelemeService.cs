using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices
{
    /// <summary>Firma okuma işlemleri (listele/seç/say) — IFirmaService'in listeleme kanadı.</summary>
    public interface IFirmaListelemeService
    {
        Task<ApiDataResponse<FirmaModel>> GetByFirmaIdAsync(long firmaId);
        Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarPageAsync(
            int skip,
            int take,
            DataRequest<Firma> request);
        Task<bool> IsFirmaAnyAsync();
        Task<ApiDataResponse<int>> GetFirmalarCountAsync(DataRequest<Firma> request);
        Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarWithUserId(DataRequest<Firma> request, long userId);
        Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarWithMaliDonemler(DataRequest<Firma> request);
    }
}
