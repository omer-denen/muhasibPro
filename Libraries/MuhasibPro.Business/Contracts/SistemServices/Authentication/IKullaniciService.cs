using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.SistemServices.Authentication
{
    /// <summary>Kullanıcı yönetimi — repo'yu sarar (kural: aynı kural iki yerde uygulanmaz).
    /// Kayıt (Register) AuthenticationService'tedir; burada profil/şifre/durum/silme yönetilir.</summary>
    public interface IKullaniciService
    {
        Task<ApiDataResponse<KullaniciModel>> GetKullaniciAsync(long id);
        Task<ApiDataResponse<KullaniciModel>> GetByUsernameAsync(string username);
        Task<ApiDataResponse<List<KullaniciModel>>> GetKullanicilarAsync();
        Task<ApiDataResponse<int>> UpdateKullaniciAsync(KullaniciModel model);
        Task<ApiDataResponse<int>> SetAktifAsync(long id, bool aktif);
        Task<ApiDataResponse<int>> SifreBelirleAsync(long id, string yeniSifre);
        Task<ApiDataResponse<int>> DeleteKullaniciAsync(long id);
    }
}
