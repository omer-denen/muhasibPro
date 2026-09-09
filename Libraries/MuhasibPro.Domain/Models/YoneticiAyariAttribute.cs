namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// Kritik ayar işareti — bu değer yalnızca yönetici değiştirebilir.
    /// İşaretsiz ayarlar normal kullanıcıya açıktır.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class YoneticiAyariAttribute : Attribute
    {
    }
}
