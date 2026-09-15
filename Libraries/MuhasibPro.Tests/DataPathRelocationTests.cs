using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Database.Common.Helpers;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.91-A: veri kökü taşıma (eski %LocalAppData% → yeni %AppData%).
/// Kaynak yok/yeni zaten dolu/geliştirme modu → atlar; eski veri varken yeni köke taşır.
/// Taşıma başarısızsa eski veri korunur (fail-closed).</summary>
public class DataPathRelocationTests : IDisposable
{
    private readonly string _root;
    private readonly string _legacyRoot;
    private readonly string _newRoot;

    public DataPathRelocationTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "muhasibpro-reloc-" + Guid.NewGuid().ToString("N"));
        _legacyRoot = Path.Combine(_root, "legacy");
        _newRoot = Path.Combine(_root, "new");
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* test temizliği best-effort */ }
    }

    private static void WriteSistemDb(string dataRoot)
    {
        var dbDir = Path.Combine(dataRoot, DatabaseConstants.DATABASE_FOLDER);
        Directory.CreateDirectory(dbDir);
        File.WriteAllBytes(Path.Combine(dbDir, DatabaseConstants.SISTEM_DB_NAME), new byte[256]);
    }

    private DataPathRelocationService Build(bool devMode)
    {
        var paths = new Mock<IApplicationPaths>();
        paths.Setup(p => p.GetAppDataFolderPath()).Returns(() =>
        {
            Directory.CreateDirectory(_newRoot);
            return _newRoot;
        });
        paths.Setup(p => p.GetLegacyDataRootPath()).Returns(_legacyRoot);
        paths.Setup(p => p.IsSqliteDatabaseFileValid(It.IsAny<string>()))
             .Returns<string>(f => File.Exists(f) && new FileInfo(f).Length >= DatabaseConstants.MIN_SQLITE_FILE_SIZE);

        var env = new Mock<IEnvironmentDetector>();
        env.Setup(e => e.IsDevelopment()).Returns(devMode);

        return new DataPathRelocationService(paths.Object, env.Object, Mock.Of<ILogger<DataPathRelocationService>>());
    }

    [Fact]
    public async Task Gelistirme_Modunda_Tasimaz()
    {
        WriteSistemDb(_legacyRoot);

        var sonuc = await Build(devMode: true).EnsureRelocatedAsync();

        sonuc.basarili.Should().BeTrue();
        sonuc.tasindi.Should().BeFalse();
        File.Exists(Path.Combine(_legacyRoot, DatabaseConstants.DATABASE_FOLDER, DatabaseConstants.SISTEM_DB_NAME)).Should().BeTrue();
    }

    [Fact]
    public async Task Eski_Veri_Yoksa_Tasimaz()
    {
        var sonuc = await Build(devMode: false).EnsureRelocatedAsync();

        sonuc.basarili.Should().BeTrue();
        sonuc.tasindi.Should().BeFalse();
        File.Exists(Path.Combine(_newRoot, DatabaseConstants.DATABASE_FOLDER, DatabaseConstants.SISTEM_DB_NAME)).Should().BeFalse();
    }

    [Fact]
    public async Task Yeni_Konumda_Veri_Varsa_Eskiye_Dokunmaz()
    {
        WriteSistemDb(_legacyRoot);
        WriteSistemDb(_newRoot);

        var sonuc = await Build(devMode: false).EnsureRelocatedAsync();

        sonuc.basarili.Should().BeTrue();
        sonuc.tasindi.Should().BeFalse();
        File.Exists(Path.Combine(_legacyRoot, DatabaseConstants.DATABASE_FOLDER, DatabaseConstants.SISTEM_DB_NAME)).Should().BeTrue();
    }

    [Fact]
    public async Task Eski_Veriyi_Yeni_Koke_Tasir()
    {
        WriteSistemDb(_legacyRoot);
        var yeniSistem = Path.Combine(_newRoot, DatabaseConstants.DATABASE_FOLDER, DatabaseConstants.SISTEM_DB_NAME);
        var eskiDb = Path.Combine(_legacyRoot, DatabaseConstants.DATABASE_FOLDER);

        var sonuc = await Build(devMode: false).EnsureRelocatedAsync();

        sonuc.basarili.Should().BeTrue();
        sonuc.tasindi.Should().BeTrue();
        File.Exists(yeniSistem).Should().BeTrue();
        Directory.Exists(eskiDb).Should().BeFalse();
    }

    [Fact]
    public async Task Legacy_Kok_Yoksa_Tasimaz()
    {
        var paths = new Mock<IApplicationPaths>();
        paths.Setup(p => p.GetLegacyDataRootPath()).Returns((string?)null);
        var env = new Mock<IEnvironmentDetector>();
        env.Setup(e => e.IsDevelopment()).Returns(false);
        var servis = new DataPathRelocationService(paths.Object, env.Object, Mock.Of<ILogger<DataPathRelocationService>>());

        var sonuc = await servis.EnsureRelocatedAsync();

        sonuc.basarili.Should().BeTrue();
        sonuc.tasindi.Should().BeFalse();
    }
}
