using System.Security.Cryptography;
using System.Text;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;

namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Faz 6.93: gömülü Markdown yardım içeriğini bilgi tabanı kayıtlarına çevirir. Saf fonksiyondur.</summary>
public static class YardimMarkdownCozumleyici
{
    /// <summary>Çözümlenmiş madde (kayıt + içerik hash'i).</summary>
    public record CozulmusMadde(YardimKaydi Kayit, string IcerikHash);

    /// <summary>Tek Markdown dosyasını çözer. Format: `#` sayfa, `##` madde, `Etiket: a, b` (opsiyonel).</summary>
    public static IReadOnlyList<CozulmusMadde> Cozumle(string dosyaAdi, string hamMetin)
    {
        var sonuc = new List<CozulmusMadde>();
        if (string.IsNullOrWhiteSpace(hamMetin))
            return sonuc;

        var kok = (dosyaAdi ?? string.Empty).Trim();
        if (kok.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            kok = kok[..^3];
        if (string.IsNullOrWhiteSpace(kok))
            kok = "yardim";

        string sayfa = kok;
        string? baslik = null;
        var govde = new StringBuilder();
        var etiketler = new List<string>();
        int sira = 0;

        void Bosalt()
        {
            if (string.IsNullOrWhiteSpace(baslik))
                return;
            sira++;
            var kayit = new YardimKaydi
            {
                Anahtar = kok + "#" + sira,
                Sayfa = sayfa,
                Baslik = baslik.Trim(),
                Icerik = govde.ToString().Trim(),
                Etiketler = etiketler.AsReadOnly()
            };
            sonuc.Add(new CozulmusMadde(kayit, IcerikHashHesapla(kayit.Baslik, kayit.Icerik)));
            baslik = null;
            govde.Clear();
            etiketler = new List<string>();
        }

        foreach (var hamSatir in hamMetin.Split(['\r', '\n']))
        {
            var satir = hamSatir.Trim();
            if (satir.StartsWith("## ", StringComparison.Ordinal))
            {
                Bosalt();
                baslik = satir[3..].Trim();
            }
            else if (satir.StartsWith("# ", StringComparison.Ordinal))
            {
                Bosalt();
                var ad = satir[2..].Trim();
                if (!string.IsNullOrWhiteSpace(ad))
                    sayfa = ad;
            }
            else if (baslik is not null && satir.StartsWith("Etiket:", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var parca in satir[7..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!string.IsNullOrWhiteSpace(parca))
                        etiketler.Add(parca);
                }
            }
            else if (baslik is not null)
            {
                if (govde.Length > 0)
                    govde.Append('\n');
                govde.Append(hamSatir.Trim());
            }
        }
        Bosalt();

        return sonuc;
    }

    /// <summary>Madde içerik hash'i: SHA256(Baslik + "\n" + Icerik), hex.</summary>
    public static string IcerikHashHesapla(string baslik, string icerik)
    {
        var ham = (baslik ?? string.Empty) + "\n" + (icerik ?? string.Empty);
        var bayt = SHA256.HashData(Encoding.UTF8.GetBytes(ham));
        return Convert.ToHexString(bayt);
    }
}
