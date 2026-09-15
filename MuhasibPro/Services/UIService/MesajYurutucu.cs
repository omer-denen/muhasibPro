using MuhasibPro.Domain.Enum;
using System.Diagnostics;

namespace MuhasibPro.Services.UIService
{
    /// <summary>
    /// Status çubuğu async işlem yürütücüsü (tek sorumluluk: progress + sonuç akışı).
    /// Mesaj/progress gösterimi dışarıdan delege ile verilir; durum tutmaz.
    /// </summary>
    public sealed class MesajYurutucu
    {
        private readonly Action<string, StatusMessageType, int?> _mesajGoster;
        private readonly Action<string, double> _progressGoster;
        private readonly Action<double> _progressGuncelle;
        private readonly Action _progressGizle;

        public MesajYurutucu(
            Action<string, StatusMessageType, int?> mesajGoster,
            Action<string, double> progressGoster,
            Action<double> progressGuncelle,
            Action progressGizle)
        {
            _mesajGoster = mesajGoster;
            _progressGoster = progressGoster;
            _progressGuncelle = progressGuncelle;
            _progressGizle = progressGizle;
        }

        public async Task ExecuteWithProgressAsync(
            Func<Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null)
        {
            var stopwatch = measureTime ? Stopwatch.StartNew() : null;

            _progressGoster(progressMessage, -1);

            try
            {
                await action();
                stopwatch?.Stop();

                if (!string.IsNullOrEmpty(successMessage))
                {
                    _mesajGoster(OlcumMetni(successMessage, stopwatch, "F3"), StatusMessageType.Success, successAutoHideSeconds);
                    await Task.Delay(300);
                }
                else
                {
                    _progressGizle();
                }
            }
            catch (Exception ex)
            {
                stopwatch?.Stop();
                _mesajGoster(string.IsNullOrEmpty(errorMessage) ? $"Hata: {ex.Message}" : errorMessage, StatusMessageType.Error, -1);
                throw;
            }
        }

        public async Task ExecuteWithProgressAsync(
            Func<IProgress<double>, Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null)
        {
            var stopwatch = measureTime ? Stopwatch.StartNew() : null;

            _progressGoster(progressMessage, 0);

            var progress = new Progress<double>(percent => _progressGuncelle(percent));

            try
            {
                await action(progress);
                stopwatch?.Stop();

                if (!string.IsNullOrEmpty(successMessage))
                {
                    _mesajGoster(OlcumMetni(successMessage, stopwatch, "F3"), StatusMessageType.Success, successAutoHideSeconds);
                    await Task.Delay(300);
                }
                else
                {
                    _progressGizle();
                }
            }
            catch (Exception ex)
            {
                stopwatch?.Stop();
                _mesajGoster(string.IsNullOrEmpty(errorMessage) ? $"Hata: {ex.Message}" : errorMessage, StatusMessageType.Error, -1);
                throw;
            }
        }

        public async Task ExecuteActionAsync(
            Func<Task> action,
            string startMessage = null,
            StatusMessageType startMessageType = StatusMessageType.Info,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null)
        {
            var stopwatch = measureTime ? Stopwatch.StartNew() : null;

            if (!string.IsNullOrEmpty(startMessage))
                _mesajGoster(startMessage, startMessageType, 0);

            await Task.Delay(300);

            try
            {
                await action();
                stopwatch?.Stop();

                if (!string.IsNullOrEmpty(successMessage))
                    _mesajGoster(OlcumMetni(successMessage, stopwatch, "F1"), StatusMessageType.Success, successAutoHideSeconds);
            }
            catch (Exception ex)
            {
                stopwatch?.Stop();
                _mesajGoster(string.IsNullOrEmpty(errorMessage) ? $"İşlem başarısız: {ex.Message}" : errorMessage, StatusMessageType.Error, -1);
                throw;
            }
        }

        private static string OlcumMetni(string mesaj, Stopwatch stopwatch, string format)
            => stopwatch == null ? mesaj : $"{mesaj} ({stopwatch.Elapsed.TotalSeconds.ToString(format)} saniye)";
    }
}
