using FluentAssertions;
using MuhasibPro.Business.Services.UIService;
using Velopack.Sources;

namespace MuhasibPro.Tests;

public class UpdateFeedSourceTests
{
    [Fact]
    public void BosAdres_NullDoner()
    {
        UpdateFeedSourceFactory.Create("", false).Should().BeNull();
        UpdateFeedSourceFactory.Create("   ", false).Should().BeNull();
        UpdateFeedSourceFactory.Create(null!, false).Should().BeNull();
    }

    [Fact]
    public void GitHubAdresi_GithubSourceUretir()
    {
        var source = UpdateFeedSourceFactory.Create("https://github.com/kullanici/repo", false);

        source.Should().BeOfType<GithubSource>();
    }

    [Fact]
    public void DuzAdres_SimpleWebSourceUretir()
    {
        var source = UpdateFeedSourceFactory.Create("https://ornek.com/feed", false);

        source.Should().BeOfType<SimpleWebSource>();
    }

    [Fact]
    public void DuzAdres_TimeoutGecerliOlur()
    {
        // Canlı 1.1.1 bulgusu: timeout 0 → HttpClient "must be greater than TimeSpan.Zero".
        var source = UpdateFeedSourceFactory.Create("https://ornek.com/feed", false) as SimpleWebSource;

        source.Should().NotBeNull();
        source!.Timeout.Should().BeGreaterThan(0);
    }
}
