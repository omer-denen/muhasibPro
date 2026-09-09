using Velopack.Sources;

namespace MuhasibPro.Business.Services.UIService;

/// <summary>
/// Manuel feed adresinden Velopack kaynağı üretir (GitHub repo veya düz feed).
/// Boş adreste null döner — servis ağa çıkmadan durur.
/// </summary>
public static class UpdateFeedSourceFactory
{
    public static IUpdateSource Create(string feedUrl, bool includePrereleases)
    {
        if (string.IsNullOrWhiteSpace(feedUrl))
            return null;
        var url = feedUrl.Trim().TrimEnd('/');
        if (url.Contains("github.com", StringComparison.OrdinalIgnoreCase))
            return new GithubSource(url, null, includePrereleases, null);
        return new SimpleWebSource(url, null, 0);
    }
}
