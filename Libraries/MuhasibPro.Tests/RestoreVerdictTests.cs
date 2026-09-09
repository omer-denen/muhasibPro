using FluentAssertions;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Tests;

public class RestoreVerdictTests
{
    private const string KurulumA = "AAAABBBBCCCCDDDD";
    private const string KurulumB = "EEEEFFFFGGGGHHHH";
    private const string MakineA = "MACHINE-A-GUID";
    private const string MakineB = "MACHINE-B-GUID";

    private static MaliDonemModel Hedef(long firmaId = 1, long id = 10, string db = "db-FIRMA01_2027") => new()
    {
        Id = id,
        FirmaId = firmaId,
        DatabaseName = db,
        MaliYil = 2027
    };

    private static DatabaseBackupResult YedekKimlikli(
        string db = "db-FIRMA01_2027",
        long? firmaId = 1,
        long? maliId = 10,
        string? kurulum = KurulumA,
        string? makine = MakineA,
        string? version = "1.1.15.1430",
        KullaniciRolTip? rol = KullaniciRolTip.Yönetici) => new()
        {
            DatabaseName = db,
            BackupFileName = db + "_20240101_120000_abcd.backup",
            KimlikFirmaId = firmaId,
            KimlikMaliDonemId = maliId,
            KimlikKurulumId = kurulum,
            KimlikMakineId = makine,
            KimlikVersion = version,
            KimlikRol = rol
        };

    [Fact]
    public void Kimliksiz_yedek_Warning_doner()
    {
        var yedek = new DatabaseBackupResult { DatabaseName = "db-FIRMA01_2027", BackupFileName = "db-FIRMA01_2027_20240101_120000_abcd.backup" };
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(), KurulumA, MakineA, "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.Warning);
        verdict.IsKimliksiz.Should().BeTrue();
    }

    [Fact]
    public void Firma_uyusmazligi_RequireCode()
    {
        var yedek = YedekKimlikli(firmaId: 99);
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(firmaId: 1), KurulumA, MakineA, "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.RequireCode);
        verdict.Baslik.Should().Contain("Firma");
    }

    [Fact]
    public void MaliDonem_uyusmazligi_RequireCode()
    {
        var yedek = YedekKimlikli(maliId: 99);
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(id: 10), KurulumA, MakineA, "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.RequireCode);
        verdict.Baslik.Should().Contain("Mali dönem");
    }

    [Fact]
    public void DatabaseName_uyusmazligi_RequireCode()
    {
        var yedek = YedekKimlikli(db: "db-FIRMA01_2028");
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(db: "db-FIRMA01_2027"), KurulumA, MakineA, "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.RequireCode);
    }

    [Fact]
    public void Version_daha_yeni_Block()
    {
        var yedek = YedekKimlikli(version: "1.2.0.0");
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(), KurulumA, MakineA, hedefVersion: "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.Block);
        verdict.Engelli.Should().BeTrue();
    }

    [Fact]
    public void Version_daha_eski_Warning()
    {
        var yedek = YedekKimlikli(version: "1.0.0.0");
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(), KurulumA, MakineA, hedefVersion: "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.Warning);
        verdict.Baslik.Should().Contain("Eski sürüm");
    }

    [Fact]
    public void Kurulum_farkli_Warning()
    {
        var yedek = YedekKimlikli(kurulum: KurulumB);
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(), KurulumA, MakineA, "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.Warning);
        verdict.Baslik.Should().Contain("Taşınmış");
    }

    [Fact]
    public void Makine_farkli_Warning()
    {
        var yedek = YedekKimlikli(makine: MakineB);
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(), KurulumA, MakineA, "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.Warning);
    }

    [Fact]
    public void Tum_uyumlu_Allow()
    {
        var yedek = YedekKimlikli();
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(), KurulumA, MakineA, "1.1.15.1430");
        verdict.Kind.Should().Be(RestoreVerdictKind.Allow);
    }

    [Fact]
    public void HedefVersion_null_ise_Version_kontrol_atlanir()
    {
        var yedek = YedekKimlikli(version: "9.9.9.9");
        var verdict = RestoreVerdictEvaluator.Evaluate(yedek, Hedef(), KurulumA, MakineA, hedefVersion: null);
        verdict.Kind.Should().Be(RestoreVerdictKind.Allow);
    }
}
