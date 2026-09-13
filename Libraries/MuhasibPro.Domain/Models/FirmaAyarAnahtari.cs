namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// Firma-kapsamlı ayar anahtarı tek kaynağı (Kural 4).
    /// Yönetim penceresi firma bağlamında açılır; her firmanın liste/saklama
    /// ayarı kendi anahtarında durur: <c>{SettingsKey}:F{firmaId}</c>.
    /// Anahtarsız firma (0 ve altı) global anahtara düşer (şablon).
    /// Okuma sırası: firma anahtarı → global → fabrika default'u.
    /// </summary>
    public static class FirmaAyarAnahtari
    {
        public static string KeyFor(string baseKey, long firmaId)
            => firmaId > 0 ? $"{baseKey}:F{firmaId}" : baseKey;
    }
}
