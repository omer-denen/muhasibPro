using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>
/// Genel Bakış bölümü: KPI'lar + toplu işlemler + arama + dönem tablosu.
/// Tek sorumluluk: firmanın tüm dönemlerine kuşbakışı + batch operasyonlar.
/// </summary>
public class DonemGenelBakisViewModel : ViewModelBase
{
    public DonemGenelBakisViewModel(
        ICommonServices commonServices,
        MaliDonemListViewModel maliDonemList,
        ITenantSQLiteDatabaseOperationService operationService) : base(commonServices)
    {
        MaliDonemList = maliDonemList;
        OperationService = operationService;
    }

    public MaliDonemListViewModel MaliDonemList { get; }
    public ITenantSQLiteDatabaseOperationService OperationService { get; }

    private string _aramaMetni = string.Empty;
    public string AramaMetni
    {
        get => _aramaMetni;
        set
        {
            if (Set(ref _aramaMetni, value ?? string.Empty))
                Filtrele();
        }
    }

    private List<MaliDonemModel> _filtreliDonemler = new();
    public List<MaliDonemModel> FiltreliDonemler
    {
        get => _filtreliDonemler;
        private set => Set(ref _filtreliDonemler, value);
    }

    private string _toplamDepolamaMetni = "—";
    public string ToplamDepolamaMetni
    {
        get => _toplamDepolamaMetni;
        private set => Set(ref _toplamDepolamaMetni, value);
    }

    private string _kayitliDbAltMetni = "—";
    public string KayitliDbAltMetni
    {
        get => _kayitliDbAltMetni;
        private set => Set(ref _kayitliDbAltMetni, value);
    }

    private string _ortalamaDepolamaMetni = "—";
    public string OrtalamaDepolamaMetni
    {
        get => _ortalamaDepolamaMetni;
        private set => Set(ref _ortalamaDepolamaMetni, value);
    }

    private string _butunlukAltMetni = "—";
    public string ButunlukAltMetni
    {
        get => _butunlukAltMetni;
        private set => Set(ref _butunlukAltMetni, value);
    }

    private string _gosterilenMetni = string.Empty;
    public string GosterilenMetni
    {
        get => _gosterilenMetni;
        private set => Set(ref _gosterilenMetni, value);
    }

    private string _walModuMetni = "—";
    public string WalModuMetni
    {
        get => _walModuMetni;
        private set => Set(ref _walModuMetni, value);
    }

    private string _butunlukSkoruMetni = "—";
    public string ButunlukSkoruMetni
    {
        get => _butunlukSkoruMetni;
        private set => Set(ref _butunlukSkoruMetni, value);
    }

    private bool _isGenelBakisYukleniyor;
    /// <summary>KPI şeridi tazelenirken iskelet/ring gösterir (Kural 11).</summary>
    public bool IsGenelBakisYukleniyor
    {
        get => _isGenelBakisYukleniyor;
        private set => Set(ref _isGenelBakisYukleniyor, value);
    }

    /// <summary>Liste yenilendiğinde çağrılır: filtre + KPI (bellek-içi, hızlı).
    /// Ağır işler burada YOK: durum rozetleri liste akışının paralel analizinden,
    /// derin analizler "Derin Analiz Çalıştır" butonundan gelir (lazy).</summary>
    public async Task YenileAsync()
    {
        IsGenelBakisYukleniyor = true;
        try
        {
            Filtrele();
            HesaplaKpi();
            await Task.CompletedTask;
        }
        finally
        {
            IsGenelBakisYukleniyor = false;
        }
    }

    public void Filtrele()
    {
        var tum = MaliDonemList.ItemsSource?.Where(m => m != null).ToList() ?? new List<MaliDonemModel>();
        var q = _aramaMetni.Trim();
        FiltreliDonemler = string.IsNullOrEmpty(q)
            ? tum
            : tum.Where(m =>
                m.MaliYil.ToString().Contains(q) ||
                (m.DatabaseName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                m.DbKisaDurumMetni.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        GosterilenMetni = $"Gösterilen: {FiltreliDonemler.Count} / {tum.Count} Veritabanı";
    }

    private void HesaplaKpi()
    {
        var tum = MaliDonemList.ItemsSource?.Where(m => m != null).ToList() ?? new List<MaliDonemModel>();
        long toplam = tum.Sum(m => m.DbDerin?.DosyaBoyutu ?? m.TenantDetails?.DosyaBoyutu ?? 0);
        ToplamDepolamaMetni = FormatBoyut(toplam);
        int aktif = tum.Count(m => !m.KapaliMi);
        KayitliDbAltMetni = $"{aktif} Aktif Çalışma Dönemi";
        OrtalamaDepolamaMetni = tum.Count == 0 ? "Ortalama: —" : $"Ortalama: {FormatBoyut(toplam / tum.Count)}/DB";

        // Derin analiz yapılmış dönemleri sayarak dürüst KPI üret (analiz bitmeden %0 yalanı kapanır).
        int analizli = tum.Count(m => m.DbDerin != null);
        int analizliDurum = tum.Count(m => m.DbAnalizYapildi);
        int wal = tum.Count(m => string.Equals(m.DbDerin?.JournalModu, "wal", StringComparison.OrdinalIgnoreCase));
        WalModuMetni = analizli == 0 ? "—" : $" %{100 * wal / tum.Count} WAL Aktif";

        // Sağlıklı skoru: durum analizi yapılmamış dönemler hesaptan dışlanır.
        int saglikli = tum.Count(m => m.DbGuncelMi);
        if (analizliDurum == 0)
            ButunlukSkoruMetni = "Durum analizi bekleniyor";
        else if (tum.Count == 0)
            ButunlukSkoruMetni = "—";
        else
            ButunlukSkoruMetni = $" %{100 * saglikli / analizliDurum} Sağlıklı ({analizliDurum}/{tum.Count} analiz)";

        int hata = tum.Count(m => m.DbKontrolGerekliMi);
        ButunlukAltMetni = analizli == 0 ? "Derin analiz bekleniyor" : $"PRAGMA Doğrulandı ({hata} Hata)";
    }

    /// <summary>Tek dönem yedekle (tablo satırı). Satır yazımı çağıran yapar (RefreshAllAsync).</summary>
    public async Task<bool> YedekleAsync(MaliDonemModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.DatabaseName))
            return false;
        try
        {
            var response = await OperationService.CreateBackupAsync(model.DatabaseName, Domain.Enum.DatabaseEnum.DatabaseBackupType.Manual);
            bool ok = response.Success && response.Data != null && response.Data.IsBackupComleted;
            if (ok)
                await SatirVitrininiYazAsync(model, response.Data);
            NotificationService.ShowTagged(ok ? "Yedek Alındı" : "Yedek Alınamadı",
                ok ? $"{model.MaliYil} dönemi yedeklendi." : response.Message,
                ok ? NotificationType.Success : NotificationType.Warning,
                "Yedekle", NotificationGroups.Yedek);
            return ok;
        }
        catch (Exception ex)
        {
            NotificationService.ShowTagged("Yedek Hatası", ex.Message, NotificationType.Danger,
                "Yedekle", NotificationGroups.Yedek);
            return false;
        }
    }

    public async Task TopluYedekleAsync()
    {        var tum = MaliDonemList.ItemsSource?.Where(m => m != null && !m.DbDosyaYokMu).ToList() ?? new List<MaliDonemModel>();
        int ok = 0;
        foreach (var model in tum)
        {
            try
            {
                var response = await OperationService.CreateBackupAsync(model.DatabaseName, Domain.Enum.DatabaseEnum.DatabaseBackupType.Automatic);
                if (response.Success && response.Data != null && response.Data.IsBackupComleted)
                {
                    await SatirVitrininiYazAsync(model, response.Data);
                    ok++;
                }
            }
            catch { /* tek satır batch'i durdurmaz */ }
        }
        NotificationService.ShowTagged("Toplu Yedek", $"{ok}/{tum.Count} dönem yedeklendi.", ok == tum.Count ? NotificationType.Success : NotificationType.Warning,
            "TopluYedekle", NotificationGroups.DonemIslemleri);
    }

    /// <summary>Başarılı yedek sonrası Global.db satırına vitrin verisini işler (Son Yedek + Boyut).</summary>
    private async Task SatirVitrininiYazAsync(MaliDonemModel model, DatabaseBackupResult sonuc)
    {
        try
        {
            if (model == null || MaliDonemList?.MaliDonemService == null)
                return;
            model.TenantDetails ??= new TenantDetailsModel();
            model.TenantDetails.SonYedekTarihi = DateTime.Now;
            if (sonuc.BackupFileSizeBytes > 0)
                model.TenantDetails.DosyaBoyutu = sonuc.BackupFileSizeBytes;
            await MaliDonemList.MaliDonemService.UpdateMaliDonemAsync(model);
        }
        catch { /* vitrin yazımı yedek başarısını gölgelemez */ }
    }

    public async Task TopluTestAsync()    {
        var tum = MaliDonemList.ItemsSource?.Where(m => m != null).ToList() ?? new List<MaliDonemModel>();
        foreach (var model in tum)
        {
            try { await MaliDonemList.AnalyzeDbStatusAsync(model); }
            catch { /* devam */ }
        }
        HesaplaKpi();
        NotificationService.ShowTagged("Toplu Test", $"{tum.Count} dönem analiz edildi.", NotificationType.Success,
            "TopluTest", NotificationGroups.DonemIslemleri);
    }

    public async Task TopluBakimAsync()
    {
        var tum = MaliDonemList.ItemsSource?.Where(m => m != null && !m.DbDosyaYokMu).ToList() ?? new List<MaliDonemModel>();
        int ok = 0;
        foreach (var model in tum)
        {
            try
            {
                var v = await OperationService.BakimCalistirAsync(model.DatabaseName, "VACUUM");
                var r = await OperationService.BakimCalistirAsync(model.DatabaseName, "REINDEX");
                if (v.Success && r.Success)
                    ok++;
            }
            catch { /* devam */ }
        }
        NotificationService.ShowTagged("Toplu Bakım", $"{ok}/{tum.Count} dönemde VACUUM+REINDEX tamamlandı.", ok == tum.Count ? NotificationType.Success : NotificationType.Warning,
            "TopluBakim", NotificationGroups.DonemIslemleri);
    }

    private static string FormatBoyut(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        double kb = bytes / 1024d;
        if (kb < 1024) return $"{kb:0.#} KB";
        double mb = kb / 1024d;
        return $"{mb:0.#} MB";
    }
}
