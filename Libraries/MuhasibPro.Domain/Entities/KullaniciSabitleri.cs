namespace MuhasibPro.Domain.Entities;

/// <summary>
/// Kullanıcı kimliği sabitleri — sihirli sayı yasağı (kural 7).
/// Girişsiz/sistem yazımlarının <c>KaydedenId</c> varsayılanı buradan gelir;
/// kullanıcı-girişli yazımlar auth'tan gelen gerçek id'yi kullanır.
/// </summary>
public static class KullaniciSabitleri
{
    /// <summary>
    /// Seed yöneticisi (korkutomer) — <c>SistemDbContext.SeedUser</c> bu Id'yi sabitler.
    /// Değeri değişirse seed + migration birlikte güncellenir.
    /// </summary>
    public const long SeedYoneticiId = 5413300800L;
}
