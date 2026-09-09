using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices
{
    /// <summary>Firma kaydetme işlemleri (ekle/güncelle) — IFirmaService'in kaydetme kanadı.</summary>
    public interface IFirmaKayitService
    {
        Task<ApiDataResponse<int>> UpdateFirmaAsync(FirmaModel model);
    }
}
