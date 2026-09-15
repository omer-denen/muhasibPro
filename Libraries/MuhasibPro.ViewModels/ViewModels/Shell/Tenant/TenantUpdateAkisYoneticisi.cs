using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using System.Collections.ObjectModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>
    /// Güncelleme akışının motoru: Yedek → Göç → Doğrulama (+hata halinde otomatik geri alma).
    /// Yalnız iş akışını ve adım ilerlemesini yönetir; sayfa (navigation/dialog/bağlam) bilmez.
    /// </summary>
    public class TenantUpdateAkisYoneticisi : ObservableObject
    {
        private readonly ITenantDatabaseUpdateService _updateService;
        private readonly ITenantSQLiteDatabaseOperationService _operations;

        private TenantUpdateStep _backupStep;
        private TenantUpdateStep _migrateStep;
        private TenantUpdateStep _validateStep;
        private TenantUpdateStep _restoreStep;
        private TenantUpdateStep _aktifStep;

        public TenantUpdateAkisYoneticisi(
            ITenantDatabaseUpdateService updateService,
            ITenantSQLiteDatabaseOperationService operations)
        {
            _updateService = updateService;
            _operations = operations;
            Steps = new ObservableCollection<TenantUpdateStep>();
            Reset();
        }

        public ObservableCollection<TenantUpdateStep> Steps { get; }

        private int _progressYuzde;
        public int ProgressYuzde { get => _progressYuzde; private set => Set(ref _progressYuzde, value); }

        private string _aktifAdim = string.Empty;
        public string AktifAdim { get => _aktifAdim; private set => Set(ref _aktifAdim, value); }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; private set => Set(ref _isRunning, value); }

        private bool _isCompleted;
        public bool IsCompleted { get => _isCompleted; private set => Set(ref _isCompleted, value); }

        private bool _restoredFromBackup;
        public bool RestoredFromBackup { get => _restoredFromBackup; private set => Set(ref _restoredFromBackup, value); }

        private string _backupPath = string.Empty;
        public string BackupPath { get => _backupPath; private set => Set(ref _backupPath, value); }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (Set(ref _errorMessage, value))
                    NotifyPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private string _resultMessage = string.Empty;
        public string ResultMessage { get => _resultMessage; private set => Set(ref _resultMessage, value); }

        /// <summary>İşlem başlamadan önceki temiz duruma döner (adımlar yeniden kurulur).</summary>
        public void Reset()
        {
            Steps.Clear();
            _restoreStep = null;
            _aktifStep = null;
            _backupStep = new TenantUpdateStep("1", "Yedek");
            _migrateStep = new TenantUpdateStep("2", "Göç");
            _validateStep = new TenantUpdateStep("3", "Doğrulama");
            Steps.Add(_backupStep);
            Steps.Add(_migrateStep);
            Steps.Add(_validateStep);
            ErrorMessage = string.Empty;
            ResultMessage = string.Empty;
            RestoredFromBackup = false;
            BackupPath = string.Empty;
            IsCompleted = false;
            ProgressYuzde = 0;
            AktifAdim = string.Empty;
        }

        /// <summary>Yedek → Göç → Doğrulama akışını çalıştırır; hata halinde yedekten otomatik geri alır.</summary>
        public async Task RunAsync(TenantDatabaseUpdateArgs args, TenantUpdateCheckResult check)
        {
            IsRunning = true;
            ErrorMessage = string.Empty;
            ResultMessage = string.Empty;
            try
            {
                // 1. Güncelleme öncesi yedek (bilinen geri dönüş noktası)
                _aktifStep = _backupStep;
                _backupStep.MarkRunning("VACUUM INTO ile güvenlik yedeği alınıyor...");
                Basla(5, "Yedek alınıyor");

                var backup = await _operations.CreateBackupAsync(args.DatabaseName, DatabaseBackupType.Migration);
                if (backup?.Success != true || backup.Data == null || !backup.Data.IsBackupComleted || string.IsNullOrEmpty(backup.Data.BackupFilePath))
                {
                    Fail(_backupStep, "Yedek alınamadı: " + (backup?.Message ?? "bilinmeyen hata"));
                    return;
                }
                BackupPath = backup.Data.BackupFilePath;
                _backupStep.MarkDone("Yedek hazır: " + backup.Data.BackupFileName);

                // 2. Göç (yedek-önce-göç + durum yayını servis içinde)
                _aktifStep = _migrateStep;
                _migrateStep.MarkRunning("Göçler uygulanıyor...");
                Basla(30, "Göç uygulanıyor");

                var switched = await _updateService.SwitchAndPublishAsync(args.DatabaseName, args.Firma, args.MaliDonem);
                if (!switched.Success)
                {
                    _migrateStep.MarkFault(switched.ErrorMessage);
                    await AutoRestoreAsync(args, "Geçiş başarısız: " + switched.ErrorMessage);
                    return;
                }
                _migrateStep.MarkDone("Sürüm " + check.TargetVersion + " uygulandı.");

                // 3. Doğrulama
                _aktifStep = _validateStep;
                _validateStep.MarkRunning("Bağlantı + şema + bekleyen göç kontrolü...");
                Basla(70, "Doğrulanıyor");

                if (await _updateService.ValidateAsync(args.DatabaseName))
                {
                    _validateStep.MarkDone("Veritabanı sağlıklı ve güncel.");
                    ProgressYuzde = 100;
                    FinishSuccess(args.MaliDonem.MaliYil + " dönemi güncellendi ve doğrulandı.");
                    return;
                }

                _validateStep.MarkFault("Doğrulama geçilemedi.");
                await AutoRestoreAsync(args, "Doğrulama geçilemedi.");
            }
            catch (Exception ex)
            {
                Fail(_aktifStep, "Güncelleme sırasında beklenmeyen hata: " + ex.Message);
            }
            finally
            {
                IsRunning = false;
                AktifAdim = string.Empty;
            }
        }

        private void Basla(int yuzde, string adim) => (ProgressYuzde, AktifAdim) = (yuzde, adim);

        private async Task AutoRestoreAsync(TenantDatabaseUpdateArgs args, string reason)
        {
            if (_restoreStep == null)
            {
                _restoreStep = new TenantUpdateStep("4", "Geri alma");
                Steps.Add(_restoreStep);
            }
            _aktifStep = _restoreStep;
            _restoreStep.MarkRunning("Güncelleme öncesi yedeğe dönülüyor...");
            Basla(90, "Geri alınıyor");

            var restored = await _operations.RestoreBackupAsync(args.DatabaseName, BackupPath);
            if (restored?.Success == true && restored.Data != null && restored.Data.IsRestoreSuccess
                && await _updateService.ValidateAsync(args.DatabaseName))
            {
                _restoreStep.MarkDone("Yedekten geri alındı ve doğrulandı.");
                RestoredFromBackup = true;
                ProgressYuzde = 100;
                FinishSuccess(reason + " Güncelleme öncesi yedekten geri alındı — veriler korunuyor.");
                return;
            }

            _restoreStep.MarkFault("Geri alma başarısız.");
            Fail(_restoreStep, reason + " Otomatik geri alma da başarısız — Mali Dönem Yönetim → Dönem Yedekleri → Geri Yükle ile manuel alın: " + BackupPath);
        }

        private void FinishSuccess(string message)
        {
            ResultMessage = message;
            IsCompleted = true;
        }

        private void Fail(TenantUpdateStep step, string message)
        {
            if (step != null && !step.IsFaulted)
                step.MarkFault(message);
            ErrorMessage = message;
        }
    }
}
