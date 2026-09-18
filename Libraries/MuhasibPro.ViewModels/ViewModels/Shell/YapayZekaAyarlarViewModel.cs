using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
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

/// <summary>Denetim Masası "Yapay Zeka" bölümü VM'i (Faz 6.92 Adım 5).
/// Tek cümle: AI ayar modelini (sabit model + davranış eşikleri) panele bağlar; disk/model yönetimini sunar.</summary>
public class YapayZekaAyarlarViewModel : ViewModelBase
{
    private readonly IAiAsistanSettingsProvider? _saglayici;
    private readonly ISurumOzellikService? _surum;
    private readonly IAsistanSohbetService? _sohbet;
    private readonly IYardimBilgiTabani? _yardimTabani;
    private AiAsistanSettings _ayarlar = new();
    private bool _yuklendi;

    public YapayZekaAyarlarViewModel(
        ICommonServices commonServices,
        IAiAsistanSettingsProvider? saglayici = null,
        ISurumOzellikService? surum = null,
        IAsistanSohbetService? sohbet = null,
        IYardimBilgiTabani? yardimTabani = null) : base(commonServices)
    {
        _saglayici = saglayici;
        _surum = surum;
        _sohbet = sohbet;
        _yardimTabani = yardimTabani;
    }

    public async Task LoadAsync()
    {
        IsYukleniyor = true;
        try
        {
            if (_saglayici != null)
                _ayarlar = await _saglayici.GetAsync() ?? new AiAsistanSettings();
            _yuklendi = true;
            NotifyPropertyChanged(nameof(ModelAlias));
            NotifyPropertyChanged(nameof(EtkinMi));
            NotifyPropertyChanged(nameof(MaksGecmisTur));
            NotifyPropertyChanged(nameof(EnFazlaMadde));
            NotifyPropertyChanged(nameof(SoruZamanAsimiSn));
            await SurumuYukleAsync();
            await ModelDurumunuYukleAsync();
            await DizinDurumunuYukleAsync();
        }
        finally
        {
            IsYukleniyor = false;
        }
        await ModelleriYukleAsync();
    }

    /// <summary>Sabit sohbet modeli (S1 tam kilit, Oturum 292): ürün sabitidir, kullanıcı değiştiremez.</summary>
    public string ModelAlias => _ayarlar.GetModelAlias();

    public bool EtkinMi
    {
        get => _ayarlar.EtkinMi;
        set
        {
            if (_ayarlar.EtkinMi == value) return;
            _ayarlar.EtkinMi = value;
            NotifyPropertyChanged(nameof(EtkinMi));
            _ = SaveAsync();
        }
    }

    public int MaksGecmisTur
    {
        get => _ayarlar.MaksGecmisTur;
        set
        {
            if (_ayarlar.MaksGecmisTur == value) return;
            _ayarlar.MaksGecmisTur = value;
            NotifyPropertyChanged(nameof(MaksGecmisTur));
            _ = SaveAsync();
        }
    }

    public int EnFazlaMadde
    {
        get => _ayarlar.EnFazlaMadde;
        set
        {
            if (_ayarlar.EnFazlaMadde == value) return;
            _ayarlar.EnFazlaMadde = value;
            NotifyPropertyChanged(nameof(EnFazlaMadde));
            _ = SaveAsync();
        }
    }

    public int SoruZamanAsimiSn
    {
        get => _ayarlar.SoruZamanAsimiSn;
        set
        {
            if (_ayarlar.SoruZamanAsimiSn == value) return;
            _ayarlar.SoruZamanAsimiSn = value;
            NotifyPropertyChanged(nameof(SoruZamanAsimiSn));
            _ = SaveAsync();
        }
    }

    private bool _isYukleniyor;
    public bool IsYukleniyor
    {
        get => _isYukleniyor;
        private set => Set(ref _isYukleniyor, value);
    }

    private string _hataMetni = string.Empty;
    public string HataMetni
    {
        get => _hataMetni;
        private set => Set(ref _hataMetni, value);
    }

    private string _surumDurumMetni = string.Empty;
    public string SurumDurumMetni
    {
        get => _surumDurumMetni;
        private set => Set(ref _surumDurumMetni, value);
    }

    private string _modelDurumMetni = string.Empty;
    public string ModelDurumMetni
    {
        get => _modelDurumMetni;
        private set => Set(ref _modelDurumMetni, value);
    }

    private string _dizinDurumMetni = string.Empty;
    /// <summary>Yardım bilgi tabanı dizini (madde sayısı + semantik indeks) — Faz 6.93.</summary>
    public string DizinDurumMetni
    {
        get => _dizinDurumMetni;
        private set => Set(ref _dizinDurumMetni, value);
    }

    /// <summary>İndirilmiş modeller (disk yönetimi listesi).</summary>
    public ObservableCollection<AsistanModelSatiri> Modeller { get; } = new();

    private bool _isModellerYukleniyor;
    public bool IsModellerYukleniyor
    {
        get => _isModellerYukleniyor;
        private set => Set(ref _isModellerYukleniyor, value);
    }

    private bool _modelListesiBosMu = true;
    public bool ModelListesiBosMu
    {
        get => _modelListesiBosMu;
        private set => Set(ref _modelListesiBosMu, value);
    }

    private string _diskMetni = "—";
    /// <summary>Disk özeti ("N model • X GB").</summary>
    public string DiskMetni
    {
        get => _diskMetni;
        private set => Set(ref _diskMetni, value);
    }

    private string _modellerHataMetni = string.Empty;
    /// <summary>Model listesi hatası (genel HataMetni'nden ayrı — yetki hatasıyla karışmasın).</summary>
    public string ModellerHataMetni
    {
        get => _modellerHataMetni;
        private set => Set(ref _modellerHataMetni, value);
    }

    private bool _isModelIslemde;
    /// <summary>Model silme işlemi sürüyor (Kural 11).</summary>
    public bool IsModelIslemde
    {
        get => _isModelIslemde;
        private set { if (Set(ref _isModelIslemde, value)) ModelKomutlariniTazele(); }
    }

    private AsyncRelayCommand? _modelleriYenile;
    public ICommand ModelleriYenileCommand => _modelleriYenile ??= new AsyncRelayCommand(ModelleriYukleAsync);

    private AsyncRelayCommand<AsistanModelSatiri>? _modelSil;
    public ICommand ModelSilCommand => _modelSil ??= new AsyncRelayCommand<AsistanModelSatiri>(
        ModelSilAsync, s => s != null && !IsModelIslemde);

    private void ModelKomutlariniTazele()
    {
        _modelleriYenile?.RaiseCanExecuteChanged();
        _modelSil?.RaiseCanExecuteChanged();
    }

    public async Task SaveAsync()
    {
        if (!_yuklendi || _saglayici == null)
            return;
        try
        {
            HataMetni = string.Empty;
            await _saglayici.SaveAsync(_ayarlar);
        }
        catch (UnauthorizedAccessException)
        {
            HataMetni = "Bu ayar yalnızca yönetici tarafından değiştirilebilir.";
            StatusError(HataMetni);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            HataMetni = ex.Message;
            StatusError(ex.Message);
        }
    }

    /// <summary>İndirilmiş modelleri + disk özetini yükler (Kural 11 bayrak; hata listeye özel metinde).</summary>
    public async Task ModelleriYukleAsync()
    {
        if (_sohbet == null)
        {
            ModellerHataMetni = "Asistan servisi bulunamadı.";
            return;
        }
        IsModellerYukleniyor = true;
        ModelListesiBosMu = false;
        ModellerHataMetni = string.Empty;
        try
        {
            var liste = await _sohbet.ModelleriGetirAsync();
            var disk = await _sohbet.DiskKullanimiAsync();
            await ContextService.RunAsync(() =>
            {
                Modeller.Clear();
                foreach (var satir in liste
                    .Where(m => m.IndirildiMi)
                    .OrderByDescending(m => m.YukluMu)
                    .ThenBy(m => m.GosterimAdi, StringComparer.OrdinalIgnoreCase))
                {
                    Modeller.Add(new AsistanModelSatiri(satir));
                }
                ModelListesiBosMu = Modeller.Count == 0;
            });
            DiskMetni = disk.ModelSayisi == 0
                ? "İndirilmiş model yok"
                : $"{disk.ModelSayisi} model • {AsistanModelSatiri.BaytMetni(disk.ToplamBayt)}";
        }
        catch (Exception ex)
        {
            ModellerHataMetni = ex.Message;
        }
        finally
        {
            IsModellerYukleniyor = false;
        }
    }

    /// <summary>Modeli diskten siler (onaylı; yüklüyse servis önce bırakır).</summary>
    public async Task ModelSilAsync(AsistanModelSatiri satir)
    {
        if (satir == null || _sohbet == null)
            return;
        var ekUyari = satir.YukluMu ? " Model şu an yüklü; silmeden önce bellekten bırakılacak." : string.Empty;
        var onay = await DialogService.ShowConfirmationAsync(
            "Model silinsin mi?",
            $"'{satir.Baslik}' modeli diskten kalıcı olarak silinecek.{ekUyari} Bu işlem geri alınamaz.",
            "Kalıcı Sil",
            "Vazgeç");
        if (!onay)
            return;

        IsModelIslemde = true;
        HataMetni = string.Empty;
        try
        {
            var sonuc = await _sohbet.ModelSilAsync(satir.Alias);
            if (sonuc.BasariliMi)
            {
                StatusActionMessage(sonuc.Mesaj, StatusMessageType.Success);
                await ModelDurumunuYukleAsync();
                await ModelleriYukleAsync();
            }
            else
            {
                HataMetni = sonuc.Mesaj;
                StatusError(sonuc.Mesaj);
            }
        }
        catch (Exception ex)
        {
            HataMetni = ex.Message;
            StatusError(ex.Message);
            await LogSistemExceptionAsync("YapayZekaAyarlar", "ModelSil", ex);
        }
        finally
        {
            IsModelIslemde = false;
        }
    }

    private async Task SurumuYukleAsync()
    {
        if (_surum == null)
        {
            SurumDurumMetni = "Denetlenemedi.";
            return;
        }
        try
        {
            var hak = await _surum.AiAsistanHakkiAsync();
            SurumDurumMetni = hak.HakVarMi
                ? $"{hak.Tur} sürümü — AI asistan etkin"
                : string.IsNullOrWhiteSpace(hak.Gerekce) ? $"{hak.Tur} sürümü — AI kapalı" : hak.Gerekce;
        }
        catch
        {
            SurumDurumMetni = "Denetlenemedi.";
        }
    }

    private async Task ModelDurumunuYukleAsync()
    {
        if (_sohbet == null)
        {
            ModelDurumMetni = "Bilinmiyor.";
            return;
        }
        try
        {
            var durum = await _sohbet.DurumuGetirAsync();
            ModelDurumMetni = durum.HazirMi ? $"Hazır ({durum.Mesaj})" : "Hazır değil — çalışma alanındaki asistan panelinden indirilir";
        }
        catch
        {
            ModelDurumMetni = "Bilinmiyor.";
        }
    }

    /// <summary>Yardım bilgi tabanı dizinini okur (Faz 6.93) — burada hazırlanmaz; panelde ilk soruda kurulur.</summary>
    private async Task DizinDurumunuYukleAsync()
    {
        if (_yardimTabani == null)
        {
            DizinDurumMetni = "Bilinmiyor.";
            return;
        }
        try
        {
            var durum = await _yardimTabani.DurumGetirAsync();
            DizinDurumMetni = durum.MaddeSayisi == 0
                ? "Hazır değil — çalışma alanındaki asistan panelinde ilk soruda hazırlanır"
                : durum.VektorVarMi
                    ? $"{durum.MaddeSayisi} madde • anlamsal arama açık"
                    : $"{durum.MaddeSayisi} madde • yalnız anahtar kelime";
        }
        catch
        {
            DizinDurumMetni = "Bilinmiyor.";
        }
    }

}
