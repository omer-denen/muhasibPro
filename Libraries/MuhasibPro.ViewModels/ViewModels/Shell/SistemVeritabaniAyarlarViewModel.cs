using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası "Sistem Veritabanı" bölümünün VM'i (M4 SystemDb, kullanıcı-bazlı).
/// Tek cümle: SQLite bağlantı/sistem modelini panele bağlar (oku + otomatik kaydet + yetki).
/// Not: bu paneldeki 5 alanın tamamı modelde [YoneticiAyari]'dir.</summary>
public class SistemVeritabaniAyarlarViewModel : ViewModelBase
{
    private readonly IDatabaseSettingsProvider _saglayici;
    private readonly IAuthenticationService _auth;
    private DatabaseSettingsModel _ayarlar = new();
    private bool _yuklendi;

    public SistemVeritabaniAyarlarViewModel(
        ICommonServices commonServices,
        IDatabaseSettingsProvider saglayici = null,
        IAuthenticationService auth = null) : base(commonServices)
    {
        _saglayici = saglayici;
        _auth = auth;
    }

    /// <summary>Tüm satırlar yöneticiye açık (modelde 5/5 [YoneticiAyari]).</summary>
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
        NotifyPropertyChanged(nameof(SistemKeepLast));
        NotifyPropertyChanged(nameof(BusyTimeoutMs));
        NotifyPropertyChanged(nameof(JournalMode));
        NotifyPropertyChanged(nameof(Synchronous));
        NotifyPropertyChanged(nameof(VacuumOnBackup));
    }

    public int SistemKeepLast
    {
        get => _ayarlar.SistemKeepLast;
        set { if (_ayarlar.SistemKeepLast == value) return; _ayarlar.SistemKeepLast = value; NotifyPropertyChanged(nameof(SistemKeepLast)); _ = SaveAsync(); }
    }

    public int BusyTimeoutMs
    {
        get => _ayarlar.BusyTimeoutMs;
        set { if (_ayarlar.BusyTimeoutMs == value) return; _ayarlar.BusyTimeoutMs = value; NotifyPropertyChanged(nameof(BusyTimeoutMs)); _ = SaveAsync(); }
    }

    public string JournalMode
    {
        get => _ayarlar.JournalMode;
        set { if (_ayarlar.JournalMode == value) return; _ayarlar.JournalMode = value; NotifyPropertyChanged(nameof(JournalMode)); _ = SaveAsync(); }
    }

    public string Synchronous
    {
        get => _ayarlar.Synchronous;
        set { if (_ayarlar.Synchronous == value) return; _ayarlar.Synchronous = value; NotifyPropertyChanged(nameof(Synchronous)); _ = SaveAsync(); }
    }

    public bool VacuumOnBackup
    {
        get => _ayarlar.VacuumOnBackup;
        set { if (_ayarlar.VacuumOnBackup == value) return; _ayarlar.VacuumOnBackup = value; NotifyPropertyChanged(nameof(VacuumOnBackup)); _ = SaveAsync(); }
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
