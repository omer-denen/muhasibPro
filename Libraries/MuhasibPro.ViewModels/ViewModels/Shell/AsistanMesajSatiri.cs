using MuhasibPro.Business.DTOModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Sohbet satırı (Faz 6.92 Adım 5). Streaming ile içerik büyür (INPC).</summary>
public class AsistanMesajSatiri : ObservableObject
{
    public AsistanMesajSatiri(bool kullaniciMi, string icerik)
    {
        KullaniciMi = kullaniciMi;
        _icerik = icerik ?? string.Empty;
        Zaman = DateTime.Now.ToString("HH:mm:ss");
    }

    public bool KullaniciMi { get; }
    public bool AsistanMi => !KullaniciMi;
    public string Zaman { get; }

    private string _icerik;
    public string Icerik
    {
        get => _icerik;
        set => Set(ref _icerik, value);
    }

    public void ParcaEkle(string parca)
    {
        if (!string.IsNullOrEmpty(parca))
            Icerik += parca;
    }
}
