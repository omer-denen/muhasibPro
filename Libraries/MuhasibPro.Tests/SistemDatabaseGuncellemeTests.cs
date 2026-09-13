using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.77 Adım 1: ApplyPendingSistemMigrationsAsync (yedek-önce-göç) zincir testleri.</summary>
public class SistemDatabaseGuncellemeTests
{
    private static SistemDatabaseService Servis(
        Mock<ISistemMigrationManager> goc,
        Mock<ISistemBackupManager> yedek) =>
        new(Mock.Of<ILogService>(), goc.Object, yedek.Object);

    private static DatabaseConnectionAnalysis GecerliDurum() => new()
    {
        IsDatabaseExists = true,
        CanConnect = true,
        DatabaseValid = true
    };

    [Fact]
    public async Task BekleyenYoksa_BilgiDoner_GocCalismaz()
    {
        var goc = new Mock<ISistemMigrationManager>();
        goc.Setup(m => m.GetPendingMigrationsAsync()).ReturnsAsync(new List<string>());
        var yedek = new Mock<ISistemBackupManager>();

        var (ok, mesaj) = await Servis(goc, yedek).ApplyPendingSistemMigrationsAsync();

        ok.Should().BeFalse();
        mesaj.Should().Contain("Bekleyen");
        yedek.Verify(m => m.CreateBackupAsync(It.IsAny<DatabaseBackupType>()), Times.Never);
    }

    [Fact]
    public async Task YedekAlinamayina_Durur_GocCalismaz()
    {
        var goc = new Mock<ISistemMigrationManager>();
        goc.Setup(m => m.GetPendingMigrationsAsync()).ReturnsAsync(new List<string> { "20260911_X" });
        var yedek = new Mock<ISistemBackupManager>();
        yedek.Setup(m => m.CreateBackupAsync(DatabaseBackupType.Manual))
            .ReturnsAsync((DatabaseBackupResult)null!);

        var (ok, mesaj) = await Servis(goc, yedek).ApplyPendingSistemMigrationsAsync();

        ok.Should().BeFalse();
        mesaj.Should().Contain("yedek");
        goc.Verify(m => m.InitializeSistemDatabaseAsync(), Times.Never);
    }

    [Fact]
    public async Task GocBasarisizsa_HataDoner()
    {
        var goc = new Mock<ISistemMigrationManager>();
        goc.Setup(m => m.GetPendingMigrationsAsync()).ReturnsAsync(new List<string> { "20260911_X" });
        goc.Setup(m => m.InitializeSistemDatabaseAsync()).ReturnsAsync((false, "göç patladı"));
        var yedek = new Mock<ISistemBackupManager>();
        yedek.Setup(m => m.CreateBackupAsync(DatabaseBackupType.Manual))
            .ReturnsAsync(new DatabaseBackupResult { IsBackupComleted = true });

        var (ok, mesaj) = await Servis(goc, yedek).ApplyPendingSistemMigrationsAsync();

        ok.Should().BeFalse();
        mesaj.Should().Contain("göç patladı");
    }

    [Fact]
    public async Task BasariliAkis_YedekGocDogrulama()
    {
        var goc = new Mock<ISistemMigrationManager>();
        goc.Setup(m => m.GetPendingMigrationsAsync()).ReturnsAsync(new List<string> { "20260911_X" });
        goc.Setup(m => m.InitializeSistemDatabaseAsync()).ReturnsAsync((true, "göç uygulandı"));
        goc.Setup(m => m.GetSistemDatabaseStateAsync()).ReturnsAsync(GecerliDurum());
        var yedek = new Mock<ISistemBackupManager>();
        yedek.Setup(m => m.CreateBackupAsync(DatabaseBackupType.Manual))
            .ReturnsAsync(new DatabaseBackupResult { IsBackupComleted = true });

        var (ok, mesaj) = await Servis(goc, yedek).ApplyPendingSistemMigrationsAsync();

        ok.Should().BeTrue();
        mesaj.Should().Contain("göç uygulandı");
        yedek.Verify(m => m.CreateBackupAsync(DatabaseBackupType.Manual), Times.Once);
    }
}
