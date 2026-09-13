using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.77 Adım 2: SistemYedek + SistemGuncelleme VM zincir testleri (Kural 5/7/11).</summary>
public class SistemYedekGuncellemeViewModelTests
{
    private static Mock<ICommonServices> OrtakServisler(bool dialogOnay = true)
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var dialog = new Mock<IDialogService>();
        dialog.Setup(d => d.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dialogOnay);
        dialog.Setup(d => d.ShowSistemRestoreVerifyAsync(It.IsAny<SistemRestoreAnalizSonuc>()))
            .ReturnsAsync(dialogOnay);
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        ortak.SetupGet(o => o.DialogService).Returns(dialog.Object);
        ortak.SetupGet(o => o.StatusMessageService).Returns(Mock.Of<Business.Contracts.UIServices.IStatusMessageService>());
        return ortak;
    }

    private static Mock<IDatabaseSettingsProvider> Ayarlar(int keep = 3)
    {
        var saglayici = new Mock<IDatabaseSettingsProvider>();
        saglayici.Setup(s => s.GetAsync(It.IsAny<long>()))
            .ReturnsAsync(new DatabaseSettingsModel { SistemKeepLast = keep });
        return saglayici;
    }

    private static Mock<ISistemRestoreAnalizService> AnalizServisi(bool engelli = false, bool kayip = false)
    {
        var sonuc = new SistemRestoreAnalizSonuc
        {
            YedekDosyaAdi = "a.backup",
            Hukum = new RestoreVerdict
            {
                Kind = engelli ? RestoreVerdictKind.Block
                    : kayip ? RestoreVerdictKind.RequireCode
                    : RestoreVerdictKind.Allow,
                Baslik = engelli ? "Engellendi" : kayip ? "Kayıp kayıt riski" : "Uygun",
                Aciklama = "-"
            }
        };
        if (kayip) sonuc.Fark.KayipKayitlar.Add("2028 dönemi");
        var analiz = new Mock<ISistemRestoreAnalizService>();
        analiz.Setup(a => a.AnalizEtAsync(It.IsAny<string>())).ReturnsAsync(sonuc);
        return analiz;
    }

    [Fact]
    public async Task YedekYukle_Listeler_SaklamaModelden()
    {
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>
            {
                new() { BackupFileName = "a.backup", IsBackupComleted = true },
                new() { BackupFileName = "b.backup", IsBackupComleted = true }
            }, "ok"));
        var vm = new SistemYedekViewModel(OrtakServisler().Object, operasyon.Object, Ayarlar(5).Object, AnalizServisi().Object);

        await vm.YukleAsync();

        vm.Yedekler.Should().HaveCount(2);
        vm.SaklamaMetni.Should().Contain("5");
        vm.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task YedekAl_FifoKeepModelden()
    {
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        operasyon.Setup(o => o.CreateBackupAsync(DatabaseBackupType.Manual)).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseBackupResult>(
                new DatabaseBackupResult { BackupFileName = "yeni.backup", IsBackupComleted = true }, "ok"));
        operasyon.Setup(o => o.GetBackupHistoryAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        operasyon.Setup(o => o.CleanOldBackupsAsync(3)).ReturnsAsync(
            new SuccessApiDataResponse<int>(0, "temizlenecek yok"));
        var vm = new SistemYedekViewModel(OrtakServisler().Object, operasyon.Object, Ayarlar(3).Object, AnalizServisi().Object);

        await vm.YedekAlAsync();

        operasyon.Verify(o => o.CleanOldBackupsAsync(3), Times.Once);
        vm.IsHata.Should().BeFalse();
        vm.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task GeriYukle_Onaysiz_Cagirmaz()
    {
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        var vm = new SistemYedekViewModel(OrtakServisler(false).Object, operasyon.Object, Ayarlar().Object, AnalizServisi().Object);

        await vm.GeriYukleAsync(new DatabaseBackupResult { BackupFileName = "a.backup" });

        operasyon.Verify(o => o.RestoreBackupAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GeriYukle_Onayli_Cagirir()
    {
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        operasyon.Setup(o => o.RestoreBackupAsync("a.backup")).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseRestoreExecutionResult>(new DatabaseRestoreExecutionResult(), "geri yüklendi"));
        operasyon.Setup(o => o.GetBackupHistoryAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        var vm = new SistemYedekViewModel(OrtakServisler(true).Object, operasyon.Object, Ayarlar().Object, AnalizServisi().Object);

        await vm.GeriYukleAsync(new DatabaseBackupResult { BackupFileName = "a.backup" });

        operasyon.Verify(o => o.RestoreBackupAsync("a.backup"), Times.Once);
        vm.IsHata.Should().BeFalse();
    }

    [Fact]
    public async Task GeriYukle_BosCagri_Dokunmaz()
    {
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        var vm = new SistemYedekViewModel(OrtakServisler(true).Object, operasyon.Object, Ayarlar().Object, AnalizServisi().Object);

        await vm.GeriYukleAsync(null);

        operasyon.Verify(o => o.RestoreBackupAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GeriYukle_Engelli_RestoreEtmez()
    {
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        var vm = new SistemYedekViewModel(OrtakServisler(true).Object, operasyon.Object, Ayarlar().Object, AnalizServisi(engelli: true).Object);

        await vm.GeriYukleAsync(new DatabaseBackupResult { BackupFileName = "a.backup" });

        operasyon.Verify(o => o.RestoreBackupAsync(It.IsAny<string>()), Times.Never);
        vm.IsHata.Should().BeTrue();
    }

    [Fact]
    public async Task GeriYukle_KayipKayitli_Onayla_RestoreEder()
    {
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        operasyon.Setup(o => o.RestoreBackupAsync("a.backup")).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseRestoreExecutionResult>(new DatabaseRestoreExecutionResult(), "geri yüklendi"));
        operasyon.Setup(o => o.GetBackupHistoryAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        var vm = new SistemYedekViewModel(OrtakServisler(true).Object, operasyon.Object, Ayarlar().Object, AnalizServisi(kayip: true).Object);

        await vm.GeriYukleAsync(new DatabaseBackupResult { BackupFileName = "a.backup" });

        operasyon.Verify(o => o.RestoreBackupAsync("a.backup"), Times.Once);
        vm.IsHata.Should().BeFalse();
    }

    [Fact]
    public async Task Yukle_LimitAsilinca_Budar()
    {
        // 5 yedek varken limit 3: yükleme budar, liste 3'e iner (canlı tur bulgusu).
        List<DatabaseBackupResult> YedekUret(int n) =>
            Enumerable.Range(1, n).Select(i => new DatabaseBackupResult { BackupFileName = $"{i}.backup", IsBackupComleted = true }).ToList();
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        operasyon.SetupSequence(o => o.GetBackupHistoryAsync())
            .ReturnsAsync(new SuccessApiDataResponse<List<DatabaseBackupResult>>(YedekUret(5), "ok"))
            .ReturnsAsync(new SuccessApiDataResponse<List<DatabaseBackupResult>>(YedekUret(3), "ok"));
        operasyon.Setup(o => o.CleanOldBackupsAsync(3)).ReturnsAsync(
            new SuccessApiDataResponse<int>(2, "2 silindi"));
        var vm = new SistemYedekViewModel(OrtakServisler().Object, operasyon.Object, Ayarlar(3).Object, AnalizServisi().Object);

        await vm.YukleAsync();

        operasyon.Verify(o => o.CleanOldBackupsAsync(3), Times.Once);
        vm.Yedekler.Should().HaveCount(3);
        vm.IsHata.Should().BeFalse();
        vm.DurumMetni.Should().Contain("temizlendi");
    }

    [Fact]
    public async Task YedekAl_TemizlikBasarisizsa_Uyarir()
    {
        var operasyon = new Mock<ISistemDatabaseOperationService>();
        operasyon.Setup(o => o.CreateBackupAsync(DatabaseBackupType.Manual)).ReturnsAsync(
            new SuccessApiDataResponse<DatabaseBackupResult>(
                new DatabaseBackupResult { BackupFileName = "yeni.backup", IsBackupComleted = true }, "ok"));
        operasyon.Setup(o => o.GetBackupHistoryAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(new List<DatabaseBackupResult>(), "ok"));
        operasyon.Setup(o => o.CleanOldBackupsAsync(3)).ReturnsAsync(
            new ErrorApiDataResponse<int>(0, "disk kilitli"));
        var vm = new SistemYedekViewModel(OrtakServisler().Object, operasyon.Object, Ayarlar(3).Object, AnalizServisi().Object);

        await vm.YedekAlAsync();

        vm.IsHata.Should().BeTrue();
        vm.DurumMetni.Should().Contain("temizliği yapılamadı");
    }

    [Fact]
    public async Task Guncelle_Onaysiz_Cagirmaz()
    {
        var servis = new Mock<ISistemDatabaseService>();
        servis.Setup(s => s.GetPendingMigrationsAsync()).ReturnsAsync(new List<string> { "20260911_X" });
        var vm = new SistemGuncellemeViewModel(OrtakServisler(false).Object, servis.Object);
        await vm.YukleAsync();

        await vm.GuncelleAsync();

        servis.Verify(s => s.ApplyPendingSistemMigrationsAsync(), Times.Never);
    }

    [Fact]
    public async Task Guncelle_Onayli_Uygular_ListeyiTazeler()
    {
        var servis = new Mock<ISistemDatabaseService>();
        servis.SetupSequence(s => s.GetPendingMigrationsAsync())
            .ReturnsAsync(new List<string> { "20260911_X" })
            .ReturnsAsync(new List<string>());
        servis.Setup(s => s.ApplyPendingSistemMigrationsAsync()).ReturnsAsync((true, "güncellendi"));
        var vm = new SistemGuncellemeViewModel(OrtakServisler(true).Object, servis.Object);
        await vm.YukleAsync();
        vm.GocVarMi.Should().BeTrue();

        await vm.GuncelleAsync();

        servis.Verify(s => s.ApplyPendingSistemMigrationsAsync(), Times.Once);
        vm.IsHata.Should().BeFalse();
        vm.GocVarMi.Should().BeFalse();
        vm.IsBusy.Should().BeFalse();
    }
}
