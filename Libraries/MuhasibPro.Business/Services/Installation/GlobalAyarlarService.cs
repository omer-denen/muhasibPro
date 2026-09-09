using Microsoft.EntityFrameworkCore;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Business.Services.Installation;

public class GlobalAyarlarService : IGlobalAyarlarService
{
    private readonly SistemDbContext _db;

    public GlobalAyarlarService(SistemDbContext db)
    {
        _db = db;
    }

    public async Task<string?> GetAsync(string anahtar)
    {
        var row = await _db.GlobalAyarlar.AsNoTracking().FirstOrDefaultAsync(x => x.Anahtar == anahtar);
        return row?.Deger;
    }

    public async Task SetAsync(string anahtar, string deger, string? aciklama = null)
    {
        var existing = await _db.GlobalAyarlar.FirstOrDefaultAsync(x => x.Anahtar == anahtar);
        if (existing != null)
        {
            existing.Deger = deger;
            existing.GuncellemeTarihi = DateTime.Now;
            if (aciklama != null) existing.Aciklama = aciklama;
        }
        else
        {
            _db.GlobalAyarlar.Add(new GlobalAyarlar
            {
                Id = DateTime.UtcNow.Ticks,
                Anahtar = anahtar,
                Deger = deger,
                Aciklama = aciklama,
                KayitTarihi = DateTime.Now,
                KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                AktifMi = true
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task<string> GetOrCreateAsync(string anahtar, Func<string> factory, string? aciklama = null)
    {
        var existing = await GetAsync(anahtar);
        if (!string.IsNullOrWhiteSpace(existing))
            return existing!;

        var value = factory();
        await SetAsync(anahtar, value, aciklama);
        return value;
    }
}
