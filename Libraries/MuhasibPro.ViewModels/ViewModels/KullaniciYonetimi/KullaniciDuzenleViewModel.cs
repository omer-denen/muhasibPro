using System.Collections.ObjectModel;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

/// <summary>Faz 6.85 K2: kullanıcı ekle/düzenle formu (composition — liste VM'i barındırır).
/// Tek cümle: form alanlarını tutar, doğrular ve kaydeder; liste/navigasyon bilmez.</summary>
public class KullaniciDuzenleViewModel : ObservableObject
{
    private readonly IKullaniciService _kullaniciService;
    private long _duzenlenenId;

    public KullaniciDuzenleViewModel(IKullaniciService kullaniciService)
    {
        _kullaniciService = kullaniciService;
    }

    public ObservableCollection<KullaniciRolModel> Roller { get; } = new();

    private bool _gorunur;
    public bool Gorunur { get => _gorunur; set => Set(ref _gorunur, value); }

    private bool _yeniMi;
    public bool YeniMi
    {
        get => _yeniMi;
        private set { if (Set(ref _yeniMi, value)) NotifyPropertyChanged(nameof(Baslik)); }
    }

    public string Baslik => YeniMi ? "Yeni Kullanıcı" : "Kullanıcıyı Düzenle";

    private string _kullaniciAdi = string.Empty;
    public string KullaniciAdi { get => _kullaniciAdi; set => Set(ref _kullaniciAdi, value); }

    private string _adi = string.Empty;
    public string Adi { get => _adi; set => Set(ref _adi, value); }

    private string _soyadi = string.Empty;
    public string Soyadi { get => _soyadi; set => Set(ref _soyadi, value); }

    private string _eposta = string.Empty;
    public string Eposta { get => _eposta; set => Set(ref _eposta, value); }

    private string _telefon = string.Empty;
    public string Telefon { get => _telefon; set => Set(ref _telefon, value); }

    private bool _aktif = true;
    public bool Aktif { get => _aktif; set => Set(ref _aktif, value); }

    private KullaniciRolModel _seciliRol;
    public KullaniciRolModel SeciliRol { get => _seciliRol; set => Set(ref _seciliRol, value); }

    private string _sifre = string.Empty;
    public string Sifre { get => _sifre; set => Set(ref _sifre, value); }

    private string _sifreTekrar = string.Empty;
    public string SifreTekrar { get => _sifreTekrar; set => Set(ref _sifreTekrar, value); }

    private string _hataMetni = string.Empty;
    public string HataMetni
    {
        get => _hataMetni;
        set { if (Set(ref _hataMetni, value)) NotifyPropertyChanged(nameof(HataVar)); }
    }
    public bool HataVar => !string.IsNullOrWhiteSpace(HataMetni);

    private bool _calisiyor;
    public bool Calisiyor { get => _calisiyor; set => Set(ref _calisiyor, value); }

    public void RolleriYukle(IEnumerable<KullaniciRolModel> roller)
    {
        Roller.Clear();
        foreach (var rol in roller) Roller.Add(rol);
    }

    public void YeniIcinHazirla()
    {
        _duzenlenenId = 0;
        KullaniciAdi = string.Empty;
        Adi = string.Empty;
        Soyadi = string.Empty;
        Eposta = string.Empty;
        Telefon = string.Empty;
        Aktif = true;
        Sifre = string.Empty;
        SifreTekrar = string.Empty;
        SeciliRol = Roller.FirstOrDefault(r => r.RolTip == KullaniciRolTip.Kullanici) ?? Roller.FirstOrDefault();
        HataMetni = string.Empty;
        YeniMi = true;
        Gorunur = true;
    }

    public void DuzenleIcinHazirla(KullaniciModel kullanici)
    {
        if (kullanici == null) return;
        _duzenlenenId = kullanici.Id;
        KullaniciAdi = kullanici.KullaniciAdi;
        Adi = kullanici.Adi;
        Soyadi = kullanici.Soyadi;
        Eposta = kullanici.Eposta;
        Telefon = kullanici.Telefon;
        Aktif = kullanici.AktifMi;
        Sifre = string.Empty;
        SifreTekrar = string.Empty;
        SeciliRol = kullanici.RolId > 0
            ? Roller.FirstOrDefault(r => r.Id == kullanici.RolId)
            : Roller.FirstOrDefault(r => r.RolTip == KullaniciRolTip.Kullanici);
        HataMetni = string.Empty;
        YeniMi = false;
        Gorunur = true;
    }

    public void Kapat()
    {
        Gorunur = false;
        HataMetni = string.Empty;
    }

    /// <summary>Formu doğrular ve kaydeder; başarıda true döner, hatada <see cref="HataMetni"/> doldurur.</summary>
    public async Task<bool> KaydetAsync(long firmaId)
    {
        HataMetni = string.Empty;
        Calisiyor = true;
        try
        {
            if (string.IsNullOrWhiteSpace(KullaniciAdi) || string.IsNullOrWhiteSpace(Adi))
            {
                HataMetni = "Kullanıcı adı ve ad zorunludur.";
                return false;
            }

            var model = new KullaniciModel
            {
                Id = _duzenlenenId,
                KullaniciAdi = KullaniciAdi,
                Adi = Adi,
                Soyadi = Soyadi,
                Eposta = Eposta,
                Telefon = Telefon,
                AktifMi = Aktif
            };

            if (YeniMi)
            {
                if (Sifre != SifreTekrar)
                {
                    HataMetni = "Şifreler eşleşmiyor.";
                    return false;
                }
                var sonuc = await _kullaniciService.CreateKullaniciAsync(model, Sifre, firmaId, SeciliRol?.Id ?? 0);
                if (!sonuc.Success) { HataMetni = sonuc.Message; return false; }
            }
            else
            {
                var guncelle = await _kullaniciService.UpdateKullaniciAsync(model);
                if (!guncelle.Success) { HataMetni = guncelle.Message; return false; }
                if (firmaId > 0 && SeciliRol != null)
                {
                    var rol = await _kullaniciService.RolAtaAsync(_duzenlenenId, firmaId, SeciliRol.Id);
                    if (!rol.Success) { HataMetni = rol.Message; return false; }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            HataMetni = ex.GetBaseException().Message;
            return false;
        }
        finally
        {
            Calisiyor = false;
        }
    }
}
