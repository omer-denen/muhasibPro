namespace MuhasibPro.Business.DTOModel.SistemModel
{
    /// <summary>
    /// Kural 13 yardım dialogu maddesi (MVVM): ViewModel içeriği Business DTO ile taşır,
    /// App katmanındaki DialogService bunu Views.Components.YardimDialog'a dönüştürür.
    /// </summary>
    public class YardimMaddesiDto
    {
        public string Baslik { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
    }
}
