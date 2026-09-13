using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Services.CommonServices;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.Common;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Ayar panellerinin ortak saf mantığı: damga/çakışma-metni/preset/sıfırlama.
/// Durum tutmaz; snapshot her çocuk VM'nindir (paneller arası sahte çakışma olmaz).</summary>
internal static class AyarDestek
{
    /// <summary>Sayfa presetleri (MS kılavuzu: seçenek kümesi ComboBox ile).</summary>
    public static readonly int[] SayfaPresetleri = [2, 4, 8, 12, 20];

    /// <summary>Saklama presetleri (model 1-20 aralığına uyar; sektör 3/7/14 dizisi).</summary>
    public static readonly int[] SaklamaPresetleri = [3, 5, 7, 14, 20];

    /// <summary>Kayıtlı özel değeri en yakın preset'e çeker (liste boş görünmesin).</summary>
    public static int EnYakinPreset(int deger, int[] presetler)
    {
        int enIyi = presetler[0];
        foreach (int aday in presetler)
        {
            if (Math.Abs(aday - deger) < Math.Abs(enIyi - deger))
                enIyi = aday;
        }
        return enIyi;
    }

    public static void Damgala(TenantSettings s, string ad)
    {
        s.SonDegistiren = ad;
        s.SonDegisiklikTarihi = DateTime.Now;
    }

    public static void Damgala(EntityRegistrySettings s, string ad)
    {
        s.SonDegistiren = ad;
        s.SonDegisiklikTarihi = DateTime.Now;
    }

    public static void Damgala(DatabaseSettingsModel s, string ad)
    {
        s.SonDegistiren = ad;
        s.SonDegisiklikTarihi = DateTime.Now;
    }

    /// <summary>Üç modelin en güncel damgası ("Ad • tarih" ya da boş).</summary>
    public static string SonDamga(TenantSettings tenant, EntityRegistrySettings entity, DatabaseSettingsModel db)
    {
        var adaylar = new[]
        {
            (Ad: tenant?.SonDegistiren, Tarih: tenant?.SonDegisiklikTarihi ?? default),
            (Ad: entity?.SonDegistiren, Tarih: entity?.SonDegisiklikTarihi ?? default),
            (Ad: db?.SonDegistiren, Tarih: db?.SonDegisiklikTarihi ?? default)
        };
        var son = adaylar
            .Where(a => !string.IsNullOrWhiteSpace(a.Ad) && a.Tarih != default)
            .OrderByDescending(a => a.Tarih)
            .FirstOrDefault();
        return string.IsNullOrWhiteSpace(son.Ad)
            ? string.Empty
            : $"{son.Ad} • {son.Tarih:g}";
    }

    public static string CakismaMetni(TenantSettings tenant, EntityRegistrySettings entity, DatabaseSettingsModel db)
    {
        var damga = SonDamga(tenant, entity, db);
        return string.IsNullOrWhiteSpace(damga)
            ? "Bu firmanın ayarları başka bir yönetici tarafından değiştirildi. Üzerine yazılsın mı?"
            : $"Bu firmanın ayarları değiştirildi (son: {damga}). Üzerine yazılsın mı?";
    }

    /// <summary>Sağlayıcıdan firma-anahtarlı okuma, yoksa LocalSettings fallback'i.</summary>
    public static async Task<TenantSettings> TenantEtkiliOkuAsync(
        ITenantSettingsProvider provider, ILocalSettingsService localSettings, long firmaId)
    {
        try
        {
            if (provider != null)
                return await provider.GetAsync(firmaId);
            if (localSettings != null)
            {
                var ayar = await localSettings.ReadSettingAsync<TenantSettings>(
                    FirmaAyarAnahtari.KeyFor(TenantSettings.SettingsKey, firmaId))
                    ?? await localSettings.ReadSettingAsync<TenantSettings>(TenantSettings.SettingsKey);
                if (ayar != null)
                    return ayar;
            }
        }
        catch { }
        return new TenantSettings();
    }

    /// <summary>Sağlayıcıdan firma-anahtarlı okuma, yoksa LocalSettings fallback'i.</summary>
    public static async Task<EntityRegistrySettings> EntityEtkiliOkuAsync(
        IEntityRegistrySettingsProvider provider, ILocalSettingsService localSettings, long firmaId)
    {
        try
        {
            if (provider != null)
                return await provider.GetAsync(firmaId);
            if (localSettings != null)
            {
                var ayar = await localSettings.ReadSettingAsync<EntityRegistrySettings>(
                    FirmaAyarAnahtari.KeyFor(EntityRegistrySettings.SettingsKey, firmaId))
                    ?? await localSettings.ReadSettingAsync<EntityRegistrySettings>(EntityRegistrySettings.SettingsKey);
                if (ayar != null)
                    return ayar;
            }
        }
        catch { }
        return new EntityRegistrySettings();
    }

    /// <summary>Görünen kullanıcı adı (damga için; çözülemezse "Bilinmiyor").</summary>
    public static string GorunenAd(IAuthenticationService auth)
    {
        try
        {
            var model = auth?.CurrentAccount?.KullaniciModel;
            var ad = (model?.AdiSoyadi ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(ad))
                return ad;
            if (!string.IsNullOrWhiteSpace(model?.KullaniciAdi))
                return model.KullaniciAdi;
        }
        catch { }
        return "Bilinmiyor";
    }

    /// <summary>Sıfırlama: üç modele fabrika default'u yazar (yayınlama orkestratörde).</summary>
    public static async Task VarsayilanlariYazAsync(
        ITenantSettingsProvider tenantAyarlari,
        IEntityRegistrySettingsProvider entityAyarlari,
        ILocalSettingsService localSettings,
        IAuthenticationService auth,
        long firmaId,
        string ad)
    {
        var tenant = new TenantSettings { SonDegistiren = ad, SonDegisiklikTarihi = DateTime.Now };
        var entity = new EntityRegistrySettings { SonDegistiren = ad, SonDegisiklikTarihi = DateTime.Now };
        var db = new DatabaseSettingsModel { SonDegistiren = ad, SonDegisiklikTarihi = DateTime.Now };
        if (tenantAyarlari != null)
            await tenantAyarlari.SaveAsync(tenant, firmaId);
        if (entityAyarlari != null)
            await entityAyarlari.SaveAsync(entity, firmaId);
        if (localSettings != null)
        {
            AyarYetkiDenetimi.KritikDegisiklikleriDogrula(db,
                await FirmaAyarlari.EtkiliVeritabaniAyariniOkuAsync(localSettings, firmaId), auth);
            await localSettings.SaveSettingAsync(FirmaAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, firmaId), db);
        }
    }
}
