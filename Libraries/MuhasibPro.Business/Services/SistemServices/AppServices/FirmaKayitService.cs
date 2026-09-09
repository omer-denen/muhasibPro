using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Extensions.SistemService.AppService;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.SistemServices.AppServices
{
    /// <summary>Firma kaydetme kanadı (ekle/güncelle) — kod deseni EntityRegistrySettings'ten okunur.</summary>
    public class FirmaKayitService : IFirmaKayitService
    {
        private readonly IFirmaRepository _firmaRepository;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly ILogService _logService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IBitmapToolsService _bitmapTools;
        private readonly IEntityRegistrySettingsProvider _ayarlar;

        public FirmaKayitService(
            IFirmaRepository firmaRepository,
            IUnitOfWork<SistemDbContext> unitOfWork,
            ILogService logService,
            IAuthenticationService authenticationService,
            IBitmapToolsService bitmapTools,
            IEntityRegistrySettingsProvider ayarlar)
        {
            _firmaRepository = firmaRepository;
            _unitOfWork = unitOfWork;
            _logService = logService;
            _authenticationService = authenticationService;
            _bitmapTools = bitmapTools;
            _ayarlar = ayarlar;
        }

        public async Task<ApiDataResponse<int>> UpdateFirmaAsync(FirmaModel model)
        {
            if (_authenticationService.GetCurrentUserId <= 0)
                return new ErrorApiDataResponse<int>(data: 0, message: "⚠️ Giriş yapmış bir kullanıcı bulunamadı!");
            if (model == null)
                return new ErrorApiDataResponse<int>(data: 0, message: "⚠️ İşlem yapılacak firma bilgisi boş olamaz!");

            var ayar = await _ayarlar.GetAsync();
            if (ayar.ValidationStrict
                && !string.IsNullOrWhiteSpace(model.FirmaKodu)
                && !FirmaKodHelper.IsValid(model.FirmaKodu, ayar.GetFirmaKodPattern()))
                return new ErrorApiDataResponse<int>(data: 0, message: $"⚠️ Firma kodu desene uymuyor ({ayar.GetFirmaKodPattern()}).");

            long firmaId = model.Id;
            try
            {
                var firma = firmaId > 0 ? await _firmaRepository.GetByFirmaIdAsync(firmaId) : new Firma();
                if (firmaId == 0)
                    firma.KaydedenId = _authenticationService.GetCurrentUserId;
                if (firmaId > 0)
                    model.GuncelleyenId = _authenticationService.GetCurrentUserId;
                FirmaServiceExtensions.UpdateFirmaModel(firma, model);
                await _firmaRepository.UpdateFirmaAsync(firma);

                var result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    await _logService.SistemLogService
                        .SistemLogInformationAsync(
                            "Firma İşlemleri",
                            "Firma Ekle/Güncelle",
                            $"Firma {(firmaId > 0 ? "güncellendi" : "eklendi")}",
                            $"Etkilenen kayıt: {result}");
                    var updateFirma = await FirmaServiceExtensions.GetByFirmaIdAsync(_firmaRepository, _bitmapTools, firma.Id);
                    if (updateFirma.Success && updateFirma.Data != null)
                        model.Merge(updateFirma.Data);
                    return new SuccessApiDataResponse<int>(data: result, message: $"✅ Firma {(firmaId > 0 ? "güncellendi" : "eklendi")}");
                }
                return new ErrorApiDataResponse<int>(data: 0, message: "❌ Firma ekleme/güncelleme işlemi başarısız oldu!");
            }
            catch (Exception ex)
            {
                await LogException(nameof(UpdateFirmaAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"[HATA] ❌ Firma ekleme/güncelleme işlemi başarız oldu! => {ex.Message}");
            }
        }

        private async Task LogException(string methodName, Exception ex)
        { await _logService.SistemLogService.SistemLogExceptionAsync(nameof(FirmaKayitService), methodName, ex); }
    }
}
