namespace MuhasibPro.Business.ResultModels.TenantResultModels
{
    /// <summary>Tenant geçiş (yedek-önce-göç + yayın) sonucunun salt-veri karşılığı.</summary>
    public class TenantUpdateSwitchResult
    {
        public bool Success { get; set; }
        public string ConnectionMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
