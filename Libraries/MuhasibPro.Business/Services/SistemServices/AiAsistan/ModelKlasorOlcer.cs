namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Model önbellek klasörü disk ölçümü (SDK'sız System.IO; 277 model-yönetimi isteği).</summary>
public static class ModelKlasorOlcer
{
    /// <summary>Klasördeki tüm dosyaların bayt toplamı. Yol boş/geçersiz ya da klasör yoksa 0 döner (fırlatmaz).
    /// Kilitli/geçici okunamayan dosyalar atlanır (ölçüm en-iyi-gayrettir, yönetim ekranını düşürmez).</summary>
    public static long BaytToplaminiHesapla(string? klasorYolu)
    {
        if (string.IsNullOrWhiteSpace(klasorYolu) || !Directory.Exists(klasorYolu))
            return 0;
        long toplam = 0;
        foreach (var dosya in Directory.EnumerateFiles(klasorYolu, "*", SearchOption.AllDirectories))
        {
            try
            {
                toplam += new FileInfo(dosya).Length;
            }
            catch (IOException)
            {
                // Ölçüm en-iyi-gayret: kilitli dosya atlanır, toplam kalanlarla döner.
            }
            catch (UnauthorizedAccessException)
            {
                // Ölçüm en-iyi-gayret: erişilemeyen dosya atlanır.
            }
        }
        return toplam;
    }
}
