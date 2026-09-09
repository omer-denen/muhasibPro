namespace MuhasibPro.Business.Contracts.UIServices.CommonServices.Events
{
    /// <summary>
    /// M5 → M3/M1: tenant DB için şema güncellemesi saptandı. Rozet (refresh'siz) + durum/toast besler;
    /// mevcut TenantUpdated→DevamEt→MainShell akışı korunur.
    /// </summary>
    public sealed record TenantUpdateAvailableEvent(
        string DatabaseName,
        string FromVersion,
        string ToVersion)
        : DomainEvent("M5", DateTime.UtcNow);
}
