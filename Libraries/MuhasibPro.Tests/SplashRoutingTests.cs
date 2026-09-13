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

    private static ApiDataResponse<DatabaseConnectionAnalysis> DbState(
        bool exists, bool connect, bool hasError, bool valid, List<string>? pending = null)
        => new SuccessApiDataResponse<DatabaseConnectionAnalysis>(
            new DatabaseConnectionAnalysis
            {
                IsDatabaseExists = exists,
                CanConnect = connect,
                HasError = hasError,
                DatabaseValid = valid,
                PendingMigrations = pending ?? new List<string>()
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

    // === 3 yollu karar testleri ===

    [Fact]
    public async Task DecideRoute_DbYok_FirstSetup()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(exists: false, connect: false, hasError: false, valid: false));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.IsDatabaseExists.Should().BeFalse();
        karar.Target.Should().Be(SplashTarget.FirstSetup);
    }

    [Fact]
    public async Task DecideRoute_DbVar_GecerliMigrationYok_Login()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(true, true, false, true));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.IsDatabaseReady.Should().BeTrue();
        karar.IsDatabaseExists.Should().BeTrue();
        karar.HasPendingMigrations.Should().BeFalse();
        karar.Target.Should().Be(SplashTarget.Login);
    }

    [Fact]
    public async Task DecideRoute_DbVar_MigrationGerekiyor_MigrationRequired()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(true, true, false, true, new List<string> { "20260908_AddIdentity" }));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.IsDatabaseExists.Should().BeTrue();
        karar.HasPendingMigrations.Should().BeTrue();
        karar.PendingMigrationCount.Should().Be(1);
        karar.Target.Should().Be(SplashTarget.MigrationRequired);
    }

    [Fact]
    public async Task DecideRoute_DbVar_BozukSema_MigrationRequired()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(true, true, false, false));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.IsDatabaseExists.Should().BeTrue();
        karar.IsDatabaseReady.Should().BeFalse();
        karar.Target.Should().Be(SplashTarget.MigrationRequired);
    }

    [Fact]
    public async Task DecideRoute_ServisPatlarsa_FailClosed_FirstSetup()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync()).ThrowsAsync(new InvalidOperationException("db yok"));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.IsDatabaseReady.Should().BeFalse();
        karar.IsDatabaseExists.Should().BeFalse();
        karar.Target.Should().Be(SplashTarget.FirstSetup);
    }

    [Fact]
    public async Task DecideRoute_StartupOverride_ReadyKorunur_ExistsPendingDbden()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(true, true, false, true, new List<string> { "M1" }));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        // startupDbReady=false override: ready=false, ama exists ve pending DB'den gelir
        var karar = await svc.DecideRouteAsync(false);

        karar.IsDatabaseReady.Should().BeFalse();
        karar.IsDatabaseExists.Should().BeTrue();
        karar.HasPendingMigrations.Should().BeTrue();
        karar.Target.Should().Be(SplashTarget.MigrationRequired);
    }

    // === Transfer testleri (degismedi) ===

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

    // === Adım 0: karar izi ===

    [Fact]
    public async Task DecideRoute_KararOzeti_HedefiVeSayilariTasir()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(true, true, false, true, new List<string> { "M1", "M2" }));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.KararOzeti.Should().Contain("exists=True");
        karar.KararOzeti.Should().Contain("pending=2");
        karar.KararOzeti.Should().Contain("target=MigrationRequired");
    }

    [Fact]
    public async Task DecideRoute_KararOzeti_LoginYolunuTasir()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(true, true, false, true));
        var svc = BuildService(sistem.Object,
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantSQLiteDatabaseService>(), Mock.Of<ITenantVersionReader>(), Mock.Of<IEventBus>());

        var karar = await svc.DecideRouteAsync(null);

        karar.KararOzeti.Should().Contain("ready=True");
        karar.KararOzeti.Should().Contain("target=Login");
    }
}
