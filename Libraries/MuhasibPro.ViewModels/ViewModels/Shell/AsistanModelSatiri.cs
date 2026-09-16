using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Model yönetimi listesi satırı (Faz 6.92 — Denetim "Model yönetimi").
/// Tek cümle: servis DTO'sunu görüntü metinlerine çevirir (başlık + okunur boyut).</summary>
public class AsistanModelSatiri
{
    public AsistanModelSatiri(AsistanModelDto dto)
    {
        Alias = dto.Alias;
        GosterimAdi = dto.GosterimAdi;
        YukluMu = dto.YukluMu;
        BoyutMetni = BaytMetni(dto.BoyutBayt);
    }

    public string Alias { get; }

    public string GosterimAdi { get; }

    public bool YukluMu { get; }

    /// <summary>Okunur disk boyutu ("—" ölçülemediyse).</summary>
    public string BoyutMetni { get; }

    /// <summary>Görünen ad ile alias aynıysa tek, değilse "ad (alias)".</summary>
    public string Baslik => string.Equals(GosterimAdi, Alias, StringComparison.OrdinalIgnoreCase)
        ? Alias
        : $"{GosterimAdi} ({Alias})";

    /// <summary>Baytı okunur metne çevirir (KB/MB/GB/TB; 0/null → "—").</summary>
    public static string BaytMetni(long? bayt)
    {
        if (bayt is not > 0)
            return "—";
        double deger = bayt.Value;
        string[] birimler = ["B", "KB", "MB", "GB", "TB"];
        int birim = 0;
        while (deger >= 1024 && birim < birimler.Length - 1)
        {
            deger /= 1024;
            birim++;
        }
        return birim == 0 ? $"{deger:0} {birimler[birim]}" : $"{deger:0.#} {birimler[birim]}";
    }
}
