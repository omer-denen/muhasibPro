using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Sistem.SistemKurulum;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Sistem;

public class SistemTestResult : ObservableObject
{
    public string TestAdi { get; set; } = string.Empty;
    public string Kategori { get; set; } = string.Empty;
    public bool BasariliMi { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public string Detay { get; set; } = string.Empty;
    public string DurumMetni => BasariliMi ? "Başarılı" : "Başarısız";
}

// Orkestratör: sadece composition + navigation + Logs — veritabanı oluşturma/silme/listelime alt VM'lerde
public class SistemKurulumViewModel : ViewModelBase
{
    private readonly SistemDatabaseStatusViewModel _status;
    private readonly SistemDatabaseCreationViewModel _creation;
    private readonly SistemDiagnosticsViewModel _diagnostics;
    private readonly KurulumKayitViewModel _kurulum;

    public SistemKurulumViewModel(
        ICommonServices commonServices,
        IApplicationPaths appPaths,
        ISistemDatabaseService sistemDatabaseService,
        ISistemDiagnosticsService diagnosticsService,
        IKurulumKayitService kurulumService,
        IMakineKimligiProvider makineProvider) : base(commonServices)
    {
        _status = new SistemDatabaseStatusViewModel(appPaths, sistemDatabaseService, diagnosticsService, commonServices);
        _creation = new SistemDatabaseCreationViewModel(sistemDatabaseService, commonServices);
        _diagnostics = new SistemDiagnosticsViewModel(appPaths, sistemDatabaseService, diagnosticsService, commonServices);
        _kurulum = new KurulumKayitViewModel(commonServices, kurulumService, makineProvider);

        Logs = new ObservableCollection<string>();
        Logs.CollectionChanged += (_, __) => NotifyPropertyChanged(nameof(LogsText));

        InitializeGlobalDbCommand = new RelayCommand(async () => await InitializeGlobalDbAsync());
        RepairDatabaseCommand = new RelayCommand(async () => await RepairDatabaseAsync());
        TestSystemClassesCommand = new RelayCommand(async () => await TestSystemClassesAsync());
        GoToLoginCommand = new RelayCommand(async () => await GoToLoginAsync());

        // Alt VM'lerin IsBusy değişimini orkestratörde yansıt
        _status.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(IsBusy)) { NotifyPropertyChanged(nameof(CanExecuteKurulum)); NotifyPropertyChanged(nameof(CanAnalyze)); } };
        _creation.PropertyChanged += (_, e) => { if (e.PropertyName is nameof(SistemDatabaseCreationViewModel.IsBusy) or nameof(SistemDatabaseCreationViewModel.IsProgressVisible)) { NotifyPropertyChanged(nameof(IsBusy)); NotifyPropertyChanged(nameof(IsProgressVisible)); NotifyPropertyChanged(nameof(ProgressValue)); } };
        _diagnostics.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(IsBusy)) NotifyPropertyChanged(nameof(IsBusy)); };
        _kurulum.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(IsBusy)) NotifyPropertyChanged(nameof(IsBusy)); };

        _ = LoadInitialStateAsync();
    }

    // UI binds to orchestrator — delegation to status/creation/diagnostics
    public ObservableCollection<string> Logs { get; }
    public string LogsText => string.Join(Environment.NewLine, Logs);
    public ObservableCollection<SistemTestResult> TestSonuclari => _diagnostics.TestSonuclari;

    private string _statusMessage = "Hazır - İlk kurulum bekleniyor";
    public string StatusMessage { get => _statusMessage; set => Set(ref _statusMessage, value); }

    // Splash yönlendirmesiyle gelindiğinde (DB yok) net ilk-kurulum durumu
    private bool _firstSetupMode;

    public void SetFirstSetupMode()
    {
        _firstSetupMode = true;
        UpdateStatusMessage();
        NotifyPropertyChanged(nameof(StatusMessage));
    }

    public string DbPath => _status.DbPath;
    public string DbSize => _status.DbSize;
    public string DbVersion => _status.DbVersion;
    public bool DbExists => _status.DbExists;
    public string DbExistsMetni => _status.DbExistsMetni;
    public bool IsDbValid => _status.IsDbValid;
    public string IsDbValidMetni => _status.IsDbValidMetni;
    public bool IsKurulumGerekli => _status.IsKurulumGerekli;
    public bool IsKurulumTamamlandi => _status.IsKurulumTamamlandi;
    public string DbDurumMetni => _status.DbDurumMetni;
    public string DbDurumAciklama => _status.DbDurumAciklama;
    public bool CanExecuteKurulum => !IsBusy && _status.CanExecuteKurulum;
    public bool CanAnalyze => !IsBusy && _status.CanAnalyze;
    public int TableCount => _status.TableCount;
    public string KullaniciSayisi => _status.KullaniciSayisi;
    public string FirmaSayisi => _status.FirmaSayisi;

    public KurulumKayitViewModel KurulumKayit => _kurulum;

    public double ProgressValue => _creation.ProgressValue;
    public bool IsProgressVisible => _creation.IsProgressVisible;

    public override bool IsBusy
    {
        get => base.IsBusy || _status.IsBusy || _creation.IsBusy || _diagnostics.IsBusy || _kurulum.IsBusy;
        set => base.IsBusy = value;
    }

    public ICommand InitializeGlobalDbCommand { get; }
    public ICommand RepairDatabaseCommand { get; }
    public ICommand TestSystemClassesCommand { get; }
    public ICommand GoToLoginCommand { get; }

    // DB var ama geçersiz — onarılabilir durumu
    public bool CanRepair => DbExists && !IsDbValid && !IsBusy;

    private async Task LoadInitialStateAsync()
    {
        try { await _status.LoadInitialStateAsync(AddLog); }
        catch (Exception ex) { AddLog($"[HATA] İlk durum yüklenemedi: {ex.Message}"); }
        try { await _kurulum.YukleAsync(); } catch { }
        UpdateStatusMessage();
        NotifyAll();
    }

    private async Task RefreshDbStateAsync()
    {
        await _status.RefreshAsync(AddLog);
        UpdateStatusMessage();
        NotifyAll();
    }

    private void UpdateStatusMessage()
    {
        if (IsBusy) return;
        if (DbExists && IsDbValid)
            StatusMessage = "Veritabanı hazır";
        else if (DbExists && !IsDbValid)
            StatusMessage = "Veritabanı hasarlı — onarım veya yeniden kurulum gerekiyor";
        else if (_firstSetupMode)
            StatusMessage = "Veritabanı bulunamadı — kurulum gerekiyor";
        else
            StatusMessage = "Hazır - İlk kurulum bekleniyor";
    }

    private void NotifyAll()
    {
        NotifyPropertyChanged(nameof(DbPath)); NotifyPropertyChanged(nameof(DbSize)); NotifyPropertyChanged(nameof(DbVersion));
        NotifyPropertyChanged(nameof(DbExists)); NotifyPropertyChanged(nameof(DbExistsMetni)); NotifyPropertyChanged(nameof(IsDbValid)); NotifyPropertyChanged(nameof(IsDbValidMetni));
        NotifyPropertyChanged(nameof(IsKurulumGerekli)); NotifyPropertyChanged(nameof(IsKurulumTamamlandi));
        NotifyPropertyChanged(nameof(DbDurumMetni)); NotifyPropertyChanged(nameof(DbDurumAciklama));
        NotifyPropertyChanged(nameof(CanExecuteKurulum)); NotifyPropertyChanged(nameof(CanAnalyze)); NotifyPropertyChanged(nameof(CanRepair));
        NotifyPropertyChanged(nameof(TableCount)); NotifyPropertyChanged(nameof(KullaniciSayisi)); NotifyPropertyChanged(nameof(FirmaSayisi));
        NotifyPropertyChanged(nameof(StatusMessage)); NotifyPropertyChanged(nameof(ProgressValue)); NotifyPropertyChanged(nameof(IsProgressVisible)); NotifyPropertyChanged(nameof(IsBusy));
    }

    public async Task InitializeGlobalDbAsync()
    {
        var (success, message) = await _creation.ExecuteAsync(AddLog, RefreshDbStateAsync, s => StatusMessage = s);
        TestSonuclari.Add(new SistemTestResult
        {
            Kategori = "Veritabanı",
            TestAdi = "İlk Kurulum",
            BasariliMi = success,
            Mesaj = message,
            Detay = $"Dosya: {(DbExists ? "Var" : "Yok")}, Doğrulama: {(IsDbValid ? "Geçerli" : "Geçersiz")}, Bileşen: {TableCount}"
        });
        if (success) await TestSystemClassesAsync();
        NotifyAll();
    }

    public async Task TestSystemClassesAsync()
    {
        await _diagnostics.RunAsync(AddLog, s => StatusMessage = s);
        NotifyPropertyChanged(nameof(TestSonuclari));
    }

    private async Task GoToLoginAsync()
    {
        await RefreshDbStateAsync();
        if (!DbExists || !IsDbValid)
        {
            var detail = !DbExists
                ? "Sistem veritabanı dosyası (Sistem.db) bulunamadı."
                : "Sistem veritabanı dosyası mevcut ancak hasarlı veya geçersiz.";
            AddLog($"[ENGEL] Veritabanı hazır değil — login'e gidilemez. {detail}");
            StatusMessage = "Önce Sistem Veritabanını kurmalısınız";
            await DialogService.ShowAsync(
                "Veritabanı hazır değil",
                $"{detail}\n\nLütfen 'Kurulumu Başlat' ile Global.db'yi oluşturun{(DbExists ? " veya 'Onar' ile mevcut veritabanını onarmayı deneyin" : "")}.");
            return;
        }
        NavigationService.Navigate<MuhasibPro.ViewModels.ViewModels.Shell.LoginViewModel>();
        await Task.CompletedTask;
    }

    /// <summary>
    /// DB var ama geçersiz — silip yeniden oluşturmayı dener (repair).
    /// Kullanıcı onayı ister çünkü mevcut veri kaybolabilir.
    /// </summary>
    private async Task RepairDatabaseAsync()
    {
        if (!DbExists)
        {
            AddLog("[BİLGİ] Veritabanı dosyası yok — onarım yerine kurulum başlatın.");
            return;
        }
        var confirmed = await DialogService.ShowAsync(
            "Veritabanı Onarımı",
            "Mevcut Sistem.db dosyası hasarlı görünüyor. Onarım, veritabanını silip sıfırdan oluşturacak.\n\n⚠️ Mevcut kullanıcı ve firma kayıtları kaybolabilir.\n\nDevam etmek istiyor musunuz?",
            "Onar ve Yeniden Oluştur",
            "İptal");
        if (!confirmed)
        {
            AddLog("[İPTAL] Onarım kullanıcı tarafından iptal edildi.");
            return;
        }
        AddLog("[ONARIM] Hasarlı veritabanı siliniyor...");
        try
        {
            var dbPath = _status.DbPath;
            if (System.IO.File.Exists(dbPath))
            {
                System.IO.File.Delete(dbPath);
                // WAL ve SHM dosyalarını da temizle
                var walPath = dbPath + "-wal";
                var shmPath = dbPath + "-shm";
                if (System.IO.File.Exists(walPath)) System.IO.File.Delete(walPath);
                if (System.IO.File.Exists(shmPath)) System.IO.File.Delete(shmPath);
                AddLog("[ONARIM] Eski dosyalar silindi.");
            }
        }
        catch (Exception ex)
        {
            AddLog($"[HATA] Dosya silinemedi: {ex.Message}");
            StatusMessage = $"Onarım başarısız: {ex.Message}";
            NotifyAll();
            return;
        }
        // Yeniden oluştur
        await InitializeGlobalDbAsync();
    }

    private void AddLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        ContextService.RunAsync(() =>
        {
            Logs.Add(line);
            NotifyPropertyChanged(nameof(LogsText));
        });
    }
}
