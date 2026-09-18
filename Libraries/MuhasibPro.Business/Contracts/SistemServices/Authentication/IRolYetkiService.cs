using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.SistemServices.Authentication;

/// <summary>Faz 6.85 K3: rol → izin matrisi (izin matrisi UI) — yalnız `Kullanıcı` rolü düzenlenebilir.</summary>
public interface IRolYetkiService
{
    /// <summary>Tüm izinler + verilen rolün seçim durumu (kategori/okunabilir ad ile).</summary>
    Task<ApiDataResponse<List<RolIzinModel>>> GetMatrisAsync(long rolId);

    /// <summary>Tek izni role ekler/çıkarır (yönetici kapılı) ve yetki cache'ini temizler.</summary>
    Task<ApiDataResponse<int>> SetIzinAsync(long rolId, Permission izin, bool aktif);
}
