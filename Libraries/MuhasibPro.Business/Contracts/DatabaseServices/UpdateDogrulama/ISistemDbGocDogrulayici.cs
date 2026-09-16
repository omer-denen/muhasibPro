namespace MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D: Sistem.db güncelleme-sonrası doğrulama — ileri-uyumluluk guard,
    /// gerekli göç + verify; göç düşerse güncelleme öncesi yedekten restore + tekrar verify (fail-closed).</summary>
    public interface ISistemDbGocDogrulayici
    {
        /// <param name="preUpdateYedekYolu">Güncelleme öncesi alınan doğrulanmış Sistem.db yedeğinin tam yolu.</param>
        Task<DogrulamaAdimSonucu> DogrulaAsync(string? preUpdateYedekYolu, CancellationToken cancellationToken = default);
    }
}
