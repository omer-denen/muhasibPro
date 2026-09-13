using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.CommonServices
{
    /// <summary>
    /// Providersiz modelin (M4 <see cref="DatabaseSettingsModel"/>) firma-kapsamlı
    /// etkili okuması — tek kaynak (Kural 4). Sıra: firma anahtarı → global → default.
    /// </summary>
    public static class FirmaAyarlari
    {
        public static async Task<DatabaseSettingsModel> EtkiliVeritabaniAyariniOkuAsync(
            ILocalSettingsService localSettings, long firmaId)
        {
            try
            {
                if (localSettings != null && firmaId > 0)
                {
                    var ozel = await localSettings.ReadSettingAsync<DatabaseSettingsModel>(
                        FirmaAyarAnahtari.KeyFor(DatabaseSettingsModel.SettingsKey, firmaId));
                    if (ozel != null)
                        return ozel;
                }
            }
            catch { /* global şablona düş */ }
            try
            {
                if (localSettings != null)
                {
                    var global = await localSettings.ReadSettingAsync<DatabaseSettingsModel>(DatabaseSettingsModel.SettingsKey);
                    if (global != null)
                        return global;
                }
            }
            catch { /* fabrika default'una düş */ }
            return new DatabaseSettingsModel();
        }
    }
}
