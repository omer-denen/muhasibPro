namespace MuhasibPro.Business.Contracts.UIServices.CommonServices.Events
{
    /// <summary>Taşınmış-veri (kurulum/makine uyuşmazlığı) bulundu — bilgilendirme amaçlı, akışı engellemez.</summary>
    public sealed record TransferDetectedEvent(
        string CurrentKurulumId,
        string CurrentMachineId,
        IReadOnlyList<string> Mismatches)
        : DomainEvent("M5", DateTime.UtcNow);
}
