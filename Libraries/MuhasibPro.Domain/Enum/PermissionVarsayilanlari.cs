namespace MuhasibPro.Domain.Enum;

/// <summary>
/// Rol → izin matrisi varsayılanları (Faz 6.85 K1). Seed (HasData) + K3 izin matrisi
/// bu tek kaynaktan okur; iki yerde farklı set üretilmez.
/// <para><b>Yönetici</b> = tüm izinler (ayrıca <c>PermissionService</c> bu rolü bypass eder).</para>
/// <para><b>Kullanıcı</b> = temel görüntüleme seti (modül görüntüleme + AI yardım); yönetici K3'te düzenleyebilir.</para>
/// </summary>
public static class PermissionVarsayilanlari
{
    /// <summary>Tüm yetki değerleri (enum tanımının tek yansıması).</summary>
    public static IReadOnlyList<Permission> TumIzinler { get; } = System.Enum.GetValues<Permission>();

    /// <summary>Yönetici rolünün varsayılan izinleri — tüm izinler.</summary>
    public static IReadOnlyList<Permission> Yonetici => TumIzinler;

    /// <summary>
    /// Kullanıcı rolünün varsayılan izinleri: yalnız temel görüntüleme (modül <c>*_Goruntule</c>)
    /// + AI yardım asistanı. Veritabanı/log yönetimi ve tüm yazma işlemleri varsayılan olarak kapalıdır.
    /// </summary>
    public static IReadOnlyList<Permission> Kullanici { get; } = new[]
    {
        Permission.Cari_Goruntule,
        Permission.Stok_Goruntule,
        Permission.Fis_Goruntule,
        Permission.Fatura_Goruntule,
        Permission.Irsaliye_Goruntule,
        Permission.Kasa_Goruntule,
        Permission.Banka_Goruntule,
        Permission.Cek_Goruntule,
        Permission.Senet_Goruntule,
        Permission.Personel_Goruntule,
        Permission.Siparis_Goruntule,
        Permission.Teklif_Goruntule,
        Permission.Taksit_Goruntule,
        Permission.HesapPlani_Goruntule,
        Permission.Rapor_Goruntule,
        Permission.AiAsistan_Kullan
    };
}
