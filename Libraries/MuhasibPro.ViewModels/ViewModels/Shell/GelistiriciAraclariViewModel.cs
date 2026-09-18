using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.DevServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.DevModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Sistem;
using MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;
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
    private readonly IUpdateService _updateService;
    private readonly SistemDiagnosticsViewModel _diagnostics;
    private readonly IModulTestCalistirici _modulTestleri;
    private readonly ISurumOzellikService _surumService;
    private readonly IAsistanSohbetService _asistanSohbet;
    private readonly IYardimBilgiTabani? _yardimBilgiTabani;

    private DevAracDurumuModel _durum = new();
    private UpdateSettingsModel _guncellemeAyarlari;
    private bool _islemSuruyor;

    public GelistiriciAraclariViewModel(
        ICommonServices commonServices,
        IDevModeProvider devMode,
        IDevAraclariService araclar,
        IYolAciciService yolAcici,
        IUpdateService updateService = null,
        IApplicationPaths appPaths = null,
        ISistemDatabaseService sistemDb = null,
        ISistemDiagnosticsService diagnosticsService = null,
        IModulTestCalistirici modulTestleri = null,
        ISurumOzellikService surumService = null,
        IAsistanSohbetService asistanSohbet = null,
        IYardimBilgiTabani yardimBilgiTabani = null) : base(commonServices)
    {
        _devMode = devMode;
        _araclar = araclar;
        _yolAcici = yolAcici;
        _updateService = updateService;
        _modulTestleri = modulTestleri;
        _surumService = surumService;
        _asistanSohbet = asistanSohbet;
        _yardimBilgiTabani = yardimBilgiTabani;

        // Sistem tanılama (7 test) dev-mode'a yeniden kullanım için bağlanır (Kural 4: kopya yok).
        if (appPaths != null && sistemDb != null && diagnosticsService != null)
            _diagnostics = new SistemDiagnosticsViewModel(appPaths, sistemDb, diagnosticsService, commonServices);

        YenileCommand = new AsyncRelayCommand(YenileAsync, () => !_islemSuruyor);
        KimligiOnarCommand = new AsyncRelayCommand(KimligiOnarAsync, () => !_islemSuruyor);
        KimligiSifirlaCommand = new AsyncRelayCommand(KimligiSifirlaAsync, () => !_islemSuruyor);
        TransferTaraCommand = new AsyncRelayCommand(TransferTaraAsync, () => !_islemSuruyor);
        LogKlasoruAcCommand = new RelayCommand(() => YolAc(_durum.LogKlasoru, "Log klasörü"));
        VeriKlasoruAcCommand = new RelayCommand(() => YolAc(_durum.VeriKlasoru, "Veri klasörü"));
        VarsayilanaSifirlaCommand = new RelayCommand(VarsayilanaSifirla);
        KaynagiDogrulaCommand = new AsyncRelayCommand(KaynagiDogrulaAsync);
        AiOzTestCommand = new AsyncRelayCommand(AiOzTestAsync);
        TanilamaCalistirCommand = new AsyncRelayCommand(TanilamaCalistirAsync, () => !_islemSuruyor);
        ModulTestCalistirCommand = new AsyncRelayCommand(ModulTestCalistirAsync, () => !_islemSuruyor);
    }

    /// <summary>Log/arayüz damgası (ör. "DEV"; pencere başlığında gösterilir).</summary>
    public string Etiket => _devMode?.Etiket ?? "DEV";

    public ICommand YenileCommand { get; }
    public ICommand KimligiOnarCommand { get; }
    public ICommand KimligiSifirlaCommand { get; }
    public ICommand TransferTaraCommand { get; }
    public ICommand LogKlasoruAcCommand { get; }
    public ICommand VeriKlasoruAcCommand { get; }
    public ICommand VarsayilanaSifirlaCommand { get; }
    public ICommand KaynagiDogrulaCommand { get; }
    public ICommand AiOzTestCommand { get; }
    public ICommand TanilamaCalistirCommand { get; }
    /// <summary>Çalışma-zamanı tanılama sonuçları (sistem sınıfları + güncelleme kaynağı + kimlik).</summary>
    public ObservableCollection<SistemTestResult> TanilamaSonuclari { get; } = new();

    private string _tanilamaOzeti = string.Empty;
    public string TanilamaOzeti
    {
        get => _tanilamaOzeti;
        private set
        {
            if (Set(ref _tanilamaOzeti, value))
                NotifyPropertyChanged(nameof(TanilamaVar));
        }
    }

    public bool TanilamaVar => !string.IsNullOrWhiteSpace(_tanilamaOzeti);

    public ICommand ModulTestCalistirCommand { get; }

    /// <summary>Modül entegrasyon testleri (donanım POST): DI kaydı/çözümü + kritik akış smoke'ları.</summary>
    public ObservableCollection<ModulTestSonucu> ModulTestSonuclari { get; } = new();

    private string _modulTestOzeti = string.Empty;
    public string ModulTestOzeti
    {
        get => _modulTestOzeti;
        private set
        {
            if (Set(ref _modulTestOzeti, value))
                NotifyPropertyChanged(nameof(ModulTestVar));
        }
    }

    public bool ModulTestVar => !string.IsNullOrWhiteSpace(_modulTestOzeti);

    /// <summary>Güncelleme kaynağı adresi (repo/feed) — dev-mode buradan değiştirir; değişiklik anında kaydedilir.</summary>
    public string FeedUrl
    {
        get => _guncellemeAyarlari?.FeedUrl ?? string.Empty;
        set
        {
            if (_guncellemeAyarlari != null && _guncellemeAyarlari.FeedUrl != value?.Trim())
            {
                _guncellemeAyarlari.FeedUrl = value?.Trim() ?? string.Empty;
                NotifyPropertyChanged(nameof(FeedUrl));
                _ = GuncellemeKaynakKaydetAsync();
            }
        }
    }

    /// <summary>Derlemede gömülü varsayılan kaynak (mevcut git repo adresi); boş olabilir.</summary>
    public string VarsayilanFeedUrl => UpdateSettingsModel.VarsayilanFeedUrl;

    private async Task GuncellemeKaynakKaydetAsync()
    {
        if (_updateService == null || _guncellemeAyarlari == null)
            return;
        await _updateService.SaveSettingsAsync(_guncellemeAyarlari);
        Sonuc(StatusMessageType.Success, "Güncelleme kaynağı kaydedildi.");
    }

    private void VarsayilanaSifirla()
    {
        if (string.IsNullOrWhiteSpace(VarsayilanFeedUrl))
        {
            Sonuc(StatusMessageType.Warning, "Varsayılan kaynak adresi bulunamadı — bu derlemede gömülü git adresi yok.");
            return;
        }
        FeedUrl = VarsayilanFeedUrl;
        Sonuc(StatusMessageType.Success, "Güncelleme kaynağı varsayılana döndürüldü.");
    }

    private string _dogrulamaDetayi = string.Empty;
    /// <summary>"Kaynağı Doğrula" sonucu — normalize öz-testi (+ varsa bağlantı denemesi) satırları.</summary>
    public string DogrulamaDetayi
    {
        get => _dogrulamaDetayi;
        private set
        {
            if (Set(ref _dogrulamaDetayi, value))
                NotifyPropertyChanged(nameof(DogrulamaVar));
        }
    }

    public bool DogrulamaVar => !string.IsNullOrWhiteSpace(_dogrulamaDetayi);

    private string _aiOzTestDetayi = string.Empty;
    /// <summary>"AI Öz-testi" sonucu — sürüm hakkı + model durumu + yardım dizini satırları (Faz 6.93).</summary>
    public string AiOzTestDetayi
    {
        get => _aiOzTestDetayi;
        private set
        {
            if (Set(ref _aiOzTestDetayi, value))
                NotifyPropertyChanged(nameof(AiOzTestVar));
        }
    }

    public bool AiOzTestVar => !string.IsNullOrWhiteSpace(_aiOzTestDetayi);

    /// <summary>
    /// Dev-mode öz-testi: derlemede gömülü kaynağı + normalize sözleşmesini (birim testleriyle aynı örneklerle)
    /// doğrular; ardından kaynağa bağlanmayı dener. Testlerin dev-mode'a entegrasyonu.
    /// </summary>
    private async Task KaynagiDogrulaAsync()
    {
        var ozTest = AppGuncellemeBilgisi.OzTest();
        var satirlar = new List<string>
        {
            $"Gömülü varsayılan: {Metin(VarsayilanFeedUrl)}",
            $"Girilen adres (normalize): {Metin(AppGuncellemeBilgisi.Normalize(FeedUrl))}",
            $"Normalizasyon öz-testi: {ozTest.Gecen}/{ozTest.Toplam} geçti",
        };
        satirlar.AddRange(ozTest.Detaylar);

        if (_updateService != null && !string.IsNullOrWhiteSpace(FeedUrl))
        {
            try
            {
                var bilgi = await _updateService.CheckForUpdatesAsync();
                satirlar.Add(bilgi == null
                    ? "Bağlantı: kaynağa ulaşıldı — güncel."
                    : $"Bağlantı: güncelleme var — v{bilgi.TargetFullRelease.Version}.");
            }
            catch (Exception ex)
            {
                satirlar.Add("Bağlantı: ulaşılamadı — " + ex.Message);
            }
        }

        DogrulamaDetayi = string.Join(Environment.NewLine, satirlar);
        Sonuc(ozTest.Basarili ? StatusMessageType.Success : StatusMessageType.Error,
            ozTest.Basarili
                ? $"Güncelleme kaynağı doğrulandı (öz-test {ozTest.Gecen}/{ozTest.Toplam})."
                : "Güncelleme kaynağı öz-testi başarısız.");
    }

    /// <summary>
    /// Dev-mode AI öz-testi (Faz 6.92/6.93): sürüm hakkı + model durumu + yardım dizini (madde sayısı + semantik indeks).
    /// Salt-okunur: model indirmez/yüklemez; eksik servis satırda raporlanır, fırlatılmaz.
    /// </summary>
    public async Task AiOzTestAsync()
    {
        var satirlar = new List<string>();
        bool hak = false, hazir = false;

        if (_surumService == null)
            satirlar.Add("Sürüm hakkı: servis çözülemedi.");
        else
        {
            try
            {
                var h = await _surumService.AiAsistanHakkiAsync();
                hak = h.HakVarMi;
                satirlar.Add(hak
                    ? $"Sürüm hakkı: {h.Tur} — AI açık."
                    : $"Sürüm hakkı: {h.Tur} — kapalı ({Metin(h.Gerekce)}).");
            }
            catch (Exception ex)
            {
                satirlar.Add("Sürüm hakkı: okunamadı — " + ex.Message);
            }
        }

        if (_asistanSohbet == null)
            satirlar.Add("Model durumu: servis çözülemedi.");
        else
        {
            try
            {
                var durum = await _asistanSohbet.DurumuGetirAsync();
                hazir = durum.HazirMi;
                satirlar.Add($"Model durumu: {(hazir ? "Hazır" : "Hazır değil")} ({Metin(durum.Mesaj)}).");
            }
            catch (Exception ex)
            {
                satirlar.Add("Model durumu: okunamadı — " + ex.Message);
            }
        }

        if (_yardimBilgiTabani == null)
            satirlar.Add("Yardım dizini: servis çözülemedi.");
        else
        {
            try
            {
                var dizin = await _yardimBilgiTabani.DurumGetirAsync();
                satirlar.Add($"Yardım dizini: {dizin.MaddeSayisi} madde • semantik indeks: {(dizin.VektorVarMi ? "var" : "yok")}.");
            }
            catch (Exception ex)
            {
                satirlar.Add("Yardım dizini: okunamadı — " + ex.Message);
            }
        }

        AiOzTestDetayi = string.Join(Environment.NewLine, satirlar);
        bool gecti = hak && hazir;
        Sonuc(gecti ? StatusMessageType.Success : StatusMessageType.Warning,
            gecti ? "AI öz-testi geçti." : "AI öz-testi tamamlandı — ayrıntı yukarıda.");
    }

    private static string Metin(string deger) => string.IsNullOrWhiteSpace(deger) ? "-" : deger;

    /// <summary>
    /// Tüm çalışma-zamanı tanılamayı çalıştırır: mevcut sistem sınıfı testleri (7) +
    /// güncelleme kaynağı öz-testi + kimlik/damga özeti. "Sorun nerede" tek listede görünür.
    /// Not: Bu xUnit paketi değildir (o CI'da koşar); uygulama içi çalışma-zamanı kontrolleridir.
    /// </summary>
    private async Task TanilamaCalistirAsync()
    {
        if (_diagnostics == null)
        {
            Sonuc(StatusMessageType.Warning, "Tanılama servisleri kullanılamıyor (bağımlılıklar çözülemedi).");
            return;
        }
        if (_islemSuruyor)
            return;

        IsYukleniyor = true;
        SonucMesaji = "Tanılama çalışıyor…";
        try
        {
            await _diagnostics.RunAsync(_ => { }, _ => { });

            var tumu = new List<SistemTestResult>();
            tumu.AddRange(_diagnostics.TestSonuclari);
            tumu.AddRange(UzantiKontrolleri());

            TanilamaSonuclari.Clear();
            foreach (var test in tumu)
                TanilamaSonuclari.Add(test);

            var gecen = tumu.Count(t => t.BasariliMi);
            TanilamaOzeti = $"Tanılama: {gecen}/{tumu.Count} geçti • {DateTime.Now:HH:mm:ss}";
            Sonuc(gecen == tumu.Count ? StatusMessageType.Success : StatusMessageType.Warning, TanilamaOzeti);
        }
        catch (Exception ex)
        {
            Sonuc(StatusMessageType.Error, "Tanılama hatası: " + ex.Message);
        }
        finally
        {
            IsYukleniyor = false;
        }
    }

    private IEnumerable<SistemTestResult> UzantiKontrolleri()
    {
        var ozTest = AppGuncellemeBilgisi.OzTest();
        yield return new SistemTestResult
        {
            Kategori = "Güncelleme",
            TestAdi = "Kaynak normalize öz-testi",
            BasariliMi = ozTest.Basarili,
            Mesaj = $"{ozTest.Gecen}/{ozTest.Toplam} örnek geçti",
            Detay = $"Gömülü varsayılan: {Metin(VarsayilanFeedUrl)} • Girilen (normalize): {Metin(AppGuncellemeBilgisi.Normalize(FeedUrl))}"
        };
        yield return new SistemTestResult
        {
            Kategori = "Kimlik/Veri",
            TestAdi = "Dönem damgaları",
            BasariliMi = true,
            Mesaj = string.IsNullOrWhiteSpace(DamgaOzeti) ? "Damga bilgisi yok" : DamgaOzeti,
            Detay = $"Kurulum={KurulumIdKisa}, Makine={MakineIdKisa}, Tenant kaydı={TenantDamgalari.Count}"
        };
    }

    /// <summary>
    /// Modül entegrasyon testleri (donanım POST): her modülün DI'da kayıtlı/çözülebilir ve kritik akışının
    /// çalıştığını doğrular. Sonuç "hangi modül kırık" sorusunu tek listede yanıtlar.
    /// </summary>
    private async Task ModulTestCalistirAsync()
    {
        if (_modulTestleri == null)
        {
            Sonuc(StatusMessageType.Warning, "Modül testleri servisi kullanılamıyor (bağımlılık çözülemedi).");
            return;
        }
        if (_islemSuruyor)
            return;

        IsYukleniyor = true;
        SonucMesaji = "Modül entegrasyon testleri çalışıyor…";
        try
        {
            var sonuclar = await _modulTestleri.CalistirAsync();

            ModulTestSonuclari.Clear();
            foreach (var s in sonuclar)
                ModulTestSonuclari.Add(s);

            var gecen = sonuclar.Count(s => s.Basarili);
            ModulTestOzeti = $"Modül testleri: {gecen}/{sonuclar.Count} geçti • {DateTime.Now:HH:mm:ss}";
            Sonuc(gecen == sonuclar.Count ? StatusMessageType.Success : StatusMessageType.Warning, ModulTestOzeti);
        }
        catch (Exception ex)
        {
            Sonuc(StatusMessageType.Error, "Modül testi hatası: " + ex.Message);
        }
        finally
        {
            IsYukleniyor = false;
        }
    }

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

            _guncellemeAyarlari = _updateService != null ? await _updateService.GetSettingsAsync() : null;
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
            NotifyPropertyChanged(nameof(FeedUrl));
            NotifyPropertyChanged(nameof(VarsayilanFeedUrl));
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

}
