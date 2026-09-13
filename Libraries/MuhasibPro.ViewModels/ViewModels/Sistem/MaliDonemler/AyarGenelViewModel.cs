using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Genel bölümü: firma kartı + oluşturan + dönem sayısı. Tek iş: firma kimliğini göstermek.</summary>
public class AyarGenelViewModel : AyarBolumViewModel
{
    private readonly IFirmaService _firmaService;
    private readonly IKullaniciService _kullaniciService;

    public AyarGenelViewModel(
        ICommonServices commonServices,
        IAyarHatti hat,
        IFirmaService firmaService = null,
        IKullaniciService kullaniciService = null)
        : base(commonServices, hat)
    {
        _firmaService = firmaService;
        _kullaniciService = kullaniciService;
    }

    private FirmaModel _firma;
    /// <summary>Tam firma modeli (servis yoksa null kalır).</summary>
    public FirmaModel Firma
    {
        get => _firma;
        private set
        {
            if (Set(ref _firma, value))
            {
                NotifyPropertyChanged(nameof(FirmaDetaySatiri));
                NotifyPropertyChanged(nameof(KayitTarihiMetni));
            }
        }
    }

    /// <summary>Kart alt satırı: dolu alanlar (Yetkili/İl/Vergi/Tel/E-posta) • ile birleşir.</summary>
    public string FirmaDetaySatiri
    {
        get
        {
            if (Firma == null)
                return string.Empty;
            var vergi = string.Join(" ", new[] { Firma.VergiDairesi, Firma.VergiNo }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            return string.Join(" • ", new[] { Firma.YetkiliKisi, Firma.Il, vergi, Firma.Telefon1, Firma.Eposta }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }

    public string KayitTarihiMetni => Firma == null || Firma.KayitTarihi == default
        ? string.Empty
        : $"Kayıt: {Firma.KayitTarihi:g}";

    private int _donemSayisi;
    /// <summary>Dönem sayısı (orkestratör listeden yazar).</summary>
    public int DonemSayisi
    {
        get => _donemSayisi;
        set => Set(ref _donemSayisi, value);
    }

    private string _olusturanAdi = string.Empty;
    /// <summary>Firmayı oluşturan (best-effort; bulunamazsa "ID: n").</summary>
    public string OlusturanAdi
    {
        get => _olusturanAdi;
        private set => Set(ref _olusturanAdi, value);
    }

    public override async Task YukleAsync(long firmaId)
    {
        await base.YukleAsync(firmaId);
        if (_firmaService == null || firmaId <= 0)
            return;
        try
        {
            var sonuc = await _firmaService.GetByFirmaIdAsync(firmaId);
            if (sonuc?.Success == true && sonuc.Data != null)
            {
                Firma = sonuc.Data;
                await OlusturanCozAsync(sonuc.Data.KaydedenId);
            }
        }
        catch { }
    }

    private async Task OlusturanCozAsync(long kaydedenId)
    {
        OlusturanAdi = string.Empty;
        if (_kullaniciService == null || kaydedenId <= 0)
        {
            OlusturanAdi = kaydedenId > 0 ? $"ID: {kaydedenId}" : string.Empty;
            return;
        }
        try
        {
            var sonuc = await _kullaniciService.GetKullaniciAsync(kaydedenId);
            var model = sonuc?.Data;
            var ad = (model?.AdiSoyadi ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(ad))
                ad = (model?.KullaniciAdi ?? string.Empty).Trim();
            OlusturanAdi = string.IsNullOrWhiteSpace(ad) ? $"ID: {kaydedenId}" : ad;
        }
        catch
        {
            OlusturanAdi = $"ID: {kaydedenId}";
        }
    }
}
