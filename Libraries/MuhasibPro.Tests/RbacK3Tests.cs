using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Data.Repository.Common.BaseRepo;
using MuhasibPro.Data.Repository.SistemRepos;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.85 K3 — rol izin matrisi servisi: RolPermission okuma/yazma + yönetici kapısı + cache temizliği.</summary>
public class RbacK3Tests
{
    private static (SistemDbContext ctx, SqliteConnection keepAlive) CreateContext()
    {
        var cs = $"Data Source=rbac3{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var keepAlive = new SqliteConnection(cs);
        keepAlive.Open();
        var options = new DbContextOptionsBuilder<SistemDbContext>().UseSqlite(cs).Options;
        return (new SistemDbContext(options), keepAlive);
    }

    private static IAuthenticationService Kimlik(bool yonetici)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(yonetici ? KullaniciSabitleri.SeedYoneticiId : 999);
        auth.SetupGet(a => a.CurrentAccount).Returns(new HesapModel
        {
            KullaniciId = yonetici ? KullaniciSabitleri.SeedYoneticiId : 999,
            KullaniciModel = new KullaniciModel
            {
                Rol = new KullaniciRolModel
                {
                    RolTip = yonetici ? KullaniciRolTip.Yönetici : KullaniciRolTip.Kullanici
                }
            }
        });
        return auth.Object;
    }

    private static RolYetkiService KurServis(SistemDbContext ctx, bool yonetici, out Mock<IPermissionService> perm)
    {
        perm = new Mock<IPermissionService>();
        return new RolYetkiService(
            new RolPermissionRepository(ctx),
            perm.Object,
            Kimlik(yonetici),
            new UnitOfWork<SistemDbContext>(ctx));
    }

    [Fact]
    public async Task GetMatris_SeedIle_SeciliSeti_Dondurur()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var svc = KurServis(ctx, true, out _);

            var sonuc = await svc.GetMatrisAsync(KullaniciRolSabitleri.KullaniciRolId);

            sonuc.Success.Should().BeTrue();
            sonuc.Data.Should().HaveCount(PermissionVarsayilanlari.TumIzinler.Count);
            sonuc.Data.Where(x => x.Secili).Select(x => x.Izin)
                .Should().BeEquivalentTo(PermissionVarsayilanlari.Kullanici);
            sonuc.Data.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.Kategori) && !string.IsNullOrWhiteSpace(x.Ad));
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task SetIzin_EklerVeKaldirir_CacheTemizlenir()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var svc = KurServis(ctx, true, out var perm);

            var ekle = await svc.SetIzinAsync(KullaniciRolSabitleri.KullaniciRolId, Permission.Cari_Ekle, true);
            ekle.Success.Should().BeTrue();
            (await ctx.RolPermissionlar.CountAsync(x =>
                x.RolId == KullaniciRolSabitleri.KullaniciRolId && x.PermissionId == Permission.Cari_Ekle))
                .Should().Be(1);

            var kaldir = await svc.SetIzinAsync(KullaniciRolSabitleri.KullaniciRolId, Permission.Cari_Ekle, false);
            kaldir.Success.Should().BeTrue();
            (await ctx.RolPermissionlar.CountAsync(x =>
                x.RolId == KullaniciRolSabitleri.KullaniciRolId && x.PermissionId == Permission.Cari_Ekle))
                .Should().Be(0);

            perm.Verify(p => p.ClearCache(), Times.Exactly(2));
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task SetIzin_YoneticiRolu_Ve_Yetkisiz_Reddedilir()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();

            var yoneticiRol = KurServis(ctx, true, out _);
            var sonuc = await yoneticiRol.SetIzinAsync(
                KullaniciRolSabitleri.YoneticiRolId, Permission.Cari_Goruntule, false);
            sonuc.Success.Should().BeFalse();

            var normal = KurServis(ctx, false, out var permNormal);
            var yetkisiz = await normal.SetIzinAsync(
                KullaniciRolSabitleri.KullaniciRolId, Permission.Cari_Goruntule, false);
            yetkisiz.Success.Should().BeFalse();
            permNormal.Verify(p => p.ClearCache(), Times.Never);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }
}
