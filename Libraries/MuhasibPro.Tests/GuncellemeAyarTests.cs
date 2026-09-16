using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.ViewModels.Settings;
using Velopack;

namespace MuhasibPro.Tests;

/// <summary>Denetim Güncelleme bölümü: mevcut UpdateViewModel providersız kırılmaz
/// (Denetim iç çerçevede gömülü hostlanır — yeni VM yok).</summary>
public class GuncellemeAyarTests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        ortak.SetupGet(o => o.StatusMessageService).Returns(Mock.Of<IStatusMessageService>());
        return ortak;
    }

    [Fact]
    public async Task Providersiz_Initialize_Kirilmaz()
    {
        var vm = new UpdateViewModel(null!, OrtakServisler().Object, null!);

        var eylem = () => vm.InitializeAsync();

        await eylem.Should().NotThrowAsync();
        vm.LastCheckText.Should().Be("Son denetleme: Hiçbir zaman");
    }

    [Fact]
    public async Task Cikis_Abonelik_Birakmaz()
    {
        var vm = new UpdateViewModel(null!, OrtakServisler().Object, null!);
        await vm.InitializeAsync();

        var eylem = () => { vm.Unsubscribe(); return Task.CompletedTask; };

        await eylem.Should().NotThrowAsync();
    }

    /// <summary>UI thread'den await edilen async kapı (sync sürüm deadlock yapıyordu).</summary>
    private sealed class SahteGuncellemeServisi : IUpdateService
    {
        private readonly bool _bekleyenYenidenBaslatma;
        public SahteGuncellemeServisi(bool bekleyenYenidenBaslatma = false)
            => _bekleyenYenidenBaslatma = bekleyenYenidenBaslatma;

        public Task<UpdateSettingsModel> GetSettingsAsync()
            => Task.FromResult(new UpdateSettingsModel { FeedUrl = "https://ornek.com/feed" });
        public Task SaveSettingsAsync(UpdateSettingsModel settings) => Task.CompletedTask;
        public Task<UpdateInfo?> CheckForUpdatesAsync(bool includePrereleases = false)
            => Task.FromResult<UpdateInfo?>(null);
        public Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null) => Task.FromResult(false);
        public void ApplyUpdatesAndRestart(params string[] restartArgs) { }
        public void ApplyUpdatesAndRestartWithDatabaseSync(params string[] restartArgs) { }
        public Task<bool> IsUpdatePendingRestartAsync() => Task.FromResult(_bekleyenYenidenBaslatma);
        public Task<bool> PrepareForUpdateAsync() => Task.FromResult(true);
    }

    [Fact]
    public async Task BekleyenYenidenBaslatma_Durumu_Yansir()
    {
        var vm = new UpdateViewModel(new SahteGuncellemeServisi(bekleyenYenidenBaslatma: true), OrtakServisler().Object, null!);

        await vm.InitializeAsync();

        vm.CurrentState.Should().Be(UpdateState.RestartRequired);
    }

    [Fact]
    public async Task BekleyenYoksa_KontrolYolu_Calisir()
    {
        var vm = new UpdateViewModel(new SahteGuncellemeServisi(), OrtakServisler().Object, null!);

        await vm.InitializeAsync();

        vm.CurrentState.Should().Be(UpdateState.Idle);
    }
}
