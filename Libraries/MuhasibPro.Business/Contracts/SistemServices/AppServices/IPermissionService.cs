using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Permission permission);
    Task<bool> HasPermissionAsync(long kullaniciId, long firmaId, Permission permission);

    /// <summary>Firma seçimi gerektirmeyen, kullanıcı düzeyinde izin kontrolü (Faz 6.85 K4).
    /// Kullanıcının herhangi bir firmadaki rolü izni içeriyorsa (veya Yönetici ise) true.
    /// Seçim-öncesi yüzeyler (Denetim Masası, FirmaShell butonları) için.</summary>
    Task<bool> KullaniciYetkisiVarMiAsync(Permission permission);

    void ClearCache();
}
