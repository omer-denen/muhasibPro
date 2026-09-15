using MuhasibPro.Domain.Enum;
using System.ComponentModel;

namespace MuhasibPro.Services.UIService
{
    /// <summary>
    /// Status çubuğu mesaj/progress durumu (tek sorumluluk: durum tut + değişimi bildir).
    /// UI/renk bilmez; tema ve ikon eşlemesi View katmanındadır.
    /// Property adları <see cref="MuhasibPro.Business.Contracts.UIServices.IStatusMessageService"/> ile hizalıdır.
    /// </summary>
    public sealed class StatusMesajDurumu : INotifyPropertyChanged
    {
        private string _statusMessage = "Hazır";
        private StatusMessageType _messageType = StatusMessageType.Info;
        private bool _isProgressVisible;
        private bool _isProgressIndeterminate = true;
        private double _progressValue;

        public event PropertyChangedEventHandler PropertyChanged;

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage == value) return;
                _statusMessage = value;
                Raise(nameof(StatusMessage));
            }
        }

        public StatusMessageType MessageType
        {
            get => _messageType;
            private set
            {
                if (_messageType == value) return;
                _messageType = value;
                Raise(nameof(MessageType));
            }
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            private set
            {
                if (_isProgressVisible == value) return;
                _isProgressVisible = value;
                Raise(nameof(IsProgressVisible));
                Raise(nameof(ShowProgressBar));
            }
        }

        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            private set
            {
                if (_isProgressIndeterminate == value) return;
                _isProgressIndeterminate = value;
                Raise(nameof(IsProgressIndeterminate));
                Raise(nameof(ShowProgressBar));
                Raise(nameof(ProgressText));
            }
        }

        public double ProgressValue
        {
            get => _progressValue;
            private set
            {
                var yeni = Math.Clamp(value, 0, 100);
                if (Math.Abs(_progressValue - yeni) <= 0.01) return;
                _progressValue = yeni;
                Raise(nameof(ProgressValue));
                Raise(nameof(ProgressText));
            }
        }

        public bool ShowProgressBar => IsProgressVisible && !IsProgressIndeterminate;

        public string ProgressText => IsProgressIndeterminate ? string.Empty : $"{ProgressValue:F0}%";

        public void MesajiAyarla(string message, StatusMessageType tur)
        {
            StatusMessage = Sanitize(message);
            MessageType = tur;
            ProgressGizle();
        }

        public void ProgressGoster(string message, double percent)
        {
            if (!string.IsNullOrEmpty(message))
                StatusMessage = Sanitize(message);
            MessageType = StatusMessageType.Info;
            IsProgressVisible = true;
            if (percent < 0)
            {
                IsProgressIndeterminate = true;
                ProgressValue = 0;
            }
            else
            {
                IsProgressIndeterminate = false;
                ProgressValue = percent;
            }
        }

        public void ProgressGuncelle(double percent) => ProgressValue = percent;

        public void ProgressGizle()
        {
            IsProgressVisible = false;
            IsProgressIndeterminate = true;
            ProgressValue = 0;
        }

        public void Temizle()
        {
            StatusMessage = "Hazır";
            MessageType = StatusMessageType.Info;
            ProgressGizle();
        }

        private static string Sanitize(string message)
            => string.IsNullOrEmpty(message)
                ? string.Empty
                : message.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim();

        private void Raise(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
