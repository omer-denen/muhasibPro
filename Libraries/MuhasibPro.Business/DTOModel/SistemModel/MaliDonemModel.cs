using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Business.DTOModel.SistemModel;

public class MaliDonemModel : ObservableObject
{
    public static MaliDonemModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };

    public long FirmaId { get; set; }
    public int MaliYil { get; set; }
    public string DatabaseName { get; set; }

    public DatabaseType DatabaseType { get; set; }
    public FirmaModel FirmaModel { get; set; }

    /// <summary>
    /// Seçili firma/dönemin tenant veritabanı vitrin verisi (GetTenantDetails akışı).
    /// Dönem kartı bu nesneden boyut / yedek / durum göstermek için kullanılır.
    /// </summary>
    public TenantDetailsModel TenantDetails { get; set; }

    public bool IsNew => Id <= 0;

    /// <summary>Arşivlenmiş mi? (TenantDetails yoksa false).</summary>
    public bool ArsivliMi => TenantDetails?.ArsivliMi ?? false;

    /// <summary>Kapalı/arşiv pill'i — Durum != Acik veya AktifMi=false veya arşiv.</summary>
    public bool KapaliMi => ArsivliMi || !AktifMi || (TenantDetails != null && TenantDetails.Durum != Domain.Enum.DonemDurum.Acik);

    /// <summary>DB Durumu kısa rozet metni (kartta asla "-" yok).</summary>
    public string DbKisaDurumMetni => !DbAnalizYapildi ? "Analiz bekleniyor" : DbDurum switch
    {
        DatabaseStatusResult.Healty => "Güncel",
        DatabaseStatusResult.RequiredUpdating => "Güncelleme Gerekli",
        DatabaseStatusResult.DatabaseNotFound => "Dosya Yok",
        DatabaseStatusResult.ConnectionFailed => "Bağlantı Yok",
        DatabaseStatusResult.InvalidSchema => "Onarım Gerekli",
        DatabaseStatusResult.UnknownError => "Kontrol Gerekli",
        DatabaseStatusResult.RestoreCompleted => "Geri Yüklendi",
        _ => "Durum yok"
    };

    /// <summary>Boyut metni (ör. 36 KB / 14.8 MB) — B/KB/MB/GB akıllı format.</summary>
    public string BoyutMetni
    {
        get
        {
            if (TenantDetails?.DosyaBoyutu is not long bytes || bytes <= 0)
                return "Boyut hesaplanamadı";
            if (bytes < 1024)
                return $"{bytes} B";
            double kb = bytes / 1024d;
            if (kb < 1024)
                return $"{kb:0.#} KB";
            double mb = kb / 1024d;
            return mb >= 1024 ? $"{mb / 1024:0.#} GB" : $"{mb:0.#} MB";
        }
    }

    /// <summary>Vitrin metinlerini tazele (TenantDetails analiz/backfill sonrası kart güncellensin).</summary>
    public void RefreshVitrin()
    {
        NotifyPropertyChanged(nameof(BoyutMetni));
        NotifyPropertyChanged(nameof(SonYedekMetni));
        NotifyPropertyChanged(nameof(ArsivButonMetni));
    }

    /// <summary>Arşiv butonu metni — arşivliyse "Arşivden Çıkar", değilse "Arşivle".</summary>
    public string ArsivButonMetni => ArsivliMi ? "Arşivden Çıkar" : "Arşivle";

    /// <summary>Son yedek tarihi metni — yedek yoksa "Yedek bulunamadı." (kartta asla "-" yok).</summary>
    public string SonYedekMetni
    {
        get
        {
            if (TenantDetails?.SonYedekTarihi is not DateTime dt || dt == default)
                return "Yedek bulunamadı.";
            return dt.ToString("yyyy-MM-dd HH:mm");
        }
    }

    private bool _dbAnalizYapildi;
    /// <summary>
    /// Tenant veritabanı analiz durumu (GetTenantDatabaseStateAsync). DbAnalizYapildi=true iken geçerli;
    /// liste yüklendiğinde işlenir (FirmaShell dönem kartı "DB Durumu" rozeti).
    /// </summary>
    public bool DbAnalizYapildi
    {
        get => _dbAnalizYapildi;
        set
        {
            if (Set(ref _dbAnalizYapildi, value))
            {
                NotifyPropertyChanged(nameof(DbGuncelMi));
                NotifyPropertyChanged(nameof(DbGuncellemeGerekliMi));
                NotifyPropertyChanged(nameof(DbDosyaYokMu));
                NotifyPropertyChanged(nameof(DbKurtarilabilir));
                NotifyPropertyChanged(nameof(DbKontrolGerekliMi));
                NotifyPropertyChanged(nameof(DbKisaDurumMetni));
            }
        }
    }

    private DatabaseStatusResult _dbDurum;
    /// <summary>Analiz sonucu durum (Healty = Güncel, RequiredUpdating = Güncelleme Gerekli, ...).</summary>
    public DatabaseStatusResult DbDurum
    {
        get => _dbDurum;
        set
        {
            if (Set(ref _dbDurum, value))
            {
                NotifyPropertyChanged(nameof(DbGuncelMi));
                NotifyPropertyChanged(nameof(DbGuncellemeGerekliMi));
                NotifyPropertyChanged(nameof(DbDosyaYokMu));
                NotifyPropertyChanged(nameof(DbKurtarilabilir));
                NotifyPropertyChanged(nameof(DbKontrolGerekliMi));
                NotifyPropertyChanged(nameof(DbKisaDurumMetni));
            }
        }
    }

    private int _dbBekleyenGuncellemeSayisi;
    /// <summary>Bekleyen güncelleme (migration) sayısı — "Güncelleme Gerekli" iken &gt; 0.</summary>
    public int DbBekleyenGuncellemeSayisi { get => _dbBekleyenGuncellemeSayisi; set => Set(ref _dbBekleyenGuncellemeSayisi, value); }

    private string _dbAnalizDetay;
    /// <summary>Analiz detay mesajı (kart üzerinde tooltip).</summary>
    public string DbAnalizDetay { get => _dbAnalizDetay; set => Set(ref _dbAnalizDetay, value); }

    private DatabaseConnectionAnalysis _dbAnaliz;
    /// <summary>
    /// Son tam analiz nesnesi (yönetim paneli spec-sheet buradan okur: sürüm/tablo/bekleyen migration).
    /// Analiz yoksa (dosya silinmişse de) null olabilir — spec-sheet null-güvenli gösterir.
    /// </summary>
    public DatabaseConnectionAnalysis DbAnaliz
    {
        get => _dbAnaliz;
        set
        {
            if (Set(ref _dbAnaliz, value))
                NotifyPropertyChanged(nameof(DbBekleyenGuncellemeListesi));
        }
    }

    /// <summary>Bekleyen migration listesi (analiz yoksa boş).</summary>
    public List<string> DbBekleyenGuncellemeListesi => DbAnaliz?.PendingMigrations ?? new List<string>();

    private TenantDerinAnaliz _dbDerin;
    /// <summary>Derin analiz sonucu (Genel Bakış tablosu: WAL/boyut/kayıt). Yoksa null — şablon FallbackValue gösterir.</summary>
    public TenantDerinAnaliz DbDerin
    {
        get => _dbDerin;
        set
        {
            if (Set(ref _dbDerin, value))
            {
                NotifyPropertyChanged(nameof(DbWalMetni));
                NotifyPropertyChanged(nameof(DbTabloKayitMetni));
            }
        }
    }

    /// <summary>WAL dosya boyutu metni (derin analizden; yoksa "—").</summary>
    public string DbWalMetni
    {
        get
        {
            if (DbDerin == null)
                return "—";
            long b = DbDerin.WalBoyutu;
            if (b < 1024) return $"{b} B";
            return $"{b / 1024d:0.#} KB";
        }
    }

    /// <summary>Tablo/kayıt özeti (derin analizden; yoksa "—").</summary>
    public string DbTabloKayitMetni => DbDerin == null ? "—" : $"{DbDerin.TabloSayisi} Tablo • {DbDerin.ToplamKayit} Kayıt";

    private bool _isDbAnalyzing;
    /// <summary>Seçimde analiz sürerken kart içinde ProgressRing gösterilir.</summary>
    public bool IsDbAnalyzing { get => _isDbAnalyzing; set => Set(ref _isDbAnalyzing, value); }

    private bool _sonCalisilanMi;
    /// <summary>En son giriş yapılan dönem mi? (FirmaShell satırında "Son çalışılan" rozeti; liste yüklemede işaretlenir).</summary>
    public bool SonCalisilanMi { get => _sonCalisilanMi; set => Set(ref _sonCalisilanMi, value); }

    public bool DbGuncelMi => DbAnalizYapildi && DbDurum == DatabaseStatusResult.Healty;

    public bool DbGuncellemeGerekliMi => DbAnalizYapildi && DbDurum == DatabaseStatusResult.RequiredUpdating;

    public bool DbDosyaYokMu => DbAnalizYapildi && DbDurum == DatabaseStatusResult.DatabaseNotFound;

    private bool _dbYedekVarMi;
    /// <summary>Dosya yokken geri yüklenebilecek yedek var mı? (kart Kurtar butonu).</summary>
    public bool DbYedekVarMi
    {
        get => _dbYedekVarMi;
        set
        {
            if (Set(ref _dbYedekVarMi, value))
                NotifyPropertyChanged(nameof(DbKurtarilabilir));
        }
    }

    private int _dbYedekSayisi;
    /// <summary>Bulunan yedek sayısı (Kurtar dialogu + bilgi).</summary>
    public int DbYedekSayisi { get => _dbYedekSayisi; set => Set(ref _dbYedekSayisi, value); }

    /// <summary>Dosya yok + yedek var → Kurtar göster; dosya yok + yedek yok → sadece Sil.</summary>
    public bool DbKurtarilabilir => DbDosyaYokMu && DbYedekVarMi;

    public bool DbKontrolGerekliMi => DbAnalizYapildi
        && (DbDurum == DatabaseStatusResult.InvalidSchema
            || DbDurum == DatabaseStatusResult.ConnectionFailed
            || DbDurum == DatabaseStatusResult.UnknownError);

    public override void Merge(ObservableObject source)
    {
        if (source is MaliDonemModel model)
            Merge(model);
    }

    public void Merge(MaliDonemModel source)
    {
        if (source != null)
        {
            Id = source.Id;
            FirmaId = source.FirmaId;
            MaliYil = source.MaliYil;
            DatabaseName = source.DatabaseName;
            DatabaseType = source.DatabaseType;
            FirmaModel = source.FirmaModel;
            TenantDetails = source.TenantDetails;
            DbAnalizYapildi = source.DbAnalizYapildi;
            DbDurum = source.DbDurum;
            DbBekleyenGuncellemeSayisi = source.DbBekleyenGuncellemeSayisi;
            DbAnalizDetay = source.DbAnalizDetay;
            DbAnaliz = source.DbAnaliz;
            DbYedekVarMi = source.DbYedekVarMi;
            DbYedekSayisi = source.DbYedekSayisi;

            AktifMi = source.AktifMi;
            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;
        }
    }

    public override string ToString() { return IsEmpty ? "----" : $"{MaliYil} - {FirmaModel?.KisaUnvani}"; }
}
