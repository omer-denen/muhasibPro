namespace MuhasibPro.Business.Contracts.UIServices.CommonServices.Events
{
    /// <summary>Tenant geri-yüklemesi tamamlandı — yedek listesi + Bilinmeyen taraması tazelenir.</summary>
    public sealed record TenantRestoreCompletedEvent(string DatabaseName, string BackupFileName)
        : DomainEvent("M5", DateTime.UtcNow);
}
