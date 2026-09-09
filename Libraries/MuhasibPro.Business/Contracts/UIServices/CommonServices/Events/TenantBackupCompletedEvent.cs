namespace MuhasibPro.Business.Contracts.UIServices.CommonServices.Events
{
    /// <summary>Tenant yedeği tamamlandı — yedek listesi + Bilinmeyen taraması tazelenir.</summary>
    public sealed record TenantBackupCompletedEvent(string DatabaseName, string BackupFileName)
        : DomainEvent("M5", DateTime.UtcNow);
}
