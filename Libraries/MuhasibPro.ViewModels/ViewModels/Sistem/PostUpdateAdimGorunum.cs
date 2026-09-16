using MuhasibPro.Business.DTOModel;
using MuhasibPro.Domain.Models.PostUpdateModel;

namespace MuhasibPro.ViewModels.ViewModels.Sistem;

/// <summary>Güncelleme sonrası doğrulama ekranındaki adım rozeti (Kural 12 — adım rozetli akış).
/// Renk/ikon eşlemesi View'dadır (converter); bu sınıf yalnız veri taşır.</summary>
public class PostUpdateAdimGorunum : ObservableObject
{
    public PostUpdateAdimGorunum(string ad, string kisaAd, string aciklama)
    {
        Ad = ad;
        KisaAd = kisaAd;
        Aciklama = aciklama;
    }

    public string Ad { get; }

    /// <summary>Adım şeridinde gösterilen kısa etiket (tek satıra sığar).</summary>
    public string KisaAd { get; }

    public string Aciklama { get; }

    /// <summary>Adım şeridinde ilk adım mı (sol bağlayıcı gizlenir).</summary>
    public bool IlkMi { get; set; }

    /// <summary>Adım şeridinde son adım mı (sağ bağlayıcı gizlenir).</summary>
    public bool SonMu { get; set; }

    private PostUpdateAdimDurumu _durum = PostUpdateAdimDurumu.Bekliyor;
    public PostUpdateAdimDurumu Durum
    {
        get => _durum;
        set
        {
            if (Set(ref _durum, value))
            {
                NotifyPropertyChanged(nameof(DevamEdiyorMu));
                NotifyPropertyChanged(nameof(IkonGorunurMu));
                NotifyPropertyChanged(nameof(BekliyorMu));
                NotifyPropertyChanged(nameof(BasariliMi));
                NotifyPropertyChanged(nameof(UyariMi));
                NotifyPropertyChanged(nameof(AtlandiMi));
                NotifyPropertyChanged(nameof(HataMi));
            }
        }
    }

    /// <summary>Adım çalışıyor → rozet yerine dönen ProgressRing gösterilir.</summary>
    public bool DevamEdiyorMu => _durum == PostUpdateAdimDurumu.DevamEdiyor;

    /// <summary>Adım terminal/bekliyor → durum ikonu gösterilir.</summary>
    public bool IkonGorunurMu => !DevamEdiyorMu;

    // Renk/ikon View'da tema-farkında ThemeResource overlay'leriyle seçilir (converter yok — Light/Dark güvenli).
    public bool BekliyorMu => _durum == PostUpdateAdimDurumu.Bekliyor;
    public bool BasariliMi => _durum == PostUpdateAdimDurumu.Basarili;
    public bool UyariMi => _durum == PostUpdateAdimDurumu.Uyari;
    public bool AtlandiMi => _durum == PostUpdateAdimDurumu.Atlandi;
    public bool HataMi => _durum == PostUpdateAdimDurumu.Hata;

    private string _mesaj = string.Empty;
    public string Mesaj
    {
        get => _mesaj;
        set => Set(ref _mesaj, value);
    }
}
