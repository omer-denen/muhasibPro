using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

// Tek sorumluluk: Global.db oluşturma / güncelleme saga (InitializeSistemDatabaseAsync) + progress
public class SistemDatabaseCreationViewModel : ViewModelBase
{
    private readonly ISistemDatabaseService _sistemDatabaseService;

    public SistemDatabaseCreationViewModel(ISistemDatabaseService sistemDatabaseService, Business.Contracts.UIServices.CommonServices.ICommonServices commonServices) : base(commonServices)
    {
        _sistemDatabaseService = sistemDatabaseService;
    }

    private double _progressValue;
    public double ProgressValue { get => _progressValue; set => Set(ref _progressValue, value); }

    private bool _isProgressVisible;
    public bool IsProgressVisible { get => _isProgressVisible; set => Set(ref _isProgressVisible, value); }

    public async Task<(bool success, string message)> ExecuteAsync(Action<string> log, Func<Task> refreshStatus, Action<string> setStatus)
    {
        if (IsBusy) return (false, "Meşgul");
        IsBusy = true;
        IsProgressVisible = true;
        ProgressValue = 0;
        setStatus("Sistem veritabanı hazırlanıyor, lütfen bekleyin...");
        log("═══════════════════════════════════════");
        log("[BAŞLAT] Veritabanı kurulumu başlatılıyor...");

        try
        {
            ProgressValue = 20;
            var progress = new Progress<AnalysisProgress>(p =>
            {
                ContextService.RunAsync(() =>
                {
                    log($"[PROGRESS {p.Percentage}%] {p.Message}");
                    ProgressValue = p.Percentage;
                });
            });

            ProgressValue = 40;
            var (success, message) = await _sistemDatabaseService.InitializeSistemDatabaseAsync();
            ProgressValue = 90;
            log($"[SONUÇ] Başarı: {success}");
            log($"[MESAJ] {message}");

            await refreshStatus();
            ProgressValue = 100;
            setStatus(success ? "✓ Veritabanı hazır — Girişe geçebilirsiniz" : $"✗ Kurulum başarısız: {message}");

            return (success, message);
        }
        catch (Exception ex)
        {
            log($"[EXCEPTION] {ex.GetType().Name}: {ex.Message}");
            setStatus($"Hata: {ex.Message}");
            return (false, ex.Message);
        }
        finally
        {
            IsBusy = false;
            IsProgressVisible = false;
        }
    }
}
