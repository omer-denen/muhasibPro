using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.DataContext.Configurations;
using MuhasibPro.Data.DataContext.SeedDataSistem;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.DataContext;

public class SistemDbContext : DbContext
{
    public DbSet<SistemLog> SistemLogs { get; set; } = null!;
    public DbSet<AppVersion> AppVersiyonlar { get; set; } = null!;
    public DbSet<AppDbVersion> AppDbVersiyonlar { get; set; } = null!;
    public DbSet<Hesap> Hesaplar { get; set; } = null!;
    public DbSet<Kullanici> Kullanicilar { get; set; } = null!;
    public DbSet<KullaniciRol> KullaniciRoller { get; set; } = null!;
    public DbSet<Firma> Firmalar { get; set; } = null!;
    public DbSet<MaliDonem> MaliDonemler { get; set; } = null!;
    public DbSet<KullaniciFirmaRol> KullaniciFirmaRoller { get; set; } = null!;
    public DbSet<RolPermission> RolPermissionlar { get; set; } = null!;
    public DbSet<AuditLog> AuditLoglar { get; set; } = null!;
    public DbSet<Lisans> Lisanslar { get; set; } = null!;
    public DbSet<GlobalAyarlar> GlobalAyarlar { get; set; } = null!;
    public DbSet<OturumKaydi> OturumKayitlari { get; set; } = null!;

    protected SistemDbContext()
    {
    }

    public SistemDbContext(DbContextOptions<SistemDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        SeedUser(modelBuilder);
        SeedInitialVersion(modelBuilder);
        modelBuilder.ApplyConfiguration(new MaliDonemConfiguration());
        modelBuilder.ApplyConfiguration(new KullanicilarConfiguration());
        modelBuilder.Entity<Hesap>().HasKey(h => h.KullaniciId);
        modelBuilder.Entity<KullaniciFirmaRol>().HasKey(x => new { x.KullaniciId, x.FirmaId });
        modelBuilder.Entity<RolPermission>().HasKey(x => new { x.RolId, x.PermissionId });
        modelBuilder.Entity<Kullanici>().HasMany(x => x.KullaniciFirmaRoller).WithOne(x => x.Kullanici)
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Firma>().HasMany(x => x.KullaniciFirmaRoller).WithOne(x => x.Firma)
            .HasForeignKey(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<KullaniciFirmaRol>().HasOne(x => x.Rol).WithMany()
            .HasForeignKey(x => x.RolId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<KullaniciRol>().HasMany(x => x.Yetkiler).WithOne(x => x.Rol)
            .HasForeignKey(x => x.RolId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Firma>().HasMany(x => x.MaliDonemler).WithOne(x => x.Firma)
            .HasForeignKey(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void SeedUser(ModelBuilder modelBuilder)
    {
        var kullanici = new Kullanici
        {
            Id = KullaniciSabitleri.SeedYoneticiId,
            Adi = "Ömer",
            AktifMi = true,
            Eposta = "korkutomer@gmail.com",
            KaydedenId = KullaniciSabitleri.SeedYoneticiId,
            KayitTarihi = new DateTime(2025, 3, 12),
            KullaniciAdi = "korkutomer",
            ParolaHash = "AQAAAAIAAYagAAAAECnYdlrjFiWFJc+FGeGDmvR87uz20oU/Z0K4JE9ddoF2VUnmHw0idEFX8UPOb4cpzQ==",
            Soyadi = "Korkut",
            Telefon = "0 (541) 330 0800",
            ArananTerim = "korkutomer, Ömer Korkut, Yönetici"
        };
        modelBuilder.Entity<Kullanici>().HasData(kullanici);
        SeedDataKullaniciRol.SeedKullaniciRoller(modelBuilder);
    }

    private void SeedInitialVersion(ModelBuilder modelBuilder)
    {
        var appDbVersion = new AppDbVersion
        {
            DatabaseName = "Sistem.db",
            CurrentAppVersion = "1.0.0",
            CurrentAppVersionLastUpdate = new DateTime(2025, 9, 22),
            PreviousAppVersiyon = null,
            CurrentDatabaseVersion = "1.0.0",
            CurrentDatabaseLastUpdate = new DateTime(2025, 9, 22),
            PreviousDatabaseVersion = null
        };
        modelBuilder.Entity<AppDbVersion>().HasData(appDbVersion);
    }
}
