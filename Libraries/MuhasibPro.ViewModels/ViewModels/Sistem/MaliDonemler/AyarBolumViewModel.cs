using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Runtime.CompilerServices;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Çocuk ayar VM'lerinin orkestratöre açtığı dar kanal (bar/yayın/ad).</summary>
public interface IAyarHatti
{
    string GorunenAd();
    void Yayinla(string anahtar);
    void HataBildir(string metin);
    void HatayiTemizle();
    void CakismaGoster(string metin, Func<Task> bekleyenYazim, AyarBolumViewModel kaynak);
    void KaydedildiBildir(bool sayfaBoyutuDegisti);
}

/// <summary>Ayar bölümü tabanı: FirmaId + anında-kayıt yardımcısı. Bar/yayın orkestratörde.</summary>
public abstract class AyarBolumViewModel : ViewModelBase
{
    protected readonly IAyarHatti Hat;

    public long FirmaId { get; private set; }

    protected AyarBolumViewModel(ICommonServices commonServices, IAyarHatti hat)
        : base(commonServices)
    {
        Hat = hat;
    }

    public virtual Task YukleAsync(long firmaId)
    {
        FirmaId = firmaId;
        return Task.CompletedTask;
    }

    /// <summary>Alan değişince hatayı temizleyip kaydı ateşler (ateşle-unut).</summary>
    protected void AlanDegisti<T>(ref T alan, T deger, Func<Task> kaydet, [CallerMemberName] string prop = null)
    {
        if (Set(ref alan, deger, prop))
        {
            Hat.HatayiTemizle();
            _ = kaydet();
        }
    }
}
