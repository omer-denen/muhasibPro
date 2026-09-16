namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Faz 6.93: Türkçe-duyarlı lexical (anahtar-kelime) skorlama — RRF girdisi.</summary>
public static class YardimSkorlayici
{
    /// <summary>Faz 6.93: bilgi tabanı kaydı için sıralı lexical skor (RRF girdisi).</summary>
    public record KayitSkor(string Anahtar, double Skor);

    private static readonly char[] Ayraclar =
        [' ', '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']', '\t', '\r', '\n'];

    internal static IReadOnlyList<string> Jetonla(string metin) =>
        KelimelereAyir(Normaliza(metin))
            .Where(t => t.Length >= 2)
            .Distinct()
            .ToList();

    /// <summary>Türkçe-duyarlı normalizasyon: küçük harf + diakritik sadeleştirme (TR-FTS emsali).</summary>
    internal static string Normaliza(string? metin)
    {
        if (string.IsNullOrEmpty(metin))
            return string.Empty;
        var kucuk = metin.ToLowerInvariant();
        kucuk = kucuk.Replace('â', 'a').Replace('à', 'a').Replace('á', 'a').Replace('ä', 'a')
            .Replace('ç', 'c')
            .Replace('ğ', 'g')
            .Replace('î', 'i').Replace('ï', 'i').Replace('ı', 'i')
            .Replace('ö', 'o')
            .Replace('ş', 's')
            .Replace('ü', 'u').Replace('û', 'u')
            .Replace("i̇", "i");
        return kucuk;
    }

    internal static IReadOnlyList<string> KelimelereAyir(string metin) =>
        (metin ?? string.Empty)
            .Split(Ayraclar, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

    /// <summary>Faz 6.93: bilgi tabanı kayıtlarını lexical skorla sıralar (başlık ağırlıklı + starts-with).</summary>
    public static IReadOnlyList<KayitSkor> KayitlariSirala(
        string soru, IReadOnlyList<(string Anahtar, string Baslik, string Icerik, string Etiketler)> kayitlar)
    {
        if (string.IsNullOrWhiteSpace(soru) || kayitlar is null || kayitlar.Count == 0)
            return [];
        var jetonlar = Jetonla(soru);
        if (jetonlar.Count == 0)
            return [];

        var sonuc = new List<KayitSkor>();
        foreach (var kayit in kayitlar)
        {
            if (string.IsNullOrWhiteSpace(kayit.Anahtar))
                continue;
            double skor = KayitSkorla(jetonlar, kayit.Baslik, kayit.Icerik, kayit.Etiketler);
            if (skor > 0)
                sonuc.Add(new KayitSkor(kayit.Anahtar, skor));
        }
        return sonuc.OrderByDescending(e => e.Skor).ToList();
    }

    private static double KayitSkorla(IReadOnlyList<string> jetonlar, string? baslik, string? icerik, string? etiketler)
    {
        var normBaslik = Normaliza(baslik);
        var normIcerik = Normaliza(icerik);
        var normEtiket = Normaliza(etiketler);
        var baslikKelimeler = KelimelereAyir(normBaslik);
        var icerikKelimeler = KelimelereAyir(normIcerik);
        double skor = 0;
        foreach (var jeton in jetonlar)
        {
            if (normBaslik.Contains(jeton, StringComparison.Ordinal))
                skor += 3;
            if (normIcerik.Contains(jeton, StringComparison.Ordinal))
                skor += 1;
            if (normEtiket.Contains(jeton, StringComparison.Ordinal))
                skor += 2;
            if (baslikKelimeler.Any(k => k.StartsWith(jeton, StringComparison.Ordinal)))
                skor += 2;
            else if (icerikKelimeler.Any(k => k.StartsWith(jeton, StringComparison.Ordinal)))
                skor += 1;
        }
        return skor;
    }
}
