using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Data.Database.Common.Helpers;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Tests;

public class TenantDatabaseUpdateTests
{
    #region TenantMigrationDescriber (gerçek migration yapısı)

    [Fact]
    public void Describe_AddTenantIdentity_BesKolonGercekYapi()
    {
        var describer = new TenantMigrationDescriber();

        var desc = describer.Describe("20260908_AddTenantIdentity");

        desc.IsKnown.Should().BeTrue();
        desc.Details.Should().HaveCount(5);
        desc.Details.Should().Contain(l => l.Contains("FirmaId") && l.Contains("TenantDatabaseVersiyonlar"));
        desc.Details.Should().Contain(l => l.Contains("KurulumId"));
        desc.Summary.Should().Contain("5 kolon");
        desc.Summary.Should().Contain("TenantDatabaseVersiyonlar");
    }

    [Fact]
    public void Describe_InitialApp_IkiTablo()
    {
        var describer = new TenantMigrationDescriber();

        var desc = describer.Describe("20260828170927_InitialApp");

        desc.IsKnown.Should().BeTrue();
        desc.Summary.Should().Contain("2 tablo");
        desc.Summary.Should().Contain("AppLogs");
    }

    [Fact]
    public void Describe_Bilinmeyen_SemaGocuFallback()
    {
        var describer = new TenantMigrationDescriber();

        var desc = describer.Describe("20990101_HayaliGoc");

        desc.IsKnown.Should().BeFalse();
        desc.Summary.Should().Be("Şema Değişikliği");
        desc.Details.Should().BeEmpty();
    }

    [Fact]
    public void Describe_BosId_Bilinmeyen()
    {
        var describer = new TenantMigrationDescriber();

        describer.Describe(null!).IsKnown.Should().BeFalse();
        describer.Describe("").IsKnown.Should().BeFalse();
    }

    [Fact]
    public void Describe_AddTenantIdentity_TabloGuncellemesi()
    {
        var desc = new TenantMigrationDescriber().Describe("20260908_AddTenantIdentity");

        desc.ChangeKind.Should().Be("Tablo güncellemesi");
        desc.Headline.Should().Be("'TenantDatabaseVersiyonlar' tablosu güncellemesi");
        desc.Tables.Should().ContainSingle()
            .Which.AddedColumns.Should().BeEquivalentTo("FirmaId", "MaliDonemId", "OlusturanKullaniciId", "MakineId", "KurulumId");
    }

    [Fact]
    public void Describe_InitialApp_TabloGruplari()
    {
        var desc = new TenantMigrationDescriber().Describe("20260828170927_InitialApp");

        desc.ChangeKind.Should().Be("Tablo güncellemesi");
        desc.Headline.Should().Be("Tablo güncellemesi (2 tablo)");
        desc.Tables.Should().HaveCount(2);
    }

    #endregion

    #region DbSchemaVersions (migration → SemVer eşlemesi)

    [Fact]
    public void SurumTenantIdentity_BirBirSifir()
    {
        MuhasibPro.Data.Database.Common.Helpers.DbSchemaVersions
            .ForMigration("20260908_AddTenantIdentity").Should().Be("1.1.0");
    }

    [Fact]
    public void SurumInitialApp_BirSifirSifir()
    {
        MuhasibPro.Data.Database.Common.Helpers.DbSchemaVersions
            .ForMigration("20260828170927_InitialApp").Should().Be("1.0.0");
    }

    [Fact]
    public void SurumBilinmeyen_IlkSemayaDuser()
    {
        MuhasibPro.Data.Database.Common.Helpers.DbSchemaVersions
            .ForMigration("20990101_GelecektenGoc").Should().Be("1.0.0");
        MuhasibPro.Data.Database.Common.Helpers.DbSchemaVersions
            .ForMigration(null).Should().Be("1.0.0");
    }

    [Fact]
    public void SurumSiralamasi_EskiYenidenKucuk()
    {
        var eski = MuhasibPro.Data.Database.Common.Helpers.DbSchemaVersions
            .ForMigration("20260828170927_InitialApp");
        var yeni = MuhasibPro.Data.Database.Common.Helpers.DbSchemaVersions
            .ForMigration("20260908_AddTenantIdentity");

        MuhasibPro.Domain.Utilities.SemanticVersion.IsGreater(yeni, eski).Should().BeTrue();
    }

    #endregion

    #region TenantDatabaseUpdateService

    private static (Mock<ITenantSQLiteDatabaseService> tenant, Mock<IFirmaWithMaliDonemSelectedService> selected, Mock<ITenantMigrationDescriber> describer) Mocks()
    {
        var tenant = new Mock<ITenantSQLiteDatabaseService>();
        var selected = new Mock<IFirmaWithMaliDonemSelectedService>();
        var describer = new Mock<ITenantMigrationDescriber>();
        describer.Setup(d => d.Describe(It.IsAny<string>())).Returns((string id) => new TenantMigrationDescription
        {
            MigrationId = id,
            IsKnown = true,
            Summary = "özet",
            Details = new List<string> { "Detay satırı" }
        });
        return (tenant, selected, describer);
    }

    private static DatabaseConnectionAnalysis UpdateGerekenDurum() => new()
    {
        CurrentVersion = "1.0.0.0",
        Message = "Güncelleme var",
        DatabaseValid = true,
        PendingMigrations = new List<string> { "20260908_AddTenantIdentity" }
    };

    [Fact]
    public async Task CheckUpdateRequired_GuncellemeGerekli_OnayMetniGercekDetayli()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseConnectionAnalysis>(UpdateGerekenDurum(), "ok"));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);

        var check = await svc.CheckUpdateRequiredAsync("db-TEST_2027");

        check.CheckSucceeded.Should().BeTrue();
        check.NeedsUpdate.Should().BeTrue();
        check.PendingCount.Should().Be(1);
        check.ConfirmMessage.Should().Contain("Detay satırı");
        check.ConfirmMessage.Should().Contain("Mevcut dönem verileri");
        check.ConfirmMessage.Should().NotContain("Admin enum sabit");
        check.PendingSummaries.Should().ContainSingle().Which.Should().Be("özet");
    }

    [Fact]
    public async Task CheckUpdateRequired_GercekYapi_HeadlineVeTablolar()
    {
        var tenant = new Mock<ITenantSQLiteDatabaseService>();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseConnectionAnalysis>(UpdateGerekenDurum(), "ok"));
        var selected = new Mock<IFirmaWithMaliDonemSelectedService>();
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, new TenantMigrationDescriber());

        var check = await svc.CheckUpdateRequiredAsync("db-TEST_2027");

        check.Headline.Should().Be("'TenantDatabaseVersiyonlar' tablosu güncellemesi");
        check.TableChanges.Should().ContainSingle()
            .Which.AddedColumns.Should().Contain("FirmaId");
    }

    [Fact]
    public async Task CheckUpdateRequired_Guncel_OnayYok()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseConnectionAnalysis>(new DatabaseConnectionAnalysis
            {
                CurrentVersion = "2.0",
                DatabaseValid = true,
                PendingMigrations = new List<string>()
            }, "ok"));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);

        var check = await svc.CheckUpdateRequiredAsync("db-TEST_2027");

        check.CheckSucceeded.Should().BeTrue();
        check.NeedsUpdate.Should().BeFalse();
        check.ConfirmMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckUpdateRequired_DurumYok_DogrudanBaglanti()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new ErrorApiDataResponse<DatabaseConnectionAnalysis>(null!, "okunamadı"));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);

        var check = await svc.CheckUpdateRequiredAsync("db-TEST_2027");

        check.CheckSucceeded.Should().BeFalse();
        check.NeedsUpdate.Should().BeFalse();
    }

    [Fact]
    public async Task CheckUpdateRequired_ServisAtarsa_YutmazDevam()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("kesinti"));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);

        var check = await svc.CheckUpdateRequiredAsync("db-TEST_2027");

        check.CheckSucceeded.Should().BeFalse();
        check.NeedsUpdate.Should().BeFalse();
    }

    [Fact]
    public async Task SwitchAndPublish_Basarili_DurumuYayinlar()
    {
        var (tenant, selected, describer) = Mocks();
        var ctx = new TenantContext { DatabaseName = "db-TEST_2027" };
        tenant.Setup(t => t.SwitchTenantAsync("db-TEST_2027"))
            .ReturnsAsync(new SuccessApiDataResponse<TenantContext>(ctx, "bağlandı"));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);
        var firma = new FirmaModel();
        var donem = new MaliDonemModel();

        var result = await svc.SwitchAndPublishAsync("db-TEST_2027", firma, donem);

        result.Success.Should().BeTrue();
        result.ConnectionMessage.Should().Be("bağlandı");
        selected.VerifySet(s => s.SelectedFirma = firma);
        selected.VerifySet(s => s.SelectedMaliDonem = donem);
        selected.VerifySet(s => s.ConnectedTenantDb = ctx);
    }

    [Fact]
    public async Task SwitchAndPublish_Hatali_YayinYapmazVarsayilanMesaj()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.SwitchTenantAsync("db-TEST_2027"))
            .ReturnsAsync(new ErrorApiDataResponse<TenantContext>(null!, null!));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);

        var result = await svc.SwitchAndPublishAsync("db-TEST_2027", new FirmaModel(), new MaliDonemModel());

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Bağlantı kurulamadı");
        selected.VerifySet(s => s.SelectedFirma = It.IsAny<FirmaModel>(), Times.Never);
    }

    #endregion

    #region TenantDatabaseUpdateCoordinator

    private static (Mock<MuhasibPro.Business.Contracts.UIServices.CommonServices.ICommonServices> common, Mock<MuhasibPro.Business.Contracts.UIServices.CommonServices.IDialogService> dialogs, Mock<MuhasibPro.Business.Contracts.UIServices.CommonServices.INavigationService> nav, Mock<ITenantDatabaseUpdateService> update) CoordinatorMocks(TenantUpdateDecision karar)
    {
        var dialogs = new Mock<MuhasibPro.Business.Contracts.UIServices.CommonServices.IDialogService>();
        dialogs.Setup(d => d.ShowTenantUpdateConfirmAsync(It.IsAny<TenantUpdateCheckResult>())).ReturnsAsync(karar);
        var nav = new Mock<MuhasibPro.Business.Contracts.UIServices.CommonServices.INavigationService>();
        nav.Setup(n => n.CreateNewViewAsync<MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateViewModel>(It.IsAny<object>(), It.IsAny<string>())).ReturnsAsync(1);
        var common = new Mock<MuhasibPro.Business.Contracts.UIServices.CommonServices.ICommonServices>();
        common.SetupGet(c => c.DialogService).Returns(dialogs.Object);
        common.SetupGet(c => c.NavigationService).Returns(nav.Object);
        return (common, dialogs, nav, new Mock<ITenantDatabaseUpdateService>());
    }

    private static TenantUpdateCheckResult GuncellemeGerekenKontrol() => new()
    {
        DatabaseName = "db-TEST_2027",
        CheckSucceeded = true,
        NeedsUpdate = true,
        PendingCount = 1
    };

    [Fact]
    public async Task Coordinator_GuncellemeYoksa_DogrudanSwitch()
    {
        var (common, dialogs, nav, update) = CoordinatorMocks(TenantUpdateDecision.Vazgec);
        update.Setup(u => u.CheckUpdateRequiredAsync("db-TEST_2027")).ReturnsAsync(new TenantUpdateCheckResult { CheckSucceeded = true, NeedsUpdate = false });
        update.Setup(u => u.SwitchAndPublishAsync("db-TEST_2027", It.IsAny<FirmaModel>(), It.IsAny<MaliDonemModel>()))
            .ReturnsAsync(new TenantUpdateSwitchResult { Success = true, ConnectionMessage = "bağlandı" });
        var progress = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantUpdateProgressViewModel();
        var coordinator = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateCoordinator(common.Object, update.Object, progress);

        await coordinator.EnsureSwitchedAsync("db-TEST_2027", new FirmaModel(), new MaliDonemModel());

        update.Verify(u => u.SwitchAndPublishAsync("db-TEST_2027", It.IsAny<FirmaModel>(), It.IsAny<MaliDonemModel>()), Times.Once);
        dialogs.Verify(d => d.ShowTenantUpdateConfirmAsync(It.IsAny<TenantUpdateCheckResult>()), Times.Never);
        progress.IsUpdating.Should().BeFalse();
    }

    [Fact]
    public async Task Coordinator_SimdiGuncelle_SayfayaYonlendirSwitchYok()
    {
        var (common, dialogs, nav, update) = CoordinatorMocks(TenantUpdateDecision.SimdiGuncelle);
        update.Setup(u => u.CheckUpdateRequiredAsync("db-TEST_2027")).ReturnsAsync(GuncellemeGerekenKontrol());
        var progress = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantUpdateProgressViewModel();
        var coordinator = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateCoordinator(common.Object, update.Object, progress);

        await coordinator.EnsureSwitchedAsync("db-TEST_2027", new FirmaModel(), new MaliDonemModel());

        nav.Verify(n => n.CreateNewViewAsync<MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateViewModel>(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        update.Verify(u => u.SwitchAndPublishAsync(It.IsAny<string>(), It.IsAny<FirmaModel>(), It.IsAny<MaliDonemModel>()), Times.Never);
        progress.IsUpdating.Should().BeFalse();
    }

    [Fact]
    public async Task Coordinator_DahaSonra_Kalis()
    {
        var (common, dialogs, nav, update) = CoordinatorMocks(TenantUpdateDecision.DahaSonra);
        update.Setup(u => u.CheckUpdateRequiredAsync("db-TEST_2027")).ReturnsAsync(GuncellemeGerekenKontrol());
        var progress = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantUpdateProgressViewModel();
        var coordinator = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateCoordinator(common.Object, update.Object, progress);

        await coordinator.EnsureSwitchedAsync("db-TEST_2027", new FirmaModel(), new MaliDonemModel());

        nav.Verify(n => n.CreateNewViewAsync<MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateViewModel>(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        update.Verify(u => u.SwitchAndPublishAsync(It.IsAny<string>(), It.IsAny<FirmaModel>(), It.IsAny<MaliDonemModel>()), Times.Never);
        progress.IsUpdating.Should().BeFalse();
    }

    [Fact]
    public async Task Coordinator_Vazgec_Kalis()
    {
        var (common, dialogs, nav, update) = CoordinatorMocks(TenantUpdateDecision.Vazgec);
        update.Setup(u => u.CheckUpdateRequiredAsync("db-TEST_2027")).ReturnsAsync(GuncellemeGerekenKontrol());
        var progress = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantUpdateProgressViewModel();
        var coordinator = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateCoordinator(common.Object, update.Object, progress);

        await coordinator.EnsureSwitchedAsync("db-TEST_2027", new FirmaModel(), new MaliDonemModel());

        nav.Verify(n => n.CreateNewViewAsync<MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateViewModel>(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        update.Verify(u => u.SwitchAndPublishAsync(It.IsAny<string>(), It.IsAny<FirmaModel>(), It.IsAny<MaliDonemModel>()), Times.Never);
    }

    [Fact]
    public async Task Coordinator_DogrudanSwitchHatali_HataDialogu()
    {
        var (common, dialogs, nav, update) = CoordinatorMocks(TenantUpdateDecision.Vazgec);
        update.Setup(u => u.CheckUpdateRequiredAsync("db-TEST_2027")).ReturnsAsync(new TenantUpdateCheckResult { CheckSucceeded = false });
        update.Setup(u => u.SwitchAndPublishAsync("db-TEST_2027", It.IsAny<FirmaModel>(), It.IsAny<MaliDonemModel>()))
            .ReturnsAsync(new TenantUpdateSwitchResult { Success = false, ErrorMessage = "bağlanamadı" });
        var progress = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantUpdateProgressViewModel();
        var coordinator = new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateCoordinator(common.Object, update.Object, progress);

        await coordinator.EnsureSwitchedAsync("db-TEST_2027", new FirmaModel(), new MaliDonemModel());

        dialogs.Verify(d => d.ShowAsync("Güncelleme Hatası", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region ValidateAsync

    [Fact]
    public async Task Validate_Saglikli_True()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseConnectionAnalysis>(new DatabaseConnectionAnalysis
            {
                CanConnect = true,
                DatabaseValid = true,
                PendingMigrations = new List<string>()
            }, "ok"));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);

        (await svc.ValidateAsync("db-TEST_2027")).Should().BeTrue();
    }

    [Fact]
    public async Task Validate_BekleyenGoc_False()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseConnectionAnalysis>(UpdateGerekenDurum(), "ok"));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);

        (await svc.ValidateAsync("db-TEST_2027")).Should().BeFalse();
    }

    [Fact]
    public async Task Validate_DurumYok_False()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new ErrorApiDataResponse<DatabaseConnectionAnalysis>(null!, "okunamadı"));
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object);

        (await svc.ValidateAsync("db-TEST_2027")).Should().BeFalse();
    }

    #endregion

    #region UpdatePageSaga

    private static (Mock<ITenantDatabaseUpdateService> update, Mock<ITenantSQLiteDatabaseOperationService> ops) PageMocks(
        bool validateFirst = true, bool validateAfterRestore = true)
    {
        var update = new Mock<ITenantDatabaseUpdateService>();
        update.Setup(u => u.CheckUpdateRequiredAsync("db-TEST_2027")).ReturnsAsync(new TenantUpdateCheckResult
        {
            DatabaseName = "db-TEST_2027",
            CheckSucceeded = true,
            NeedsUpdate = true,
            CurrentVersion = "1.0",
            TargetVersion = "2.0",
            PendingCount = 1,
            PendingSummaries = new List<string> { "özet" },
            UpdateDetailLines = new List<string> { "detay" }
        });
        var ctx = new TenantContext { DatabaseName = "db-TEST_2027" };
        update.Setup(u => u.SwitchAndPublishAsync("db-TEST_2027", It.IsAny<FirmaModel>(), It.IsAny<MaliDonemModel>()))
            .ReturnsAsync(new TenantUpdateSwitchResult { Success = true });
        var validates = new Queue<bool>(new[] { validateFirst, validateAfterRestore });
        update.Setup(u => u.ValidateAsync("db-TEST_2027")).ReturnsAsync(() => validates.Count > 0 ? validates.Dequeue() : true);

        var ops = new Mock<ITenantSQLiteDatabaseOperationService>();
        ops.Setup(o => o.CreateBackupAsync("db-TEST_2027", MuhasibPro.Domain.Enum.DatabaseEnum.DatabaseBackupType.Migration))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseBackupResult>(new DatabaseBackupResult
            {
                IsBackupComleted = true,
                BackupFilePath = "C:\\yedek.backup",
                BackupFileName = "yedek.backup"
            }, "ok"));
        ops.Setup(o => o.RestoreBackupAsync("db-TEST_2027", It.IsAny<string>()))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseRestoreExecutionResult>(
                new DatabaseRestoreExecutionResult { IsRestoreSuccess = true }, "ok"));
        return (update, ops);
    }

    private static MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateViewModel PageVm(
        Mock<ITenantDatabaseUpdateService> update, Mock<ITenantSQLiteDatabaseOperationService> ops)
    {
        var common = new Mock<MuhasibPro.Business.Contracts.UIServices.CommonServices.ICommonServices>();
        var settings = new Mock<MuhasibPro.Business.Contracts.UIServices.ILocalSettingsService>();
        var selected = new Mock<IFirmaWithMaliDonemSelectedService>();
        return new MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateViewModel(
            common.Object, update.Object, ops.Object, settings.Object, selected.Object);
    }

    private static MuhasibPro.ViewModels.ViewModels.Shell.Tenant.TenantDatabaseUpdateArgs PageArgs() => new()
    {
        DatabaseName = "db-TEST_2027",
        Firma = new FirmaModel(),
        MaliDonem = new MaliDonemModel()
    };

    private static async Task BekleAsync(Func<bool> bitis)
    {
        for (var i = 0; i < 100 && !bitis(); i++)
            await Task.Delay(20);
    }

    [Fact]
    public async Task Saga_Basarili_Tamamlanir()
    {
        var (update, ops) = PageMocks();
        var vm = PageVm(update, ops);
        await vm.LoadAsync(PageArgs());

        vm.StartUpdateCommand.Execute(null);
        await BekleAsync(() => !vm.IsRunning);

        vm.IsCompleted.Should().BeTrue();
        vm.HasError.Should().BeFalse();
        vm.Steps.Should().OnlyContain(s => s.IsDone);
        ops.Verify(o => o.RestoreBackupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Saga_DogrulamaBasarisiz_OtomatikGeriAlir()
    {
        var (update, ops) = PageMocks(validateFirst: false, validateAfterRestore: true);
        var vm = PageVm(update, ops);
        await vm.LoadAsync(PageArgs());

        vm.StartUpdateCommand.Execute(null);
        await BekleAsync(() => !vm.IsRunning);

        vm.IsCompleted.Should().BeTrue();
        vm.RestoredFromBackup.Should().BeTrue();
        ops.Verify(o => o.RestoreBackupAsync("db-TEST_2027", "C:\\yedek.backup"), Times.Once);
    }

    [Fact]
    public async Task Saga_GeriAlmaBasarisiz_Hata()
    {
        var (update, ops) = PageMocks(validateFirst: false, validateAfterRestore: false);
        ops.Setup(o => o.RestoreBackupAsync("db-TEST_2027", It.IsAny<string>()))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseRestoreExecutionResult>(
                new DatabaseRestoreExecutionResult { IsRestoreSuccess = false, Message = "yazma hatası" }, "hata"));
        var vm = PageVm(update, ops);
        await vm.LoadAsync(PageArgs());

        vm.StartUpdateCommand.Execute(null);
        await BekleAsync(() => !vm.IsRunning);

        vm.IsCompleted.Should().BeFalse();
        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("C:\\yedek.backup");
    }

    [Fact]
    public async Task Saga_YedekBasarisiz_SwitchCagrilmaz()
    {
        var (update, ops) = PageMocks();
        ops.Setup(o => o.CreateBackupAsync("db-TEST_2027", MuhasibPro.Domain.Enum.DatabaseEnum.DatabaseBackupType.Migration))
            .ReturnsAsync(new ErrorApiDataResponse<DatabaseBackupResult>(null!, "disk dolu"));
        var vm = PageVm(update, ops);
        await vm.LoadAsync(PageArgs());

        vm.StartUpdateCommand.Execute(null);
        await BekleAsync(() => !vm.IsRunning);

        update.Verify(u => u.SwitchAndPublishAsync(It.IsAny<string>(), It.IsAny<FirmaModel>(), It.IsAny<MaliDonemModel>()), Times.Never);
        vm.HasError.Should().BeTrue();
    }

    #endregion

    #region E1 UpdateAvailable yayını

    [Fact]
    public async Task CheckUpdateRequired_GuncellemeVarken_OlayiYayinlar()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseConnectionAnalysis>(UpdateGerekenDurum(), "ok"));
        var bus = new Mock<IEventBus>();
        TenantUpdateAvailableEvent yakalanan = null!;
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<TenantUpdateAvailableEvent>()))
            .Callback<object, TenantUpdateAvailableEvent>((s, e) => yakalanan = e);
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object, bus.Object);

        var check = await svc.CheckUpdateRequiredAsync("db-TEST_2027");

        check.NeedsUpdate.Should().BeTrue();
        yakalanan.Should().NotBeNull();
        yakalanan.DatabaseName.Should().Be("db-TEST_2027");
        yakalanan.FromVersion.Should().Be(check.CurrentVersion);
        yakalanan.ToVersion.Should().Be(check.TargetVersion);
    }

    [Fact]
    public async Task CheckUpdateRequired_Guncelken_OlayYayinlanmaz()
    {
        var (tenant, selected, describer) = Mocks();
        tenant.Setup(t => t.GetTenantDatabaseStateAsync("db-TEST_2027"))
            .ReturnsAsync(new SuccessApiDataResponse<DatabaseConnectionAnalysis>(new DatabaseConnectionAnalysis
            {
                CurrentVersion = "2.0",
                DatabaseValid = true,
                PendingMigrations = new List<string>()
            }, "ok"));
        var bus = new Mock<IEventBus>();
        var svc = new TenantDatabaseUpdateService(tenant.Object, selected.Object, describer.Object, bus.Object);

        var check = await svc.CheckUpdateRequiredAsync("db-TEST_2027");

        check.NeedsUpdate.Should().BeFalse();
        bus.Verify(b => b.Publish(It.IsAny<object>(), It.IsAny<TenantUpdateAvailableEvent>()), Times.Never);
    }

    #endregion
}
