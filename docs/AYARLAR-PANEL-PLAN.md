# Per-View Ayar Panelleri — Plan (Her View Kendi İşleminin Ayarını Yapar)

> **REVİZYON (Oturum 187-188, kullanıcı kararı):** Per-view Expander haritası YÜRÜRLÜKTEN KALKTI. Yerine **FirmaShellView içi Denetim Masası** (Seçim|Ayarlar anahtarı → sol nav + sağ içerik, Windows Ayarlar dili): tüm pre-dönem ayarları + Yonetim içeriği tek kapıda; ayarlar **kullanıcı bazlı** (`Anahtar:U{id}`, giriş-öncesi global fallback); `YonetimAyarlarDialog` kalkıyor. Aşağıdaki tablo arşiv-niteliklidir; yeni bölümler Denetim Masası'na yazılır.

## 1. Harita — View ↔ Modül ↔ Settings Modeli ↔ ViewModel ↔ Panel Yeri

| View (dosya) | Modül | Settings Modeli (Domain/Models) | ViewModel (mevcut / yeni) | Panel Yeri (XAML) | Kritik alanlar (`[YoneticiAyari]`) |
|---|---|---|---|---|---|
| `Splash/ExtendedSplash.xaml:1` | M1 AppPlatform | `AppPlatformSettings.cs:7` (`ThemeDefault`, `SplashStepDelayMs`, `StatusAutoHideMs`, `NotificationEnabled`) | `ExtendedSplashViewModel` (mevcut, `IAppPlatformSettingsProvider` eklenir) | Footer altı `Expander` “Görünüm & Bildirim Ayarları” — `CustomModernCard:1` içinde | `ApplicationDataFolder`, `LocalSettingsFile` |
| `Login/LoginView.xaml:1` | M2 Identity | `IdentitySettings.cs:7` (`MaxFailedAttempts:12`, `LockoutMinutes:15`, `AttemptWindowMinutes:18`, `Pbkdf2Iterations:24`, `MinPasswordLength:27`) | `LoginViewModel` (`IIdentitySettingsProvider` opsiyonel) + `QuickLoginAccountsViewModel` max hesabı | Sağ form altı `Expander` “Giriş Güvenliği” — `NumberBox` (1-50) + `ToggleSwitch` | `MaxFailedAttempts`, `LockoutMinutes` vb. |
| `Firma/FirmaView.xaml:1` + `Firmalar/FirmalarView.xaml:1` | M3 EntityRegistry | `EntityRegistrySettings.cs:10` (`FirmaKodPattern:15`, `DefaultDurum:18`, `AcikPageSize:21`, `ArsivPageSize:23`, `ValidationStrict:28`) | `FirmaViewModel` / `FirmalarViewModel` (`IEntityRegistrySettingsProvider`) | Liste başlığı yanında `⚙️` `Button` → `Flyout` veya alt `Expander` “Firma Kayıt Ayarları” — `TextBox` desen + `ComboBox` durum + `CheckBox` strict | `FirmaKodPattern`, `ValidationStrict` |
| `MaliDonem/MaliDonemYonetimView.xaml:1` | M5 Tenant | `TenantSettings.cs:14` (`YedekPageSize:24`, `BilinmeyenPageSize:26`) + M3 `EntityRegistrySettings` (`AcikPageSize:21`, `ArsivPageSize:23`) | `YonetimAyarlarViewModel` (facade, firmaId'li providerlar) | Pane en altı Ayarlar butonu → `YonetimAyarlarDialog` (QuickDialog: Liste 4 NumberBox + rozet) — Oturum 143-144'te uygulandı ✅🧪 | — (kritikler bu dialogda yok) |
| `MaliDonemYonetimView` (aynı) | M4 SystemDb | `DatabaseSettingsModel.cs:7` (`MaxManuelYedekSayisi:13` yönetici + 3 toggle) | `YonetimAyarlarViewModel` (`FirmaAyarlari` etkili okuma) | Aynı dialogun Saklama bölümü + "Varsayılanlara dön" + çakışma barı — Oturum 143-144 ✅🧪 | `MaxManuelYedekSayisi` |
| `ShellViews/Shell/FirmaShellView.xaml:1` | M3+M5 | `EntityRegistrySettings` + `TenantSettings` (yukarıdaki alt kümeler) | `FirmaShellViewModel` (`IFirmaWithMaliDonemSelectedService` aynası) | `FirmalarListControl.xaml:1` / `MaliDonemlerListControl.xaml:1` başlıklarında `⚙️` → `Settings Flyout` (sayfa başına kopya yok, `/Controls` ortak `ViewSettingsFlyout`’a yönlenir) | Aynı kritikler |
| `Settings/DatabaseSettingsView.xaml:15` | M4 | `DatabaseSettingsModel.cs:7` tümü | `DatabaseSettingsViewModel.cs:36` (mevcut) | Mevcut 4 kart korunur, eksik `SistemKeepLast`/`BusyTimeoutMs`/`JournalMode` için 2. kart genişletilir | — |
| `Settings/UpdateView.xaml:1` | M1 | `UpdateSettingsModel.cs:4` (`AutoCheckOnStartup:7`, `ShowNotifications:8`, `IncludeBetaVersions:9`, `FeedUrl:12`) | `UpdateViewModel` | `Pivot` içi “Ayarlar” sekmesi zaten var — `FeedUrl` + `CheckBox`’lar | — |
| `MainShell/MainShellView.xaml:1` | M4+M5 | — (navigasyon, ayar yok) | `MainShellViewModel` | Ayar yok — yalnız `NavSidebarControl.xaml:1` `Ayarlar` menüsü korunur | — |
| `SistemKurulum/SistemKurulumView.xaml:1` | M1+M4 | `AppPlatformSettings` + `DatabaseSettingsModel` salt-okunur | `SistemKurulumViewModel` (4 çocuk VM) | `KurulumKayitPanel.xaml:1` altı “Kurulum Yolu Ayarları” salt-okunur `TextBlock` (kritikler yönetici-only) | — |

## 2. Ortak Desen — Her Panel İçin

- **XAML:** `Border Style="{StaticResource CustomModernCard}" Padding="16"` + `Expander` (kapalıyken görünmez, açıkken `Spacing="12"`). İçeride `TextBlock` başlık `FontSize="13" FontWeight="SemiBold" Foreground="{ThemeResource MuhasibTextPrimaryBrush}"`, açıklama `FontSize="11" Foreground="{ThemeResource MuhasibSageSecondaryBrush}"`, kontrol `NumberBox` (`Minimum/Maximum` clamp aralığı modelden) / `ToggleSwitch` / `ComboBox` / `TextBox`. Kritik alan yanında `Border Padding="8,4" CornerRadius="{StaticResource MuhasibPillCornerRadius}" Background="{ThemeResource MuhasibPetrolTintBrush}"` “Yönetici” rozeti + `ToolTipService.ToolTip="Yalnızca yönetici değiştirebilir"`.
- **Code-behind:** `ViewModel.Property` `x:Bind Mode=TwoWay` + `IsEnabled="{x:Bind ViewModel.IsYonetici, Mode=OneWay}"` (veya `AyarYetkiDenetimi` exception → `StatusError` satır içi). Gölge yok (buz iç kartta gerekmez).
- **Yetki:** `AyarYetkiDenetimi.KritikDegisiklikleriDogrula:1` tek kaynak — ViewModel `SaveAsync:1` içinde `ReadSettingAsync` eski değerle karşılaştırıp `UnauthorizedAccessException` atar → `StatusError` (satır içi, `DialogService` değil). `[YoneticiAyari]` işaretli alanlar yalnızca `KullaniciRolTip.Yönetici` iken yazılır, diğerleri tüm kullanıcılara açık (`CEKIRDEK-MODUL-PLAN.md:18` Kural 9).
- **Kalıcılık:** `ILocalSettingsService.SaveSettingAsync(SettingsKey, model)` + `IEventBus.Publish(new AppSettingsChangedEvent(SettingsKey))` (opsiyonel `IEventBus` ctor paramı — providersız kurulum kırılmaz). `I*SettingsProvider:1` `Get/Save + Clamp + Yetki` deseni (M2/M3 örneği) korunur.
- **Generic kural:** Yeni liste/sonuç tipi yok — `GenericListViewModel<T>`, `ApiDataResponse<T>`, `IEventBus.Publish<TEvent>` genişletilir (`CEKIRDEK-MODUL-PLAN.md:12` Kural 3). Aynı işi yapan iki sınıf olmayacak — `grep 0 caller` silinir (Kural 4).
- **Ortak Controls kaydı:** Her yeni ayar paneli `MaliDonem/Yonetim/Components/` veya `Views/Components/` altında tek `UserControl` olur; sayfa başına kopya yazılmaz. Eklendiğinde `CEKIRDEK-MODUL-PLAN.md:213` **Ortak Controls Kaydı** tablosuna satır eklenir (Kural 8).

## 3. AI Studio / Biz İş Bölümü

- **Biz (kod):** Provider + Clamp + Yetki + ViewModel property + `SaveAsync` + `LoadAsync` + `ArchitectureTests` bekçisi + `dotnet test` (her ayar için 3-7 test: clamp/yetki/fallback). Hardcoded fallback (`?? 5`, `= 8`, `CleanOldBackupsAsync(3)`) silinir, model `Get*()` kullanılır. **Kural (`LOG-121-140.md:87` / `CEKIRDEK-MODUL-PLAN.md:16` Kural 7):** View’in işlemi `Request` modeliyle çağrılırken değer **ayar modelinden** okunur — ör. `DonemYedeklerViewModel.YedekAlAsync:171` `dbSettings.GetManuelKeep()` → `OperationService.CleanOldBackupsAsync(_databaseName, keep)` (eski `keepLast = 10` / `CleanOldBackupsAsync(3)` kalkar), `TenantSettings` için `BusyTimeoutMsToSeconds:72` aynı şekilde.
- **AI Studio (XAML):** Mevcut View’ların üzerine **yalnız** ilgili `Expander`/`Flyout` panelini `CustomModernCard` diliyle ekler, `x:Bind` sözleşmesini korur, yeni `Style` yaratmaz (varsa `RAPOR.md`’ye eksik token yazar, biz `DesignTokens.xaml`’a ekleriz). Çıktı `viewpackage/Views/...` aynı yollara — biz seçerek alırız (`AI-STUDIO-VIEW-BRIEF.md:7` dar brief).

## 4. Sıra ve Bağımlılık

1. **B1-B3 bug fix’leri** (`KONTROL-LISTESI.md:6.60`) — UI’de en iyi görüldüğü için tasarım güncellemesiyle birlikte, canlı UIA ile.
2. **Per-view paneller** yukarıdaki tabloda **L0→L4** sırasıyla: `AppPlatform` (Splash) → `Identity` (Login) → `EntityRegistry` (Firma) → `Tenant` (MaliDonemYonetim Yedek/Bilinmeyen) → `SystemDb` (Yedek & Saklama) → `Update` (Feed). Her panel tek PR (view + VM + provider + test).
3. **Doğrulama her panelde:** `dotnet build --no-incremental` 0/0 + `dotnet test` + canlı: panel açılır, değer değişir, `LocalSettings.json` yazar, `AppSettingsChangedEvent` yayılır, kritik alan yönetici-dışı red eder (inline hata).

## 5. 6.65-revizyon — Ayar dialogu v2 (Oturum 145 planı, kullanıcı onaylı)

Kararlar: menü adları `Panel Görünüm` / `Yedekleme`; rozet kalkar → header-sağ **durum butonu** (`Varsayılan` göstergeli / `Varsayılanlara Dön` aksiyonlu, tooltip'li); `Son değiştiren`/bilgi satırları kalkar (denetim JSON'da durur); en üst menü `Genel` (firma kartı + oluşturan); CommunityToolkit SettingsCard **emüle** edilir (yeni paket yok — gerekçe aşağıda); otomatik kayıt korunur + `Kaydedildi • HH:mm` mini bildirimi; arkaplan dumanı sistem davranışıdır, tüm yüzeyler opak tutulur.

| # | Değişiklik | Dosya |
|---|---|---|
| 1 | Menü: `Liste görünümü→Panel Görünüm`, `Yedek saklama→Yedekleme`; en üste `Genel` (ikon `IconBuilding`) | `YonetimAyarlarDialog.xaml` |
| 2 | Header-sağ: rozet Border'lar silinir → `DurumButonu` (Button): `IsOzel=false` → Text `Varsayılan`, `IsEnabled=false`, tooltip "Bu firma şablon değerleri kullanıyor"; `true` → Text `Varsayılanlara Dön`, Click=`OnSifirlaClick`, tooltip "Şablon değerlere dön (firma anahtarı sıfırlanır)" | aynı + `.xaml.cs` (yeni handler yok, mevcut `OnSifirlaClick` bağlanır) |
| 3 | Footer kartı silinir (`SonDegisiklikMetni` + json-notu UI'dan kalkar; `SifirlaAsync` + damga korunur) | aynı |
| 4 | YENİ `GenelPanel` (`YonetimAyarlar/AyarGenelPanel.xaml(+.cs)`, DataContext=facade): firma kartı (monogram `Initials` + kod/unvan + Yetkili/İl-Vergi/Tel-Eposta + `N dönem`) + `OlusturanAdi` (`IKullaniciService.GetKullaniciAsync(KaydedenId)` best-effort, fallback `ID: {n}`) + `KayitTarihi`; menüde ilk sırada, `SelectedIndex=0` buraya alınır | 2 YENİ dosya + facade'a `Firma`/`OlusturanAdi`/`DonemSayisi` |
| 5 | `OnBolumSecildi` generik kalır (Tag→panel); yeni grup kuralı aynen | `.xaml.cs` (değişiklik yok) |
| 6 | SettingsCard emülasyonu: her satır `Grid(Auto ikon+metin * / kontrol)` + `16px` başlık + `11px` açıklama (mevcut desen zaten birebir — yalnız spacing/token denetimi, yeni stil yok) | paneller (gerekirse) |
| 7 | `KaydedildiBildirimi` (VM: `string` + `bool`, `Task.Delay(3000)` sayaçlı otomatik silme) → header-buton yanı mini metin; her başarılı save'de `Kaydedildi • HH:mm:ss` | `YonetimAyarlarViewModel.cs` + dialog header |
| 8 | Opaklık: pane/content `CardBackground*` solid korunur (duman sistem overlay'i — kod değişikliği yok) | — |

Neden yeni paket yok: `CommunityToolkit.WinUI.Controls.SettingsControls` projede kayıtlı değil; gerçek `SettingsCard` için paket+restore+stil-çakışma riski alınmaz — mevcut satır deseni (ikon+başlık+açıklama/sol, kontrol/sağ) SettingsCard ile görsel birebir, token'lar bizde.

## 6. Riskler

- `bin/Debug` vs `bin/x64/Debug` karışıklığı (Mühür’de izleniyordu) — `slnx→sln` bu planın dışı.
- WMC9999 bu ortamda kırık — stil hatası Windows’ta derlenir.
- `Pooling` gibi kritik anahtarlar canlı tenant bağlantısını etkiler — `TenantSettingsProvider` değişikliği sonrası `BusyTimeoutMsToSeconds:72` clamp’i doğrulanmalı.

## 7. Uygulama durumu (Oturum 158-172)
- §5 v2 uygulandı (6.66) + v3 ekran-görüntüsü dili (Oturum 169: menü petrol hap + satır-başı beyaz kartlar) + `Kapat`→X (Oturum 171).
- Sapmalar (onaylı): +2 ayar rafta (0 tüketici, Kural 7); NumberBox→ComboBox preset (MS kılavuzu); 6.67 VM bölünmesi (orkestratör + 3 çocuk).
- Borç: build 0/0 + test 247/247 + canlı (Windows).
