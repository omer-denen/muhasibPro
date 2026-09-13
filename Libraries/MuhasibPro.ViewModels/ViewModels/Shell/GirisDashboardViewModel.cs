using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using System.Collections.ObjectModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

/// <summary>Firma kartı içindeki mali dönem satırı (expander içeriği; salt okunur özet).</summary>
public class DonemKarti
{
    public int MaliYil { get; init; }
    public string DatabaseName { get; init; } = string.Empty;
    public string AltMetin => string.IsNullOrWhiteSpace(DatabaseName) ? "Veritabanı adı yok" : DatabaseName;
}

/// <summary>Giriş dashboard kartı (salt özet: unvan + kod + dönem sayısı + dönem satırları).</summary>
public class FirmaDashboardKarti
{
    public long FirmaId { get; init; }
    public string Unvan { get; init; } = string.Empty;
    public string Kod { get; init; } = string.Empty;
    public string Initials { get; init; } = "?";
    public int DonemSayisi { get; init; }
    public IReadOnlyList<int> Yillar { get; init; } = Array.Empty<int>();
    public IReadOnlyList<DonemKarti> Donemler { get; init; } = Array.Empty<DonemKarti>();

    /// <summary>Expander açıklaması: "3 dönem • 2025, 2026, 2027".</summary>
    public string DonemOzeti => DonemSayisi == 0
        ? "Dönem yok"
        : $"{DonemSayisi} dönem{(DonemSayisi > 1 ? " • " + string.Join(", ", Yillar) : string.Empty)}";
}

/// <summary>Denetim Masası "Giriş" bölümü (dashboard: uygulama/sistem veritabanı/güncelleme durumu + firmalar).
/// Tek cümle: giriş yapanın firma/dönem özetini + uygulama durum bilgilerini kurar,
/// bölüm linklerini üst VM'e iletir.</summary>
public class GirisDashboardViewModel : ViewModelBase
{
    private readonly IFirmaService _firmaService;
    private readonly IAuthenticationService _auth;
    private readonly ISistemDatabaseService _sistemDb;
    private readonly IUpdateService _updateService;

    public GirisDashboardViewModel(
        ICommonServices commonServices,
        IFirmaService firmaService = null,
        IAuthenticationService auth = null,
        ISistemDatabaseService sistemDb = null,
        IUpdateService updateService = null) : base(commonServices)
    {
        _firmaService = firmaService;
        _auth = auth;
        _sistemDb = sistemDb;
        _updateService = updateService;
    }

    /// <summary>RichButton/bağlantı kaynaklı bölüm isteği (üst VM SeciliBolum'a bağlar).</summary>
    public event Action<AyarBolumu> BolumAcildi;

    public void BolumuAc(AyarBolumu bolum) => BolumAcildi?.Invoke(bolum);

    private string _kullaniciAdi = string.Empty;
    public string KullaniciAdi
    {
        get => _kullaniciAdi;
        private set => Set(ref _kullaniciAdi, value);
    }

    private string _kullaniciRolMetni = string.Empty;
    public string KullaniciRolMetni
    {
        get => _kullaniciRolMetni;
        private set => Set(ref _kullaniciRolMetni, value);
    }

    /// <summary>Kullanıcı adının baş harfleri (foto yoksa varsayılan avatar).</summary>
    public string KullaniciInitials => string.IsNullOrWhiteSpace(KullaniciAdi)
        ? "?"
        : string.Concat(KullaniciAdi.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));

    private object _profilResmi;
    /// <summary>Profil fotoğrafı (yoksa baş harf avatarı).</summary>
    public object ProfilResmi
    {
        get => _profilResmi;
        private set => Set(ref _profilResmi, value);
    }

    public ObservableCollection<FirmaDashboardKarti> Kartlar { get; } = new();

    public bool HasFirma => Kartlar.Count > 0;

    /// <summary>Firma listesi yükleniyor bayrağı (Kural 11: ring + sonuç).</summary>
    private bool _isFirmalarYukleniyor;
    public bool IsFirmalarYukleniyor
    {
        get => _isFirmalarYukleniyor;
        private set
        {
            if (Set(ref _isFirmalarYukleniyor, value))
                NotifyPropertyChanged(nameof(BosDurumGoster));
        }
    }

    /// <summary>Boş-durum metni yalnız yükleme bitince ve kayıt yoksa görünür (Kural 11).</summary>
    public bool BosDurumGoster => !IsFirmalarYukleniyor && !HasFirma;

    /// <summary>Sistem.db analizi sürüyor bayrağı (Kural 11).</summary>
    private bool _isSistemDbYukleniyor;
    public bool IsSistemDbYukleniyor
    {
        get => _isSistemDbYukleniyor;
        private set => Set(ref _isSistemDbYukleniyor, value);
    }

    private string _sistemDbAltMetni = "Analiz ediliyor…";
    /// <summary>RichButton alt satırı: durum + dosya boyutu (yüklenirken "Analiz ediliyor…").</summary>
    public string SistemDbAltMetni
    {
        get => _sistemDbAltMetni;
        private set => Set(ref _sistemDbAltMetni, value);
    }

    private string _sistemDbDetayMetni = string.Empty;
    /// <summary>Analiz detay mesajı (kart üzerinde tooltip).</summary>
    public string SistemDbDetayMetni
    {
        get => _sistemDbDetayMetni;
        private set => Set(ref _sistemDbDetayMetni, value);
    }

    private string _sonKontrolMetni = "Henüz denetlenmedi";
    /// <summary>Güncelleme RichButton alt satırı (denetim sonucu; her Denetim Masası girişinde tazelenir).</summary>
    public string GuncellemeAltMetni
    {
        get => _sonKontrolMetni;
        private set => Set(ref _sonKontrolMetni, value);
    }

    /// <summary>Güncelleme denetimi sürüyor bayrağı (Kural 11 — UI kilitlenmez, ring gösterilir).</summary>
    private bool _isGuncellemeDenetleniyor;
    public bool IsGuncellemeDenetleniyor
    {
        get => _isGuncellemeDenetleniyor;
        private set => Set(ref _isGuncellemeDenetleniyor, value);
    }

    /// <summary>Denetim Masası penceresi başına bir kez güncelleme denetimi (her girişte taranır).</summary>
    private bool _guncellemeDenetlendi;

    public async Task YukleAsync()
    {
        YukleKullaniciBilgisi();
        await FirmalariYukleAsync();
        await SistemDbDurumunuYukleAsync();
        await GuncellemeDurumunuYukleAsync();
    }

    private async Task FirmalariYukleAsync()
    {
        Kartlar.Clear();
        IsFirmalarYukleniyor = true;
        try
        {
            long kullaniciId = 0;
            try { if (_auth != null && _auth.IsAuthenticated) kullaniciId = _auth.CurrentAccount?.KullaniciId ?? 0; }
            catch { kullaniciId = 0; }
            if (kullaniciId <= 0 || _firmaService == null)
                return;
            var sonuc = await _firmaService.GetFirmalarWithUserId(new DataRequest<Firma>(), kullaniciId);
            var firmalar = sonuc?.Data?.Where(f => f != null).ToList() ?? new List<FirmaModel>();
            foreach (var firma in firmalar)
            {
                var donemler = firma.MaliDonemler?.Where(m => m != null).ToList() ?? new List<MaliDonemModel>();
                Kartlar.Add(new FirmaDashboardKarti
                {
                    FirmaId = firma.Id,
                    Unvan = firma.KisaUnvani ?? string.Empty,
                    Kod = firma.FirmaKodu ?? string.Empty,
                    Initials = firma.Initials ?? "?",
                    DonemSayisi = donemler.Count,
                    Yillar = donemler.Select(m => m.MaliYil).Distinct().OrderBy(y => y).ToList(),
                    Donemler = donemler
                        .OrderByDescending(m => m.MaliYil)
                        .Select(m => new DonemKarti
                        {
                            MaliYil = m.MaliYil,
                            DatabaseName = m.DatabaseName ?? string.Empty
                        })
                        .ToList()
                });
            }
        }
        catch
        {
            Kartlar.Clear();
        }
        finally
        {
            IsFirmalarYukleniyor = false;
            NotifyPropertyChanged(nameof(HasFirma));
            NotifyPropertyChanged(nameof(BosDurumGoster));
        }
    }

    private async Task SistemDbDurumunuYukleAsync()
    {
        IsSistemDbYukleniyor = true;
        SistemDbAltMetni = "Analiz ediliyor…";
        try
        {
            if (_sistemDb == null)
            {
                SistemDbAltMetni = "Durum alınamadı";
                return;
            }
            var yanit = await _sistemDb.GetSistemDatabaseStateAsync();
            var analiz = yanit?.Data;
            if (analiz == null)
            {
                SistemDbAltMetni = "Durum alınamadı";
                return;
            }
            SistemDbAltMetni = analiz.GetStatus() switch
            {
                DatabaseStatusResult.Healty => $"Bağlı • Güncel • {analiz.FileSizeDisplay}",
                DatabaseStatusResult.RequiredUpdating => $"Güncelleme gerekli • {analiz.FileSizeDisplay}",
                DatabaseStatusResult.DatabaseNotFound => "Dosya yok",
                DatabaseStatusResult.ConnectionFailed => "Bağlantı yok",
                DatabaseStatusResult.InvalidSchema => "Onarım gerekli",
                DatabaseStatusResult.RestoreCompleted => $"Geri yüklendi • {analiz.FileSizeDisplay}",
                _ => $"Kontrol gerekli • {analiz.FileSizeDisplay}"
            };
            SistemDbDetayMetni = analiz.GetStatusMessage();
        }
        catch
        {
            SistemDbAltMetni = "Durum alınamadı";
        }
        finally
        {
            IsSistemDbYukleniyor = false;
        }
    }

    /// <summary>Her Denetim Masası girişinde güncelleme taraması — UI kilitlenmez (20 sn tavan, Kural 11).</summary>
    private async Task GuncellemeDurumunuYukleAsync()
    {
        if (_guncellemeDenetlendi)
            return;
        _guncellemeDenetlendi = true;
        if (_updateService == null)
        {
            GuncellemeAltMetni = "Güncelleme servisi kullanılamıyor";
            return;
        }
        IsGuncellemeDenetleniyor = true;
        GuncellemeAltMetni = "Denetleniyor…";
        try
        {
            var ayar = await _updateService.GetSettingsAsync();
            if (string.IsNullOrWhiteSpace(ayar?.FeedUrl))
            {
                GuncellemeAltMetni = "Güncelleme kaynağı tanımlı değil";
                return;
            }
            var gorev = _updateService.CheckForUpdatesAsync();
            var tamamlanan = await Task.WhenAny(gorev, Task.Delay(TimeSpan.FromSeconds(20)));
            if (tamamlanan != gorev)
            {
                // Zaman aşımında arka planda kalan görevin hatası gözlemlenir (unobserved exception olmasın)
                _ = gorev.ContinueWith(t => { _ = t.Exception; }, TaskScheduler.Default);
                GuncellemeAltMetni = "Denetim zaman aşımına uğradı";
                return;
            }
            var bilgi = await gorev;
            GuncellemeAltMetni = bilgi == null
                ? $"Güncel • Son kontrol: {DateTime.Now:dd.MM.yyyy HH:mm}"
                : $"Güncelleme var: v{bilgi.TargetFullRelease.Version}";
        }
        catch
        {
            GuncellemeAltMetni = "Denetlenemedi • Kaynağa ulaşılamadı";
        }
        finally
        {
            IsGuncellemeDenetleniyor = false;
        }
    }

    /// <summary>Sistem Veritabanı kartı aksiyonu — Veritabanı bölümünü açar.</summary>
    public void VeritabaniBolumunuAc() => BolumuAc(AyarBolumu.Veritabani);

    /// <summary>Güncelleme kartı aksiyonu — Güncelleme bölümünü açar.</summary>
    public void GuncellemeBolumunuAc() => BolumuAc(AyarBolumu.Guncelleme);

    /// <summary>Karttaki "Gelişmiş Yönetim" linki (yönetim penceresini firma bağlamıyla açar).</summary>
    public async Task YonetimiAcAsync(FirmaDashboardKarti kart)
    {
        if (kart == null || NavigationService == null)
            return;
        try
        {
            await NavigationService.CreateNewViewAsync<MaliDonemYonetimViewModel>(
                new MaliDonemYonetimArgs
                {
                    FirmaId = kart.FirmaId,
                    FirmaKodu = kart.Kod,
                    KisaUnvani = kart.Unvan
                },
                $"{kart.Unvan} — Mali Dönem Yönetimi");
        }
        catch { }
    }

    private void YukleKullaniciBilgisi()
    {
        try
        {
            var hesap = _auth != null && _auth.IsAuthenticated ? _auth.CurrentAccount : null;
            var model = hesap?.KullaniciModel;
            KullaniciAdi = model?.KullaniciAdi ?? string.Empty;
            var rol = model?.Rol?.RolAdi ?? model?.Rol?.RolTip.ToString();
            KullaniciRolMetni = string.IsNullOrWhiteSpace(rol) ? "Kullanıcı" : rol;
            ProfilResmi = model?.ResimSource ?? model?.Resim;
            NotifyPropertyChanged(nameof(KullaniciInitials));
        }
        catch
        {
            KullaniciAdi = string.Empty;
            KullaniciRolMetni = "Kullanıcı";
            ProfilResmi = null;
        }
    }

    /// <summary>Boş-durum "Yeni Firma Oluştur" linki (mevcut FirmaDetails akışı).</summary>
    public async Task YeniFirmaAcAsync()
    {
        if (NavigationService == null)
            return;
        try
        {
            if (IsMainWindow)
                await NavigationService.CreateNewViewAsync<FirmaDetailsViewModel>(new FirmaDetailsArgs());
            else
                NavigationService.Navigate<FirmaDetailsViewModel>(new FirmaDetailsArgs());
        }
        catch { }
    }
}
