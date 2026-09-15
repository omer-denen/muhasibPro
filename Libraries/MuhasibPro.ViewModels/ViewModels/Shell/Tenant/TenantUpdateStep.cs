using MuhasibPro.Business.DTOModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>Tek güncelleme adımı (Yedek/Göç/Doğrulama/Geri alma) — saf durum taşıyıcı.</summary>
    public class TenantUpdateStep : ObservableObject
    {
        public TenantUpdateStep(string number, string title)
        {
            Number = number;
            Title = title;
        }

        public string Number { get; }
        public string Title { get; }

        private string _detail = "Bekliyor";
        public string Detail { get => _detail; private set => Set(ref _detail, value); }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; private set => Set(ref _isRunning, value); }

        private bool _isDone;
        public bool IsDone { get => _isDone; private set => Set(ref _isDone, value); }

        private bool _isFaulted;
        public bool IsFaulted { get => _isFaulted; private set => Set(ref _isFaulted, value); }

        public bool IsPending => !IsRunning && !IsDone && !IsFaulted;

        public void MarkRunning(string detail)
        {
            Detail = detail;
            IsFaulted = false;
            IsDone = false;
            IsRunning = true;
            NotifyPropertyChanged(nameof(IsPending));
        }

        public void MarkDone(string detail)
        {
            Detail = detail;
            IsRunning = false;
            IsDone = true;
            NotifyPropertyChanged(nameof(IsPending));
        }

        public void MarkFault(string detail)
        {
            Detail = detail;
            IsRunning = false;
            IsFaulted = true;
            NotifyPropertyChanged(nameof(IsPending));
        }
    }
}
