using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Extensions.SistemService.AppService;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.SistemServices.AppServices
{
    /// <summary>Firma okuma kanadı (listele/seç/say) — FirmaService kompozisyonunun parçası.</summary>
    public class FirmaListelemeService : IFirmaListelemeService
    {
        private readonly IFirmaRepository _firmaRepository;
        private readonly IBitmapToolsService _bitmapTools;
        private readonly ILogService _logService;
        private readonly IAuthenticationService _authenticationService;

        public FirmaListelemeService(
            IFirmaRepository firmaRepository,
            IBitmapToolsService bitmapTools,
            ILogService logService,
            IAuthenticationService authenticationService)
        {
            _firmaRepository = firmaRepository;
            _bitmapTools = bitmapTools;
            _logService = logService;
            _authenticationService = authenticationService;
        }

        public async Task<ApiDataResponse<FirmaModel>> GetByFirmaIdAsync(long firmaId)
        {
            try
            {
                var item = await FirmaServiceExtensions.GetByFirmaIdAsync(_firmaRepository, _bitmapTools, firmaId);
                if (item.Data == null)
                    return new ErrorApiDataResponse<FirmaModel>(data: null, message: item.Message);
                return new SuccessApiDataResponse<FirmaModel>(item.Data, item.Message);
            }
            catch (Exception ex)
            {
                await LogException(nameof(GetByFirmaIdAsync), ex);
                return new ErrorApiDataResponse<FirmaModel>(data: null, message: $"[HATA]: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarPageAsync(
            int skip,
            int take,
            DataRequest<Firma> request)
        {
            var models = new List<FirmaModel>();
            try
            {
                var items = await _firmaRepository.GetFirmalarAsync(skip, take, request);
                if (items == null || items.Count < 0)
                    return new ErrorApiDataResponse<IList<FirmaModel>>(data: models, message: "🔴 Listelenecek veri bulunamadı!");
                foreach (var item in items)
                {
                    var model = await FirmaServiceExtensions.CreateFirmaModelAsync(item, false, _bitmapTools);
                    if (model != null)
                    {
                        model.MaliDonemler = FirmaServiceExtensions.ToHafifDonemListesi(item.MaliDonemler);
                        models.Add(model);
                    }
                }
                return new SuccessApiDataResponse<IList<FirmaModel>>(data: models, message: "✅ İşlem başarılı", resultCount: models.Count);
            }
            catch (Exception ex)
            {
                await LogException(nameof(GetFirmalarPageAsync), ex);
                return new ErrorApiDataResponse<IList<FirmaModel>>(data: models, message: $"[HATA]: {ex.Message}");
            }
        }

        public async Task<bool> IsFirmaAnyAsync() => await _firmaRepository.IsFirmaAnyAsync();

        public async Task<ApiDataResponse<int>> GetFirmalarCountAsync(DataRequest<Firma> request)
        {
            try
            {
                var count = await _firmaRepository.GetFirmalarCountAsync(request);
                if (count == 0)
                    return new ErrorApiDataResponse<int>(data: 0, message: "🔴 Sayılacak veri bulunamadı!");
                return new SuccessApiDataResponse<int>(data: count, message: "✅ İşlem başarılı", resultCount: count);
            }
            catch (Exception ex)
            {
                await LogException(nameof(GetFirmalarCountAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"[HATA]: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarWithUserId(DataRequest<Firma> request, long userId)
        {
            var currentUserId = _authenticationService.GetCurrentUserId;
            var models = new List<FirmaModel>();
            try
            {
                var firmaList = await _firmaRepository.GetFirmaKeysUserIdAsync(request, userId);
                if (firmaList == null || !firmaList.Any())
                    return new ErrorApiDataResponse<IList<FirmaModel>>(data: models, message: "🔴 Listelenecek veri bulunamadı!");
                if (currentUserId > 0 && currentUserId != userId)
                    return new ErrorApiDataResponse<IList<FirmaModel>>(data: models, message: "❌ Başka bir kullanıcının firmalarına erişim izniniz yok.");
                foreach (var item in firmaList)
                {
                    var model = await FirmaServiceExtensions.CreateFirmaModelAsync(item, true, _bitmapTools);
                    if (model != null)
                    {
                        model.MaliDonemler = FirmaServiceExtensions.ToHafifDonemListesi(item.MaliDonemler);
                        models.Add(model);
                    }
                }
                return new SuccessApiDataResponse<IList<FirmaModel>>(data: models, message: "✅ İşlem başarılı");
            }
            catch (Exception ex)
            {
                await LogException(nameof(GetFirmalarWithUserId), ex);
                return new ErrorApiDataResponse<IList<FirmaModel>>(data: null, $"[HATA]: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarWithMaliDonemler(DataRequest<Firma> request)
        {
            var models = new List<FirmaModel>();
            try
            {
                var firmalar = await _firmaRepository.GetFirmalarWithMaliDonemler(request);
                if (!firmalar.Any())
                    return new ErrorApiDataResponse<IList<FirmaModel>>(data: models, message: "🔴 Listelenecek veri bulunamadı!");
                models = firmalar.Select(FirmaServiceExtensions.ToDonemliFirmaModel).ToList();
                return new SuccessApiDataResponse<IList<FirmaModel>>(data: models, message: "✅ Firmalar listelendi", resultCount: models.Count);
            }
            catch (Exception ex)
            {
                await LogException(nameof(GetFirmalarWithMaliDonemler), ex);
                return new ErrorApiDataResponse<IList<FirmaModel>>(data: null, $"[HATA]: {ex.Message}");
            }
        }

        private async Task LogException(string methodName, Exception ex)
        { await _logService.SistemLogService.SistemLogExceptionAsync(nameof(FirmaListelemeService), methodName, ex); }
    }
}
