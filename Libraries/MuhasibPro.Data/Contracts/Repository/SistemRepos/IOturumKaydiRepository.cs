using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Contracts.Repository.SistemRepos;

public interface IOturumKaydiRepository : IRepository<OturumKaydi>
{
    Task<IList<OturumKaydi>> GetAktifOturumlarAsync();
}
