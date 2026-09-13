using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

// Tek sorumluluk: bekleyen sistem göçlerini listele + yedek-önce-göç ile uygula.
public class SistemGuncellemeViewModel : ViewModelBase
{
    private readonly ISistemDatabaseService _sistemDatabaseService;

    /// <summary>Sayfa işlem günlüğüne yazar (orkestratör bağlar).</summary>
    public Action<string>? LogEkle;

    public SistemGuncellemeViewModel(
        ICommonServices commonServices,
        ISistemDatabaseService sistemDatabaseService) : base(commonServices)
    {
        _sistemDatabaseService = sistemDatabaseService;
        BekleyenGocler = new ObservableCollection<string>();
        GuncelleCommand = new RelayCommand(async () => await GuncelleAsync());
        YenileCommand = new RelayCommand(async () => await YukleAsync());
    }

    public ObservableCollection<string> BekleyenGocler { get; }

    public bool GocVarMi => BekleyenGocler.Count > 0;

    private string _adimMetni = string.Empty;
    public string AdimMetni { get => _adimMetni; set => Set(ref _adimMetni, value); }

    private double _progressValue;
    public double ProgressValue { get => _progressValue; set => Set(ref _progressValue, value); }

    private bool _isProgressVisible;
    public bool IsProgressVisible { get => _isProgressVisible; set => Set(ref _isProgressVisible, value); }

    private string _durumMetni = string.Empty;
    public string DurumMetni { get => _durumMetni; set => Set(ref _durumMetni, value); }

    private bool _isHata;
    public bool IsHata { get => _isHata; set => Set(ref _isHata, value); }

    public ICommand GuncelleCommand { get; }
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
        try
        {
            var liste = await _sistemDatabaseService.GetPendingMigrationsAsync();
            BekleyenGocler.Clear();
            foreach (var g in liste)
                BekleyenGocler.Add(g);
            NotifyPropertyChanged(nameof(GocVarMi));
            DurumMetni = GocVarMi ? $"{BekleyenGocler.Count} bekleyen güncelleme var" : "Sistem güncel";
        }
        catch (Exception ex)
        {
            DurumMetni = $"Liste alınamadı: {ex.Message}";
            IsHata = true;
        }
    }

    public async Task GuncelleAsync()
    {
        if (IsBusy || !GocVarMi) return;
        bool onay = await DialogService.ShowAsync(
            "Sistem Güncellemesi",
            $"{BekleyenGocler.Count} bekleyen güncelleme uygulanacak.\n\nÖnce manuel yedek alınır, göç uygulanır, sonuç doğrulanır. Devam edilsin mi?",
            "Güncelle",
            "Vazgeç");
        if (!onay) return;
        IsBusy = true;
        IsProgressVisible = true;
        IsHata = false;
        try
        {
            AdimMetni = "1/3 Yedek alınıyor...";
            ProgressValue = 20;
            LogEkle?.Invoke("[GÜNCELLE] Bekleyen sistem göçleri uygulanıyor...");
            AdimMetni = "2/3 Göç uygulanıyor...";
            ProgressValue = 55;
            var (ok, mesaj) = await _sistemDatabaseService.ApplyPendingSistemMigrationsAsync();
            ProgressValue = 90;
            AdimMetni = "3/3 Doğrulanıyor...";
            await YukleIcAsync();
            DurumMetni = mesaj;
            IsHata = !ok;
            LogEkle?.Invoke(ok ? $"[GÜNCELLE] Başarılı: {mesaj}" : $"[HATA] Güncelleme: {mesaj}");
            ProgressValue = 100;
            AdimMetni = string.Empty;
        }
        catch (Exception ex)
        {
            DurumMetni = $"Hata: {ex.Message}";
            IsHata = true;
            LogEkle?.Invoke($"[HATA] Güncelleme: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsProgressVisible = false;
        }
    }
}
