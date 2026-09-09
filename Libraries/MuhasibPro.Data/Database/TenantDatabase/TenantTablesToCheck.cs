using MuhasibPro.Data.DataContext;

namespace MuhasibPro.Data.Database.TenantDatabase
{
    /// <summary>
    /// Tenant (dönem) veritabanında "varlığı kontrol edilecek" tabloların tek kaynağı.
    /// Tablo adları gerçek AppDbContext DbSet adlarından (nameof) türetilir —
    /// böylece TenantSQLiteMigrationManager şişmez, tablo listesi tek yerden yönetilir.
    /// </summary>
    public static class TenantTablesToCheck
    {
        public static readonly string[] All =
        {
            // Sadece "ağır" tablolar değil, veritabanı kurulu mu? sorusuna cevap veren kritik tablolar.
            nameof(AppDbContext.TenantDatabaseVersiyonlar),
            nameof(AppDbContext.AppLogs)
        };
    }
}