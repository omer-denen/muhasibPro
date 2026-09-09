using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Services.UIService;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Tests;

public class SplashRoutingTests
{
    private static SplashRoutingService BuildService(
        ISistemDatabaseService sistem,
        IKurulumKayitService kurulum,
        IMakineKimligiProvider makine,
        ITenantSQLiteDatabaseService tenant,
        ITenantVersionReader reader,
        IEventBus bus)
        => new(sistem, kurulum, makine, tenant, reader, bus);

    private static ApiDataResponse<DatabaseConnectionAnalysis> DbState(bool exists, bool connect, bool hasError, bool valid)
        => new SuccessApiDataResponse<DatabaseConnectionAnalysis>(
            new DatabaseConnectionAnalysis
            {
                IsDatabaseExists = exists,
                CanConnect = connect,
                HasError = hasError,
                DatabaseValid = valid
            }, "durum");

    private static SplashRoutingService BuildDefault(
        out Mock<ITenantSQLiteDatabaseService> tenant,
        out Mock<ITenantVersionReader> reader,
        out Mock<IEventBus> bus)
    {
        var sistem = new Mock<ISistemDatabaseService>();
        var kurulum = new Mock<IKurulumKayitService>();
        kurulum.Setup(k => k.GetOrCreateAsync())
            .ReturnsAsync(new KurulumKayitModel { KurulumId = "K123456789", MachineGuid = "M1" });
        var makine = new Mock<IMakineKimligiProvider>();
        makine.Setup(m => m.GetMachineIdAsync()).ReturnsAsync("M1");
        tenant = new Mock<ITenantSQLiteDatabaseService>();
        tenant.Setup(t => t.BackfillMissingTenantIdentitiesAsync()).ReturnsAsync(0);
        reader = new Mock<ITenantVersionReader>();
        bus = new Mock<IEventBus>();
        return BuildService(sistem.Object, kurulum.Object, makine.Object, tenant.Object, reader.Object, bus.Object);
    }

    [Fact]
    public async Task DecideRoute_StartupFalse_KurulumaYonlendirir()
    {
        var svc = BuildDefault(out _, out _, out _);

        var karar = await svc.DecideRouteAsync(false);

        karar.IsDatabaseReady.Should().BeFalse();
        karar.Target.Should().Be(SplashTarget.SetupRequired);
    }

    [Fact]
    public async Task DecideRoute_StartupTrue_LogineYonlendirir()
    {
        var svc = BuildDefault(out _, out _, out _);

        var karar = await svc.DecideRouteAsync(true);

        karar.IsDatabaseReady.Should().BeTrue();
        karar.Target.Should().Be(SplashTarget.Login);
    }

    [Fact]
    public async Task DecideRoute_StartupNull_GecerliDbde_LogineYonlendirir()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(true, true, false, true));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.IsDatabaseReady.Should().BeTrue();
    }

    [Fact]
    public async Task DecideRoute_StartupNull_BozukDbde_KurulumaYonlendirir()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(true, true, false, false));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.IsDatabaseReady.Should().BeFalse();
        karar.Target.Should().Be(SplashTarget.SetupRequired);
    }

    [Fact]
    public async Task DecideRoute_ServisPatlarsa_GuvenliVarsayilanLogin()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync()).ThrowsAsync(new InvalidOperationException("db yok"));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.IsDatabaseReady.Should().BeTrue();
    }

    [Fact]
    public async Task CheckTransfer_UyumsuzlukYoksa_BosDoner_OlayYayinlamaz()
    {
        var svc = BuildDefault(out var tenant, out var reader, out var bus);
        reader.Setup(r => r.ScanMismatchesAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<TenantMismatchInfo>());

        var sonuc = await svc.CheckTransferAsync();

        sonuc.HasMismatches.Should().BeFalse();
        sonuc.CurrentKurulumId.Should().Be("K123456789");
        sonuc.CurrentMachineId.Should().Be("M1");
        tenant.Verify(t => t.BackfillMissingTenantIdentitiesAsync(), Times.Once);
        bus.Verify(b => b.Publish(It.IsAny<object>(), It.IsAny<TransferDetectedEvent>()), Times.Never);
    }

    [Fact]
    public async Task CheckTransfer_UyumsuzlukVarsa_Formatlar_Ve_OlayYayinlar()
    {
        var svc = BuildDefault(out _, out var reader, out var bus);
        reader.Setup(r => r.ScanMismatchesAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<TenantMismatchInfo>
            {
                new() { DatabaseName = "db-KODU_2027", VersiyonKurulumId = "ESKIKURULUM01", VersiyonMakineId = "M9", KurulumFarkli = true, MakineFarkli = true }
            });

        var sonuc = await svc.CheckTransferAsync();

        sonuc.HasMismatches.Should().BeTrue();
        sonuc.Mismatches.Should().HaveCount(1);
        sonuc.Mismatches[0].Should().Contain("db-KODU_2027");
        sonuc.Mismatches[0].Should().Contain("ESKIKURU");
        bus.Verify(b => b.Publish(It.IsAny<object>(), It.IsAny<TransferDetectedEvent>()), Times.Once);
    }

    [Fact]
    public void FormatMismatch_KisaIdleri_Korur()
    {
        var satir = SplashRoutingService.FormatMismatch(
            new TenantMismatchInfo { DatabaseName = "db-A_2026", VersiyonKurulumId = "ABC", MakineFarkli = false },
            "K123456789");

        satir.Should().Be("db-A_2026 • Kurulum:ABC → K1234567 • Makine:aynı");
    }
}
