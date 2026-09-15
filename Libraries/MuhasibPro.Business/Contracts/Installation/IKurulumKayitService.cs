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

    /// <summary>Kimlik kaybı onarımı: kurulum kimliğini verilen değere geri yazar
    /// (tenant damgalarıyla uyum — aynı makinede yenilenen kimliğin düzeltilmesi).</summary>
    Task UpdateKurulumIdAsync(string kurulumId);
}
