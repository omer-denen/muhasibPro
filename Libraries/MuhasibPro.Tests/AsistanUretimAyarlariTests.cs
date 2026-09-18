using FluentAssertions;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

/// <summary>S2 deterministik üretim (Oturum 292): sıcaklık/tohum sabitleri sürüklenmeye karşı kilitlidir.</summary>
public class AsistanUretimAyarlariTests
{
    [Fact]
    public void UretimSicakligi_Sifir_Olur()
    {
        AiAsistanSettings.UretimSicakligi.Should().Be(0f);
    }

    [Fact]
    public void UretimSabitTohumu_SabitKalir()
    {
        AiAsistanSettings.UretimSabitTohumu.Should().Be(292291);
    }
}
