using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Tests;

public class BusinessDatabaseTests
{
    #region TenantHelperExtensions.ValidateMaliYil

    [Theory]
    [InlineData(2026)]
    [InlineData(2027)]
    [InlineData(2100)]
    public void ValidateMaliYil_GecerliYil_Basari(int maliYil)
    {
        var sonuc = maliYil.ValidateMaliYil();
        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ValidateMaliYil_SifirVeyaNegatif_Hata(int maliYil)
    {
        var sonuc = maliYil.ValidateMaliYil();
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("sıfır veya negatif");
    }

    [Fact]
    public void ValidateMaliYil_CokEski_Hata()
    {
        var sonuc = (DateTime.Now.Year - 3).ValidateMaliYil();
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("çok eski");
    }

    [Fact]
    public void ValidateMaliYil_CokIleri_Hata()
    {
        var sonuc = 2101.ValidateMaliYil();
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("çok ileri");
    }

    #endregion

    #region TenantHelperExtensions.ValidateMaliDonemExistsAsync

    [Fact]
    public async Task ValidateMaliDonemExistsAsync_DonemMevcut_Hata()
    {
        var donemSvc = new Mock<IMaliDonemService>();
        donemSvc.Setup(s => s.IsMaliDonemExistsAsync(1, 2027)).ReturnsAsync(true);

        var sonuc = await donemSvc.Object.ValidateMaliDonemExistsAsync(1, 2027);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("zaten mevcut");
    }

    [Fact]
    public async Task ValidateMaliDonemExistsAsync_DonemYok_Basari()
    {
        var donemSvc = new Mock<IMaliDonemService>();
        donemSvc.Setup(s => s.IsMaliDonemExistsAsync(1, 2027)).ReturnsAsync(false);

        var sonuc = await donemSvc.Object.ValidateMaliDonemExistsAsync(1, 2027);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateMaliDonemExistsAsync_ServisException_Hata()
    {
        var donemSvc = new Mock<IMaliDonemService>();
        donemSvc.Setup(s => s.IsMaliDonemExistsAsync(It.IsAny<long>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("db capraz"));

        var sonuc = await donemSvc.Object.ValidateMaliDonemExistsAsync(1, 2027);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("Mali dönem kontrol hatası").And.Contain("db capraz");
    }

    #endregion

    #region TenantHelperExtensions.GenerateDatabaseName

    private static Mock<IApplicationPaths> BuildPathsMock(bool fileExists)
    {
        var paths = new Mock<IApplicationPaths>();
        paths.Setup(p => p.SanitizeDatabaseName(It.IsAny<string>())).Returns((string s) => s);
        paths.Setup(p => p.TenantDatabaseFileExists(It.IsAny<string>())).Returns(fileExists);
        return paths;
    }

    [Fact]
    public void GenerateDatabaseName_NullApplicationPaths_Hata()
    {
        IApplicationPaths? paths = null;
        var sonuc = TenantHelperExtensions.GenerateDatabaseName(paths!, "FIRMA01", 2027);
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Be("ApplicationPaths null");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GenerateDatabaseName_BosFirmaKodu_Hata(string? firmaKodu)
    {
        var sonuc = BuildPathsMock(false).Object.GenerateDatabaseName(firmaKodu!, 2027);
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Be("Firma Kodu boş olamaz!");
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void GenerateDatabaseName_GecersizYil_Hata(int maliYil)
    {
        var sonuc = BuildPathsMock(false).Object.GenerateDatabaseName("FIRMA01", maliYil);
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Be("Geçersiz mali dönem yılı!");
    }

    [Fact]
    public void GenerateDatabaseName_AdKullanimda_Hata()
    {
        var sonuc = BuildPathsMock(true).Object.GenerateDatabaseName("FIRMA01", 2027);
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("kullanılıyor");
    }

    [Fact]
    public void GenerateDatabaseName_Basari()
    {
        var sonuc = BuildPathsMock(false).Object.GenerateDatabaseName("firma01", 2027);
        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().Be("db-FIRMA01_2027", "prefix 'db-' + firma kodu buyuk harf (invariant) + _yil, tek ayrac");
    }

    #endregion

    #region TenantHelperExtensions.ValidateFirmaAsync

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateFirmaAsync_GecersizId_Hata(long firmaId)
    {
        var sonuc = await new Mock<IFirmaService>().Object.ValidateFirmaAsync(firmaId);
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("Firma ID boş veya geçersiz");
    }

    [Fact]
    public async Task ValidateFirmaAsync_ServisHataDoner_HataMesajiGecir()
    {
        var firmaSvc = new Mock<IFirmaService>();
        firmaSvc.Setup(s => s.GetByFirmaIdAsync(7))
            .ReturnsAsync(new ErrorApiDataResponse<FirmaModel>(null!, "Firma bulunamadı"));

        var sonuc = await firmaSvc.Object.ValidateFirmaAsync(7);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Be("Firma bulunamadı");
    }

    [Fact]
    public async Task ValidateFirmaAsync_Basari()
    {
        var model = new FirmaModel { Id = 7, FirmaKodu = "F07", KisaUnvani = "ACME" };
        var firmaSvc = new Mock<IFirmaService>();
        firmaSvc.Setup(s => s.GetByFirmaIdAsync(7))
            .ReturnsAsync(new SuccessApiDataResponse<FirmaModel>(model, "getirildi"));

        var sonuc = await firmaSvc.Object.ValidateFirmaAsync(7);

        sonuc.Success.Should().BeTrue();
        sonuc.Data!.FirmaKodu.Should().Be("F07");
    }

    [Fact]
    public async Task ValidateFirmaAsync_ServisException_Hata()
    {
        var firmaSvc = new Mock<IFirmaService>();
        firmaSvc.Setup(s => s.GetByFirmaIdAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("baglanti koptu"));

        var sonuc = await firmaSvc.Object.ValidateFirmaAsync(7);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("Firma doğrulanırken hata").And.Contain("baglanti koptu");
    }

    #endregion

    #region TenantSQLiteSelectionService

    private static DatabaseMigrationExecutionResult MigrationResult(bool healthy) =>
        new()
        {
            DatabaseName = "db-test",
            HasError = !healthy,
            CanConnect = healthy,
            Message = healthy ? "ok" : "migration patladı"
        };

    private static (TenantSQLiteSelectionService service, Mock<ITenantSQLiteSelectionManager> manager, Mock<IMessageService> messages)
        BuildSelectionService(
            bool tenantLoaded = false,
            TenantContext? current = null,
            bool canConnect = true,
            bool initHealthy = true,
            TenantContext? switched = null)
    {
        var manager = new Mock<ITenantSQLiteSelectionManager>();
        manager.SetupGet(m => m.IsTenantLoaded).Returns(tenantLoaded);
        manager.Setup(m => m.GetCurrentTenant()).Returns(current ?? TenantContext.Empty);
        manager.Setup(m => m.SwitchToTenantAsync(It.IsAny<TenantContext>()))
            .Returns(switched ?? new TenantContext { DatabaseName = "db-yeni" });

        var dbManager = new Mock<ITenantSQLiteDatabaseManager>();
        dbManager.Setup(m => m.InitializeTenantDatabaseAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync(MigrationResult(initHealthy));

        var connFactory = new Mock<ITenantSQLiteConnectionStringFactory>();
        connFactory.Setup(m => m.ValidateConnectionStringAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync((canConnect, "test", $"Data Source={Guid.NewGuid():N}"));

        var sistemLog = new Mock<ISistemLogService>();
        var logSvc = new Mock<ILogService>();
        logSvc.SetupGet(l => l.SistemLogService).Returns(sistemLog.Object);

        var messages = new Mock<IMessageService>();
        var service = new TenantSQLiteSelectionService(
            manager.Object, messages.Object, logSvc.Object, dbManager.Object, connFactory.Object);
        return (service, manager, messages);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SwitchTenantAsync_BosVeritabaniAdi_Hata(string? databaseName)
    {
        var (service, _, _) = BuildSelectionService();
        var sonuc = await service.SwitchTenantAsync(databaseName!);
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Be("Veritabanı adı boş olamaz");
    }

    [Fact]
    public async Task SwitchTenantAsync_AyniTenant_Hata()
    {
        var (service, _, _) = BuildSelectionService(
            tenantLoaded: true,
            current: new TenantContext { DatabaseName = "DB_X" });

        var sonuc = await service.SwitchTenantAsync("db_x");

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("Zaten bu mali dönemi kullanıyorsunuz");
    }

    [Fact]
    public async Task SwitchTenantAsync_BaglantiDizesiBasarisiz_Hata()
    {
        var (service, _, _) = BuildSelectionService(canConnect: false);
        var sonuc = await service.SwitchTenantAsync("db-yok");
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("bağlantı dizesi oluşturulamadı");
    }

    [Fact]
    public async Task SwitchTenantAsync_InitBasarisiz_Hata()
    {
        var (service, manager, _) = BuildSelectionService(initHealthy: false);
        var sonuc = await service.SwitchTenantAsync("db-bozuk");

        sonuc.Success.Should().BeFalse();
        manager.Verify(m => m.SwitchToTenantAsync(It.IsAny<TenantContext>()), Times.Never);
    }

    [Fact]
    public async Task SwitchTenantAsync_Basari()
    {
        var (service, manager, _) = BuildSelectionService();
        var sonuc = await service.SwitchTenantAsync("db-yeni");

        sonuc.Success.Should().BeTrue();
        sonuc.Message.Should().Contain("db-yeni").And.Contain("dönemine geçildi");
        manager.Verify(m => m.SwitchToTenantAsync(It.Is<TenantContext>(t => t.DatabaseName == "db-yeni")), Times.Once);
    }

    [Fact]
    public void TenantChanged_Olayi_MessageServiceEYayilmali()
    {
        var (service, manager, messages) = BuildSelectionService();
        var tenant = new TenantContext { DatabaseName = "db-olay" };

        manager.Raise(m => m.TenantChanged += null, tenant);

        messages.Verify(m => m.Send(service, "TenantChanged", tenant), Times.Once);
    }

    [Fact]
    public async Task DisconnectCurrentTenantAsync_Yuklenmemis_Hata()
    {
        var (service, _, _) = BuildSelectionService(tenantLoaded: false);
        var sonuc = await service.DisconnectCurrentTenantAsync();
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("bağlantı bulunamadı");
    }

    [Fact]
    public async Task DisconnectCurrentTenantAsync_Basari()
    {
        var manager = new Mock<ITenantSQLiteSelectionManager>();
        var yuklu = true;
        manager.SetupGet(m => m.IsTenantLoaded).Returns(() => yuklu);
        manager.Setup(m => m.ClearCurrentTenant()).Callback(() => yuklu = false);
        var sistemLog = new Mock<ISistemLogService>();
        var logSvc = new Mock<ILogService>();
        logSvc.SetupGet(l => l.SistemLogService).Returns(sistemLog.Object);

        var service = new TenantSQLiteSelectionService(
            manager.Object,
            new Mock<IMessageService>().Object,
            logSvc.Object,
            new Mock<ITenantSQLiteDatabaseManager>().Object,
            new Mock<ITenantSQLiteConnectionStringFactory>().Object);

        var sonuc = await service.DisconnectCurrentTenantAsync();

        sonuc.Success.Should().BeTrue();
        manager.Verify(m => m.ClearCurrentTenant(), Times.Once);
    }

    #endregion

    #region TenantSQLiteDatabaseSelectedDetailService

    private static TenantSQLiteDatabaseSelectedDetailService BuildDetailService(
        Mock<IMaliDonemService> donemSvc,
        Mock<IApplicationPaths> paths)
    {
        var sistemLog = new Mock<ISistemLogService>();
        var logSvc = new Mock<ILogService>();
        logSvc.SetupGet(l => l.SistemLogService).Returns(sistemLog.Object);
        return new TenantSQLiteDatabaseSelectedDetailService(
            donemSvc.Object, logSvc.Object, new Mock<IFirmaService>().Object, paths.Object);
    }

    private static MaliDonemModel TestDonem() => new()
    {
        Id = 42,
        FirmaId = 7,
        MaliYil = 2027,
        DatabaseName = "db-acme_2027",
        KaydedenId = 3,
        FirmaModel = new FirmaModel { Id = 7, FirmaKodu = "ACME", KisaUnvani = "Acme A.Ş." }
    };

    [Fact]
    public async Task GetTenantDetailsAsync_GecersizId_Hata()
    {
        var service = BuildDetailService(new Mock<IMaliDonemService>(), BuildPathsMock(true));
        var sonuc = await service.GetTenantDetailsAsync(0);
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("Mali Dönem ID boş veya geçersiz");
    }

    [Fact]
    public async Task GetTenantDetailsAsync_DonemDonmez_Hata()
    {
        var donemSvc = new Mock<IMaliDonemService>();
        donemSvc.Setup(s => s.GetByMaliDonemIdAsync(42))
            .ReturnsAsync(new ErrorApiDataResponse<MaliDonemModel>(null!, "Mali dönem bulunamadı"));

        var service = BuildDetailService(donemSvc, BuildPathsMock(true));
        var sonuc = await service.GetTenantDetailsAsync(42);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Be("Mali dönem bulunamadı");
    }

    [Fact]
    public async Task GetTenantDetailsAsync_SuccessFalseDataDolu_Regresyon_HataVerilmeli()
    {
        // Bug #1 regresyonu: Success=false + Data!=null guard'i ATLAMAMALI
        var donemSvc = new Mock<IMaliDonemService>();
        donemSvc.Setup(s => s.GetByMaliDonemIdAsync(42))
            .ReturnsAsync(new ErrorApiDataResponse<MaliDonemModel>(TestDonem(), "kısmi veri hatası"));

        var service = BuildDetailService(donemSvc, BuildPathsMock(true));
        var sonuc = await service.GetTenantDetailsAsync(42);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Be("kısmi veri hatası");
    }

    [Fact]
    public async Task GetTenantDetailsAsync_DbDosyasiYok_Hata()
    {
        var donemSvc = new Mock<IMaliDonemService>();
        donemSvc.Setup(s => s.GetByMaliDonemIdAsync(42))
            .ReturnsAsync(new SuccessApiDataResponse<MaliDonemModel>(TestDonem(), "ok"));

        var service = BuildDetailService(donemSvc, BuildPathsMock(false));
        var sonuc = await service.GetTenantDetailsAsync(42);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Be("Veritabanı bulunamadı");
    }

    [Fact]
    public async Task GetTenantDetailsAsync_Basari_Esleme()
    {
        var donemSvc = new Mock<IMaliDonemService>();
        donemSvc.Setup(s => s.GetByMaliDonemIdAsync(42))
            .ReturnsAsync(new SuccessApiDataResponse<MaliDonemModel>(TestDonem(), "ok"));

        var service = BuildDetailService(donemSvc, BuildPathsMock(true));
        var sonuc = await service.GetTenantDetailsAsync(42);

        sonuc.Success.Should().BeTrue();
        sonuc.Data!.MaliDonemId.Should().Be(42);
        sonuc.Data.DatabaseName.Should().Be("db-acme_2027");
        sonuc.Data.MaliYil.Should().Be(2027);
        sonuc.Data.UserId.Should().Be(3);
        sonuc.Data.FirmaId.Should().Be(7);
        sonuc.Data.FirmaKodu.Should().Be("ACME");
        sonuc.Data.FirmaKisaUnvan.Should().Be("Acme A.Ş.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task GetUserTenantsForSelectionAsync_GecersizUserId_Hata(long userId)
    {
        var service = BuildDetailService(new Mock<IMaliDonemService>(), BuildPathsMock(true));
        var sonuc = await service.GetUserTenantsForSelectionAsync(new DataRequest<Firma>(), userId);
        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("Kullanıcı ID boş veya geçersiz");
    }

    [Fact]
    public async Task GetUserTenantsForSelectionAsync_FirmaYok_BosListeBasari()
    {
        var donemSvc = new Mock<IMaliDonemService>();
        var sistemLog = new Mock<ISistemLogService>();
        var logSvc = new Mock<ILogService>();
        logSvc.SetupGet(l => l.SistemLogService).Returns(sistemLog.Object);
        var firmaSvc = new Mock<IFirmaService>();
        firmaSvc.Setup(s => s.GetFirmalarWithUserId(It.IsAny<DataRequest<Firma>>(), 3))
            .ReturnsAsync(new SuccessApiDataResponse<IList<FirmaModel>>(new List<FirmaModel>(), "boş"));

        var service = new TenantSQLiteDatabaseSelectedDetailService(
            donemSvc.Object, logSvc.Object, firmaSvc.Object, BuildPathsMock(true).Object);

        var sonuc = await service.GetUserTenantsForSelectionAsync(new DataRequest<Firma>(), 3);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().BeEmpty();
        sonuc.Message.Should().Contain("firması bulunamadı");
    }

    [Fact]
    public async Task GetUserTenantsForSelectionAsync_Basari_YilaGre_azalanVeFiltreli()
    {
        var donem2026 = new MaliDonemModel { Id = 1, FirmaId = 7, MaliYil = 2026, DatabaseName = "db-acme_2026", KaydedenId = 3 };
        var donem2027 = new MaliDonemModel { Id = 2, FirmaId = 7, MaliYil = 2027, DatabaseName = "db-acme_2027", KaydedenId = 3 };
        var donemBosDb = new MaliDonemModel { Id = 3, FirmaId = 7, MaliYil = 2028, DatabaseName = "  ", KaydedenId = 3 };
        var donemBaskasina = new MaliDonemModel { Id = 4, FirmaId = 7, MaliYil = 2025, DatabaseName = "db-acme_2025", KaydedenId = 99 };
        var firma = new FirmaModel
        {
            Id = 7,
            FirmaKodu = "ACME",
            KisaUnvani = "Acme A.Ş.",
            MaliDonemler = new List<MaliDonemModel> { donem2026, donem2027, donemBosDb, donemBaskasina }
        };

        var firmaSvc = new Mock<IFirmaService>();
        firmaSvc.Setup(s => s.GetFirmalarWithUserId(It.IsAny<DataRequest<Firma>>(), 3))
            .ReturnsAsync(new SuccessApiDataResponse<IList<FirmaModel>>(new List<FirmaModel> { firma }, "ok"));
        var sistemLog = new Mock<ISistemLogService>();
        var logSvc = new Mock<ILogService>();
        logSvc.SetupGet(l => l.SistemLogService).Returns(sistemLog.Object);

        var service = new TenantSQLiteDatabaseSelectedDetailService(
            new Mock<IMaliDonemService>().Object, logSvc.Object, firmaSvc.Object, BuildPathsMock(true).Object);

        var sonuc = await service.GetUserTenantsForSelectionAsync(new DataRequest<Firma>(), 3);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().HaveCount(2, "boş DatabaseName ve başka kullanıcıya ait dönemler elenir");
        sonuc.Data!.Select(t => t.MaliYil).Should().ContainInOrder(2027, 2026);
        sonuc.Data[0].FirmaKodu.Should().Be("ACME");
    }

    #endregion
}
