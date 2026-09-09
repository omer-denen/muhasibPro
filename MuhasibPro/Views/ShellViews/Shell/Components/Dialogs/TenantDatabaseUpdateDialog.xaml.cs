using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Business.ResultModels.TenantResultModels;

namespace MuhasibPro.Views.ShellViews.Shell.Components.Dialogs;

/// <summary>
/// Tenant güncelleme ön-bilgi dialogu — saf karar ekranı (detaylar ve göç sayfada).
/// Sonuç: Primary = Güncelle, Secondary = Daha sonra, Close/None = Vazgeç.
/// </summary>
public sealed partial class TenantDatabaseUpdateDialog : ContentDialog
{
    public TenantDatabaseUpdateDialog()
    {
        InitializeComponent();
    }

    public void SetState(TenantUpdateCheckResult check)
    {
        FirmaText.Text = string.IsNullOrWhiteSpace(check.FirmaUnvani) ? string.Empty : $"Firma: {check.FirmaUnvani}";
        FirmaText.Visibility = string.IsNullOrWhiteSpace(check.FirmaUnvani) ? Visibility.Collapsed : Visibility.Visible;
        DatabaseNameText.Text = check.DatabaseName;
        YearText.Text = check.MaliYil > 0 ? check.MaliYil.ToString() : "—";
        CurrentVersionText.Text = check.CurrentVersion ?? "—";
        TargetVersionText.Text = check.TargetVersion;
        MigrationCountText.Text = $"{check.PendingCount} göç";
        HeadlineText.Text = check.Headline;
    }
}
