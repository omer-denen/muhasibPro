using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>
    /// Tenant güncelleme yönlendiricisi: güncelleme yoksa doğrudan geçiş,
    /// varsa ön-bilgi dialogu (Güncelle → sayfa, Daha sonra/Vazgeç → shell'de kal).
    /// Göç/doğrulama/geri alma güncelleme sayfasındadır, burada dialog/progress dışında iş yok.
    /// </summary>
    public class TenantDatabaseUpdateCoordinator
    {
        private readonly IDialogService _dialogs;
        private readonly INavigationService _navigation;
        private readonly ITenantDatabaseUpdateService _updateService;
        private readonly TenantUpdateProgressViewModel _progress;

        public TenantDatabaseUpdateCoordinator(
            ICommonServices commonServices,
            ITenantDatabaseUpdateService updateService,
            TenantUpdateProgressViewModel progress)
        {
            _dialogs = commonServices.DialogService;
            _navigation = commonServices.NavigationService;
            _updateService = updateService;
            _progress = progress;
        }

        public async Task EnsureSwitchedAsync(string databaseName, FirmaModel firma, MaliDonemModel maliDonem)
        {
            _progress.BeginCheck();

            var check = await _updateService.CheckUpdateRequiredAsync(databaseName);
            if (!check.CheckSucceeded || !check.NeedsUpdate)
            {
                // Güncelleme yok — doğrudan geçiş (yedek-önce-göç + durum yayını servis içinde)
                _progress.BeginSwitch();
                var result = await _updateService.SwitchAndPublishAsync(databaseName, firma, maliDonem);
                _progress.EndProgress();

                if (!result.Success)
                {
                    // C3 fix: "Geri Al" butonu gerçek restore yapmıyor — dürüst bilgi ver.
                    await _dialogs.ShowAsync("Geçiş Hatası",
                        $"{result.ErrorMessage}\n\nMali Dönem Yönetim → Dönem Yedekleri → Geri Yükle ile en son yedeği geri yükleyebilirsiniz.",
                        "Tamam");
                }
                return;
            }

            // Güncelleme var — ön-bilgi dialogu (firma/dönem ile), karar sayfaya taşır
            _progress.EndProgress();
            check.FirmaUnvani = firma?.KisaUnvani ?? string.Empty;
            check.MaliYil = maliDonem?.MaliYil ?? 0;
            var decision = await _dialogs.ShowTenantUpdateConfirmAsync(check);
            if (decision != TenantUpdateDecision.SimdiGuncelle)
                return; // DahaSonra/Vazgeç → shell'de kal, güncel başka dönem seçilebilir

            await _navigation.CreateNewViewAsync<TenantDatabaseUpdateViewModel>(new ShellArgs
            {
                Parameter = new TenantDatabaseUpdateArgs
                {
                    DatabaseName = databaseName,
                    Firma = firma,
                    MaliDonem = maliDonem
                }
            }, "Veritabanı Güncelleme");
        }
    }
}
