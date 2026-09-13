using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.CommonServices;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Liste bölümü: 4 sayfa boyutu + Tenant/Entity kaydı. Tek iş: liste sayfalaması.</summary>
public class AyarListeViewModel : AyarBolumViewModel
{
    private readonly ITenantSettingsProvider _tenantAyarlari;
    private readonly IEntityRegistrySettingsProvider _entityAyarlari;
    private readonly ILocalSettingsService _localSettings;

    public AyarListeViewModel(
        ICommonServices commonServices,
        IAyarHatti hat,
        ITenantSettingsProvider tenantAyarlari = null,
        IEntityRegistrySettingsProvider entityAyarlari = null,
        ILocalSettingsService localSettings = null)
        : base(commonServices, hat)
    {
        _tenantAyarlari = tenantAyarlari;
        _entityAyarlari = entityAyarlari;
        _localSettings = localSettings;
    }

    private int _acikPageSize = 8;
    public int AcikPageSize
    {
        get => _acikPageSize;
        set => AlanDegisti(ref _acikPageSize, Math.Clamp(value, 1, 50), () => EntitySayfaKaydetAsync());
    }

    private int _arsivPageSize = 5;
    public int ArsivPageSize
    {
        get => _arsivPageSize;
        set => AlanDegisti(ref _arsivPageSize, Math.Clamp(value, 1, 50), () => EntitySayfaKaydetAsync());
    }

    private int _yedekPageSize = 4;
    public int YedekPageSize
    {
        get => _yedekPageSize;
        set => AlanDegisti(ref _yedekPageSize, Math.Clamp(value, 1, 50), () => TenantSayfaKaydetAsync());
    }

    private int _bilinmeyenPageSize = 4;
    public int BilinmeyenPageSize
    {
        get => _bilinmeyenPageSize;
        set => AlanDegisti(ref _bilinmeyenPageSize, Math.Clamp(value, 1, 50), () => TenantSayfaKaydetAsync());
    }

    private ListeAnligi _snapshot = new(8, 5, 4, 4);

    /// <summary>Ham kayıtdan sapma (preset-dışı değer de özel sayılır).</summary>
    public bool OzelMi => !_snapshot.Equals(new ListeAnligi(8, 5, 4, 4));

    public override async Task YukleAsync(long firmaId)
    {
        await base.YukleAsync(firmaId);
        var tenant = await TenantEtkiliAsync();
        var entity = await EntityEtkiliAsync();
        _snapshot = new ListeAnligi(entity.GetAcikPageSize(), entity.GetArsivPageSize(),
            tenant.GetYedekPageSize(), tenant.GetBilinmeyenPageSize());
        _acikPageSize = AyarDestek.EnYakinPreset(_snapshot.Acik, AyarDestek.SayfaPresetleri);
        _arsivPageSize = AyarDestek.EnYakinPreset(_snapshot.Arsiv, AyarDestek.SayfaPresetleri);
        _yedekPageSize = AyarDestek.EnYakinPreset(_snapshot.Yedek, AyarDestek.SayfaPresetleri);
        _bilinmeyenPageSize = AyarDestek.EnYakinPreset(_snapshot.Bilinmeyen, AyarDestek.SayfaPresetleri);
        NotifyPropertyChanged(nameof(AcikPageSize));
        NotifyPropertyChanged(nameof(ArsivPageSize));
        NotifyPropertyChanged(nameof(YedekPageSize));
        NotifyPropertyChanged(nameof(BilinmeyenPageSize));
    }

    private async Task TenantSayfaKaydetAsync(bool zorla = false)
    {
        if (_tenantAyarlari == null)
            return;
        try
        {
            var guncel = await _tenantAyarlari.GetAsync(FirmaId);
            if (!zorla && !_snapshot.Equals(new ListeAnligi(
                _snapshot.Acik, _snapshot.Arsiv,
                guncel.GetYedekPageSize(), guncel.GetBilinmeyenPageSize())))
            {
                Hat.CakismaGoster(AyarDestek.CakismaMetni(guncel, null, null),
                    () => TenantSayfaKaydetAsync(true), this);
                return;
            }
            guncel.YedekPageSize = YedekPageSize;
            guncel.BilinmeyenPageSize = BilinmeyenPageSize;
            AyarDestek.Damgala(guncel, Hat.GorunenAd());
            await _tenantAyarlari.SaveAsync(guncel, FirmaId);
            Hat.Yayinla(FirmaAyarAnahtari.KeyFor(TenantSettings.SettingsKey, FirmaId));
            _snapshot = AnlikDeger();
            Hat.KaydedildiBildir(sayfaBoyutuDegisti: true);
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

    private async Task EntitySayfaKaydetAsync(bool zorla = false)
    {
        if (_entityAyarlari == null)
            return;
        try
        {
            var guncel = await _entityAyarlari.GetAsync(FirmaId);
            if (!zorla && !_snapshot.Equals(new ListeAnligi(
                guncel.GetAcikPageSize(), guncel.GetArsivPageSize(),
                _snapshot.Yedek, _snapshot.Bilinmeyen)))
            {
                Hat.CakismaGoster(AyarDestek.CakismaMetni(null, guncel, null),
                    () => EntitySayfaKaydetAsync(true), this);
                return;
            }
            guncel.AcikPageSize = AcikPageSize;
            guncel.ArsivPageSize = ArsivPageSize;
            AyarDestek.Damgala(guncel, Hat.GorunenAd());
            await _entityAyarlari.SaveAsync(guncel, FirmaId);
            Hat.Yayinla(FirmaAyarAnahtari.KeyFor(EntityRegistrySettings.SettingsKey, FirmaId));
            _snapshot = AnlikDeger();
            Hat.KaydedildiBildir(sayfaBoyutuDegisti: true);
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

    private ListeAnligi AnlikDeger() => new(AcikPageSize, ArsivPageSize, YedekPageSize, BilinmeyenPageSize);

    private Task<TenantSettings> TenantEtkiliAsync()
        => AyarDestek.TenantEtkiliOkuAsync(_tenantAyarlari, _localSettings, FirmaId);

    private Task<EntityRegistrySettings> EntityEtkiliAsync()
        => AyarDestek.EntityEtkiliOkuAsync(_entityAyarlari, _localSettings, FirmaId);

    private sealed record ListeAnligi(int Acik, int Arsiv, int Yedek, int Bilinmeyen);
}
