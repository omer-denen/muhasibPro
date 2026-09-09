using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Data.Repository.Common.BaseRepo;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Repository.SistemRepos;

public class OturumKaydiRepository : BaseRepository<SistemDbContext, OturumKaydi>, IOturumKaydiRepository
{
    public OturumKaydiRepository(SistemDbContext context) : base(context) { }

    public async Task<IList<OturumKaydi>> GetAktifOturumlarAsync() => await DbSet.Where(x => x.AktifMi).ToListAsync();
}
