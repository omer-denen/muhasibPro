using FluentAssertions;
using MuhasibPro.Domain.Entities;
using System.Text.RegularExpressions;

namespace MuhasibPro.Tests;

/// <summary>KaydedenId varsayılanı tek kaynaktan gelir — sihirli sayı bekçisi.
/// Kapsam: üretim kodu (Migrations donmuş EF tarihi + test fixture'ları hariç).</summary>
public class KaydedenIdVarsayilanTests
{
    private static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (File.Exists(Path.Combine(dir, "MuhasibPro.slnx"))) return dir;
            dir = Path.GetDirectoryName(dir)!;
        }
        return AppContext.BaseDirectory;
    }

    [Fact]
    public void SeedYoneticiId_Degeri_Sabitlidir()
    {
        KullaniciSabitleri.SeedYoneticiId.Should().Be(5413300800L);
    }

    [Fact]
    public void KaydedenId_Sihirli_Sayi_Icermez()
    {
        var kok = Path.Combine(RepoRoot(), "Libraries");
        var desen = new Regex(@"KaydedenId\s*=\s*(1(?!\d)|241341L|0000000800|24134175366|5413300800L?)\s*[,;]");
        var ihlaller = new List<string>();

        foreach (var dosya in Directory.EnumerateFiles(kok, "*.cs", SearchOption.AllDirectories))
        {
            if (dosya.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"))
                continue;
            if (dosya.Contains($"{Path.DirectorySeparatorChar}MuhasibPro.Tests{Path.DirectorySeparatorChar}"))
                continue;
            if (Path.GetFileName(dosya) == "KullaniciSabitleri.cs")
                continue;
            var eslesme = desen.Match(File.ReadAllText(dosya));
            if (eslesme.Success)
                ihlaller.Add($"{dosya}: {eslesme.Value.Trim()}");
        }

        ihlaller.Should().BeEmpty("KaydedenId varsayılanı KullaniciSabitleri.SeedYoneticiId tek kaynağından gelmeli");
    }
}
