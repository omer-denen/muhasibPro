namespace MuhasibPro.Domain.Entities;

/// <summary>
/// Kullanıcı rolü kimliği sabitleri — sihirli sayı yasağı (Kural 7).
/// Rol Id'leri <c>SeedDataKullaniciRol</c> içinde seed edilir; KFR/izin yazımları buradan okur.
/// Değerler değişirse seed + migration birlikte güncellenir.
/// </summary>
public static class KullaniciRolSabitleri
{
    /// <summary>Yönetici rolü — tüm izinlere sahip (PermissionService bypass eder).</summary>
    public const long YoneticiRolId = 241341L;

    /// <summary>Kullanıcı rolü — varsayılan temel görüntüleme seti (K3'te düzenlenebilir).</summary>
    public const long KullaniciRolId = 241342L;
}
