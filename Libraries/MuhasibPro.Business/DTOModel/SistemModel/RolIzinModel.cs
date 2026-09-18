using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Business.DTOModel.SistemModel;

/// <summary>Faz 6.85 K3: izin matrisi satırı (kategori + okunabilir ad + seçim durumu).</summary>
public class RolIzinModel : ObservableObject
{
    public Permission Izin { get; set; }
    public string Kategori { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;

    private bool _secili;
    public bool Secili { get => _secili; set => Set(ref _secili, value); }
}
