namespace MuhasibPro.Business.Contracts.UIServices.CommonServices.Events
{
    /// <summary>Tipli olay zarfı tabanı — tüm modül olayları buradan türer (magic string yayın yasak).</summary>
    public abstract record DomainEvent(string Module, DateTime OccurredAtUtc);
}
