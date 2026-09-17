using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.DevServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası bölümleri (Windows Ayarlar dili: sol nav + sağ içerik).</summary>
public enum AyarBolumu
{
    GirisPaneli,
    Gorunum,
    Giris,
    Firma,
    Veritabani,
    Donem,
    Guncelleme,
    /// <summary>Yapay Zeka (Faz 6.92).</summary>
    YapayZeka,
    /// <summary>Geliştirici Araçları (yalnız DEBUG; Faz 6.82).</summary>
    GelistiriciAraclari
}

/// <summary>FirmaShell içi Ayarlar sekmesinin orkestratörü (Denetim Masası).
/// Tek cümle: bölüm durumu + kullanıcı-firma bağlamını tutar, bölüm VM'lerini barındırır.</summary>
public class DenetimMasasiViewModel : ViewModelBase
{
    private readonly IFirmaService _firmaService;
    private readonly IAuthenticationService _auth;
    private readonly IDevModeProvider _devMode;

    /// <summary>Görünüm & Bildirim bölümü (AppPlatform, kullanıcı bazlı).</summary>
    public AppPlatformAyarlarViewModel Gorunum { get; }

    /// <summary>Giriş Güvenliği bölümü (Identity, global).</summary>
    public IdentityAyarlarViewModel Giris { get; }

    /// <summary>Giriş dashboard bölümü (firmalarım + dönemlerim + önerilen).</summary>
    public GirisDashboardViewModel GirisPaneli { get; }

    /// <summary>Yedek Saklama bölümü (M4 SystemDb, kullanıcı-bazlı).</summary>
    public YedekSaklamaAyarlarViewModel YedekSaklama { get; }

    /// <summary>Sistem Veritabanı bölümü (M4 SystemDb, kullanıcı-bazlı).</summary>
    public SistemVeritabaniAyarlarViewModel SistemVeritabani { get; }

    /// <summary>Firma Kayıt bölümü (M3 EntityRegistry, global şablon).</summary>
    public FirmaKayitAyarlarViewModel FirmaKayit { get; }

    /// <summary>Dönem bölümü (M5 Tenant, global şablon).</summary>
    public DonemAyarlarViewModel Donem { get; }

    /// <summary>Yapay Zeka bölümü (Faz 6.92).</summary>
    public YapayZekaAyarlarViewModel YapayZeka { get; }

    /// <summary>Geliştirici Araçları bölümü (yalnız DEBUG; Faz 6.82).</summary>
    public GelistiriciAraclariViewModel GelistiriciAraclari { get; }

    public DenetimMasasiViewModel(
        ICommonServices commonServices,
        IFirmaService firmaService,
        IAppPlatformSettingsProvider saglayici = null,
        IAuthenticationService auth = null,
        IIdentitySettingsProvider kimlikSaglayici = null,
        IDatabaseSettingsProvider veritabaniSaglayici = null,
        IEntityRegistrySettingsProvider kayitSaglayici = null,
        ITenantSettingsProvider donemSaglayici = null,
        ISistemDatabaseService sistemDb = null,
        IUpdateService updateService = null,
        IDevModeProvider devMode = null,
        IDevAraclariService devAraclari = null,
        IYolAciciService yolAcici = null,
        IApplicationPaths appPaths = null,
        ISistemDiagnosticsService diagnosticsService = null,
        IModulTestCalistirici modulTestleri = null,
        IAiAsistanSettingsProvider aiSaglayici = null,
        ISurumOzellikService surumService = null,
        IAsistanSohbetService asistanSohbet = null,
        IYardimBilgiTabani yardimBilgiTabani = null) : base(commonServices)
    {
        _firmaService = firmaService;
        _auth = auth;
        _devMode = devMode;
        Gorunum = new AppPlatformAyarlarViewModel(commonServices, saglayici);
        Giris = new IdentityAyarlarViewModel(commonServices, kimlikSaglayici, auth);
        YedekSaklama = new YedekSaklamaAyarlarViewModel(commonServices, veritabaniSaglayici, auth);
        SistemVeritabani = new SistemVeritabaniAyarlarViewModel(commonServices, veritabaniSaglayici, auth);
        FirmaKayit = new FirmaKayitAyarlarViewModel(commonServices, kayitSaglayici, auth);
        Donem = new DonemAyarlarViewModel(commonServices, donemSaglayici, auth);
        GirisPaneli = new GirisDashboardViewModel(commonServices, firmaService, auth, sistemDb, updateService);
        YapayZeka = new YapayZekaAyarlarViewModel(commonServices, aiSaglayici, auth, surumService, asistanSohbet, yardimBilgiTabani);
        GelistiriciAraclari = new GelistiriciAraclariViewModel(commonServices, devMode, devAraclari, yolAcici, updateService, appPaths, sistemDb, diagnosticsService, modulTestleri, surumService, asistanSohbet, yardimBilgiTabani);
        GirisPaneli.BolumAcildi += b => SeciliBolum = b;
        Menuler = new ObservableCollection<AyarlarNavigationMenu>(AyarlarNavigationMenu.VarsayilanMenuler());
        GorunurMenuleriTazele();
    }

    /// <summary>Sol nav menü kataloğu (9 bölüm).</summary>
    public ObservableCollection<AyarlarNavigationMenu> Menuler { get; }

    /// <summary>NavigationView kaynağı (kullanıcı-bazlı süzülmüş; demo yönetici ile tümü).</summary>
    public ObservableCollection<AyarlarNavigationMenu> GorunurMenuler { get; } = new();

    /// <summary>Nav'da seçili menü (NavigationView SelectedItem iki-yönlü aynası; ağaçta özyinelemeli bulunur).</summary>
    public AyarlarNavigationMenu SeciliMenu
    {
        get => MenuBul(GorunurMenuler, SeciliBolum) ?? MenuBul(Menuler, SeciliBolum);
        set
        {
            if (value == null || value.Bolum == SeciliBolum)
                return;
            SeciliBolum = value.Bolum;
        }
    }

    /// <summary>Menü ağacında bölümü özyinelemeli bulur.</summary>
    private static AyarlarNavigationMenu MenuBul(IEnumerable<AyarlarNavigationMenu> menuler, AyarBolumu bolum)
    {
        if (menuler == null)
            return null;
        foreach (var menu in menuler)
        {
            if (menu == null)
                continue;
            if (menu.Bolum == bolum)
                return menu;
            var alt = MenuBul(menu.AltMenuler, bolum);
            if (alt != null)
                return alt;
        }
        return null;
    }

    private AyarBolumu _seciliBolum = AyarBolumu.GirisPaneli;
    public AyarBolumu SeciliBolum
    {
        get => _seciliBolum;
        set
        {
            if (Set(ref _seciliBolum, value))
            {
                NotifyPropertyChanged(nameof(SeciliMenu));
                NotifyPropertyChanged(nameof(SeciliMenuBaslik));
                NotifyPropertyChanged(nameof(SeciliMenuSimge));
            }
        }
    }

    /// <summary>Başlık — null-güvenli (x:Bind Glyph/Text çökmesin).</summary>
    public string SeciliMenuBaslik => SeciliMenu?.Baslik ?? string.Empty;

    /// <summary>Simge — null-güvenli (FontIcon Glyph boş olmasın).</summary>
    public string SeciliMenuSimge => SeciliMenu?.Simge ?? "\uE713";

    /// <summary>Menü arama metni (Win11 Ayarlar "Bir ayar bulun"; boşken tüm menüler).</summary>
    public string AramaMetni
    {
        get => _aramaMetni;
        set
        {
            if (Set(ref _aramaMetni, value ?? string.Empty))
                GorunurMenuleriTazele();
        }
    }
    private string _aramaMetni = string.Empty;

    /// <summary>Menü görünürlüğünü kullanıcı bağlamı + aramaya göre tazeler.
    /// Demo yönetici ile tüm menüler görünür (admin-dev); kullanıcı-bazlı gizleme kuralları MenuGorunurMu'ya eklenir.</summary>
    public void GorunurMenuleriTazele()
    {
        string arama = (_aramaMetni ?? string.Empty).Trim();
        var yeni = Menuler.Where(m => m != null && MenuAgaciUyarMi(m, arama)).ToList();
        if (!GorunurMenuler.SequenceEqual(yeni))
        {
            GorunurMenuler.Clear();
            foreach (var menu in yeni)
                GorunurMenuler.Add(menu);
        }
        if (MenuBul(GorunurMenuler, SeciliBolum) == null)
        {
            var ilk = GorunurMenuler.FirstOrDefault();
            if (ilk != null)
                SeciliBolum = ilk.Bolum;
        }
        NotifyPropertyChanged(nameof(SeciliMenu));
        NotifyPropertyChanged(nameof(SeciliMenuBaslik));
        NotifyPropertyChanged(nameof(SeciliMenuSimge));
    }

    /// <summary>Üst öğe: kendisi veya herhangi bir alt öğesi görünür + aramaya uyarsa listelenir.</summary>
    private bool MenuAgaciUyarMi(AyarlarNavigationMenu menu, string arama)
    {
        if (menu.AltMenuler == null || menu.AltMenuler.Count == 0)
            return MenuGorunurMu(menu.Bolum) && AramaUyarMi(menu, arama);
        if (MenuGorunurMu(menu.Bolum) && AramaUyarMi(menu, arama))
            return true;
        foreach (var alt in menu.AltMenuler)
        {
            if (alt != null && MenuAgaciUyarMi(alt, arama))
                return true;
        }
        return false;
    }

    /// <summary>Bölümün bu kullanıcıya görünürlüğü (Geliştirici Araçları yalnız DEBUG kapısında).</summary>
    private bool MenuGorunurMu(AyarBolumu bolum)
    {
        if (bolum == AyarBolumu.GelistiriciAraclari)
            return _devMode?.IsEnabled ?? false;
        return true;
    }

    /// <summary>Arama süzgeci (başlık veya açıklamada geçer; boş arama hepsini geçirir).</summary>
    private static bool AramaUyarMi(AyarlarNavigationMenu menu, string arama)
        => string.IsNullOrEmpty(arama)
            || (menu.Baslik ?? string.Empty).Contains(arama, StringComparison.OrdinalIgnoreCase)
            || (menu.Aciklama ?? string.Empty).Contains(arama, StringComparison.OrdinalIgnoreCase);

    private int _firmaSayisi;
    /// <summary>Giriş yapanın kayıtlı firma sayısı (seçim yok, salt sayı).</summary>
    public int FirmaSayisi
    {
        get => _firmaSayisi;
        private set
        {
            if (Set(ref _firmaSayisi, value))
                NotifyPropertyChanged(nameof(FirmaSayisiMetni));
        }
    }

    /// <summary>Hero durum bloğu: kayıtlı firma sayısı.</summary>
    public string FirmaSayisiMetni => $"{FirmaSayisi} kayıtlı";

    /// <summary>Kullanıcı kartı + firma sayısı yükler (seçim yapmaz; firmasız da sekme açılır).</summary>
    public async Task LoadAsync()
    {
        YukleKullaniciKartini();
        FirmaSayisi = await FirmaSayisiniOkuAsync();
        GorunurMenuleriTazele();
    }

    private async Task<int> FirmaSayisiniOkuAsync()
    {
        try
        {
            long kullaniciId = 0;
            try { if (_auth != null && _auth.IsAuthenticated) kullaniciId = _auth.CurrentAccount?.KullaniciId ?? 0; }
            catch { kullaniciId = 0; }
            if (kullaniciId <= 0 || _firmaService == null)
                return 0;
            var sonuc = await _firmaService.GetFirmalarWithUserId(new DataRequest<Firma>(), kullaniciId);
            return sonuc?.Data?.Count(f => f != null) ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private string _kullaniciAdi = string.Empty;
    /// <summary>Sol nav üstü kullanıcı kartı (Windows Ayarlar dili).</summary>
    public string KullaniciAdi
    {
        get => _kullaniciAdi;
        private set => Set(ref _kullaniciAdi, value);
    }

    private string _kullaniciRolMetni = string.Empty;
    public string KullaniciRolMetni
    {
        get => _kullaniciRolMetni;
        private set => Set(ref _kullaniciRolMetni, value);
    }

    /// <summary>Kullanıcı adının baş harfleri (foto yoksa varsayılan avatar).</summary>
    public string KullaniciInitials => string.IsNullOrWhiteSpace(KullaniciAdi)
        ? "?"
        : string.Concat(KullaniciAdi.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));

    private object _profilResmi;
    /// <summary>Profil fotoğrafı (KullaniciModel.ResimSource passthrough; null ise varsayılan avatar).</summary>
    public object ProfilResmi
    {
        get => _profilResmi;
        private set => Set(ref _profilResmi, value);
    }

    private void YukleKullaniciKartini()
    {
        try
        {
            var hesap = _auth != null && _auth.IsAuthenticated ? _auth.CurrentAccount : null;
            var model = hesap?.KullaniciModel;
            KullaniciAdi = model?.KullaniciAdi ?? string.Empty;
            var rol = model?.Rol?.RolAdi ?? model?.Rol?.RolTip.ToString();
            KullaniciRolMetni = string.IsNullOrWhiteSpace(rol) ? "Kullanıcı" : rol;
            ProfilResmi = model?.ResimSource ?? model?.Resim;
            NotifyPropertyChanged(nameof(KullaniciInitials));
        }
        catch
        {
            KullaniciAdi = string.Empty;
            KullaniciRolMetni = "Kullanıcı";
            ProfilResmi = null;
        }
    }
}
