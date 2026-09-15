using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Services.UIService
{
    /// <summary>
    /// Status çubuğu mesaj servisi — ince facade.
    /// Sorumlulukları: durum (<see cref="StatusMesajDurumu"/>), otomatik gizleme
    /// (<see cref="MesajOtoGizleme"/>) ve async yürütme (<see cref="MesajYurutucu"/>).
    /// Renk/ikon eşlemesi View katmanındadır (tema kaynakları).
    /// </summary>
    public class StatusMessageService : IStatusMessageService
    {
        private readonly StatusMesajDurumu _durum = new();
        private readonly MesajOtoGizleme _otoGizleme;
        private readonly MesajYurutucu _yurutucu;

        public StatusMessageService(IAppPlatformSettingsProvider ayarSaglayici)
        {
            _otoGizleme = new MesajOtoGizleme(ayarSaglayici, () => Dispatch(Clear));
            _yurutucu = new MesajYurutucu(GosterMesaj, GosterProgress, GuncelleProgress, GizleProgress);
            _durum.PropertyChanged += (_, e) => Dispatch(() => PropertyChanged?.Invoke(this, e));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        public string StatusMessage => _durum.StatusMessage;
        public StatusMessageType MessageType => _durum.MessageType;
        public bool IsProgressVisible => _durum.IsProgressVisible;
        public bool IsProgressIndeterminate => _durum.IsProgressIndeterminate;
        public double ProgressValue => _durum.ProgressValue;
        public bool ShowProgressBar => _durum.ShowProgressBar;
        public string ProgressText => _durum.ProgressText;
        #endregion

        #region Basic Methods
        public void ShowMessage(string message, StatusMessageType type, int? autoHideSeconds = null)
            => Dispatch(() =>
            {
                _otoGizleme.Iptal();
                _durum.MesajiAyarla(message, type);
                _otoGizleme.Baslat(autoHideSeconds);
            });

        public void Clear()
            => Dispatch(() =>
            {
                _otoGizleme.Iptal();
                _durum.Temizle();
            });
        #endregion

        #region Progress Methods
        public void ShowProgress(string message, double progressPercent = -1)
            => Dispatch(() =>
            {
                _otoGizleme.Iptal();
                _durum.ProgressGoster(message, progressPercent);
            });

        public void UpdateProgress(double progressPercent)
            => Dispatch(() => _durum.ProgressGuncelle(progressPercent));

        public void HideProgress()
            => Dispatch(() => _durum.ProgressGizle());
        #endregion

        #region Advanced Async Methods
        public Task ExecuteWithProgressAsync(
            Func<Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null)
            => _yurutucu.ExecuteWithProgressAsync(action, progressMessage, successMessage, errorMessage, measureTime, successAutoHideSeconds);

        public Task ExecuteWithProgressAsync(
            Func<IProgress<double>, Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null)
            => _yurutucu.ExecuteWithProgressAsync(action, progressMessage, successMessage, errorMessage, measureTime, successAutoHideSeconds);

        public Task ExecuteActionAsync(
            Func<Task> action,
            string startMessage = null,
            StatusMessageType startMessageType = StatusMessageType.Info,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null)
            => _yurutucu.ExecuteActionAsync(action, startMessage, startMessageType, successMessage, errorMessage, measureTime, successAutoHideSeconds);
        #endregion

        #region Delegates for MesajYurutucu
        private void GosterMesaj(string message, StatusMessageType type, int? autoHideSeconds)
            => ShowMessage(message, type, autoHideSeconds);

        private void GosterProgress(string message, double percent)
            => ShowProgress(message, percent);

        private void GuncelleProgress(double percent)
            => UpdateProgress(percent);

        private void GizleProgress()
            => HideProgress();
        #endregion

        private static void Dispatch(Action action)
        {
            var queue = App._dispatcherQueue;
            if (queue == null || queue.HasThreadAccess)
                action();
            else
                queue.TryEnqueue(() => action());
        }
    }
}
