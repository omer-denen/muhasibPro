using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Tests;

/// <summary>Faz 5 M2-politika ilk adım: yetki/lisans DI kayıtları + PermissionService
/// (ölü ITenantContext yerine seçim+auth aynaları).</summary>
public class PolitikaTests
{
    private static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (File.Exists(Path.Combine(dir, "MuhasibPro.slnx"))) return dir;
            dir = Path.GetDirectoryName(dir)!;
        }
        return AppContext.BaseDirectory;
    }

    [Fact]
    public void PolitikaServisleri_DI_Scoped_Kayitli()
    {
        var src = File.ReadAllText(Path.Combine(RepoRoot(),
            "Libraries/MuhasibPro.Business/HostBuilder/AddServicesHostBuilderExtensions.cs"));
        src.Should().Contain("AddScoped<IPermissionService, PermissionService>");
        src.Should().Contain("AddScoped<IModuleLicenseService, ModuleLicenseService>");
    }

    [Fact]
    public void PolitikaRepolar_DI_Scoped_Kayitli()
    {
        var src = File.ReadAllText(Path.Combine(RepoRoot(),
            "Libraries/MuhasibPro.Business/HostBuilder/AddRepositoryHostBuilderExtensions.cs"));
        src.Should().Contain("AddScoped<IKullaniciFirmaRolRepository, KullaniciFirmaRolRepository>");
        src.Should().Contain("AddScoped<IRolPermissionRepository, RolPermissionRepository>");
    }

    private static PermissionService KurServis(
        out Mock<IKullaniciFirmaRolRepository> kfr,
        out Mock<IRolPermissionRepository> rp,
        bool girisVar = true,
        long firmaId = 7,
        long kullaniciId = 5)
    {
        kfr = new Mock<IKullaniciFirmaRolRepository>();
        kfr.Setup(r => r.GetByKullaniciIdAsync(kullaniciId)).ReturnsAsync(new List<KullaniciFirmaRol>
        {
            new() { KullaniciId = kullaniciId, FirmaId = firmaId, RolId = 3 }
        });
        rp = new Mock<IRolPermissionRepository>();
        rp.Setup(r => r.GetByRolIdAsync(3)).ReturnsAsync(new List<RolPermission>
        {
            new() { RolId = 3, PermissionId = Permission.Veritabani_Goruntule }
        });
        var secim = new Mock<IFirmaWithMaliDonemSelectedService>();
        secim.SetupGet(s => s.SelectedFirma).Returns(new FirmaModel { Id = firmaId });
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(girisVar);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(kullaniciId);
        return new PermissionService(kfr.Object, rp.Object, secim.Object, auth.Object);
    }

    [Fact]
    public async Task HasPermission_RoldekiYetki_Dogru_Digeri_Yanlis()
    {
        var svc = KurServis(out _, out _);
        (await svc.HasPermissionAsync(Permission.Veritabani_Goruntule)).Should().BeTrue();
        (await svc.HasPermissionAsync(Permission.Veritabani_YedekAl)).Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_IdliAsiriYukleme_Cache_Kullanir()
    {
        var svc = KurServis(out var kfr, out var rp);
        (await svc.HasPermissionAsync(5, 7, Permission.Veritabani_Goruntule)).Should().BeTrue();
        (await svc.HasPermissionAsync(5, 7, Permission.Veritabani_Goruntule)).Should().BeTrue();
        kfr.Verify(r => r.GetByKullaniciIdAsync(5), Times.Once);
        rp.Verify(r => r.GetByRolIdAsync(3), Times.Once);
        svc.ClearCache();
        (await svc.HasPermissionAsync(5, 7, Permission.Veritabani_Goruntule)).Should().BeTrue();
        kfr.Verify(r => r.GetByKullaniciIdAsync(5), Times.Exactly(2));
    }

    [Fact]
    public async Task HasPermission_Girissiz_False_RepoCagrilmaz()
    {
        var svc = KurServis(out var kfr, out _, girisVar: false);
        (await svc.HasPermissionAsync(Permission.Veritabani_Goruntule)).Should().BeFalse();
        kfr.Verify(r => r.GetByKullaniciIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task HasPermission_Firmasiz_False()
    {
        var kfr = new Mock<IKullaniciFirmaRolRepository>();
        var rp = new Mock<IRolPermissionRepository>();
        var secim = new Mock<IFirmaWithMaliDonemSelectedService>();
        secim.SetupGet(s => s.SelectedFirma).Returns((FirmaModel)null!);
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(5);
        var svc = new PermissionService(kfr.Object, rp.Object, secim.Object, auth.Object);
        (await svc.HasPermissionAsync(Permission.Veritabani_Goruntule)).Should().BeFalse();
    }
}
