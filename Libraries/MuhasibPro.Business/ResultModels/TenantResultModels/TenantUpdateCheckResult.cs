namespace MuhasibPro.Business.ResultModels.TenantResultModels
{
    /// <summary>Tenant güncelleme gereksinim kontrolünün salt-veri sonucu (dialog metni dahil, UI'sız).</summary>
    public class TenantUpdateCheckResult
    {
        public string DatabaseName { get; set; } = string.Empty;
        public bool CheckSucceeded { get; set; }
        public bool NeedsUpdate { get; set; }
        public string CurrentVersion { get; set; }
        public string TargetVersion { get; set; } = "-";
        public int PendingCount { get; set; }
        public List<string> PendingLines { get; set; } = new();
        /// <summary>Bekleyen göçlerin tek-satır özetleri (dialog "Ana değişiklik" başlığı).</summary>
        public List<string> PendingSummaries { get; set; } = new();
        /// <summary>Seçili firma ünvanı + dönem yılı (Coordinator doldurur — dialog/sayfa gösterir).</summary>
        public string FirmaUnvani { get; set; } = string.Empty;
        public int MaliYil { get; set; }
        /// <summary>Tek-satır başlık (örn. "'X' tablosu güncellemesi").</summary>
        public string Headline { get; set; } = "Şema Değişikliği";
        /// <summary>Tablo bazında değişiklikler (sayfa Expander listesi).</summary>
        public List<MuhasibPro.Data.Contracts.Database.Common.Helpers.TenantTableChange> TableChanges { get; set; } = new();
        /// <summary>Gerçek migration yapısından üretilen detay satırları (onay dialogunun "Ne güncellenecek?" bölümü).</summary>
        public List<string> UpdateDetailLines { get; set; } = new();
        public string StatusMessage { get; set; }
        public string ConfirmMessage { get; set; } = string.Empty;
    }
}
