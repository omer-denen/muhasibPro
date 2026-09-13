using FluentAssertions;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;

namespace MuhasibPro.Tests;

/// <summary>Oturum 218 (Faz 6.71/4-D1): bildirim grup sabitleri + tip etiketleri.</summary>
public class BildirimGruplariTests
{
    [Theory]
    [InlineData(NotificationType.Info, "Bilgi")]
    [InlineData(NotificationType.Success, "Başarılı")]
    [InlineData(NotificationType.Warning, "Uyarı")]
    [InlineData(NotificationType.Danger, "Hata")]
    public void Etiket_TipMetni(NotificationType tur, string beklenen)
        => NotificationGroups.Etiket(tur).Should().Be(beklenen);

    [Fact]
    public void GrupSabitleri_BosDegilVeBenzersiz()
    {
        var gruplar = new[]
        {
            NotificationGroups.Yedek,
            NotificationGroups.DonemIslemleri,
            NotificationGroups.Bakim,
            NotificationGroups.Analiz,
            NotificationGroups.Yonetim,
            NotificationGroups.Guncelleme,
            NotificationGroups.Sistem
        };
        gruplar.Should().OnlyHaveUniqueItems();
        foreach (var grup in gruplar)
        {
            grup.Should().NotBeNullOrWhiteSpace();
            NotificationGroups.Baslik(grup).Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void Baslik_BilinmeyenGruba_Duser()
        => NotificationGroups.Baslik("YokBoyleGrup").Should().Be("MuhasibPro");
}
