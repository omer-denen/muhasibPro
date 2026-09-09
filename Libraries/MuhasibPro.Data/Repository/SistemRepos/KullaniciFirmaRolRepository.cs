using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Repository.SistemRepos;

public class KullaniciFirmaRolRepository : IKullaniciFirmaRolRepository
{
    private readonly SistemDbContext _context;
    public KullaniciFirmaRolRepository(SistemDbContext context) => _context = context;

    public async Task AddAsync(KullaniciFirmaRol entity) => await _context.KullaniciFirmaRoller.AddAsync(entity);
    public Task DeleteAsync(KullaniciFirmaRol entity) { _context.KullaniciFirmaRoller.Remove(entity); return Task.CompletedTask; }
    public async Task<KullaniciFirmaRol?> FindAsync(long kullaniciId, long firmaId) => await _context.KullaniciFirmaRoller.FirstOrDefaultAsync(x => x.KullaniciId == kullaniciId && x.FirmaId == firmaId);
    public async Task<IList<KullaniciFirmaRol>> GetByKullaniciIdAsync(long kullaniciId) => await _context.KullaniciFirmaRoller.Where(x => x.KullaniciId == kullaniciId).Include(x => x.Rol).Include(x => x.Firma).ToListAsync();
    public async Task<IList<KullaniciFirmaRol>> GetByFirmaIdAsync(long firmaId) => await _context.KullaniciFirmaRoller.Where(x => x.FirmaId == firmaId).Include(x => x.Kullanici).Include(x => x.Rol).ToListAsync();
    public async Task<IList<KullaniciFirmaRol>> GetAllAsync() => await _context.KullaniciFirmaRoller.Include(x => x.Kullanici).Include(x => x.Firma).Include(x => x.Rol).ToListAsync();
}
