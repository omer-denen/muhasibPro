namespace MuhasibPro.Data.Database.Common.Helpers;

/// <summary>
/// Şema sürümleri tek kaynağı — migration kimliği → SemVer eşlemesi.
/// KURAL: yeni migration eklenirken buraya bir satır eklenir.
/// Geriye-uyumlu şema değişimi MINOR artırır (1.1.0 → 1.2.0);
/// kırıcı değişim MAJOR artırır (1.x → 2.0.0).
/// </summary>
public static class DbSchemaVersions
{
    /// <summary>Boş/bilinmeyen migration karşılığı — ilk şema.</summary>
    public const string InitialSchemaVersion = "1.0.0";

    /// <summary>Bu kodun dağıttığı güncel şema.</summary>
    public const string CurrentSchemaVersion = "1.1.0";

    private static readonly Dictionary<string, string> Harita = new(StringComparer.OrdinalIgnoreCase)
    {
        ["20260828170905_Initial"] = "1.0.0",
        ["20260828170927_InitialApp"] = "1.0.0",
        ["20260908_AddTenantIdentity"] = "1.1.0",
    };

    /// <summary>Migration kimliğinden SemVer üretir; bilinmeyende ilk şemaya düşer.</summary>
    public static string ForMigration(string? migrationId)
    {
        if (!string.IsNullOrWhiteSpace(migrationId) && Harita.TryGetValue(migrationId, out var version))
            return version;
        return InitialSchemaVersion;
    }
}
