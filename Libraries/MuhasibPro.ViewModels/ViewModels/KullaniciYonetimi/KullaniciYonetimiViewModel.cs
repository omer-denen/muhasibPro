using System.Collections.ObjectModel;
using System.Windows.Input;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

/// <summary>Faz 6.85 K2: Kullanıcı Yönetimi penceresi orkestratörü.
/// Tek cümle: aktif firmanın kullanıcı listesini + formu (composition) yönetir; iş kuralı servistedir.</summary>
public class KullaniciYonetimiViewModel : ViewModelBase
{
    private readonly IKullaniciService _kullaniciService;
    private readonly IFirmaWithMaliDonemSelectedService _selection;
    private readonly IAuthenticationService _auth;
    private long _firmaId;

    public KullaniciYonetimiViewModel(
        ICommonServices commonServices,
        IKullaniciService kullaniciService,
        IFirmaWithMaliDonemSelectedService selection,
        IAuthenticationService auth) : base(commonServices)
    {
        _kullaniciService = kullaniciService;
        _selection = selection;
        _auth = auth;
        Duzenle = new KullaniciDuzenleViewModel(kullaniciService);
    }

    public ObservableCollection<KullaniciModel> Kullanicilar { get; } = new();

    /// <summary>Ekle/düzenle formu (composition).</summary>
    public KullaniciDuzenleViewModel Duzenle { get; }

    private KullaniciModel _seciliKullanici;
    public KullaniciModel SeciliKullanici
    {
        get => _seciliKullanici;
        set { if (Set(ref _seciliKullanici, value)) NotifyPropertyChanged(nameof(SeciliVar)); }
    }
    public bool SeciliVar => SeciliKullanici != null;

    private string _firmaAdi = string.Empty;
    public string FirmaAdi { get => _firmaAdi; set => Set(ref _firmaAdi, value); }

    private bool _isYukleniyor;
    public bool IsYukleniyor { get => _isYukleniyor; set => Set(ref _isYukleniyor, value); }

    private string _hataMetni = string.Empty;
    public string HataMetni { get => _hataMetni; set { if (Set(ref _hataMetni, value)) NotifyPropertyChanged(nameof(HataVar)); } }
    public bool HataVar => !string.IsNullOrWhiteSpace(HataMetni);

    private string _basariMetni = string.Empty;
    public string BasariMetni { get => _basariMetni; set { if (Set(ref _basariMetni, value)) NotifyPropertyChanged(nameof(BasariVar)); } }
    public bool BasariVar => !string.IsNullOrWhiteSpace(BasariMetni);

    public bool ListeBos => !IsYukleniyor && Kullanicilar.Count == 0;

    public ICommand YenileCommand => new RelayCommand(async () => await LoadAsync());
    public ICommand YeniCommand => new RelayCommand(YeniKullanici);
    public ICommand DuzenleCommand => new RelayCommand<KullaniciModel>(k => SeciliyiDuzenle(k));
    public ICommand AktifDegistirCommand => new RelayCommand<KullaniciModel>(async k => await AktifDegistirAsync(k));
    public ICommand SifreBelirleCommand => new RelayCommand<KullaniciModel>(async k => await SifreBelirleAsync(k));
    public ICommand SilCommand => new RelayCommand<KullaniciModel>(async k => await SilAsync(k));
    public ICommand KaydetCommand => new RelayCommand(async () => await KaydetAsync());
    public ICommand VazgecCommand => new RelayCommand(() => Duzenle.Kapat());

    public async Task LoadAsync()
    {
        BasariMetni = string.Empty;
        HataMetni = string.Empty;
        if (!AyarYetkiDenetimi.KullaniciYoneticiMi(_auth))
        {
            HataMetni = "Bu ekranı yalnızca yönetici kullanabilir.";
            return;
        }

        IsYukleniyor = true;
        try
        {
            var firma = _selection.SelectedFirma;
            _firmaId = firma?.Id ?? 0;
            FirmaAdi = string.IsNullOrWhiteSpace(firma?.KisaUnvani) ? "Firma seçili değil" : firma!.KisaUnvani;

            var roller = await _kullaniciService.GetRollerAsync();
            if (roller.Success) Duzenle.RolleriYukle(roller.Data);

            var liste = await _kullaniciService.GetKullanicilarWithRolAsync(_firmaId);
            Kullanicilar.Clear();
            if (liste.Success && liste.Data != null)
                foreach (var kullanici in liste.Data) Kullanicilar.Add(kullanici);
            else
                HataMetni = liste.Message;
        }
        catch (Exception ex) { HataMetni = ex.Message; }
        finally
        {
            IsYukleniyor = false;
            NotifyPropertyChanged(nameof(ListeBos));
        }
    }

    private void YeniKullanici()
    {
        HataMetni = string.Empty;
        BasariMetni = string.Empty;
        Duzenle.YeniIcinHazirla();
    }

    private void SeciliyiDuzenle()
    {
        if (SeciliKullanici == null) return;
        HataMetni = string.Empty;
        BasariMetni = string.Empty;
        Duzenle.DuzenleIcinHazirla(SeciliKullanici);
    }

    private void SeciliyiDuzenle(KullaniciModel kullanici)
    {
        if (kullanici == null) return;
        SeciliKullanici = kullanici;
        SeciliyiDuzenle();
    }

    private async Task AktifDegistirAsync(KullaniciModel kullanici)
    {
        if (kullanici == null) return;
        HataMetni = string.Empty;
        BasariMetni = string.Empty;
        var hedefAktif = !kullanici.AktifMi;
        var sonuc = await _kullaniciService.SetAktifAsync(kullanici.Id, hedefAktif);
        if (!sonuc.Success) { HataMetni = sonuc.Message; return; }
        await LoadAsync();
        BasariMetni = hedefAktif ? "Kullanıcı aktifleştirildi." : "Kullanıcı pasife alındı.";
    }

    private async Task SifreBelirleAsync(KullaniciModel kullanici)
    {
        if (kullanici == null) return;
        HataMetni = string.Empty;
        BasariMetni = string.Empty;
        var yeniSifre = await DialogService.ShowInputAsync("Yeni Şifre", $"{kullanici.AdiSoyadi} için yeni şifre", string.Empty);
        if (string.IsNullOrWhiteSpace(yeniSifre)) return;
        var sonuc = await _kullaniciService.SifreBelirleAsync(kullanici.Id, yeniSifre);
        if (!sonuc.Success) { HataMetni = sonuc.Message; return; }
        BasariMetni = "Şifre güncellendi.";
    }

    private async Task SilAsync(KullaniciModel kullanici)
    {
        if (kullanici == null) return;
        HataMetni = string.Empty;
        BasariMetni = string.Empty;
        var onay = await DialogService.ShowConfirmationAsync(
            "Kullanıcı silinsin mi?", $"{kullanici.AdiSoyadi} kalıcı olarak silinecek.", "Sil", "Vazgeç");
        if (!onay) return;
        var sonuc = await _kullaniciService.DeleteKullaniciAsync(kullanici.Id);
        if (!sonuc.Success) { HataMetni = sonuc.Message; return; }
        await LoadAsync();
        BasariMetni = "Kullanıcı silindi.";
    }

    private async Task KaydetAsync()
    {
        BasariMetni = string.Empty;
        var yeni = Duzenle.YeniMi;
        var basarili = await Duzenle.KaydetAsync(_firmaId);
        if (!basarili)
        {
            HataMetni = Duzenle.HataMetni;
            return;
        }
        Duzenle.Kapat();
        await LoadAsync();
        BasariMetni = yeni ? "Kullanıcı oluşturuldu." : "Kullanıcı güncellendi.";
    }
}
