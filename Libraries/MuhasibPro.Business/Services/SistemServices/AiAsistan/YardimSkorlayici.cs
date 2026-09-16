using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Faz 6.92 RAG v1: anahtar-kelime eşleştirme (derlem 100 parçadan küçük — vektör yok).</summary>
public static class YardimSkorlayici
{
    public record Eslesme(YardimliSayfaDto Sayfa, YardimMaddesiDto Madde, int Skor);

    private static readonly char[] Ayraclar =
        [' ', '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']', '\t', '\r', '\n'];

    public static IReadOnlyList<Eslesme> IlgiliMaddeleriBul(
        string soru, IReadOnlyList<YardimliSayfaDto> sayfalar, int enFazla)
    {
        if (string.IsNullOrWhiteSpace(soru) || sayfalar is null || sayfalar.Count == 0 || enFazla <= 0)
            return [];
        var jetonlar = Jetonla(soru);
        if (jetonlar.Count == 0)
            return [];

        var sonuc = new List<Eslesme>();
        foreach (var sayfa in sayfalar)
        {
            if (sayfa?.Maddeler is null)
                continue;
            foreach (var madde in sayfa.Maddeler)
            {
                if (madde is null)
                    continue;
                int skor = Skorla(jetonlar, madde);
                if (skor > 0)
                    sonuc.Add(new Eslesme(sayfa, madde, skor));
            }
        }
        return sonuc.OrderByDescending(e => e.Skor).Take(enFazla).ToList();
    }

    internal static IReadOnlyList<string> Jetonla(string metin) =>
        metin.ToLowerInvariant()
            .Split(Ayraclar, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 2)
            .Distinct()
            .ToList();

    private static int Skorla(IReadOnlyList<string> jetonlar, YardimMaddesiDto madde)
    {
        var baslik = (madde.Baslik ?? string.Empty).ToLowerInvariant();
        var aciklama = (madde.Aciklama ?? string.Empty).ToLowerInvariant();
        int skor = 0;
        foreach (var jeton in jetonlar)
        {
            if (baslik.Contains(jeton, StringComparison.Ordinal))
                skor += 3;
            if (aciklama.Contains(jeton, StringComparison.Ordinal))
                skor += 1;
        }
        return skor;
    }
}
