using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Services.CommonServices;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Firma ayar dialogu orkestratörü: 3 bölüm VM'ini kurar, başlık/durum/barları tutar.
/// Tek sorumluluk: ekran orkestrasyonu (alan mantığı çocuklarda, kayıt sağlayıcılarda).</summary>
public class YonetimAyarlarViewModel : ViewModelBase, IAyarHatti
{
    private readonly ILocalSettingsService _localSettings;
    private readonly ITenantSettingsProvider _tenantAyarlari;
    private readonly IEntityRegistrySettingsProvider _entityAyarlari;
    private readonly IAuthenticationService _auth;
    private readonly IEventBus _eventBus;

    public YonetimAyarlarViewModel(
        ICommonServices commonServices,
        ILocalSettingsService localSettingsService,
        ITenantSettingsProvider tenantAyarlari = null,
        IEntityRegistrySettingsProvider entityAyarlari = null,
        IAuthenticationService auth = null,
        IEventBus eventBus = null,
        IFirmaService firmaService = null,
        IKullaniciService kullaniciService = null) : base(commonServices)
    {
        _localSettings = localSettingsService;
        _tenantAyarlari = tenantAyarlari;
        _entityAyarlari = entityAyarlari;
        _auth = auth;
        _eventBus = eventBus;
        Genel = new AyarGenelViewModel(commonServices, this, firmaService, kullaniciService);
        Liste = new AyarListeViewModel(commonServices, this, tenantAyarlari, entityAyarlari, localSettingsService);
        Saklama = new AyarSaklamaViewModel(commonServices, this, localSettingsService, auth);
    }

    public long FirmaId { get; set; }

    private string _firmaBasligi = string.Empty;
    public string FirmaBasligi
    {
        get => _firmaBasligi;
        set => Set(ref _firmaBasligi, value);
    }

    public AyarGenelViewModel Genel { get; }
    public AyarListeViewModel Liste { get; }
    public AyarSaklamaViewModel Saklama { get; }

    private bool _isOzel;
    /// <summary>Durum: firma şablondan saptıysa true (Özelleştirilmiş), yoksa false (Varsayılan).</summary>
    public bool IsOzel
    {
        get => _isOzel;
        private set
        {
            if (Set(ref _isOzel, value))
            {
                NotifyPropertyChanged(nameof(RozetMetni));
                NotifyPropertyChanged(nameof(DurumButonMetni));
                NotifyPropertyChanged(nameof(DurumButonIpucu));
            }
        }
    }
    public string RozetMetni => IsOzel ? "Özelleştirilmiş" : "Varsayılan";
    /// <summary>Header-sağ durum butonu metni (rozetin yerini aldı).</summary>
    public string DurumButonMetni => IsOzel ? "Varsayılanlara Dön" : "Varsayılan";
    public string DurumButonIpucu => IsOzel
        ? "Şablon değerlere dön (firma anahtarı sıfırlanır)"
        : "Bu firma şablon değerleri kullanıyor";

    private string _hataMetni = string.Empty;
    public string HataMetni
    {
        get => _hataMetni;
        private set
        {
            if (Set(ref _hataMetni, value))
                NotifyPropertyChanged(nameof(HasHata));
        }
    }
    public bool HasHata => !string.IsNullOrWhiteSpace(HataMetni);

    private string _conflictMetni = string.Empty;
    public string ConflictMetni
    {
        get => _conflictMetni;
        private set
        {
            if (Set(ref _conflictMetni, value))
                NotifyPropertyChanged(nameof(ConflictVar));
        }
    }
    public bool ConflictVar => !string.IsNullOrWhiteSpace(ConflictMetni);
    private Func<Task> _bekleyenYazim;
    private AyarBolumViewModel _cakisanBolum;

    /// <summary>Dialog kapanışında orkestratör listeleri tazelesin mi (sayfa boyutu değiştiyse).</summary>
    public bool SayfaBoyutuDegisti { get; private set; }

    private string _kaydedildiMetni = string.Empty;
    /// <summary>Otomatik kayıt mini bildirimi ("Kaydedildi • HH:mm:ss", 3 sn sonra silinir).</summary>
    public string KaydedildiMetni
    {
        get => _kaydedildiMetni;
        private set
        {
            if (Set(ref _kaydedildiMetni, value))
                NotifyPropertyChanged(nameof(HasKaydedildi));
        }
    }
    public bool HasKaydedildi => !string.IsNullOrWhiteSpace(KaydedildiMetni);
    private int _kaydedildiSeri;

    public async Task LoadAsync()
    {
        await Liste.YukleAsync(FirmaId);
        await Saklama.YukleAsync(FirmaId);
        SayfaBoyutuDegisti = false;
        ConflictMetni = string.Empty;
        HataMetni = string.Empty;
        KaydedildiMetni = string.Empty;
        RozetiTazele();
        await Genel.YukleAsync(FirmaId);
    }

    /// <summary>Firma anahtarına fabrika default'larını yazar (sıfırlama = şablona dönüş).</summary>
    public async Task SifirlaAsync()
    {
        try
        {
            HataMetni = string.Empty;
            ConflictMetni = string.Empty;
            await AyarDestek.VarsayilanlariYazAsync(_tenantAyarlari, _entityAyarlari,
                _localSettings, _auth, FirmaId, AyarDestek.GorunenAd(_auth));
            Yayinla(TenantSettings.SettingsKey);
            Yayinla(EntityRegistrySettings.SettingsKey);
            Yayinla(FirmaAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, FirmaId));
            await Liste.YukleAsync(FirmaId);
            await Saklama.YukleAsync(FirmaId);
            SayfaBoyutuDegisti = true;
            RozetiTazele();
            KaydedildiGoster();
        }
        catch (UnauthorizedAccessException ex)
        {
            HataMetni = ex.Message;
        }
        catch (Exception ex)
        {
            HataMetni = ex.Message;
        }
    }

    /// <summary>Çakışma barı: başkasının değişimini ezip bekleyen yazımı çalıştırır.</summary>
    public async Task OnConflictOverwriteAsync()
    {
        ConflictMetni = string.Empty;
        if (_bekleyenYazim != null)
        {
            var yazim = _bekleyenYazim;
            _bekleyenYazim = null;
            await yazim();
        }
    }

    /// <summary>Çakışma barı: yazımdan vazgeçip çakışan bölümü yeniden yükler.</summary>
    public async Task OnConflictVazgecAsync()
    {
        ConflictMetni = string.Empty;
        _bekleyenYazim = null;
        if (_cakisanBolum != null)
            await _cakisanBolum.YukleAsync(FirmaId);
    }

    private void RozetiTazele() => IsOzel = Liste.OzelMi || Saklama.OzelMi;

    /// <summary>Her başarılı otomatik kayıtta mini bildirim (3 sn sayaçlı, üst üste yazımda sıfırlanır;
    /// temizlik UI thread'ine postalanır — arka thread'den notify yasak).</summary>
    private void KaydedildiGoster()
    {
        KaydedildiMetni = $"Kaydedildi • {DateTime.Now:HH:mm:ss}";
        int seri = ++_kaydedildiSeri;
        var context = SynchronizationContext.Current;
        _ = Task.Delay(3000).ContinueWith(_ =>
        {
            if (seri != _kaydedildiSeri)
                return;
            if (context != null)
                context.Post(_ => KaydedildiMetni = string.Empty, null);
            else
                KaydedildiMetni = string.Empty;
        });
    }

    private void Yayinla(string anahtar)
        => _eventBus?.Publish(this, new AppSettingsChangedEvent(anahtar));

    string IAyarHatti.GorunenAd() => AyarDestek.GorunenAd(_auth);
    void IAyarHatti.Yayinla(string anahtar) => Yayinla(anahtar);
    void IAyarHatti.HataBildir(string metin) => HataMetni = metin;
    void IAyarHatti.HatayiTemizle() => HataMetni = string.Empty;

    void IAyarHatti.CakismaGoster(string metin, Func<Task> bekleyenYazim, AyarBolumViewModel kaynak)
    {
        ConflictMetni = metin;
        _bekleyenYazim = bekleyenYazim;
        _cakisanBolum = kaynak;
    }

    void IAyarHatti.KaydedildiBildir(bool sayfaBoyutuDegisti)
    {
        if (sayfaBoyutuDegisti)
            SayfaBoyutuDegisti = true;
        RozetiTazele();
        KaydedildiGoster();
    }
}
