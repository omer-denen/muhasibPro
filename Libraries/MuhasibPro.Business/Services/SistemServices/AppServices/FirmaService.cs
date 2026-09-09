using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.SistemServices.AppServices
{
    /// <summary>
    /// Firma orkestratörü: composition + forward (başka iş yapmaz).
    /// Listeleme → <see cref="IFirmaListelemeService"/>, kaydetme → <see cref="IFirmaKayitService"/>,
    /// silme → <see cref="IFirmaSilmeService"/>.
    /// </summary>
    public class FirmaService : IFirmaService
    {
        private readonly IFirmaListelemeService _listeleme;
        private readonly IFirmaKayitService _kayit;
        private readonly IFirmaSilmeService _silme;

        public FirmaService(
            IFirmaListelemeService listeleme,
            IFirmaKayitService kayit,
            IFirmaSilmeService silme)
        {
            _listeleme = listeleme;
            _kayit = kayit;
            _silme = silme;
        }

        public Task<ApiDataResponse<FirmaModel>> GetByFirmaIdAsync(long firmaId)
            => _listeleme.GetByFirmaIdAsync(firmaId);

        public Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarPageAsync(int skip, int take, DataRequest<Firma> request)
            => _listeleme.GetFirmalarPageAsync(skip, take, request);

        public Task<bool> IsFirmaAnyAsync()
            => _listeleme.IsFirmaAnyAsync();

        public Task<ApiDataResponse<int>> GetFirmalarCountAsync(DataRequest<Firma> request)
            => _listeleme.GetFirmalarCountAsync(request);

        public Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarWithUserId(DataRequest<Firma> request, long userId)
            => _listeleme.GetFirmalarWithUserId(request, userId);

        public Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarWithMaliDonemler(DataRequest<Firma> request)
            => _listeleme.GetFirmalarWithMaliDonemler(request);

        public Task<ApiDataResponse<int>> UpdateFirmaAsync(FirmaModel model)
            => _kayit.UpdateFirmaAsync(model);

        public Task<ApiDataResponse<int>> DeleteFirmaAsync(long firmaId)
            => _silme.DeleteFirmaAsync(firmaId);

        public Task<ApiDataResponse<int>> DeleteFirmaRangeAsync(int index, int length, DataRequest<Firma> request)
            => _silme.DeleteFirmaRangeAsync(index, length, request);
    }
}
