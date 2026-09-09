using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Sistem;
using System.Collections.ObjectModel;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.SistemKurulum;

// Tek sorumluluk: Sistem sınıfları testleri (7 test) + TestSonuclari
public class SistemDiagnosticsViewModel : ViewModelBase
{
    private readonly IApplicationPaths _appPaths;
    private readonly ISistemDatabaseService _sistemDatabaseService;
    private readonly ISistemDiagnosticsService _diagnosticsService;

    public SistemDiagnosticsViewModel(IApplicationPaths appPaths, ISistemDatabaseService sistemDatabaseService, ISistemDiagnosticsService diagnosticsService, Business.Contracts.UIServices.CommonServices.ICommonServices commonServices) : base(commonServices)
    {
        _appPaths = appPaths;
        _sistemDatabaseService = sistemDatabaseService;
        _diagnosticsService = diagnosticsService;
    }

    public ObservableCollection<SistemTestResult> TestSonuclari { get; } = new();

    public async Task RunAsync(Action<string> log, Action<string> setStatus)
    {
        if (IsBusy) return;
        IsBusy = true;
        setStatus("Sistem sınıfları test ediliyor...");
        log("═══════════════════════════════════════");
        log("[TEST] Sistem veritabanı sınıfları test başlıyor...");

        var tests = new List<SistemTestResult>
        {
            await TestApplicationPathsAsync(),
            await TestDbContextAsync(),
            await TestMigrationManagerStateAsync(),
            await TestPendingMigrationsAsync(),
            await TestFullDiagAsync(),
            await TestFirmaMaliDonemAsync(),
            await TestKullaniciFirmaRolAsync()
        };

        TestSonuclari.Clear();
        foreach (var t in tests)
        {
            TestSonuclari.Add(t);
            log($"[{(t.BasariliMi ? "PASS" : "FAIL")}] {t.Kategori}/{t.TestAdi}: {t.Mesaj}");
            if (!string.IsNullOrEmpty(t.Detay)) log($"       ↳ {t.Detay}");
        }

        var passCount = tests.Count(t => t.BasariliMi);
        setStatus($"Test tamamlandı: {passCount}/{tests.Count} başarılı");
        log($"[ÖZET] {passCount}/{tests.Count} test geçti");
        IsBusy = false;
    }

    private Task<SistemTestResult> TestApplicationPathsAsync()
    {
        try
        {
            var path = _appPaths.GetSistemDatabaseFilePath();
            var exists = _appPaths.SistemDatabaseFileExists();
            var valid = _appPaths.IsSistemDatabaseValid();
            var size = _appPaths.GetSistemDatabaseSize();
            var dir = System.IO.Path.GetDirectoryName(path);
            var dirExists = System.IO.Directory.Exists(dir);
            return Task.FromResult(new SistemTestResult
            {
                Kategori = "Dosya",
                TestAdi = "Konum ve Erişim",
                BasariliMi = !string.IsNullOrEmpty(path) && dirExists,
                Mesaj = exists ? $"Veritabanı dosyası hazır ({size / 1024.0:F1} KB, Doğrulama: {(valid ? "Geçerli" : "Geçersiz")})" : "Veritabanı henüz oluşturulmadı — kurulum gerekli",
                Detay = $"Konum: {path}"
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new SistemTestResult { Kategori = "Dosya", TestAdi = "Konum ve Erişim", BasariliMi = false, Mesaj = "Dosya kontrolü başarısız: " + ex.Message, Detay = ex.ToString() });
        }
    }

    private async Task<SistemTestResult> TestDbContextAsync()
    {
        try
        {
            var canConnect = await _diagnosticsService.CanConnectAsync();
            var kullaniciCount = await _diagnosticsService.GetKullaniciCountAsync();
            return new SistemTestResult
            {
                Kategori = "Bağlantı",
                TestAdi = "Veritabanına Erişim",
                BasariliMi = canConnect,
                Mesaj = canConnect ? $"Bağlantı başarılı — {kullaniciCount} kullanıcı kaydı bulundu" : "Veritabanına bağlanılamadı, lütfen kurulumu tekrar deneyin",
                Detay = $"Erişim: {(canConnect ? "Başarılı" : "Başarısız")}, Kullanıcı: {kullaniciCount}"
            };
        }
        catch (Exception ex)
        {
            return new SistemTestResult { Kategori = "Bağlantı", TestAdi = "Veritabanına Erişim", BasariliMi = false, Mesaj = "Bağlantı hatası: " + ex.Message, Detay = ex.InnerException?.Message ?? "" };
        }
    }

    private async Task<SistemTestResult> TestMigrationManagerStateAsync()
    {
        try
        {
            var response = await _sistemDatabaseService.GetSistemDatabaseStateAsync();
            var state = response?.Data;
            // Not: ApiDataResponse.Success her durumda true döner ki model verisi kaybolmasın; karar model alanlarına göre verilir
            var basarili = state != null && state.IsDatabaseExists && state.CanConnect && !state.HasError && state.DatabaseValid;
            return new SistemTestResult
            {
                Kategori = "ISistemDatabaseService",
                TestAdi = "GetSistemDatabaseStateAsync",
                BasariliMi = basarili,
                Mesaj = state == null ? response?.Message ?? "Yanıt boş" : $"Tables={state.TableCount}, Valid={state.DatabaseValid}, CanConnect={state.CanConnect}, HasError={state.HasError}",
                Detay = state == null ? "" : $"Pending={state.PendingMigrations?.Count ?? 0}, Version={state.CurrentVersion}, Status={state.GetStatus()}"
            };
        }
        catch (Exception ex)
        {
            return new SistemTestResult { Kategori = "ISistemDatabaseService", TestAdi = "GetSistemDatabaseStateAsync", BasariliMi = false, Mesaj = ex.Message };
        }
    }

    private async Task<SistemTestResult> TestPendingMigrationsAsync()
    {
        try
        {
            var pending = await _sistemDatabaseService.GetPendingMigrationsAsync();
            return new SistemTestResult
            {
                Kategori = "ISistemDatabaseService",
                TestAdi = "GetPendingMigrationsAsync",
                BasariliMi = true,
                Mesaj = pending.Count == 0 ? "Bekleyen migration yok (güncel)" : $"{pending.Count} bekleyen var",
                Detay = string.Join(", ", pending.Take(3))
            };
        }
        catch (Exception ex) { return new SistemTestResult { Kategori = "ISistemDatabaseService", TestAdi = "GetPendingMigrationsAsync", BasariliMi = false, Mesaj = ex.Message }; }
    }

    private async Task<SistemTestResult> TestFullDiagAsync()
    {
        try
        {
            var progress = new Progress<AnalysisProgress>();
            var diag = await _sistemDatabaseService.GetSistemDatabaseFullDiagStateAsync(progress, null);
            return new SistemTestResult
            {
                Kategori = "ISistemDatabaseService",
                TestAdi = "GetSistemDatabaseFullDiagStateAsync",
                BasariliMi = !diag.HasError && diag.IsDatabaseExists,
                Mesaj = diag.HasError ? $"Hata: {diag.Message}" : $"Diag OK, Tables={diag.TableCount}",
                Detay = $"HasError={diag.HasError}, IsEmpty={diag.IsEmptyDatabase}"
            };
        }
        catch (Exception ex) { return new SistemTestResult { Kategori = "ISistemDatabaseService", TestAdi = "FullDiag", BasariliMi = false, Mesaj = ex.Message }; }
    }

    private async Task<SistemTestResult> TestFirmaMaliDonemAsync()
    {
        try
        {
            var (firmaCount, donemCount, ilkFirma) = await _diagnosticsService.GetFirmaMaliDonemStatsAsync();
            return new SistemTestResult
            {
                Kategori = "Sistem Veri",
                TestAdi = "Firma/MaliDonem",
                BasariliMi = true,
                Mesaj = $"Firma={firmaCount}, Dönem={donemCount}",
                Detay = !string.IsNullOrEmpty(ilkFirma) ? $"İlk firma: {ilkFirma}" : "Firma yok"
            };
        }
        catch (Exception ex) { return new SistemTestResult { Kategori = "Sistem Veri", TestAdi = "Firma/MaliDonem", BasariliMi = false, Mesaj = ex.Message }; }
    }

    private async Task<SistemTestResult> TestKullaniciFirmaRolAsync()
    {
        try
        {
            var (kfrCount, permCount, sample) = await _diagnosticsService.GetKullaniciFirmaRolStatsAsync();
            return new SistemTestResult
            {
                Kategori = "Sistem Yetki",
                TestAdi = "KullaniciFirmaRol/RolPermission",
                BasariliMi = true,
                Mesaj = $"KFR={kfrCount}, RolPermission={permCount}",
                Detay = !string.IsNullOrEmpty(sample) ? $"Örnek: {sample}" : "Kayıt yok"
            };
        }
        catch (Exception ex) { return new SistemTestResult { Kategori = "Sistem Yetki", TestAdi = "KullaniciFirmaRol", BasariliMi = false, Mesaj = ex.Message }; }
    }
}
