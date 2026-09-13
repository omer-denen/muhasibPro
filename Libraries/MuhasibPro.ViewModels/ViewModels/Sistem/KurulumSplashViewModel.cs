using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem;

/// <summary>
/// Ilk kurulum splash ekrani: DB yok senaryosunda otomatik DB olusturma + seed + dogrulama.
/// Sayfa yuklenince baslar, basarili olunca Login'e gecer, basarisiz olunca hata gosterir.
/// </summary>
public class KurulumSplashViewModel : ViewModelBase
{
    private readonly ISistemDatabaseService _sistemDatabaseService;

    public KurulumSplashViewModel(
        ISistemDatabaseService sistemDatabaseService,
        ICommonServices commonServices) : base(commonServices)
    {
        _sistemDatabaseService = sistemDatabaseService;
    }

    private double _progressValue;
    public double ProgressValue
    {
        get => _progressValue;
        set => Set(ref _progressValue, value);
    }

    private string _statusMessage = "Hazirlanıyor...";
    public string StatusMessage
    {
        get => _statusMessage;
        set => Set(ref _statusMessage, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => Set(ref _hasError, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => Set(ref _errorMessage, value);
    }

    /// <summary>
    /// Sayfa yuklenince cagrilir. DB olusturma islemini baslatir.
    /// Basarili olunca Login'e navigate eder.
    /// </summary>
    public async Task<bool> RunSetupAsync()
    {
        if (IsBusy) return false;
        IsBusy = true;
        HasError = false;
        ProgressValue = 0;

        try
        {
            StatusMessage = "Sistem veritabani olusturuluyor...";
            ProgressValue = 10;

            await Task.Delay(200); // UI guncellemesi icin kisa bekleme

            StatusMessage = "Veritabani dosyasi hazirlaniyor...";
            ProgressValue = 30;

            var (success, message) = await _sistemDatabaseService.InitializeSistemDatabaseAsync();

            ProgressValue = 80;

            if (success)
            {
                StatusMessage = "Veritabani dogrulaması yapılıyor...";
                ProgressValue = 90;
                await Task.Delay(300);

                StatusMessage = "Kurulum tamamlandi";
                ProgressValue = 100;
                return true;
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
                StatusMessage = "Kurulum basarisiz";
                return false;
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "Beklenmeyen hata";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
