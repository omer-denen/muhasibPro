namespace MuhasibPro.Domain.Utilities;

/// <summary>
/// Semantik sürüm tek kaynağı — ayrıştırma/normalleştirme/karşılaştırma.
/// Format: 3-parça sayısal <c>major.minor.patch</c> ("1.1.0"). Karşılaştırma sayısaldır
/// ("1.10.0" &gt; "1.9.0"; string compare tuzağına düşülmez).
/// Ön-sürüm/build metadatası (<c>1.1.4-beta</c>, <c>1.1.4+build</c>) yok sayılır;
/// yalnız <c>major.minor.patch</c> karşılaştırılır (Revizyon 3 — sürüm kıyası sağlamlaştırma).
/// Eski takvim şeması (1.yyyy.MMdd.HHmm) geriye-uyumlu okunur: 0.x bandına
/// indirgenir, böylece her gerçek SemVer'den küçük sayılır ve kendi içinde sıralı kalır.
/// </summary>
public static class SemanticVersion
{
    /// <summary>Üç parçaya normalleştirir; çöp girdide "0.0.0" döner.</summary>
    public static string Normalize(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return "0.0.0";

        var parts = MetadatayiAyikla(version.Trim()).Split('.');
        var numbers = new List<int>();
        foreach (var p in parts)
        {
            if (!int.TryParse(p, out var n) || n < 0)
                return "0.0.0";
            numbers.Add(n);
        }

        // Eski takvim şeması: 1.yyyy.MMdd.HHmm → 0.MMdd.HHmm (örn 1.2026.0908.0 → 0.908.0)
        if (numbers.Count == 4 && numbers[1] >= 2000 && numbers[1] <= 2100)
            return $"0.{numbers[2]}.{numbers[3]}";

        while (numbers.Count < 3)
            numbers.Add(0);
        return $"{numbers[0]}.{numbers[1]}.{numbers[2]}";
    }

    public static bool IsValid(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return false;
        var parts = MetadatayiAyikla(version.Trim()).Split('.');
        return parts.Length == 3 && parts.All(p => int.TryParse(p, out var n) && n >= 0);
    }

    /// <summary>Ön-sürüm (<c>-beta</c>) ve build (<c>+build</c>) ekini atar; yalnız sayısal gövde kalır.</summary>
    private static string MetadatayiAyikla(string version)
    {
        int tire = version.IndexOf('-');
        int arti = version.IndexOf('+');
        int kesim = tire < 0 ? arti : arti < 0 ? tire : Math.Min(tire, arti);
        return kesim >= 0 ? version[..kesim] : version;
    }

    /// <summary>Sayısal karşılaştırma: -1 / 0 / +1. İkisi de çöpse ham stringe düşer.</summary>
    public static int Compare(string? a, string? b)
    {
        var na = Normalize(a);
        var nb = Normalize(b);
        var pa = na.Split('.').Select(int.Parse).ToArray();
        var pb = nb.Split('.').Select(int.Parse).ToArray();
        for (int i = 0; i < 3; i++)
        {
            if (pa[i] != pb[i])
                return pa[i].CompareTo(pb[i]);
        }
        if (na == "0.0.0" && nb == "0.0.0" && !string.Equals(a, b, StringComparison.Ordinal))
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        return 0;
    }

    public static bool IsGreater(string? a, string? b) => Compare(a, b) > 0;
    public static bool IsLess(string? a, string? b) => Compare(a, b) < 0;
    public static bool IsEqual(string? a, string? b) => Compare(a, b) == 0;
}
