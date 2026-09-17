using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Database.SistemDatabase;

/// <summary>
/// Faz 6.85 K1 — RBAC açılış backfill'i: KFR kaydı olmayan firmalara seed yöneticiyi
/// <see cref="KullaniciRolSabitleri.YoneticiRolId"/> rolüyle atar. Idempotenttir; her açılışta
/// güvenle çalışır (eklenecek firma yoksa 0 döner).
/// </summary>
public static class SistemRbacBackfill
{
    public static async Task<int> EnsureAsync(SistemDbContext context, CancellationToken cancellationToken = default)
    {
        var kfrliFirmaIdleri = await context.KullaniciFirmaRoller
            .Select(x => x.FirmaId)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var kfrsizFirmaIdleri = await context.Firmalar
            .Where(f => !kfrliFirmaIdleri.Contains(f.Id))
            .Select(f => f.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (kfrsizFirmaIdleri.Count == 0)
            return 0;

        foreach (var firmaId in kfrsizFirmaIdleri)
        {
            await context.KullaniciFirmaRoller.AddAsync(new KullaniciFirmaRol
            {
                KullaniciId = KullaniciSabitleri.SeedYoneticiId,
                FirmaId = firmaId,
                RolId = KullaniciRolSabitleri.YoneticiRolId
            }, cancellationToken).ConfigureAwait(false);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return kfrsizFirmaIdleri.Count;
    }
}
