using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası "Giriş Güvenliği" bölümünün VM'i (Identity, GLOBAL —
/// kilit eşikleri login-öncesi tüketilir, kullanıcı-anahtarına alınamaz).
/// Tek cümle: Identity modelini panele bağlar (oku + otomatik kaydet + yetki).</summary>
public class IdentityAyarlarViewModel : ViewModelBase
{
    private readonly IIdentitySettingsProvider _saglayici;
    private readonly IAuthenticationService _auth;
    private IdentitySettings _ayarlar = new();
    private bool _yuklendi;

    public IdentityAyarlarViewModel(
        ICommonServices commonServices,
        IIdentitySettingsProvider saglayici = null,
        IAuthenticationService auth = null) : base(commonServices)
    {
        _saglayici = saglayici;
        _auth = auth;
    }

    /// <summary>Kritik satırlar (kilit/hash) yalnız yöneticiye açık.</summary>
    public bool IsYonetici { get; private set; }

    public async Task LoadAsync()
    {
        if (_saglayici != null)
            _ayarlar = await _saglayici.GetAsync() ?? new IdentitySettings();
        try
        {
            IsYonetici = AyarYetkiDenetimi.KullaniciYoneticiMi(_auth);
        }
        catch { IsYonetici = false; }
        _yuklendi = true;
        NotifyPropertyChanged(nameof(IsYonetici));
        NotifyPropertyChanged(nameof(MaxFailedAttempts));
        NotifyPropertyChanged(nameof(LockoutMinutes));
        NotifyPropertyChanged(nameof(AttemptWindowMinutes));
        NotifyPropertyChanged(nameof(Pbkdf2Iterations));
        NotifyPropertyChanged(nameof(MinPasswordLength));
    }

    public int MaxFailedAttempts
    {
        get => _ayarlar.MaxFailedAttempts;
        set { if (_ayarlar.MaxFailedAttempts == value) return; _ayarlar.MaxFailedAttempts = value; NotifyPropertyChanged(nameof(MaxFailedAttempts)); _ = SaveAsync(); }
    }

    public int LockoutMinutes
    {
        get => _ayarlar.LockoutMinutes;
        set { if (_ayarlar.LockoutMinutes == value) return; _ayarlar.LockoutMinutes = value; NotifyPropertyChanged(nameof(LockoutMinutes)); _ = SaveAsync(); }
    }

    public int AttemptWindowMinutes
    {
        get => _ayarlar.AttemptWindowMinutes;
        set { if (_ayarlar.AttemptWindowMinutes == value) return; _ayarlar.AttemptWindowMinutes = value; NotifyPropertyChanged(nameof(AttemptWindowMinutes)); _ = SaveAsync(); }
    }

    public int Pbkdf2Iterations
    {
        get => _ayarlar.Pbkdf2Iterations;
        set { if (_ayarlar.Pbkdf2Iterations == value) return; _ayarlar.Pbkdf2Iterations = value; NotifyPropertyChanged(nameof(Pbkdf2Iterations)); _ = SaveAsync(); }
    }

    public int MinPasswordLength
    {
        get => _ayarlar.MinPasswordLength;
        set { if (_ayarlar.MinPasswordLength == value) return; _ayarlar.MinPasswordLength = value; NotifyPropertyChanged(nameof(MinPasswordLength)); _ = SaveAsync(); }
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
