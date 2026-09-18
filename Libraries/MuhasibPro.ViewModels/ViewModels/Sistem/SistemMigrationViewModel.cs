using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem;

/// <summary>
/// DEV-ONLY: açılışta bekleyen Sistem.db şema göçlerini otomatik uygular.
/// Güncelleme sonrası doğrulama sagasının dev karşılığıdır (<c>PostUpdatePending</c> bayrağı dev'de
/// EF migration eklerken oluşmaz; bu yüzden saga dev'i tetiklemez).
/// Motor: <see cref="ISistemDatabaseService.ApplyPendingSistemMigrationsAsync"/> (ön-yedek → göç → doğrulama).
/// </summary>
public class SistemMigrationViewModel : ViewModelBase
{
    private readonly ISistemDatabaseService _sistemDatabaseService;

    public SistemMigrationViewModel(
        ISistemDatabaseService sistemDatabaseService,
        ICommonServices commonServices) : base(commonServices)
    {
        _sistemDatabaseService = sistemDatabaseService;
    }

    private string _statusMessage = "Bekleyen şema güncellemeleri denetleniyor...";
    public string StatusMessage
    {
        get => _statusMessage;
        set => Set(ref _statusMessage, value);
    }

    private int _bekleyenSayisi;
    public int BekleyenSayisi
    {
        get => _bekleyenSayisi;
        set => Set(ref _bekleyenSayisi, value);
    }

    private bool _basarili;
    public bool Basarili
    {
        get => _basarili;
        set { if (Set(ref _basarili, value)) NotifyPropertyChanged(nameof(SonucHazir)); }
    }

    private bool _hataVar;
    public bool HataVar
    {
        get => _hataVar;
        set { if (Set(ref _hataVar, value)) NotifyPropertyChanged(nameof(SonucHazir)); }
    }

    private string _hataMetni = string.Empty;
    public string HataMetni
    {
        get => _hataMetni;
        set => Set(ref _hataMetni, value);
    }

    /// <summary>Sonuç yazıldı mı (başarı veya hata) — View geçiş kapısı.</summary>
    public bool SonucHazir => _basarili || _hataVar;

    /// <summary>Bekleyen göçleri uygular. Sonuç: <see cref="Basarili"/> / <see cref="HataVar"/>.</summary>
    public async Task CalistirAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        Basarili = false;
        HataVar = false;
        HataMetni = string.Empty;
        try
        {
            StatusMessage = "Bekleyen şema güncellemeleri denetleniyor...";
            var pending = await _sistemDatabaseService.GetPendingMigrationsAsync();
            BekleyenSayisi = pending?.Count ?? 0;

            if (BekleyenSayisi == 0)
            {
                Basarili = true;
                StatusMessage = "Bekleyen şema güncellemesi yok.";
                return;
            }

            StatusMessage = $"{BekleyenSayisi} şema güncellemesi uygulanıyor (ön yedek alınıyor, göç + doğrulama)...";
            var (ok, message) = await _sistemDatabaseService.ApplyPendingSistemMigrationsAsync();

            if (ok)
            {
                Basarili = true;
                StatusMessage = $"{BekleyenSayisi} şema güncellemesi uygulandı.";
            }
            else
            {
                HataVar = true;
                HataMetni = string.IsNullOrWhiteSpace(message) ? "Şema güncellemesi uygulanamadı." : message;
                StatusMessage = "Şema güncellemesi uygulanamadı.";
            }
        }
        catch (Exception ex)
        {
            HataVar = true;
            HataMetni = ex.GetBaseException().Message;
            StatusMessage = "Şema güncellemesi sırasında beklenmeyen hata.";
        }
        finally
        {
            IsBusy = false;
            NotifyPropertyChanged(nameof(SonucHazir));
        }
    }
}
