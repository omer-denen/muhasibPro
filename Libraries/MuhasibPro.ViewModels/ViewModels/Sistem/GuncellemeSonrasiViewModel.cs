using System.Collections.ObjectModel;
using MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Models.PostUpdateModel;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem;

/// <summary>Faz 6.91-D: uygulama güncellendikten sonra açılışta çalışan doğrulama ekranı.
/// Sonuç hero'su üstte; altında üç adımlı şerit (Uygulama → Sistem.db → Dönemler).
/// Temizse otomatik Login'e; uyarıda "Devam Et"; uygulama hatasında "Kapat"; Sistem.db hatasında "Veritabanı Yönetimi".</summary>
public class GuncellemeSonrasiViewModel : ViewModelBase
{
    private const string AdUygulama = "Uygulama Dosyaları";
    private const string AdSistemDb = "Sistem Veritabanı";
    private const string AdDonemler = "Mali Dönem Veritabanları";

    private readonly IPostUpdateDogrulamaService _saga;
    private readonly Dictionary<string, PostUpdateAdimGorunum> _adimSozluk = new(StringComparer.Ordinal);

    public GuncellemeSonrasiViewModel(
        IPostUpdateDogrulamaService saga,
        ICommonServices commonServices) : base(commonServices)
    {
        _saga = saga;

        Adimlar = new ObservableCollection<PostUpdateAdimGorunum>
        {
            new(AdUygulama, "Uygulama", "Kritik dosyalar ve sürüm"),
            new(AdSistemDb, "Sistem.db", "Şema uyumluluğu, güncelleme ve doğrulama"),
            new(AdDonemler, "Dönemler", "Dönem taraması ve bozuk kurtarma")
        };
        for (int i = 0; i < Adimlar.Count; i++)
        {
            Adimlar[i].IlkMi = i == 0;
            Adimlar[i].SonMu = i == Adimlar.Count - 1;
            _adimSozluk[Adimlar[i].Ad] = Adimlar[i];
        }
    }

    public ObservableCollection<PostUpdateAdimGorunum> Adimlar { get; }

    public ObservableCollection<string> RaporSatirlari { get; } = new();

    private double _progressValue;
    public double ProgressValue
    {
        get => _progressValue;
        set => Set(ref _progressValue, value);
    }

    private string _statusMessage = "Doğrulama başlatılıyor...";
    public string StatusMessage
    {
        get => _statusMessage;
        set => Set(ref _statusMessage, value);
    }

    private bool _sonucHazir;
    public bool SonucHazir
    {
        get => _sonucHazir;
        set => Set(ref _sonucHazir, value);
    }

    private PostUpdateSonucTuru _sonucTuru = PostUpdateSonucTuru.Temiz;
    public PostUpdateSonucTuru SonucTuru
    {
        get => _sonucTuru;
        set
        {
            if (Set(ref _sonucTuru, value))
            {
                NotifyPropertyChanged(nameof(TemizMi));
                NotifyPropertyChanged(nameof(DikkatMi));
                NotifyPropertyChanged(nameof(UygulamaBasarisizMi));
                NotifyPropertyChanged(nameof(SistemBasarisizMi));
                NotifyPropertyChanged(nameof(DevamEtGorunur));
                NotifyPropertyChanged(nameof(UygulamaHatasiGorunur));
                NotifyPropertyChanged(nameof(SistemHatasiGorunur));
                NotifyPropertyChanged(nameof(Bloklayici));
                NotifyPropertyChanged(nameof(DevamEdilebilir));
            }
        }
    }

    /// <summary>Hero başlığı — saga'dan gelen kullanıcı metni (Kural 7 — karar veriden).</summary>
    private string _sonucBaslik = string.Empty;
    public string SonucBaslik
    {
        get => _sonucBaslik;
        set => Set(ref _sonucBaslik, value);
    }

    /// <summary>Hero açıklaması — tek cümlelik özet/sebep (Kural 12 — tek özet).</summary>
    private string _sonucAciklama = string.Empty;
    public string SonucAciklama
    {
        get => _sonucAciklama;
        set => Set(ref _sonucAciklama, value);
    }

    private bool _uyariVar;
    public bool UyariVar
    {
        get => _uyariVar;
        set => Set(ref _uyariVar, value);
    }

    // ---- Hero durumundan türeyen görünürlükler ----
    public bool TemizMi => _sonucTuru == PostUpdateSonucTuru.Temiz;
    public bool DikkatMi => _sonucTuru == PostUpdateSonucTuru.Dikkat;
    public bool UygulamaBasarisizMi => _sonucTuru == PostUpdateSonucTuru.UygulamaBasarisiz;
    public bool SistemBasarisizMi => _sonucTuru == PostUpdateSonucTuru.SistemBasarisiz;

    public bool DevamEtGorunur => TemizMi || DikkatMi;
    public bool UygulamaHatasiGorunur => UygulamaBasarisizMi;
    public bool SistemHatasiGorunur => SistemBasarisizMi;

    /// <summary>Temiz sonuç → View kısa bir an sonra otomatik Login'e geçer.</summary>
    public bool OtomatikGecis => TemizMi;

    /// <summary>Sert blok (yalnız Sistem.db) → giriş kapalı, yönetim ekranına yönlendirilir.</summary>
    public bool Bloklayici => SistemBasarisizMi;

    /// <summary>Sonuç hazır ve sert blok yok → Login'e (veya yönetime) devam edilebilir.</summary>
    public bool DevamEdilebilir => SonucHazir && !Bloklayici;

    /// <summary>Sayfa yüklenince çağrılır. Dönen sonuç: View navigasyon kapısı için.</summary>
    public async Task<PostUpdateDogrulamaSonucu?> CalistirAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return null;

        IsBusy = true;
        ResetView();

        try
        {
            var ilerleme = new Progress<PostUpdateAdimSonucu>(OnIlerleme);
            var sonuc = await _saga.CalistirAsync(ilerleme, cancellationToken);
            ApplyResult(sonuc);
            return sonuc;
        }
        catch (OperationCanceledException)
        {
            ShowUnexpected("Doğrulama zaman aşımına uğradı.", "Zaman aşımı");
            return null;
        }
        catch (Exception ex)
        {
            ShowUnexpected(ex.Message, "Beklenmeyen hata");
            return null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetView()
    {
        UyariVar = false;
        SonucHazir = false;
        SonucBaslik = string.Empty;
        SonucAciklama = string.Empty;
        RaporSatirlari.Clear();
        ProgressValue = 0;
        StatusMessage = "Doğrulama başlatılıyor...";
        foreach (var adim in Adimlar)
        {
            adim.Durum = PostUpdateAdimDurumu.Bekliyor;
            adim.Mesaj = string.Empty;
        }
    }

    private void ApplyResult(PostUpdateDogrulamaSonucu sonuc)
    {
        SonucTuru = sonuc.SonucTuru;
        SonucBaslik = sonuc.Baslik;
        SonucAciklama = sonuc.Ozet;
        UyariVar = sonuc.SonucTuru == PostUpdateSonucTuru.Dikkat;

        // Adım rozetlerini saga sonucundan DOĞRUDAN yaz: Progress<T> kuyruğunda geciken
        // bildirimler ApplyResult sonrası (ProgressValue=100 ile) düşürülür; terminal durum
        // yalnız bu yolla garanti edilir (Kural 12 — adım izi güvenilirliği).
        foreach (var adim in sonuc.Adimlar)
        {
            if (_adimSozluk.TryGetValue(adim.Ad, out var gorunum))
            {
                gorunum.Durum = adim.Durum;
                gorunum.Mesaj = adim.Mesaj;
            }
        }

        foreach (var satir in sonuc.RaporSatirlari)
            RaporSatirlari.Add(satir);

        // Uyarı veren adımların mesajı "Dikkat gerektiren durumlar" listesinde görünür;
        // hata adımlarının sebebi hero açıklamasında zaten taşınır (adım şeridi yalnız rozet taşır).
        foreach (var adim in sonuc.Adimlar.Where(a => a.Durum == PostUpdateAdimDurumu.Uyari
                     && !string.IsNullOrWhiteSpace(a.Mesaj)))
        {
            var satir = $"{adim.Ad}: {adim.Mesaj}";
            if (!RaporSatirlari.Contains(satir))
                RaporSatirlari.Add(satir);
        }

        ProgressValue = 100;
        StatusMessage = sonuc.SonucTuru == PostUpdateSonucTuru.SistemBasarisiz ? "Doğrulama tamamlanamadı" : "Doğrulama tamamlandı";
        SonucHazir = true;
    }

    /// <summary>Saga beklenmedik şekilde düştüğünde ekranı yine de sonuç durumuna getirir (sert blok).</summary>
    private void ShowUnexpected(string aciklama, string durum)
    {
        SonucTuru = PostUpdateSonucTuru.SistemBasarisiz;
        SonucBaslik = "Doğrulama tamamlanamadı";
        SonucAciklama = aciklama;
        StatusMessage = durum;
        SonucHazir = true;
    }

    private void OnIlerleme(PostUpdateAdimSonucu adim)
    {
        // Kural 12: bar/adım geri sarmaz — geç gelen (stale) ilerleme bildirimi atlanır.
        if (adim.Yuzde < ProgressValue)
            return;

        if (_adimSozluk.TryGetValue(adim.Ad, out var gorunum))
        {
            // Geç gelen "devam ediyor" bildirimi terminal (basarılı/uyarı/atlandı/hata) adımı geri alamaz.
            bool terminalMi = gorunum.Durum is PostUpdateAdimDurumu.Basarili
                or PostUpdateAdimDurumu.Uyari
                or PostUpdateAdimDurumu.Atlandi
                or PostUpdateAdimDurumu.Hata;
            if (adim.Durum == PostUpdateAdimDurumu.DevamEdiyor && terminalMi)
                return;

            gorunum.Durum = adim.Durum;
            if (!string.IsNullOrWhiteSpace(adim.Mesaj))
                gorunum.Mesaj = adim.Mesaj;
        }

        if (adim.Yuzde > ProgressValue)
            ProgressValue = adim.Yuzde;
        // Durum satırı yalnız adım başlangıç mesajlarını izler (bitiş/alt-ilerleme mesajları
        // sonuç panelinde görünür) — sönümle animasyonu kuyruğu şişmez, son mesaj gecikmez.
        if (adim.Durum == PostUpdateAdimDurumu.DevamEdiyor && !string.IsNullOrWhiteSpace(adim.Mesaj))
            StatusMessage = adim.Mesaj;
    }
}
