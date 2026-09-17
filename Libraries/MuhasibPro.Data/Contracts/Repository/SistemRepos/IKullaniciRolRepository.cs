using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Contracts.Repository.SistemRepos;

/// <summary>Faz 6.85 K2: roller (<c>KullaniciRoller</c>) salt-okuma deposu — rol seçici + listeleme.</summary>
public interface IKullaniciRolRepository
{
    Task<IList<KullaniciRol>> GetAllAsync();
    Task<KullaniciRol?> GetByIdAsync(long id);
}
