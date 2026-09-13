using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.CommonServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.Common;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Saklama bölümü: yedek limiti + 3 bakım anahtarı. Tek iş: saklama kuralları.</summary>
public class AyarSaklamaViewModel : AyarBolumViewModel
{
    private readonly ILocalSettingsService _localSettings;
    private readonly IAuthenticationService _auth;

    public AyarSaklamaViewModel(
        ICommonServices commonServices,
        IAyarHatti hat,
        ILocalSettingsService localSettings = null,
        IAuthenticationService auth = null)
        : base(commonServices, hat)
    {
        _localSettings = localSettings;
        _auth = auth;
    }

    private int _maxManuelYedekSayisi = 5;
    public int MaxManuelYedekSayisi
    {
        get => _maxManuelYedekSayisi;
        set => AlanDegisti(ref _maxManuelYedekSayisi, Math.Clamp(value, 1, 20), () => SaklamaKaydetAsync());
    }

    private bool _otomatikTemizlemeAcik = true;
    public bool OtomatikTemizlemeAcik
    {
        get => _otomatikTemizlemeAcik;
        set => AlanDegisti(ref _otomatikTemizlemeAcik, value, () => SaklamaKaydetAsync());
    }

    private bool _kapanistaOtomatikYedek;
    public bool KapanistaOtomatikYedek
    {
        get => _kapanistaOtomatikYedek;
        set => AlanDegisti(ref _kapanistaOtomatikYedek, value, () => SaklamaKaydetAsync());
    }

    private bool _haftalikButunlukKontrolu = true;
    public bool HaftalikButunlukKontrolu
    {
        get => _haftalikButunlukKontrolu;
        set => AlanDegisti(ref _haftalikButunlukKontrolu, value, () => SaklamaKaydetAsync());
    }

    private bool _isYonetici;
    /// <summary>Kritik alan (saklama limiti) yalnızca yöneticiye açık.</summary>
    public bool IsYonetici
    {
        get => _isYonetici;
        private set => Set(ref _isYonetici, value);
    }

    private SaklamaAnligi _snapshot = new(5, true, false, true);

    /// <summary>Ham kayıtdan sapma (preset-dışı değer de özel sayılır).</summary>
    public bool OzelMi => !_snapshot.Equals(new SaklamaAnligi(5, true, false, true));

    public override async Task YukleAsync(long firmaId)
    {
        await base.YukleAsync(firmaId);
        IsYonetici = AyarYetkiDenetimi.KullaniciYoneticiMi(_auth);
        var db = await DbEtkiliAsync();
        _snapshot = new SaklamaAnligi(db.GetManuelKeep(), db.OtomatikTemizlemeAcik,
            db.KapanistaOtomatikYedek, db.HaftalikButunlukKontrolu);
        _maxManuelYedekSayisi = AyarDestek.EnYakinPreset(_snapshot.Keep, AyarDestek.SaklamaPresetleri);
        _otomatikTemizlemeAcik = _snapshot.OtoTemiz;
        _kapanistaOtomatikYedek = _snapshot.Kapanis;
        _haftalikButunlukKontrolu = _snapshot.Haftalik;
        NotifyPropertyChanged(nameof(MaxManuelYedekSayisi));
        NotifyPropertyChanged(nameof(OtomatikTemizlemeAcik));
        NotifyPropertyChanged(nameof(KapanistaOtomatikYedek));
        NotifyPropertyChanged(nameof(HaftalikButunlukKontrolu));
    }

    private async Task SaklamaKaydetAsync(bool zorla = false)
    {
        if (_localSettings == null)
            return;
        try
        {
            var guncel = await DbEtkiliAsync();
            if (!zorla && !_snapshot.Equals(new SaklamaAnligi(
                guncel.GetManuelKeep(),
                guncel.OtomatikTemizlemeAcik,
                guncel.KapanistaOtomatikYedek,
                guncel.HaftalikButunlukKontrolu)))
            {
                Hat.CakismaGoster(AyarDestek.CakismaMetni(null, null, guncel),
                    () => SaklamaKaydetAsync(true), this);
                return;
            }
            guncel.MaxManuelYedekSayisi = MaxManuelYedekSayisi;
            guncel.OtomatikTemizlemeAcik = OtomatikTemizlemeAcik;
            guncel.KapanistaOtomatikYedek = KapanistaOtomatikYedek;
            guncel.HaftalikButunlukKontrolu = HaftalikButunlukKontrolu;
            AyarDestek.Damgala(guncel, Hat.GorunenAd());
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(guncel,
                await DbEtkiliAsync(), _auth);
            await _localSettings.SaveSettingAsync(FirmaAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, FirmaId), guncel);
            Hat.Yayinla(FirmaAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, FirmaId));
            _snapshot = AnlikDeger();
            Hat.KaydedildiBildir(sayfaBoyutuDegisti: false);
        }
        catch (UnauthorizedAccessException ex)
        {
            Hat.HataBildir(ex.Message);
            await YukleAsync(FirmaId);
        }
        catch (Exception ex)
        {
            Hat.HataBildir(ex.Message);
        }
    }

    private SaklamaAnligi AnlikDeger() => new(
        MaxManuelYedekSayisi, OtomatikTemizlemeAcik, KapanistaOtomatikYedek, HaftalikButunlukKontrolu);

    private Task<DatabaseSettingsModel> DbEtkiliAsync()
        => FirmaAyarlari.EtkiliVeritabaniAyariniOkuAsync(_localSettings, FirmaId);

    private sealed record SaklamaAnligi(int Keep, bool OtoTemiz, bool Kapanis, bool Haftalik);
}
