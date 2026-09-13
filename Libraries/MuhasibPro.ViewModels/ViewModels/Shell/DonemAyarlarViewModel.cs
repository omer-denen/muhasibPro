using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası "Dönem" bölümü (M5 Tenant, global şablon).
/// Tek cümle: dönem eşiği modelini panele bağlar (oku + otomatik kaydet + yetki).</summary>
public class DonemAyarlarViewModel : ViewModelBase
{
    private readonly ITenantSettingsProvider _saglayici;
    private readonly IAuthenticationService _auth;
    private TenantSettings _ayarlar = new();
    private bool _yuklendi;

    public DonemAyarlarViewModel(
        ICommonServices commonServices,
        ITenantSettingsProvider saglayici = null,
        IAuthenticationService auth = null) : base(commonServices)
    {
        _saglayici = saglayici;
        _auth = auth;
    }

    /// <summary>Eşik değerleri yalnız yöneticiye açık (modelde [YoneticiAyari]).</summary>
    public bool IsYonetici { get; private set; }

    public async Task LoadAsync()
    {
        if (_saglayici != null)
            _ayarlar = await _saglayici.GetAsync() ?? new TenantSettings();
        try
        {
            IsYonetici = AyarYetkiDenetimi.KullaniciYoneticiMi(_auth);
        }
        catch { IsYonetici = false; }
        _yuklendi = true;
        NotifyPropertyChanged(nameof(IsYonetici));
        NotifyPropertyChanged(nameof(YedekPageSize));
        NotifyPropertyChanged(nameof(BilinmeyenPageSize));
        NotifyPropertyChanged(nameof(BackfillSayfaBoyutu));
        NotifyPropertyChanged(nameof(MigrationRetry));
        NotifyPropertyChanged(nameof(BakimTimeoutSec));
        NotifyPropertyChanged(nameof(CommandTimeoutSec));
        NotifyPropertyChanged(nameof(BusyTimeoutMs));
        NotifyPropertyChanged(nameof(Pooling));
    }

    public int YedekPageSize
    {
        get => _ayarlar.YedekPageSize;
        set { if (_ayarlar.YedekPageSize == value) return; _ayarlar.YedekPageSize = value; NotifyPropertyChanged(nameof(YedekPageSize)); _ = SaveAsync(); }
    }

    public int BilinmeyenPageSize
    {
        get => _ayarlar.BilinmeyenPageSize;
        set { if (_ayarlar.BilinmeyenPageSize == value) return; _ayarlar.BilinmeyenPageSize = value; NotifyPropertyChanged(nameof(BilinmeyenPageSize)); _ = SaveAsync(); }
    }

    public int BackfillSayfaBoyutu
    {
        get => _ayarlar.BackfillSayfaBoyutu;
        set { if (_ayarlar.BackfillSayfaBoyutu == value) return; _ayarlar.BackfillSayfaBoyutu = value; NotifyPropertyChanged(nameof(BackfillSayfaBoyutu)); _ = SaveAsync(); }
    }

    public int MigrationRetry
    {
        get => _ayarlar.MigrationRetry;
        set { if (_ayarlar.MigrationRetry == value) return; _ayarlar.MigrationRetry = value; NotifyPropertyChanged(nameof(MigrationRetry)); _ = SaveAsync(); }
    }

    public int BakimTimeoutSec
    {
        get => _ayarlar.BakimTimeoutSec;
        set { if (_ayarlar.BakimTimeoutSec == value) return; _ayarlar.BakimTimeoutSec = value; NotifyPropertyChanged(nameof(BakimTimeoutSec)); _ = SaveAsync(); }
    }

    public int CommandTimeoutSec
    {
        get => _ayarlar.CommandTimeoutSec;
        set { if (_ayarlar.CommandTimeoutSec == value) return; _ayarlar.CommandTimeoutSec = value; NotifyPropertyChanged(nameof(CommandTimeoutSec)); _ = SaveAsync(); }
    }

    public int BusyTimeoutMs
    {
        get => _ayarlar.BusyTimeoutMs;
        set { if (_ayarlar.BusyTimeoutMs == value) return; _ayarlar.BusyTimeoutMs = value; NotifyPropertyChanged(nameof(BusyTimeoutMs)); _ = SaveAsync(); }
    }

    public bool Pooling
    {
        get => _ayarlar.Pooling;
        set { if (_ayarlar.Pooling == value) return; _ayarlar.Pooling = value; NotifyPropertyChanged(nameof(Pooling)); _ = SaveAsync(); }
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
