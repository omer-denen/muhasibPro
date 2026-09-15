namespace MuhasibPro.Business.Contracts.UIServices;

/// <summary>
/// İşletim sistemi kabuğunda klasör/dosya açma (Business kontratı, uygulaması App katmanında).
/// Tek cümle: verilen yolu varsayılan dosya gezgininde açar.
/// </summary>
public interface IYolAciciService
{
    /// <summary>Klasörü (veya dosyayı seçili olarak) varsayılan gezginde açar. Başarısızsa false döner.</summary>
    bool KlasoruAc(string yol);
}
