using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Contracts.SistemServices.AppServices;

public class ModuleInfoDto
{
    public ModuleType ModuleType { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string IconKey { get; set; } = string.Empty;
}

public interface IModuleLicenseService
{
    Task<bool> IsModuleActiveAsync(long firmaId, ModuleType moduleType);
    Task<ApiDataResponse<List<ModuleInfoDto>>> GetFirmaModulesAsync(long firmaId);
    Task<ApiDataResponse<bool>> SetModuleStatusAsync(long firmaId, ModuleType moduleType, bool isActive);
    Task<ApiDataResponse<bool>> UpdateActiveModulesAsync(long firmaId, IEnumerable<ModuleType> activeModules);
}
