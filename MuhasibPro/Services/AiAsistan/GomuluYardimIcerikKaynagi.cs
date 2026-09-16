using System.Reflection;
using System.Text;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;

namespace MuhasibPro.Services.AiAsistan;

/// <summary>Faz 6.93: gömülü Markdown yardım içeriği okuyucusu (EmbeddedResource).</summary>
public sealed class GomuluYardimIcerikKaynagi : IYardimIcerikKaynagi
{
    private const string KaynakOneki = "MuhasibPro.Yardim.";

    public IReadOnlyList<YardimHamKaynak> KaynaklariGetir()
    {
        var sonuc = new List<YardimHamKaynak>();
        var derleme = typeof(GomuluYardimIcerikKaynagi).Assembly;
        foreach (var ad in derleme.GetManifestResourceNames().OrderBy(n => n, StringComparer.Ordinal))
        {
            if (!ad.StartsWith(KaynakOneki, StringComparison.Ordinal) || !ad.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                continue;
            using var akim = derleme.GetManifestResourceStream(ad);
            if (akim is null)
                continue;
            using var okuyucu = new StreamReader(akim, Encoding.UTF8);
            sonuc.Add(new YardimHamKaynak
            {
                DosyaAdi = ad[KaynakOneki.Length..],
                HamMetin = okuyucu.ReadToEnd()
            });
        }
        return sonuc;
    }
}
