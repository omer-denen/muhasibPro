using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Tests;

/// <summary>Faz 5 M2-politika: LisansService (tür ayardan, satırlar tablodan).</summary>
public class LisansServiceTests
{
    private sealed class BellekAyarlari : ILocalSettingsService
    {
        public readonly Dictionary<string, object> Kutu = new();
        public Task<T?> ReadSettingAsync<T>(string key)
        {
            if (Kutu.TryGetValue(key, out var raw) && raw is T deger)
                return Task.FromResult<T?>(deger);
            return Task.FromResult<T?>(default);
        }
        public Task SaveSettingAsync<T>(string key, T value)
        {
            Kutu[key] = value!;
            return Task.CompletedTask;
        }
    }

    private static (SistemDbContext ctx, SqliteConnection keepAlive) CreateContext()
    {
        var cs = $"Data Source=lisans{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var keepAlive = new SqliteConnection(cs);
        keepAlive.Open();
        var options = new DbContextOptionsBuilder<SistemDbContext>().UseSqlite(cs).Options;
        return (new SistemDbContext(options), keepAlive);
    }

    private static IAuthenticationService Kimlik(long id, KullaniciRolTip? rol)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(rol.HasValue);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(rol.HasValue ? id : -1);
        auth.SetupGet(a => a.CurrentAccount).Returns(rol.HasValue
            ? new HesapModel
            {
                KullaniciId = id,
                KullaniciModel = new KullaniciModel { Rol = new KullaniciRolModel { RolTip = rol.Value } }
            }
            : null!);
        return auth.Object;
    }

    private static LisansService KurServis(
        SistemDbContext ctx, BellekAyarlari bellek, IAuthenticationService auth)
    {
        return new LisansService(ctx, new LicenseSettingsProvider(bellek, auth), auth);
    }

    [Fact]
    public async Task Deneme_Satir_Istemez()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var svc = KurServis(ctx, new BellekAyarlari(), Kimlik(1, KullaniciRolTip.Yönetici));

            var durum = await svc.GetLisansDurumuAsync();

            durum.Success.Should().BeTrue();
            durum.Data.Tur.Should().Be(LisansTuru.Deneme);
            durum.Data.GecerliMi.Should().BeTrue();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task Standart_Satirsiz_Gecersiz()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var bellek = new BellekAyarlari();
            var auth = Kimlik(1, KullaniciRolTip.Yönetici);
            await new LicenseSettingsProvider(bellek, auth).SaveAsync(
                new MuhasibPro.Domain.Models.LicenseSettings { Tur = LisansTuru.Standart });
            var svc = KurServis(ctx, bellek, auth);

            var durum = await svc.GetLisansDurumuAsync();

            durum.Data.Tur.Should().Be(LisansTuru.Standart);
            durum.Data.GecerliMi.Should().BeFalse();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task Kaydet_Yonetici_Kaydeder_KaydedenId_Auth()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var bellek = new BellekAyarlari();
            var auth = Kimlik(42, KullaniciRolTip.Yönetici);
            await new LicenseSettingsProvider(bellek, auth).SaveAsync(
                new MuhasibPro.Domain.Models.LicenseSettings { Tur = LisansTuru.Profesyonel });
            var svc = KurServis(ctx, bellek, auth);

            var kayit = await svc.KaydetLisansAsync(LisansTuru.Profesyonel, "ANAHTAR-1",
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(30), "yıllık");
            kayit.Success.Should().BeTrue();

            var satir = await ctx.Lisanslar.SingleAsync();
            satir.KaydedenId.Should().Be(42);

            var durum = await svc.GetLisansDurumuAsync();
            durum.Data.GecerliMi.Should().BeTrue();
            durum.Data.KalanGun.Should().Be(30);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task Kaydet_NormalKullanici_Reddedilir()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var svc = KurServis(ctx, new BellekAyarlari(), Kimlik(11, KullaniciRolTip.Kullanici));

            var kayit = await svc.KaydetLisansAsync(LisansTuru.Standart, "ANAHTAR-2",
                DateTime.Today, DateTime.Today.AddDays(10));

            kayit.Success.Should().BeFalse();
            (await ctx.Lisanslar.CountAsync()).Should().Be(0);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task Kaydet_TersTarih_Reddedilir()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var svc = KurServis(ctx, new BellekAyarlari(), Kimlik(1, KullaniciRolTip.Yönetici));

            var kayit = await svc.KaydetLisansAsync(LisansTuru.Standart, "ANAHTAR-3",
                DateTime.Today, DateTime.Today.AddDays(-5));

            kayit.Success.Should().BeFalse();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task Suresi_Dolmus_Gecersiz()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();
            var bellek = new BellekAyarlari();
            var auth = Kimlik(1, KullaniciRolTip.Yönetici);
            await new LicenseSettingsProvider(bellek, auth).SaveAsync(
                new MuhasibPro.Domain.Models.LicenseSettings { Tur = LisansTuru.Kurumsal });
            var svc = KurServis(ctx, bellek, auth);
            await svc.KaydetLisansAsync(LisansTuru.Kurumsal, "ESKI",
                DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-1));

            var durum = await svc.GetLisansDurumuAsync();

            durum.Data.GecerliMi.Should().BeFalse();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }
}
