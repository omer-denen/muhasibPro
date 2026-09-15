using FluentAssertions;
using MuhasibPro.Data.Database.Common.Helpers;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.91-C: ileri-uyumluluk guard — disk şeması binary'den yeniyse fail-closed.
/// DbSchemaVersions karşılaştırması + analiz sonucu FutureSchema durumu.</summary>
public class IleriUyumlulukGuardTests
{
    [Theory]
    [InlineData("1.1.0", false)]   // aynı sürüm
    [InlineData("1.0.0", false)]   // eski sürüm (göç edilebilir)
    [InlineData("1.2.0", true)]    // daha yeni minor
    [InlineData("2.0.0", true)]    // daha yeni major
    [InlineData("1.10.0", true)]   // sayısal karşılaştırma (1.10 > 1.1)
    [InlineData(null, false)]      // sürüm yok
    [InlineData("", false)]        // boş
    public void DbSchemaVersions_IleriUyumluluk(string? disktekiSurum, bool beklenen)
    {
        DbSchemaVersions.IsNewerThanSupported(disktekiSurum).Should().Be(beklenen);
    }

    [Fact]
    public void Analiz_IleriSemada_FutureSchema_Durumu_Doner()
    {
        var analiz = new DatabaseConnectionAnalysis
        {
            DatabaseName = "Sistem.db",
            IsDatabaseExists = true,
            CanConnect = true,
            DatabaseValid = false,
            IsFutureSchema = true,
            HasError = true,
            CurrentVersion = "1.2.0"
        };

        analiz.GetStatus().Should().Be(DatabaseStatusResult.FutureSchema);
        analiz.GetStatusMessage().Should().Contain("daha yeni").And.Contain("1.2.0");
    }

    [Fact]
    public void Analiz_FutureSchema_Degilse_Hata_Durumu_Degismez()
    {
        var analiz = new DatabaseConnectionAnalysis
        {
            IsDatabaseExists = true,
            CanConnect = true,
            HasError = true,
            IsFutureSchema = false
        };

        analiz.GetStatus().Should().Be(DatabaseStatusResult.UnknownError);
    }
}
