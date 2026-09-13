using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using System.Diagnostics;

namespace MuhasibPro.Tests;

/// <summary>B1-A regresyonu: firma seçiminde (liste yüklemede) YALNIZ seçili kart değil
/// TÜM dönemler analizlenir; her kart kendi DB Durumu rozetini alır. E1 abonesi
/// DatabaseName'i büyük/küçük harf duyarsız eşleştirir.</summary>
public class MaliDonemTopluAnalizTests
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

    private static MaliDonemModel Donem(long id, int yil, string db) => new()
    {
        Id = id,
        FirmaId = 7,
        MaliYil = yil,
        DatabaseName = db,
        AktifMi = true
    };

    private static DatabaseConnectionAnalysis Analiz(string db, bool guncel) => new()
    {
        DatabaseName = db,
        IsDatabaseExists = true,
        CanConnect = true,
        DatabaseValid = true,
        CurrentVersion = guncel ? "1.1.0" : "1.1.28.1709",
        PendingMigrations = guncel ? new List<string>() : new List<string> { "20260908_AddTenantIdentity" },
        DatabaseFileSizeBytes = 1024
    };

    private static Mock<IMaliDonemService> DonemServisi(List<MaliDonemModel> liste)
    {
        var svc = new Mock<IMaliDonemService>();
        svc.Setup(s => s.GetMaliDonemlerCountAsync(It.IsAny<DataRequest<MaliDonem>>()))
            .ReturnsAsync(new SuccessApiDataResponse<int>(liste.Count, "ok"));
        svc.Setup(s => s.GetMaliDonemlerWithFirmaId(It.IsAny<DataRequest<MaliDonem>>(), It.IsAny<long>()))
            .ReturnsAsync(new SuccessApiDataResponse<IList<MaliDonemModel>>(liste, "ok"));
        // Backfill atlanır: satırda boyut dolu varsayılır (yazım yolu bu testin dışı).
        svc.Setup(s => s.GetByMaliDonemIdAsync(It.IsAny<long>()))
            .ReturnsAsync((long id) =>
            {
                var satir = Donem(id, 2026, "db");
                satir.TenantDetails = new TenantDetailsModel { DosyaBoyutu = 1024 };
                return new SuccessApiDataResponse<MaliDonemModel>(satir, "ok");
            });
        return svc;
    }

    private static Mock<ITenantSQLiteDatabaseService> TenantServisi() =>
        TenantServisi(_ => true);

    private static Mock<ITenantSQLiteDatabaseService> TenantServisi(Func<string, bool> guncelMi)
    {
        var svc = new Mock<ITenantSQLiteDatabaseService>();
        svc.Setup(s => s.GetTenantDatabaseStateAsync(It.IsAny<string>()))
            .ReturnsAsync((string db) => new SuccessApiDataResponse<DatabaseConnectionAnalysis>(
                Analiz(db, guncelMi(db)), "ok"));
        return svc;
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
    public async Task Liste_Yuklenince_TumDonemler_Analizlenir_HerKartKendiRozetiniAlir()
    {
        var liste = new List<MaliDonemModel>
        {
            Donem(1, 2026, "db-F-0001_2026"),
            Donem(2, 2027, "db-F-0001_2027")
        };
        var vm = new MaliDonemListViewModel(
            OrtakServisler().Object, DonemServisi(liste).Object,
            TenantServisi(db => !db.EndsWith("2027")).Object, null!);

        await vm.LoadAsync(new MaliDonemListArgs { FirmaId = 7 }, silent: true);

        (await BekleAsync(() => liste.All(m => m.DbAnalizYapildi)))
            .Should().BeTrue("fire-and-forget toplu analiz iki kartı da işlemeli");

        var guncel = liste[0];
        guncel.DbDurum.Should().Be(DatabaseStatusResult.Healty);
        guncel.DbGuncelMi.Should().BeTrue();

        var eski = liste[1];
        eski.DbDurum.Should().Be(DatabaseStatusResult.RequiredUpdating);
        eski.DbGuncellemeGerekliMi.Should().BeTrue();
        eski.DbBekleyenGuncellemeSayisi.Should().Be(1);
    }

    [Fact]
    public async Task E1_Olay_DatabaseName_FarkliHarfle_Gelse_Dahi_RozetDuser()
    {
        var liste = new List<MaliDonemModel> { Donem(2, 2027, "db-F-0001_2027") };
        var bus = new Mock<IEventBus>();
        Action<object, TenantUpdateAvailableEvent> yakalanan = null!;
        bus.Setup(b => b.Subscribe<TenantUpdateAvailableEvent>(
                It.IsAny<object>(), It.IsAny<Action<object, TenantUpdateAvailableEvent>>()))
            .Callback<object, Action<object, TenantUpdateAvailableEvent>>((_, h) => yakalanan = h);
        var vm = new MaliDonemListViewModel(
            OrtakServisler().Object, DonemServisi(liste).Object,
            TenantServisi().Object, bus.Object);

        await vm.LoadAsync(new MaliDonemListArgs { FirmaId = 7 }, silent: true);
        vm.Subscribe();
        yakalanan.Should().NotBeNull("VM E1'e abone olmalı");
        var model = vm.ItemsSource.Single();
        model.DbAnalizYapildi = false;

        yakalanan(vm, new TenantUpdateAvailableEvent("db-f-0001_2027", "1.1.28.1709", "1.1.0"));

        model.DbAnalizYapildi.Should().BeTrue();
        model.DbDurum.Should().Be(DatabaseStatusResult.RequiredUpdating);
        model.DbAnalizDetay.Should().Contain("→");
    }
}
