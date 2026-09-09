using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Contracts.Repository.SistemRepos;

public interface IRolPermissionRepository
{
    Task AddAsync(RolPermission entity);
    Task AddRangeAsync(IEnumerable<RolPermission> entities);
    Task DeleteAsync(RolPermission entity);
    Task<IList<RolPermission>> GetByRolIdAsync(long rolId);
    Task<IList<RolPermission>> GetAllAsync();
}
