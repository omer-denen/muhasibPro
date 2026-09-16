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
    private CancellationTokenSource? _gonderCts;

    public AsistanSohbetViewModel(
        ICommonServices commonServices,
        IAsistanSohbetService sohbet,
        IAiAsistanSettingsProvider ayarlar,
        ISurumOzellikService surum,
        IPermissionService yetki,
        IFirmaWithMaliDonemSelectedService secim) : base(commonServices)
    {
        _sohbet = sohbet;
        _ayarlar = ayarlar;
        _surum = surum;
        _yetki = yetki;
        _secim = secim;
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
        !KilitliMi && !IsGonderiliyor && !IsHazirlaniyor && !string.IsNullOrWhiteSpace(SoruMetni);

    public async Task LoadAsync()
    {
        if (Mesajlar.Count == 0)
        {
            Mesajlar.Add(new AsistanMesajSatiri(false,
                "Merhaba! Ben MuhasibPro yardım asistanıyım. Uygulamanın yardım maddelerine göre cevap veririm — örneğin 'mali dönem nasıl arşivlenir?' diye sorabilirsin."));
        }
        await KapiyiDenetleAsync();
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
                ModelDurumMetni = durum.HazirMi ? $"Model hazır ({durum.Mesaj})" : "Model hazır değil — ilk soruda indirilir";
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

    public async Task GonderAsync()
    {
        if (!GonderilebilirMi())
            return;
        HataMetni = string.Empty;
        await KapiyiDenetleAsync();
        if (KilitliMi || string.IsNullOrWhiteSpace(SoruMetni))
            return;

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
            var istek = new AsistanSoruDto
            {
                Soru = soru,
                SayfaAnahtari = MainShellViewModel.YardimAnahtari,
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
            StatusError(ex.Message);
        }
        finally
        {
            IsHazirlaniyor = false;
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
