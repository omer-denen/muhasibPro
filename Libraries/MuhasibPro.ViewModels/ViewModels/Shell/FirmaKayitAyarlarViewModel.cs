using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası "Firma Kayıt" bölümü (M3 EntityRegistry, global şablon).
/// Tek cümle: kayıt modelini panele bağlar (oku + otomatik kaydet + yetki).</summary>
public class FirmaKayitAyarlarViewModel : ViewModelBase
{
    private readonly IEntityRegistrySettingsProvider _saglayici;
    private readonly IAuthenticationService _auth;
    private EntityRegistrySettings _ayarlar = new();
    private bool _yuklendi;

    public FirmaKayitAyarlarViewModel(
        ICommonServices commonServices,
        IEntityRegistrySettingsProvider saglayici = null,
        IAuthenticationService auth = null) : base(commonServices)
    {
        _saglayici = saglayici;
        _auth = auth;
    }

    /// <summary>Kod deseni + sıkı doğrulama yalnız yöneticiye açık (modelde [YoneticiAyari]).</summary>
    public bool IsYonetici { get; private set; }

    public async Task LoadAsync()
    {
        if (_saglayici != null)
            _ayarlar = await _saglayici.GetAsync() ?? new EntityRegistrySettings();
        try
        {
            IsYonetici = AyarYetkiDenetimi.KullaniciYoneticiMi(_auth);
        }
        catch { IsYonetici = false; }
        _yuklendi = true;
        NotifyPropertyChanged(nameof(IsYonetici));
        NotifyPropertyChanged(nameof(FirmaKodPattern));
        NotifyPropertyChanged(nameof(SeciliDefaultDurum));
        NotifyPropertyChanged(nameof(AcikPageSize));
        NotifyPropertyChanged(nameof(ArsivPageSize));
        NotifyPropertyChanged(nameof(ValidationStrict));
    }

    public string FirmaKodPattern
    {
        get => _ayarlar.FirmaKodPattern;
        set { if (_ayarlar.FirmaKodPattern == value) return; _ayarlar.FirmaKodPattern = value ?? string.Empty; NotifyPropertyChanged(nameof(FirmaKodPattern)); _ = SaveAsync(); }
    }

    /// <summary>Yeni mali dönem varsayılan durumu (ComboBox metni ↔ enum, kayıpsız).</summary>
    public List<string> DefaultDurumSecenekleri { get; } = new() { "Açık", "Kapalı", "Arşivli", "Silinmeyi bekliyor", "Silinmiş" };

    public string SeciliDefaultDurum
    {
        get => DurumAdi(_ayarlar.DefaultDurum);
        set
        {
            var durum = DurumParse(value);
            if (_ayarlar.DefaultDurum == durum) return;
            _ayarlar.DefaultDurum = durum;
            NotifyPropertyChanged(nameof(SeciliDefaultDurum));
            _ = SaveAsync();
        }
    }

    public int AcikPageSize
    {
        get => _ayarlar.AcikPageSize;
        set { if (_ayarlar.AcikPageSize == value) return; _ayarlar.AcikPageSize = value; NotifyPropertyChanged(nameof(AcikPageSize)); _ = SaveAsync(); }
    }

    public int ArsivPageSize
    {
        get => _ayarlar.ArsivPageSize;
        set { if (_ayarlar.ArsivPageSize == value) return; _ayarlar.ArsivPageSize = value; NotifyPropertyChanged(nameof(ArsivPageSize)); _ = SaveAsync(); }
    }

    public bool ValidationStrict
    {
        get => _ayarlar.ValidationStrict;
        set { if (_ayarlar.ValidationStrict == value) return; _ayarlar.ValidationStrict = value; NotifyPropertyChanged(nameof(ValidationStrict)); _ = SaveAsync(); }
    }

    public async Task SaveAsync()
    {
        if (!_yuklendi || _saglayici == null)
            return;
        try
        {
            await _saglayici.SaveAsync(_ayarlar);
        }
        catch (UnauthorizedAccessException)
        {
            StatusError("Bu ayar yalnızca yönetici tarafından değiştirilebilir.");
            await LoadAsync();
        }
    }

    private static string DurumAdi(DonemDurum durum) => durum switch
    {
        DonemDurum.Acik => "Açık",
        DonemDurum.Kapali => "Kapalı",
        DonemDurum.Arsivlenmis => "Arşivli",
        DonemDurum.SilinmeBekliyor => "Silinmeyi bekliyor",
        DonemDurum.Silindi => "Silinmiş",
        _ => "Açık"
    };

    private static DonemDurum DurumParse(string ad) => ad switch
    {
        "Kapalı" => DonemDurum.Kapali,
        "Arşivli" => DonemDurum.Arsivlenmis,
        "Silinmeyi bekliyor" => DonemDurum.SilinmeBekliyor,
        "Silinmiş" => DonemDurum.Silindi,
        _ => DonemDurum.Acik
    };
}
