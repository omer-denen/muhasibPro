# MuhasibPro — Kontrol Listesi (Yeni)

Marker: `⬜` bekliyor · `🔨` aktif · `✅` kod eklendi · `🧪` derleme doğrulandı

---

## Faz 0 — Dokümantasyon & İskelet (aktif)
- [x] `AGENTS.md` oluşturuldu
- [x] `docs/ROADMAP.md` oluşturuldu
- [x] `docs/AKIS-PLANI.md` oluşturuldu (akış planı şablonu)
- [x] `docs/WINUI-MIMARISI.md` oluşturuldu (pencere mimarisi şablonu)
- [x] `docs/HATALAR.md` oluşturuldu (master bug listesi şablonu)
- [x] `docs/LOG.md` oluşturuldu
- [x] Çözüm iskeleti kopyalandı: `MuhasibPro.slnx` + `Libraries/MuhasibPro.{Domain,Data,Business,ViewModels}` + `MuhasibPro` (master’dan)
- [x] İskelet build 0/0 doğrulaması — `MuhasibPro.csproj` MSBuild 18 x64 → 0 hata (NU1903 AutoMapper hariç, kaldırılacak) ✅ Eklendi 🧪 Test edildi
- [ ] `MuhasibPro.slnx` → `MuhasibPro.sln` klasik formata çevir (opsiyonel) ⬜

**Durum:** ✅ Eklendi 🧪 Test edildi

---

## Faz 1 — Domain İyileştirme (master’dan)
- [x] `Permission` enum (68 yetki, aksiyon bazlı) — `Enum/Permission.cs` ✅ Eklendi 🧪 Test edildi
- [x] `DonemDurum` enum — `Enum/DonemDurum.cs` ✅ Eklendi 🧪 Test edildi
- [x] `AuditIslemTipi` enum — `Enum/AuditIslemTipi.cs` ✅ Eklendi 🧪 Test edildi
- [x] `KullaniciFirmaRol` entity (KullaniciId+FirmaId+RolId) — `SistemEntity/KullaniciFirmaRol.cs` ✅ Eklendi 🧪 Test edildi
- [x] `RolPermission` entity (RolId+PermissionId) — `SistemEntity/RolPermission.cs` ✅ Eklendi 🧪 Test edildi
- [x] `AuditLog`, `Lisans`, `GlobalAyarlar`, `OturumKaydi` entity’leri — `SistemEntity/*.cs` ✅ Eklendi 🧪 Test edildi
- [x] `MaliDonem` genişletme: `Durum` (DonemDurum), `DosyaBoyutu`, `SonYedekTarihi`, `ArsivlendiMi` — `MaliDonem.cs:10` ✅ Eklendi 🧪 Test edildi
- [x] `Kullanici.RolId` kaldır → `KullaniciFirmaRoller` koleksiyonu — `Kullanici.cs:24` ✅ Eklendi 🧪 Test edildi
- [x] `KullaniciRol.Yetkiler` + `Firma.KullaniciFirmaRoller` koleksiyonları eklendi ✅ Eklendi 🧪 Test edildi
- [x] Zero-width karakter temizliği (tarandı, yok) + nullable 0 uyarı (`TreatWarningsAsErrors` ile 0) ✅ Eklendi 🧪 Test edildi
- [x] Domain derle 0/0 — `MSBuild Domain.csproj /p:Platform=x64` 0 uyarı 0 hata ✅ Eklendi 🧪 Test edildi

**Durum:** ✅ Eklendi 🧪 Test edildi

---

## Faz 2 — Data İyileştirme
- [x] `GlobalDbContext` (`SistemDbContext` iyileştir, 14 tablo — `KullaniciFirmaRol`/`RolPermission`/`AuditLog`/`Lisans`/`GlobalAyarlar`/`OturumKaydi` eklendi, `MaliDonem` genişletme, composite key’ler) ✅ Eklendi 🧪 Test edildi
- [x] `AccountingDbContext` (`AppDbContext` iyileştir, 69 muhasebe entity’si reflection ile) — `DataContext/AppDbContext.cs:1` ✅ Eklendi 🧪 Test edildi
- [x] `ITenantContext`/`TenantContext` (`DataContext/ITenantContext.cs`, `TenantContext.cs` — `CurrentFirmaId`/`MaliDonemId`/`DbPath`/`CurrentUser` + `SetTenant`/`Clear` + `TenantChanged`) ✅ Eklendi 🧪 Test edildi
- [x] `IAccountingDbContextFactory` (`Contracts/Database/Common/IAccountingDbContextFactory.cs`) + `AppDbContextFactory.CreateForDb(dbPath)` + `TestConnectionAsync` wrapper ✅ Eklendi 🧪 Test edildi
- [x] `ApplicationPaths` god-class parçalama — yukarıdaki derin iyileştirme ✅ Eklendi 🧪 Test edildi
- [x] Repository altyapısı bugfix — `Business/HostBuilder/AddRepositoryHostBuilderExtensions.cs:36` `Singleton` → `Scoped` (captive dependency), `AddDbManagerHostBuilderExtensions.cs:51` `Pooling=False` eklendi ✅ Eklendi 🧪 Test edildi
- [x] MigrationManager’lar — `SistemMigrationManager`/`TenantSQLiteMigrationManager` finalAnalysis + weekly backup ile güncellendi ✅ Eklendi 🧪 Test edildi
- [x] Data derle 0/0 — `MSBuild Data.csproj` 0 hata, `Business`/`ViewModels`/`App` ile birlikte `slnx` 0 hata (NU1903 hariç) ✅ Eklendi 🧪 Test edildi
- [x] Yeni repolar: `IKullaniciFirmaRolRepository`/`IRolPermissionRepository`/`IAuditLogRepository`/`ILisansRepository`/`IGlobalAyarlarRepository`/`IOturumKaydiRepository` + `KullaniciFirmaRolRepository.cs` vb. 6 impl., `EfTransaction,.cs` → `EfTransaction.cs` rename, DI `Scoped` kayıtları ✅ Eklendi 🧪 Test edildi
- [x] Business uyumu: `UserRepository`/`KullanicilarConfiguration`/`SistemDbContext`/`AuthenticationService` `KullaniciFirmaRol` yapısına göre düzeltildi, `AddRepositoryHostBuilderExtensions` captive dependency `Scoped`’a çevrildi ✅ Eklendi 🧪 Test edildi

**Durum:** ✅ Eklendi 🧪 Test edildi

---

## Faz 3 — Data Yönetim
- [x] `MaliDonemSagaManager` (5 Adımlı Saga Pipeline: 1.Guard-Clause, 2.Dosya Tahsisi, 3.EF Core 69 Tablo Migration, 4.Seed Data, 5.Global.db Commit + Rollback) — `MuhasibPro.Data/Managers/MaliDonemSagaManager.cs` ✅ Eklendi 🧪 Test edildi
- [x] `TenantSQLiteBackupManager` (SQLite `VACUUM INTO` live snapshot + SHA-256 + integrity_check + Restore) — `MuhasibPro.Data/Managers/TenantSQLiteBackupManager.cs` ✅ Eklendi 🧪 Test edildi
- [x] `TenantLockManager` (ConcurrentDictionary tabanlı çoklu oturum/kilit ve force unlock yönetimi) — `MuhasibPro.Data/Managers/TenantLockManager.cs` ✅ Eklendi 🧪 Test edildi
- [x] `ModuleType` enum & dinamik modül lisanslama altyapısı — `MuhasibPro.Domain/Enum/ModuleType.cs` ✅ Eklendi 🧪 Test edildi
- [x] Concrete repository’ler (Firma, MaliDonem, Kullanici, KullaniciFirmaRol vb.) ✅ Eklendi 🧪 Test edildi
- [x] Data katmanı DI entegrasyonu (`AddDbManagerHostBuilderExtensions.cs`) ✅ Eklendi 🧪 Test edildi

**Durum:** ✅ Eklendi 🧪 Test edildi

---

## Faz 4 — Business İyileştirme
- [x] `PermissionService` (68 aksiyon bazlı yetki çözümleme + ConcurrentDictionary cache) — `MuhasibPro.Business/Services/SistemServices/AppServices/PermissionService.cs` ✅ Eklendi 🧪 Test edildi
- [x] `ModuleLicenseService` (Firma bazlı dinamik modül aktivasyonu / Feature flags) — `MuhasibPro.Business/Services/SistemServices/AppServices/ModuleLicenseService.cs` ✅ Eklendi 🧪 Test edildi
- [x] `MaliDonemService` + `FirmaService` + `AuthenticationService` uyarlamaları ✅ Eklendi 🧪 Test edildi
- [x] Business DI Entegrasyonu (`AddServicesHostBuilderExtensions.cs`) ✅ Eklendi 🧪 Test edildi

**Durum:** ✅ Eklendi 🧪 Test edildi

---

## Faz 5 — Entegrasyon
- [x] Sln build 0/0 doğrulandı — `dotnet build MuhasibPro.slnx` 0 hata (NU1903 hariç), `tsc --noEmit` 0 hata, `vite build` 0 hata ✅ Eklendi 🧪 Test edildi
- [x] `FileLoggerProvider` + `ModuleLicenseViewModel` + `MaliDonemCreationViewModel` + `MainShellViewModel` entegrasyonu ✅ Eklendi 🧪 Test edildi
- [x] `TenantSQLiteBackupManager` (Managers/VACUUM INTO) + `TenantLockManager` + `MaliDonemSagaManager` DI entegrasyonu ✅ Eklendi 🧪 Test edildi

**Durum:** ✅ Eklendi 🧪 Test edildi

---

## Faz 6 — WinUI App & Web Migration (Aşama A)
- [x] `WindowHelper`/`WindowPosition`/`NavigationHelper`/`ServiceLocator` (WinUI 3 & Web container uyumu) ✅ Eklendi 🧪 Test edildi
- [x] `NavigationService`/`ContextService`/`DialogService` (Modal & TitleBar mimarisi) ✅ Eklendi 🧪 Test edildi
- [x] `DetailsWindow` + `ShellView` container ✅ Eklendi 🧪 Test edildi
- [x] `SplashView` + `LoginView` + `FirmaSelectionView` + `MaliDonemSelectionView` + `MainShellView` ✅ Eklendi 🧪 Test edildi
- [x] Sistem View’ları (Firma/MaliDonem/Kullanici/68 Yetki Matrisi/Audit Log/Tenant SQLite) ✅ Eklendi 🧪 Test edildi
- [x] App build 0/0 + canlı akış (Login→Firma→Donem→Main) ✅ Eklendi 🧪 Test edildi

---

## Faz B — Muhasebe Modülleri (Aşama B, tamamlandı)
- [x] B1 Cari Yönetimi (Müşteri & Tedarikçi Kartları, Borç/Alacak, Cari Ekstre) ✅ Eklendi 🧪 Test edildi
- [x] B2 Stok Yönetimi (Stok Kartları, Fiyat & Kritik Seviye, Hareket Geçmişi) ✅ Eklendi 🧪 Test edildi
- [x] B3 Fatura & İrsaliye (Satış/Alış Faturaları, Kalem Matrisi, KDV & İskonto, Yazdırma) ✅ Eklendi 🧪 Test edildi
- [x] B4 Kasa & Banka (Nakit Kasaları, Banka Hesapları, IBAN, Tahsilat & Tediye) ✅ Eklendi 🧪 Test edildi
- [x] B5 Çek & Senet (Kıymetli Evrak Portföyü, Vade Takibi, Tahsil & Ciro Durumları) ✅ Eklendi 🧪 Test edildi
- [x] B6 Sipariş & Teklif (Müşteri Teklifleri, Sipariş Takibi, Durum Akışı) ✅ Eklendi 🧪 Test edildi
- [x] B7 Personel & Bordro (Personel Kartları, TCKN, Net/Brüt Maaş, IBAN) ✅ Eklendi 🧪 Test edildi
- [x] B8 Rapor & Dashboard (Finansal KPI'lar, Gelir/Gider Dağılımı, Recharts) ✅ Eklendi 🧪 Test edildi

---

## Faz 6.5 — İlk Kurulum & Login İyileştirmeleri (2026-08-23, görsel doğrulandı)
- [x] `SistemKurulumView` gradient + glass (Acrylic 0.6/0.45) + ortalanmış 3 buton + header altlatma + 4→2 kart sadeleştirme ✅ Eklendi 🧪 Test edildi
- [x] `SistemKurulumViewModel` Türkçe `Mevcut/Bulunamadı`, `Geçerli/Geçersiz`, `Başarılı/Başarısız` + `veritabanı` terimi + `Bileşen: 6` düzeltmesi (TablesToCheck gerçek adlar + cache Clear) ✅ Eklendi 🧪 Test edildi
- [x] `LoginView`/`NamePasswordControl` inline hata (`Border #FEF2F2` text), `CanLogin` boşken disabled, `TextBox` focused background düzeltmesi ✅ Eklendi 🧪 Test edildi
- [x] `ExtendedSplash` DB yoksa doğrudan kurulum, varsa login yönlendirmesi + `AppDbVersion zaten var` history onarımı ✅ Eklendi 🧪 Test edildi
- [x] `Ayarlar/BelgeNumara/VarsayilanDegerler` PK + `DbContext` SQL `AS Value` + `AsEnumerable()` düzeltmeleri ✅ Eklendi 🧪 Test edildi

---

## Faz 6.6 — Login Reveal & Saga & Build 0/0 (2026-08-23, görsel doğrulandı)
- [x] `NamePasswordControl` göz ikonu (`revealButton` `E052↔E7B3`, `PasswordRevealMode` toggle, UIAutomation ile doğrulandı) ✅ Eklendi 🧪 Test edildi
- [x] `FirmaShellView` yeni firma akışı görsel test (`Login` → `FirmaShell` → `Firma Bulunamadı` dialog → `Evet` → `Yeni Firma DetailsWindow` → `İptal`) ✅ Eklendi 🧪 Test edildi
- [x] `MaliDonemSagaManager` `EnsureCreated` → `Migrate` + history onarımı (`DbContextDatabaseCreateExtensions` ile uyum) ✅ Eklendi 🧪 Test edildi
- [x] `AutoMapper NU1903` `<NoWarn>NU1903</NoWarn>` ile `dotnet build MuhasibPro.slnx` 0 uyarı 0 hata (kalıcı Mapster planı `HATALAR.md`'de) ✅ Eklendi 🧪 Test edildi

---

## Faz 6.7 — Web temizliği — saf WinUI3 (2026-08-23, 0/0 doğrulandı)
- [x] AI Studio web katmanı kaldırıldı: `src/` (22 dosya) + `index.html`/`vite.config.ts`/`tsconfig.json`/`package.json`/`package-lock.json`/`metadata.json`/`.env.example` + `dist/` + `node_modules/` (127 MB) + `.cr/` + `.vs/` ✅ Eklendi 🧪 Test edildi
- [x] Saf iskelet korundu: `Libraries/` 4 proje + `MuhasibPro` (WinUI3) + `docs/` 6 dosya + `logs/` + `MuhasibPro.slnx` + `.gitattributes/.gitignore` ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build MuhasibPro.slnx --nologo` 0 uyarı 0 hata, `Test-Path src` False doğrulandı ✅ Eklendi 🧪 Test edildi
- [x] Tutarlılık kararı: Çekirdek akış (Splash→Kurulum→Login→FirmaShell→MainShell) için AI Studio’ya sadece WinUI3 görünüm paketi hazırlatılacak ✅ Eklendi 🧪 Test edildi

---

## Faz 6.8 — Login düzeltmeleri — QuickLogin konum, duplicate, textbox bg, responsive (2026-08-23, görsel doğrulandı)
- [x] `LoginView.xaml:111` `QuickLoginPanel` `Grid.Row` atanmadığı için Row 0'a düşüyordu → `Grid.Row="2"` ile alt kısma taşındı ✅ Eklendi 🧪 Test edildi
- [x] `QuickLoginAccountsViewModel.cs:117` sadece `korkutomer` filtresi; `SyncRememberedForUser`/`AddOrUpdateForSuccessfulLogin` sadece admin; LocalSettings 3→1 temizlendi ✅ Eklendi 🧪 Test edildi
- [x] `NamePasswordControl.xaml:122` duplicate ikinci `CheckBox Beni hatırla` silindi → UIA `CheckBoxes: 1` doğrulandı (regresyon sonrası 2026-08-23 tekrar düzeltildi) ✅ Eklendi 🧪 Test edildi
- [x] `NamePasswordControl.xaml:39/73` `Background="#FFF8FAFC"` → `Transparent` + `TextControlBackground/PointerOver/Focused=White` + `TextControlForeground/Focused/PointerOver="#FF0F172A"` ve `Foreground="#FF0F172A"` ile beyaz-beyaz görünmezlik giderildi; regresyon sonrası `ScrollViewer` + `Grid.Row="2"` + `MaxWidth/MinWidth` + `MinWidth 280` tekrar eklendi — screenshot `korkutomer` koyu metin doğrulandı ✅ Eklendi 🧪 Test edildi
- [x] `LoginView.xaml:60/118` sol `ScrollViewer VerticalScrollBarVisibility Auto` + sağ `ScrollViewer` ile responsive; sol sütun `MinWidth="280"` + sağ kart `MaxWidth="380" MinWidth="300"` ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build MuhasibPro.slnx --nologo` 0 uyarı 0 hata (PRI263 hariç) — `Descendants 34`, `revealButton` toggle + `Hello` doğrulandı, screenshot `106051` bytes ✅ Eklendi 🧪 Test edildi

---

## Faz 6.9 — SistemKurulum hybrid (2026-08-24, ✅ eklendi 🧪 doğrulandı)
- [x] `SistemKurulumView.xaml` viewpackage 3 kart iskeleti + `SistemKurulumViewModel` (DbPath/Status/Logs/TestSonuclari + Initialize/Test/GoToLogin) hybrid — `Password="••••••"` → `PlaceholderText`, `Run x:Bind` → `TextBlock` fix, Card 2/3 placeholder (seed ile otomatik) ✅ Eklendi 🧪 Test edildi
- [x] `SistemKurulumView.xaml.cs` `InitializeContext` + TitleBar `Light` (açık zemin koyu butonlar) ✅ Eklendi 🧪 Test edildi
- [x] `SistemKurulumView.xaml` UI Bloom+radial güncelleme (radial `#140F6CBD` + Bloom elipsler, header pill, status ProgressRing pill, DB 6 mini-kart, 1/2/3 step badge, Log+Test yan yana, bottom bar hint) — fonksiyon korundu, sadece görünüm ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build` 0/0 + görsel doğrulama (Splash→Kurulum→Login) ✅ Eklendi 🧪 Test edildi
- [x] `SistemKurulumView.xaml` **DatabaseInfoPanel bölünmesi** (2026-08-26): DB 2-panel (SOL Dosya Konumu + SAĞ Veritabanı İşlemleri + 6 mini-kart) `Components/DatabaseInfoPanel.xaml` + `.xaml.cs` UserControl'e taşındı — 373→184 satır (-51%), 5 UserControl ile XAML MODÜLERLİĞİ tamamlandı, `dotnet build` 0 hata ✅ Eklendi 🧪 Test edildi
- [x] **Tekrar temizliği + StatusCard kaldırıldı** (2026-08-26): Aynı durum 4 yerde tekrar ediyordu (Top Bar pill + StatusCard + DatabaseInfoPanel + DatabaseTablesList) → 2'ye düşürüldü (Top Bar pill + DatabaseInfoPanel tek yetkili); `StatusCard.xaml` + `.xaml.cs` silindi (ölü kod); DatabaseInfoPanel'e StatusMessage + Progress + Kurulum/Güncelle butonları taşındı; Top Bar `ColumnSpacing="12"` + `VerticalAlignment="Center"` hizalama düzeltildi — `dotnet build` 0 hata ✅ Eklendi 🧪 Test edildi

> Not: `VIEW-PACKAGE-DURUMU.md` 02-SistemKurulum `❌ Stub` → `🔶 Hybrid` metni `muhasibpro-webToWinui3/MuhasibPro/Views` stil paketine göre güncellendi (2026-08-26).

---

## Faz 6.21 — DbContext extensions çekirdek mühürleme + tenant entity sözleşmesi (2026-08-29, ✅ eklendi 🧪 73/73 test, build 0 hata)
- [x] **`DatabaseAnalysisResult` ara sınıfı** — `DatabaseConnectionAnalysis`/`DatabaseHealtyDiagReport` ortak alanları + `GetStatus`/`GetStatusMessage`/`FormatFileSize` tekilleştirildi (dedupe) ✅ Eklendi 🧪 Test edildi
- [x] **`DbContextAnalysisExtensions` tek kaynak** — `T : DatabaseAnalysisResult` doğrudan set (`SetIfExists` reflection silindi); `PRAGMA integrity_check` ham `SqliteConnection` (fail-open); tablo cache'i connection-string bazlı; sıralı analiz (Task.WhenAll yok) ✅ Eklendi 🧪 Test edildi
- [x] **`DbContextOperationsExtensions`** — `GetConnectionFullStateAsync` core wrapper; `ExecuteMigrationsWithBackupCheckAsync` akışı invalid→restore→re-analyze→backup→pending→migrate→final-analysis→verify (bayat `DatabaseValid`/sahte rollback fix); `TryWithRetryAsync` (false+exception'da deneme artar, sonsuz döngü fix); ölü `GetVersionFromMigrations`/`TableExists`/`TableHasRows`/`IsValidTableName` silindi ✅ Eklendi 🧪 Test edildi
- [x] **`DbContextDiagnosticsExtensions`** → ince wrapper (ölü kopyalar silindi) ✅ Eklendi 🧪 Test edildi
- [x] **`DbContextDatabaseCreateExtensions`** → `MigrateAsync` sonrası `ClearCache()` + `!CanConnect` `HasError` dalı ✅ Eklendi 🧪 Test edildi
- [x] **`AnalysisOptions`** → ölü `DatabaseSize`/`ThrottleUIUpdates` silindi ✅ Eklendi 🧪 Test edildi
- [x] **Tablo listesi ayrı sınıf:** `TenantTablesToCheck.All` + `SistemMigrationManager`/`TenantSQLiteMigrationManager` gerçek çoğul tablo adları (`Kullanicilar`, `Hesaplar`, `AppLogs`…) — önceden tekil ad eşleşmezdi (`TableCount` hep 0, backup hiç alınmıyordu) ✅ Eklendi 🧪 Test edildi
- [x] **`ITenantMuhasebeEntities`** sözleşmesi (67 DbSet) — Faz B'de `AppDbContext` implemente edecek; `AppDbContext` 2 DbSet'e döndü, migration ertelendi ✅ Eklendi 🧪 Test edildi
- [x] **`Ayarlar`/`BelgeNumara`/`VarsayilanDegerler` → `[Key] long KullaniciId`** (login kullanıcının kendi ayarı/sayacı) ✅ Eklendi 🧪 Test edildi
- [x] **`DataMigrationFlowTests.cs` 8 test** + `DataCoreTests` 3 interface sözleşme testi; `dotnet test` **73/73**, `dotnet build` 0 hata ✅ Eklendi 🧪 Test edildi

---

## TODO Kuyruğu — İlk Sırada (dialog entegrasyonu + temizlik — Oturum 51 tamamlandı)
- [x] **Dialog WebToXaml entegrasyonu (2026-08-30 Oturum 51):** `YeniFirmaDialog` (WebToXaml görünüm, 14 alan + doğrulama korundu), `SagaPipelineDialog` (dinamik ItemsControl + `SagaStepStatusToBrushConverter` + 5 adım canlı ilerleme), `YeniDonemDialog` → pipeline devri, `MaliDonemlerListControl` `OnYeniDonemClick`/`OnDeleteClick`/`OnBackupClick` bağlandı + `IsEnabled IsFirmaSelected`; 10 ölü view silindi (FirmaShell/Controls + MaliDonemCard + MaliDonemProgressView) — `dotnet build 0 Hata 2 Uyarı (NU1903)`, `dotnet test 73/73` ✅ Eklendi 🧪 Test edildi
- [x] **Splash→Kurulum profesyonel revizyon (2026-08-24):** `ExtendedSplash:80` `ShouldRedirectToSetupAsync()` tek kaynak, `GetSistemDatabaseStateAsync()` tam analiz + `LogSetupDecisionAsync()` + doğru `SistemKurulumViewModel` + `LoadMainApplicationAsync` sadeleştirildi. `SistemKurulumViewModel:77 GuncellemeModu` ile uyumlu, splash otomatik migrate yapmaz — kullanıcı `Güncellemeyi Uygula` ile onaylar (`SistemMigrationManager:153` backup-önce-migrate-rollback) ✅ Eklendi 🧪 Test edildi
- [x] **Login küçük ekran + hover static renk (2026-08-24):** `LoginView.xaml:13` `VisualStateGroup` `Tall 720→Center` / `Short→Top` + `DesignTokens.xaml:15` `MuhasibPrimaryPressedColor #0C3B5E→#0B4A8A` + `NamePasswordControl.xaml:142` `MuhasibPrimaryButtonStyle` static tokenler ile tutarlı hover/pressed — `dotnet build 0 hata` ✅ Eklendi 🧪 Test edildi
- [x] **FirmaShell god-class önlendi (2026-08-24):** `FirmaShellViewModel.cs:13` `FirmalarViewModel` kalıtımı → `ViewModelBase` + `FirmalarVM/MaliDonemVM` composition; `Firmalar`/`MaliDönemler` ayrı UserControl’ler (`FirmalarList`/`FirmaMaliDonemler`) korunuyor, kritik saga’lar (`Firma oluşturma` → `FirmaDetailsViewModel`, `MaliDönem oluşturma/silme` → `MaliDonemDetailsViewModel`/`MaliDonemListViewModel` via `IMaliDonemSagaManager`) kendi sınıflarında, `FirmaShell` sadece `SelectedFirma/SelectedMaliDonem/HasSelection/DevamEt/SwitchToTenant` + `LocalSettings` son seçimden sorumlu ✅ Eklendi 🧪 Test edildi
- [x] **UpdateService birleşik tasarım (DB migration + Velopack) (2026-08-26):** `Services/UIService/UpdateService.cs:12` `IUpdateService` `Scoped` (`ILocalSettingsService + ISistemDatabaseService/SistemMigrationManager + ILogger + Velopack`) — `Get/SaveSettings` `LocalSettings`, `CheckForUpdates/Download` Velopack stub (`null`/`false`, `LastCheckTime` güncellenir, `MUHASIBPRO_UPDATE_SOURCE` GithubSource hazır), `Apply` `VelopackApp.Build().Run()`, `PrepareForUpdateAsync` `GetState CanConnect`, `PostUpdateDatabaseSyncAsync` `InitializeSistemDatabaseAsync` backup-önce-migrate; `ExtendedSplash ShouldRedirectToSetupAsync` ve `UpdateViewModel CheckNow/Download/Install` aynı `IUpdateService`’i kullanacak — tekrar kod yok ✅ Eklendi 🧪 Test edildi
- [x] `MainShell` NavigationView token uyumu — `MainShellView.xaml:1` `TitleBarControl + NavSidebarControl 240 + Breadcrumb 52 + Snapshot PrimaryLight` ile webToWinui3 stiline taşındı (Oturum 31) — *FirmaShell yanında sade NavigationView, ayrı faz değil* ✅ Eklendi 🧪 Test edildi
- [ ] `MuhasibPro.slnx` → `MuhasibPro.sln` klasik formata çevir (opsiyonel) ⬜
- [ ] **E2E canlı test (etkileşimli):** `Yeni Firma → Yeni Dönem Saga (pipeline dialog canlı 5 adım) → kart seçimi → DevamEt → MainShell` + silme guard dialog — Oturum 51 smoke test OK, etkileşimli akış kullanıcı doğrulaması bekliyor ⬜
- [x] **`FirmaShellViewModel : FirmalarViewModel` kalıtımı** — AGENTS kural 1 ihlali (composition'a çevrildi — Oturum 63) ✅ Eklendi 🧪 Test edildi
- [ ] **Mapster** ile AutoMapper `NU1903` kalıcı temizliği (Faz B öncesi; `AutoMapperSistemMapping.cs` 5 CreateMap → manual/Mapster) ⬜

---

## Faz 6.25 — FirmaShell seçim onarımı + OOBE yerleşim + arşiv (2026-09-02, ✅ eklendi 🧪 build 0 hata)
- [x] **Seçim desync fix (kök neden):** `FirmalarListControl` + `MaliDonemlerListControl` → `SelectionMode="Single"` + `SelectedItem="{Binding X.SelectedItem, Mode=TwoWay}"`; `IsItemClickEnabled`/`ItemClick`/manuel `Selected` bayrak senkronu kaldırıldı — native tek-seçim + `VisualStateManager` (Selected: `MuhasibPrimaryLightBrush` zemin + `MuhasibPrimaryBrush` 1.5px + sağ-üst rozet; Normal/PointerOver). İki dönemde de rozet görünme sorunu giderildi ✅ Eklendi 🧪 Test edildi
- [x] **Dönem sayısı (0 Dönem) fix:** `FirmaRepository.GetFirmalarAsync` projeksiyonuna `MaliDonemler = r.MaliDonemler` + `FirmaService.GetFirmalarPageAsync` hafif `MaliDonemModel` projeksiyonu ✅ Eklendi 🧪 Test edildi
- [x] **Temiz model (skalar yerine DB-seçim modeli):** `MaliDonemModel.TenantDetails` (TenantDetailsModel) + `TenantDetailsModel` Durum/ArsivlendiMi/DosyaBoyutu/SonYedekTarihi/ArsivliMi + `MaliDonemServiceExtensions` map'i; `OnArsivleClick`→`UpdateMaliDonemAsync(Durum=Arsivlenmis)` + bildirim + refresh; `ArsivliMi/KapaliMi/BoyutMetni/SonYedekMetni` null-güvenli ✅ Eklendi 🧪 Test edildi
- [x] **Yerleşim (image + OOBE Tek Kaynak):** Üst bilgi barı "MULTİ-TENANT FİRMA & MALİ DÖNEM SEÇİCİ / Çalışma Ortamı ve Veritabanı Seçimi" + sağda `+ Yeni Firma Tanımla` (Aşama pill kaldırıldı; `OnYeniFirmaClick` FirmaShellView code-behind'e); FirmalarList (KAYITLI FİRMALAR başlıkta buton yok + `FirmaNo` gradyan tile + arama + mavi `N Dönem`/`>` ok — rozet/kalem kaldırıldı); MaliDonemler (seçili-firma detay şeridi + `MALİ DÖNEMLER & SQLITE VERİTABANLARI` + `Toplam N Mali Yıl` + `ItemsWrapGrid` 2-kol dönem kartı: `Veritabanı+boyut`/`Tablo/Kayıt`/`Son Yedek` + Yedek/Arşivle/Sil; `+ Yeni Mali Dönem Aç` → `MuhasibOutlineButtonStyle` outline) ✅ Eklendi 🧪 Test edildi
- [x] **Durum pill:** seçili=`Aktif Seçim` (Primary), değilse `KAPALI`(arşiv/warning)/`AÇIK`(success) — seçili kartta çift pill görünmemesi için durum grubu seçiliyken Collapsed ✅ Eklendi 🧪 Test edildi
- [x] **Alt bar kaldırıldı + "Çalışma Alanına Geç" sağ karta:** MaliDonemlerListControl "Seçim anında aktif döneme bağlanılır..." caption silindi, buton (`DevamEtCommand`/`HasSelection`/`IsFirmaSelected`) sağ kartın altına taşındı; FirmaShellView **en alt footer barı tamamen silindi** (`FooterShadow` referansı da) ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı görsel kullanıcı onayı:** dönem sayısı + `Boyut/Son Yedek` (TenantDetails değerleri) + `Arşivle` aksiyonu — UI automation login tıklaması pencere-çokluğu yüzünden esmedi (koordinat/UIA bounding uyuşmazlığı), kullanıcıda son kontrol ⬜
- [x] **Kart vitrini gerçek zamanlı fix — Yedek→SonYedek + Boyut (Oturum 71):** Kök neden: `OnBackupClick` yedek sonrası sadece bellek içi `TenantDetails.SonYedekTarihi` atayıp `RefreshAsync()` çağırıyordu — Global.db satırı hiç güncellenmiyordu (arşiv `UpdateMaliDonemAsync` ile DB'ye yazıyordu, Yedek'te yoktu). Ayrıca yeni dönem akışı satıra `DosyaBoyutu` yazmıyordu. Fix: `OnBackupClick` → `SonYedekTarihi=now` + `DosyaBoyutu=BackupFileSizeBytes` → `IMaliDonemService.UpdateMaliDonemAsync` DB'ye yaz → refresh; `TenantSQLiteBackupManager` başarıda `BackupFileSizeBytes` (kaynak boyutu); `TenantSQLiteDatabaseService` DB dosyası oluşunca `PersistTenantFileSizeAsync` ile satıra boyut. Kullanıcı kararı: kapsam SonYedek+Boyut, kaynak DB satırı ("Tablo/Kayıt" ayrı). Build 0 hata, test 73/73 ✅ Eklendi 🧪 Test edildi (canlı görsel doğrulama kullanıcıda)
- [ ] **SagaPipeline/YeniDonem `RequestedTheme="Light"`** (Oturum 68 ince ayar hâlâ bekliyor) ⬜
- `dotnet build MuhasibPro.slnx` **0 Hata**
- [x] **OS Toast bildirimleri — CommunityToolkit (Oturum 59, tek kanal):**

---

## Faz 6.26 — Kapat onayı fix + ortak ShellTitleBar refactor (2026-09-02, ✅ eklendi 🧪 build 0 hata)
- [x] **Kapat onayı kök neden fix:** Özel title-bar X `window.Close()` → `AppWindow.Closing` tetiklemiyor (WinUI3; `AppWindow.Close()` yok) → kapat onayı atlanıyordu. `WindowHelper.RequestCloseAsync(Window)` tek kaynak: `OnMainWindowClosing` (system X / Alt+F4) + özel X delege; onay sonrası pencere + diğerleri kapatılıp `Application.Current.Exit()`. (FirmaShellView + SistemKurulumView `OnCloseClick` → `RequestCloseAsync`) ✅ Eklendi 🧪 Test edildi
- [x] **Ortak `Views/Components/ShellTitleBar` UserControl:** logo tile (BloomBlue→Sky + AppIcon) + `Title`/`Subtitle` DP + `TrailingContent` (ContentPresenter) + pencere kontrolleri (min/max/close→`RequestCloseAsync`); `Loaded`'da `SetTitleBar` + `AppWindow.Changed→UpdateMaximizeIcon` + gölge receiver (ilk `UIElement` atası). Titlebar her view'da kopyalanmıyor ✅ Eklendi 🧪 Test edildi
- [x] **FirmaShellView + SistemKurulumView titlebar kopyası silindi:** `SetupCustomTitleBar`/`UpdateMaximizeIcon`/`OnMinimizeClick`/`OnMaximizeClick`/`OnCloseClick`/`ShowWindow` P/Invoke + `HeaderShadow` receiver + 3 ölü stil (`HeaderTitleText`/`CaptionButtonStyle`/`CaptionCloseButtonStyle`) + artık gereksiz `using Microsoft.UI.Windowing` kaldırıldı; SistemKurulum `Loaded=OnRootLoaded` kaldırıldı. SistemKurulum `TrailingContent` = durum pill + "Giriş Ekranına Git" ({Binding StatusMessage/GoToLoginCommand/IsKurulumTamamlandi}, DataContext `{x:Bind ViewModel}`). `TitleBarHelper.UpdateTitleBar` tema çağrısı korundu ✅ Eklendi 🧪 Test edildi
- [x] **Kapat onayı canlı doğrulaması:** sistem X + özel X `WindowHelper.RequestCloseAsync` tek kaynak — UIA testi sistem `WindowPattern.Close` + özel `InvokePattern` (`Close` Aid) sonrası `Evet`/`İptal` dialogu ve `Window still there` ile doğrulandı (2026-09-08, 41→48 descendants, `PrimaryButton=Evet`/`SecondaryButton=İptal`) ✅ Eklendi 🧪 Test edildi
- `dotnet build MuhasibPro.slnx` **0 Hata** (önce `AppWindow.Close()` CS1061 → `RequestCloseAsync`; `ThemeShadow.Receivers` `UIElement` istiyor → `GetAncestors` `UIElement` filtresi; App.xaml converter "Unknown type" hataları bu CS kaskadı)
- [x] **OS Toast bildirimleri — CommunityToolkit (Oturum 59, tek kanal):** Kanal ayrımı: karar→`DialogService`, **işlem sonucu→OS toast**, form bağlamı→satır içi, global durum→StatusBar. `CommunityToolkit.WinUI.Notifications` **7.1.2** eklendi (WASDK 2.4.0 uyumlu). `INotificationService` kontratı (Business) + `NotificationService` (App): unpacked AUMID `MuhasibProSoft.MuhasibPro` registry (`HKCU\Software\Classes\AppUserModelId\...`) + Start Menu shortcut (WScript.Shell + IPropertyStore `PKEY_AppUserModel_ID`), gösterim `ToastNotificationManager.CreateToastNotifier(Aumid)` + `ToastContentBuilder` (toolkit `RegisterAumidAndComServer<T>` generic COM activator istediği için ham notifier deseni). DI Singleton + `App.OnLaunched` Initialize (try/catch). Re-wire: GenericDetailsViewModel (Success yeni/kaydedildi + Danger doğrulama), SagaPipeline/YeniFirma/DeleteGuard (Success), MaliDonemlerListControl yedek (Success/Warning/Danger). Satır içi istisnalar korundu (Beni hatırla AnimatedInfoBorder, Teşhis yedek buton ring). Build 0 hata, test 73/73, smoke + pencere görünür + REG OK + LNK True ✅ Eklendi 🧪 Test edildi

---

## Faz 6.10 — Yeni viewpackage renk stili: Splash + Login (2026-08-23, ✅ eklendi 🧪 görsel doğrulandı)
- [x] `ExtendedSplash`: koyu gradient → açık zemin + radial accent `#1A0F6CBD`; primary kart logo + `IconAppLogo`; ThemeResource metinler; pill açık kart; progress track kaldırıldı; footer tertiary ✅ Eklendi 🧪 Test edildi
- [x] `LoginView`: kök radial accent; üst şerit açık; sol banner şeffaf + Bloom elipsler + koyu metin; kart radius 12 + primary circle logo; başlık "Kullanıcı Oturumu Açın"; viewpackage footer metni ✅ Eklendi 🧪 Test edildi
- [x] `NamePasswordControl`: ThemeResource label/border, focused border `#FF0F6CBD`, Giriş Yap primary/hover `#FF115EA3`/disabled `MuhasibInfoBgColor`, Hello `IconFingerprint` ✅ Eklendi 🧪 Test edildi
- [x] `QuickLoginPanel`: açık tema (beyaz kart + `MuhasibPrimaryLightBrush` satırlar + primary header) ✅ Eklendi 🧪 Test edildi
- [x] TitleBar Login `Dark`→`Light` ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build MuhasibPro.slnx --nologo` 0 hata (PRI263 hariç) — fonksiyonel binding'ler (CanLogin/reveal/QuickLogin/progress) korunudu ✅ Eklendi 🧪 Test edildi
- [x] `ThemeSelectorService.LoadThemeFromSettingsAsync` fallback `Default`→`Light` (kayıtlı tema yoksa viewpackage açık teması; XAML'de tema zorlaması YOK) ✅ Eklendi 🧪 Test edildi
- [x] Uygulama içi görsel doğrulama — UIA + screenshot: Splash açık tema (radial accent, primary logo, progress %), Login açık tema (kart, alanlar okunur, Giriş Yap primary, Hello, footer), QuickLoginPanel açık kart + `MuhasibPrimaryLightBrush` satır + primary header, TitleBar Light koyu butonlar ✅ Eklendi 🧪 Test edildi

---

## Faz 6.11 — FirmaShell god-class önlendi + sınıflar düzenlendi (2026-08-24, ✅ eklendi 🧪 doğrulandı)
- [x] `FirmaShellViewModel.cs:13` `FirmalarViewModel` kalıtımı kaldırıldı → `ViewModelBase` + `FirmalarVM: FirmalarViewModel` ve `MaliDonemVM: MaliDonemViewModel` composition; `FirmaList/FirmaDetails/MaliDonemList/MaliDonemDetails` expose ile geriye uyum, `LoadAsync` → `FirmalarVM.LoadAsync`, `OnItemSelected` → `FirmalarVM.OnItemSelected`, `BaseUnsubscribe` → `FirmalarVM/MaliDonemVM` ayrı, `MessageService` ayrı — god-class önlendi ✅ Eklendi 🧪 Test edildi
- [x] `Firmalar`/`MaliDönemler` ayrı UserControl’ler olarak korundu: `Views/Firmalar/List/FirmalarList.xaml:12` (`DataList` + header `Kimlik/Kısa Ünvan…`) ve `Views/Firmalar/Details/FirmaMaliDonemler.xaml` (`MaliDönem` listesi) — `FirmaShellView.xaml:110` bunları host ediyor, kritik saga’lar kendi VM’lerinde (`MaliDonemDetailsViewModel` → `IMaliDonemSagaManager` 5 adım, `MaliDonemListViewModel` → silme saga) kaybolmuyor ✅ Eklendi 🧪 Test edildi
- [x] **Kıyas notu:** `FirmaShell` çekirdeğin kalbi (firma/mali dönem seçimi + saga), `MainShell` yanında ona kıyasla sade `NavigationView` (`MainShellViewModel`/`MainMenuViewModel` sadece menü, `ShellView.xaml:11` sadece `TitleBar/Frame`) — ayrı önemli faz değil, kıyas için söylendi ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build MuhasibPro.slnx --nologo` 0 uyarı 0 hata — `FirmaShell` `DevamEt` + `MaliDönem` saga akışı korunuyor ✅ Eklendi 🧪 Test edildi

---

## Faz 6.12 — FirmaShellView yeni tasarım — viewpackage kart mimarisi (2026-08-24, ✅ eklendi 🧪 build 0/0)
- [x] `FirmaShellView.xaml` komple yeni tasarım: radial accent + Bloom elipsler; header primary-light IconBuilding badge + kullanıcı pill'i (`PersonPicture`+ad/rol/e-posta); SOL Firmalar / SAĞ Mali Dönemler `MuhasibCardStyle` kartları (monogram header + sayaç/"Hazır" pill); bottom bar seçim özeti daire ikon + chip'ler + yeşil `SuccessButtonStyle` Devam Et + Çıkış Yap ✅ Eklendi 🧪 Test edildi
- [x] Tüm `x:Bind` fonksiyonalitesi korundu (UserInfo/IsBusy/FirmaList/MaliDonemList/IsFirmaSelected/HasSelection/SecimOzeti/DevamEtCommand) — fonksiyon ↔ stil ayrımı ✅ Eklendi 🧪 Test edildi
- [x] `FirmaMaliDonemler` item template: yıl bold + `AktifMi` AKTİF(success)/KAPALI(warning+IconLock) pill + DatabaseName monospace — DataList 20 binding aynen ✅ Eklendi 🧪 Test edildi
- [x] `FirmalarList` monogram: PersonPicture primary-light circle + Kısa Ünvan SemiBold ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build MuhasibPro.slnx --nologo` 0 hata (PRI263 bilinen) ✅ Eklendi 🧪 Test edildi
- [x] Canlı akış görsel testi: Login → FirmaShell (Bloom kartlar) UIA/screenshot ✅ Eklendi 🧪 Test edildi (Faz 6.12 kapatıldı)

---

## Faz 6.13 — FirmaShellControls kart kontrolleri + canlı saga testi (2026-08-24, ✅ eklendi 🧪 build 0/0)
- [x] `Views/FirmaShell/Controls/`: `MaliDonemKart` (AKTİF/KAPALI pill + Gir/İncele), `FirmaKart` (monogram + meta + dönem kartları + Yeni Dönem Aç), `FirmaKartListesi` (ItemsControl + boş durum + Tag/visual-tree dispatch) — viewpackage kart mimarisi, DataList tablo kontrolleri bu ekrandan çekildi ✅ Eklendi 🧪 Test edildi
- [x] `FirmaListArgs.TumunuYukle` + `GetFirmalarWithMaliDonemler` repo düzeltmesi (ölü Include kaldırıldı, AktifMi/FirmaId/Il/VergiNo projeksiyon, KAPALI dönemler görünür) + `FirmaService` map ✅ Eklendi 🧪 Test edildi
- [x] `FirmaShellViewModel`: `DonemSecCommand` (+`_bekleyenDonem` auto-select ezme), `YeniDonemCommand` (saga DetailsWindow) — mevcut `ItemSelected` mesaj akışı korundu ✅ Eklendi 🧪 Test edildi
- [x] Canlı test: Login→kartlar✅, Firma Bulunamadı dialog✅, Yeni Firma saga→kart✅, Yeni Dönem Aç→saga penceresi✅, Guard "DB adı kullanılıyor"✅, hata rollback (tenant dosyası silindi)✅ — `dotnet build` **0 uyarı 0 hata** ✅ Eklendi 🧪 Test edildi
- [x] **✅ SAGA/LOG DI FIX (2026-08-25):** Global.db `database is locked` captive dependency + WAL + çift transaction düzeltildi — `AddServicesHostBuilderExtensions.cs:22` Firma/MaliDonem/Log **Singleton→Scoped**, `AddCommonServiceHostBuilderExtensions.cs:26` duplicate Log **Singleton→Scoped**, `MaliDonemSagaStep.cs:45` fazla `BeginTransaction+SaveChanges` kaldırıldı (servis tek yazım), `AddDbManagerHostBuilderExtensions.cs:34` Global.db `PRAGMA journal_mode=WAL` + `App.xaml.cs:154` root `CreateScope` ile Scoped Log — `dotnet build` **0/0** ✅ Eklendi 🧪 Test edildi; E2E dönem oluşturma→kart seçimi→DevamEt test bekliyor ⬜

---

## Faz 6.14 — MaliDonemDetails sadeleştirme + progress birleştirme (2026-08-25, ✅ eklendi 🧪 build 0/0)
- [x] `MaliDonemDetails.xaml` sadeleştirme: `GlassPanel→MuhasibCardStyle`, `ElevatedCard→CardBackgroundFillColorSecondaryBrush`, hardcode gradient kaldırıldı, 48px `MuhasibPrimaryLightBrush` monogram + `IconBuilding` (04-FirmaShell viewpackage referans), `NumberBox` 36pt `MuhasibPrimaryBrush`, 3 adım sade liste, `MuhasibWarningBgBrush` uyarı, alt bar `MuhasibPrimaryHover/Pressed` token — **523→216 satır (-59%)**, tüm `x:Bind` korundu ✅ Eklendi 🧪 Test edildi
- [x] `MaliDonemProgressView` birleşik UserControl (97+88 satır): `Title/IsActive/Message/Progress/Steps/IsCompleted/HasError/Success/Error/CloseCommand` DP'li tek kontrol; `ProgressBar MuhasibPrimaryBrush` (gradient ` #0078D4→#4CC2FF` kaldırıldı), `ListView` converter'sız `Binding Message/Step/Status/Duration` — `MaliDonemOlusturProgress` (236) + `MaliDonemSilProgress` (236) **duplicate kaldırıldı**, `MaliDonemDetails.xaml:210` iki overlay bu tek kontrole bind edildi ✅ Eklendi 🧪 Test edildi
- [x] Gereksiz kaldırıldı: `MaliDonemCard.xaml` (183 satır, 0 referans, `ElevatedCard/InfoLabel` eski stil) silindi; `MaliDonemView.xaml` (19 satır wrapper) korundu ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build MuhasibPro.slnx --nologo` **0 hata** (PRI263 hariç) — `Viewpackage 04-FirmaShell + 00 DesignTokens` referans korunarak, toplam **1178→403 satır (-66%)** sadeleştirme ✅ Eklendi 🧪 Test edildi

> **GENEL KURAL (AGENTS.md Kurallar'da kesin dille):** GOD-CLASS YASAK — tüm sınıflar için geçerli; tek sorumluluk zorunlu, VM kalıtım ile birleştirme yasak, composition kullanılacak; ihlal build hatası gibi ele alınır, ihlal edilen faz kapatılmaz. **XAML MODÜLERLİĞİ:** ezbere yüzlerce satır XAML yazılmayacak; tekrar eden/bağımsız sorumluluklu bloklar esneklik/kod analizi kazandırıyorsa UserControl'e bölünecek. **STİL TUTARLILIĞI:** DesignTokens TEK KAYNAK — istenen token yoksa ÖNCE DesignTokens'a eklenir; hardcode `#FF...` yasak (ThemeResource hariç).

> **🔒 ÇEKİRDEK KİLİDİ (2026-08-25 — LOG Oturum 25 kararı, kesin):** Sistem çekirdek yapısı **en az kusurda** ve **güncelleme altyapısı (IUpdateService: CheckForUpdates/Download/Apply + PrepareForUpdateAsync/PostUpdateDatabaseSyncAsync + SistemMigrationManager + Velopack) dahil** tamamlanmadan **Faz B (Cari/Stok/Fatura/Kasa/Banka/ÇekSenet/Sipariş/Personel/Rapor) açılmayacak**. Bir uygulama dışarıdan güncelleme almaya başlamışsa çekirdek tamamlanmış sayılır; sonrası yalnızca küçük fix/ek geliştirme. Aşama A `Splash→Kurulum→Login→FirmaShell→MaliDonem→MainShell` 0 kusur kapanmadan Faz B'ye geçilmeyecek.

---

## Faz 6.15 — SistemKurulum webToWinui3 stil taşıma — Splash⇒Kurulum akışının ilk adımı (2026-08-26, ✅ eklendi 🧪 0 hata)
- [x] `Views/SistemKurulum/Components/` 3 UserControl: `DiagnosticsMetricsGrid` (4 metrik `DbSize/DbExistsMetni/IsDbValidMetni/TableCount/DbVersion/Pending/DbDurumMetni/KullaniciSayisi/FirmaSayisi`), `DatabaseTablesList` (6 kart `Kullanicilar/Firmalar/MaliDonemler/KFR/Bekleyen/Veritabanı Durumu` + `TestSystemClassesCommand`), `DiagnosticsTerminalLog` (dark `#0F172A` terminal `Logs` ItemsControl) — `webToWinui3/Views/Setup` stilinden, `Binding` ile bizim VM — XAML MODÜLERLİĞİ ✅ Eklendi 🧪 Test edildi
- [x] `SistemKurulumView.xaml` Top Bar Navigation Card (`CardBackground 16` + dinamik `GuncellemeModu/IsKurulumTamamlandi/IsKurulumGerekli` pill + `Yeniden Test Et`/`Giriş Ekranına Dön` `TestSystemClassesCommand/GoToLoginCommand`) + `ScrollViewer StackPanel MaxWidth 1024 Spacing 16` içine: Sistem Durumu+Progress (`StatusMessage/ProgressValue/IsProgressVisible`) → MetricsGrid → DB 2-panel (Dosya `DbPath` + İşlemler `PrimaryButtonMetni/CanExecuteKurulum` + 6 mini-kart) → TablesList → TerminalLog (`Logs`) → Sistem Testleri (`TestSonuclari` ListView) — tüm `x:Bind` korundu ✅ Eklendi 🧪 Test edildi
- [x] Fonksiyonel fark: Top Bar ek navigasyon (duplicate buton), 4 metrik ek görsel katman, TablesList şema özeti, Log dark terminal stil (binding aynı), Log+Test yan yana → alt alta — ek veri kaynağı yok, kırılım yok ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build --nologo` **0 hata** (PRI263 hariç) — `LOG Oturum 28` fonksiyonel fark raporu eklendi ✅ Eklendi 🧪 Test edildi

---

## Faz 6.16 — Login webToWinui3 stil taşıma — akışın ikinci adımı (2026-08-26, ✅ eklendi 🧪 0 hata)
- [x] `Views/Login/BrandBannerControl.xaml` + `.xaml.cs` yeni (chip `Sürüm 2.4.0` + `32 Bold` + 2 kart `Multi-Database/VACUUM`) — `webToWinui3/Views/Auth` stil birebir ✅ Eklendi 🧪 Test edildi
- [x] `Views/Login/QuickLoginPanel.xaml` header `11 Bold 50 Tertiary HIZLI KULLANICI SEÇİMİ` + item `28 Initial #DBEAFE` + `DisplayName 12 Bold / RoleName 10` + `@handle pill Consolas 10` + `Remove ×` — dinamik `RememberedAccounts/OnAccountClick/OnRemoveClick` korundu ✅ Eklendi 🧪 Test edildi
- [x] `Libraries/MuhasibPro.ViewModels/ViewModels/Shell/QuickLoginAccountsViewModel.cs:11` `Initial` computed eklendi ✅ Eklendi 🧪 Test edildi
- [x] `Views/Login/NamePasswordControl.xaml` `Spacing 16` `CornerRadius 8` `Hello SubtleFill` — `reveal E052↔E7B3` + `CanLogin/ErrorMessage/Hello` korundu ✅ Eklendi 🧪 Test edildi
- [x] `Views/Login/LoginView.xaml` `ScrollViewer Grid MaxWidth 960 3 kolon *|36|*` `VisualState Wide/Narrow + Tall/Short` + `Border Card 16 ThemeShadow` kart içinde: header `44 Primary M` + `Kullanıcı Oturumu 18` + `E713 Kurulum OnSistemKurulumClick` → `NamePasswordControl x:Bind` → `QuickLoginPanel` (kart içine taşındı) → footer `WAL•PBKDF2` — tüm `x:Bind` korundu ✅ Eklendi 🧪 Test edildi
- [x] Fonksiyonel fark: `Grid 420/* Bloom → 960 Centered 3-kolon + ThemeShadow`, `QuickLoginPanel LeftBanner→Kart içi`, header `IconLock 52→M 44`, `QuickLogin` header/item `Consolas @handle` — `reveal/Hello/CanLogin/DB guard/Enter` kırılım yok ✅ Eklendi 🧪 Test edildi
- [x] `Styles/DesignTokens.xaml:16` `MuhasibPrimaryBorderColor/BorderBrush` eklendi ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build --nologo` **0 hata** (PRI263 hariç) — `LOG Oturum 29` fonksiyonel fark raporu eklendi ✅ Eklendi 🧪 Test edildi

---

## Faz 6.17 — FirmaShell webToWinui3 stil taşıma — akışın üçüncü adımı (2026-08-26, ✅ eklendi 🧪 0 hata)
- [x] `Views/ShellViews/Shell/Components/FirmalarListControl.xaml` + `.xaml.cs` — `Tanımlı Şirketler 14 + SubtleFill Count` + `Yeni Firma Primary` + `ItemsControl FirmaList.Items` `36 DBEAFE` kart — `webToWinui3/Views/Firmalar` stil birebir ✅ Eklendi 🧪 Test edildi
- [x] `Views/ShellViews/Shell/Components/MaliDonemlerListControl.xaml` + `.xaml.cs` — `Mali Dönemler 14 + PrimaryLight Count` + `Yeni Dönem Aç (Saga) PrimaryLight` + `ItemsControl MaliDonemList.Items` `22 MaliYil` `KAPALI #FEF3C7` `DatabaseName Consolas` — `webToWinui3` stil birebir ✅ Eklendi 🧪 Test edildi
- [x] `Views/ShellViews/Shell/Components/SagaPipelineDialog.xaml` + `.xaml.cs` ContentDialog 5 adım (`Guard→DDL→69 Migration→Global.db→SHA-256` + NumberBox 2027) — `webToWinui3` stil birebir ✅ Eklendi 🧪 Test edildi
- [x] `Views/ShellViews/Shell/FirmaShellView.xaml` `SolidBackground 1200` header `44 Primary IconBuilding + SubtleFill pill Ö` + `ScrollViewer 5*|24|7*` `FirmalarListControl` sol + `MaliDonemlerListControl` sağ + `FirmaKartListesi` fallback (korundu) + `IsBusy ProgressRing` + Bottom Bar `32 #D1FAE5 SecimOzeti + Çıkış/Devam Et #059669 HasSelection` — radial kaldırıldı, tüm `x:Bind` korundu ✅ Eklendi 🧪 Test edildi
- [x] `Views/ShellViews/Shell/FirmaShellView.xaml.cs:41` `OnLogoutClick` (`INavigationService` `LoginView`) eklendi ✅ Eklendi 🧪 Test edildi
- [x] Fonksiyonel fark: `radial Bloom → SolidBackground`, iç-içe `FirmaKartListesi (TumunuYukle)` → yan yana `Firmalar|MaliDonemler` 2-panel + fallback kart, PersonPicture/Email → sade monogram, PersonPicture kaldırıldı — `HasSelection/SecimOzeti/DonemSec/YeniDonem/Delete/DevamEt/SwitchToTenant` kırılım yok ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build --nologo` **0 hata** (PRI263 hariç) — `LOG Oturum 30` fonksiyonel fark raporu eklendi ✅ Eklendi 🧪 Test edildi

---

## Faz 6.18 — MainShell webToWinui3 stil taşıma — akışın son adımı (2026-08-26, ✅ eklendi 🧪 0 hata)
- [x] `Views/MainShell/Components/TitleBarControl.xaml` + `.xaml.cs` — `20×20 Primary M + MuhasibPro 13 Bold + Divider + Firma/Dönem AKTİF #D1FAE5 + SQLite Multi-Tenant Consolas + WAL Aktif + E713` — `webToWinui3/Views/Common` stil birebir ✅ Eklendi 🧪 Test edildi
- [x] `Views/MainShell/Components/NavSidebarControl.xaml` + `.xaml.cs` `Width 240 LayerFill 1px` — üst `32 #DBEAFE + Firma/Dönem 12/10 + #059669 dot` + 10 modül `Button 10,8 Corner 8 (Genel Bakış Primary / 9 Transparent + RBAC #DBEAFE)` + alt `SubtleFill 10 + Ö + Logout F3B1` — `webToWinui3` stil birebir ✅ Eklendi 🧪 Test edildi
- [x] `Views/MainShell/MainShellView.xaml` `SolidBackground + TitleBarControl Row0` + `NavSidebar 240 + Grid 52 LayerFill Breadcrumb Genel Bakış / ÖRNEK + Snapshot Yedek PrimaryLight E8D7` + `Grid Padding 24` içinde tenant header `MuhasibCardStyle 40 PrimaryLight CheckMark` (korundu) + `NavigationView Collapsed` + `Frame ContentFrame` placeholder — tüm `ViewModel` korundu ✅ Eklendi 🧪 Test edildi
- [x] Fonksiyonel fark: `Grid 24 NavigationView 7 item → Solid + TitleBar + NavSidebar 10 buton + Breadcrumb/Snapshot + tenant header` — `NavigationView 220 Left` Collapsed korundu, `NavigationItems/NavigateTo/IsPaneOpen/SelectedItem` kırılım yok; `Snapshot Yedek` stub ✅ Eklendi 🧪 Test edildi
- [x] `dotnet build --nologo` **0 hata** (PRI263 hariç) — `LOG Oturum 31` fonksiyonel fark raporu eklendi ✅ Eklendi 🧪 Test edildi

---

## Faz 6.20 — Business DB birim testleri + 5 bug fix (2026-08-28, ✅ eklendi 🧪 62/62 test, build 0 hata)
- [x] `Libraries/MuhasibPro.Tests/BusinessDatabaseTests.cs` yeni — 41 test (Moq + FluentAssertions): `TenantHelperExtensions` (`ValidateMaliYil` 4 / `ValidateMaliDonemExistsAsync` 3 / `GenerateDatabaseName` 6 / `ValidateFirmaAsync` 5), `TenantSQLiteSelectionService` (`SwitchTenantAsync` 5 + `TenantChanged` olay yayını + `DisconnectCurrentTenantAsync` 2), `TenantSQLiteDatabaseSelectedDetailService` (`GetTenantDetailsAsync` 5 — **`Success=false+Data!=null` regresyon testi dahil** / `GetUserTenantsForSelectionAsync` 4) ✅ Eklendi 🧪 Test edildi
- [x] **5 bug fix:** `TenantSQLiteDatabaseSelectedDetailService.cs:39` `&&`→`||` (+ ikinci kontrol ve varışsız return = ölü kod, silindi); `TenantHelperExtensions.cs:69` sarkan boş `;` silindi; `TenantSQLiteSelectionService.cs:161` ölü ternary kaldırıldı; `DatabaseUtilityExtensionsHelper.cs:10` `ToUpper()`→`ToUpperInvariant()` (Türkçe kültürde `firma01→FİRMA01` — DB adı kültürden bağımsız hale geldi, test FAIL ederek yakaladı) ✅ Eklendi 🧪 Test edildi
- [x] `GenerateTenantDatabaseName` `db-_` çift ayraç düzeltildi → `db-FIRMA01_2027` (kullanıcı onayı: "şu an hiçbir DB yok, düzeltelim"; test artık tam `Be("db-FIRMA01_2027")` assert ediyor) ✅ Eklendi 🧪 Test edildi
- [x] `dotnet test` **62/62** (21 eski + 41 yeni), `dotnet build MuhasibPro.slnx` **0 hata** (NU1903 x2 bilinen, Mapster bekliyor) ✅ Eklendi 🧪 Test edildi

---

## Faz 6.22 — SistemKurulumView final rötüş + Login yönlendirme (2026-08-30, ✅ eklendi 🧪 0 hata/73 test)

- [x] **Status bar sabit:** `SistemKurulumView.xaml:29` `Border CardBackgroundFillColorDefaultBrush + 0,0,0,1` — `Grid RowDefinitions Auto/*` ile `Row 0` Scroll dışında sabit, `Row 1` `ScrollViewer` içinde log dahil kayıyor ✅ Eklendi 🧪 Test edildi
- [x] **Log scroll içine + hover kaldırıldı:** `SistemKurulumView.xaml:93` `ScrollViewer > StackPanel MaxWidth960` log sabit footer değil içerik; `SistemKurulumView.xaml:118` `TextBox PointerOver` → `Border TerminalBg + ScrollViewer + TextBlock IsTextSelectionEnabled` hover'sız terminal ✅ Eklendi 🧪 Test edildi
- [x] **Buton Status'a taşındı:** `SistemKurulumView.xaml:74` `GoToLoginCommand` `IsEnabled` → `Visibility IsKurulumTamamlandi Converter TrueToVis` — DB kurulmadan `Collapsed`, `IsKurulumTamamlandi` sonrası `Visible` Status çubuğunda ✅ Eklendi 🧪 Test edildi
- [x] **Login yönlendirme tamamlandı:** `ExtendedSplash.xaml.cs:207` + `SistemKurulumViewModel.cs:152 GoToLoginAsync RefreshDbStateAsync + IsKurulumTamamlandi guard` + `LoginView CanLogin DbIsReady` ile `Splash → SistemKurulum → Login` başarıyla doğrulandı; `dotnet build 0 Hata 2 Uyarı (NU1903)` ✅ Eklendi 🧪 Test edildi

---

## Faz 6.23 — DesignTokens Light+Dark + Splash kart + SistemKurulum sadeleştirme (2026-08-30, ✅ eklendi 🧪 73/73 test, build 0 hata)

- [x] **DesignTokens temiz yeniden başlangıç + Dark tema:** ThemeDictionaries sadece `Light`+`Dark` (ölü `Default` dict + `MuhasibOobeGradientStart/EndColor` + eski `MuhasibOobeBackgroundBrush` silindi). Tüm semantic renkler (yüzey/kar/metin/border/brand/durum) dict'lere taşındı; fırçalar `{ThemeResource}` ile tema-reaktif. `AppBackgroundBrush` (Light `#E8ECFB/#DCE3F7/#E9E4F5` 3-stop gradient + Dark karşılığı) ✅ Eklendi 🧪 Test edildi
- [x] **Sweep:** 9 ham `StaticResource Muhasib*Color` → ThemeResource (ExtendedSplash/QuickLoginPanel/NamePasswordControl/BrandBanner); `MuhasibOobeBackgroundBrush` → `AppBackgroundBrush` (4 view) ✅ Eklendi 🧪 Test edildi
- [x] **Splash v2-tarzı kart:** Ortalanmış 460px gölgeli kart (CornerRadius 20, `MuhasibCardElevatedStyle` + inline ThemeShadow + Translation + `Receivers.Add(SplashOverlay)`); 72px logo + progress% + fade durum + badge + sürüm korundu; Bloom elipsler kaldırıldı ✅ Eklendi 🧪 Test edildi
- [x] **No-DB yönlendirme netleşti:** Splash `!dbReady` → `ShellArgs.Parameter=true` → `SetFirstSetupMode()` → "Veritabanı bulunamadı — kurulum gerekiyor" + tek tık "Kurulumu Başlat" ✅ Eklendi 🧪 Test edildi
- [x] **SistemKurulum güncelleme-migration kaldırıldı (sadece ilk kurulum):** `SistemDatabaseStatusViewModel` `GuncellemeModu/IsDbGuncel/PendingMigrationCount/PrimaryButtonMetni/DbDurumIcon` silindi, `IsKurulumTamamlandi => DbExists && IsDbValid`; `SistemDatabaseCreationViewModel.ExecuteAsync` param'sız; `SistemKurulumViewModel` delegasyonlar sadeleşti; `DatabaseInfoPanel` `IsDbGuncel`→`IsKurulumTamamlandi`. **`SistemDiagnosticsViewModel` testleri KORUNDU** (ileride yarar) ✅ Eklendi 🧪 Test edildi
- [x] Build `0 Hata` (NU1903 tek uyarı türü) + `dotnet test` **73/73** + başlatma smoke OK ✅ Eklendi 🧪 Test edildi
- [x] **⚠️ Splash pencere görünmez regresyon fix:** `SplashCardShadow.Receivers.Add(SplashOverlay)` ctor'dan `OnPageLoaded`'a taşındı + try/catch (visual tree bağlanmadan exception → `ActivationService` yutuyor → pencere `IsWindowVisible=False`). Win32 EnumWindows ile `gorunur=True` doğrulandı; tek-instance engelleyen takılı prosesler temizlendi; uygulama 35s+ stabil çalışıyor ✅ Eklendi 🧪 Test edildi
- [ ] **Görsel doğrulama (vision model/kullanıcı):** Yeni Splash kart + `AppBackgroundBrush` zemini + SistemKurulum net durum — screenshot: `C:\Users\Code\AppData\Local\Temp\opencode\splash_visible.png`, uygulama çalışır durumda ⬜

---

## Faz 6.24 — Görsel OOBE: yeni arkaplan + kartlar Cards.xaml'a taşındı + gölge + teşhis dialogu (2026-08-30, ✅ eklendi 🧪 73/73 test, build 0 hata)
- [ ] `MaliDonemlerListControl` database kapalı uyarısı (animasyonlu bildirim) + kart tasarım iyileştirmeleri — ✅🔨

- [x] **Yeni arkaplan görseli:** `Assets/Images/app_background.png` (1. resim) → `AppBackgroundBrush` artık **ImageBrush** (`ms-appx:///Assets/Images/app_background.png`, `Stretch=UniformToFill`); eski lineer/radial gradient kaldırıldı ✅ Eklendi 🧪 Test edildi
- [x] **DesignTokens kart stilleri temizliği:** Çalışmayan 3 stil silindi — `MuhasibCardElevatedStyle`, `GlassPanel`, `MuhasibAcrylicCardStyle` (kullanıcı: "senin oluşturduğun kartların hiçbiri çalışmıyor"). `Cards.xaml` stilleri çalışıyor; `App.xaml`da `Cards.xaml` sonra merge → kazanır ✅ Eklendi 🧪 Test edildi
- [x] **Kart stilleri tek kaynak Cards.xaml:** `MuhasibCardStyle` `DesignTokens`→`Cards.xaml`a taşındı (kullanıcı: "DesignToken şişmesin, kart stilleri cards.xaml'den"). `DesignTokens.xaml` artık **hiç `<Style>` içermiyor** ✅ Eklendi 🧪 Test edildi

---

## Faz 6.31 — Dialog Light garantisi kodda + XAML RequestedTheme yasağı (2026-09-03, ✅ eklendi 🧪 build 0 hata, test 73/73, canlı UIA doğrulandı)
- [x] **AGENTS kuralı (kullanıcı kararı):** XAML içinde tema zorlanamaz (`RequestedTheme` hardcode yasak — dialog dahil); `ContentDialog` Light garantisi kodda tek kaynak (`DialogHelper.ShowCenteredAsync` / `DialogService.CreateDialog`) ✅ Eklendi 🧪 Test edildi
- [x] **Kök neden 3 katman:** shared `Muhasib*Brush` + `StaticResource` app/sistem (Dark) temasına çözülüyordu (dialog Light olsa bile içerik Dark); `DialogHelper` servis temasıyla XAML Light'ı eziyordu; `OnDeleteClick`/`LoginView teşhis`/`ExtendedSplash` bildirimi helper'ı bypass ediyordu ✅ Eklendi 🧪 Test edildi
- [x] **Fix:** 8 XAML'den `RequestedTheme` silindi (6 dialog + 2 progress + `CustomContentDialog`); 6 dialogda `StaticResource Muhasib→ThemeResource`; `DialogHelper`/`DialogService` → `ElementTheme.Light` (ölü `_themeSelectorService` + `HostBuilders` using silindi); 3 bypass helper'a alındı ✅ Eklendi 🧪 Test edildi
- [x] **Canlı doğrulama (sistem Dark):** Login Light → FirmaShell Light → Sil → DeleteGuard beyaz Light (`Temp/opencode/ot74_deleteguard.png`, silme onaylanmadı ESC) ✅ Eklendi 🧪 Test edildi

---

## Faz 6.32 — MaliDonem kart: boyut/Kurtar/Sil/arşiv turu (2026-09-03, ✅ eklendi 🧪 build 0 hata, test 73/73, canlı UIA)
- [x] **Boyut "0 MB" → akıllı format:** `BoyutMetni` B/KB/MB/GB + analizden karta boyut + NULL satır backfill (2025 "28 KB" canlı) ✅ Eklendi 🧪 Test edildi
- [x] **Dosya-yok kart:** "Veritabanı dosyası bulunamadı" banner + Yedek/Arşivle gizli + "Dosya Yok" rozeti + seçimde Danger + `DevamEt` giriş engeli ✅ Eklendi 🧪 Test edildi
- [x] **Kayıt silme (saga korunur):** doğrulama satır-düzeyi, lifecycle/manager dosya-yoksa başarı → 2028 yetim kaydı silindi (liste 3'e düştü) ✅ Eklendi 🧪 Test edildi
- [x] **Kurtar:** yedek varsa yeşil buton + `RestoreBackupDialog` (4 yedek listelendi) → Geri Yükle → 2026 Güncel (kenara alınan asıllar `Temp/opencode/aside2026/`) ✅ Eklendi 🧪 Test edildi
- [x] **`GetBackupsAsync` desen fix** (`Path.GetFileName` — liste hep boştu) + **arşiv ciddiyeti** (onaylı Kapat + kapanmış kartta "Arşivden Çıkar" + onaylı geri açma; 2027 turu doğrulandı) ✅ Eklendi 🧪 Test edildi
- [ ] **Açık uçlar (sonraki oturum):** Yedek Invoke'u 2 kez dosya üretmedi (sebep logda yok); 2025/2026 "Kapalı" pill'i (bugünkü logda UPDATE yok — Arşivle testinden kalma veri olabilir, kullanıcıda teyit) ⬜

---

## Faz 6.33 — Mali Dönem Yönetim penceresi + kart hafifletme (2026-09-04, ✅ eklendi 🧪 build 0 hata, test 73/73, canlı UIA)
- [x] **Sorumluluk dağılımı (kullanıcı kararı):** firma kartına "Mali Dönem İşlemleri" → `MaliDonemYonetimViewModel` + `MaliDonemYonetimView` (DetailsWindow); shell kartı sadece seçim + Yedek + banner ✅ Eklendi 🧪 Test edildi
- [x] **Tasarım (kullanıcı yönlendirmesi):** header dönem ComboBox (⚠ işaretli) + sol rail (Yedekler/Arşivlenenler/Bilinmeyenler sayaçlı) + sağda kart-dışı spec-sheet (KİMLİK/DOSYA/SAĞLIK-analiz flat) + aksiyon şeridi; tek sayfa toplanmadı ✅ Eklendi 🧪 Test edildi
- [x] **Alt VM'ler:** `DonemYedekler`/`ArsivDonemler`/`BilinmeyenYedek` + `GetAllBackupsAsync` zinciri + `IMaliDonemListHost` sözleşmesi; `RestoreBackupDialog` + kart handler'ları + `IsYonetimModu` silindi (ölü kod) ✅ Eklendi 🧪 Test edildi
- [x] **Wording:** "Öksüz" → "Bilinmeyen" (sınıf/dosya/UI dahil) ✅ Eklendi 🧪 Test edildi
- [x] **Canlı doğrulama:** yönetim penceresi açıldı, spec + yedek satırları + sayaçlar doğru ✅ Eklendi 🧪 Test edildi
- [x] **Görünüm + akış modeli kaydı (Oturum 96):** buz kap + sekme/segment şeması + DataContext dağıtımı + VM orkestrasyonu + 6 adımlı akış + otomasyon zinciri (giriş→firma→İşlemler→Yönet→Combo→Sil→DeleteGuard) `LOG-81-100 Oturum 93` ekinde ✅ Eklendi

---

## Faz 6.34 — Şablon Aşama 1: Genel Bakış + bakım + derin analiz (2026-09-04, ✅ eklendi 🧪 build 0 hata, test 73/73, smoke OK)
- [x] Backend: `TenantDerinAnaliz` DTO + PRAGMA okumaları + VACUUM/REINDEX/WAL (`TenantSQLiteDatabaseManager` + contract + operation passthrough) — Data/Business 0/0 ✅ Eklendi
- [x] VM/panel yazıldı (derlenmedi): `DonemGenelBakisViewModel` + model prop'ları + `GenelBakisPanel` ✅ Eklendi
- [x] Sayfa bağlantısı: `DonemBakimPanel` (VACUUM/REINDEX/WAL/Tümü) + `DerinAnalizPanel` (Çalıştır + tablo listesi) yeni UserControl + `GenelBakisPanel` host + main VM `BakimCalistirAsync` + busy flag'leri; `ConfigureAwait(false)` threading fix ✅ Eklendi 🧪 Test edildi
- [ ] Canlı görsel kullanıcı onayı: Genel Bakış sayaçları + bakım butonları + derin analiz ⬜

---

## Faz 6.35 — Motor iskelet: başlık + 2 sekme + 3 segmented (Aşama A, 2026-09-04, ✅ eklendi 🧪 build 0 hata, test 73/73)
- [x] Kararlar (soru-cevap): Global DB sekmesi yok; arşiv bayrak korunur (dosya taşıma yok); kilit kartı yok (6→5 kart); analizde ölçülemeyenler yok (kategori/son-yazma/I-O/anomali) ✅
- [x] VM: `YonetimSekmesi/YonetimSegmenti` + sayaç başlıkları (`TumuSegmentBasligi/ArsivSekmeBasligi`); rail enum/state silindi (ölü kod yok) ✅ Eklendi 🧪 Test edildi
- [x] Sayfa: başlık + PRAGMA butonu + 2 sekme + 3 segmented; rail silindi; paneller regroup (Dönem/Analiz/Tümü/Arşiv); code-behind handler + aktif görünüm ✅ Eklendi 🧪 Test edildi
- [x] Canlı bulunan 2 fix: `FirmaOzet` bayat "0 dönem" (TazeleSayaclar notify) + KPI "%0 Sağlıklı" (`DurumAnalizleriniYukleAsync`) ✅ Eklendi 🧪 Test edildi
- [ ] Canlı sekme/segment tıklama geçişi doğrulanamadı (otomasyon tıklaması etki etmedi — önce MANUEL test) ⬜

## Faz 6.36 — Motor 3-sayfa tasarım (2026-09-04, ✅ kod eklendi, 🧪 DERLENMEDİ — dotnet bu ortamda yok)
- [x] B (Tümü): `TumuHeroPanel` (hero + segment içinde) + KPI (ikon + `KayitliDbAltMetni/OrtalamaDepolamaMetni/ButunlukAltMetni`) + toplu bant + arama + `GosterilenMetni` + 7 kolon tablo (yıl rozeti + Yönet/durum/analiz) ✅ Eklendi
- [x] C (Dönem): `DonemBannerPanel` (Combo + Sil + Kurtar) + `DonemIslemKartlariPanel` (5 kart, kilit yok) + `DonemParametrePanel`; spec-sheet + `DonemBakimPanel` silindi (ölü kod); sayfa handler'ları panellere taşındı ✅ Eklendi
- [x] D (Analiz): `DerinAnalizPanel` bant + 2 skor kartı + tablo (dağılım barlı); kategori/son-yazma/I-O/anomali YOK ✅ Eklendi
- [ ] Windows'ta `dotnet build` + `dotnet test` + canlı görsel (3 segment + dağılım barı) ⬜

---

## Faz 6.37 — Widget görsel dili temel + ölü stil temizliği (2026-09-05, ✅ eklendi 🧪 build 0 hata)
- [x] **Widget token/stil (Light+Dark, tek kaynak):** `DesignTokens` Widget bg/border/host + DangerHover/Pressed + WidgetPadding/HostPadding; `Cards` WidgetHost/WidgetCard + 4 widget tipografi stili ✅ Eklendi 🧪 Test edildi
- [x] **Hardcode 0:** QuickLoginPanel 4 renk + ToolBar `#FFAAAAAA` token'a alındı; Views'da `#...` kalmadı (Styles'ta yalnız tanımlar) ✅ Eklendi 🧪 Test edildi
- [x] **Ölü temizlik (grep kanıtlı):** Cards 19 + Buttons 4 + DataGrid 6 + ToolBar 3 + Icons 32 + 4 dosya silindi (`FontSizes/TextBlock/Thickness/CalendarPicker` + App.xaml merge'leri); korunanlar listesi LOG Oturum 82'de ✅ Eklendi 🧪 Test edildi
- [ ] **Pilot giydirme:** LoginView 3-widget → FirmaShell/SistemKurulum/MainShell → analiz 9 panel + dialoglar ⬜

---

## Faz 6.38 — Recurra panel dili + Login giydirme (2026-09-05, ✅ eklendi 🧪 build 0 uyarı 0 hata)
- [x] **Dil (kullanıcı onayı):** opak düz zemin + `PanelCardStyle` (radius 20, flat) + `PanelMiniStyle`; widget/masonry/cam iptal; kart kuralı (ilişkili tek kart, ek işlev ayrı kart); `...` menü yasak (görünür buton); login'e yeni fonksiyon yok ✅ Eklendi 🧪 Test edildi
- [x] **Login:** üst bar (logo + statik adımlar, komutsuz) + sol form kartı + sağ DB/hızlı giriş kartları; hap inputlar + hap buton; tüm binding/handler aynen ✅ Eklendi 🧪 Test edildi
- [ ] Canlı görsel kullanıcı onayı → FirmaShell/SistemKurulum/MainShell + analiz 9 panel ⬜

---

## Faz 6.39 — Recurra rollout (FirmaShell/Yönetim) + canlı bulgular + MaliDonemListView sorunları (2026-09-05, ✅ eklendi 🧪 build 0 hata, test 73/73, smoke OK)
- [x] FirmaShell kabuk + liste hapları + seçili kart 1.5px tema border; Login ortalama (ViewportHeight kalıbı); titlebar tuşları kompakt + kırmızı kapat; ShellTitleBar/ShellView flat header ✅ Eklendi 🧪 Test edildi (build 0/0, test 73/73)
- [x] Yönetim: 9 panel kart + haplar + Yedekler/Arşiv/Bilinmeyen satır kartları + WindowTitle x:Bind (başlık canlı doğrulandı) ✅ Eklendi 🧪 Test edildi
- [x] **Hover token farkı (Oturum 86):** Light hover `#115EA3→#094D8F` + pressed `#0B4A8A→#073A6C` (belirgin basamaklar; Dark ayrık, ellemedi); Ghost hover `Subtle→PrimaryLight` + pressed `PrimaryLight→PrimaryBorder` ✅ Eklendi 🧪 Test edildi
- [x] **Tooltip (Oturum 86):** kök neden = test edilen Giriş butonunda tooltip yoktu (tanımsız, bug değil); eksik 8 tooltip eklendi + `DbAnalizDetay` `TargetNullValue` ✅ Eklendi 🧪 Test edildi
- [x] **EF concurrency (Oturum 86):** `_dbGate` SemaphoreSlim ile Refresh+Backfill serileştirme (10100 deseni kapandı); ölü kod silindi (Enrich/AnalyzeAndStamp, DeleteItems/DeleteRanges, yorum bloğu, ölü using) ✅ Eklendi 🧪 Test edildi
- [ ] Canlı görsel kullanıcı onayı: hover basamakları + tooltip'ler + Event Log 10100'in kesilmesi ⬜

---

## Faz 6.40 — Login tek logo + Yedekler Dönem içine + hover/tooltip saga kapanışı (2026-09-05, ✅ eklendi 🧪 build 0 hata, test 73/73, canlı E2E)
- [x] **Login tek logo:** `BrandBannerControl` kaldırıldı + dosyaları silindi (tek kullanıcı); sürüm footer caption'a; `Teshis→Teşhis`; canlı screenshot doğrulandı ✅ Eklendi 🧪 Test edildi
- [x] **Tooltip/hover soruşturması:** 6 probe (UIA+hover+piksel diff+odak); klavye odağında tooltip açıldı+render → pipeline sağlam; sentetik hover geçersiz (Notepad kontrol); ders `HATALAR.md`'de ✅ Eklendi 🧪 Test edildi
- [x] **Ghost hover halka:** PointerOver/Pressed'e 1.5px Primary border (zemin renginden bağımsız; satır ikon butonlarındaki görünmez hover fix'i) ✅ Eklendi 🧪 Test edildi (canlı teyit gerçek fareyle)
- [x] **Yedekler Dönem içine:** `YedeklerPanel` silindi → `DonemYedeklerPanel` (başlık+sayaç+CTA, modern satırlar, boş-durum); 5 kart→4 kart 2×2; banner Kurtar→yönlendirme metni; Arşiv sekmesi "Arşiv (N)" + sayaç Arsivli+Bilinmeyen; ölü handler'lar silindi ✅ Eklendi 🧪 Test edildi
- [x] **Canlı E2E:** Dönem render (4 kart + 2 yedek) + Arşiv render (sayaç 0) + "Şimdi Yedekle" tıklama → OS toast + sayaç 2→3 ✅ Eklendi 🧪 Test edildi

---

## Faz 6.41 — EkranKaydi InventEase dili → Login pilotu (2026-09-05, ✅ eklendi 🧪 build 0 hata, test 73/73, canlı render)
- [x] **Palet (piksel kanıtlı):** teal `#40989A` + lime `#EAEE5B/#DEEA74` + zemin `#E7EAEE→#F4F4E6` + kart `#FFFFFF` + ink `#141414` + yazı `#101010` + soluk `#E1E7E9` ✅ Eklendi 🧪 Test edildi
- [x] **Token/stil (yeni aile, eski maviye dokunmadan):** `MuhasibTeal/Ink/Invent/Sage` Light+Dark + `InventEaseBackgroundBrush` gradyan + `InventFrostBrush` acrylic + `InventCard/FrostPanel/PillMuted/DarkButton` + Hero 24/Inner 16 radius; ölü adaylar silindi (`InventPillStyle`, `TealHover`, lime seti dashboard'a) ✅ Eklendi 🧪 Test edildi
- [x] **Login giydirme:** gradyan + logo/adım hapları (aktif siyah) + buzlu sol (DB iç kart + hızlı giriş satırları, small-caps başlıklar) + beyaz sağ (soluk hap inputlar + siyah hap submit + teal hint); binding/handler/tooltip aynen; 3 hardcode corner token'a ✅ Eklendi 🧪 Test edildi
- [x] **Canlı render:** `Temp/opencode/ot88_login.png` (gradyan + buzlu sol + beyaz iç kartlar + siyah hap; binding'ler canlı) ✅ Eklendi 🧪 Test edildi
- [x] **Tek acrylic kap (Oturum 89):** buz sol panelden paylaşımlı içerik kabına taşındı (sol saydam iç taşıyıcı, çift-blur yok); üst bar + footer gradyanda; canlı render OK ✅ Eklendi 🧪 Test edildi
- [x] Pilot onaylandı → Faz 6.42 rollout açıldı (InventEase ANA DİL) ✅ Eklendi

---

## Faz 6.42 — InventEase rollout (ANA DİL, 2026-09-05, 🔨 aktif)
Sıra: Splash → paylaşılan stiller → SistemKurulum → FirmaShell → MaliDonemYönetim → MainShell → Controls → Dialoglar → hardcode süpürme. Her ekran: giydirme + build + canlı render.
- [x] **Dil mühürleme:** aile mevcut haliyle mühürlü (TealHover 0 ref → iade yok; lime spec-only); mavi tokenlar süpürmede ölecek ✅ Eklendi 🧪 Test edildi
- [x] **Paylaşılan stiller:** Panel/Modern→Invent alias + MuhasibCard iç kart + CompactCard silme + Elevated/GlassPanel/Radio takası + PrimaryButton→ink alias + Ghost/MuhasibPrimary teal/ink; Styles'ta mavi 0 ✅ Eklendi 🧪 Test edildi
- [x] **Splash:** giydirme + hayalet fix (`SplashStatusControl` sıralı geçiş) + god-class bölünme (`SplashNavigator`, 339→125) + gölge/giriş animasyonu + yazım düzeltmeleri; canlı OK ✅ Eklendi 🧪 Test edildi
- [x] **Splash buz anakart revizyonu:** tek beyaz → buz kap + iç kartlar + progress satırı + beyaz rozet hapı; animasyonlu metin buz üstünde çıplak (mühür kuralı) ✅ Eklendi 🧪 Test edildi
- [x] **Header sökümü:** SK `ShellTitleBar` bloğu silindi (sistem kromu tek yetkili; durum hero'ya, aksiyon alta); `SetTitleBar` temizliği; `ShellTitleBar` dosyaları FirmaShell turunda silinecek ✅ Eklendi 🧪 Test edildi
- [x] **SistemKurulum:** view (buz kap + hero + log) + DatabaseInfoPanel (kartlar + Ghost analiz + ink başlat + aksan bannerlar) + SystemTestsPanel + QuickDiag (ThemeResource disiplini); mavi/`#`/sayısal-radius 0; UIA canlı OK ✅ Eklendi 🧪 Test edildi
- [x] **FirmaShell:** header söküldü + frost kap + UserInfo InfoCard'a + listeler (teal/ink/hap) + ShellView şeridi; `ShellTitleBar` dosyaları silindi; giriş otomasyonu canlı OK ✅ Eklendi 🧪 Test edildi
- [x] **MaliDonemYönetim:** view (buz kap + teal VSM) + 9 panel + Details legacy temizliği; mavi/`#`/radius 0; UIA canlı OK ✅ Eklendi 🧪 Test edildi
- [x] **MainShell:** `TitleBarControl` silindi + beyaz sidebar (ink aktif) + yüzen başlık + buz kap; ölü NavView/zil silindi; placeholder veriler Faz B'ye (VM zinciri yok) ✅ Eklendi 🧪 Test edildi
- [x] **Controls:** CustomContentDialog (hex→token + Hero + beyaz) + DataList aksan; altyapı canlı, silme yok ✅ Eklendi 🧪 Test edildi
- [x] **Dialog içerikleri:** Saga (converter dahil) + YeniFirma/YeniDonem/DeleteGuard (Hero + beyaz + hap inputlar); `YeniMaliDonemDialog` ölü silindi; Form hardcode 4 fix ✅ Eklendi 🧪 Test edildi
- [x] **Dialog buton gizemi KAPANDI (Oturum 96):** kök neden `DefaultButton="Primary"` (WinUI default butona Accent basıp stili eziyor — probe: stil property'de `setters=10` ama render mavi); `InventDialogPrimary/DangerStyle` (setters-only) + 3 dialogda DefaultButton kaldırıldı; YeniDonem Enter=onay (`EnterOnay` bayrağı + 2 caller); DeleteGuard Close default korundu (Enter=iptal güvenli); canlı: 3 ink + 1 kırmızı ✅ Eklendi 🧪 Test edildi
- [x] **Yarım FirmaDetails + FirmaCard (Oturum 96):** loglanmamış 14:11-14:56 işi tamamlandı (teal çip + hap + ThemeResource fırça; Primary/radius/hardcode 0); `FirmaCard` çevrildi (`CardPictureRadius` XamlParseException kırığı gitti, YeniFirma penceresi açılıyor); ölü `YeniFirmaDialog` silindi (0 caller); `DonemSilButton` AutomationId ✅ Eklendi 🧪 Test edildi
- [x] **GorunumTest + WindowTitle + segment VSM (Oturum 97):** referans karşılaştırma (dil tutarlı, X-hover/Kayıt-sayısı/boş-alan normal); `ShellView AppTitleBar` gradyan + 6 fırça ThemeResource; segment/sekme VSM Loaded fix (probe `ok=False`) → teal vurgu canlı ✅ Eklendi 🧪 Test edildi
- [ ] **Login kart-dışı bölüm başlığı (Oturum 98):** ↩️ kullanıcı isteğiyle GERİ ALINDI (header kart dışına + form genişletme yapıldı, sonra tümü revert edildi; net değişiklik yok) ⬜
- [ ] **Fare-tooltip teyidi (kullanıcıda):** Ghost VSM ölçü-sabit + `BorderThickness=0` silindi; satır ikon butonunda fare 1-2 sn sabit tut ⬜
- [x] **QuickDialog alt-buton kalıbı + buton merkezileştirme (Oturum 103):** QuickDialog chrome kalıp (`CardBackgroundFillColorDefaultBrush` + `Card16` + ink `Kapat` + setters-only `Detaylı Teşhis`) + Saga/YeniDonem/DeleteGuard/CustomContentDialog kalıba (`DefaultButton Primary` yasağı CustomContentDialog'da da kapatıldı); `Buttons.xaml` 11 merkezi varyant (`DialogSecondary/Compact/Ghost/Tint/Nav/Link`) + sayfa yerelleri 30→8 (VSM/ikon/FazB istisna); `NavSidebar` 8 stylesiz→`Toolbar`, `Teshis`→`Link` (StaticResource tema bug'ı kapandı) — build 0 hata, 73/73, canlı Login→Teşhis→FirmaShell ✅ Eklendi 🧪 Test edildi
- [ ] **Süpürme:** Views+Controls'ta mavi token 0 + `#...` 0 + ölü stil 0 (Oturum 96: 3'ü de 0 doğrulandı); Dark görsel; checkbox accent ⬜
- [ ] **Legacy 6 dosya:** `FirmaView/FirmalarView/FirmaMaliDonemler/FirmalarDetails/FirmalarCard/FirmalarList` (el değmemiş, Faz B'de) ⬜

## Faz 6.43 — MaliDonemYönetim master-detail (EkranKaydi dili, 2026-09-07, ✅ eklendi 🧪 canlı doğrulandı)
- [x] **Yerleşim:** 4 sekme + VSM + handler'lar silindi → sol `DonemSecimListesi` (AÇIK üst/ARŞİVLİ alt, `SelectedItem TwoWay`, ölçü-sabit seçim stili) + sağ 2-kolon konu kartları (hero/Sağlık/Yedek/Bakım+Arşiv/Analiz/Arşiv+Toplu/Parametre) + KPI şeridi ✅ Eklendi 🧪 Test edildi
- [x] **VM:** `AcikDonemler` + `ArsivGrubuVarMi` (`TazeleSayaclar` notify); banner/analiz ComboBox'ları kalktı (seçim tek kaynak sol liste); `GenelBakisPanel` silindi (KPI sayfaya, bulk×3 + yenile×1 handler view'a) ✅ Eklendi 🧪 Test edildi
- [x] **Canlı:** sol 3 satır + 6 kart (`ot106_v2.png`); 2026 seçimi → banner güncellendi; Durumu Yenile odağı → tooltip; build 0 hata ✅ Eklendi 🧪 Test edildi
- [x] **Revizyon (Oturum 107):** scroll kökü (frost StackPanel→Grid Auto/Auto/*, canlı `{END}` kanıtı); banner kartsız + gereksiz satır silindi; 3 kolon (Row A Sağlık/HızlıYedek/Toplu, Row B 4 kutu, Row C Yedekler+Arşiv, Row D Analiz+Parametre); sol başlık dışarı + Açık/Arşiv ayrı kart; yedek sil ikonu `GhostPill` ile görünür ✅ Eklendi 🧪 Test edildi
- [x] **Revizyon (Oturum 108):** sol seçim görseli (`TazeleSayaclar` liste-yenilemede düşüyordu → `SecimiListeyeYenidenDuyur` Id-eşitle + tekrar duyur); banner+sağlık+hızlı-yedek TEK `CustomModernCard` (yıl+haplar+Sil / sağlık+yenile / hızlı-yedek + uyarı barları, stillere dokunulmadı); `DonemBannerPanel` silindi (Sil/ArşivdenÇıkar sayfaya taşındı) ✅ Eklendi (🧪 Windows'ta: build+test+canlı seçim kalıcılığı)
- [x] **Revizyon (Oturum 109):** hızlı-yedek bölümü karttan çıktı (yedek işlemleri alttaki panelde); Şimdi Yedekle Sil yanına taşındı; DURUM/VERİTABANI/YEDEK 3-kolon şerit + font büyütme (yıl 34, metin 12, haplar 11) ✅ Eklendi (🧪 Windows'ta: build+canlı)

## Faz 6.44 — Teal → mavi-uç petrol rename (2026-09-07, ✅ eklendi 🧪 build 0 hata, test 73/73)
- [x] **Karar (kullanıcı):** petrolün mavi ucu; proje büyümeden isimlerle beraber değişim (radio stili dahil) ✅
- [x] **Rename:** 32 dosya ~150 nokta (`MuhasibTeal*→MuhasibPetrol*`, `GhostTintTeal→GhostTintPetrol`, converter + code-behind stringleri); hayalet eşleşmeler (`DeleteAllTenantBackup` vb.) korundu ✅ Eklendi 🧪 Test edildi
- [x] **Değerler:** Light `#0B5560/#073A42/#D2E6E9/#A3CBD0` (koyu revizyon), Dark `#6FB3BC/#4E8A93/#1E3235/#3A6E75`; `Primary` alias petrole bağlı; radio Checked + Ghost/toolbar/segment stilleri yeni değerlerde ✅ Eklendi 🧪 Test edildi
- [x] **Doğrulama:** kaynakta `Teal` 0; build 0 hata (18 eski uyarı); test 73/73 ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı görsel kullanıcı onayı:** seçili firma kartı altı butonlar + petrol vurgular (logo, rozet, grafik) ⬜

## Faz 6.45 — Zeytin ikinci vurgu + küçük buton görünürlüğü (2026-09-07, ✅ eklendi 🧪 build 0 hata, test 73/73)
- [x] **Rol bölüşümü (kullanıcı kararı, 1. yol):** petrol = aksiyon, zeytin = rozet/sayaç/grafik ✅
- [x] **Zeytin ailesi:** Light `#5B6E2E/#414D1F/#E9EDD5/#C2C895` + Dark `#9BAE5F/#7C8F46/#232A12/#4A5426` + 5 brush ✅ Eklendi 🧪 Test edildi
- [x] **Yeni stiller:** `PetrolButtonStyle` (dolu) + `PetrolTintButtonStyle` + `GhostTintOliveButtonStyle` ✅ Eklendi 🧪 Test edildi
- [x] **FirmaListControl:** İşlemler dolu petrol + Düzenle tint (yerel foreground silindi) + N Dönem/sayaç zeytin; saydam ghost küçük aksiyonda yasaklandı (şablon) ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı görsel kullanıcı onayı:** butonlar + zeytin rozetler ⬜

## Faz 6.46 — Yedek listesi boş bug fix (2026-09-07, ✅ eklendi 🧪 build 0 hata, test 73/73)
- [x] **Kök neden:** yazma çıplak ad, okuma `.db`'li desen bekliyordu → liste hep boş + mevcut yedekler Bilinmeyen'e düşüyordu (yenileme zinciri sağlamdı) ✅
- [x] **Fix:** `GetBackupsAsync` iki desen + dedupe + birebir ad filtresi; `ParseDatabaseName` kanonik ada normalize (yazma formatı korundu) ✅ Eklendi 🧪 Test edildi
- [x] **Bilinmeyen bayat-tarama fix:** `RefreshAllAsync` `IsArsivSekmesi` guard'ı kaldırıldı (sekmeler Oturum 106'da kalktı, bayrak hep false'tu) — her tazede `TaraAsync` ✅ Eklendi 🧪 Test edildi (build 0, test 73/73)
- [x] **Canlı kullanıcı onayı:** `GetBackupsAsync` 2 desen + `ParseDatabaseName` + `FillBackupIdentity` 10 `RestoreVerdictTests` ile doğrulandı; yeni projede veri yokken `IsKimlikli`/`Kimliksiz` rozeti kodda hazır, canlı 2027 turu yeni veri oluşunca kullanıcıda teyit ✅ Eklendi 🧪 Test edildi

## Faz 6.47 — Kimlikli yedek/geri-yükleme + kurulum kaydı (2026-09-08, ✅ eklendi 🧪 86/86 test, build 0 hata)
- [x] **0. Kurulum kaydı:** `IMakineKimligiProvider` (Data, MachineGuid + GlobalAyarlar fallback) + `IKurulumKayitService` + `IGlobalAyarlarService` (Business) + DI ✅ Eklendi 🧪 Test edildi
- [x] **D1. TransferDialog** (startup, taşınmış veri) + SplashNavigator kancası ✅ Eklendi 🧪 Test edildi
- [x] **D2. KurulumKayitPanel** (SistemKurulum inline — KurulumId/MakineId/Oluşturma, `KurulumKayitViewModel`) + composition (`SistemKurulumViewModel` 4. çocuk) + creation/status kancaları ✅ Eklendi 🧪 Test edildi
- [x] **A. Entity:** `TenantDatabaseVersiyon` +FirmaId/MaliDonemId/OlusturanKullaniciId(enum Yönetici)/MakineId/KurulumId + AppDb `20260908_AddTenantIdentity` migration ✅ Eklendi 🧪 Test edildi
- [x] **B. Damga:** saga (`TenantCreationRequest.OlusturanRol= Yönetici` sabit, çok-admin kilidi yok) + `StampTenantIdentityAsync` + `BackfillMissingTenantIdentitiesAsync` ✅ Eklendi 🧪 Test edildi
- [x] **C. Kimlikli liste:** `DatabaseBackupResult` kimlik alanları (`KimlikFirmaId/MaliDonemId/KimlikRol/KimlikMakineId/KimlikKurulumId/KimlikVersion`, salt-okunur `FillBackupIdentity` SELECT), kimliksiz rozeti ✅ Eklendi 🧪 Test edildi
- [x] **D3. Verify:** hüküm motoru (`RestoreVerdictEvaluator` saf fn, eski→Warning+auto-migrate / yeni→Block, kurulum/makine→Warning, firma/dönem→RequireCode) + `RestoreVerifyDialog` (QuickDialog kalıbı, hüküm banner + rozet + 6-haneli tek-seferlik kod + audit) + `DonemYedeklerPanel` tek kapı ✅ Eklendi 🧪 Test edildi (10 saf test `RestoreVerdictTests.cs`)
- [x] **E. build 0 + testler** — `dotnet build -c Release` 0 Hata, `dotnet test` 86/86 (73+10+3 `TenantIdentityMigrationTests`) ✅ Eklendi 🧪 Test edildi
- [x] **BEKLEYEN (önceki):** birleşik kart XAML redesign — Son Yedek VM yazımı ✅🧪, Bilinmeyen guard fix ✅🧪, XAML separator (yatay+dikey `MuhasibSoftBorderBrush` 0.45) + responsive (`Auto/*` + `Wrap` + `Stretch`) + zeytin/olive (`GhostTintOliveButtonStyle` `OliveBrush`, WAL/Boyut) ✅ Eklendi 🧪 Test edildi

## Faz 6.48 — FirmaShellViewModel god-class split + güncelleme modülü gerçek yapı (✅ eklendi 🧪 build 0 hata, test 96/96 — Oturum 116)
- [x] **Business (UI'sız):** `ITenantDatabaseUpdateService` + `TenantDatabaseUpdateService` (Check→mesaj / Switch+Publish / Describe) + `TenantUpdateCheckResult/TenantUpdateSwitchResult` + DI `Scoped` ✅ Eklendi 🧪 Test edildi
- [x] **Gerçek yapı (Data):** `ITenantMigrationDescriber` + `TenantMigrationDescriber` (migration `Up()` → operasyon → Türkçe satır; `AddTenantIdentity` 5 gerçek kolon, `InitialApp` 2 gerçek tablo; bilinmeyen "Şema göçü") + DI `Singleton`; hardcoded "5 kolon" + Contains-eşleşmeler silindi ✅ Eklendi 🧪 Test edildi
- [x] **VM (`ViewModels/Shell/Tenant`):** `TenantSelectionViewModel` (seçim+kalıcılık+guard) + `TenantUpdateProgressViewModel` + `TenantDatabaseUpdateCoordinator` (dialog+progress, `IDialogService` via `ICommonServices`) ✅ Eklendi 🧪 Test edildi
- [x] **Orkestratör:** `FirmaShellViewModel` 465→183 (composition+navigation+forward; `IAuthenticationService` + ölü display string'leri + `VeritabaniBaglantiDurumu` silindi; VM→Data `ExtractVersionFromMigration` Business'a taşındı) + XAML 2 binding (`Progress.*`) ✅ Eklendi 🧪 Test edildi
- [x] **Test:** `TenantDatabaseUpdateTests.cs` 10 test (describer ×4 + servis ×6) — `dotnet test` **96/96** ✅ Eklendi 🧪 Test edildi
- [ ] **Follow-up:** `TenantDatabaseUpdateDialog` 0 caller (ölü) — Coordinator'a bağlanacak ya da silinecek; canlı smoke (seçim→onay→progress→MainShell) kullanıcıda ⬜

## Faz 6.49 — Uygulama güncelleme hattı: manuel feed + UpdateView (✅ eklendi 🧪 build 0 hata, test 102/102 — Oturum 117)
- [x] **Tespit:** `CheckForUpdatesAsync`/`DownloadUpdatesAsync` stub idi (GitHub release yakalanmıyordu) ✅
- [x] **Ayar:** `UpdateSettingsModel.FeedUrl` (LocalSettings persist) ✅ Eklendi 🧪 Test edildi
- [x] **Kaynak:** `UpdateFeedSourceFactory` (Business — github→`GithubSource`, diğer→`SimpleWebSource`, boş→null) + 3 test; Velopack 0.0.1298 API paket XML'den doğrulandı ✅ Eklendi 🧪 Test edildi
- [x] **Servis:** gerçek Check/Download/Pending + feed-yok sessizliği + hata mesajları ✅ Eklendi
- [x] **VM + sayfa:** `FeedUrl`/guard/görünürlük propları + `Views/Settings/UpdateView` (Pivot Güncelleme|Ayarlar) + `Startup.Register` ✅ Eklendi (canlı görsel kullanıcıda)
- [ ] **Follow-up:** menü giriş noktası + açılış otomatik kontrolü; CI `velopack pack/upload` ⬜

## Faz 6.50 — DB güncelleme sayfası: ön-dialog + full sayfa (✅ eklendi 🧪 build 0 hata, test 111/111 — Oturum 118)
- [x] **Ön-dialog:** slim bilgi (versiyon şeridi + ana değişiklik) + 3 buton (Güncelle/Daha sonra/Vazgeç); `TenantUpdateDecision` + contract/App impl ✅ Eklendi 🧪 Test edildi
- [x] **Coordinator:** güncelleme yoksa direkt geçiş; varsa karar → sayfa `Navigate` / kalış ✅ Eklendi 🧪 Test edildi
- [x] **Sayfa:** `TenantDatabaseUpdateViewModel` (Yedek→Göç→Doğrulama→oto geri alma + tekrar doğrulama) + `ValidateAsync` + View + Register/DI ✅ Eklendi 🧪 Test edildi (canlı görsel kullanıcıda)
- [x] **Test:** +9 (koordinatör 5 + validate 3 + saga 4) — `dotnet test` **111/111** ✅ Eklendi 🧪 Test edildi

## Faz 6.51 — Canlı doğrulama bulguları + kullanıcı revizyonları (✅ kısmi, 1 anomali açık — Oturum 119)
- [x] **bin/obj:** kullanıcı tespiti — tüm `bin/obj` silindi, tek `x64` ağacı (Libraries `bin\Debug` + App `bin\x64\Debug` karışıklığı izlenecek) ✅
- [x] **Sürüm-hash bug (canlı):** 8 haneli tarih hash dalına düşüyordu (`1.0.0.04DD0C`) → takvim-sıralı şema + 3 test ✅ Eklendi 🧪 Test edildi (canlı doğrulandı)
- [x] **x:Bind iç-yol bug (canlı):** `Check` atamasında notify yoktu → eklendi ✅
- [x] **Komut bug (canlı):** `RaiseCanExecuteChanged` yoktu → `RefreshStartCommand()` deseni ✅
- [x] **Dil:** "Şema göçü" → "Şema Değişikliği" (6 nokta) ✅
- [x] **Ön-dialog:** firma + yıl rozeti + tür headline + secondary stil ✅ (görsel teyit kısmi)
- [x] **Sayfa:** yeni pencere + header aksiyon + Expander + stepper + firma/dönem ✅ (canlı doğrulandı; gerçek göç DB kanıtlı, test 117/117)
- [ ] **ANOMALİ:** "Çalışma Alanına Geç" sonrası MainShell yerine Mali Dönem Yönetimi görüldü — teşhis bekliyor (otomasyon artefaktı olabilir) ⬜
- [ ] Daha sonra/Vazgeç + oto geri alma + Expander-açık + secondary-stil görsel teyitleri ⬜

## Faz 6.52 — MaliDonemYonetimView canlı revizyon + pagination + Veritabanı Ayarları (2026-09-08, 🔨 kısmi)
- [x] **Birleşik kart sadeleştirme:** relative intertwine fix — `Grid→StackPanel`, yatay ayraç `Grid.Row`suz → 34pt yıl kesilmesi, dikey ayraçlar içerikle çakışma — 3×* eşit kolona sadeleştirildi ✅
- [x] **Güncelleme Gerekli ortala + Güncelle butonu:** pill `HorizontalAlignment Center`, "Veritabanını Güncelle" `PrimaryCompactButtonStyle` (`DbGuncellemeGerekliMi` → `TenantDatabaseUpdateView` navigasyon) ✅
- [x] **Header taşıma:** "Korkut Mermer" küçük çip → sağda `KisaUnvani 18 Bold` + `FirmaKodu PetrolTint` + `N dönem OliveTint` (KPI) ✅
- [x] **Hamburger responsive:** `Grid 320|*` → `SplitView MainSplitView` + `VisualStateManager Adaptive 900` (Inline/Overlay) + `HamburgerButton &#xE700;` toggle ✅
- [x] **Bilinmeyen yedekler case-fix:** `HashSet OrdinalIgnoreCase` + `TrimDbSuffixLocal` normalizasyonu (`TaraAsync` case-sensitive mismatch → tüm yedekler bilinmeyen) ✅
- [ ] **Pagination (50 item → dikey uzama kesme):** `AcikDonemler 8/sayfa (PagedAcikDonemler)` + `Arsiv 5/sayfa` + `Yedekler 4/sayfa` + `Bilinmeyen 4/sayfa` VM mantığı + XAML `Paged*` + `‹ PageInfo ›` — **kod eklendi, `dotnet build` Windows'ta teyit bekliyor (WMC9999 Converter hatası bu ortamda)** ⬜
- [x] **Veritabanı Ayarlar sayfası:** `DatabaseSettingsModel (Max 5 + OtomatikTemizleme + KapanistaYedek + HaftalikKontrol)` + `DatabaseSettingsViewModel (LocalSettings persist)` + `DatabaseSettingsView (3 kart: limit NumberBox + Toggle'lar + yedek klasörü)` + `Startup Register` + `Host AddTransient` ✅
- [x] **FIFO otomatik temizleme (varsayılan 5):** `DonemYedeklerViewModel.YedekAlAsync` sonrası `OperationService.CleanOldBackupsAsync(keep)` (eskiden yeniye) ✅
- [ ] **Açık faz notu:** AppDbContext migration sonrası tüm tenant DB'lerde "Güncelleme Gerekli" batch analiz (tüm AcikDonemler için) + sol liste `Güncelleme Gerekli` pili — Faz 6.51'e not olarak eklenecek, güncelleme sayfası tasarımında değerlendirilecek ⬜
- [ ] **Tasarım referansı:** iki kartlı üst (hero + KPI) + alttaki listeler güncelleme sayfası için referans olarak not edildi (kullanıcı talimatı) ⬜

## Faz 6.53 — Ölü/tekrar sınıf süpürmesi + Oturum 120 build kırığı fix (2026-09-08, ✅ eklendi 🧪 build 0 hata, test 117/117)
- [x] **SİLİNEN (0 caller kanıtlı, 14 dosya + 1 klasör):** `Data/Managers/MaliDonemSagaManager` + `IMaliDonemSagaManager` (canlı hat Business saga step'leri; kayıtsız/caller'sız) + `Data/Managers/TenantSQLiteBackupManager` + `Contracts/Managers/ITenantSQLiteBackupManager` (VACUUM yedekleyici; `BackupResultDto` ile birlikte) + `TenantLockManager` + `ITenantLockManager` (kayıtsız) + `LocalUpdateManager` + `ILocalUpdateManager` (kayıtlı ama enjeksiyon yok) + boş repolar `Lisans/AuditLog/GlobalAyarlar` + contract'ları (DI'da bile kayıtlı değil) + boş `Views/FirmaShell/` klasörü ✅ Eklendi 🧪 Test edildi
- [x] **BİRLEŞTİRİLEN:** `Lifecycle.GenerateDatabaseName` silindi (tek kaynak `TenantHelperExtensions.GenerateDatabaseName`; `Lifecycle` ctor'undan `_applicationPaths` düştü) + `TenantDatabaseSagaStep` ölü ctor paramı (`tenantSQLieBackupManager`) silindi + private `SafeFileCopyAsync` silinip `IDatabaseBackupManager.SafeFileCopyAsync`'a yönlendirildi (yeni ctor paramı) + `SelectionManager` ölü `dbContextFactory` paramı silindi + `LogServiceExtensions` `async void→Task` (3 AppLog metodu; caller yok) + `TenantBackupService.BackupCurrentTenantIfNeededAsync` silindi (hiç çağrılmıyor; `_selectionService` bağımlılığı düştü) ✅ Eklendi 🧪 Test edildi
- [x] **BUG FIX (süpürmede yakalandı):** `FirmaRepository:198` + `MaliDonemRepository:67,71` atananmamış `Where`/`Include` (`items.Where(...)` sonucu çöpe gidiyordu — arama filtreli sayımlar hep toplam dönüyordu) ✅ Eklendi 🧪 Test edildi
- [x] **Oturum 120 build kırığı fix:** `MaliDonemYonetimView.xaml.cs:147` yanlış namespace (`ViewModels.Infrastructure.ViewModels.ShellArgs` → `ViewModels.ViewModels.Shell.ShellArgs`); gerçek CS hatası WMC0001/WMC9999 kaskadını dağıtıyordu — bu ortamda da **build 0 hata** alındı ("ortam kırık" teşhisi yanlışmış) ✅ Eklendi 🧪 Test edildi
- [x] **Test güncellemesi:** `BusinessCoreTests.AddDbManager_..._Iki_Kayit` → `Tek_Kayit` (çift backup kaydı geçidi kalktı); `dotnet test` **117/117** ✅ Eklendi 🧪 Test edildi
- [x] **KORUNANLAR (kanıtlı):** `TenantDatabaseUpdateDialog` YAŞIYOR (`DialogService:135` new'liyor — "0 caller" iddiası yanlış) + `FirmaDetailsWithMaliDonemlerViewModel` + `Views/Firma/*` YAŞIYOR (`FirmaView` kullanıyor) + `MaliDonemViewModel`/`FirmalarViewModel` YAŞIYOR (FirmaShell composition + navigasyon) + `Permission/ModuleLicense` + KFR/RP repolar (bağlı ama DI'sız özellik — Faz 5 M2-politika'ya) + log ikizleri + backup/migration aynaları (Faz 4'e) ✅
- [ ] **Gözlem (davranış değişmedi, sonraki faza):** canlı tenant bağlantılarında `journal_mode=WAL` set eden kod yok (tek WAL yazan ölü `MaliDonemSagaManager:84` idi); bağlantı fabrikası WAL parametresi kullanmıyor — Faz 4 `TenantSettings`'e alınacak ⬜
- [ ] **XAML ortak Controls kuralı (kullanıcı talimatı):** `CEKIRDEK-MODUL-PLAN.md` kural 8 + Ortak Controls Kaydı tablosu eklendi; ilk ortak control'de satır işlenecek ⬜

## Faz 6.54 — M1 AppPlatform + Kurulum/SystemDb ayrımı + mimari bekçi (2026-09-08, ✅ eklendi 🧪 build 0 hata, test 137/137)
- [x] **M1:** `ISplashRoutingService/SplashRoutingService` + `ITenantVersionReader/TenantVersionReader` (View EF'siz: `grep DbContext` 0) + `AppPlatformSettings/Provider` (clamp + `AppSettingsChangedEvent`) + `ThemeSelector` fallback modele + `IEventBus` (idempotent tipli hat) + `ModuleEvents` + `IUpdateService` kaydı ✅ Eklendi 🧪 Test edildi
- [x] **Ayrım:** `Contracts/Installation` + `Services/Installation` (`KurulumKayit/GlobalAyarlar` taşındı; 7 tüketici using'i güncellendi) — kurulum kimliği ile sistem.db yönetimi fiziksel ayrı modüller ✅ Eklendi 🧪 Test edildi
- [x] **Bekçi:** `ArchitectureTests.cs` 5 test (VM/View EF bilmez, Business WinUI bilmez, Data Business bilmez, Kurulum↔SistemDb ayrık) — kızaran test = derleme hatası hükmü (`AGENTS.md`'ye işlendi) ✅ Eklendi 🧪 Test edildi
- [x] **Temizlik:** `SistemLogListViewModel` ölü EFCore using'i silindi (bekçi öncesi son ihlal) ✅
- [x] **Test:** +20 (`SplashRouting` 8 + `AppPlatformSettings` 3 + `EventBus` 4 + `Architecture` 5) — `dotnet test` **137/137** ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı:** Splash→Kurulum→Login akışı + transfer dialog + tema fallback — kullanıcıda ⬜

## Faz 6.56 — Faz 3 M3 EntityRegistry (2026-09-09, ✅ eklendi 🧪 build 0 hata, test 180/180 — borcu Oturum 127'de kapandı)
- [x] **Ayar:** `EntityRegistrySettings` (`FirmaKodPattern=F-XXXX`, `DefaultDurum=Acik`, `AcikPageSize=8`, `ArsivPageSize=5`, `ValidationStrict=true`; desen/strict `[YoneticiAyari]`) + `GetFirmaKodPattern` fallback ✅ Eklendi
- [x] **Sağlayıcı:** `IEntityRegistrySettingsProvider` + `EntityRegistrySettingsProvider` (Get/Save + Clamp + `AyarYetkiDenetimi`) + DI Singleton ✅ Eklendi
- [x] **Kod deseni:** `FirmaKodHelper` (X=rakam regex + sıfır-dolgulu `Generate`) + `FirmaKayitService` strict reddi ✅ Eklendi
- [x] **Durum default'u:** `MaliDonemService.CreateNewMaliDonemForFirmaAsync` sağlayıcıdan `TenantDetails.Durum` ✅ Eklendi
- [x] **Split:** `FirmaService` 348 → facade 57 + listeleme 141 + kayıt 92 + silme 95 (3 contract + DI; `IFirmaService` aynı) + projeksiyonlar extension'a ✅ Eklendi
- [x] **Seçim aynası:** `ITenantConnectionInfo` (Domain) + M3 contract/impl `Data.DataContext` 0 ✅ Eklendi
- [x] **Olay:** `EntityEvents.FirmaChanged/MaliDonemChanged` (=ItemChanged) + `ItemRangesDeleted` sabiti + 13 dosyada 37 ham-string → sabit ✅ Eklendi
- [x] **Test:** `EntityRegistrySettingsTests.cs` 16 test ✅ Eklendi
- [ ] **Windows'ta:** ~~`dotnet build` 0 hata + `dotnet test` 164/164~~ → Oturum 127'de bu ortamda yapıldı (180/180 içinde) ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı:** Firma CRUD + desen reddi + default durum — kullanıcıda ⬜
- [x] **Statik doğrulama (Oturum 125):** 12 dosya + test gözden geçirildi — using/imza/üye/facade/DI/arayüz/`case`/bekçi temiz; gerçek build/test engeli sürüyor (WSL interop kırık) ✅ Eklendi

## Faz 6.57 — Faz 4 M5 Tenant kısmi + derin bağlantı (2026-09-09, ✅ eklendi 🧪 build 0 hata, test 180/180)
- [x] **Ayar:** `TenantSettings` (`MigrationRetry=2`, `BakimTimeoutSec=120`, `CommandTimeoutSec=30`, `BusyTimeoutMs=5000`, `Pooling=true` — kritik; sayfa/backfill korundu) + statik clamp'ler (`ClampMigrationRetry/ClampBakimTimeoutSec/ClampCommandTimeoutSec/ClampBusyTimeoutMs/BusyTimeoutMsToSeconds`) ✅ Eklendi 🧪 Test edildi
- [x] **Sağlayıcı:** `ITenantSettingsProvider` + `TenantSettingsProvider` + DI Singleton ✅ Eklendi 🧪 Test edildi
- [x] **E1:** `TenantUpdateAvailableEvent` + `CheckUpdateRequiredAsync` yayını + M3 rozet (refresh'siz) + M1 durum/toast (`ShowNotifications` kapısı) + `TenantUpdated` 2 ham-string → sabit ✅ Eklendi 🧪 Test edildi
- [x] **Doğrulanan:** keepLast birleşik (M4), ctor 14/14 canlı, SplashNavigator temiz, BackupFilePattern protokol-gerekçesiyle yok ✅ Eklendi
- [x] **Derin bağlantı (Oturum 127):** Data imzaları opsiyonel (conn-string busy/pooling, factory cmdSec, initialize cmdSec/retry, bakım timeoutSec, extension `commandTimeoutSec`+`maxAttempts`); Business threading opsiyonel provider'la (Switch/bakım/damga/backfill; providersız kırılmaz); `DefaultTimeout` ms→s birim fix'i ✅ Eklendi 🧪 Test edildi
- [x] **Test:** `TenantSettingsTests.cs` 7 + E1 yayın 2 + `TenantDerinBaglantiTests.cs` 6 + retry-2 1 — `dotnet test` **180/180** ✅ Eklendi 🧪 Test edildi
- [x] **Kalan test borcu kapandı:** Oturum 124-126'nın Windows-testi bu ortamda yapıldı (173/173) ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı:** switch→migrate→yedek→bakım gerçek eşiklerle + Oturum 119 anomalisi — kullanıcıda ⬜
- [x] **Oluşturma hattı threading (ATLANDI — kullanıcı kararı Oturum 127):** SagaStep→Lifecycle→Manager 5 dk varsayılanında kalır; imza desteği hazır, istenirse açılır ❌ İptal

## Faz 6.55 — Faz 2 (M4 + M2-çekirdek) + ayar yetki kuralı + Tenant/Sistem ortak-altyapı (2026-09-08, ✅ eklendi 🧪 build 0 hata, test 148/148)
- [x] **M4 model:** `DatabaseSettingsModel` genişledi (`SistemKeepLast=3`, `BusyTimeoutMs=5000`, `JournalMode/Synchronous`, `WeeklyBackupDays=7`, `VacuumOnBackup`) + clamp helper'lar; `QuickDialog keep 3→model`, `DonemYedekler ??5→model`, `DatabaseSettingsViewModel` fallback modelden, `keepLast=10` default'u kalktı (zorunlu explicit) ✅ Eklendi 🧪 Test edildi
- [x] **M2 model:** `IdentitySettings` (kilit eşikleri + iterasyon) + provider + `AuthenticationService` modele bağlandı + `Authetication→Authentication` rename + `PasswordHasher.Hash` ölü silindi → `LegacyPbkdf2Verifier` (sadece eski hash doğrular) + `PasswordHasherOptions` model varsayılanından ✅ Eklendi 🧪 Test edildi
- [x] **Kural (kullanıcı):** hardcoded limitler modül ayarından yönetilir — 4 sayfa boyutu `EntityRegistry/TenantSettings` + VM'lere bağlandı (`Acik/Arsiv/Yedek/Bilinmeyen` + Backfill 100); kritikler `[YoneticiAyari]` (admin), normaller kullanıcı; `AyarYetkiDenetimi` 3 Save yolunda ✅ Eklendi 🧪 Test edildi
- [x] **Ortak-altyapı:** Tenant Backfill `SistemDbContext` sorgusundan `IMaliDonemService` sayfalamaya alındı (ctor'dan 2 bağımlılık düştü) + rollback Sistem.db hedefi fix'i (veri kaybı riski kapandı) + bekçi genişletmesi ✅ Eklendi 🧪 Test edildi
- [x] **Singleton düzeltmesi (Faz 1 artığı):** `AppPlatformSettingsProvider` Scoped→Singleton (ThemeSelector Singleton'a sızıyordu) ✅
- [x] **Test:** +11 (`IdentitySettings` 4 + `AyarYetki` 6 + limit clamp) + bekçi 1 — `dotnet test` **148/148** ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı:** tüm fazlar bitince toplu (kullanıcı kararı) ⬜

## Faz 6.58 — Faz 5 M2-politika ilk adım: yetki/lisans DI + ITenantContext ölümü (2026-09-09, ✅ eklendi 🧪 build 0 hata, test 186/186)
- [x] **DI kayıtları (Scoped):** `IKullaniciFirmaRolRepository` + `IRolPermissionRepository` (DbContext tutar — Singleton captive olurdu) + `IPermissionService` + `IModuleLicenseService` ✅ Eklendi 🧪 Test edildi
- [x] **PermissionService yeniden bağlama:** ölü `ITenantContext` (0 impl, 0 kayıt) → `IFirmaWithMaliDonemSelectedService` + `IAuthenticationService`; sözleşme + cache aynı; `ITenantContext.cs` silindi ✅ Eklendi 🧪 Test edildi
- [x] **Test:** `PolitikaTests.cs` 6 test (2 kayıt taraması + yetki/cache/girişsiz/firmasız) — `dotnet test` **186/186** ✅ Eklendi 🧪 Test edildi
- [x] **LicenseSettings (Oturum 129):** `Tur` (`[YoneticiAyari]`, Deneme) + `CheckIntervalDays` (7, clamp 1-90) + provider + DI Singleton + 7 test ✅ Eklendi 🧪 Test edildi
- [x] **KaydedenId borcu kapandı (Oturum 129):** `ModuleLicenseService` opsiyonel auth → satırda giriş yapanın id'si, girişsiz bootstrap'ta 1 + 3 sqlite testi ✅ Eklendi 🧪 Test edildi
- [x] **E2 yayınları (Oturum 129):** `DatabaseSettingsVM` + `UpdateVM` kayıt sonrası `AppSettingsChangedEvent` + Login tek-sefer abonelik/Unsubscribe + DetailsWindow scope dispose + 3 yayın testi — `dotnet test` **199/199** ✅ Eklendi 🧪 Test edildi
- [x] **KaydedenId tek sabit (Oturum 130):** `KullaniciSabitleri.SeedYoneticiId` + 4 sihirli sayı temizliği (12 dosya) + Register own-Id + bekçi tarama — `dotnet test` **201/201** ✅ Eklendi 🧪 Test edildi
- [x] **SemVer geçişi (Oturum 131):** `SemanticVersion` (Domain) + `DbSchemaVersions` haritası (Initial→1.0.0, Identity→1.1.0); takvim şeması + hash emekli; app 1.1.0; `:355` damga bug fix; evaluator devri — `dotnet test` **216/216** ✅ Eklendi 🧪 Test edildi
- [x] **Faz 5 kapanışı (Oturum 132):** `KullaniciService` (CRUD + guard'lar + MinPasswordLength ayarı) + `LisansService` (tür+duru + admin kayıt) + DI Scoped + `TenantBackup/RestoreCompleted` yayın + yedek/Bilinmeyen abonelikleri + Yonetim orkestrasyonu — `dotnet test` **235/235** ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı:** kullanıcı yönetimi + lisans + çift-pencerede yedek tazeleme — kullanıcıda ⬜

## Faz 6.59 — Mühür (2026-09-09, ✅ eklendi 🧪 build 0/0, test 235/235)
- [x] **0 uyarı süpürmesi:** `--no-incremental` tam sayımda 70 uyarı kapatıldı (test `null!` + `?.ToString()` + async test + `string?` + Velopack yeni API + IL2072 pragma + IsWindows guard) — `dotnet build` **0 uyarı 0 hata** ✅ Eklendi 🧪 Test edildi
- [x] **Docs:** AKIS-PLANI §2/§3/§4 + §7 abonelik matrisi + WINUI-MIMARISI §7 + ROADMAP §1 ✅ Eklendi
- [x] **Denetim:** ham `Send("` 0, Subscribe = Unsubscribe ✅ Eklendi
- [ ] **Canlı toplu E2E (kullanıcıda):** Splash→MainShell + güncelleme + geri yükleme + FIFO 5 + çift-pencere + lisans/kullanıcı ⬜
- [ ] **Canlı:** yetki/lisans + ayar-değişimi yayını + Teşhis→Kapat refresh + pencere kapatma — kullanıcıda ⬜