using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Repository.SistemRepos;

public class KullaniciRolRepository : IKullaniciRolRepository
{
    private readonly SistemDbContext _context;
    public KullaniciRolRepository(SistemDbContext context) => _context = context;

    public async Task<IList<KullaniciRol>> GetAllAsync()
        => await _context.KullaniciRoller.OrderBy(x => x.RolTip).ToListAsync();

    public async Task<KullaniciRol?> GetByIdAsync(long id)
        => await _context.KullaniciRoller.FirstOrDefaultAsync(x => x.Id == id);
}
