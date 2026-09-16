namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Faz 6.93: lexical + vektör sıralamalarını RRF ile birleştirir. Saf fonksiyondur.</summary>
public static class RrfBirlestirici
{
    /// <summary>RRF sabiti (vstash emsali).</summary>
    public const int K = 60;

    /// <summary>İki sıralı listeyi RRF skoruyla birleştirir. Skor = Σ w / (K + sıra).</summary>
    public static IReadOnlyList<(string Anahtar, double Skor)> Birlestir(
        IReadOnlyList<string>? lexicalSira,
        IReadOnlyList<string>? vektorSira,
        double agirlikLexical = 0.4,
        double agirlikVektor = 0.6)
    {
        var skorlar = new Dictionary<string, double>(StringComparer.Ordinal);
        PuanEkle(skorlar, lexicalSira, agirlikLexical);
        PuanEkle(skorlar, vektorSira, agirlikVektor);
        return skorlar
            .OrderByDescending(e => e.Value)
            .Select(e => (e.Key, e.Value))
            .ToList();
    }

    private static void PuanEkle(Dictionary<string, double> skorlar, IReadOnlyList<string>? sira, double agirlik)
    {
        if (sira is null || agirlik <= 0)
            return;
        for (int i = 0; i < sira.Count; i++)
        {
            var anahtar = sira[i];
            if (string.IsNullOrWhiteSpace(anahtar))
                continue;
            var puan = agirlik / (K + 1 + i);
            skorlar[anahtar] = skorlar.TryGetValue(anahtar, out var mevcut) ? mevcut + puan : puan;
        }
    }
}
