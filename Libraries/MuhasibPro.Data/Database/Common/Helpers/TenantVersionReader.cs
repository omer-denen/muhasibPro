using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.DataContext;

namespace MuhasibPro.Data.Database.Common.Helpers
{
    /// <summary>Taşınmış-veri taraması — Sistem satırları + tenant damgaları, best-effort.</summary>
    public class TenantVersionReader : ITenantVersionReader
    {
        private readonly SistemDbContext _sistemDb;
        private readonly IAppDbContextFactory _dbFactory;
        private readonly IApplicationPaths _appPaths;

        public TenantVersionReader(
            SistemDbContext sistemDb,
            IAppDbContextFactory dbFactory,
            IApplicationPaths appPaths)
        {
            _sistemDb = sistemDb;
            _dbFactory = dbFactory;
            _appPaths = appPaths;
        }

        public async Task<IReadOnlyList<TenantMismatchInfo>> ScanMismatchesAsync(string currentKurulumId, string currentMachineId)
        {
            var result = new List<TenantMismatchInfo>();
            try
            {
                var dbNames = await _sistemDb.MaliDonemler
                    .AsNoTracking()
                    .Select(md => md.DatabaseName)
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var dbName in dbNames)
                {
                    if (string.IsNullOrWhiteSpace(dbName) || !_appPaths.TenantDatabaseFileExists(dbName))
                        continue;

                    try
                    {
                        using var ctx = _dbFactory.CreateDbContext(dbName);
                        var ver = await ctx.TenantDatabaseVersiyonlar
                            .AsNoTracking()
                            .FirstOrDefaultAsync(v => v.DatabaseName == dbName)
                            .ConfigureAwait(false);
                        if (ver == null)
                            continue;

                        bool kurulumFarkli = !string.IsNullOrWhiteSpace(ver.KurulumId)
                            && !string.Equals(ver.KurulumId, currentKurulumId, StringComparison.OrdinalIgnoreCase);
                        bool makineFarkli = !string.IsNullOrWhiteSpace(ver.MakineId)
                            && !string.Equals(ver.MakineId, currentMachineId, StringComparison.OrdinalIgnoreCase);

                        if (kurulumFarkli || makineFarkli)
                        {
                            result.Add(new TenantMismatchInfo
                            {
                                DatabaseName = dbName,
                                VersiyonKurulumId = ver.KurulumId,
                                VersiyonMakineId = ver.MakineId,
                                KurulumFarkli = kurulumFarkli,
                                MakineFarkli = makineFarkli
                            });
                        }
                    }
                    catch { /* dosya başına best-effort */ }
                }
            }
            catch { /* best-effort */ }

            return result;
        }
    }
}
