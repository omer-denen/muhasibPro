using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using System.Collections.Generic;

namespace MuhasibPro.Tests;

/// <summary>Oturum 202: yedek-silme sonuç kontrolü + silinen-dönem ayrımı + busy bayrakları.</summary>
public class YonetimSilmeYuklemeTests
{
    private sealed class BildirimYakalayici : INotificationService
    {
        public readonly List<(string baslik, string mesaj, NotificationType tur)> Bildirimler = new();
        public readonly List<(string baslik, string mesaj, NotificationType tur, string etiket, string grup)> EtiketliBildirimler = new();
        public void Show(string title, string message, NotificationType type = NotificationType.Info)
            => Bildirimler.Add((title, message, type));
        public void ShowTagged(string title, string message, NotificationType type, string tag, string group)
            => EtiketliBildirimler.Add((title, message, type, tag, group));
    }

    private static Mock<ICommonServices> OrtakServisler(
        bool dialogOnay = true,
        BildirimYakalayici bildirim = null!,
        Mock<Business.Contracts.UIServices.IStatusMessageService> durum = null!,
        bool yaziliOnay = true)
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var dialog = new Mock<IDialogService>();
        dialog.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dialogOnay);
        dialog.Setup(d => d.ShowBackupDeleteGuardAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(yaziliOnay);
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        ortak.SetupGet(o => o.DialogService).Returns(dialog.Object);
        ortak.SetupGet(o => o.NotificationService).Returns((bildirim ?? new BildirimYakalayici()));
        ortak.SetupGet(o => o.StatusMessageService).Returns((durum ?? new Mock<Business.Contracts.UIServices.IStatusMessageService>()).Object);
        return ortak;
    }

    [Fact]
    public async Task YedekSil_Basarisiz_DangerListedeKalir()
    {
        var bildirim = new BildirimYakalayici();
        var ortak = OrtakServisler(bildirim: bildirim);
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync("db-A")).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>
            {
                new() { DatabaseName = "db-A", BackupFileName = "f1.backup", BackupFilePath = "C:/x/f1.backup" }
            }, "ok"));
        var yedekServisi = new Mock<ITenantBackupService>();
        yedekServisi.Setup(s => s.TryDeleteBackupFileAsync(It.IsAny<string>())).ReturnsAsync((false, "kilitli"));
        var vm = new DonemYedeklerViewModel(ortak.Object, operasyon.Object,
            yedekServisi.Object, Mock.Of<IMaliDonemService>(), null!, null!);
        await vm.YukleAsync("db-A");
        vm.SelectedYedek = new DatabaseBackupResult
        {
            DatabaseName = "db-A",
            BackupFileName = "f1.backup",
            BackupFilePath = "C:/x/f1.backup"
        };

        await vm.YedekSilAsync();

        bildirim.EtiketliBildirimler.Should().ContainSingle(b => b.tur == NotificationType.Danger);
        bildirim.EtiketliBildirimler[0].baslik.Should().Be("Yedek Silinemedi");
        bildirim.EtiketliBildirimler[0].etiket.Should().Be("YedekSil");
        bildirim.EtiketliBildirimler[0].grup.Should().Be(NotificationGroups.Yedek);
        vm.Yedekler.Should().ContainSingle("silinmeyen dosya listede kalmalı");
        vm.IsYedeklerYukleniyor.Should().BeFalse("bayrak finally ile kapanmalı");
    }

    [Fact]
    public async Task YedekSil_Basarili_SuccessListeBosalir()
    {
        var bildirim = new BildirimYakalayici();
        var ortak = OrtakServisler(bildirim: bildirim);
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync("db-A")).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        var yedekServisi = new Mock<ITenantBackupService>();
        yedekServisi.Setup(s => s.TryDeleteBackupFileAsync(It.IsAny<string>())).ReturnsAsync((true, string.Empty));
        var vm = new DonemYedeklerViewModel(ortak.Object, operasyon.Object,
            yedekServisi.Object, Mock.Of<IMaliDonemService>(), null!, null!);
        vm.SelectedYedek = new DatabaseBackupResult
        {
            DatabaseName = "db-A",
            BackupFileName = "f1.backup",
            BackupFilePath = "C:/x/f1.backup"
        };

        await vm.YedekSilAsync();

        bildirim.EtiketliBildirimler.Should().ContainSingle(b => b.tur == NotificationType.Success);
        bildirim.EtiketliBildirimler[0].etiket.Should().Be("YedekSil");
        bildirim.EtiketliBildirimler[0].grup.Should().Be(NotificationGroups.Yedek);
        vm.Yedekler.Should().BeEmpty();
        vm.IsYedeklerYukleniyor.Should().BeFalse();
    }

    [Fact]
    public async Task SilinenDonemYedegi_AyriListede_BilinmeyeneKarismaz()
    {
        var ortak = OrtakServisler();
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetAllBackupsAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>
            {
                new() { DatabaseName = "db-ESKI_2099", BackupFileName = "eski.backup", KimlikFirmaId = 7, KimlikMaliDonemId = 99 },
                new() { DatabaseName = "db-YABANCI_2099", BackupFileName = "yabanci.backup" },
                new() { DatabaseName = "db-F7_2026", BackupFileName = "kayitli.backup", KimlikFirmaId = 7, KimlikMaliDonemId = 1 }
            }, "ok"));
        var donemler = new Mock<IMaliDonemService>();
        donemler.Setup(s => s.GetMaliDonemlerWithFirmaId(It.IsAny<DataRequest<MaliDonem>>(), 7))
            .ReturnsAsync(new SuccessApiDataResponse<IList<MaliDonemModel>>(new List<MaliDonemModel>
            {
                new() { Id = 1, DatabaseName = "db-F7_2026" },
                new() { Id = 2, DatabaseName = "db-F7_2027" }
            }, "ok"));
        var vm = new BilinmeyenYedekViewModel(ortak.Object, operasyon.Object,
            Mock.Of<ITenantBackupService>(), donemler.Object, null!, null!);
        vm.FirmaId = 7;

        await vm.TaraAsync();

        vm.SilinenDonemYedekleri.Should().ContainSingle("dönemi silinmiş kimlikli yedek ayrı listede");
        vm.SilinenDonemYedekleri[0].BackupFileName.Should().Be("eski.backup");
        vm.BilinmeyenYedekler.Should().ContainSingle("kimliksiz yabancı bilinmeyende");
        vm.BilinmeyenYedekler[0].BackupFileName.Should().Be("yabanci.backup");
        vm.IsTaraniyor.Should().BeFalse();
    }

    [Fact]
    public async Task YedekSil_AltSinirAltinda_YaziliOnayIster()
    {
        var bildirim = new BildirimYakalayici();
        var ortak = OrtakServisler(bildirim: bildirim, yaziliOnay: true);
        var dialog = Mock.Get(ortak.Object.DialogService);
        var yerel = new Mock<Business.Contracts.UIServices.ILocalSettingsService>();
        yerel.Setup(l => l.ReadSettingAsync<DatabaseSettingsModel>(It.IsAny<string>()))
            .ReturnsAsync(new DatabaseSettingsModel { MaxManuelYedekSayisi = 5 });
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync("db-A")).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        var yedekServisi = new Mock<ITenantBackupService>();
        yedekServisi.Setup(s => s.TryDeleteBackupFileAsync(It.IsAny<string>())).ReturnsAsync((true, string.Empty));
        var vm = new DonemYedeklerViewModel(ortak.Object, operasyon.Object,
            yedekServisi.Object, Mock.Of<IMaliDonemService>(), yerel.Object, null!);
        // 2 yedekle yükle: silme sonrası 1 < keep(5) → yazılı onay yolu.
        operasyon.Setup(o => o.GetBackupHistoryAsync("db-A")).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>
            {
                new() { DatabaseName = "db-A", BackupFileName = "f1.backup", BackupFilePath = "C:/x/f1.backup" },
                new() { DatabaseName = "db-A", BackupFileName = "f2.backup", BackupFilePath = "C:/x/f2.backup" }
            }, "ok"));
        await vm.YukleAsync("db-A");
        vm.SelectedYedek = vm.Yedekler[0];

        await vm.YedekSilAsync();

        dialog.Verify(d => d.ShowBackupDeleteGuardAsync("f1.backup", 1, 5), Times.Once);
        dialog.Verify(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        yedekServisi.Verify(s => s.TryDeleteBackupFileAsync("C:/x/f1.backup"), Times.Once);
        bildirim.EtiketliBildirimler.Should().ContainSingle(b => b.tur == NotificationType.Success);
        bildirim.EtiketliBildirimler[0].etiket.Should().Be("YedekSil");
        bildirim.EtiketliBildirimler[0].grup.Should().Be(NotificationGroups.Yedek);
    }

    [Fact]
    public async Task YedekSil_AltSinirAltinda_Reddederse_Silmez()
    {
        var ortak = OrtakServisler(yaziliOnay: false);
        var dialog = Mock.Get(ortak.Object.DialogService);
        var yerel = new Mock<Business.Contracts.UIServices.ILocalSettingsService>();
        yerel.Setup(l => l.ReadSettingAsync<DatabaseSettingsModel>(It.IsAny<string>()))
            .ReturnsAsync(new DatabaseSettingsModel { MaxManuelYedekSayisi = 5 });
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync("db-A")).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>
            {
                new() { DatabaseName = "db-A", BackupFileName = "f1.backup", BackupFilePath = "C:/x/f1.backup" }
            }, "ok"));
        var yedekServisi = new Mock<ITenantBackupService>();
        var vm = new DonemYedeklerViewModel(ortak.Object, operasyon.Object,
            yedekServisi.Object, Mock.Of<IMaliDonemService>(), yerel.Object, null!);
        await vm.YukleAsync("db-A");
        vm.SelectedYedek = vm.Yedekler[0];

        await vm.YedekSilAsync();

        dialog.Verify(d => d.ShowBackupDeleteGuardAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        yedekServisi.Verify(s => s.TryDeleteBackupFileAsync(It.IsAny<string>()), Times.Never);
        vm.Yedekler.Should().ContainSingle("reddedilen silme listeyi değiştirmemeli");
    }

    [Fact]
    public async Task YedekSil_AltSinirUstunde_NormalOnay()
    {
        var ortak = OrtakServisler(dialogOnay: true);
        var dialog = Mock.Get(ortak.Object.DialogService);
        var yerel = new Mock<Business.Contracts.UIServices.ILocalSettingsService>();
        yerel.Setup(l => l.ReadSettingAsync<DatabaseSettingsModel>(It.IsAny<string>()))
            .ReturnsAsync(new DatabaseSettingsModel { MaxManuelYedekSayisi = 1 });
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync("db-A")).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>
            {
                new() { DatabaseName = "db-A", BackupFileName = "f8.backup", BackupFilePath = "C:/x/f8.backup" },
                new() { DatabaseName = "db-A", BackupFileName = "f9.backup", BackupFilePath = "C:/x/f9.backup" }
            }, "ok"));
        var yedekServisi = new Mock<ITenantBackupService>();
        yedekServisi.Setup(s => s.TryDeleteBackupFileAsync(It.IsAny<string>())).ReturnsAsync((true, string.Empty));
        var vm = new DonemYedeklerViewModel(ortak.Object, operasyon.Object,
            yedekServisi.Object, Mock.Of<IMaliDonemService>(), yerel.Object, null!);
        await vm.YukleAsync("db-A");
        vm.SelectedYedek = vm.Yedekler[0];

        await vm.YedekSilAsync();

        dialog.Verify(d => d.ShowBackupDeleteGuardAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        dialog.Verify(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GercekDosya_Silme_Nedenli()
    {
        var svc = new MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService.Common.TenantBackupService(null!, null!);
        var dizin = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "muhasibpro-siltest-" + System.Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(dizin);
        try
        {
            var normal = System.IO.Path.Combine(dizin, "a.backup");
            await System.IO.File.WriteAllTextAsync(normal, "x");
            var (ok1, _) = await svc.TryDeleteBackupFileAsync(normal);
            ok1.Should().BeTrue();
            System.IO.File.Exists(normal).Should().BeFalse();

            var saltOkunur = System.IO.Path.Combine(dizin, "b.backup");
            await System.IO.File.WriteAllTextAsync(saltOkunur, "x");
            System.IO.File.SetAttributes(saltOkunur, System.IO.FileAttributes.ReadOnly);
            var (ok2, _) = await svc.TryDeleteBackupFileAsync(saltOkunur);
            ok2.Should().BeTrue("salt-okunur bayrağı temizlenmeli");

            var (ok3, neden3) = await svc.TryDeleteBackupFileAsync(System.IO.Path.Combine(dizin, "yok.backup"));
            ok3.Should().BeFalse();
            neden3.Should().NotBeNullOrWhiteSpace("neden yutulmamalı");
        }
        finally
        {
            try { System.IO.Directory.Delete(dizin, true); } catch { }
        }
    }

    [Fact]
    public async Task Tara_Bayrak_HataYolundaKapanir()
    {
        var ortak = OrtakServisler();
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetAllBackupsAsync()).ThrowsAsync(new System.InvalidOperationException("disk yok"));
        var vm = new BilinmeyenYedekViewModel(ortak.Object, operasyon.Object,
            Mock.Of<ITenantBackupService>(), Mock.Of<IMaliDonemService>(), null!, null!);

        await vm.TaraAsync();

        vm.IsTaraniyor.Should().BeFalse("bayrak finally ile kapanmalı");
        vm.BilinmeyenYedekler.Should().BeEmpty();
        vm.SilinenDonemYedekleri.Should().BeEmpty();
    }
}
