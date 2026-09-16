using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>
    /// Tenant güncelleme yönlendiricisi (Faz 6.91-E): erişim anında karar + **inline** göç.
    /// Güncelleme yoksa doğrudan geçiş; pending ise onay dialogu sonrası göç motoru
    /// (Yedek → Göç → Doğrulama → oto geri alma) **sayfa açmadan** burada çalışır; sonuç tek bildirimdir.
    /// </summary>
    public class TenantDatabaseUpdateCoordinator
    {
        private readonly IDialogService _dialogs;
        private readonly ITenantDatabaseUpdateService _updateService;
        private readonly TenantUpdateProgressViewModel _progress;

        public TenantDatabaseUpdateCoordinator(
            ICommonServices commonServices,
            ITenantDatabaseUpdateService updateService,
            ITenantSQLiteDatabaseOperationService operations,
            TenantUpdateProgressViewModel progress)
        {
            _dialogs = commonServices.DialogService;
            _updateService = updateService;
            _progress = progress;
            Akis = new TenantUpdateAkisYoneticisi(updateService, operations);
        }

        /// <summary>Inline göç motoru (FirmaShell ilerleme yüzeyi buna bağlanır).</summary>
        public TenantUpdateAkisYoneticisi Akis { get; }

        public async Task EnsureSwitchedAsync(string databaseName, FirmaModel firma, MaliDonemModel maliDonem)
        {
            _progress.BeginCheck();
            Akis.Reset();

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

            // Pending — ön-bilgi dialogu (onay/daha sonra/vazgeç)
            _progress.EndProgress();
            check.FirmaUnvani = firma?.KisaUnvani ?? string.Empty;
            check.MaliYil = maliDonem?.MaliYil ?? 0;
            var decision = await _dialogs.ShowTenantUpdateConfirmAsync(check);
            if (decision != TenantUpdateDecision.SimdiGuncelle)
                return; // DahaSonra/Vazgeç → shell'de kal, başka dönem seçilebilir

            // INLINE göç: sayfa yok; ilerleme Akis üzerinden FirmaShell'de gösterilir (Kural 12).
            await Akis.RunAsync(new TenantDatabaseUpdateArgs
            {
                DatabaseName = databaseName,
                Firma = firma,
                MaliDonem = maliDonem
            }, check);

            if (Akis.IsCompleted && !Akis.HasError)
                await _dialogs.ShowSuccessAsync("Dönem güncellendi", Akis.ResultMessage, "Tamam");
            else
                await _dialogs.ShowErrorAsync("Güncelleme başarısız", Akis.ErrorMessage, "Tamam");
        }
    }
}
