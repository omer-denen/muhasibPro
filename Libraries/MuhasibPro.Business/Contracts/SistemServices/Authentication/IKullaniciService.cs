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
        /// <summary>Firma bazlı kullanıcı listesi — her kullanıcının o firmadaki rolü (<c>RolId</c>) doldurulur.</summary>
        Task<ApiDataResponse<List<KullaniciModel>>> GetKullanicilarWithRolAsync(long firmaId);
        /// <summary>Rol kataloğu (rol seçici).</summary>
        Task<ApiDataResponse<List<KullaniciRolModel>>> GetRollerAsync();
        /// <summary>Yönetici kapılı yeni kullanıcı: kullanıcı adı benzersiz + parola hash + firma-başına rol (KFR).</summary>
        Task<ApiDataResponse<int>> CreateKullaniciAsync(KullaniciModel model, string sifre, long firmaId, long rolId);
        /// <summary>Yönetici kapılı firma-başına rol ataması (KFR upsert).</summary>
        Task<ApiDataResponse<int>> RolAtaAsync(long kullaniciId, long firmaId, long rolId);
        Task<ApiDataResponse<int>> UpdateKullaniciAsync(KullaniciModel model);
        Task<ApiDataResponse<int>> SetAktifAsync(long id, bool aktif);
        Task<ApiDataResponse<int>> SifreBelirleAsync(long id, string yeniSifre);
        Task<ApiDataResponse<int>> DeleteKullaniciAsync(long id);
    }
}
