using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

// Tek sorumluluk: Sistem.db yedek listesi + al + geri-yükle (operasyon servisi üzerinden, Kural 5).
public class SistemYedekViewModel : ViewModelBase
{
    private readonly ISistemDatabaseOperationService _operasyon;
    private readonly IDatabaseSettingsProvider _ayarlar;
    private readonly ISistemRestoreAnalizService _analiz;

    /// <summary>Sayfa işlem günlüğüne yazar (orkestratör bağlar; dialogda boş kalır).</summary>
    public Action<string>? LogEkle;

    public SistemYedekViewModel(
        ICommonServices commonServices,
        ISistemDatabaseOperationService operasyon,
        IDatabaseSettingsProvider ayarlar,
        ISistemRestoreAnalizService analiz) : base(commonServices)
    {
        _operasyon = operasyon;
        _ayarlar = ayarlar;
        _analiz = analiz;
        Yedekler = new ObservableCollection<DatabaseBackupResult>();
        YedekAlCommand = new RelayCommand(async () => await YedekAlAsync());
        YenileCommand = new RelayCommand(async () => await YukleAsync());
    }

    public ObservableCollection<DatabaseBackupResult> Yedekler { get; }

    private bool _yedekListesiBos = true;
    public bool YedekListesiBos { get => _yedekListesiBos; set => Set(ref _yedekListesiBos, value); }

    private string _sonYedekMetni = "Son yedek: —";
    public string SonYedekMetni { get => _sonYedekMetni; set => Set(ref _sonYedekMetni, value); }

    private string _saklamaMetni = string.Empty;
    public string SaklamaMetni { get => _saklamaMetni; set => Set(ref _saklamaMetni, value); }

    private string _durumMetni = string.Empty;
    public string DurumMetni { get => _durumMetni; set => Set(ref _durumMetni, value); }

    private bool _isHata;
    public bool IsHata { get => _isHata; set => Set(ref _isHata, value); }

    public ICommand YedekAlCommand { get; }
    public ICommand YenileCommand { get; }

    public async Task YukleAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await YukleIcAsync(); }
        finally { IsBusy = false; }
    }

    private async Task YukleIcAsync()
    {
        IsHata = false;
        DurumMetni = "Yükleniyor...";
        string bilgi = string.Empty;
        try
        {
            var ayar = await _ayarlar.GetAsync();
            int keep = ayar.GetSistemKeep();
            SaklamaMetni = $"En fazla {keep} manuel yedek saklanır";
            var resp = await _operasyon.GetBackupHistoryAsync();
            var liste = (resp.Data ?? new List<DatabaseBackupResult>()).ToList();
            // Limit-uygulama: liste yalnızca okumazdı (5 varken 3 yazardı) — yüklemede de budanır (Kural 7: keep modelden).
            if (liste.Count > keep)
            {
                var temizlik = await _operasyon.CleanOldBackupsAsync(keep);
                if (temizlik.Success && temizlik.Data > 0)
                {
                    resp = await _operasyon.GetBackupHistoryAsync();
                    liste = (resp.Data ?? new List<DatabaseBackupResult>()).ToList();
                    bilgi = $"Limit ({keep}) aşıldığı için {temizlik.Data} eski yedek temizlendi.";
                    LogEkle?.Invoke($"[YEDEK] Limit-uygulama: {temizlik.Data} eski yedek silindi (keep={keep}).");
                }
                else
                {
                    bilgi = $"Limit ({keep}) aşıldı ({liste.Count} yedek) ama temizlik yapılamadı: {temizlik.Message}";
                    IsHata = true;
                    LogEkle?.Invoke($"[HATA] Limit-uygulama başarısız: {temizlik.Message}");
                }
            }
            Yedekler.Clear();
            foreach (var y in liste)
                Yedekler.Add(y);
            YedekListesiBos = Yedekler.Count == 0;
            var son = _operasyon.GetLastBackupDate();
            SonYedekMetni = son.HasValue ? $"Son yedek: {son:yyyy-MM-dd HH:mm} • Toplam: {Yedekler.Count}" : $"Yedek yok • Toplam: {Yedekler.Count}";
            DurumMetni = bilgi;
        }
        catch (Exception ex)
        {
            DurumMetni = $"Liste alınamadı: {ex.Message}";
            IsHata = true;
        }
    }

    public async Task YedekAlAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        IsHata = false;
        DurumMetni = "Yedek alınıyor...";
        LogEkle?.Invoke("[YEDEK] Manuel sistem yedeği alınıyor...");
        try
        {
            var resp = await _operasyon.CreateBackupAsync(DatabaseBackupType.Manual);
            if (!resp.Success)
            {
                DurumMetni = resp.Message;
                IsHata = true;
                LogEkle?.Invoke($"[HATA] Yedek alınamadı: {resp.Message}");
                return;
            }
            var ayar = await _ayarlar.GetAsync();
            int keep = ayar.GetSistemKeep();
            var temizlik = await _operasyon.CleanOldBackupsAsync(keep);
            await YukleIcAsync();
            if (temizlik.Success)
            {
                IsHata = false;
                DurumMetni = temizlik.Data > 0
                    ? $"Yedek alındı, {temizlik.Data} eski yedek temizlendi (en fazla {keep} saklanır)"
                    : $"Yedek alındı (en fazla {keep} saklanır)";
            }
            else
            {
                DurumMetni = $"Yedek alındı ama eski yedek temizliği yapılamadı: {temizlik.Message}";
                IsHata = true;
            }
            LogEkle?.Invoke($"[YEDEK] Alındı: {resp.Data?.BackupFileName}");
        }
        catch (Exception ex)
        {
            DurumMetni = $"Hata: {ex.Message}";
            IsHata = true;
            LogEkle?.Invoke($"[HATA] Yedek: {ex.Message}");
        }
        finally { IsBusy = false; }
    }

    /// <summary>
    /// Faz 6.78 Adım 3 — tek kapı: önce analiz (dosya + mevcut↔yedek farkı + hüküm), sonra hüküm dialogu.
    /// Block'ta geri yükleme yapılmaz; RequireCode'da dialog tek-seferlik kodu doğrular.
    /// </summary>
    public async Task GeriYukleAsync(DatabaseBackupResult? yedek)
    {
        if (IsBusy || yedek == null) return;
        IsBusy = true;
        IsHata = false;
        DurumMetni = "Yedek analiz ediliyor...";
        LogEkle?.Invoke($"[ANALİZ] {yedek.BackupFileName} inceleniyor...");
        try
        {
            var analiz = await _analiz.AnalizEtAsync(yedek.BackupFileName);
            LogEkle?.Invoke($"[ANALİZ] Hüküm: {analiz.Hukum.Baslik} ({analiz.Hukum.Kind})");
            if (analiz.Fark.KayipVarMi)
                LogEkle?.Invoke($"[ANALİZ] {analiz.FarkOzeti}: {string.Join("; ", analiz.Fark.KayipKayitlar.Concat(analiz.Fark.KayipTenantDosyalari).Take(5))}");

            bool onay = await DialogService.ShowSistemRestoreVerifyAsync(analiz);
            if (!onay || analiz.Hukum.Engelli)
            {
                DurumMetni = analiz.Hukum.Engelli
                    ? analiz.Hukum.Aciklama
                    : "Geri yükleme iptal edildi.";
                IsHata = analiz.Hukum.Engelli;
                LogEkle?.Invoke(analiz.Hukum.Engelli
                    ? $"[ENGELLENDİ] {analiz.Hukum.Baslik}: {analiz.Hukum.Aciklama}"
                    : "[İPTAL] Kullanıcı geri yüklemeyi durdurdu.");
                return;
            }

            DurumMetni = "Geri yükleniyor...";
            LogEkle?.Invoke($"[GERİ-YÜKLE] {yedek.BackupFileName} geri yükleniyor...");
            var resp = await _operasyon.RestoreBackupAsync(yedek.BackupFileName);
            await YukleIcAsync();
            DurumMetni = resp.Message;
            IsHata = !resp.Success;
            LogEkle?.Invoke(resp.Success ? "[GERİ-YÜKLE] Başarılı." : $"[HATA] Geri yükleme: {resp.Message}");
        }
        catch (Exception ex)
        {
            DurumMetni = $"Hata: {ex.Message}";
            IsHata = true;
            LogEkle?.Invoke($"[HATA] Geri yükleme: {ex.Message}");
        }
        finally { IsBusy = false; }
    }
}
