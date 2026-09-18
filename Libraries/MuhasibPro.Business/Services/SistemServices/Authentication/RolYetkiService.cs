using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.SistemServices.Authentication;

/// <summary>Faz 6.85 K3: izin matrisi servisi — `RolPermission` satırlarını okur/yazar.
/// Yönetici rolü sabittir (bypass); yalnız `Kullanıcı` rolü düzenlenir. Değişimde yetki cache'i temizlenir.</summary>
public class RolYetkiService : IRolYetkiService
{
    private readonly IRolPermissionRepository _rolPermRepo;
    private readonly IPermissionService _permissionService;
    private readonly IAuthenticationService _auth;
    private readonly IUnitOfWork<SistemDbContext> _unitOfWork;

    public RolYetkiService(
        IRolPermissionRepository rolPermRepo,
        IPermissionService permissionService,
        IAuthenticationService auth,
        IUnitOfWork<SistemDbContext> unitOfWork)
    {
        _rolPermRepo = rolPermRepo;
        _permissionService = permissionService;
        _auth = auth;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiDataResponse<List<RolIzinModel>>> GetMatrisAsync(long rolId)
    {
        var mevcut = await _rolPermRepo.GetByRolIdAsync(rolId);
        var seciliSet = mevcut.Select(x => x.PermissionId).ToHashSet();

        var liste = PermissionVarsayilanlari.TumIzinler
            .Select(izin => new RolIzinModel
            {
                Izin = izin,
                Kategori = KategoriAdi(izin),
                Ad = AksiyonAdi(izin),
                Secili = seciliSet.Contains(izin)
            })
            .ToList();

        return new SuccessApiDataResponse<List<RolIzinModel>>(liste, $"{liste.Count} izin bulundu.");
    }

    public async Task<ApiDataResponse<int>> SetIzinAsync(long rolId, Permission izin, bool aktif)
    {
        if (!AyarYetkiDenetimi.KullaniciYoneticiMi(_auth))
            return new ErrorApiDataResponse<int>(0, "İzin değiştirmek için yönetici olmalısınız.");
        if (rolId == KullaniciRolSabitleri.YoneticiRolId)
            return new ErrorApiDataResponse<int>(0, "Yönetici rolü tüm izinlere sabittir; düzenlenemez.");

        var mevcut = await _rolPermRepo.GetByRolIdAsync(rolId);
        var varOlan = mevcut.FirstOrDefault(x => x.PermissionId == izin);

        if (aktif && varOlan == null)
            await _rolPermRepo.AddAsync(new RolPermission { RolId = rolId, PermissionId = izin });
        else if (!aktif && varOlan != null)
            await _rolPermRepo.DeleteAsync(varOlan);
        else
            return new SuccessApiDataResponse<int>(0, "Değişiklik yok.");

        var sonuc = await _unitOfWork.SaveChangesAsync();
        _permissionService.ClearCache();
        return new SuccessApiDataResponse<int>(sonuc, aktif ? "İzin verildi." : "İzin kaldırıldı.");
    }

    private static string KategoriAdi(Permission izin)
    {
        var onEk = izin.ToString().Split('_')[0];
        return onEk switch
        {
            "Fis" => "Fiş",
            "Irsaliye" => "İrsaliye",
            "Cek" => "Çek",
            "Siparis" => "Sipariş",
            "HesapPlani" => "Hesap Planı",
            "DonemSonu" => "Dönem Sonu",
            "Rapor" => "Raporlama",
            "Kullanici" => "Kullanıcı",
            "MaliDonem" => "Mali Dönem",
            "Veritabani" => "Veritabanı",
            "AiAsistan" => "AI Yardım Asistanı",
            _ => onEk
        };
    }

    private static string AksiyonAdi(Permission izin)
    {
        var parcalar = izin.ToString().Split('_');
        var eylem = parcalar.Length > 1 ? parcalar[^1] : izin.ToString();
        return eylem switch
        {
            "Goruntule" => "Görüntüle",
            "Duzenle" => "Düzenle",
            "Onayla" => "Onayla",
            "Iptal" => "İptal",
            "DisaAktar" => "Dışa Aktar",
            "Baslat" => "Başlat",
            "Yonet" => "Yönet",
            "YedekAl" => "Yedek Al",
            "GeriYukle" => "Geri Yükle",
            "Arsivle" => "Arşivle",
            "KaliciSil" => "Kalıcı Sil",
            "Kullan" => "Kullan",
            _ => eylem
        };
    }
}
