using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Domain.Enum;
using System.Collections.Concurrent;

namespace MuhasibPro.Business.Services.SistemServices.AppServices;

/// <summary>Kullanıcı+firma rollerinden aksiyon yetkisi çözer.
/// Ölü ITenantContext bağı kaldırıldı (Oturum 128): seçim aynası
/// (IFirmaWithMaliDonemSelectedService) + giriş (IAuthenticationService) canlı kaynaklardır.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IKullaniciFirmaRolRepository _kfrRepo;
    private readonly IRolPermissionRepository _rolPermRepo;
    private readonly IFirmaWithMaliDonemSelectedService _selection;
    private readonly IAuthenticationService _auth;
    private readonly ConcurrentDictionary<(long kullaniciId, long firmaId), HashSet<Permission>> _cache = new();

    public PermissionService(
        IKullaniciFirmaRolRepository kfrRepo,
        IRolPermissionRepository rolPermRepo,
        IFirmaWithMaliDonemSelectedService selection,
        IAuthenticationService auth)
    {
        _kfrRepo = kfrRepo;
        _rolPermRepo = rolPermRepo;
        _selection = selection;
        _auth = auth;
    }

    public async Task<bool> HasPermissionAsync(Permission permission)
    {
        if (!_auth.IsAuthenticated) return false;
        var firmaId = _selection.SelectedFirma?.Id;
        if (firmaId == null || firmaId <= 0) return false;
        return await HasPermissionAsync(_auth.GetCurrentUserId, firmaId.Value, permission);
    }

    public async Task<bool> HasPermissionAsync(long kullaniciId, long firmaId, Permission permission)
    {
        var key = (kullaniciId, firmaId);
        if (_cache.TryGetValue(key, out var perms))
            return perms.Contains(permission);

        var kfrList = await _kfrRepo.GetByKullaniciIdAsync(kullaniciId);
        var firmaKfr = kfrList.FirstOrDefault(x => x.FirmaId == firmaId);
        if (firmaKfr == null) return false;

        var rolPerms = await _rolPermRepo.GetByRolIdAsync(firmaKfr.RolId);
        var permSet = new HashSet<Permission>(rolPerms.Select(x => x.PermissionId));
        _cache[key] = permSet;
        return permSet.Contains(permission);
    }

    public void ClearCache() => _cache.Clear();
}
