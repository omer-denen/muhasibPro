using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>MainShell alt panel sohbet VM'i (Faz 6.92 Adım 5).
/// Tek cümle: kapı denetimi + model hazırlığı + streaming soru/cevap akışını yönetir.</summary>
public class AsistanSohbetViewModel : ViewModelBase
{
    private readonly IAsistanSohbetService _sohbet;
    private readonly IAiAsistanSettingsProvider _ayarlar;
    private readonly ISurumOzellikService _surum;
    private readonly IPermissionService _yetki;
    private readonly IFirmaWithMaliDonemSelectedService _secim;
    private readonly IYardimBilgiTabani? _yardimTabani;
    private CancellationTokenSource? _gonderCts;
    private bool _panelHazirlikCalisiyor;

    public AsistanSohbetViewModel(
        ICommonServices commonServices,
        IAsistanSohbetService sohbet,
        IAiAsistanSettingsProvider ayarlar,
        ISurumOzellikService surum,
        IPermissionService yetki,
        IFirmaWithMaliDonemSelectedService secim,
        IYardimBilgiTabani? yardimTabani = null) : base(commonServices)
    {
        _sohbet = sohbet;
        _ayarlar = ayarlar;
        _surum = surum;
        _yetki = yetki;
        _secim = secim;
        _yardimTabani = yardimTabani;
    }

    public ObservableCollection<AsistanMesajSatiri> Mesajlar { get; } = new();

    private string _soruMetni = string.Empty;
    public string SoruMetni
    {
        get => _soruMetni;
        set { if (Set(ref _soruMetni, value)) GonderKomutunuTazele(); }
    }

    private bool _isGonderiliyor;
    public bool IsGonderiliyor
    {
        get => _isGonderiliyor;
        private set { if (Set(ref _isGonderiliyor, value)) GonderKomutunuTazele(); }
    }

    private bool _isHazirlaniyor;
    public bool IsHazirlaniyor
    {
        get => _isHazirlaniyor;
        private set { if (Set(ref _isHazirlaniyor, value)) GonderKomutunuTazele(); }
    }

    private double _hazirlikYuzde;
    public double HazirlikYuzde
    {
        get => _hazirlikYuzde;
        private set => Set(ref _hazirlikYuzde, value);
    }

    private string _hazirlikAsamasi = string.Empty;
    public string HazirlikAsamasi
    {
        get => _hazirlikAsamasi;
        private set => Set(ref _hazirlikAsamasi, value);
    }

    private string _hataMetni = string.Empty;
    public string HataMetni
    {
        get => _hataMetni;
        private set => Set(ref _hataMetni, value);
    }

    private bool _kilitliMi = true;
    public bool KilitliMi
    {
        get => _kilitliMi;
        private set { if (Set(ref _kilitliMi, value)) GonderKomutunuTazele(); }
    }

    private string _kilitMetni = "Denetleniyor...";
    public string KilitMetni
    {
        get => _kilitMetni;
        private set => Set(ref _kilitMetni, value);
    }

    private string _modelDurumMetni = string.Empty;
    public string ModelDurumMetni
    {
        get => _modelDurumMetni;
        private set => Set(ref _modelDurumMetni, value);
    }

    private string _dizinDurumMetni = string.Empty;
    /// <summary>Yardım bilgi tabanı dizin durumu (madde sayısı + semantik indeks) — Faz 6.93.</summary>
    public string DizinDurumMetni
    {
        get => _dizinDurumMetni;
        private set => Set(ref _dizinDurumMetni, value);
    }

    private bool _isDizinHazirlaniyor;
    /// <summary>Yardım dizini hazırlanıyor (Kural 11: ring/bar bayrağı try-finally ile kapanır).</summary>
    public bool IsDizinHazirlaniyor
    {
        get => _isDizinHazirlaniyor;
        private set { if (Set(ref _isDizinHazirlaniyor, value)) GonderKomutunuTazele(); }
    }

    private string _dizinAsamasi = string.Empty;
    public string DizinAsamasi
    {
        get => _dizinAsamasi;
        private set => Set(ref _dizinAsamasi, value);
    }

    private double _dizinYuzde;
    public double DizinYuzde
    {
        get => _dizinYuzde;
        private set => Set(ref _dizinYuzde, value);
    }

    private AsyncRelayCommand? _gonder;
    public ICommand GonderCommand => _gonder ??= new AsyncRelayCommand(GonderAsync, GonderilebilirMi);
    private RelayCommand? _durdur;
    public ICommand DurdurCommand => _durdur ??= new RelayCommand(Durdur);
    private RelayCommand? _temizle;
    public ICommand TemizleCommand => _temizle ??= new RelayCommand(Temizle);

    private void GonderKomutunuTazele()
    {
        _gonder?.RaiseCanExecuteChanged();
    }

    private bool GonderilebilirMi() =>
        !KilitliMi && !IsGonderiliyor && !IsHazirlaniyor && !IsDizinHazirlaniyor && !string.IsNullOrWhiteSpace(SoruMetni);

    public async Task LoadAsync()
    {
        if (Mesajlar.Count == 0)
        {
            Mesajlar.Add(new AsistanMesajSatiri(false,
                "Merhaba! Ben MuhasibPro yardım asistanıyım. Uygulamanın yardım maddelerine göre cevap veririm — örneğin 'mali dönem nasıl arşivlenir?' diye sorabilirsin."));
        }
        await KapiyiDenetleAsync();
        if (!KilitliMi && _yardimTabani != null)
        {
            // Panel açılışı: dizini model indirmeden (lexical) hazırla; semantik indeks ilk soruda eklenir.
            await DiziniHazirlaAsync(modelIndirmeyeIzin: false);
        }
    }

    public async Task KapiyiDenetleAsync()
    {
        try
        {
            var hak = await _surum.AiAsistanHakkiAsync();
            if (!hak.HakVarMi)
            {
                Kilitle(hak.Gerekce);
                return;
            }
            if (!await _yetki.HasPermissionAsync(Permission.AiAsistan_Kullan))
            {
                Kilitle("Bu özellik için yetkiniz yok. Yöneticinizden AI asistanı yetkisi isteyin.");
                return;
            }
            var ayar = await _ayarlar.GetAsync();
            if (!ayar.EtkinMi)
            {
                Kilitle("Asistan kapalı. Ayarlar → Yapay Zeka bölümünden açabilirsiniz.");
                return;
            }
            KilitliMi = false;
            KilitMetni = string.Empty;
            var durum = await _sohbet.DurumuGetirAsync();
            if (durum.HazirMi && !ModelAliasUyusuyorMu(durum, ayar))
                ModelDurumMetni = "Model ayarı değişti — ilk soruda yeniden hazırlanır";
            else
                // "Hazır" ⇔ cevaplayabilir. Yüklü değilken nötr durum yazılır; gerçek hâl
                // panel açılışında (PanelAcildiAsync) önbellek denetimiyle netleşir.
                ModelDurumMetni = durum.HazirMi ? $"Model hazır ({durum.Mesaj})" : "Model ilk soruda hazırlanır";
        }
        catch (Exception ex)
        {
            Kilitle("Asistan denetimi başarısız.");
            HataMetni = ex.Message;
        }
    }

    private void Kilitle(string gerekce)
    {
        KilitliMi = true;
        KilitMetni = gerekce;
        ModelDurumMetni = string.Empty;
    }

    /// <summary>Asistan paneli (flyout) açıldığında çağrılır. "Hazır" her zaman "cevaplayabilir" demektir:
    /// model zaten yüklüyse durum tazelenir; indirilmişse (cached) ön-yüklenir → "Model hazır";
    /// indirilmemişse nötr "ilk soruda indirilecek" yazılır (asla "hazır değil" çelişkisi doğmaz). Fırlatmaz.</summary>
    public async Task PanelAcildiAsync()
    {
        if (_panelHazirlikCalisiyor)
            return;
        _panelHazirlikCalisiyor = true;
        try
        {
            await KapiyiDenetleAsync();
            if (KilitliMi)
                return;

            var durum = await _sohbet.DurumuGetirAsync();
            if (durum.HazirMi)
            {
                ModelDurumMetni = $"Model hazır ({durum.Mesaj})";
                return;
            }

            var ayar = await _ayarlar.GetAsync();
            if (!await ModelIndirilmisMiAsync(ayar.GetModelAlias()))
            {
                ModelDurumMetni = "Model ilk soruda indirilecek";
                return;
            }

            await HazirlaIcAsync();
        }
        catch (Exception ex)
        {
            ModelDurumMetni = "Model hazırlanamadı: " + ex.Message;
        }
        finally
        {
            _panelHazirlikCalisiyor = false;
        }
    }

    /// <summary>Aktif model diskte indirilmiş mi (indirme yapmaz; katalog denetimi). Hata → false.</summary>
    private async Task<bool> ModelIndirilmisMiAsync(string alias)
    {
        try
        {
            var modeller = await _sohbet.ModelleriGetirAsync();
            return modeller.Any(m => m.IndirildiMi && string.Equals(m.Alias, alias, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    public async Task GonderAsync()
    {
        if (!GonderilebilirMi())
            return;
        HataMetni = string.Empty;
        await KapiyiDenetleAsync();
        if (KilitliMi || string.IsNullOrWhiteSpace(SoruMetni))
            return;

        if (_yardimTabani != null)
        {
            var dizin = await _yardimTabani.DurumGetirAsync();
            if (!dizin.VektorVarMi)
                await DiziniHazirlaAsync(modelIndirmeyeIzin: true);
        }

        var soru = SoruMetni.Trim();
        SoruMetni = string.Empty;
        var gecmis = Mesajlar
            .Where(m => !string.IsNullOrWhiteSpace(m.Icerik))
            .Select(m => new AsistanMesajDto
            {
                Rol = m.KullaniciMi ? "kullanici" : "asistan",
                Icerik = m.Icerik
            }).ToList();
        await ContextService.RunAsync(() => Mesajlar.Add(new AsistanMesajSatiri(true, soru)));

        IsGonderiliyor = true;
        _gonderCts = new CancellationTokenSource();
        try
        {
            var durum = await _sohbet.DurumuGetirAsync();
            var ayar = await _ayarlar.GetAsync();
            if (!durum.HazirMi || !ModelAliasUyusuyorMu(durum, ayar))
                await HazirlaIcAsync();
            // Model hazırlanamadıysa ham "Asistan hazır değil" yerine gerçek sebep hata bandında kalsın (Kural 11/12).
            if (!string.IsNullOrEmpty(HataMetni))
                return;
            var istek = new AsistanSoruDto
            {
                Soru = soru,
                SayfaAnahtari = "MainShell",
                FirmaAdi = FirmaAdi(),
                DonemAdi = DonemAdi(),
                Gecmis = gecmis
            };
            var yanit = new AsistanMesajSatiri(false, string.Empty);
            await ContextService.RunAsync(() => Mesajlar.Add(yanit));
            await foreach (var parca in _sohbet.SorStreamingAsync(istek, _gonderCts.Token))
            {
                var p = parca;
                await ContextService.RunAsync(() => yanit.ParcaEkle(p));
            }
            if (string.IsNullOrEmpty(yanit.Icerik))
                await ContextService.RunAsync(() => yanit.Icerik = "(boş yanıt)");
        }
        catch (OperationCanceledException)
        {
            HataMetni = "Yanıt durduruldu.";
        }
        catch (Exception ex)
        {
            HataMetni = ex.Message;
            StatusError(ex.Message);
            await LogSistemExceptionAsync("AsistanSohbet", "Soru", ex);
        }
        finally
        {
            IsGonderiliyor = false;
            _gonderCts?.Dispose();
            _gonderCts = null;
        }
    }

    public void Durdur() => _gonderCts?.Cancel();

    private async Task HazirlaIcAsync()
    {
        HataMetni = string.Empty;
        IsHazirlaniyor = true;
        try
        {
            var ilerleme = new Progress<AsistanDurumDto>(d =>
            {
                HazirlikYuzde = d.IlerlemeYuzde ?? 0;
                HazirlikAsamasi = d.Asama;
            });
            await _sohbet.HazirlaAsync(ilerleme, CancellationToken.None);
            var durum = await _sohbet.DurumuGetirAsync();
            ModelDurumMetni = $"Model hazır ({durum.Mesaj})";
        }
        catch (Exception ex)
        {
            HataMetni = ex.Message;
            ModelDurumMetni = "Model hazırlanamadı: " + ex.Message;
            StatusError(ex.Message);
        }
        finally
        {
            IsHazirlaniyor = false;
        }
    }

    /// <summary>Yardım bilgi tabanı dizinini hazırlar (Kural 11/12: bayrak + determinate bar + sonuç).
    /// Model indirmeye izin yoksa yalnız lexical dizin kurulur (fırlatmaz — motor hatayı yutar).</summary>
    private async Task DiziniHazirlaAsync(bool modelIndirmeyeIzin)
    {
        if (_yardimTabani == null)
            return;
        IsDizinHazirlaniyor = true;
        DizinAsamasi = "Hazırlanıyor";
        DizinYuzde = 0;
        try
        {
            var ilerleme = new Progress<YardimIndexDurumu>(d =>
            {
                if (d.IlerlemeYuzde.HasValue)
                    DizinYuzde = d.IlerlemeYuzde.Value;
                if (!string.IsNullOrWhiteSpace(d.Asama))
                    DizinAsamasi = d.Asama;
            });
            await _yardimTabani.HazirlaAsync(modelIndirmeyeIzin, ilerleme);
            await DizinDurumunuYukleAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            HataMetni = ex.Message;
            StatusError(ex.Message);
            await LogSistemExceptionAsync("AsistanSohbet", "YardimDizini", ex);
        }
        finally
        {
            IsDizinHazirlaniyor = false;
        }
    }

    /// <summary>Yardım dizini durumunu okur (madde sayısı + semantik indeks var/yok) — Kural 11: boş-durum metni.</summary>
    public async Task DizinDurumunuYukleAsync()
    {
        if (_yardimTabani == null)
        {
            DizinDurumMetni = string.Empty;
            return;
        }
        try
        {
            var durum = await _yardimTabani.DurumGetirAsync();
            DizinDurumMetni = durum.MaddeSayisi == 0
                ? "Yardım dizini hazır değil — ilk soruda hazırlanır"
                : durum.VektorVarMi
                    ? $"Yardım dizini hazır: {durum.MaddeSayisi} madde • anlamsal arama açık"
                    : $"Yardım dizini hazır: {durum.MaddeSayisi} madde • yalnız anahtar kelime";
        }
        catch (Exception ex)
        {
            DizinDurumMetni = "Yardım dizini okunamadı: " + ex.Message;
        }
    }

    public void Temizle()
    {
        Mesajlar.Clear();
        HataMetni = string.Empty;
    }

    /// <summary>Yüklü model alias'ı ayardaki alias ile uyuşuyor mu (denetim + soru öncesi).</summary>
    private static bool ModelAliasUyusuyorMu(AsistanDurumDto durum, AiAsistanSettings ayar) =>
        string.Equals(durum.Mesaj, ayar.GetModelAlias(), StringComparison.OrdinalIgnoreCase);

    private string? FirmaAdi()
    {
        var kisa = _secim?.SelectedFirma?.KisaUnvani;
        if (!string.IsNullOrWhiteSpace(kisa))
            return kisa;
        var tam = _secim?.SelectedFirma?.TamUnvani;
        if (!string.IsNullOrWhiteSpace(tam))
            return tam;
        var kod = _secim?.SelectedFirma?.FirmaKodu;
        return string.IsNullOrWhiteSpace(kod) ? null : kod;
    }

    private string? DonemAdi()
    {
        var donem = _secim?.SelectedMaliDonem;
        return donem != null && donem.MaliYil > 0 ? donem.MaliYil.ToString() : null;
    }
}
