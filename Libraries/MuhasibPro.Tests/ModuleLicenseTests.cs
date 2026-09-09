using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Tests;

/// <summary>Faz 5 M2-politika: ModuleLicenseService KaydedenId artık auth'tan gelir
/// (girişsiz bootstrap'ta sistem kaydı 1).</summary>
public class ModuleLicenseTests
{
    private static (SistemDbContext ctx, SqliteConnection keepAlive) CreateContext()
    {
        var cs = $"Data Source=license{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var keepAlive = new SqliteConnection(cs);
        keepAlive.Open();
        var options = new DbContextOptionsBuilder<SistemDbContext>().UseSqlite(cs).Options;
        return (new SistemDbContext(options), keepAlive);
    }

    private static IAuthenticationService Kimlik(long kullaniciId)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(kullaniciId);
        return auth.Object;
    }

    [Fact]
    public async Task KaydedenId_Auth_Kullanicisindan_Gelir()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var svc = new ModuleLicenseService(ctx, Mock.Of<ILogService>(), Kimlik(42));

            var sonuc = await svc.UpdateActiveModulesAsync(5, new[] { ModuleType.Cari, ModuleType.Stok });

            sonuc.Success.Should().BeTrue();
            var satir = await ctx.GlobalAyarlar.SingleAsync(s => s.Anahtar == "Firma_5_ActiveModules");
            satir.KaydedenId.Should().Be(42);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task KaydedenId_Girissiz_SeedYoneticiye_Duser()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var svc = new ModuleLicenseService(ctx, Mock.Of<ILogService>());

            await svc.UpdateActiveModulesAsync(6, new[] { ModuleType.Cari });

            var satir = await ctx.GlobalAyarlar.SingleAsync(s => s.Anahtar == "Firma_6_ActiveModules");
            satir.KaydedenId.Should().Be(KullaniciSabitleri.SeedYoneticiId);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task Ikinci_Kayit_Ayni_Satiri_Gunceller()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var svc = new ModuleLicenseService(ctx, Mock.Of<ILogService>(), Kimlik(7));

            await svc.UpdateActiveModulesAsync(8, new[] { ModuleType.Cari });
            await svc.UpdateActiveModulesAsync(8, new[] { ModuleType.Stok });

            ctx.GlobalAyarlar.Count(s => s.Anahtar == "Firma_8_ActiveModules").Should().Be(1);
            var satir = await ctx.GlobalAyarlar.SingleAsync(s => s.Anahtar == "Firma_8_ActiveModules");
            satir.Deger.Should().Be(ModuleType.Stok.ToString());
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }
}
