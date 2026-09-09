using MuhasibPro.Business.DTOModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>Tenant güncelleme/geçiş ilerleme durumu. Servis bilmez, sadece ilerleme taşır.</summary>
    public class TenantUpdateProgressViewModel : ObservableObject
    {
        private bool _isUpdating;
        public bool IsUpdating { get => _isUpdating; set => Set(ref _isUpdating, value); }

        private string _updateStatus = string.Empty;
        public string UpdateStatus { get => _updateStatus; set => Set(ref _updateStatus, value); }

        private double _updateProgress;
        public double UpdateProgress { get => _updateProgress; set => Set(ref _updateProgress, value); }

        public void BeginCheck()
        {
            IsUpdating = true;
            UpdateStatus = "Güncelleme kontrolü yapılıyor...";
            UpdateProgress = 10;
        }

        public void BeginUpdate()
        {
            IsUpdating = true;
            UpdateStatus = "Yedek alınıyor ve göçler uygulanıyor...";
            UpdateProgress = 50;
        }

        public void BeginSwitch()
        {
            UpdateStatus = "Veritabanı bağlanıyor ve güncelleniyor...";
            UpdateProgress = 70;
        }

        public void EndProgress()
        {
            IsUpdating = false;
            UpdateStatus = string.Empty;
            UpdateProgress = 0;
        }
    }
}
