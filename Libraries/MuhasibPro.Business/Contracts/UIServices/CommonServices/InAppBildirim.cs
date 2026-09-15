namespace MuhasibPro.Business.Contracts.UIServices.CommonServices;

/// <summary>
/// Uygulama içi bildirim modeli (InfoBar host'u tüketir; OS toast terk edildi — Oturum 270 kararı).
/// </summary>
public sealed class InAppBildirim
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Baslik { get; set; } = string.Empty;

    public string Mesaj { get; set; } = string.Empty;

    public NotificationType Tur { get; set; } = NotificationType.Info;

    /// <summary>Kimlik etiketi; aynı etiket+grup yeni bildirimle değiştirilir (üst üste dizilme kapanır).</summary>
    public string Etiket { get; set; } = string.Empty;

    public string Grup { get; set; } = string.Empty;

    public DateTime Olusturma { get; set; } = DateTime.Now;
}
