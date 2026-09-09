using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Data.DataContext.Configurations;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Tests;

public class DataCoreTests
{
    [Fact]
    public void SistemDbContext_14_DbSet_Icermeli()
    {
        var props = typeof(SistemDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.Name).ToHashSet();
        props.Should().Contain("Kullanicilar");
        props.Should().Contain("KullaniciRoller");
        props.Should().Contain("KullaniciFirmaRoller");
        props.Should().Contain("RolPermissionlar");
        props.Should().Contain("Firmalar");
        props.Should().Contain("MaliDonemler");
        props.Should().Contain("SistemLogs");
        props.Should().Contain("Hesaplar");
        props.Should().Contain("AuditLoglar");
        props.Should().Contain("Lisanslar");
        props.Should().Contain("GlobalAyarlar");
        props.Should().Contain("OturumKayitlari");
        props.Count.Should().BeGreaterThanOrEqualTo(14, "SistemDbContext 14 tablo (Oturum 2) gerilememeli");
    }

    [Fact]
    public void SistemDbContext_OnModelCreating_CompositeKey_Ve_HasMany()
    {
        var src = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MuhasibPro.Data", "DataContext", "SistemDbContext.cs"));
        // Fallback: read from repo path
        if (!src.Contains("HasMany"))
        {
            var repoPath = Path.Combine(GetRepoRoot(), "Libraries/MuhasibPro.Data/DataContext/SistemDbContext.cs");
            src = File.ReadAllText(repoPath);
        }
        src.Should().Contain("KullaniciFirmaRol");
        src.Should().Contain("HasKey");
        src.Should().Contain("RolPermission");
        src.Should().Contain("HasMany(x => x.KullaniciFirmaRoller)");
        src.Should().Contain("ArananTerim"); // SeedUser uses it
    }

    [Fact]
    public void KullanicilarConfiguration_HasMany_Olmali_HasOne_Rol_Olmamali()
    {
        var repoPath = Path.Combine(GetRepoRoot(), "Libraries/MuhasibPro.Data/DataContext/Configurations/KullanicilarConfiguration.cs");
        var src = File.ReadAllText(repoPath);
        src.Should().Contain("HasMany");
        src.Should().Contain("KullaniciFirmaRoller");
        src.Should().NotContain("HasOne(k => k.Rol)");
        src.Should().NotContain("HasForeignKey(k => k.RolId)");
    }

    [Fact]
    public void UserRepository_ThenInclude_Rol_Olmali()
    {
        var repoPath = Path.Combine(GetRepoRoot(), "Libraries/MuhasibPro.Data/Repository/SistemRepos/Authentication/UserRepository.cs");
        var src = File.ReadAllText(repoPath);
        src.Should().Contain("ThenInclude");
        src.Should().Contain("KullaniciFirmaRoller");
        src.Should().NotContain("Include(a => a.Rol)");
    }

    [Fact]
    public void DbContextAnalysis_Batch_Tek_Kaynak_Olmali()
    {
        var diagPath = Path.Combine(GetRepoRoot(), "Libraries/MuhasibPro.Data/Database/Extensions/DbContextDiagnosticsExtensions.cs");
        var diagSrc = File.ReadAllText(diagPath);
        // Diagnostics'te Batch olmamalı, merkez Analysis'te olmalı (HATALAR Batch ambiguity)
        diagSrc.Should().NotContain("IEnumerable<IEnumerable<T>> Batch<T>");
        var analysisPath = Path.Combine(GetRepoRoot(), "Libraries/MuhasibPro.Data/Database/Extensions/DbContextAnalysisExtensions.cs");
        var analysisSrc = File.ReadAllText(analysisPath);
        analysisSrc.Should().Contain("IEnumerable<IEnumerable<T>> Batch<T>");
    }

    [Fact]
    public void AppDbContext_Sadece_2_DbSet_Icermeli_FazB_Kilitli()
    {
        var props = typeof(AppDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.Name).ToHashSet();
        props.Should().Contain("TenantDatabaseVersiyonlar");
        props.Should().Contain("AppLogs");
        props.Count.Should().Be(2, "AppDbContext Faz B kilitliyken sadece 2 DbSet (Tenant versiyon) olmalı; 69 muhasebe entity'si ITenantMuhasebeEntities sözleşmesinde bekliyor");
    }

    [Fact]
    public void ITenantMuhasebeEntities_69_DbSet_Sozlesmesi_Tam_Olmali()
    {
        var iface = typeof(MuhasibPro.Data.Contracts.Database.Common.ITenantMuhasebeEntities);
        var props = iface.GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.Name)
            .ToHashSet();

        props.Count.Should().Be(67, "69 muhasebe entity'si (AppLog + TenantDatabaseVersiyon hariç) interface sözleşmesinde yer almalı — entity keşfi çalışması boşa gitmemeli");

        props.Should().Contain("CariHesaplar");
        props.Should().Contain("Stoklar");
        props.Should().Contain("Faturalar");
        props.Should().Contain("Kasalar");
        props.Should().Contain("Personeller");
        props.Should().Contain("Senetler");
        props.Should().Contain("BankaHesaplar");
        props.Should().Contain("Teklifler");
        props.Should().Contain("Siparisler");
        props.Should().Contain("Ayarlar");
        props.Should().Contain("BelgeNumaralar");
        props.Should().Contain("VarsayilanDegerler");
    }

    [Fact]
    public void AppDbContext_ITenantMuhasebeEntities_Henuz_Implemente_Edilmemeli()
    {
        // Faz B (muhasebe modülleri) açılmadığı sürece AppDbContext interface'i implemente etmez.
        typeof(AppDbContext).GetInterfaces()
            .Should().NotContain(typeof(MuhasibPro.Data.Contracts.Database.Common.ITenantMuhasebeEntities),
                "Interface yalnız Faz B'de implemente edilecek");
    }

    private static string GetRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i=0;i<10;i++)
        {
            if (File.Exists(Path.Combine(dir, "MuhasibPro.slnx"))) return dir;
            dir = Path.GetDirectoryName(dir)!;
        }
        return Path.Combine(AppContext.BaseDirectory, "..","..","..","..","..");
    }
}
