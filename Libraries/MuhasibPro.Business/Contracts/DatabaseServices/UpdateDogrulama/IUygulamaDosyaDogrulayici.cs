namespace MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D: uygulama dosyası/sürüm doğrulaması — kritik dosyalar + migration assembly mevcut mu,
    /// çalışan sürüm beklenen paket sürümüyle uyuşuyor mu (salt-okunur; Velopack işi değil).</summary>
    public interface IUygulamaDosyaDogrulayici
    {
        /// <param name="beklenenSurum">Güncelleme öncesi hedeflenen sürüm (UpdateSettingsModel.LastUpdateToVersion); boşsa sürüm karşılaştırması atlanır.</param>
        Task<DogrulamaAdimSonucu> DogrulaAsync(string? beklenenSurum, CancellationToken cancellationToken = default);
    }
}
