using System.Text;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;

namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Faz 6.93: sistem promptu + bağlam + bilgi tabanı maddeleri + kırpılmış geçmiş kurar. Saf fonksiyondur.</summary>
public static class AsistanPromptKurucu
{
    /// <summary>Sohbet mesajı. Rol: "system" / "user" / "assistant" (model sözleşmesi).</summary>
    public record SohbetMesaji(string Rol, string Icerik);

    /// <summary>Tek yardım maddesi gövdesinin prompt'a giren en fazla karakteri (bağlam bütçesi, H4).</summary>
    public const int MaddeIcerikSinir = 400;

    /// <summary>Bilgi tabanı arama sonuçlarıyla kurar (uyarı-strip, bağlam, geçmiş kırpma aynı).</summary>
    public static IReadOnlyList<SohbetMesaji> AramaSonuclariylaKur(
        IReadOnlyList<YardimAramaSonucu>? bulunanlar,
        AsistanSoruDto soru,
        int maksGecmisTur)
    {
        if (soru is null)
            throw new ArgumentNullException(nameof(soru));
        if (string.IsNullOrWhiteSpace(soru.Soru))
            throw new ArgumentException("Soru boş olamaz.", nameof(soru));

        var sistem = new StringBuilder();
        sistem.Append("Sen MuhasibPro adlı ön muhasebe uygulamasının yardım asistanısın. ");
        sistem.Append("Yalnız aşağıda verilen yardım maddelerini kullan; maddelerde olmayan hiçbir bilgiyi ekleme, tahmin etme, genel muhasebe tavsiyesi verme. ");
        sistem.Append("Türkçe, kısa (en çok 3 cümle) ve işlem odaklı cevap ver; gerekiyorsa adımları '1) 2)' diye yaz. ");
        sistem.Append("Cevabını dayandırdığın maddeyi köşeli parantezle belirt (ör. [1]). ");

        var baglam = new List<string>();
        if (!string.IsNullOrWhiteSpace(soru.SayfaAnahtari))
            baglam.Add($"Bulunulan sayfa: {soru.SayfaAnahtari.Trim()}");
        if (!string.IsNullOrWhiteSpace(soru.FirmaAdi))
            baglam.Add($"Firma: {soru.FirmaAdi.Trim()}");
        if (!string.IsNullOrWhiteSpace(soru.DonemAdi))
            baglam.Add($"Mali dönem: {soru.DonemAdi.Trim()}");
        if (baglam.Count > 0)
            sistem.Append(string.Join(" ", baglam)).Append(' ');

        if (bulunanlar is { Count: > 0 })
        {
            sistem.Append("Yardım maddeleri:");
            int sira = 1;
            foreach (var sonuc in bulunanlar)
            {
                var icerik = (sonuc.Icerik ?? string.Empty).Trim();
                if (icerik.Length > MaddeIcerikSinir)
                    icerik = icerik[..MaddeIcerikSinir].TrimEnd() + "…";
                sistem.Append($" [{sira}] ({sonuc.Sayfa}) {sonuc.Baslik}: {icerik}");
                sira++;
            }
            sistem.Append(" Sorunun cevabı bu maddelerde yoksa yalnız şu cümleyi yaz: 'Bu konuda yardım maddesi yok.'");
        }
        else
        {
            sistem.Append("Hiç uygun yardım maddesi bulunamadı. Yalnız şu cümleyi yaz: 'Bu konuda yardım maddesi yok.'");
        }

        var mesajlar = new List<SohbetMesaji> { new("system", sistem.ToString()) };

        if (soru.Gecmis is { Count: > 0 } && maksGecmisTur > 0)
        {
            int alinacak = Math.Min(soru.Gecmis.Count, maksGecmisTur * 2);
            for (int i = soru.Gecmis.Count - alinacak; i < soru.Gecmis.Count; i++)
            {
                var onceki = soru.Gecmis[i];
                if (onceki is null || string.IsNullOrWhiteSpace(onceki.Icerik))
                    continue;
                var rol = onceki.Rol == "asistan" ? "assistant" : "user";
                mesajlar.Add(new SohbetMesaji(rol, onceki.Icerik));
            }
        }

        mesajlar.Add(new SohbetMesaji("user", soru.Soru.Trim()));
        return mesajlar;
    }
}
