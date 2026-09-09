using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.SistemServices.Authentication
{
    public class LisansDurumDto
    {
        public LisansTuru Tur { get; set; }
        public bool GecerliMi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public int? KalanGun { get; set; }
        public string Aciklama { get; set; } = string.Empty;
    }

    /// <summary>Lisans yönetimi — tür LicenseSettings'ten, satırlar Lisanslar tablosundan.
    /// Deneme türü satır istemez; diğer türler tarih-geçerli satır ister.</summary>
    public interface ILisansService
    {
        Task<ApiDataResponse<LisansDurumDto>> GetLisansDurumuAsync();
        Task<ApiDataResponse<List<Lisans>>> GetLisanslarAsync();
        Task<ApiDataResponse<int>> KaydetLisansAsync(LisansTuru tur, string lisansAnahtari, DateTime baslangic, DateTime bitis, string? aciklama = null);
    }
}
