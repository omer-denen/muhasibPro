using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.HostBuilders;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.UIServices;
using System.Diagnostics;

namespace MuhasibPro.Views.ShellViews.Shell.Components.Dialogs;

public sealed partial class RestoreVerifyDialog : ContentDialog
{
    private string _generatedCode = string.Empty;
    private RestoreVerdict? _verdict;

    public DatabaseBackupResult? Backup { get; set; }
    public MaliDonemModel? TargetDonem { get; set; }

    public bool RestoreSucceeded { get; private set; }

    public RestoreVerifyDialog()
    {
        InitializeComponent();
        PrimaryButtonClick += OnPrimaryButtonClick;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Backup == null || TargetDonem == null)
        {
            ShowError("Yedek veya hedef dönem bilgisi eksik.");
            IsPrimaryButtonEnabled = false;
            return;
        }

        BackupFileText.Text = Backup.BackupFileName ?? Backup.DatabaseName ?? "—";
        BackupSizeText.Text = Backup.BackupFileSizeDisplay ?? $"{Backup.BackupFileSizeBytes} B";
        KimlikDetayText.Text = $"FirmaId={Backup.KimlikFirmaId?.ToString() ?? "-"} • DönemId={Backup.KimlikMaliDonemId?.ToString() ?? "-"} • Rol={Backup.KimlikRol?.ToString() ?? "-"} • Kurulum={ShortId(Backup.KimlikKurulumId)} • Makine={ShortId(Backup.KimlikMakineId)} • Sürüm={Backup.KimlikVersion ?? "-"}";

        bool kimlikli = Backup.IsKimlikli;
        KimlikRozetText.Text = Backup.KimlikRozeti;
        KimlikRozetBorder.Background = (Brush)Application.Current.Resources[kimlikli ? "SystemFillColorSuccessBackgroundBrush" : "SystemFillColorCautionBackgroundBrush"];
        KimlikRozetBorder.BorderBrush = (Brush)Application.Current.Resources[kimlikli ? "SystemFillColorSuccessBrush" : "SystemFillColorCautionBrush"];
        KimlikRozetText.Foreground = (Brush)Application.Current.Resources[kimlikli ? "SystemFillColorSuccessBrush" : "SystemFillColorCautionBrush"];

        // Hedef versiyonu al (tenant DB'den)
        string? hedefVersion = null;
        try
        {
            var lifecycle = ServiceLocator.Current.GetService<ITenantSQLiteDatabaseLifecycleService>();
            if (lifecycle != null && TargetDonem.DatabaseName != null)
            {
                var state = await lifecycle.GetTenantDatabaseStateAsync(TargetDonem.DatabaseName);
                hedefVersion = state?.Data?.CurrentVersion;
            }
        }
        catch { }

        // Kurulum / makine idleri
        string currentKurulum = "";
        string currentMachine = "";
        try
        {
            var kurulumService = ServiceLocator.Current.GetService<IKurulumKayitService>();
            var k = kurulumService != null ? await kurulumService.GetOrCreateAsync() : null;
            currentKurulum = k?.KurulumId ?? "";
        }
        catch { }
        try
        {
            var makine = ServiceLocator.Current.GetService<MuhasibPro.Data.Contracts.Database.Common.Helpers.IMakineKimligiProvider>();
            currentMachine = makine != null ? await makine.GetMachineIdAsync() : "";
        }
        catch { }

        _verdict = RestoreVerdictEvaluator.Evaluate(Backup, TargetDonem, currentKurulum, currentMachine, hedefVersion);
        ApplyVerdict(_verdict);
    }

    private void ApplyVerdict(RestoreVerdict v)
    {
        VerdictTitle.Text = v.Baslik;
        VerdictDesc.Text = v.Aciklama;

        var res = Application.Current.Resources;
        switch (v.Kind)
        {
            case RestoreVerdictKind.Allow:
                VerdictBorder.Background = (Brush)res["SystemFillColorSuccessBackgroundBrush"];
                VerdictBorder.BorderBrush = (Brush)res["SystemFillColorSuccessBrush"];
                VerdictTitle.Foreground = (Brush)res["SystemFillColorSuccessBrush"];
                VerdictDesc.Foreground = (Brush)res["SystemFillColorSuccessBrush"];
                VerdictIcon.Glyph = "\uE73E"; // check
                VerdictIcon.Foreground = (Brush)res["SystemFillColorSuccessBrush"];
                IsPrimaryButtonEnabled = true;
                CodePanel.Visibility = Visibility.Collapsed;
                break;
            case RestoreVerdictKind.Warning:
                VerdictBorder.Background = (Brush)res["SystemFillColorCautionBackgroundBrush"];
                VerdictBorder.BorderBrush = (Brush)res["SystemFillColorCautionBrush"];
                VerdictTitle.Foreground = (Brush)res["SystemFillColorCautionBrush"];
                VerdictDesc.Foreground = (Brush)res["SystemFillColorCautionBrush"];
                VerdictIcon.Glyph = "\uE7BA";
                VerdictIcon.Foreground = (Brush)res["SystemFillColorCautionBrush"];
                IsPrimaryButtonEnabled = true;
                CodePanel.Visibility = Visibility.Collapsed;
                break;
            case RestoreVerdictKind.RequireCode:
                VerdictBorder.Background = (Brush)res["SystemFillColorCriticalBackgroundBrush"];
                VerdictBorder.BorderBrush = (Brush)res["SystemFillColorCriticalBrush"];
                VerdictTitle.Foreground = (Brush)res["SystemFillColorCriticalBrush"];
                VerdictDesc.Foreground = (Brush)res["SystemFillColorCriticalBrush"];
                VerdictIcon.Glyph = "\uE783";
                VerdictIcon.Foreground = (Brush)res["SystemFillColorCriticalBrush"];
                IsPrimaryButtonEnabled = true;
                GenerateCode();
                CodePanel.Visibility = Visibility.Visible;
                break;
            case RestoreVerdictKind.Block:
                VerdictBorder.Background = (Brush)res["CardBackgroundFillColorSecondaryBrush"];
                VerdictBorder.BorderBrush = (Brush)res["CardStrokeColorDefaultBrush"];
                VerdictTitle.Foreground = (Brush)res["TextFillColorSecondaryBrush"];
                VerdictDesc.Foreground = (Brush)res["TextFillColorSecondaryBrush"];
                VerdictIcon.Glyph = "\uE711";
                VerdictIcon.Foreground = (Brush)res["TextFillColorSecondaryBrush"];
                IsPrimaryButtonEnabled = false;
                CodePanel.Visibility = Visibility.Collapsed;
                break;
        }
    }

    private void GenerateCode()
    {
        var rnd = new Random();
        _generatedCode = rnd.Next(100000, 999999).ToString();
        GeneratedCodeText.Text = _generatedCode;
    }

    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        args.Cancel = true;

        if (_verdict == null)
        {
            ShowError("Doğrulama yapılamadı.");
            deferral.Complete();
            return;
        }
        if (_verdict.Engelli)
        {
            deferral.Complete();
            return;
        }
        if (_verdict.KodGerekli)
        {
            var input = CodeInputBox.Text?.Trim() ?? "";
            if (input != _generatedCode)
            {
                ShowError("Onay kodu hatalı. Lütfen 6 haneli kodu doğru girin.");
                deferral.Complete();
                return;
            }
        }

        if (Backup == null || TargetDonem == null)
        {
            ShowError("Yedek bilgisi eksik.");
            deferral.Complete();
            return;
        }

        try
        {
            IsPrimaryButtonEnabled = false;
            var op = ServiceLocator.Current.GetService<ITenantSQLiteDatabaseOperationService>();
            var response = await op.RestoreBackupAsync(TargetDonem.DatabaseName, Backup.BackupFileName);
            if (response.Success && response.Data != null && response.Data.IsRestoreSuccess)
            {
                RestoreSucceeded = true;
                // Audit — sistem log (best-effort)
                try
                {
                    var log = ServiceLocator.Current.GetService<MuhasibPro.Business.Contracts.SistemServices.LogServices.ILogService>();
                    await log.SistemLogService.SistemLogInformationAsync("Yedek", "Geri Yükle", $"{Backup.BackupFileName} → {TargetDonem.DatabaseName} ({_verdict.Baslik}) kod={( _verdict.KodGerekli ? "evet" : "hayır")}", $"{TargetDonem.FirmaId}/{TargetDonem.Id}");
                }
                catch { }
                ServiceLocator.Current.GetService<INotificationService>()?.Show("Geri Yüklendi", $"'{Backup.BackupFileName}' geri yüklendi.", NotificationType.Success);
                Hide();
            }
            else
            {
                ShowError(response.Message ?? "Geri yükleme başarısız.");
                IsPrimaryButtonEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RestoreVerify error: {ex.Message}");
            ShowError(ex.Message);
            IsPrimaryButtonEnabled = true;
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }

    private static string ShortId(string? id) => string.IsNullOrWhiteSpace(id) ? "-" : id.Length <= 8 ? id : id.Substring(0, 8) + "…";
}
