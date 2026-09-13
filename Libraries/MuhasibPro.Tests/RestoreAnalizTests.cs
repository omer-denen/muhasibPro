using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using MuhasibPro.Business.Services.DatabaseServices.Common;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Data.Database.Common;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.78 Adım 1: ortak restore analiz çekirdeği — hüküm matrisi + dosya-okuyucu (gerçek dosya).</summary>
public class RestoreAnalizTests
{
    private static RestoreAnalizGirdisi TemizGirdi() => new()
    {
        Dosya = new RestoreDosyaAnalizi { DosyaAdi = "a.backup", DosyaVarMi = true, IntegrityTamamMi = true, IntegrityMesaji = "ok", TabloSayisi = 14 },
        Fark = new RestoreFarkOzeti(),
        KimlikliMi = true,
        YedekVersion = "1.1.0",
        MevcutVersion = "1.1.0"
    };

    [Fact]
    public void Hukum_DosyaYoksa_Block()
    {
        var hukum = RestoreAnalizDegerlendirici.Degerlendir(new RestoreAnalizGirdisi());
        hukum.Kind.Should().Be(RestoreVerdictKind.Block);
    }

    [Fact]
    public void Hukum_IntegrityBozuksa_Block()
    {
        var girdi = TemizGirdi();
        girdi.Dosya.IntegrityTamamMi = false;
        girdi.Dosya.IntegrityMesaji = "*** database disk image is malformed ***";
        RestoreAnalizDegerlendirici.Degerlendir(girdi).Kind.Should().Be(RestoreVerdictKind.Block);
    }

    [Fact]
    public void Hukum_YedekYeniyse_Block()
    {
        var girdi = TemizGirdi();
        girdi.YedekVersion = "1.2.0";
        girdi.MevcutVersion = "1.1.0";
        var hukum = RestoreAnalizDegerlendirici.Degerlendir(girdi);
        hukum.Kind.Should().Be(RestoreVerdictKind.Block);
        hukum.Engelli.Should().BeTrue();
    }

    [Fact]
    public void Hukum_YedekEskiyse_Warning()
    {
        var girdi = TemizGirdi();
        girdi.YedekVersion = "1.0.0";
        girdi.MevcutVersion = "1.1.0";
        RestoreAnalizDegerlendirici.Degerlendir(girdi).Kind.Should().Be(RestoreVerdictKind.Warning);
    }

    [Fact]
    public void Hukum_KayipKayitVarsa_RequireCode()
    {
        var girdi = TemizGirdi();
        girdi.Fark.KayipKayitlar.Add("Mali dönem 2028");
        var hukum = RestoreAnalizDegerlendirici.Degerlendir(girdi);
        hukum.Kind.Should().Be(RestoreVerdictKind.RequireCode);
        hukum.KodGerekli.Should().BeTrue();
        hukum.Aciklama.Should().Contain("2028");
    }

    [Fact]
    public void Hukum_Kimliksizse_Warning()
    {
        var girdi = TemizGirdi();
        girdi.KimlikliMi = false;
        var hukum = RestoreAnalizDegerlendirici.Degerlendir(girdi);
        hukum.Kind.Should().Be(RestoreVerdictKind.Warning);
        hukum.IsKimliksiz.Should().BeTrue();
    }

    [Fact]
    public void Hukum_Tasinmissa_Warning()
    {
        var girdi = TemizGirdi();
        girdi.MakineFarkli = true;
        RestoreAnalizDegerlendirici.Degerlendir(girdi).Kind.Should().Be(RestoreVerdictKind.Warning);
    }

    [Fact]
    public void Hukum_Temizse_Allow()
    {
        RestoreAnalizDegerlendirici.Degerlendir(TemizGirdi()).Kind.Should().Be(RestoreVerdictKind.Allow);
    }

    [Fact]
    public void Hukum_KimlikUyusmazligi_RequireCode()
    {
        var girdi = TemizGirdi();
        girdi.KimlikUyusmazlikBaslik = "Firma uyuşmazlığı";
        girdi.KimlikUyusmazlikAciklama = "detay";
        var hukum = RestoreAnalizDegerlendirici.Degerlendir(girdi);
        hukum.Kind.Should().Be(RestoreVerdictKind.RequireCode);
        hukum.KodGerekli.Should().BeTrue();
        hukum.Baslik.Should().Be("Firma uyuşmazlığı");
    }

    [Fact]
    public void Hukum_Kimliksiz_KimlikUyusmazligini_GecersizKilar()
    {
        var girdi = TemizGirdi();
        girdi.KimlikliMi = false;
        girdi.KimlikUyusmazlikBaslik = "Firma uyuşmazlığı";
        var hukum = RestoreAnalizDegerlendirici.Degerlendir(girdi);
        hukum.Kind.Should().Be(RestoreVerdictKind.Warning);
        hukum.IsKimliksiz.Should().BeTrue();
    }

    [Fact]
    public void Hukum_Kimlik_SurumdenOnce_Hukmeder()
    {
        var girdi = TemizGirdi();
        girdi.YedekVersion = "9.9.9"; // yeni sürüm normalde Block; kimlik kıyası önce gelir
        girdi.KimlikUyusmazlikBaslik = "Mali dönem uyuşmazlığı";
        RestoreAnalizDegerlendirici.Degerlendir(girdi).Kind.Should().Be(RestoreVerdictKind.RequireCode);
    }

    private static DatabaseBackupManager Okuyucu() =>
        new(Mock.Of<ILogger<DatabaseBackupManager>>());

    private static string KlasorAc()
    {
        var klasor = Path.Combine(Path.GetTempPath(), "muhasibpro-restoreanaliz-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(klasor);
        return klasor;
    }

    [Fact]
    public async Task Okuyucu_SaglamDosyayi_Cozumler()
    {
        var klasor = KlasorAc();
        try
        {
            var yol = Path.Combine(klasor, "saglam.backup");
            await using (var conn = new SqliteConnection($"Data Source={yol};"))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE Kullanicilar (Id INTEGER PRIMARY KEY); CREATE TABLE \"__EFMigrationsHistory\" (MigrationId TEXT PRIMARY KEY, ProductVersion TEXT); INSERT INTO \"__EFMigrationsHistory\" VALUES ('20260908_AddTenantIdentity', '8.0.0');";
                await cmd.ExecuteNonQueryAsync();
            }
            SqliteConnection.ClearAllPools();

            var analiz = await Okuyucu().AnalyzeBackupFileAsync(klasor, "saglam.backup");

            analiz.DosyaVarMi.Should().BeTrue();
            analiz.IntegrityTamamMi.Should().BeTrue();
            analiz.TabloSayisi.Should().Be(2);
            analiz.GocGecmisiSurumu.Should().Be("20260908_AddTenantIdentity");
            analiz.DosyaBoyutu.Should().BeGreaterThan(0);
        }
        finally { try { Directory.Delete(klasor, true); } catch { } }
    }

    [Fact]
    public async Task Okuyucu_BozukDosyayi_Bloklar()
    {
        var klasor = KlasorAc();
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(klasor, "bozuk.backup"), new byte[] { 1, 2, 3, 4, 5 });

            var analiz = await Okuyucu().AnalyzeBackupFileAsync(klasor, "bozuk.backup");

            analiz.DosyaVarMi.Should().BeTrue();
            analiz.IntegrityTamamMi.Should().BeFalse();
        }
        finally { try { Directory.Delete(klasor, true); } catch { } }
    }

    [Fact]
    public async Task Okuyucu_OlmayanDosyayi_Bildirir()
    {
        var klasor = KlasorAc();
        try
        {
            var analiz = await Okuyucu().AnalyzeBackupFileAsync(klasor, "yok.backup");
            analiz.DosyaVarMi.Should().BeFalse();
            analiz.IntegrityTamamMi.Should().BeFalse();
        }
        finally { try { Directory.Delete(klasor, true); } catch { } }
    }
}
