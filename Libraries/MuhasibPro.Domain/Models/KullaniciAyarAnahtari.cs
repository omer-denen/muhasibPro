namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// Kullanıcı-kapsamlı ayar anahtarı tek kaynağı (Kural 4).
    /// Giriş yapan kullanıcının firma-bağımsız ayarları kendi anahtarında durur:
    /// <c>{SettingsKey}:U{kullaniciId}</c> (LocalSettings.json içinde, AppPlatform deseni).
    /// Kullanıcısız bağlam (0 ve altı) global anahtara düşer (şablon).
    /// Okuma sırası: kullanıcı anahtarı → (taşımada) eski firma anahtarı → global → fabrika default'u.
    /// </summary>
    public static class KullaniciAyarAnahtari
    {
        public static string KeyFor(string baseKey, long kullaniciId)
            => kullaniciId > 0 ? $"{baseKey}:U{kullaniciId}" : baseKey;
    }
}
