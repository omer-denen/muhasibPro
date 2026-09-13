using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Data.DataContext;

namespace MuhasibPro.Tests;

public class BusinessCoreTests
{
    [Fact]
    public void AuthenticationService_KullaniciFirmaRoller_Kullanmali_RolId_Dogrudan_Erisim_Olmamali()
    {
        var path = Path.Combine(GetRepoRoot(), "Libraries/MuhasibPro.Business/Services/SistemServices/Authentication/AuthenticationService.cs");
        var src = File.ReadAllText(path);
        src.Should().Contain("KullaniciFirmaRoller");
        src.Should().Contain("kfr.Rol");
        src.Should().NotContain("entity.RolId");
        src.Should().NotContain("entity.Rol !=");
    }

    [Fact]
    public void ISistemDatabaseService_5_Metod_Icermeli()
    {
        var type = typeof(MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices.ISistemDatabaseService);
        type.GetMethod("ValidateSistemDatabaseAsync").Should().NotBeNull();
        type.GetMethod("GetSistemDatabaseStateAsync").Should().NotBeNull();
        type.GetMethod("InitializeSistemDatabaseAsync").Should().NotBeNull();
        type.GetMethod("GetPendingMigrationsAsync").Should().NotBeNull();
        type.GetMethod("GetSistemDatabaseFullDiagStateAsync").Should().NotBeNull();
    }

    [Fact]
    public async Task SistemDiagnosticsService_Sqlite_InMemory_Calistirilmali()
    {
        var options = new DbContextOptionsBuilder<SistemDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        using var ctx = new SistemDbContext(options);
        ctx.Database.OpenConnection();
        ctx.Database.EnsureCreated();
        // Seed 1 kullanici
        ctx.Kullanicilar.Add(new MuhasibPro.Domain.Entities.SistemEntity.Kullanici
        {
            Id = 1,
            KullaniciAdi = "test",
            ParolaHash = "x",
            Adi = "A",
            Soyadi = "B",
            Eposta = "a@b.com",
            Telefon = "1",
            KaydedenId = 1,
            KayitTarihi = DateTime.UtcNow
        });
        ctx.SaveChanges();
        var svc = new MuhasibPro.Business.Services.SistemServices.AppServices.SistemDiagnosticsService(ctx);
        var count = await svc.GetKullaniciCountAsync();
        count.Should().Be(2, "Seed kullanıcı (korkutomer) + test kullanıcısı = 2");
        var canConnect = await svc.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public void AddDbManager_TenantSQLiteBackupManager_Tek_Kayit_Olmali()
    {
        // Ölü Managers/VACUUM yedekleyici silindi (Oturum 121) — tek kanonik kayıt kalır.
        var path = Path.Combine(GetRepoRoot(), "Libraries/MuhasibPro.Business/HostBuilder/AddDbManagerHostBuilderExtensions.cs");
        var src = File.ReadAllText(path);
        src.Should().NotContain("Contracts.Managers.ITenantSQLiteBackupManager");
        src.Should().Contain("Contracts.Database.TenantDatabase.ITenantSQLiteBackupManager");
    }

    private static string GetRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i=0;i<10;i++)
        {
            if (File.Exists(Path.Combine(dir, "MuhasibPro.slnx")) || File.Exists(Path.Combine(dir, "MuhasibPro.sln"))) return dir;
            dir = Path.GetDirectoryName(dir)!;
        }
        return Path.Combine(AppContext.BaseDirectory, "..","..","..","..","..");
    }
}
