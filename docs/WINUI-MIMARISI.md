# MuhasibPro — WinUI3 Pencere Mimarisi

> Kaynak: bu proje (ana proje). Katmanlı mimari ve pencere yönetimi aşağıdadır.

## 1. Katman
```
Libraries/MuhasibPro.Domain, Data, Business (Contracts/UIServices dahil)
Libraries/MuhasibPro.ViewModels (Business’a referans, WinUI yok)
MuhasibPro (WinUI App — tek WinUI projesi)
```
ViewModel ↔ View sözlüğü `NavigationService` static map üzerinden.

## 2. Pencere Yönetimi

### WindowHelper
Static sicil `_windows` / `_windowIds`, `MainWindow`, `CurrentWindow` (LastActivated), `GetActiveWindowId()` (ServiceLocator scope anahtarı), `TryActivateExistingWindow` (tek-instance), `GetWindowForElement`, PInvoke `SwitchToThisWindow`.

### WindowPosition
`PositionRelativeToParent`, `CenterOnScreen`, `SetWindowOwner` (`SetWindowLong -8`).

### DetailsWindow
Tek ikincil pencere üreticisi: `ExtendsContentIntoTitleBar`, `OverlappedPresenter.CreateForDialog` (IsModal), `SetWindowOwner(MainWindow)`, `Content = Frame → ShellView`.

### NavigationHelper
`CreateNewWindowAsync(vmType, param, title)` → tek-instance check → `new DetailsWindow` → `RegisterWindow` → `Frame.Navigate(typeof(ShellView), ShellArgs)`.

### NavigationService
Static `Register<TVm,TView>()` + instance `Frame` + `CreateNewViewAsync` → `NavigationHelper`.

## 3. ServiceLocator
`ConcurrentDictionary<int, ServiceLocator>` pencere-başına `IServiceScope`, `Current` → `WindowHelper.GetActiveWindowId()`, `GetService<T>()` scoped. Her pencere kendi VM scope’unda.

## 4. IContextService
`ContextId`, `IsMainView`, `Initialize(dispatcher, id, isMain)`, `RunAsync(Action)` (DispatcherQueue marshal), `InitializeWithContext(viewElement)` (XamlRoot → Window → id).

### 4.1 MainWindow’a Göre Dallanma (IsMainWindow)
`Libraries/MuhasibPro.ViewModels/Infrastructure/ViewModels/ViewModelBase.cs:38` `IsMainWindow => ContextService.IsMainView` — tüm VM’ler buradan dallanır:

- `GenericDetailsViewModel.cs:22` `CanGoBack => !IsMainWindow` — DetailsWindow’da geri var, MainWindow’da yok
- `GenericDetailsViewModel.cs:60` `if(!IsMainWindow) NavigationService.CloseViewAsync()` — DetailsWindow kapanır, MainWindow kalır
- `MaliDonemListViewModel.cs:174` / `FirmaListViewModel.cs:220` `if(IsMainWindow) CreateNewViewAsync<FirmaDetailsViewModel>(args) else Navigate<FirmaDetailsViewModel>(args)` — **MainWindow ise yeni pencere (DetailsWindow+ShellView) aç, değilse aynı Frame içinde navigate**
- `Views/Firmalar/FirmalarView.xaml:47` `IsButtonVisible="{x:Bind ViewModel.IsMainWindow}"` — “Yeni pencerede aç” butonu sadece MainWindow’da görünür

Her `Page` ctor’ında `ServiceLocator.Current.GetService<IContextService>().InitializeWithContext(DispatcherQueue, this)` (`LoginView.xaml.cs:17`, `ShellView.xaml.cs:41`, `FirmaShellView.xaml.cs:32`) ile `ContextId` set edilir; `MainWindow`’un `ContextId`’si `MainViewID` olur (`ContextService.cs:18`). `ServiceLocator.cs:22` `Current` → `WindowHelper.GetActiveWindowId()` ile pencere-başına scope sağlar — **olmazsa olmaz**.

## 5. ViewModel Altyapısı
`ICommonServices` (Context, Navigation, Message, Dialog, StatusBar vb.), `ViewModelBase : ObservableObject` (`IsBusy`, `Title`, `ExecuteActionAsync`), `ShellArgs {ViewModel, Parameter, UserInfo}`, `AsyncRelayCommand`.

## 6. ShellView
`MainWindow` ve `DetailsWindow` aynı `ShellView` container’ı kullanır: `TitleBar / Frame / ShellStatusBar`, `Frame.IsEnabled → ViewModel.IsEnabled`.

## 7. Akış
```
App.OnLaunched → SetMainWindow → ActivationService.ActivateAsync → theme → Frame.Navigate(ExtendedSplash)
Splash (SplashNavigator: routing + transfer check + backfill) → SistemKurulum / Login
Login → FirmaShell → MaliDonemSec (CheckUpdateRequiredAsync → ön-dialog → TenantDatabaseUpdateView
  [Yedek→Göç→Doğrulama→oto geri alma] → SwitchTenantAsync → MainShell)
Ek pencere: NavigationService.CreateNewViewAsync → DetailsWindow → ShellView → VM.LoadAsync
```

### 7.1 Güncelleme Koordinasyonu (`TenantDatabaseUpdateCoordinator`)
UI'sız `ITenantDatabaseUpdateService` (Check→mesaj/Describe) + VM koordinatörü (onay dialogu + progress):
güncelleme yoksa direkt geçiş, varsa karar → `TenantDatabaseUpdateView`'a `Navigate` / kalış.
`TenantDatabaseUpdateDialog` (ön-dialog: Güncelle/Daha sonra/Vazgeç) + `TenantDatabaseUpdateView`
(Yedek→Göç→Doğrulama→oto geri alma + `ValidateAsync` + `RefreshStartCommand`).

### 7.2 Mali Dönem Yönetimi (`MaliDonemYonetimView`, DetailsWindow)
Master-detail: sol AÇIK/ARŞİVLİ seçim listesi + sağ konu kartları; orkestratör
`MaliDonemYonetimViewModel` alt VM'leri kurar (Yedekler/Arsiv/Bilinmeyen/GenelBakis) +
`Subscribe/Unsubscribe`'ı hepsine delege eder (Subscribe = Unsubscribe kuralı).
Yedek/geri-yükleme bitince tipli olay (`TenantBackup/RestoreCompletedEvent`) listeleri tazeler.

### 7.3 UpdateView + Kapanış Temizliği
`UpdateView` (Pivot Güncelleme|Ayarlar) `OnNavigatedFrom`'da `Unsubscribe()` çağırır.
Pencere kapanışında `WindowHelper.UnregisterWindow` o pencerenin `ServiceLocator` scope'unu
dispose eder (`DisposeScope`); `ShellView` ayrıca `Window_Closed`'da VM `Unsubscribe()` yapar.

## 8. Dikkat
- Tek-instance: aynı VM ikinci pencere açılmaz
- DetailsWindow modal + owner=Main
- ContentDialog → `WindowHelper.CurrentXamlRoot`
- `ServiceLocator.Current` UI thread + `ContextService.RunAsync`
- Kapanışta scope dispose + Unsubscribe

