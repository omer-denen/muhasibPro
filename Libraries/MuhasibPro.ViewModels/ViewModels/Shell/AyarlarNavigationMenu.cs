namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası sol nav menü öğesi (NavigationView kaynağı; kullanıcı-bazlı süzülebilir).
/// Tek cümle: bölüm + başlık + açıklama + simge taşır (salt veri, seçim VM'dedir).</summary>
public class AyarlarNavigationMenu
{
    public AyarBolumu Bolum { get; }

    public string Baslik { get; }

    /// <summary>Kısa açıklama (Ayarlar "Giriş" kategori kartında gösterilir).</summary>
    public string Aciklama { get; }

    /// <summary>Segoe MDL2 glyph (Icons.xaml'daki Icon* karşılığı; VM katmanı kaynak anahtarı bilmez).</summary>
    public string Simge { get; }

    public AyarlarNavigationMenu(AyarBolumu bolum, string baslik, string simge, string aciklama = null,
        IReadOnlyList<AyarlarNavigationMenu> altMenuler = null)
    {
        Bolum = bolum;
        Baslik = baslik;
        Simge = simge;
        Aciklama = aciklama ?? string.Empty;
        AltMenuler = altMenuler ?? Array.Empty<AyarlarNavigationMenu>();
    }

    /// <summary>Alt menüler (ebeveyn grup; yaprakta boş).</summary>
    public IReadOnlyList<AyarlarNavigationMenu> AltMenuler { get; }

    /// <summary>Varsayılan katalog (7 bölüm; görünürlük kuralları VM'de uygulanır).
    /// Simge kodları Icons.xaml'daki Icon* karşılıklarıdır (E80F/E713/E72E/E821/E8B7/EA35/E787/E896).</summary>
    public static IReadOnlyList<AyarlarNavigationMenu> VarsayilanMenuler() => new List<AyarlarNavigationMenu>
    {
        new(AyarBolumu.GirisPaneli, "Giriş", Simgeden(0xE80F), "Hesap, firma ve dönem özeti"),
        new(AyarBolumu.Gorunum, "Görünüm", Simgeden(0xE713), "Tema, açılış ve bildirimler"),
        new(AyarBolumu.Giris, "Güvenlik", Simgeden(0xE72E), "Giriş koruması ve parola kuralları"),
        new(AyarBolumu.Firma, "Firma", Simgeden(0xE821), "Firma kodu ve kayıt kuralları"),
        new(AyarBolumu.Veritabani, "Veritabanı", Simgeden(0xE8B7), "Sistem ve dönem veritabanı, yedek saklama ayarları"),
        new(AyarBolumu.Donem, "Mali Dönem", Simgeden(0xE787), "Liste, güncelleme ve bağlantı ayarları"),
        new(AyarBolumu.Guncelleme, "Güncelleme", Simgeden(0xE896), "Uygulama sürümü ve güncelleme"),
    };

    private static string Simgeden(int kod) => ((char)kod).ToString();
}
