using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Common;

namespace MuhasibPro.Business.Contracts.UIServices.CommonServices;
public interface IDialogService
{
    // Mevcut metotlar
    Task ShowAsync(string title, Exception ex, string ok = "Tamam");
    Task<bool> ShowAsync(string title, string content, string ok = "Tamam", string cancel = null);
    Task ShowAsync(Result result, string ok = "Tamam");

    // Yeni modern dialog metotları
    Task ShowSuccessAsync(string title, string content, string ok = "Tamam");
    Task ShowErrorAsync(string title, string content, string ok = "Tamam");
    Task ShowWarningAsync(string title, string content, string ok = "Tamam");
    Task ShowInfoAsync(string title, string content, string ok = "Tamam");
    Task<bool> ShowConfirmationAsync(string title, string content, string confirmText = "Evet", string cancelText = "Hayır");
    Task<string> ShowInputAsync(string title, string placeholder, string defaultValue = "");
    /// <summary>Tenant güncelleme ön-bilgi dialogu (versiyon şeridi + ana değişiklik; detay sayfada).</summary>
    Task<TenantUpdateDecision> ShowTenantUpdateConfirmAsync(TenantUpdateCheckResult check);
    /// <summary>Yedek sayısı saklama alt sınırının altına düşecekse yazılı onay dialogu (kod = dosya adı).</summary>
    Task<bool> ShowBackupDeleteGuardAsync(string backupFileName, int kalanSayi, int altSinir);
    /// <summary>Sistem.db geri-yükleme tek-kapı hüküm dialogu (hüküm + fark + gerekirse 6-haneli kod).</summary>
    Task<bool> ShowSistemRestoreVerifyAsync(SistemRestoreAnalizSonuc analiz);
    /// <summary>Kural 13 ortak yardım dialogu (ViewModels içeriği DTO ile taşır; View chrome'u App'te).</summary>
    Task ShowYardimAsync(string baslik, IReadOnlyList<YardimMaddesiDto> maddeler);
}
