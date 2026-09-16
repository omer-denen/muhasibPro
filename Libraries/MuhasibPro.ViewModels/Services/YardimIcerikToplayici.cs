using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.ViewModels.Services;

/// <summary>Faz 6.92: RAG derlemi — mevcut Kural 13 statiklerinden okur (içerik sahipliği VM'de kalır).</summary>
public sealed class YardimIcerikToplayici : IYardimIcerikSaglayici
{
    public IReadOnlyList<YardimliSayfaDto> TumSayfalariGetir() => new List<YardimliSayfaDto>
    {
        new() { Anahtar = LoginViewModel.YardimAnahtari, Baslik = LoginViewModel.YardimBasligi, Maddeler = LoginViewModel.YardimMaddeleri() },
        new() { Anahtar = FirmaShellViewModel.YardimAnahtari, Baslik = FirmaShellViewModel.YardimBasligi, Maddeler = FirmaShellViewModel.YardimMaddeleri() },
        new() { Anahtar = MainShellViewModel.YardimAnahtari, Baslik = MainShellViewModel.YardimBasligi, Maddeler = MainShellViewModel.YardimMaddeleri() },
        new() { Anahtar = MaliDonemYonetimViewModel.YardimAnahtari, Baslik = MaliDonemYonetimViewModel.YardimBasligi, Maddeler = MaliDonemYonetimViewModel.YardimMaddeleri() },
        new() { Anahtar = DatabaseSettingsViewModel.YardimAnahtari, Baslik = DatabaseSettingsViewModel.YardimBasligi, Maddeler = DatabaseSettingsViewModel.YardimMaddeleri() },
        new() { Anahtar = UpdateViewModel.YardimAnahtari, Baslik = UpdateViewModel.YardimBasligi, Maddeler = UpdateViewModel.YardimMaddeleri() },
        new() { Anahtar = GelistiriciAraclariViewModel.YardimAnahtari, Baslik = GelistiriciAraclariViewModel.YardimBasligi, Maddeler = GelistiriciAraclariViewModel.YardimMaddeleri() },
    };

    public IReadOnlyList<YardimMaddesiDto> SayfaGetir(string sayfaAnahtari) =>
        TumSayfalariGetir()
            .FirstOrDefault(s => s.Anahtar.Equals(sayfaAnahtari, StringComparison.OrdinalIgnoreCase))
            ?.Maddeler ?? [];
}
