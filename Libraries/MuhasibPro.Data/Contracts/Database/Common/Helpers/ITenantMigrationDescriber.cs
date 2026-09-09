namespace MuhasibPro.Data.Contracts.Database.Common.Helpers;

/// <summary>
/// Bekleyen tenant göçlerini gerçek migration sınıflarından açıklar.
/// Yeni migration eklenince ek kod gerekmez (assembly taraması).
/// </summary>
public interface ITenantMigrationDescriber
{
    TenantMigrationDescription Describe(string migrationId);
}
