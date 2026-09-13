using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using System.Diagnostics;

namespace MuhasibPro.Tests;

/// <summary>Faz 5 E2: yedek/geri-yükleme tamamlanınca tipli olay yayınlanır;
/// yedek listesi + Bilinmeyen taraması tazelenir; Subscribe = Unsubscribe.</summary>
public class YedekOlayTests
{
    private sealed class AtesleyenBus : IEventBus
    {
        private readonly object _kilit = new();
        private readonly Dictionary<object, List<(Type tur, Delegate isleyici)>> _aboneler = new();
        public readonly List<object> Aboneler = new();
        public readonly List<object> Cikartilanlar = new();
        public readonly List<DomainEvent> Yayinlar = new();

        public void Publish<TEvent>(object sender, TEvent @event) where TEvent : DomainEvent
        {
            List<(object hedef, Delegate isleyici)> cagrilacak = new();
            lock (_kilit)
            {
                Yayinlar.Add(@event);
                foreach (var (hedef, liste) in _aboneler)
                    foreach (var (tur, isleyici) in liste)
                        if (tur == typeof(TEvent))
                            cagrilacak.Add((hedef, isleyici));
            }
            foreach (var (_, isleyici) in cagrilacak)
                isleyici.DynamicInvoke(sender, @event);
        }

        public void Subscribe<TEvent>(object target, Action<object, TEvent> handler) where TEvent : DomainEvent
        {
            lock (_kilit)
            {
                if (!_aboneler.TryGetValue(target, out var liste))
                    _aboneler[target] = liste = new List<(Type, Delegate)>();
                liste.Add((typeof(TEvent), handler));
                Aboneler.Add(target);
            }
        }

        public void Unsubscribe(object target)
        {
            lock (_kilit)
            {
                _aboneler.Remove(target);
                Cikartilanlar.Add(target);
            }
        }
    }

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

    private static Mock<ISistemLogService> SessizLog()
    {
        var log = new Mock<ISistemLogService>();
        log.Setup(l => l.WriteAsync(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        log.Setup(l => l.WriteAsync(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
            .Returns(Task.CompletedTask);
        return log;
    }

    private static async Task<bool> BekleAsync(Func<bool> kosul, int zamanAsimiMs = 5000)
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
    public async Task Yedek_Basarili_Olay_Yayinlanir()
    {
        var bus = new AtesleyenBus();
        var yedekleyici = new Mock<ITenantSQLiteBackupManager>();
        yedekleyici.Setup(m => m.CreateBackupAsync("db-A", DatabaseBackupType.Manual)).ReturnsAsync(
            new DatabaseBackupResult { DatabaseName = "db-A", BackupFileName = "db-A_20240101_120000_abcd.backup", IsBackupComleted = true });
        var svc = new TenantSQLiteDatabaseOperationService(
            yedekleyici.Object, Mock.Of<ITenantSQLiteDatabaseManager>(), SessizLog().Object, null!, bus);

        var sonuc = await svc.CreateBackupAsync("db-A", DatabaseBackupType.Manual);

        sonuc.Success.Should().BeTrue();
        bus.Yayinlar.OfType<TenantBackupCompletedEvent>().Should().ContainSingle()
            .Which.DatabaseName.Should().Be("db-A");
    }

    [Fact]
    public async Task GeriYukleme_Basarili_Olay_Yayinlanir()
    {
        var bus = new AtesleyenBus();
        var yedekleyici = new Mock<ITenantSQLiteBackupManager>();
        yedekleyici.Setup(m => m.RestoreBackupAsync("db-A", "f.backup")).ReturnsAsync(
            new DatabaseRestoreExecutionResult { DatabaseName = "db-A", IsRestoreSuccess = true });
        var svc = new TenantSQLiteDatabaseOperationService(
            yedekleyici.Object, Mock.Of<ITenantSQLiteDatabaseManager>(), SessizLog().Object, null!, bus);

        var sonuc = await svc.RestoreBackupAsync("db-A", "f.backup");

        sonuc.Success.Should().BeTrue();
        bus.Yayinlar.OfType<TenantRestoreCompletedEvent>().Should().ContainSingle()
            .Which.BackupFileName.Should().Be("f.backup");
    }

    [Fact]
    public async Task YedekListesi_Olayda_Tazelenir_YanlisDb_GormezdenGelinir()
    {
        var bus = new AtesleyenBus();
        var ortak = OrtakServisler();
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync(It.IsAny<string>())).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        var vm = new DonemYedeklerViewModel(ortak.Object, operasyon.Object,
            Mock.Of<ITenantBackupService>(), Mock.Of<IMaliDonemService>(), null!, bus);
        vm.Subscribe();
        await vm.YukleAsync("db-A");

        bus.Publish(vm, new TenantBackupCompletedEvent("db-BASKA", "f.backup"));
        await Task.Delay(300);
        operasyon.Verify(o => o.GetBackupHistoryAsync(It.IsAny<string>()), Times.Once);

        bus.Publish(vm, new TenantBackupCompletedEvent("db-A", "f.backup"));
        (await BekleAsync(() =>
        {
            try { operasyon.Verify(o => o.GetBackupHistoryAsync(It.IsAny<string>()), Times.Exactly(2)); return true; }
            catch { return false; }
        })).Should().BeTrue();

        vm.Unsubscribe();
        bus.Publish(vm, new TenantBackupCompletedEvent("db-A", "f2.backup"));
        await Task.Delay(300);
        operasyon.Verify(o => o.GetBackupHistoryAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Bilinmeyen_Olayda_Tarar()
    {
        var bus = new AtesleyenBus();
        var ortak = OrtakServisler();
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetAllBackupsAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        var vm = new BilinmeyenYedekViewModel(ortak.Object, operasyon.Object,
            Mock.Of<ITenantBackupService>(), Mock.Of<IMaliDonemService>(), null!, bus);
        vm.Subscribe();

        bus.Publish(vm, new TenantRestoreCompletedEvent("db-A", "f.backup"));
        (await BekleAsync(() =>
        {
            try { operasyon.Verify(o => o.GetAllBackupsAsync(), Times.AtLeastOnce()); return true; }
            catch { return false; }
        })).Should().BeTrue();

        vm.Unsubscribe();
    }

    [Fact]
    public async Task Bilinmeyen_KayitliDonemYedegini_Listelemez_Yabanciyi_Listeler()
    {
        // HATALAR B3 planlı testi: 2 dönem × 1 yedek → Bilinmeyen 0;
        // yabancı dosya → Bilinmeyen 1 (simetrik kanonik eşleşme regresyonu).
        var bus = new AtesleyenBus();
        var ortak = OrtakServisler();
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetAllBackupsAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>
            {
                new() { DatabaseName = "db-F-0001_2026", BackupFileName = "db-F-0001_2026_20260910_120000_a1.backup" },
                new() { DatabaseName = "db-F-0001_2027.db", BackupFileName = "db-F-0001_2027.db_20260910_120000_b2.backup" },
                new() { DatabaseName = "db-YABANCI_2099", BackupFileName = "db-YABANCI_2099_20260910_120000_c3.backup" }
            }, "ok"));
        var donemler = new Mock<IMaliDonemService>();
        donemler.Setup(s => s.GetMaliDonemlerPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DataRequest<MaliDonem>>()))
            .ReturnsAsync(new SuccessApiDataResponse<IList<MaliDonemModel>>(new List<MaliDonemModel>
            {
                new() { Id = 1, DatabaseName = "db-F-0001_2026" },
                new() { Id = 2, DatabaseName = "db-F-0001_2027" }
            }, "ok"));
        var vm = new BilinmeyenYedekViewModel(ortak.Object, operasyon.Object,
            Mock.Of<ITenantBackupService>(), donemler.Object, null!, bus);

        await vm.TaraAsync();

        vm.BilinmeyenYedekler.Should().ContainSingle(
            ".db'li + çıplak desenler kanonik eşleşmeli, yalnız yabancı kalmalı");
        vm.BilinmeyenYedekler[0].DatabaseName.Should().Be("db-YABANCI_2099");
    }

    [Fact]
    public void Yonetim_Subscribe_Unsubscribe_Esit()
    {
        var bus = new AtesleyenBus();
        var ortak = OrtakServisler();
        var vm = new MaliDonemYonetimViewModel(ortak.Object,
            Mock.Of<IMaliDonemService>(), Mock.Of<ITenantSQLiteDatabaseService>(),
            Mock.Of<ITenantSQLiteDatabaseOperationService>(), Mock.Of<ITenantBackupService>(), null!, bus);

        vm.Subscribe();
        vm.Unsubscribe();

        bus.Aboneler.Should().NotBeEmpty();
        foreach (var hedef in bus.Aboneler.Distinct())
            bus.Cikartilanlar.Should().Contain(hedef);
    }
}
