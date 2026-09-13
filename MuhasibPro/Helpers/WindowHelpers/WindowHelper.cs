using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Models;
using MuhasibPro.HostBuilders;
using System.Collections.Concurrent;

namespace MuhasibPro.Helpers.WindowHelpers
{
    public static class WindowHelper
    {
        private static readonly object _lock = new object();
        private static readonly ConcurrentDictionary<int, WindowInfo> _windows = new();
        private static readonly ConcurrentDictionary<Window, int> _windowIds = new();

        private static bool _isShowingCloseDialog = false;

        public static Window MainWindow { get; private set; }

        public static Window CurrentWindow => GetActiveWindow();

        public static XamlRoot CurrentXamlRoot => GetCurrentXamlRoot();

        private static Window GetActiveWindow()
        {
            
            lock(_lock)
            {
                var activeWindow = _windows.Values
                    .Where(w => w.Window != null)
                    .OrderByDescending(w => w.LastActivated)
                    .FirstOrDefault()?.Window;
                return activeWindow ?? MainWindow;
            }
        }

        public static int GetActiveWindowId() { return CurrentWindow != null ? GetWindowId(CurrentWindow) : -1; }

        public static bool TryActivateWindow(int windowId)
        {
            var window = GetWindowById(windowId);
            if(window != null)
            {
                WindowPosition.SwitchToThisWindow(window);

                window.Activate();

                WindowPosition.SetForegroundWindow(window);
                return true;
            }

            return false;
        }

        private static XamlRoot GetCurrentXamlRoot()
        {
            // IsMainWindow ile aynı kaynak: ActiveWindow -> MainWindow, her pencere RegisterWindow ile kayıtlı
            var activeWindow = GetActiveWindow();
            if(activeWindow?.Content is FrameworkElement content && content.XamlRoot != null)
                return content.XamlRoot;

            if(MainWindow?.Content is FrameworkElement mainContent && mainContent.XamlRoot != null)
                return mainContent.XamlRoot;

            // Son çare: kayıtlı tüm pencereleri tara (DetailsWindow henüz Activated olmamış olabilir)
            foreach (var w in GetAllWindows())
            {
                if(w?.Content is FrameworkElement fe && fe.XamlRoot != null)
                    return fe.XamlRoot;
            }

            // Gerçekten hiç XamlRoot yoksa pencere henüz yüklenmemiştir — çağıran IsMainWindow ile yeni pencere açmalı
            throw new InvalidOperationException("No accessible XamlRoot found in any window. Window henüz InitializeWithContext olmamış olabilir.");
        }

        public static void SetMainWindow(Window window)
        {
           
            if(window == null)
                throw new ArgumentNullException(nameof(window));

            lock(_lock)
            {
                MainWindow = window;
                try
                {
                    int mainWindowId = (int)window.AppWindow.Id.Value;
                    RegisterWindow(window, mainWindowId, null);
                    window.AppWindow.Closing += OnMainWindowClosing;
                } catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Main window registration failed: {ex.Message}");
                    throw;
                }
            }
        }

        public static async void OnMainWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            if(_isShowingCloseDialog)
                return;
            await RequestCloseAsync(MainWindow);
        }

        /// <summary>
        /// Kapatma onayı tek kaynak — hem sistem (AppWindow.Closing → OnMainWindowClosing)
        /// hem özel title-bar kapatma butonu bu metottan geçer.
        /// </summary>
        public static async Task RequestCloseAsync(Window window)
        {
            if(_isShowingCloseDialog)
                return;
            _isShowingCloseDialog = true;

            try
            {
                var dialogService = ServiceLocator.Current.GetService<IDialogService>();
                bool shouldClose = await dialogService.ShowConfirmationAsync(
                    "Uygulamayı Kapat",
                    "Uygulamadan çıkmak istediğinize emin misiniz?",
                    "Evet", "İptal");

                if (shouldClose)
                {
                    // B5+Faz 6.79: kapanış güvenlik paketi — Sistem.db WAL checkpoint (her zaman) + ayarlıysa sistem/tenant yedeği.
                    await TryTakeExitBackupAsync();

                    // Onaylayan pencere kapanıyor — tekrar onay isteme
                    try { window.AppWindow.Closing -= OnMainWindowClosing; } catch { }
                    if (!ReferenceEquals(window, MainWindow))
                        try { MainWindow?.AppWindow.Closing -= OnMainWindowClosing; } catch { }

                    _isShowingCloseDialog = false;

                    // Diğer pencereleri kapat
                    lock (_lock)
                    {
                        var allWindows = _windows.Values.ToList();
                        foreach (var windowInfo in allWindows)
                        {
                            if (windowInfo.Window != MainWindow && windowInfo.Window != null)
                            {
                                try { windowInfo.Window.Close(); } catch { }
                            }
                        }
                    }

                    // Ana pencereyi doğrudan kapat — ikinci tıklamaya gerek kalmaz
                    try { window.Close(); } catch { }
                    if (!ReferenceEquals(window, MainWindow) && MainWindow != null)
                        try { MainWindow.Close(); } catch { }
                    try { Application.Current.Exit(); } catch { }
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Window closing error: {ex.Message}");
                try { Application.Current.Exit(); } catch { }
            }
            finally
            {
                _isShowingCloseDialog = false;
            }
        }

        /// <summary>
        /// Kapanış güvenlik paketi (B5 + Faz 6.79): Sistem.db WAL checkpoint (her zaman) +
        /// KapanistaOtomatikYedek ayarı açıksa sistem + aktif tenant yedeği.
        /// Best-effort: hata kapanışı engellemez, 10sn timeout (tenant kolu).
        /// </summary>
        private static async Task TryTakeExitBackupAsync()
        {
            try
            {
                var localSettings = ServiceLocator.Current.GetService<ILocalSettingsService>();
                if (localSettings == null) return;

                var dbSettings = await localSettings.ReadSettingAsync<DatabaseSettingsModel>(DatabaseSettingsModel.SettingsKey);
                bool kapanisYedegiAcik = dbSettings != null && dbSettings.KapanistaOtomatikYedek;

                // Faz 6.79: Sistem.db kapanış paketi (checkpoint her zaman + ayarlıysa yedek)
                try
                {
                    var yasam = ServiceLocator.Current.GetService<MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices.ISistemYasamDongusuService>();
                    if (yasam != null)
                    {
                        string sonuc = await yasam.EnsureShutdownSafetyAsync(kapanisYedegiAcik);
                        System.Diagnostics.Debug.WriteLine("[KapanışGüvenliği] " + sonuc);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[KapanışGüvenliği] Best-effort hata: {ex.Message}");
                }

                if (!kapanisYedegiAcik) return;

                var selectedService = ServiceLocator.Current.GetService<IFirmaWithMaliDonemSelectedService>();
                var databaseName = selectedService?.SelectedMaliDonem?.DatabaseName;
                if (string.IsNullOrWhiteSpace(databaseName)) return;

                var operationService = ServiceLocator.Current.GetService<ITenantSQLiteDatabaseOperationService>();
                if (operationService == null) return;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var backupTask = operationService.CreateBackupAsync(
                    databaseName, Domain.Enum.DatabaseEnum.DatabaseBackupType.Automatic);
                var completed = await Task.WhenAny(backupTask, Task.Delay(Timeout.Infinite, cts.Token));

                if (completed == backupTask && backupTask.Result?.Success == true)
                    System.Diagnostics.Debug.WriteLine("[KapanışYedeği] Yedek alındı: " + databaseName);
                else
                    System.Diagnostics.Debug.WriteLine("[KapanışYedeği] Zaman aşımı veya hata: " + databaseName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KapanışYedeği] Best-effort hata: {ex.Message}");
            }
        }

        public static void RegisterWindow(Window window, int windowId, Type viewModelType)
        {
            if(window == null)
                return;

            var windowInfo = new WindowInfo { Window = window, WindowId = windowId, ViewModelType = viewModelType };

            lock(_lock)
            {
                _windows[windowId] = windowInfo;
                _windowIds[window] = windowId;
            }

            // Window aktivasyon olayını dinle
            window.Activated += (s, e) =>
            {
                lock(_lock)
                {
                    if(_windows.TryGetValue(windowId, out var info))
                    {
                        info.LastActivated = DateTime.Now;
                    }
                }
            };

            window.Closed += (s, e) =>
            {
                UnregisterWindow(windowId);

                // Deadlock'u önlemek için doğrudan kontrol
                var activeWindow = GetActiveWindow();
                if(window == activeWindow)
                {
                    MainWindow?.Activate();
                }
            };
        }

        private static void UnregisterWindow(int windowId)
        {
            lock(_lock)
            {
                if(_windows.TryRemove(windowId, out var windowInfo))
                {
                    _windowIds.TryRemove(windowInfo.Window, out _);
                }
            }
            // Pencere kapanış temizliği — scope dispose edilmezse Scoped DbContext/VM'ler sızar.
            try { ServiceLocator.DisposeScope(windowId); } catch { }
        }

        public static Window GetWindowForElement(FrameworkElement element)
        {
            if(element?.XamlRoot == null)
                return GetActiveWindow();

            // Element'in XamlRoot'una göre window bul
            var window = GetAllWindows()
                .FirstOrDefault(w => w.Content is FrameworkElement content && content.XamlRoot == element.XamlRoot);

            return window ?? GetActiveWindow();
        }

        public static Window GetWindowByViewModel(Type viewModelType)
        {
            lock(_lock)
            {
                return _windows.Values.FirstOrDefault(w => w.ViewModelType == viewModelType)?.Window;
            }
        }

        public static bool TryActivateExistingWindow(Type viewModelType)
        {
            var existingWindow = GetWindowByViewModel(viewModelType);
            if(existingWindow == null)
                return false;

            var windowId = GetWindowId(existingWindow);
            if(windowId > 0)
            {
                ActivateWindow(windowId);
                return true;
            }
            return false;
        }

        public static List<Window> GetAllWindows()
        {
            lock(_lock)
            {
                return _windows.Values.Select(w => w.Window).Where(w => w != null).ToList();
            }
        }

        public static int GetWindowId(Window window)
        {
            if(window == null)
                return 0;

            lock(_lock)
            {
                return _windowIds.TryGetValue(window, out var windowId) ? windowId : 0;
            }
        }

        public static Window GetWindowById(int windowId)
        {
            if(windowId < -1)
                return null;

            lock(_lock)
            {
                return _windows.TryGetValue(windowId, out var windowInfo) ? windowInfo.Window : null;
            }
        }

        public static void ActivateWindow(int windowId)
        {
            try
            {
                var window = GetWindowById(windowId);
                window?.Activate();
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error activating window {windowId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Pencereyi ana pencereye göre konumlandır
        /// </summary>

     

        public class WindowInfo
        {
            public Window Window { get; set; }

            public int WindowId { get; set; }

            public Type ViewModelType { get; set; }

            public DateTime LastActivated { get; set; } = DateTime.Now;
        }
    }
}


