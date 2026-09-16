using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Denetim Masası "Yapay Zeka" bölümü VM'i (Faz 6.92 Adım 5).
/// Tek cümle: AI ayar modelini panele bağlar + model hazırlığını çalıştırır.</summary>
public class YapayZekaAyarlarViewModel : ViewModelBase
{
    internal const string YardimAnahtari = "YapayZeka";
    internal const string YardimBasligi = "Yapay Zeka — Yardım";

    private readonly IAiAsistanSettingsProvider? _saglayici;
    private readonly IAuthenticationService? _auth;
    private readonly ISurumOzellikService? _surum;
    private readonly IAsistanSohbetService? _sohbet;
    private AiAsistanSettings _ayarlar = new();
    private bool _yuklendi;

    public YapayZekaAyarlarViewModel(
        ICommonServices commonServices,
        IAiAsistanSettingsProvider? saglayici = null,
        IAuthenticationService? auth = null,
        ISurumOzellikService? surum = null,
        IAsistanSohbetService? sohbet = null) : base(commonServices)
    {
        _saglayici = saglayici;
        _auth = auth;
        _surum = surum;
        _sohbet = sohbet;
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
            NotifyPropertyChanged(nameof(IsYonetici));
            await SurumuYukleAsync();
            await ModelDurumunuYukleAsync();
        }
        finally
        {
            IsYukleniyor = false;
        }
        await ModelleriYukleAsync();
    }

    public string ModelAlias
    {
        get => _ayarlar.ModelAlias;
        set
        {
            if (_ayarlar.ModelAlias == value) return;
            _ayarlar.ModelAlias = value;
            NotifyPropertyChanged(nameof(ModelAlias));
            _ = SaveAsync();
        }
    }

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

    public bool IsYonetici => AyarYetkiDenetimi.KullaniciYoneticiMi(_auth);

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

    private bool _isModelUygulaniyor;
    /// <summary>Alias uygulama (eski bırak + yeni hazırla) sürüyor (Kural 12: determinate).</summary>
    public bool IsModelUygulaniyor
    {
        get => _isModelUygulaniyor;
        private set { if (Set(ref _isModelUygulaniyor, value)) ModelKomutlariniTazele(); }
    }

    private double _uygulamaYuzde;
    public double UygulamaYuzde
    {
        get => _uygulamaYuzde;
        private set => Set(ref _uygulamaYuzde, value);
    }

    private string _uygulamaAsamasi = string.Empty;
    public string UygulamaAsamasi
    {
        get => _uygulamaAsamasi;
        private set => Set(ref _uygulamaAsamasi, value);
    }

    private AsyncRelayCommand? _sifirla;
    public ICommand ModelAliasSifirlaCommand => _sifirla ??= new AsyncRelayCommand(ModelAliasSifirlaAsync);
    private AsyncRelayCommand? _yardim;
    public ICommand YardimCommand => _yardim ??= new AsyncRelayCommand(YardimGoster);

    private AsyncRelayCommand? _modelleriYenile;
    public ICommand ModelleriYenileCommand => _modelleriYenile ??= new AsyncRelayCommand(ModelleriYukleAsync);

    private AsyncRelayCommand<AsistanModelSatiri>? _modelSil;
    public ICommand ModelSilCommand => _modelSil ??= new AsyncRelayCommand<AsistanModelSatiri>(
        ModelSilAsync, s => s != null && !IsModelIslemde && !IsModelUygulaniyor);

    private AsyncRelayCommand? _aliasUygula;
    public ICommand ModelAliasUygulaCommand => _aliasUygula ??= new AsyncRelayCommand(ModelAliasUygulaAsync, AliasUygulanabilirMi);

    private void ModelKomutlariniTazele()
    {
        _modelleriYenile?.RaiseCanExecuteChanged();
        _modelSil?.RaiseCanExecuteChanged();
        _aliasUygula?.RaiseCanExecuteChanged();
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

    public async Task ModelAliasSifirlaAsync()
    {
        _ayarlar.ModelAlias = AiAsistanSettings.VarsayilanModelAlias;
        NotifyPropertyChanged(nameof(ModelAlias));
        await SaveAsync();
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

    private bool AliasUygulanabilirMi() =>
        _sohbet != null && IsYonetici && !IsModelUygulaniyor && !IsModelIslemde && !IsYukleniyor;

    /// <summary>Model adını uygular: ayarı kaydeder, onay alır, eski modeli bırakıp yeniyi hazırlar.</summary>
    public async Task ModelAliasUygulaAsync()
    {
        if (_sohbet == null)
            return;
        await SaveAsync();
        if (!string.IsNullOrEmpty(HataMetni))
            return;
        var alias = _ayarlar.ModelAlias;
        var onay = await DialogService.ShowConfirmationAsync(
            "Model uygulanısın mı?",
            $"'{alias}' modeli etkinleştirilecek: eski model bellekten bırakılır, yeni model gerekirse indirilip yüklenir. İndirme uzun sürebilir. Onaylıyor musunuz?",
            "Uygula",
            "Vazgeç");
        if (!onay)
            return;

        IsModelUygulaniyor = true;
        HataMetni = string.Empty;
        UygulamaYuzde = 0;
        UygulamaAsamasi = "Hazırlanıyor";
        try
        {
            var ilerleme = new Progress<AsistanDurumDto>(d =>
            {
                UygulamaYuzde = d.IlerlemeYuzde ?? 0;
                UygulamaAsamasi = d.Asama;
            });
            await _sohbet.AliasDegisiminiUygulaAsync(alias, ilerleme);
            await ModelDurumunuYukleAsync();
            await ModelleriYukleAsync();
            StatusActionMessage($"Model hazır: {alias}", StatusMessageType.Success);
        }
        catch (Exception ex)
        {
            HataMetni = ex.Message;
            StatusError(ex.Message);
            await LogSistemExceptionAsync("YapayZekaAyarlar", "ModelAliasUygula", ex);
        }
        finally
        {
            IsModelUygulaniyor = false;
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

    private async Task YardimGoster() =>
        await DialogService.ShowYardimAsync(YardimBasligi, YardimMaddeleri());

    /// <summary>Kural 13 içeriği (AI öz-yardımı RAG derlemi dışındadır — meta içerik).</summary>
    internal static List<YardimMaddesiDto> YardimMaddeleri() => new()
    {
        new() { Baslik = "Bu bölüm ne yapar?", Aciklama = "AI yardım asistanının ayarlarını ve model durumunu yönetir: sürüm hakkı, model seçimi ve davranış eşikleri." },
        new() { Baslik = "Sürüm hakkı", Aciklama = "Asistan Profesyonel ve Kurumsal sürümlerde (Deneme'de açık) çalışır. Hakkınız yoksa gerekçesi burada yazar; sohbet paneli kilitli görünür." },
        new() { Baslik = "Model adı", Aciklama = "Foundry katalogdaki model adıdır. Yalnızca yönetici değiştirir. Adı yazıp 'Uygula' dediğinizde eski model bellekten bırakılır, yeni model gerekirse indirilip yüklenir." },
        new() { Baslik = "Modeli uygula", Aciklama = "'Uygula', seçili model adını etkinleştirir. Yeni model indirilmediyse indirme başlar ve uzun sürebilir; ilerleme bu bölümde çubukla görünür. Uygulama bitince model durumu güncellenir." },
        new() { Baslik = "Model yönetimi", Aciklama = "İndirilmiş modeller boyutu ve 'Yüklü' rozetiyle listelenir; üstteki 'Yenile' listeyi ve disk kullanımını tazeler. 'Sil' modeli diskten kalıcı olarak kaldırır: önce onay ister, yüklü modeli silmeden önce bellekten bırakır. Silme geri alınamaz." },
        new() { Baslik = "Model indirme", Aciklama = "Model/yürütücü indirme bu bölümde değil, çalışma alanındaki AI Yardım Asistanı panelinden yapılır (statü çubuğu → Asistan; ilk soruda otomatik indirilir). İlerleme panelde görünür." },
        new() { Baslik = "Davranış eşikleri", Aciklama = "Geçmiş turu (soruya eklenen konuşma), madde sayısı (prompt'a giren yardım maddesi) ve soru zaman aşımı buradan ayarlanır." },
        new() { Baslik = "Etkin / kapalı", Aciklama = "Kapalıysa sohbet paneli kilitli görünür ve soru alınmaz. Model diskte kalır, yeniden açınca hazırlanır." },
    };
}
