using MuhasibPro.Domain.Enum;

namespace MuhasibPro.ViewModels.ViewModels.Settings;

/// <summary>Güncelleme ekran metinleri/ikonları/görünürlükleri (presentation plane).
/// Tek cümle: UpdateState + ham değerlerden saf metin üretir, servis/VM bilmez.</summary>
public static class UpdateUiMetinleri
{
    public static string GetStatusText(UpdateState state) => state switch
    {
        UpdateState.Idle => "Güncelsiniz",
        UpdateState.Checking => "Güncelleştirmeler denetleniyor...",
        UpdateState.UpdateAvailable => "Bir güncelleştirme hazır",
        UpdateState.Downloading => "İndiriliyor...",
        UpdateState.Downloaded => "Yüklemeye hazır",
        UpdateState.Installing => "Yükleniyor...",
        UpdateState.RestartRequired => "Yeniden başlatma bekleniyor",
        UpdateState.Error => "Bir sorun oluştu",
        _ => "Güncelsiniz"
    };

    public static string GetVersionText(UpdateState state, string version) => state switch
    {
        UpdateState.UpdateAvailable when !string.IsNullOrEmpty(version) => $"v{version} hazır",
        UpdateState.Downloaded when !string.IsNullOrEmpty(version) => $"v{version} indirildi",
        UpdateState.Error => "Uygulama güncellenemedi",
        _ => "Lütfen bekleyin..."
    };

    public static string GetButtonText(UpdateState state) => state switch
    {
        UpdateState.Idle => "Kontrol Et",
        UpdateState.Checking => "Kontrol Ediliyor...",
        UpdateState.UpdateAvailable => "İndir",
        UpdateState.Downloading => "İndiriliyor...",
        UpdateState.Downloaded => "Yükle ve Yeniden Başlat",
        UpdateState.Installing => "Yükleniyor...",
        UpdateState.RestartRequired => "Yeniden Başlatılıyor...",
        UpdateState.Error => "Tekrar Dene",
        _ => "Kontrol Et"
    };

    public static bool GetButtonEnabled(UpdateState state) => state switch
    {
        UpdateState.Checking => false,
        UpdateState.Downloading => false,
        UpdateState.Installing => false,
        UpdateState.RestartRequired => false,
        _ => true
    };

    public static bool ShouldShowUpdateCard(UpdateState state) => state switch
    {
        UpdateState.UpdateAvailable or
        UpdateState.Downloading or
        UpdateState.Downloaded or
        UpdateState.Installing or
        UpdateState.RestartRequired or
        UpdateState.Error => true,
        _ => false,
    };

    public static bool ShouldShowDetails(UpdateState state)
        => state == UpdateState.UpdateAvailable
            || state == UpdateState.Downloaded
            || state == UpdateState.Downloading;

    public static bool IsProgressVisible(UpdateState state)
        => state == UpdateState.Downloading
            || state == UpdateState.Installing;

    public static string GetStatusIcon(UpdateState state) => state switch
    {
        UpdateState.Idle or UpdateState.RestartRequired => "\uE930",
        UpdateState.Checking or UpdateState.Downloading or UpdateState.Installing => "\uE895",
        UpdateState.UpdateAvailable or UpdateState.Downloaded => "\uE946",
        UpdateState.Error => "\uE783",
        _ => "\uE946",
    };

    /// <summary>Manuel feed GitHub dışıysa değişiklik günlüğü verilemez (boş döner).</summary>
    public static string GetChangelogUrl(string feedUrl, string version)
    {
        var feed = feedUrl?.Trim().TrimEnd('/');
        if (!string.IsNullOrEmpty(feed)
            && !string.IsNullOrEmpty(version)
            && feed.Contains("github.com", StringComparison.OrdinalIgnoreCase))
            return $"{feed}/releases/tag/v{version}";
        return string.Empty;
    }

    public static string FormatLastCheckText(DateTime? lastCheck)
    {
        if (lastCheck == null)
            return "Son denetleme: Hiçbir zaman";
        var timeAgo = DateTime.Now - lastCheck.Value;
        string timeText;
        if (timeAgo.TotalMinutes < 1)
            timeText = "Az önce";
        else if (timeAgo.TotalMinutes < 60)
            timeText = $"{(int)timeAgo.TotalMinutes} dakika önce";
        else if (timeAgo.TotalHours < 24)
            timeText = $"{(int)timeAgo.TotalHours} saat önce";
        else if (timeAgo.TotalDays < 7)
            timeText = $"{(int)timeAgo.TotalDays} gün önce";
        else
            timeText = lastCheck.Value.ToString("dd.MM.yyyy");
        return $"Son denetleme: {timeText}";
    }
}
