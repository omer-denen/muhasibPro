using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Permission permission);
    Task<bool> HasPermissionAsync(long kullaniciId, long firmaId, Permission permission);
    void ClearCache();
}
