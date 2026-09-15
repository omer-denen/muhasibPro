using MuhasibPro.Business.Contracts.SistemServices.DevServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.DevModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Enum;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>
/// Denetim Masası "Geliştirici Araçları" bölümünün VM'i (yalnız DEBUG; Faz 6.82).
/// Tek cümle: kimlik/transfer/şema damgası/log aksiyonlarını onaylı yürütür, sonucu ve durumu panele verir.
/// </summary>
public class GelistiriciAraclariViewModel : ViewModelBase
{
    private readonly IDevModeProvider _devMode;
    private readonly IDevAraclariService _araclar;
    private readonly IYolAciciService _yolAcici;

    private DevAracDurumuModel _durum = new();
    private bool _islemSuruyor;

    public GelistiriciAraclariViewModel(
        ICommonServices commonServices,
        IDevModeProvider devMode,
        IDevAraclariService araclar,
        IYolAciciService yolAcici) : base(commonServices)
    {
        _devMode = devMode;
        _araclar = araclar;
        _yolAcici = yolAcici;

        YenileCommand = new AsyncRelayCommand(YenileAsync, () => !_islemSuruyor);
        KimligiOnarCommand = new AsyncRelayCommand(KimligiOnarAsync, () => !_islemSuruyor);
        KimligiSifirlaCommand = new AsyncRelayCommand(KimligiSifirlaAsync, () => !_islemSuruyor);
        TransferTaraCommand = new AsyncRelayCommand(TransferTaraAsync, () => !_islemSuruyor);
        LogKlasoruAcCommand = new RelayCommand(() => YolAc(_durum.LogKlasoru, "Log klasörü"));
        VeriKlasoruAcCommand = new RelayCommand(() => YolAc(_durum.VeriKlasoru, "Veri klasörü"));
        YardimCommand = new AsyncRelayCommand(YardimGosterAsync);
    }

    /// <summary>Log/arayüz damgası (ör. "DEV"; pencere başlığında gösterilir).</summary>
    public string Etiket => _devMode?.Etiket ?? "DEV";

    public ICommand YenileCommand { get; }
    public ICommand KimligiOnarCommand { get; }
    public ICommand KimligiSifirlaCommand { get; }
    public ICommand TransferTaraCommand { get; }
    public ICommand LogKlasoruAcCommand { get; }
    public ICommand VeriKlasoruAcCommand { get; }
    public ICommand YardimCommand { get; }

    /// <summary>Durum yükleniyor bayrağı (Kural 11 — ring; sonuç zorunlu).</summary>
    public bool IsYukleniyor
    {
        get => _islemSuruyor;
        private set
        {
            if (Set(ref _islemSuruyor, value))
                NotifyPropertyChanged(nameof(IslemCalisiyor));
        }
    }

    /// <summary>İşlem sürerken butonlar pasif (Kural 11/12).</summary>
    public bool IslemCalisiyor => _islemSuruyor;

    public ObservableCollection<DevTenantDamgaModel> TenantDamgalari { get; } = new();

    public string KurulumIdKisa => _durum.KurulumIdKisa;
    public string MakineIdKisa => _durum.MakineIdKisa;
    public string MachineGuid => string.IsNullOrWhiteSpace(_durum.MachineGuid) ? "-" : _durum.MachineGuid;
    public string UygulamaSemVer => string.IsNullOrWhiteSpace(_durum.UygulamaSemVer) ? "-" : _durum.UygulamaSemVer;
    public string LogKlasoru => string.IsNullOrWhiteSpace(_durum.LogKlasoru) ? "-" : _durum.LogKlasoru;
    public string VeriKlasoru => string.IsNullOrWhiteSpace(_durum.VeriKlasoru) ? "-" : _durum.VeriKlasoru;
    public string SistemDbYolu => string.IsNullOrWhiteSpace(_durum.SistemDbYolu) ? "-" : _durum.SistemDbYolu;
    public string DamgaOzeti => _durum.DamgaOzeti;

    /// <summary>Ayrıntılı (Debug) dosya-log seviyesi — değişiklikte anında uygulanır.</summary>
    public bool AyrintiliLog
    {
        get => _durum.AyrintiliLog;
        set
        {
            if (_durum.AyrintiliLog == value)
                return;
            _durum.AyrintiliLog = value;
            NotifyPropertyChanged(nameof(AyrintiliLog));
            _ = AyrintiliLogDegistiAsync(value);
        }
    }

    private string _sonucMesaji = string.Empty;
    /// <summary>Son işlem sonucu (satır-içi; Kural 11/12).</summary>
    public string SonucMesaji
    {
        get => _sonucMesaji;
        private set
        {
            if (Set(ref _sonucMesaji, value))
                NotifyPropertyChanged(nameof(SonucVar));
        }
    }

    private bool _sonucBasarili = true;
    public bool SonucBasarili
    {
        get => _sonucBasarili;
        private set => Set(ref _sonucBasarili, value);
    }

    public bool SonucVar => !string.IsNullOrWhiteSpace(_sonucMesaji);

    public async Task LoadAsync()
    {
        IsYukleniyor = true;
        try
        {
            _durum = await _araclar.DurumOkuAsync() ?? new DevAracDurumuModel();
            TenantDamgalari.Clear();
            foreach (var damga in _durum.TenantDamgalari)
                TenantDamgalari.Add(damga);
        }
        finally
        {
            NotifyPropertyChanged(nameof(KurulumIdKisa));
            NotifyPropertyChanged(nameof(MakineIdKisa));
            NotifyPropertyChanged(nameof(MachineGuid));
            NotifyPropertyChanged(nameof(UygulamaSemVer));
            NotifyPropertyChanged(nameof(LogKlasoru));
            NotifyPropertyChanged(nameof(VeriKlasoru));
            NotifyPropertyChanged(nameof(SistemDbYolu));
            NotifyPropertyChanged(nameof(DamgaOzeti));
            NotifyPropertyChanged(nameof(AyrintiliLog));
            IsYukleniyor = false;
        }
    }

    private async Task YenileAsync()
    {
        SonucMesaji = string.Empty;
        await LoadAsync();
        Sonuc(StatusMessageType.Info, "Durum güncellendi.");
    }

    private async Task KimligiOnarAsync()
    {
        bool onay = await DialogService.ShowConfirmationAsync(
            "Kurulum Kimliğini Onar",
            "Makinesi bu kurulumla aynı olan dönem veritabanlarının kimlik damgası güncel kurulum kimliğine eşitlenir. " +
            "Farklı makineye ait dönemlere dokunulmaz. Devam edilsin mi?",
            "Onar", "Vazgeç");
        if (!onay)
            return;

        await IslemCalistirAsync("Kurulum kimliği onarılıyor…", () => _araclar.KimligiOnarAsync(), yenile: true);
    }

    private async Task KimligiSifirlaAsync()
    {
        bool onay = await DialogService.ShowConfirmationAsync(
            "Kurulum Kimliğini Sıfırla",
            "Yeni bir kurulum kimliği üretilir ve bu makinedeki tüm dönem damgaları yeni kimliğe göre yenilenir. " +
            "Bu işlem yalnızca geliştirme amaçlıdır; eski kimlikle uyumlu yedekler 'farklı kurulum' görünebilir. Devam edilsin mi?",
            "Sıfırla", "Vazgeç");
        if (!onay)
            return;

        await IslemCalistirAsync("Kurulum kimliği sıfırlanıyor…", () => _araclar.KimligiSifirlaAsync(), yenile: true);
    }

    private async Task TransferTaraAsync()
    {
        await IslemCalistirAsync("Transfer taraması çalışıyor…", () => _araclar.TransferTaramasiAsync(), yenile: false);
    }

    private async Task AyrintiliLogDegistiAsync(bool acik)
    {
        var sonuc = await _araclar.AyrintiliLogAyarlaAsync(acik);
        Sonuc(sonuc.Basarili ? StatusMessageType.Success : StatusMessageType.Error, sonuc.Mesaj);
    }

    private async Task IslemCalistirAsync(string baslangic, Func<Task<DevAracSonucuModel>> islem, bool yenile)
    {
        IsYukleniyor = true;
        SonucMesaji = baslangic;
        SonucBasarili = true;
        try
        {
            var sonuc = await islem();
            if (yenile)
                await LoadAsync();
            Sonuc(sonuc.Basarili ? StatusMessageType.Success : StatusMessageType.Error, sonuc.Mesaj);
        }
        catch (Exception ex)
        {
            Sonuc(StatusMessageType.Error, ex.Message);
        }
        finally
        {
            IsYukleniyor = false;
        }
    }

    private void YolAc(string yol, string etiket)
    {
        if (_yolAcici != null && _yolAcici.KlasoruAc(yol))
        {
            Sonuc(StatusMessageType.Success, $"{etiket} açıldı.");
            return;
        }
        Sonuc(StatusMessageType.Error, $"{etiket} açılamadı: {yol}");
    }

    private void Sonuc(StatusMessageType tip, string mesaj)
    {
        SonucMesaji = mesaj ?? string.Empty;
        SonucBasarili = tip != StatusMessageType.Error;
        if (!string.IsNullOrWhiteSpace(mesaj))
            StatusActionMessage(mesaj, tip);
    }

    private async Task YardimGosterAsync()
    {
        await DialogService.ShowYardimAsync("Geliştirici Araçları — Yardım", new List<YardimMaddesiDto>
        {
            new() { Baslik = "Bu bölüm nedir?", Aciklama = "Yalnızca geliştirme (DEBUG) derlemesinde görünen iç araçlardır; normal kullanıcı akışının parçası değildir. Her aksiyon onay ister ve Sistem günlüğüne DEV kaynağıyla yazılır." },
            new() { Baslik = "Kimlik durumu", Aciklama = "Kurulum kimliği bu uygulamanın kurulumunu, makine kimliği ise cihazı tanımlar. Listede her dönem veritabanının damgası (şema sürümü + kimlik) ve güncel kimlikle eşleşip eşleşmediği görünür." },
            new() { Baslik = "\"Kimliği Onar\"", Aciklama = "Makinesi aynı olan dönemlerin damgasını güncel kurulum kimliğine eşitler. Farklı makineye ait dönemlere dokunmaz." },
            new() { Baslik = "\"Kimliği Sıfırla\"", Aciklama = "Yeni kurulum kimliği üretir ve bu makinedeki tüm dönemleri yeni kimlikle damgalar. Yıkıcıdır; yalnızca kontrollü test/kimlik yenileme senaryosunda kullanılır." },
            new() { Baslik = "\"Transfer Taraması\"", Aciklama = "Açılışta çalışan taşınmış-veri taramasını elle tetikler; sonuçta farklı kuruluma ait dönem sayısını veya sessiz onarılan kimlik sayısını gösterir." },
            new() { Baslik = "Ayrıntılı log", Aciklama = "Dosya günlüğünün seviyesini Debug'a indirir; teşhis için daha ayrıntılı kayıt tutulur. Kapatınca Information seviyesine döner." },
            new() { Baslik = "Klasör açma", Aciklama = "\"Log klasörünü aç\" ve \"Veri klasörünü aç\" düğmeleri ilgili yolları varsayılan dosya gezgininde açar. Yollar salt-okunur gösterilir." },
        });
    }
}
