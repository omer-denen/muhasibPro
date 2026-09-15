using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.SistemServices.DevServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Services.SistemServices.DevServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.82 geliştirici araçları: kimlik onarım/sıfırlama, transfer taraması, log seviyesi ve yol tek kaynağı.</summary>
public class DevAraclariTests
{
    private const string KurulumId = "KURULUM_MEVCUT";
    private const string MakineId = "MAKINE_MEVCUT";

    [Fact]
    public async Task KimligiOnar_MakineFarkliHariç_YalnizEslesenleriHizalar()
    {
        var kurulum = new Mock<IKurulumKayitService>();
        kurulum.Setup(k => k.GetOrCreateAsync())
            .ReturnsAsync(new KurulumKayitModel { KurulumId = KurulumId, MachineGuid = "MG" });
        var makine = new Mock<IMakineKimligiProvider>();
        makine.Setup(m => m.GetMachineIdAsync()).ReturnsAsync(MakineId);

        var reader = new Mock<ITenantVersionReader>();
        reader.Setup(r => r.ScanMismatchesAsync(KurulumId, MakineId))
            .ReturnsAsync(new List<TenantMismatchInfo>
            {
                new() { DatabaseName = "db_AyniMakine", MakineFarkli = false, KurulumFarkli = true },
                new() { DatabaseName = "db_FarkliMakine", MakineFarkli = true, KurulumFarkli = true }
            });

        var hizalanan = new List<string>();
        var tenant = new Mock<ITenantSQLiteDatabaseService>();
        tenant.Setup(t => t.ReAlignTenantKurulumIdsAsync(It.IsAny<IReadOnlyCollection<string>>()))
            .Callback<IReadOnlyCollection<string>>(n => hizalanan.AddRange(n))
            .ReturnsAsync(1);

        var servis = new DevAraclariService(
            kurulum.Object, makine.Object, reader.Object, tenant.Object,
            Mock.Of<ISplashRoutingService>(), Mock.Of<ISistemLogService>(),
            new LogSeviyesiYoneticisi(), Mock.Of<IApplicationPaths>());

        var sonuc = await servis.KimligiOnarAsync();

        sonuc.Basarili.Should().BeTrue();
        hizalanan.Should().ContainSingle().Which.Should().Be("db_AyniMakine");
        hizalanan.Should().NotContain("db_FarkliMakine");
    }

    [Fact]
    public async Task KimligiSifirla_YeniKimlikUretir_VeMakinesiAyniDamgalariHizalar()
    {
        var yeniId = string.Empty;
        var kurulum = new Mock<IKurulumKayitService>();
        kurulum.Setup(k => k.GetOrCreateAsync())
            .ReturnsAsync(new KurulumKayitModel { KurulumId = KurulumId, MachineGuid = "MG" });
        kurulum.Setup(k => k.UpdateKurulumIdAsync(It.IsAny<string>()))
            .Callback<string>(id => yeniId = id)
            .Returns(Task.CompletedTask);

        var makine = new Mock<IMakineKimligiProvider>();
        makine.Setup(m => m.GetMachineIdAsync()).ReturnsAsync(MakineId);

        var reader = new Mock<ITenantVersionReader>();
        reader.Setup(r => r.GetStampsAsync())
            .ReturnsAsync(new List<TenantDamgaInfo>
            {
                new() { DatabaseName = "db_Ayni", MakineId = MakineId, KurulumId = KurulumId },
                new() { DatabaseName = "db_Farkli", MakineId = "BaskaMakine", KurulumId = KurulumId }
            });

        var hizalanan = new List<string>();
        var tenant = new Mock<ITenantSQLiteDatabaseService>();
        tenant.Setup(t => t.ReAlignTenantKurulumIdsAsync(It.IsAny<IReadOnlyCollection<string>>()))
            .Callback<IReadOnlyCollection<string>>(n => hizalanan.AddRange(n))
            .ReturnsAsync(1);

        var servis = new DevAraclariService(
            kurulum.Object, makine.Object, reader.Object, tenant.Object,
            Mock.Of<ISplashRoutingService>(), Mock.Of<ISistemLogService>(),
            new LogSeviyesiYoneticisi(), Mock.Of<IApplicationPaths>());

        var sonuc = await servis.KimligiSifirlaAsync();

        sonuc.Basarili.Should().BeTrue();
        yeniId.Should().HaveLength(32, "Guid N formatı");
        yeniId.Should().NotBe(KurulumId);
        hizalanan.Should().ContainSingle().Which.Should().Be("db_Ayni");
        hizalanan.Should().NotContain("db_Farkli");
    }

    [Fact]
    public async Task TransferTaramasi_FarkVarsa_OzetDoner()
    {
        var splash = new Mock<ISplashRoutingService>();
        splash.Setup(s => s.CheckTransferAsync())
            .ReturnsAsync(new TransferCheckResult { Mismatches = new[] { "a", "b" } });

        var servis = new DevAraclariService(
            Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
            Mock.Of<ITenantVersionReader>(), Mock.Of<ITenantSQLiteDatabaseService>(),
            splash.Object, Mock.Of<ISistemLogService>(),
            new LogSeviyesiYoneticisi(), Mock.Of<IApplicationPaths>());

        var sonuc = await servis.TransferTaramasiAsync();

        sonuc.Basarili.Should().BeTrue();
        sonuc.Mesaj.Should().Contain("2 dönem");
    }

    [Fact]
    public async Task AyrintiliLogAyarla_EsikDegisir()
    {
        var yonetici = new LogSeviyesiYoneticisi();
        try
        {
            var servis = new DevAraclariService(
                Mock.Of<IKurulumKayitService>(), Mock.Of<IMakineKimligiProvider>(),
                Mock.Of<ITenantVersionReader>(), Mock.Of<ITenantSQLiteDatabaseService>(),
                Mock.Of<ISplashRoutingService>(), Mock.Of<ISistemLogService>(),
                yonetici, Mock.Of<IApplicationPaths>());

            await servis.AyrintiliLogAyarlaAsync(true);
            yonetici.AyrintiliAktif.Should().BeTrue();

            await servis.AyrintiliLogAyarlaAsync(false);
            yonetici.AyrintiliAktif.Should().BeFalse();
        }
        finally
        {
            yonetici.AyrintiliAyarla(false);
        }
    }

    [Fact]
    public void LogDosyaYolu_BugununDosyasi_LogsAltinda()
    {
        var dosya = LogDosyaYolu.BugununDosyasi;

        Path.GetFileName(Path.GetDirectoryName(dosya)!).Should().Be("logs");
        Path.GetFileName(dosya).Should().Be($"muhasib-{DateTime.Now:yyyyMMdd}.txt");
    }
}
