using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using System.Diagnostics;

namespace MuhasibPro.Tests;

/// <summary>Madde 2 (B2 raporu): yönetim sayfasında bayat dönem seçilince "uyarıdan
/// aksiyona" veri yolu çalışır — SelectedDonem analizi RequiredUpdating üretir,
/// `DbGuncellemeGerekliMi` true olur (DonemOzetCard'taki uyarı barı + "Veritabanını
/// Güncelle" butonu bu koşula bağlıdır; yönlendirme `OnGuncelleClick` → UpdateView).</summary>
public class YonetimGuncelleAksiyonTests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        return ortak;
    }

    private static MaliDonemYonetimViewModel SayfaKur()
    {
        var tenant = new Mock<ITenantSQLiteDatabaseService>();
        tenant.Setup(s => s.GetTenantDatabaseStateAsync(It.IsAny<string>()))
            .ReturnsAsync((string db) => new SuccessApiDataResponse<DatabaseConnectionAnalysis>(
                new DatabaseConnectionAnalysis
                {
                    DatabaseName = db,
                    IsDatabaseExists = true,
                    CanConnect = true,
                    DatabaseValid = true,
                    CurrentVersion = "1.1.28.1709",
                    PendingMigrations = new List<string> { "20260908_AddTenantIdentity" },
                    DatabaseFileSizeBytes = 2048
                }, "ok"));
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync(It.IsAny<string>())).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        return new MaliDonemYonetimViewModel(
            OrtakServisler().Object, Mock.Of<IMaliDonemService>(), tenant.Object,
            operasyon.Object, Mock.Of<ITenantBackupService>(), null!);
    }

    private static async Task<bool> BekleAsync(Func<bool> kosul, int zamanAsimiMs = 15000)
    {
        var kronometre = Stopwatch.StartNew();
        while (kronometre.ElapsedMilliseconds < zamanAsimiMs)
        {
            if (kosul()) return true;
            await Task.Delay(50);
        }
        return kosul();
    }

    [Fact]
    public async Task BayatDonemSecilince_GuncellemeKosulu_DogruOlur()
    {
        var vm = SayfaKur();
        var donem = new MaliDonemModel
        {
            Id = 2,
            FirmaId = 7,
            MaliYil = 2027,
            DatabaseName = "db-F-0001_2027",
            AktifMi = true
        };

        vm.SelectedDonem = donem;

        (await BekleAsync(() => donem.DbAnalizYapildi))
            .Should().BeTrue("seçim DonemDegistiAsync → analiz tetiklemeli");
        donem.DbGuncellemeGerekliMi.Should().BeTrue(
            "bayat analizde uyarı barı + Güncelle butonu koşulu (SelectedDonem.DbGuncellemeGerekliMi) true olmalı");
        donem.DbBekleyenGuncellemeSayisi.Should().Be(1);
    }
}
