using MuhasibPro.Domain.Models.PostUpdateModel;

namespace MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D: güncelleme sonrası doğrulama sagası.
    /// Tetik kalıcıdır: <c>UpdateSettingsModel.LastUpdateToVersion</c> dolu + <c>LastUpdateVerifiedAt</c> boş iken çalışır
    /// (uygulama ilk yeniden başlatmada çökse bile sonraki açılışta devam eder).</summary>
    public interface IPostUpdateDogrulamaService
    {
        /// <summary>Bu açılışta doğrulama gerekli mi (kalıcı damgadan okur).</summary>
        Task<bool> GerekliMiAsync();

        /// <summary>Doğrulama sagasını çalıştırır. Başarıda <see cref="UpdateSettingsModel.LastUpdateVerifiedAt"/> damgalanır.
        /// Bloklayıcı hatada uygulama açılışı yönlendirilir (giriş kapalı).</summary>
        Task<PostUpdateDogrulamaSonucu> CalistirAsync(
            IProgress<PostUpdateAdimSonucu>? ilerleme = null,
            CancellationToken cancellationToken = default);
    }
}
