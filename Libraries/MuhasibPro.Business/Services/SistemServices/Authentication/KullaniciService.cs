using Microsoft.AspNetCore.Identity;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.Contracts.Repository.SistemRepos.Authentication;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.SistemServices.Authentication
{
    public class KullaniciService : IKullaniciService
    {
        private readonly IUserRepository _kullaniciRepository;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPasswordHasher<Kullanici> _passwordHasher;
        private readonly IIdentitySettingsProvider _identitySettings;

        public KullaniciService(
            IUserRepository kullaniciRepository,
            IUnitOfWork<SistemDbContext> unitOfWork,
            IAuthenticationService authenticationService,
            IPasswordHasher<Kullanici> passwordHasher,
            IIdentitySettingsProvider identitySettings = null!)
        {
            _kullaniciRepository = kullaniciRepository;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _passwordHasher = passwordHasher;
            _identitySettings = identitySettings;
        }

        public async Task<ApiDataResponse<KullaniciModel>> GetKullaniciAsync(long id)
        {
            var entity = await _kullaniciRepository.GetByIdAsync(id);
            if (entity == null)
                return new ErrorApiDataResponse<KullaniciModel>(null, "Kullanıcı bulunamadı.");
            return new SuccessApiDataResponse<KullaniciModel>(ToModel(entity), "Kullanıcı getirildi.");
        }

        public async Task<ApiDataResponse<KullaniciModel>> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return new ErrorApiDataResponse<KullaniciModel>(null, "Kullanıcı adı boş olamaz.");
            var entity = await _kullaniciRepository.GetByUsernameAsync(username.Trim());
            if (entity == null)
                return new ErrorApiDataResponse<KullaniciModel>(null, "Kullanıcı bulunamadı.");
            return new SuccessApiDataResponse<KullaniciModel>(ToModel(entity), "Kullanıcı getirildi.");
        }

        public async Task<ApiDataResponse<List<KullaniciModel>>> GetKullanicilarAsync()
        {
            var entities = await _kullaniciRepository.GetAllAsync();
            var liste = entities.Select(ToModel).ToList();
            return new SuccessApiDataResponse<List<KullaniciModel>>(liste, $"{liste.Count} kullanıcı listelendi.");
        }

        public async Task<ApiDataResponse<int>> UpdateKullaniciAsync(KullaniciModel model)
        {
            if (_authenticationService.GetCurrentUserId <= 0)
                return new ErrorApiDataResponse<int>(0, "İşlem yapan kullanıcı bilgisi alınamadı!");
            if (model == null)
                return new ErrorApiDataResponse<int>(0, "Güncellenecek kullanıcı bilgisi boş olamaz!");

            var entity = await _kullaniciRepository.GetByIdAsync(model.Id);
            if (entity == null)
                return new ErrorApiDataResponse<int>(0, "Kullanıcı bulunamadı.");

            entity.Adi = model.Adi?.Trim() ?? entity.Adi;
            entity.Soyadi = model.Soyadi?.Trim() ?? entity.Soyadi;
            entity.Eposta = model.Eposta?.Trim() ?? entity.Eposta;
            entity.Telefon = model.Telefon?.Trim() ?? entity.Telefon;
            entity.AktifMi = model.AktifMi;
            entity.GuncelleyenId = _authenticationService.GetCurrentUserId;
            entity.GuncellemeTarihi = DateTime.Now;

            await _kullaniciRepository.UpdateAsync(entity);
            var sonuc = await _unitOfWork.SaveChangesAsync();
            return new SuccessApiDataResponse<int>(sonuc, "Kullanıcı güncellendi.");
        }

        public async Task<ApiDataResponse<int>> SetAktifAsync(long id, bool aktif)
        {
            if (_authenticationService.GetCurrentUserId <= 0)
                return new ErrorApiDataResponse<int>(0, "İşlem yapan kullanıcı bilgisi alınamadı!");

            var entity = await _kullaniciRepository.GetByIdAsync(id);
            if (entity == null)
                return new ErrorApiDataResponse<int>(0, "Kullanıcı bulunamadı.");
            if (!aktif && entity.Id == KullaniciSabitleri.SeedYoneticiId)
                return new ErrorApiDataResponse<int>(0, "Seed yöneticisi pasife alınamaz.");

            entity.AktifMi = aktif;
            entity.GuncelleyenId = _authenticationService.GetCurrentUserId;
            entity.GuncellemeTarihi = DateTime.Now;

            await _kullaniciRepository.UpdateAsync(entity);
            var sonuc = await _unitOfWork.SaveChangesAsync();
            return new SuccessApiDataResponse<int>(sonuc, aktif ? "Kullanıcı aktifleştirildi." : "Kullanıcı pasife alındı.");
        }

        public async Task<ApiDataResponse<int>> SifreBelirleAsync(long id, string yeniSifre)
        {
            if (_authenticationService.GetCurrentUserId <= 0)
                return new ErrorApiDataResponse<int>(0, "İşlem yapan kullanıcı bilgisi alınamadı!");
            if (id != _authenticationService.GetCurrentUserId
                && !AyarYetkiDenetimi.KullaniciYoneticiMi(_authenticationService))
                return new ErrorApiDataResponse<int>(0, "Başkasının şifresini yalnızca yönetici belirleyebilir.");

            int minUzunluk = 6;
            try { minUzunluk = (await _identitySettings?.GetAsync())?.MinPasswordLength ?? 6; } catch { }
            if (string.IsNullOrEmpty(yeniSifre) || yeniSifre.Length < minUzunluk)
                return new ErrorApiDataResponse<int>(0, $"Şifre en az {minUzunluk} karakter olmalı.");

            var entity = await _kullaniciRepository.GetByIdAsync(id);
            if (entity == null)
                return new ErrorApiDataResponse<int>(0, "Kullanıcı bulunamadı.");

            entity.ParolaHash = _passwordHasher.HashPassword(entity, yeniSifre);
            entity.GuncelleyenId = _authenticationService.GetCurrentUserId;
            entity.GuncellemeTarihi = DateTime.Now;

            await _kullaniciRepository.UpdateAsync(entity);
            var sonuc = await _unitOfWork.SaveChangesAsync();
            return new SuccessApiDataResponse<int>(sonuc, "Şifre güncellendi.");
        }

        public async Task<ApiDataResponse<int>> DeleteKullaniciAsync(long id)
        {
            if (_authenticationService.GetCurrentUserId <= 0)
                return new ErrorApiDataResponse<int>(0, "İşlem yapan kullanıcı bilgisi alınamadı!");
            if (id == _authenticationService.GetCurrentUserId)
                return new ErrorApiDataResponse<int>(0, "Kendi hesabınız silinemez.");
            if (id == KullaniciSabitleri.SeedYoneticiId)
                return new ErrorApiDataResponse<int>(0, "Seed yöneticisi silinemez.");

            var entity = await _kullaniciRepository.GetByIdAsync(id);
            if (entity == null)
                return new ErrorApiDataResponse<int>(0, "Kullanıcı bulunamadı.");

            await _kullaniciRepository.DeleteAsync(entity);
            var sonuc = await _unitOfWork.SaveChangesAsync();
            return new SuccessApiDataResponse<int>(sonuc, "Kullanıcı silindi.");
        }

        private static KullaniciModel ToModel(Kullanici entity) => new()
        {
            Id = entity.Id,
            KullaniciAdi = entity.KullaniciAdi,
            Adi = entity.Adi,
            Soyadi = entity.Soyadi,
            Eposta = entity.Eposta,
            Telefon = entity.Telefon,
            Resim = entity.Resim,
            ResimOnizleme = entity.ResimOnizleme,
            AktifMi = entity.AktifMi,
            KayitTarihi = entity.KayitTarihi,
            GuncellemeTarihi = entity.GuncellemeTarihi,
            KaydedenId = entity.KaydedenId,
            GuncelleyenId = entity.GuncelleyenId
        };
    }
}
