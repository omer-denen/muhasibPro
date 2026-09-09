using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices
{
    /// <summary>Firma silme işlemleri (tekli/toplu) — IFirmaService'in silme kanadı.</summary>
    public interface IFirmaSilmeService
    {
        Task<ApiDataResponse<int>> DeleteFirmaAsync(long firmaId);
        Task<ApiDataResponse<int>> DeleteFirmaRangeAsync(int index, int length, DataRequest<Firma> request);
    }
}
