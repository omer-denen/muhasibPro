using FluentAssertions;

namespace MuhasibPro.Tests;

/// <summary>
/// Modüler mimari bekçisi — ihlal = build/test kırmızısı (AGENTS kural 5 + CEKIRDEK-MODUL-PLAN Faz 1).
/// Kaynak-tarama desenidir (BusinessCoreTests'teki DI assert'leriyle aynı aile).
/// </summary>
public class ArchitectureTests
{
    private static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (File.Exists(Path.Combine(dir, "MuhasibPro.slnx")) || File.Exists(Path.Combine(dir, "MuhasibPro.sln"))) return dir;
            dir = Path.GetDirectoryName(dir)!;
        }
        return Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..");
    }

    private static IEnumerable<(string Yol, string Icerik)> Kaynaklar(params string[] goreceliKlasorler)
    {
        var kok = RepoRoot();
        foreach (var klasor in goreceliKlasorler)
        {
            var tam = Path.Combine(kok, klasor);
            if (!Directory.Exists(tam)) continue;
            foreach (var dosya in Directory.EnumerateFiles(tam, "*.cs", SearchOption.AllDirectories))
            {
                if (dosya.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    || dosya.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                    continue;
                yield return (Path.GetRelativePath(kok, dosya), File.ReadAllText(dosya));
            }
        }
    }

    private static void YasakIcermez(
        IEnumerable<(string Yol, string Icerik)> dosyalar,
        string yasak,
        string gerekce)
    {
        var ihlaller = dosyalar
            .Where(d => d.Icerik.Contains(yasak, StringComparison.Ordinal))
            .Select(d => d.Yol)
            .ToList();
        ihlaller.Should().BeEmpty($"{gerekce} — yasak metin: '{yasak}'");
    }

    [Fact]
    public void ViewModel_EFCore_Bilmez()
    {
        var dosyalar = Kaynaklar("Libraries/MuhasibPro.ViewModels").ToList();
        YasakIcermez(dosyalar, "using Microsoft.EntityFrameworkCore", "ViewModel katmanı EF Core bilmez (kural 5)");
        YasakIcermez(dosyalar, "using MuhasibPro.Data.DataContext", "ViewModel somut DbContext bilmez (kural 5)");
    }

    [Fact]
    public void View_EFCore_Bilmez()
    {
        var dosyalar = Kaynaklar("MuhasibPro/Views", "MuhasibPro/Helpers").ToList();
        YasakIcermez(dosyalar, "using Microsoft.EntityFrameworkCore", "View katmanı EF Core bilmez (Faz 1: SplashNavigator temizliği)");
        YasakIcermez(dosyalar, "Data.DataContext.SistemDbContext", "View somut DbContext çekemez");
        YasakIcermez(dosyalar, "IAppDbContextFactory", "View tenant bağlantısı açamaz (M5 işi)");
    }

    [Fact]
    public void Business_WinUI_Bilmez()
    {
        var dosyalar = Kaynaklar("Libraries/MuhasibPro.Business", "Libraries/MuhasibPro.Data").ToList();
        YasakIcermez(dosyalar, "using Microsoft.UI.Xaml", "Business/Data WinUI bilmez (kural 5)");
        YasakIcermez(dosyalar, "ContentDialog", "Business/Data dialog bilmez");
    }

    [Fact]
    public void Data_Business_Bilmez()
    {
        var dosyalar = Kaynaklar("Libraries/MuhasibPro.Data").ToList();
        YasakIcermez(dosyalar, "using MuhasibPro.Business", "Bağımlılık yönü tek taraflı: Business→Data (döngü yasak)");
    }

    [Fact]
    public void Tenant_Ile_SistemDb_Modulleri_OrtakAltyapi_Disinda_Konusmaz()
    {
        // Ortak altyapı (her iki modülün de kullanabildiği): paths/backup/result/ortak contract'lar + IMaliDonemService.
        var tenant = Kaynaklar(
            "Libraries/MuhasibPro.Business/Services/DatabaseServices/TenantDatabaseService",
            "Libraries/MuhasibPro.Business/Contracts/DatabaseServices/TenantDatabaseServices",
            "Libraries/MuhasibPro.Data/Database/TenantDatabase").ToList();
        YasakIcermez(tenant, "SistemDatabaseService",
            "Tenant modülü sistem.db servisine yaslanmaz");
        YasakIcermez(tenant, "ISistemMigrationManager",
            "Tenant modülü sistem migration yöneticisine yaslanmaz");
        YasakIcermez(tenant, "ISistemBackupManager",
            "Tenant modülü sistem yedek yöneticisine yaslanmaz");
        YasakIcermez(tenant, "_sistemDbContext",
            "Tenant modülü somut SistemDbContext'i field olarak tutamaz (IMaliDonemService ortak sözleşmedir; IUnitOfWork<T> paylaşımlı işlem altyapısıdır)");
        YasakIcermez(tenant, "SistemDbContext.MaliDonemler",
            "Tenant modülü Sistem.db satırlarını doğrudan sorgulayamaz");
        YasakIcermez(tenant, "GetSistemDatabaseFilePath",
            "Tenant modülü Sistem.db dosya yolunu hedef gösteremez (kritik: rollback veri kaybı)");

        var sistem = Kaynaklar(
            "Libraries/MuhasibPro.Business/Services/DatabaseServices/SistemDatabaseService",
            "Libraries/MuhasibPro.Business/Contracts/DatabaseServices/SistemDatabaseServices",
            "Libraries/MuhasibPro.Data/Database/SistemDatabase").ToList();
        YasakIcermez(sistem, "TenantSQLite",
            "Sistem.db modülü tenant implementasyonuna yaslanmaz");
        YasakIcermez(sistem, "ITenantSQLite",
            "Sistem.db modülü tenant sözleşmesine yaslanmaz");
        YasakIcermez(sistem, "ITenantBackupService",
            "Sistem.db modülü tenant yedek servisine yaslanmaz");
    }

    [Fact]
    public void Kurulum_Ile_SistemDb_Yonetimi_Ayrik()
    {
        var kurulum = Kaynaklar(
            "Libraries/MuhasibPro.Business/Services/Installation",
            "Libraries/MuhasibPro.Business/Contracts/Installation").ToList();
        YasakIcermez(kurulum, "DatabaseServices",
            "Kurulum modülü (kurulum kimliği + global ayar) sistem.db/tenant servislerine yaslanmaz");

        var sistemDb = Kaynaklar(
            "Libraries/MuhasibPro.Business/Services/DatabaseServices/SistemDatabaseService",
            "Libraries/MuhasibPro.Business/Contracts/DatabaseServices/SistemDatabaseServices",
            "Libraries/MuhasibPro.Data/Database/SistemDatabase").ToList();
        YasakIcermez(sistemDb, "Contracts.Installation",
            "Sistem.db yönetimi (migrate/yedek/restore) kurulum modülüne yaslanmaz");
        YasakIcermez(sistemDb, "IKurulumKayitService",
            "Sistem.db yönetimi kurulum servisini çağıramaz (orkestrasyon M1'indir)");
    }
}
