namespace MuhasibPro.Business.Contracts.UIServices.CommonServices;

/// <summary>Bildirim grup/tag sabitleri (Oturum 218 araştırması: Tag+Group replace, MS App Notifications + Toolkit 7.1.2).</summary>
public static class NotificationGroups
{
    public const string Yedek = "Yedek";
    public const string DonemIslemleri = "DonemIslemleri";
    public const string Bakim = "Bakim";
    public const string Analiz = "Analiz";
    public const string Yonetim = "Yonetim";
    public const string Guncelleme = "Guncelleme";
    public const string Sistem = "Sistem";

    public static string Etiket(NotificationType tur) => tur switch
    {
        NotificationType.Success => "Başarılı",
        NotificationType.Warning => "Uyarı",
        NotificationType.Danger => "Hata",
        _ => "Bilgi"
    };

    public static string Baslik(string grup) => grup switch
    {
        Yedek => "Yedekleme",
        DonemIslemleri => "Dönem İşlemleri",
        Bakim => "Bakım",
        Analiz => "Analiz",
        Yonetim => "Mali Dönem Yönetimi",
        Guncelleme => "Güncelleme",
        Sistem => "Sistem",
        _ => "MuhasibPro"
    };
}
