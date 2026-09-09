using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Repository.SistemRepos;

public class RolPermissionRepository : IRolPermissionRepository
{
    private readonly SistemDbContext _context;
    public RolPermissionRepository(SistemDbContext context) => _context = context;

    public async Task AddAsync(RolPermission entity) => await _context.RolPermissionlar.AddAsync(entity);
    public async Task AddRangeAsync(IEnumerable<RolPermission> entities) => await _context.RolPermissionlar.AddRangeAsync(entities);
    public Task DeleteAsync(RolPermission entity) { _context.RolPermissionlar.Remove(entity); return Task.CompletedTask; }
    public async Task<IList<RolPermission>> GetByRolIdAsync(long rolId) => await _context.RolPermissionlar.Where(x => x.RolId == rolId).ToListAsync();
    public async Task<IList<RolPermission>> GetAllAsync() => await _context.RolPermissionlar.ToListAsync();
}
