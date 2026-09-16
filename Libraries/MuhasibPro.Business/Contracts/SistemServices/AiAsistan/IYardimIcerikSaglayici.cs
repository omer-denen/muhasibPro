using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.Business.Contracts.SistemServices.AiAsistan
{
    /// <summary>RAG derlemi sayfası: mevcut Kural 13 yardım içeriğinin aynası (tek kaynak view'de kalır).</summary>
    public class YardimliSayfaDto
    {
        public string Anahtar { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public IReadOnlyList<YardimMaddesiDto> Maddeler { get; set; } = [];
    }

    /// <summary>Faz 6.92: yardım içeriği toplayıcısı (RAG derlemi). İçerik sahipliği view'dedir; bu sözleşme yalnız okur.</summary>
    public interface IYardimIcerikSaglayici
    {
        IReadOnlyList<YardimliSayfaDto> TumSayfalariGetir();
        IReadOnlyList<YardimMaddesiDto> SayfaGetir(string sayfaAnahtari);
    }
}
