namespace MuhasibPro.Data.Contracts.Database.Common.Helpers;

public interface IMakineKimligiProvider
{
    Task<string> GetMachineIdAsync();
    Task<string> GetOrCreateFallbackMachineIdAsync();
}
