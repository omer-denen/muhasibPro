using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.HostBuilders;

namespace MuhasibPro.Views.SistemKurulum.Components;

public sealed partial class QuickSistemDbDiagDialog : ContentDialog
{
    public QuickSistemDbDiagDialog()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await RefreshAsync();
        await RefreshBackupAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            LoadingRing.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            var svc = ServiceLocator.Current.GetService<ISistemDatabaseService>();
            var resp = await svc.GetSistemDatabaseStateAsync();
            var state = resp.Data;
            if(state == null)
            {
                UpdateUi(false, "Bilinmiyor", "Durum alınamadı", "Var", "Bilinmiyor", "Geçersiz", "—", "—", "—");
                return;
            }
            // Success yok — modelden karar (AGENTS kesin kural)
            bool ready = state.IsDatabaseExists && state.CanConnect && !state.HasError && state.DatabaseValid;
            string fileExists = state.IsDatabaseExists ? "Var" : "Yok";
            string canConnect = state.CanConnect ? "Başarılı" : "Başarısız";
            string valid = state.DatabaseValid ? "Geçerli" : "Geçersiz";
            string tablePending = $"{state.TableCount} / {state.PendingMigrations?.Count ?? 0}";
            string msg = string.IsNullOrWhiteSpace(state.Message) ? (ready ? "Sistem.db hazır" : "Kurulum / onarım gerekli") : state.Message;
            string version = $"Sürüm: {state.CurrentVersion} • Boyut: {(state.DatabaseFileSizeBytes>0 ? $"{state.DatabaseFileSizeBytes/1024} KB" : "—")}";
            UpdateUi(ready, ready ? "Hazır" : "Kontrol Gerekli", msg, fileExists, canConnect, valid, tablePending, msg, version);
        }
        catch(Exception ex)
        {
            UpdateUi(false, "Hata", ex.Message, "—", "—", "—", "—", ex.Message, "—");
        }
        finally
        {
            LoadingRing.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }

    private async Task RefreshBackupAsync()
    {
        try
        {
            var backupMgr = ServiceLocator.Current.GetService<ISistemBackupManager>();
            var backups = await backupMgr.GetBackupsAsync();
            var last = backupMgr.GetLastBackupDate();
            int count = backups?.Count ?? 0;
            string lastText = last.HasValue ? last.Value.ToString("dd.MM.yyyy HH:mm") : "—";
            BackupInfoText.Text = $"Son yedek: {lastText} • Toplam: {count}";
        }
        catch
        {
            BackupInfoText.Text = "Son yedek: — • Toplam: —";
        }
    }

    private DateTime? _lastBackupTakenInSession;
    private bool _awaitingConfirm;
    private CancellationTokenSource _confirmCts;
    private async void OnBackupClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Aynı oturumda 2 dk içinde zaten alındıysa 5 sn onay iste
        if(_lastBackupTakenInSession.HasValue && (DateTime.Now - _lastBackupTakenInSession.Value).TotalMinutes < 2)
        {
            if(!_awaitingConfirm)
            {
                _awaitingConfirm = true;
                BackupButtonText.Text = "Tekrar Yedekle";
                BackupStatusText.Text = $"Bu oturumda zaten yedek alındı ({_lastBackupTakenInSession:HH:mm}) — tekrar alınsın mı? (5 sn)";
                BackupStatusText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibWarningBrush"];
                BackupStatusText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                _confirmCts?.Cancel();
                _confirmCts = new CancellationTokenSource();
                var ct = _confirmCts.Token;
                _ = Task.Run(async () =>
                {
                    try { await Task.Delay(5000, ct); } catch { return; }
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if(_awaitingConfirm)
                        {
                            BackupButtonText.Text = "Yedek Al";
                            BackupStatusText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                            _awaitingConfirm = false;
                        }
                    });
                });
                return; // ilk tıklamada yedek alma, 5 sn içinde tekrar beklenir
            }
            else
            {
                // İkinci tıklama — onayı kapat ve yedek almaya devam et
                _confirmCts?.Cancel();
                _awaitingConfirm = false;
                BackupButtonText.Text = "Yedek Al";
                BackupStatusText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
        }

        var backupMgr = ServiceLocator.Current.GetService<ISistemBackupManager>();
        try
        {
            BackupButton.IsEnabled = false;
            BackupButtonContent.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            BackupRing.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            BackupRing.IsActive = true;
            BackupStatusText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            BackupStatusText.Text = "Yedek alınıyor...";
            BackupStatusText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibSageTertiaryBrush"];
            var result = await backupMgr.CreateBackupAsync(DatabaseBackupType.Manual);
            bool ok = result != null;
            if(ok)
            {
                _lastBackupTakenInSession = DateTime.Now;
                try
                {
                    var localSettings = ServiceLocator.Current.GetService<MuhasibPro.Business.Contracts.UIServices.ILocalSettingsService>();
                    var dbSettings = localSettings != null
                        ? await localSettings.ReadSettingAsync<MuhasibPro.Domain.Models.DatabaseSettingsModel>(MuhasibPro.Domain.Models.DatabaseSettingsModel.SettingsKey)
                        : null;
                    int keep = (dbSettings ?? new MuhasibPro.Domain.Models.DatabaseSettingsModel()).GetSistemKeep();
                    await backupMgr.CleanOldBackupsAsync(keep);
                    BackupStatusText.Text = $"Yedek başarıyla alındı (en fazla {keep} manuel saklanır)";
                }
                catch { BackupStatusText.Text = "Yedek başarıyla alındı"; }
            }
            else BackupStatusText.Text = "Yedek alınamadı";
            BackupStatusText.Foreground = ok ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibSuccessBrush"] : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibDangerBrush"];
            await RefreshBackupAsync();
            await Task.Delay(1400);
            BackupStatusText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        catch(Exception ex)
        {
            BackupStatusText.Text = $"Hata: {ex.Message}";
            BackupStatusText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibDangerBrush"];
            BackupStatusText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
        finally
        {
            BackupRing.IsActive = false;
            BackupRing.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            BackupButtonContent.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            BackupButton.IsEnabled = true;
            BackupButtonText.Text = "Yedek Al";
            _awaitingConfirm = false;
        }
    }

    private void UpdateUi(bool ready, string pill, string detail, string fileExists, string canConnect, string valid, string tablePending, string msg, string version)
    {
        StatusPill.Background = ready ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibSuccessBgBrush"] : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibWarningBgBrush"];
        StatusPill.BorderBrush = ready ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibSuccessBrush"] : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibWarningBrush"];
        StatusIcon.Glyph = ready ? "\uE73E" : "\uE7BA";
        StatusIcon.Foreground = ready ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibSuccessBrush"] : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibWarningBrush"];
        StatusText.Text = pill;
        StatusText.Foreground = ready ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibSuccessBrush"] : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibWarningBrush"];
        StatusDetail.Text = detail;
        FileExistsText.Text = fileExists;
        CanConnectText.Text = canConnect;
        CanConnectText.Foreground = canConnect=="Başarılı" ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibSuccessBrush"] : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibDangerBrush"];
        ValidText.Text = valid;
        ValidText.Foreground = valid=="Geçerli" ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibSuccessBrush"] : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MuhasibWarningBrush"];
        TablePendingText.Text = tablePending;
        MessageText.Text = msg;
        VersionText.Text = version;
    }
}
