using Microsoft.AspNetCore.Identity;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.Contracts.Repository.SistemRepos.Authentication;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.Domain.Utilities.UIDGenerator;

namespace MuhasibPro.Business.Services.SistemServices.Authentication
{
    public class KullaniciService : IKullaniciService
    {
        private readonly IUserRepository _kullaniciRepository;
        private readonly IKullaniciFirmaRolRepository _kfrRepository;
        private readonly IKullaniciRolRepository _rolRepository;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPasswordHasher<Kullanici> _passwordHasher;
        private readonly IIdentitySettingsProvider _identitySettings;
        private readonly IPermissionService _permissionService;

        public KullaniciService(
            IUserRepository kullaniciRepository,
            IKullaniciFirmaRolRepository kfrRepository,
            IKullaniciRolRepository rolRepository,
            IUnitOfWork<SistemDbContext> unitOfWork,
            IAuthenticationService authenticationService,
            IPasswordHasher<Kullanici> passwordHasher,
            IIdentitySettingsProvider identitySettings = null!,
            IPermissionService permissionService = null)
        {
            _kullaniciRepository = kullaniciRepository;
            _kfrRepository = kfrRepository;
            _rolRepository = rolRepository;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _passwordHasher = passwordHasher;
            _identitySettings = identitySettings;
            _permissionService = permissionService;
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

        public async Task<ApiDataResponse<List<KullaniciModel>>> GetKullanicilarWithRolAsync(long firmaId)
        {
            var entities = await _kullaniciRepository.GetAllAsync();
            var kfrler = firmaId > 0 ? await _kfrRepository.GetByFirmaIdAsync(firmaId) : new List<KullaniciFirmaRol>();

            var liste = new List<KullaniciModel>(entities.Count);
            foreach (var entity in entities)
            {
                var model = ToModel(entity);
                var kfr = kfrler.FirstOrDefault(x => x.KullaniciId == entity.Id);
                if (kfr != null)
                {
                    model.RolId = kfr.RolId;
                    if (kfr.Rol != null)
                        model.Rol = ToRolModel(kfr.Rol);
                }
                liste.Add(model);
            }
            return new SuccessApiDataResponse<List<KullaniciModel>>(liste, $"{liste.Count} kullanıcı listelendi.");
        }

        public async Task<ApiDataResponse<List<KullaniciRolModel>>> GetRollerAsync()
        {
            var roller = await _rolRepository.GetAllAsync();
            var liste = roller.Select(ToRolModel).ToList();
            return new SuccessApiDataResponse<List<KullaniciRolModel>>(liste, $"{liste.Count} rol bulundu.");
        }

        public async Task<ApiDataResponse<int>> CreateKullaniciAsync(KullaniciModel model, string sifre, long firmaId, long rolId)
        {
            if (!AyarYetkiDenetimi.KullaniciYoneticiMi(_authenticationService))
                return new ErrorApiDataResponse<int>(0, "Kullanıcı oluşturmak için yönetici olmalısınız.");
            if (model == null)
                return new ErrorApiDataResponse<int>(0, "Kullanıcı bilgisi boş olamaz!");
            if (string.IsNullOrWhiteSpace(model.KullaniciAdi))
                return new ErrorApiDataResponse<int>(0, "Kullanıcı adı boş olamaz.");
            if (string.IsNullOrWhiteSpace(model.Adi))
                return new ErrorApiDataResponse<int>(0, "Ad boş olamaz.");
            if (await _kullaniciRepository.GetByUsernameAsync(model.KullaniciAdi.Trim()) != null)
                return new ErrorApiDataResponse<int>(0, "Bu kullanıcı adı zaten kullanılıyor.");

            int minUzunluk = 6;
            try { minUzunluk = (await _identitySettings?.GetAsync())?.MinPasswordLength ?? 6; } catch { }
            if (string.IsNullOrEmpty(sifre) || sifre.Length < minUzunluk)
                return new ErrorApiDataResponse<int>(0, $"Şifre en az {minUzunluk} karakter olmalı.");

            var kullanici = new Kullanici
            {
                Id = UIDGenerator.GenerateModuleId(UIDModuleType.Sistem),
                KullaniciAdi = model.KullaniciAdi.Trim(),
                Adi = model.Adi.Trim(),
                Soyadi = model.Soyadi?.Trim() ?? string.Empty,
                Eposta = model.Eposta?.Trim() ?? string.Empty,
                Telefon = model.Telefon?.Trim() ?? string.Empty,
                Resim = model.Resim,
                ResimOnizleme = model.ResimOnizleme,
                AktifMi = true,
                ParolaHash = string.Empty,
                KayitTarihi = DateTime.UtcNow,
                KaydedenId = _authenticationService.GetCurrentUserId
            };
            kullanici.ParolaHash = _passwordHasher.HashPassword(kullanici, sifre);
            await _kullaniciRepository.AddAsync(kullanici);

            if (firmaId > 0)
                await KfrYazAsync(kullanici.Id, firmaId, rolId > 0 ? rolId : KullaniciRolSabitleri.KullaniciRolId);

            var sonuc = await _unitOfWork.SaveChangesAsync();
            return new SuccessApiDataResponse<int>(sonuc, "Kullanıcı oluşturuldu.");
        }

        public async Task<ApiDataResponse<int>> RolAtaAsync(long kullaniciId, long firmaId, long rolId)
        {
            if (!AyarYetkiDenetimi.KullaniciYoneticiMi(_authenticationService))
                return new ErrorApiDataResponse<int>(0, "Rol atamak için yönetici olmalısınız.");
            if (kullaniciId <= 0 || firmaId <= 0 || rolId <= 0)
                return new ErrorApiDataResponse<int>(0, "Kullanıcı, firma ve rol bilgisi zorunludur.");
            if (await _kullaniciRepository.GetByIdAsync(kullaniciId) == null)
                return new ErrorApiDataResponse<int>(0, "Kullanıcı bulunamadı.");
            if (await _rolRepository.GetByIdAsync(rolId) == null)
                return new ErrorApiDataResponse<int>(0, "Rol bulunamadı.");

            await KfrYazAsync(kullaniciId, firmaId, rolId);
            var sonuc = await _unitOfWork.SaveChangesAsync();
            return new SuccessApiDataResponse<int>(sonuc, "Rol atandı.");
        }

        /// <summary>KFR upsert (kaydetmez) — çağıran tek <c>SaveChangesAsync</c> ile yazar.</summary>
        private async Task KfrYazAsync(long kullaniciId, long firmaId, long rolId)
        {
            var mevcut = await _kfrRepository.FindAsync(kullaniciId, firmaId);
            if (mevcut == null)
                await _kfrRepository.AddAsync(new KullaniciFirmaRol
                {
                    KullaniciId = kullaniciId,
                    FirmaId = firmaId,
                    RolId = rolId
                });
            else
                mevcut.RolId = rolId;
        }

        private static KullaniciRolModel ToRolModel(KullaniciRol rol) => new()
        {
            Id = rol.Id,
            RolAdi = rol.RolAdi,
            Aciklama = rol.Aciklama,
            RolTip = rol.RolTip
        };

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
            entity.Resim = model.Resim;
            entity.ResimOnizleme = model.ResimOnizleme;
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

            // K4: yıkıcı işlem — `Kullanici_Yonet` izni gerekir (servis katmanı kapısı).
            if (_permissionService != null && !await _permissionService.KullaniciYetkisiVarMiAsync(Permission.Kullanici_Yonet))
                return new ErrorApiDataResponse<int>(0, "🔒 Kullanıcı silme yetkiniz yok (Kullanıcı Yönetimi izni gerekir).");

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
