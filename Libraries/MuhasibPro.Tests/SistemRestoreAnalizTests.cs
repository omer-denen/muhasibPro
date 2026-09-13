using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Data.Database.SistemDatabase;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.78 Adım 3: Sistem restore tek-kapı analizi — hüküm matrisi + gerçek dosya snapshot okuyucu.</summary>
public class SistemRestoreAnalizTests
{
    private const string BackupDir = @"C:\tmp\backups";
    private const string SistemPath = @"C:\tmp\Sistem.db";

    private static SistemSnapshot Snap(
        int donemSayisi = 1,
        long sonDonemId = 10,
        string kurulum = "K1",
        string guid = "M1",
        string surum = "1.1.0",
        bool ok = true)
        => new()
        {
            OkunabildiMi = ok,
            Surum = surum,
            KurulumId = kurulum,
            MachineGuid = guid,
            MaliDonemler = Enumerable.Range(1, donemSayisi).Select(i => new SistemMaliDonemSatiri
            {
                Id = sonDonemId - donemSayisi + i,
                FirmaId = 1,
                MaliYil = 2024 + i,
                DatabaseName = $"db-FIRMA01_{2024 + i}"
            }).ToList(),
            FirmaIdler = new List<long> { 1 },
            KullaniciIdler = new List<long> { 1 }
        };

    private static RestoreDosyaAnalizi Dosya(bool integrity = true)
        => new()
        {
            DosyaAdi = "y.backup",
            DosyaVarMi = true,
            IntegrityTamamMi = integrity,
            IntegrityMesaji = integrity ? "ok" : "*** malformed ***",
            TabloSayisi = 14
        };

    private static SistemRestoreAnalizService Servis(
        RestoreDosyaAnalizi dosya,
        SistemSnapshot yedek,
        SistemSnapshot mevcut,
        bool tenantDosyaVarMi = false)
    {
        var backup = new Mock<IDatabaseBackupManager>();
        backup.Setup(b => b.AnalyzeBackupFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(dosya);

        var reader = new Mock<ISistemSnapshotReader>();
        reader.SetupSequence(r => r.ReadAsync(It.IsAny<string>()))
            .ReturnsAsync(yedek)   // yedek dosya
            .ReturnsAsync(mevcut); // mevcut Sistem.db

        var paths = new Mock<IApplicationPaths>();
        paths.Setup(p => p.GetBackupFolderPath()).Returns(BackupDir);
        paths.Setup(p => p.GetSistemDatabaseFilePath()).Returns(SistemPath);
        paths.Setup(p => p.TenantDatabaseFileExists(It.IsAny<string>())).Returns(tenantDosyaVarMi);

        return new SistemRestoreAnalizService(backup.Object, reader.Object, paths.Object, Mock.Of<ILogger<SistemRestoreAnalizService>>());
    }

    [Fact]
    public async Task Analiz_Temiz_Yedek_Allow()
    {
        var sonuc = await Servis(Dosya(), Snap(), Snap()).AnalizEtAsync("y.backup");

        sonuc.Hukum.Kind.Should().Be(RestoreVerdictKind.Allow);
        sonuc.Fark.KayipVarMi.Should().BeFalse();
    }

    [Fact]
    public async Task Analiz_Bozuk_Dosya_Block()
    {
        var sonuc = await Servis(Dosya(integrity: false), Snap(), Snap()).AnalizEtAsync("y.backup");

        sonuc.Hukum.Kind.Should().Be(RestoreVerdictKind.Block);
        sonuc.Hukum.Engelli.Should().BeTrue();
    }

    [Fact]
    public async Task Analiz_Yedek_Surumu_Yeni_Block()
    {
        var sonuc = await Servis(Dosya(), Snap(surum: "1.2.0"), Snap(surum: "1.1.0")).AnalizEtAsync("y.backup");

        sonuc.Hukum.Kind.Should().Be(RestoreVerdictKind.Block);
    }

    [Fact]
    public async Task Analiz_Kayip_Kayit_RequireCode()
    {
        // Mevcut 2 dönem, yedekte 1 → yedekten sonra açılan dönem kaybolur.
        var sonuc = await Servis(Dosya(), Snap(donemSayisi: 1), Snap(donemSayisi: 2)).AnalizEtAsync("y.backup");

        sonuc.Hukum.Kind.Should().Be(RestoreVerdictKind.RequireCode);
        sonuc.Hukum.KodGerekli.Should().BeTrue();
        sonuc.Fark.KayipKayitlar.Should().NotBeEmpty();
        sonuc.FarkOzeti.Should().Contain("kaybolur");
    }

    [Fact]
    public async Task Analiz_Tasinmis_Yedek_Warning()
    {
        var sonuc = await Servis(Dosya(), Snap(kurulum: "K2"), Snap(kurulum: "K1")).AnalizEtAsync("y.backup");

        sonuc.Hukum.Kind.Should().Be(RestoreVerdictKind.Warning);
        sonuc.Hukum.Baslik.Should().Contain("Taşınmış");
    }

    [Fact]
    public async Task Analiz_Okunamayan_Yedek_Block()
    {
        var sonuc = await Servis(Dosya(), Snap(ok: false), Snap()).AnalizEtAsync("y.backup");

        sonuc.Hukum.Kind.Should().Be(RestoreVerdictKind.Block);
        sonuc.Hukum.Baslik.Should().Contain("okunamadı");
    }

    [Fact]
    public async Task Analiz_Kayip_Tenant_Dosyasi_Bildirilir()
    {
        var sonuc = await Servis(Dosya(), Snap(donemSayisi: 1), Snap(donemSayisi: 2), tenantDosyaVarMi: true).AnalizEtAsync("y.backup");

        sonuc.Fark.KayipTenantDosyalari.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Okuyucu_Gercek_SistemDb_Cozumler()
    {
        var klasor = Path.Combine(Path.GetTempPath(), "muhasibpro-snaps-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(klasor);
        var yol = Path.Combine(klasor, "Sistem.db");
        try
        {
            await using (var conn = new SqliteConnection($"Data Source={yol};"))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE ""__EFMigrationsHistory"" (MigrationId TEXT PRIMARY KEY, ProductVersion TEXT);
                    INSERT INTO ""__EFMigrationsHistory"" VALUES ('20260908_AddTenantIdentity','8.0.0');
                    CREATE TABLE MaliDonemler (Id INTEGER PRIMARY KEY, FirmaId INTEGER, MaliYil INTEGER, DatabaseName TEXT);
                    INSERT INTO MaliDonemler VALUES (1,1,2027,'db-FIRMA01_2027');
                    CREATE TABLE Firmalar (Id INTEGER PRIMARY KEY);
                    INSERT INTO Firmalar VALUES (1);
                    CREATE TABLE Kullanicilar (Id INTEGER PRIMARY KEY);
                    INSERT INTO Kullanicilar VALUES (1);
                    CREATE TABLE GlobalAyarlar (Id INTEGER PRIMARY KEY, Anahtar TEXT, Deger TEXT, Aciklama TEXT);
                    INSERT INTO GlobalAyarlar VALUES (1,'KurulumId','KUR-1',NULL);
                    INSERT INTO GlobalAyarlar VALUES (2,'KurulumMachineGuid','MAC-1',NULL);";
                await cmd.ExecuteNonQueryAsync();
            }
            SqliteConnection.ClearAllPools();

            var reader = new SistemSnapshotReader(Mock.Of<ILogger<SistemSnapshotReader>>());
            var snap = await reader.ReadAsync(yol);

            snap.OkunabildiMi.Should().BeTrue();
            snap.SonMigration.Should().Be("20260908_AddTenantIdentity");
            snap.Surum.Should().Be("1.1.0");
            snap.MaliDonemler.Should().ContainSingle(d => d.MaliYil == 2027 && d.DatabaseName == "db-FIRMA01_2027");
            snap.FirmaIdler.Should().Contain(1);
            snap.KullaniciIdler.Should().Contain(1);
            snap.KurulumId.Should().Be("KUR-1");
            snap.MachineGuid.Should().Be("MAC-1");
        }
        finally { try { Directory.Delete(klasor, true); } catch { } }
    }

    [Fact]
    public async Task Okuyucu_Olmayan_Dosyayi_Bildirir()
    {
        var reader = new SistemSnapshotReader(Mock.Of<ILogger<SistemSnapshotReader>>());
        var snap = await reader.ReadAsync(Path.Combine(Path.GetTempPath(), "yok-" + Guid.NewGuid().ToString("N") + ".db"));

        snap.OkunabildiMi.Should().BeFalse();
    }
}
