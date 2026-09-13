namespace MuhasibPro.Business.Contracts.SistemServices.AppServices
{
    /// <summary>Firma kaydından ayar-sahibi kullanıcı çıkarımı (Kural 4 tek kaynak).
    /// Firma ayarları firma kaydının <c>KaydedenId</c>'sine aittir; giriş yapan yoksa 0 döner (global fallback).</summary>
    public interface IFirmaKullaniciCozucu
    {
        /// <summary>Firma kaydının KaydedenId'sini döndürür (best-effort; çözülemezse 0).</summary>
        Task<long> CozAsync(long firmaId);

        /// <summary>Giriş yapmış kullanıcının Id'si (yoksa 0).</summary>
        long GirisYapanId();
    }
}
