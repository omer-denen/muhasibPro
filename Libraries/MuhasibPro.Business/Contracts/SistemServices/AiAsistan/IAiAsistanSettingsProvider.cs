using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Contracts.SistemServices.AiAsistan
{
    public interface IAiAsistanSettingsProvider
    {
        Task<AiAsistanSettings> GetAsync();
        Task SaveAsync(AiAsistanSettings settings);
    }
}
