using Microsoft.EntityFrameworkCore;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Data.DataContext;

namespace MuhasibPro.Business.Services.SistemServices.AppServices;

public class SistemDiagnosticsService : ISistemDiagnosticsService
{
    private readonly SistemDbContext _dbContext;

    public SistemDiagnosticsService(SistemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GetKullaniciCountAsync()
    {
        return await _dbContext.Kullanicilar.CountAsync();
    }

    public async Task<int> GetFirmaCountAsync()
    {
        return await _dbContext.Firmalar.CountAsync();
    }

    public async Task<bool> CanConnectAsync()
    {
        return await _dbContext.Database.CanConnectAsync();
    }

    public async Task<(int FirmaCount, int DonemCount, string IlkFirmaKisaUnvani)> GetFirmaMaliDonemStatsAsync()
    {
        var firmalar = await _dbContext.Firmalar.Include(f => f.MaliDonemler).ToListAsync();
        var donemler = await _dbContext.MaliDonemler.ToListAsync();
        return (firmalar.Count, donemler.Count, firmalar.FirstOrDefault()?.KisaUnvani ?? string.Empty);
    }

    public async Task<(int KfrCount, int PermCount, string Sample)> GetKullaniciFirmaRolStatsAsync()
    {
        var kfr = await _dbContext.KullaniciFirmaRoller.Include(x => x.Rol).Take(5).ToListAsync();
        var permCount = await _dbContext.RolPermissionlar.CountAsync();
        var sample = kfr.Any() ? $"K={kfr.First().KullaniciId} F={kfr.First().FirmaId} R={kfr.First().RolId}" : string.Empty;
        return (kfr.Count, permCount, sample);
    }
}
