using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

// Tek sorumluluk: Global.db dosyası var mı / geçerli mi / boyut / versiyon / tablo sayısı
public class SistemDatabaseStatusViewModel : ViewModelBase
{
    private readonly IApplicationPaths _appPaths;
    private readonly ISistemDatabaseService _sistemDatabaseService;
    private readonly ISistemDiagnosticsService _diagnosticsService;

    public SistemDatabaseStatusViewModel(
        IApplicationPaths appPaths,
        ISistemDatabaseService sistemDatabaseService,
        ISistemDiagnosticsService diagnosticsService,
        Business.Contracts.UIServices.CommonServices.ICommonServices commonServices) : base(commonServices)
    {
        _appPaths = appPaths;
        _sistemDatabaseService = sistemDatabaseService;
        _diagnosticsService = diagnosticsService;
    }

    private string _dbPath = string.Empty;
    public string DbPath { get => _dbPath; set => Set(ref _dbPath, value); }

    private string _dbSize = "-";
    public string DbSize { get => _dbSize; set => Set(ref _dbSize, value); }

    private string _dbVersion = "-";
    public string DbVersion { get => _dbVersion; set { if (Set(ref _dbVersion, value)) { NotifyPropertyChanged(nameof(DbDurumAciklama)); NotifyPropertyChanged(nameof(IsKurulumTamamlandi)); NotifyPropertyChanged(nameof(CanExecuteKurulum)); } } }

    private bool _dbExists;
    public bool DbExists { get => _dbExists; set { if (Set(ref _dbExists, value)) { NotifyPropertyChanged(nameof(DbExistsMetni)); NotifyPropertyChanged(nameof(IsKurulumGerekli)); NotifyPropertyChanged(nameof(IsKurulumTamamlandi)); NotifyPropertyChanged(nameof(CanExecuteKurulum)); NotifyPropertyChanged(nameof(CanAnalyze)); NotifyPropertyChanged(nameof(DbDurumMetni)); NotifyPropertyChanged(nameof(DbDurumAciklama)); } } }
    public string DbExistsMetni => DbExists ? "Mevcut" : "Bulunamadı";

    private bool _isDbValid;
    public bool IsDbValid { get => _isDbValid; set { if (Set(ref _isDbValid, value)) { NotifyPropertyChanged(nameof(IsDbValidMetni)); NotifyPropertyChanged(nameof(IsKurulumGerekli)); NotifyPropertyChanged(nameof(IsKurulumTamamlandi)); NotifyPropertyChanged(nameof(CanExecuteKurulum)); NotifyPropertyChanged(nameof(CanAnalyze)); NotifyPropertyChanged(nameof(DbDurumMetni)); NotifyPropertyChanged(nameof(DbDurumAciklama)); } } }
    public string IsDbValidMetni => IsDbValid ? "Geçerli" : "Geçersiz";

    public bool IsKurulumGerekli => !DbExists || !IsDbValid;
    public bool IsKurulumTamamlandi => DbExists && IsDbValid;

    public string DbDurumMetni => IsKurulumTamamlandi ? "Güncel" : (DbExists && !IsDbValid) ? "Hasarlı" : "Kurulum Gerekli";
    public string DbDurumAciklama => IsKurulumTamamlandi
        ? $"v{DbVersion} • {TableCount} bileşen hazır"
        : (DbExists && !IsDbValid)
            ? "Veritabanı dosyası mevcut ancak hasarlı — onarım veya yeniden kurulum gerekiyor"
            : "Veritabanı dosyası bulunamadı";

    public bool CanExecuteKurulum => !IsBusy && !IsKurulumTamamlandi;
    public bool CanAnalyze => !IsBusy && IsDbValid;

    private int _tableCount;
    public int TableCount { get => _tableCount; set { if (Set(ref _tableCount, value)) { NotifyPropertyChanged(nameof(DbDurumAciklama)); NotifyPropertyChanged(nameof(IsKurulumTamamlandi)); NotifyPropertyChanged(nameof(CanExecuteKurulum)); } } }

    private string _kullaniciSayisi = "-";
    public string KullaniciSayisi { get => _kullaniciSayisi; set => Set(ref _kullaniciSayisi, value); }

    private string _firmaSayisi = "-";
    public string FirmaSayisi { get => _firmaSayisi; set => Set(ref _firmaSayisi, value); }

    public override bool IsBusy { get => base.IsBusy; set { base.IsBusy = value; NotifyPropertyChanged(nameof(CanExecuteKurulum)); NotifyPropertyChanged(nameof(CanAnalyze)); } }

    public async Task LoadInitialStateAsync(Action<string> log)
    {
        DbPath = _appPaths.GetSistemDatabaseFilePath();
        log($"[BİLGİ] Veritabanı konumu: {DbPath}");
        await RefreshAsync(log);
    }

    public async Task RefreshAsync(Action<string> log)
    {
        DbExists = _appPaths.SistemDatabaseFileExists();
        IsDbValid = _appPaths.IsSistemDatabaseValid();
        var size = _appPaths.GetSistemDatabaseSize();
        DbSize = size > 0 ? $"{size / 1024.0:F1} KB" : "0 KB";
        log($"[DURUM] Veritabanı dosyası: {(DbExists ? "Mevcut" : "Bulunamadı")}, Doğrulama: {(IsDbValid ? "Geçerli" : "Geçersiz")}, Boyut: {DbSize}");

        if (DbExists)
        {
            var stateResponse = await _sistemDatabaseService.GetSistemDatabaseStateAsync();
            var state = stateResponse?.Data ?? throw new InvalidOperationException(stateResponse?.Message ?? "Sistem veritabanı durumu alınamadı");
            TableCount = state.TableCount;
            DbVersion = state.CurrentVersion ?? "-";
            log($"[ANALİZ] Hazır bileşen: {TableCount}, Sürüm: {DbVersion}");

            try
            {
                var kullaniciCount = await _diagnosticsService.GetKullaniciCountAsync();
                var firmaCount = await _diagnosticsService.GetFirmaCountAsync();
                KullaniciSayisi = kullaniciCount.ToString();
                FirmaSayisi = firmaCount.ToString();
            }
            catch
            {
                KullaniciSayisi = "-";
                FirmaSayisi = "-";
            }
            log($"[VERİ] Kullanıcı sayısı: {KullaniciSayisi}, Firma sayısı: {FirmaSayisi}");
        }
        else
        {
            TableCount = 0;
            DbVersion = "-";
            KullaniciSayisi = "-";
            FirmaSayisi = "-";
            log("[BİLGİ] Veritabanı henüz oluşturulmadı — 'Kurulumu Başlat' ile tek adımda hazırlanacak.");
        }
    }
}
