using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Business.Services.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D: Sistem.db güncelleme-sonrası doğrulama.
    /// Sıra (fail-closed): ileri-uyumluluk guard → gerekliyse göç + verify →
    /// göç düşerse güncelleme öncesi yedekten restore → yetmezse blokla.</summary>
    public class SistemDbGocDogrulayici : ISistemDbGocDogrulayici
    {
        private readonly ISistemDatabaseService _sistemDatabaseService;
        private readonly ISistemDatabaseOperationService _sistemOperation;

        public SistemDbGocDogrulayici(
            ISistemDatabaseService sistemDatabaseService,
            ISistemDatabaseOperationService sistemOperation)
        {
            _sistemDatabaseService = sistemDatabaseService;
            _sistemOperation = sistemOperation;
        }

        public async Task<DogrulamaAdimSonucu> DogrulaAsync(string? preUpdateYedekYolu, CancellationToken cancellationToken = default)
        {
            var ilk = await DurumAsync();
            if (ilk == null)
                return DogrulamaAdimSonucu.Block("Sistem.db durumu okunamadı.");

            if (!ilk.IsDatabaseExists)
                return DogrulamaAdimSonucu.Ok("Sistem.db bulunamadı — ilk kurulum akışına bırakıldı.");

            // İleri-uyumluluk guard: disk şema > binary → göç/yazma yok (Faz 6.91-C kuralı).
            if (ilk.IsFutureSchema)
                return DogrulamaAdimSonucu.Block(ilk.GetStatusMessage());

            bool mudahaleGerekli = ilk.IsUpdateRequired || ilk.HasError || !ilk.CanConnect || !ilk.DatabaseValid;
            if (!mudahaleGerekli)
                return DogrulamaAdimSonucu.Ok("Sistem.db sağlıklı ve güncel.");

            // 1) Göç (manager içinde yedek-önce + rollback var).
            var (gocOk, gocMesaj) = await _sistemDatabaseService.InitializeSistemDatabaseAsync();
            var gocSonrasi = await DurumAsync();
            if (gocOk && Saglikli(gocSonrasi))
                return DogrulamaAdimSonucu.Ok("Sistem.db güncellendi ve doğrulandı.");

            // 2) Göç düştü → güncelleme öncesi doğrulanmış yedekten restore + tekrar göç.
            if (string.IsNullOrWhiteSpace(preUpdateYedekYolu) || !File.Exists(preUpdateYedekYolu))
                return DogrulamaAdimSonucu.Block($"Sistem.db güncellenemedi ({gocMesaj}) ve geri alınacak yedek bulunamadı.");

            var restore = await _sistemOperation.RestoreBackupAsync(preUpdateYedekYolu);
            var restoreSonrasi = await DurumAsync();
            if (restore?.Data?.IsRestoreSuccess != true || !Saglikli(restoreSonrasi))
                return DogrulamaAdimSonucu.Block("Sistem.db güncellenemedi ve yedekten geri alma da doğrulanamadı.");

            if (restoreSonrasi!.IsUpdateRequired)
            {
                var (retryOk, retryMesaj) = await _sistemDatabaseService.InitializeSistemDatabaseAsync();
                var retrySonrasi = await DurumAsync();
                if (!retryOk || !Saglikli(retrySonrasi))
                    return DogrulamaAdimSonucu.Block($"Sistem.db yedekten alındı ancak güncelleme yine başarısız: {retryMesaj}");
            }

            return DogrulamaAdimSonucu.Warn("Sistem.db güncellenemedi; güncelleme öncesi yedekten geri alınıp doğrulandı.");
        }

        private async Task<DatabaseConnectionAnalysis?> DurumAsync()
        {
            var resp = await _sistemDatabaseService.GetSistemDatabaseStateAsync();
            return resp?.Data;
        }

        private static bool Saglikli(DatabaseConnectionAnalysis? analiz)
            => analiz != null
               && analiz.IsDatabaseExists
               && analiz.CanConnect
               && !analiz.HasError
               && analiz.DatabaseValid
               && !analiz.IsFutureSchema;
    }
}
