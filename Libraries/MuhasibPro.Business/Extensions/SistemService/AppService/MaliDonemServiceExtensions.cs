using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Extensions.SistemService.AppService
{
    public static class MaliDonemServiceExtensions
    {
        public static void UpdateMaliDonemModel(MaliDonem target, MaliDonemModel source)
        {
            var maliDonem = ModelFactory.UpdateEntityFromModel(target, source,(
                entity,model) =>                
                {
                    entity.FirmaId = model.FirmaId;
                    entity.MaliYil = model.MaliYil;
                    entity.DatabaseName = model.DatabaseName;
                    entity.DatabaseType = model.DatabaseType;
                    if(model.TenantDetails != null)
                    {
                        entity.Durum = model.TenantDetails.Durum;
                        entity.ArsivlendiMi = model.TenantDetails.ArsivlendiMi;
                        entity.DosyaBoyutu = model.TenantDetails.DosyaBoyutu;
                        entity.SonYedekTarihi = model.TenantDetails.SonYedekTarihi;
                    }
                });
        }
        public static async Task<MaliDonemModel> CreateMaliDonemModelAsync(            
            MaliDonem source,
            bool boolIncludeAllFields,
            IBitmapToolsService bitmapTools)
        {
            if(source == null)
                throw new ArgumentNullException(nameof(source));
            try
            {
                var model = await ModelFactory.CreateModelFromEntityAsync<MaliDonemModel, MaliDonem>(
                    source,
                    boolIncludeAllFields,
                    async (model, entity, include) =>
                    {
                        model.FirmaId = entity.FirmaId;
                        model.MaliYil = entity.MaliYil;
                        model.DatabaseName = entity.DatabaseName;
                        model.DatabaseType = entity.DatabaseType;
                        model.TenantDetails = CreateTenantDetails(entity);
                        if(entity.Firma != null)
                        {
                            model.FirmaModel = await FirmaServiceExtensions.CreateFirmaModelAsync(
                                entity.Firma,
                                boolIncludeAllFields,
                                bitmapTools);
                        }
                    });
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("MaliDonemModel oluşturulurken bir hata oluştu.", ex);
            }
        }
        private static TenantDetailsModel CreateTenantDetails(MaliDonem source)
        {
            return new TenantDetailsModel
            {
                MaliDonemId = source.Id,
                FirmaId = source.FirmaId,
                FirmaKodu = source.Firma?.FirmaKodu,
                FirmaKisaUnvan = source.Firma?.KisaUnvani,
                MaliYil = source.MaliYil,
                DatabaseName = source.DatabaseName,
                Durum = source.Durum,
                ArsivlendiMi = source.ArsivlendiMi,
                DosyaBoyutu = source.DosyaBoyutu,
                SonYedekTarihi = source.SonYedekTarihi
            };
        }
        public static async Task<ApiDataResponse<MaliDonemModel>> GetByMaliDonemIdAsync(
            IMaliDonemRepository repository,            
            IBitmapToolsService bitmapTools,
            long malidonemId)
        {
            try
            {
                var item = await repository.GetByMaliDonemIdAsync(malidonemId);
                if(item == null)
                {
                    return new ErrorApiDataResponse<MaliDonemModel>(data: null, message: "⚠️ Mali Dönem bulunamadı");
                }
                var model = await CreateMaliDonemModelAsync(
                    item,
                    boolIncludeAllFields: true,
                    bitmapTools);
                return new SuccessApiDataResponse<MaliDonemModel>(data: model, message: "✅ İşlem başarılı");
            }
            catch (Exception ex)
            {
                return new ErrorApiDataResponse<MaliDonemModel>(data: null, message: $"[Hata]:{ex.Message}");
            }
        }
    }
}
