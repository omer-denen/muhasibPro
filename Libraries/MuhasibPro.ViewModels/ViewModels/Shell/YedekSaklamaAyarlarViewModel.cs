using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası "Yedek Saklama" bölümünün VM'i (M4 SystemDb, kullanıcı-bazlı).
/// Tek cümle: saklama modelini panele bağlar (oku + otomatik kaydet + yetki).</summary>
public class YedekSaklamaAyarlarViewModel : ViewModelBase
{
    private readonly IDatabaseSettingsProvider _saglayici;
    private readonly IAuthenticationService _auth;
    private DatabaseSettingsModel _ayarlar = new();
    private bool _yuklendi;

    public YedekSaklamaAyarlarViewModel(
        ICommonServices commonServices,
        IDatabaseSettingsProvider saglayici = null,
        IAuthenticationService auth = null) : base(commonServices)
    {
        _saglayici = saglayici;
        _auth = auth;
    }

    /// <summary>Saklama limiti + otomatik temizleme yalnız yöneticiye açık (modelde [YoneticiAyari]).</summary>
    public bool IsYonetici { get; private set; }

    public async Task LoadAsync()
    {
        if (_saglayici != null)
            _ayarlar = await _saglayici.GetAsync() ?? new DatabaseSettingsModel();
        try
        {
            IsYonetici = AyarYetkiDenetimi.KullaniciYoneticiMi(_auth);
        }
        catch { IsYonetici = false; }
        _yuklendi = true;
        NotifyPropertyChanged(nameof(IsYonetici));
        NotifyPropertyChanged(nameof(MaxManuelYedekSayisi));
        NotifyPropertyChanged(nameof(OtomatikTemizlemeAcik));
        NotifyPropertyChanged(nameof(KapanistaOtomatikYedek));
        NotifyPropertyChanged(nameof(WeeklyBackupDays));
    }

    public int MaxManuelYedekSayisi
    {
        get => _ayarlar.MaxManuelYedekSayisi;
        set { if (_ayarlar.MaxManuelYedekSayisi == value) return; _ayarlar.MaxManuelYedekSayisi = value; NotifyPropertyChanged(nameof(MaxManuelYedekSayisi)); _ = SaveAsync(); }
    }

    public bool OtomatikTemizlemeAcik
    {
        get => _ayarlar.OtomatikTemizlemeAcik;
        set { if (_ayarlar.OtomatikTemizlemeAcik == value) return; _ayarlar.OtomatikTemizlemeAcik = value; NotifyPropertyChanged(nameof(OtomatikTemizlemeAcik)); _ = SaveAsync(); }
    }

    public bool KapanistaOtomatikYedek
    {
        get => _ayarlar.KapanistaOtomatikYedek;
        set { if (_ayarlar.KapanistaOtomatikYedek == value) return; _ayarlar.KapanistaOtomatikYedek = value; NotifyPropertyChanged(nameof(KapanistaOtomatikYedek)); _ = SaveAsync(); }
    }

    public int WeeklyBackupDays
    {
        get => _ayarlar.WeeklyBackupDays;
        set { if (_ayarlar.WeeklyBackupDays == value) return; _ayarlar.WeeklyBackupDays = value; NotifyPropertyChanged(nameof(WeeklyBackupDays)); _ = SaveAsync(); }
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
}
