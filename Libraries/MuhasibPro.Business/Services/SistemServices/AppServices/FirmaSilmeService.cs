using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.SistemServices.AppServices
{
    /// <summary>Firma silme kanadı (tekli/toplu) — FirmaService kompozisyonunun parçası.</summary>
    public class FirmaSilmeService : IFirmaSilmeService
    {
        private readonly IFirmaRepository _firmaRepository;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly ILogService _logService;
        private readonly IAuthenticationService _authenticationService;

        public FirmaSilmeService(
            IFirmaRepository firmaRepository,
            IUnitOfWork<SistemDbContext> unitOfWork,
            ILogService logService,
            IAuthenticationService authenticationService)
        {
            _firmaRepository = firmaRepository;
            _unitOfWork = unitOfWork;
            _logService = logService;
            _authenticationService = authenticationService;
        }

        public async Task<ApiDataResponse<int>> DeleteFirmaAsync(long firmaId)
        {
            if (_authenticationService.GetCurrentUserId <= 0)
                return new ErrorApiDataResponse<int>(data: 0, message: "⚠️ Giriş yapmış bir kullanıcı bulunamadı!");
            if (firmaId <= 0)
                return new ErrorApiDataResponse<int>(data: 0, message: "⚠️ Silinecek firma bilgisi boş olamaz!");
            try
            {
                var firma = await _firmaRepository.GetByFirmaIdAsync(firmaId);
                if (firma == null)
                    return new ErrorApiDataResponse<int>(data: 0, message: "🔴 Silinecek firma bulunamadı!");

                await _firmaRepository.DeleteFirmalarAsync(firma);
                var result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    await _logService.SistemLogService
                    .SistemLogInformationAsync(
                        nameof(FirmaSilmeService),
                        nameof(DeleteFirmaAsync),
                        $"Firma başarıyla silindi",
                        $"Etkilenen kayıt: {result}");
                    return new SuccessApiDataResponse<int>(result, "✅ Firma başarıyla silindi.", resultCount: result);
                }
                return new ErrorApiDataResponse<int>(data: 0, "❌ Firma silme işlemi başarısız oldu!");
            }
            catch (Exception ex)
            {
                await LogException(nameof(DeleteFirmaAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"[HATA] ❌ Firma silinemedi! => {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteFirmaRangeAsync(int index, int length, DataRequest<Firma> request)
        {
            if (_authenticationService.GetCurrentUserId <= 0)
                return new ErrorApiDataResponse<int>(data: 0, message: "❌ Giriş yapmış bir kullanıcı bulunamadı!");
            try
            {
                var items = await _firmaRepository.GetFirmaKeysAsync(index, length, request);
                if (items == null || !items.Any())
                    return new ErrorApiDataResponse<int>(data: 0, message: "❌ Silinecek firma bulunamadı!");
                await _firmaRepository.DeleteRangeAsync(items.ToArray());
                var result = await _unitOfWork.SaveChangesAsync();
                await _logService.SistemLogService
                    .SistemLogInformationAsync(
                        nameof(FirmaSilmeService),
                        nameof(DeleteFirmaRangeAsync),
                        $"{items.Count} adet firma başarıyla silindi. Index: {index}, Length: {length}",
                        $"Etkilenen kayıt: {result}");
                return new SuccessApiDataResponse<int>(result, $"{items.Count} adet firma başarıyla silindi", resultCount: items.Count);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task LogException(string methodName, Exception ex)
        { await _logService.SistemLogService.SistemLogExceptionAsync(nameof(FirmaSilmeService), methodName, ex); }
    }
}
