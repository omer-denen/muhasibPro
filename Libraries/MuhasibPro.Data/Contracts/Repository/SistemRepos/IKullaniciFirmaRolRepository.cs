using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Contracts.Repository.SistemRepos;

public interface IKullaniciFirmaRolRepository
{
    Task AddAsync(KullaniciFirmaRol entity);
    Task DeleteAsync(KullaniciFirmaRol entity);
    Task<KullaniciFirmaRol?> FindAsync(long kullaniciId, long firmaId);
    Task<IList<KullaniciFirmaRol>> GetByKullaniciIdAsync(long kullaniciId);
    Task<IList<KullaniciFirmaRol>> GetByFirmaIdAsync(long firmaId);
    Task<IList<KullaniciFirmaRol>> GetAllAsync();
}
