using MuhasibPro.Domain.Enum;
using System.ComponentModel;

namespace MuhasibPro.Business.Contracts.UIServices
{
    /// <summary>
    /// StatusBar mesaj yönetimi için servis.
    /// Renk/ikon eşlemesi View katmanındadır (tema kaynakları); burada yalnız mesaj + tür + progress tutulur.
    /// </summary>
    public interface IStatusMessageService : INotifyPropertyChanged
    {
        #region Properties
        string StatusMessage { get; }
        StatusMessageType MessageType { get; }
        bool IsProgressVisible { get; }
        bool IsProgressIndeterminate { get; }
        double ProgressValue { get; }
        bool ShowProgressBar { get; }
        string ProgressText { get; }
        #endregion

        #region Basic Methods
        /// <summary>
        /// Mesaj göster. <paramref name="autoHideSeconds"/> null ise otomatik gizleme süresi
        /// AppPlatform ayarından (`StatusAutoHideMs`) okunur; &lt;= 0 ise gizlenmez.
        /// </summary>
        void ShowMessage(string message, StatusMessageType type, int? autoHideSeconds = null);

        /// <summary>Mesajı temizle.</summary>
        void Clear();
        #endregion

        #region Progress Methods
        /// <summary>Progress göster (yüzde verilmezse belirsiz).</summary>
        void ShowProgress(string message, double progressPercent = -1);

        /// <summary>Progress değerini güncelle.</summary>
        void UpdateProgress(double progressPercent);

        /// <summary>Progress'i gizle.</summary>
        void HideProgress();
        #endregion

        #region Advanced Async Methods
        Task ExecuteWithProgressAsync(
            Func<Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null);

        Task ExecuteWithProgressAsync(
            Func<IProgress<double>, Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null);

        Task ExecuteActionAsync(
            Func<Task> action,
            string startMessage = null,
            StatusMessageType startMessageType = StatusMessageType.Info,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int? successAutoHideSeconds = null);
        #endregion
    }
}
