using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Tests;

/// <summary>Denetim Firma Kayıt + Dönem bölümleri: providersız kırılmaz, yükleme eşleşir, değişiklik otomatik kaydedilir.</summary>
public class DenetimFirmaDonemTests
{
    private static Mock<Business.Contracts.UIServices.CommonServices.ICommonServices> OrtakServisler()
    {
        var baglam = new Mock<Business.Contracts.UIServices.CommonServices.IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var ortak = new Mock<Business.Contracts.UIServices.CommonServices.ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<Business.Contracts.UIServices.CommonServices.IMessageService>());
        ortak.SetupGet(o => o.StatusMessageService).Returns(Mock.Of<Business.Contracts.UIServices.IStatusMessageService>());
        return ortak;
    }

    private sealed class BellekKayitSaglayici : IEntityRegistrySettingsProvider
    {
        public EntityRegistrySettings Model { get; set; } = new();
        public int KayitSayisi { get; private set; }
        public Task<EntityRegistrySettings> GetAsync(long firmaId = 0) => Task.FromResult(Model);
        public Task SaveAsync(EntityRegistrySettings settings, long firmaId = 0)
        {
            Model = settings;
            KayitSayisi++;
            return Task.CompletedTask;
        }
    }

    private sealed class BellekDonemSaglayici : ITenantSettingsProvider
    {
        public TenantSettings Model { get; set; } = new();
        public int KayitSayisi { get; private set; }
        public Task<TenantSettings> GetAsync(long firmaId = 0) => Task.FromResult(Model);
        public Task SaveAsync(TenantSettings settings, long firmaId = 0)
        {
            Model = settings;
            KayitSayisi++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task FirmaKayit_Providersiz_Load_Kirilmaz()
    {
        var vm = new FirmaKayitAyarlarViewModel(OrtakServisler().Object, null!, null!);

        var eylem = () => vm.LoadAsync();

        await eylem.Should().NotThrowAsync();
        vm.FirmaKodPattern.Should().Be("F-XXXX");
        vm.SeciliDefaultDurum.Should().Be("Açık");
    }

    [Fact]
    public async Task FirmaKayit_Load_Esleme()
    {
        var bellek = new BellekKayitSaglayici
        {
            Model = new EntityRegistrySettings
            {
                FirmaKodPattern = "F-9999",
                DefaultDurum = DonemDurum.Kapali,
                AcikPageSize = 12,
                ArsivPageSize = 2,
                ValidationStrict = false
            }
        };
        var vm = new FirmaKayitAyarlarViewModel(OrtakServisler().Object, bellek, null!);

        await vm.LoadAsync();

        vm.FirmaKodPattern.Should().Be("F-9999");
        vm.SeciliDefaultDurum.Should().Be("Kapalı");
        vm.AcikPageSize.Should().Be(12);
        vm.ArsivPageSize.Should().Be(2);
        vm.ValidationStrict.Should().BeFalse();
    }

    [Fact]
    public async Task FirmaKayit_Degisiklik_OtomatikKaydeder()
    {
        var bellek = new BellekKayitSaglayici();
        var vm = new FirmaKayitAyarlarViewModel(OrtakServisler().Object, bellek, null!);
        await vm.LoadAsync();

        vm.AcikPageSize = 20;
        vm.SeciliDefaultDurum = "Arşivli";

        await BekleAsync(() => bellek.KayitSayisi >= 2);
        bellek.Model.AcikPageSize.Should().Be(20);
        bellek.Model.DefaultDurum.Should().Be(DonemDurum.Arsivlenmis);
    }

    [Fact]
    public async Task Donem_Providersiz_Load_Kirilmaz()
    {
        var vm = new DonemAyarlarViewModel(OrtakServisler().Object, null!, null!);

        var eylem = () => vm.LoadAsync();

        await eylem.Should().NotThrowAsync();
        vm.YedekPageSize.Should().Be(4);
        vm.Pooling.Should().BeTrue();
    }

    [Fact]
    public async Task Donem_Load_Esleme()
    {
        var bellek = new BellekDonemSaglayici
        {
            Model = new TenantSettings { YedekPageSize = 12, MigrationRetry = 5, Pooling = false }
        };
        var vm = new DonemAyarlarViewModel(OrtakServisler().Object, bellek, null!);

        await vm.LoadAsync();

        vm.YedekPageSize.Should().Be(12);
        vm.MigrationRetry.Should().Be(5);
        vm.Pooling.Should().BeFalse();
    }

    [Fact]
    public async Task Donem_Degisiklik_OtomatikKaydeder()
    {
        var bellek = new BellekDonemSaglayici();
        var vm = new DonemAyarlarViewModel(OrtakServisler().Object, bellek, null!);
        await vm.LoadAsync();

        vm.BakimTimeoutSec = 300;

        await BekleAsync(() => bellek.KayitSayisi >= 1);
        bellek.Model.BakimTimeoutSec.Should().Be(300);
    }

    private static async Task BekleAsync(Func<bool> kosul)
    {
        for (int i = 0; i < 50 && !kosul(); i++)
            await Task.Delay(50);
        kosul().Should().BeTrue("otomatik kayıt tetiklenmedi");
    }
}
