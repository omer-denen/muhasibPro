using Microsoft.EntityFrameworkCore;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.SistemServices.Authentication
{
    public class LisansService : ILisansService
    {
        private readonly SistemDbContext _sistemDbContext;
        private readonly ILicenseSettingsProvider _licenseSettings;
        private readonly IAuthenticationService _authenticationService;

        public LisansService(
            SistemDbContext sistemDbContext,
            ILicenseSettingsProvider licenseSettings,
            IAuthenticationService authenticationService = null!)
        {
            _sistemDbContext = sistemDbContext;
            _licenseSettings = licenseSettings;
            _authenticationService = authenticationService;
        }

        public async Task<ApiDataResponse<LisansDurumDto>> GetLisansDurumuAsync()
        {
            var ayar = await _licenseSettings.GetAsync();
            if (ayar.Tur == LisansTuru.Deneme)
            {
                return new SuccessApiDataResponse<LisansDurumDto>(new LisansDurumDto
                {
                    Tur = LisansTuru.Deneme,
                    GecerliMi = true,
                    Aciklama = "Deneme türü lisans satırı istemez."
                }, "Lisans durumu getirildi.");
            }

            var bugun = DateTime.Today;
            var satir = await _sistemDbContext.Lisanslar
                .Where(l => l.Tur == ayar.Tur && l.BaslangicTarihi.Date <= bugun && l.BitisTarihi.Date >= bugun)
                .OrderByDescending(l => l.BitisTarihi)
                .FirstOrDefaultAsync();

            if (satir == null)
            {
                return new SuccessApiDataResponse<LisansDurumDto>(new LisansDurumDto
                {
                    Tur = ayar.Tur,
                    GecerliMi = false,
                    Aciklama = "Bu türde tarih-geçerli lisans satırı yok."
                }, "Lisans durumu getirildi.");
            }

            return new SuccessApiDataResponse<LisansDurumDto>(new LisansDurumDto
            {
                Tur = ayar.Tur,
                GecerliMi = true,
                BitisTarihi = satir.BitisTarihi,
                KalanGun = (satir.BitisTarihi.Date - bugun).Days,
                Aciklama = satir.Aciklama ?? string.Empty
            }, "Lisans durumu getirildi.");
        }

        public async Task<ApiDataResponse<List<Lisans>>> GetLisanslarAsync()
        {
            var liste = await _sistemDbContext.Lisanslar
                .OrderByDescending(l => l.BitisTarihi)
                .ToListAsync();
            return new SuccessApiDataResponse<List<Lisans>>(liste, $"{liste.Count} lisans listelendi.");
        }

        public async Task<ApiDataResponse<int>> KaydetLisansAsync(
            LisansTuru tur, string lisansAnahtari, DateTime baslangic, DateTime bitis, string? aciklama = null)
        {
            if ((_authenticationService?.GetCurrentUserId ?? 0) <= 0)
                return new ErrorApiDataResponse<int>(0, "İşlem yapan kullanıcı bilgisi alınamadı!");
            if (!AyarYetkiDenetimi.KullaniciYoneticiMi(_authenticationService))
                return new ErrorApiDataResponse<int>(0, "Lisans kaydını yalnızca yönetici yapabilir.");
            if (string.IsNullOrWhiteSpace(lisansAnahtari))
                return new ErrorApiDataResponse<int>(0, "Lisans anahtarı boş olamaz.");
            if (bitis.Date < baslangic.Date)
                return new ErrorApiDataResponse<int>(0, "Bitiş tarihi başlangıçtan önce olamaz.");

            await _sistemDbContext.Lisanslar.AddAsync(new Lisans
            {
                Tur = tur,
                LisansAnahtari = lisansAnahtari.Trim(),
                BaslangicTarihi = baslangic.Date,
                BitisTarihi = bitis.Date,
                Aciklama = aciklama?.Trim(),
                KayitTarihi = DateTime.Now,
                KaydedenId = _authenticationService.GetCurrentUserId,
                AktifMi = true
            });
            var sonuc = await _sistemDbContext.SaveChangesAsync();
            return new SuccessApiDataResponse<int>(sonuc, "Lisans kaydedildi.");
        }
    }
}
