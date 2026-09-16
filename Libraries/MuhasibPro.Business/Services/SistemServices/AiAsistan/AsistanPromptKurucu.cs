using System.Text;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;

namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Faz 6.92 RAG v1: sistem promptu + bağlam + madde listesi + kırpılmış geçmiş kurar. Saf fonksiyondur.</summary>
public static class AsistanPromptKurucu
{
    /// <summary>Sohbet mesajı. Rol: "system" / "user" / "assistant" (model sözleşmesi).</summary>
    public record SohbetMesaji(string Rol, string Icerik);

    public static IReadOnlyList<SohbetMesaji> MesajlariKur(
        IReadOnlyList<YardimSkorlayici.Eslesme> bulunanlar,
        AsistanSoruDto soru,
        int maksGecmisTur)
    {
        if (soru is null)
            throw new ArgumentNullException(nameof(soru));
        if (string.IsNullOrWhiteSpace(soru.Soru))
            throw new ArgumentException("Soru boş olamaz.", nameof(soru));

        var sistem = new StringBuilder();
        sistem.Append("MuhasibPro uygulama yardım asistanısın. Yalnız aşağıdaki yardım maddelerine dayanarak cevap ver; ");
        sistem.Append("maddelerde yoksa 'Bu konuda yardım maddesi yok.' de ve uydurma. Kısa ve işlem odaklı cevap ver, Türkçe yaz.");

        var baglam = new List<string>();
        if (!string.IsNullOrWhiteSpace(soru.SayfaAnahtari))
            baglam.Add($"Bulunulan sayfa: {soru.SayfaAnahtari.Trim()}");
        if (!string.IsNullOrWhiteSpace(soru.FirmaAdi))
            baglam.Add($"Firma: {soru.FirmaAdi.Trim()}");
        if (!string.IsNullOrWhiteSpace(soru.DonemAdi))
            baglam.Add($"Mali dönem: {soru.DonemAdi.Trim()}");
        if (baglam.Count > 0)
            sistem.Append(' ').Append(string.Join(" ", baglam));

        if (bulunanlar is { Count: > 0 })
        {
            sistem.Append(" Yardım maddeleri:");
            int sira = 1;
            foreach (var eslesme in bulunanlar)
            {
                sistem.Append($" {sira}. [{eslesme.Sayfa.Baslik}] {eslesme.Madde.Baslik} — {eslesme.Madde.Aciklama}");
                sira++;
            }
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
