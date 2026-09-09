namespace MuhasibPro.Business.Contracts.UIServices.CommonServices.Events
{
    /// <summary>M1 ayar kaydı sonrası yayın — abone modül kendi ayarına göre tepki verir.</summary>
    public sealed record AppSettingsChangedEvent(string SettingsKey)
        : DomainEvent("M1", DateTime.UtcNow);
}
