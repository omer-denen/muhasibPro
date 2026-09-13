using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Tests;

/// <summary>Yedek listesi pagination regresyonu: liste yüklenince CanPrev/CanNext
/// bildirimleri gelmezse butonlar ölü kalır (IsEnabled binding tazelenmez).</summary>
public class YedekPaginationTests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(Mock.Of<IContextService>());
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        return ortak;
    }

    private static List<DatabaseBackupResult> YedekUret(int adet, string db = "db-FIRMA01_2026")
    {
        var liste = new List<DatabaseBackupResult>();
        for (int i = 0; i < adet; i++)
            liste.Add(new DatabaseBackupResult
            {
                DatabaseName = db,
                BackupFileName = $"{db}_2026090{i + 1}_120000_abc{i}.backup"
            });
        return liste;
    }

    private static DonemYedeklerViewModel VmKur(List<DatabaseBackupResult> yedekler)
    {
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync(It.IsAny<string>())).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(yedekler, "ok"));
        // LocalSettingsService null → sayfa boyutu model varsayılanı (4) korunur.
        return new DonemYedeklerViewModel(OrtakServisler().Object, operasyon.Object,
            Mock.Of<ITenantBackupService>(), Mock.Of<IMaliDonemService>(), null!);
    }

    [Fact]
    public async Task YedekListesi_Yuklenince_Sayfalama_Durumu_Guncellenir()
    {
        var vm = VmKur(YedekUret(5));
        var bildirilen = new List<string>();
        vm.PropertyChanged += (_, e) => { if (e.PropertyName != null) bildirilen.Add(e.PropertyName); };
        await vm.YukleAsync("db-FIRMA01_2026");

        vm.YedekPageSize.Should().Be(4);
        vm.YedekTotalPages.Should().Be(2);
        vm.YedekHasPagination.Should().BeTrue();
        vm.YedekCanNext.Should().BeTrue("liste ilk sayfada, ikinci sayfa varken İleri aktif olmalı");
        vm.YedekCanPrev.Should().BeFalse();
        vm.PagedYedekler.Should().HaveCount(4);
        vm.YedekPageInfo.Should().Be("1 / 2");
        // Buton IsEnabled binding'leri ancak bildirimle tazelenir — eksik bildirim = ölü pagination.
        bildirilen.Should().Contain(nameof(DonemYedeklerViewModel.YedekCanNext));
        bildirilen.Should().Contain(nameof(DonemYedeklerViewModel.YedekCanPrev));
        bildirilen.Should().Contain(nameof(DonemYedeklerViewModel.YedekPageInfo));
        bildirilen.Should().Contain(nameof(DonemYedeklerViewModel.YedekHasPagination));
    }

    [Fact]
    public async Task YedekListesi_IkinciSayfaya_Gecince_Dogru_Dilim()
    {
        var vm = VmKur(YedekUret(5));
        await vm.YukleAsync("db-FIRMA01_2026");

        vm.YedekCurrentPage = 2;

        vm.PagedYedekler.Should().HaveCount(1);
        vm.YedekCanPrev.Should().BeTrue();
        vm.YedekCanNext.Should().BeFalse();
        vm.YedekPageInfo.Should().Be("2 / 2");
    }

    [Fact]
    public async Task YedekListesi_Kisalinca_Sayfa_Tasmaz()
    {
        var yedekler = YedekUret(5);
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.GetBackupHistoryAsync(It.IsAny<string>())).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(yedekler, "ok"));
        var vm = new DonemYedeklerViewModel(OrtakServisler().Object, operasyon.Object,
            Mock.Of<ITenantBackupService>(), Mock.Of<IMaliDonemService>(), null!);
        await vm.YukleAsync("db-FIRMA01_2026");
        vm.YedekCurrentPage = 2;

        // Son sayfadaki yedekler silindi → servis yeni (kısa) liste döner.
        operasyon.Setup(o => o.GetBackupHistoryAsync(It.IsAny<string>())).ReturnsAsync(
            new SuccessApiDataResponse<List<DatabaseBackupResult>>(yedekler.Take(1).ToList(), "ok"));
        await vm.YukleAsync("db-FIRMA01_2026");

        vm.YedekCurrentPage.Should().Be(1);
        vm.PagedYedekler.Should().HaveCount(1);
        vm.YedekHasPagination.Should().BeFalse();
    }
}
