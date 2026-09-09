using MuhasibPro.Business.DTOModel;

namespace MuhasibPro.Business.Contracts.Installation;

public class KurulumKayitModel
{
    public string KurulumId { get; set; } = string.Empty;
    public string MachineGuid { get; set; } = string.Empty;
    public DateTime OlusturmaTarihi { get; set; }
}

public interface IKurulumKayitService
{
    Task<KurulumKayitModel> GetOrCreateAsync();
    Task<KurulumKayitModel?> GetAsync();
    Task EnsureAsync();
}
