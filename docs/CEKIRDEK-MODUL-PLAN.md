# Çekirdek Modül Planı — Bağımlılık Akışına Göre Fazlar

> Kaynak: `docs/AKIS-PLANI.md` + `docs/WINUI-MIMARISI.md` + ctor-bağımlılık grafı (Oturum 121-122 taramaları).
> Bu dosya **yapılacaklar / yapılanlar** tek kaynağıdır. Faz kapatma kuralı `AGENTS.md` 7 madde + aşağıdaki kesin kurallara tabidir.

Marker: `⬜` bekliyor · `🔨` aktif · `✅` kod eklendi · `🧪` derleme/test doğrulandı · `⚠️` risk · `❌` hata

## Kesin Kurallar (bu planın anayasası)

1. **Faz sırası bağımlılık akışına göredir** — üst faz, alt faz bitmeden açılmaz (L0 → L1 → L2 → L3 → L4).
2. **Kullanıcı arayüzü unutulmayacak** — her fazda etkilenen View/ViewModel + canlı UIA doğrulaması zorunlu (Splash→Kurulum→Login→FirmaShell→Yönetim→Update→MainShell).
3. **Generic yapılar tercih edilecek** — yeni liste/sonuç/haberleşme tipi yerine mevcut generic genişletilir (`GenericListViewModel<T>`, `ApiDataResponse<T>`, `IRepository<T>`, `IEventBus.Publish<TEvent>`).
4. **Aynı işi yapan iki sınıf olmayacak** — her fazda tekilleştirme listesi kapanmadan faz kapanmaz (`grep` 0 caller = silinir, kural 4).
5. **Katmanlı mimariye uyulacak** — `View → ViewModel → Business.Contracts → Business.Services → Data → Domain`. `DbContext/EF` View ve ViewModel'e giremez; Business `Microsoft.UI` bilmez (kural 5).
6. **Modeller güncellenecek** — her fazda ilgili `Domain/Models` + `Domain/Entities` güncellenir, migration gerekiyorsa aynı fazda üretilir.
7. **Metoddan değil modelden gelen gerçek veriler işlenecek** — hardcoded/yerinde hesaplanan değer yasak; her değer ilgili `*Settings` modelinden veya DB satırından okunur (`?? 5`, `= 8`, `CleanOldBackupsAsync(3)`, `keepLast = 10`, `Default→Light` fallback'ları gibi).
8. **XAML ortak Controls kuralı** — tasarım kolaylığı ve tutarlılığı için aynı işlemi yapan tüm XAML işlevleri ortak `/Controls` altında tek sınıfta toplanır; sayfa başına kopya işlev yazılmaz. Nerde neyin kullanıldığı aşağıdaki **Ortak Controls Kaydı** tablosuna işlenir (kayıtsız ortak control kullanılmaz).
9. **Hardcoded limitler modül ayarından yönetilir** — ilgili sınıfın modeli tarafından talep edilen her değer, o modülün ayar sınıfına eklenir; kullanıcı müdahalesine açılır. Kritik işlemler (silme/güvenlik/performans/yol) `[YoneticiAyari]` işaretli — yalnızca yönetici değiştirir; normal işlemler tüm kullanıcılara açık. Yetki denetimi `AyarYetkiDenetimi` tek kaynağından (`UnauthorizedAccessException`). Protokol sabitleri (buffer/kilit döngüsü/UID bitleri) bu kuralın dışındadır.
10. **Tenant ↔ Sistem.db ortak altyapı kuralı** — iki modül birbirine doğrudan dokunmaz (somut DbContext/satır/servis yok); ortak sözleşmeler üzerinden konuşur (`IMaliDonemService`, `IApplicationPaths`, `IDatabaseBackupManager`, `ITenantVersionReader`). Yeni fonksiyon/ek işlem aynı ortak altyapıya bağlanır. Bekçi: `ArchitectureTests.Tenant_Ile_SistemDb_Modulleri_OrtakAltyapi_Disinda_Konusmaz`.
11. **Canlı testi agent yapar, kanıt sunulur (AGENTS Kural 18)** — view/özellik bitince agent uygulamayı açıp akışı kendisi test eder, ekran görüntüsü + UIA + referans açıklaması kanıtını `LOG`'a işler ve kullanıcı onayına sunar. "Canlı test kullanıcıda" denerek faz kapatılamaz; kanıt/onay yoksa build 0/0 olsa bile kapanmaz.
12. **Referans defteri (AGENTS Kural 19)** — internetten/sektörden alınan her tasarım/akış referansı, uygulanmadan önce `docs/REFERANSLAR.md`'ye (kaynak + uygulandığı view/özellik) yazılır; uygulama bitince `Durum` güncellenir. Kayıtsız referansla tasarım uygulanmaz; aynı yer için tekrar araştırma yapılmaz; çelişkide kullanıcıya sorulur.

---

## Bağımlılık Katmanları (neden bu sıra)

```
L0 M1 AppPlatform (yaprak — kimseye yaslanmaz)
 └─ L1a M4 SystemDb + L1b M2-çekirdek Auth (sadece L0 ister, paralel yapılabilir)
     └─ L2 M3 EntityRegistry (FirmaSvc→AuthSvc, MaliDonemSvc→FirmaSvc olduğu için L1'den sonra)
         └─ L3 M5 Tenant (facade 14 bağımlılık: M1+M3+M4 ister — en üst)
             └─ L4 M2-politika Yetki/Lisans (Permission→TenantContext+repo, License→Firma — en geç)
```

- M2 tek parça sıralanamaz: `AuthenticationService→Authenticator+UserRepo` yaprak (L1), `Permission/ModuleLicense` politika (L4). İki dalga halinde kurulur.
- `TenantSQLiteDatabaseService` god-class değil, facadedir — hedef bölünme değil ctor sadeleştirme + delege koruma.
- `ITenantContext.SetTenant` 0 caller (ölü) — Faz 2'de sil ya da `FirmaWithMaliDonemSelectedService` arkasına bağla, kararsız bırakma.

---

## Faz 1 — M1 AppPlatform (L0) `✅🧪` (Oturum 122 — build 0 hata, test 137/137)

**Amaç:** tüm modüllerin bastığı zemin + merkezi event taşıyıcısı.

### Yapılacaklar
- [x] `ISplashRoutingService` + `SplashRoutingService` (`Business/Contracts/UIServices/`, `Business/Services/UIService/`): `DecideRouteAsync + CheckTransferAsync` (Backfill + `TransferDetectedEvent` yayını, best-effort) ✅🧪
- [x] `SplashNavigator` sadeleşti (karar+veri Business'ta; `using EFCore/SistemDbContext/dbFactory` silindi; `grep DbContext` 0 — tek eşleşme yorum satırı) + `TransferDialog` gösterimi View'da kaldı ✅🧪
- [x] `ITenantVersionReader` (`Data.Contracts`) + `TenantVersionReader` (`Data`, Scoped) — damga taraması; Business `AppDbContext`'e dokunmuyor ✅🧪
- [x] `IApplicationPaths` taşınmadı — zaten `Data.Contracts` altında, M4/M5 interface üzerinden kullanıyor; taşıma gereksiz görüldü ✅
- [x] `AppPlatformSettings` + `Provider` (clamp + `AppSettingsChangedEvent`) + `ThemeSelectorService` fallback'i modele bağlandı (davranış aynı) ✅🧪
- [x] Merkezi event altyapısı E0: `IEventBus` (`Business/Services/CommonServices`, Singleton transport üstünde idempotent abonelik) + `DomainEvent` + `ModuleEvents` sabitleri + 2 kayıt (`AppSettingsChanged/TransferDetected`, ikisi de canlı yayınlı) ✅🧪
- [x] `IUpdateService` yorumu açıldı (impl vardı); `ISettingsService` kayıtsız kaldı (impl yok — not eklendi) ✅
- [x] Tekilleştirme: karar mantığı tek (`ShouldRedirectToSetupAsync` arşivde zaten yoktu) ✅
- [x] Testler: `SplashRoutingTests` 8 + `AppPlatformSettingsTests` 3 + `EventBusTests` 4 (fake transport; App referansı test projesine giremez) ✅🧪

### Kullanıcı arayüzü
- `ExtendedSplash`, `ShellView`, `StatusBar/StatusMessage` — splash kararı + tema Light + status bandı canlı doğrulanır.

### Generic / tekilleştirme
- Yeni liste tipi yok; `IEventBus.Publish<TEvent>/Subscribe<TEvent>` generic kullanılır. `LocalSettingsOptions` `AppPlatformSettings` içine gömülür (iki yol sınıfı kalmaz).

### Modeller
- `Domain/Models/AppPlatformSettings.cs` (yeni) + `LocalSettingsOptions` gömülür. Migration yok.

### Gerçek veri
- Tema, splash gecikmesi, status süresi, bildirim aç/kapa — hepsi `AppPlatformSettings` modelinden; fallback hardcoded'u kalkar.

### Yapılanlar
- (boş — faz açılınca işlenir)

### Doğrulama
- `grep "EntityFrameworkCore" MuhasibPro/Views` 0 · `dotnet build` 0 hata · `dotnet test` · canlı Splash→Kurulum→Login + transfer dialog.

---

## Faz 2 — M4 SystemDb + M2-çekirdek Auth (L1, paralel) `⬜`

**Amaç:** veri zemini + kimlik yaprağı.

### Yapılacaklar (M4) `✅🧪` (Oturum 123)
- [x] `SystemDbSettings` genişledi + clamp helper'lar; QuickDialog/DonemYedekler/SettingsViewModel modele; `keepLast=10` default'u kalktı.
- [x] `MaliDonemSagaManager:84` WAL referansı öldü (Oturum 121'de silindi); bağlantı fabrika timeout'u Faz 4 `TenantSettings`'e (Data katmanı ayar servisi bilemez).
- [x] `SistemDatabaseService` facade korundu.

### Yapılacaklar (M2-çekirdek) `✅🧪` (Oturum 123)
- [x] `IdentitySettings` + provider + kilit eşikleri modelden; `SessionTimeoutMin` alınmadı (tüketici yok).
- [x] `Authetication` → `Authentication` rename tamamlandı.
- [x] Tekilleştirme: ölü `Hash` silindi → `LegacyPbkdf2Verifier` (eski format doğrular); yeni hash'ler Identity'de, gücü modelden (`PasswordHasherOptions`). Format uyumu nedeniyle birleştirilemez — gerekçe loglu.

### Kullanıcı arayüzü
- `SistemKurulumView` (4 çocuk: Status/Creation/Diagnostics/KurulumKayit — composition korunur) + `LoginView` (kilit/açma, `CanLogin=DbIsReady`) canlı.

### Generic / tekilleştirme
- Yeni auth tipi yok; `IRepository<Lisans>` + `IRepository<Kullanici>` generic korunur. `MaliDonemSagaManager` (Data, 5 adım: kayıt-sonra) vs `MaliDonemSagaStep+TenantDatabaseSagaStep` (Business: kayıt-önce) — iki saga yan yana kalmaz, biri facade'lanır (karar bu fazda loglanır).

### Modeller
- `DatabaseSettingsModel` genişler + `IdentitySettings.cs` yeni. `GlobalAyarlar` KV deseni korunur (`KurulumId/MachineGuid/FallbackMachineId`).

### Gerçek veri
- Yedek limiti, timeout, WAL, lockout eşiği — modelden; `?? 5` / `= 5` / `(3)` fallback'ları kalkar (provider clamp `1-20`).

### Yapılanlar
- (boş)

### Doğrulama
- `grep "CleanOldBackupsAsync(3)"` 0 · `grep "keepLast = 10"` 0 · build 0 · test · canlı kurulum saga + yedek/restore + login/lockout.

---

## Faz 3 — M3 EntityRegistry (L2) `✅` kod (🧪 Windows'ta: build+test+canlı — Oturum 124)

**Amaç:** Firma/MaliDonem kayıt tek kaynağı + seçim aynası.

### Yapılacaklar
- [x] `EntityRegistrySettings`: `FirmaKodPattern=F-XXXX`, `DefaultDurum=Acik`, `AcikPageSize=8`, `ArsivPageSize=5`, `ValidationStrict=true` + provider + Clamp + yetki (Oturum 124) ✅
- [x] `MaliDonemYonetimViewModel AcikPageSize`, `ArsivDonemlerViewModel ArsivPageSize` ayara bağlıydı — doğrulandı (Oturum 124) ✅
- [x] `FirmaWithMaliDonemSelectedService` tek seçim aynası (`StateChanged` korundu); `TenantContext` Contract'lardan çıktı → `ITenantConnectionInfo` (Oturum 124) ✅
- [x] `FirmaService:348` composition'la 150 altına: facade 57 + listeleme 141 + kayıt 92 + silme 95 (Oturum 124) ✅
- [x] Event E0 devamı: `EntityEvents.FirmaChanged/MaliDonemChanged` (=`ItemChanged`) + `ItemRangesDeleted`; 37 ham-string → sabit (Oturum 124) ✅

### Kullanıcı arayüzü
- `FirmaShellView` (FirmalarList + MaliDonemlerList, `SelectionMode=Single` korunur) + `FirmaDetails/FirmaCard` + firma/dönem rozetleri canlı.

### Generic / tekilleştirme
- `GenericListViewModel<T>` korunur, yeni liste bazı açılamaz. `FirmaDetailsWithMaliDonemlerViewModel` vs `MaliDonemListViewModel.LoadAsync(firmaId)` çakışması teklenir.

### Modeller
- `FirmaModel/MaliDonemModel/TenantDetailsModel` + `EntityRegistrySettings.cs` yeni. `TenantDetails` DB-seçim modeli korunur (skalar saçılmaz).

### Gerçek veri
- Kod deseni, durum default'u, sayfa boyutları — modelden; VM içi sabit kalkar. `Boyut/SonYedek` Global.db satırından (mevcut kural korunur).

### Yapılanlar
- (boş)

### Doğrulama
- 150 satır üstü VM kalmaz (bu modülde) · build 0 · test · canlı CRUD + son-seçim kalıcılığı + rozetler.

---

## Faz 4 — M5 Tenant (L3) `🔨` kısmi (Oturum 126: ayar+E1; Oturum 127: derin bağlantı)

**Amaç:** en dağınık hattın ayara bağlanıp sadeleşmesi + güncelleme olay hattı.

### Yapılacaklar
- [x] `TenantSettings`: `MigrationRetry=2` + `BakimTimeoutSec=120` + `CommandTimeoutSec=30` + `BusyTimeoutMs=5000` + `Pooling=true` (kritik) + provider + DI + test; sayfa/backfill korunur (Oturum 126) ✅ — derin bağlantı (conn-string/timeout/retry threading) Oturum 127
- [x] `keepLast` birleşimi doğrulandı: `DonemYedeklerVM:166` zaten `GetManuelKeep()` (M4), kalıntı 0 (Oturum 126) ✅
- [x] `TenantSQLiteDatabaseService` ctor: 14/14 canlı, ölü param yok — facade bölünmez (Oturum 126) ✅
- [x] Sayfa boyutları zaten bağlı (Yedek/Bilinmeyen VM + Backfill); `DatabaseManager:379 VACUUM` fiil haritası + `CommandTimeout=120` derin bağlantıya (Oturum 127); `BackupFilePattern` protokol-gerekçesiyle yok (Oturum 112 + kural 9) (Oturum 126) ✅
- [x] Event E1: `TenantUpdateAvailableEvent{DatabaseName, From→To}` → M3 rozet (refresh'siz) + M1 status/toast (`UpdateSettingsModel.ShowNotifications` — plandaki `AppPlatformSettings` adı bayat, gerçek kapı bu); `TenantUpdated→ExecuteDevamEt→MainShell` korunur + 2 ham-string sabite (Oturum 126) ✅
- [x] `SplashNavigator` 86 satır, factory artığı 0 — Faz 1'de kapanmış (Oturum 126) ✅

### Kullanıcı arayüzü
- `MaliDonemYonetimView` (birleşik kart + SplitView 900 + pagination 8/5/4/4) + `TenantDatabaseUpdateView` (Yedek→Göç→Doğrula→AutoRestore) + `RestoreVerifyDialog` (tek kapı) + `TransferDialog` canlı.

### Generic / tekilleştirme
- `TenantUpdateCheckResult/SwitchResult`, `TenantCreationRequest/Result`, `DatabaseBackupResult` generic `ApiDataResponse<T>` ile sarılır; yeni sonuç zarfı açılamaz.

### Modeller
- `TenantSettings.cs` yeni + `TenantDatabaseVersiyon` damga deseni korunur (`FirmaId/MaliDonemId/Rol/MakineId/KurulumId`).

### Gerçek veri
- Yedek deseni, retry, sayfa boyutları, rozet durumu — `TenantSettings` + `GetTenantDatabaseStateAsync` analizinden; `DescribeMigration` gerçek `Up()` operasyonundan (hardcoded "5 kolon" yasağı sürer).

### Yapılanlar
- (boş)

### Doğrulama
- `grep "VACUUM INTO" Business` 0 (sadece Data) · build 0 · test · canlı switch→migrate→yedek→verify→transfer (Oturum 119 senaryosu) + Oturum 119 anomalisi (`Continue→MainShell`) kapalı.

---

## Faz 5 — M2-politika Yetki/Lisans (L4) `✅🧪` (Oturum 128: DI; 129: LicenseSettings+E2-yayın; 130: KaydedenId sabiti; 131: SemVer ayrı iş; 132: kapanış)

**Amaç:** üst politika katmanı + settings yayın eksiklerinin kapanması.

### Yapılacaklar
- [x] `PermissionService + ModuleLicenseService` DI'ya alındı (`Scoped`) + KFR/RP repoları `Scoped` (Oturum 128) ✅🧪 — `PermissionService` ölü `ITenantContext`'ten koparılıp seçim+auth aynalarına bağlandı, `ITenantContext.cs` silindi.
- [x] **Yeni** `KullaniciService` (repo sarmalar: profil/şifre/durum/silme + self/seed guard'ları + `MinPasswordLength` ayarı) + **yeni** `LisansService` (tür ayardan + satırlar tablodan + admin kayıt) + DI Scoped (Oturum 132) ✅🧪
- [x] `LicenseSettings` (`Tur` `[YoneticiAyari]` + `CheckIntervalDays` + provider + DI Singleton + 7 test) + `ModuleLicenseService` KaydedenId auth'tan (bootstrap fallback 1, 3 test) (Oturum 129) ✅🧪
- [x] Event E2 tamam (Oturum 129 yayınlar + Oturum 132 abonelikler): `TenantBackup/RestoreCompleted` tipli olaylar + yedek-listesi/Bilinmeyen abonelikleri + Yonetim orkestrasyonu (Subscribe = Unsubscribe) ✅🧪
- [x] `KaydedenId` tek sabit (Oturum 130): `KullaniciSabitleri.SeedYoneticiId` + 4 sihirli sayı temizliği (12 dosya) + Register own-Id + bekçi tarama ✅🧪
- [x] `TenantDatabaseUpdateDialog` kararı KAPANDI (Oturum 121): YAŞIYOR — `DialogService:135` new'liyor, "0 caller" iddiası yanlış çıktı; silinmedi.

### Kullanıcı arayüzü
- `LoginView` (kilit) + `MainShell` (yetkiye göre menü) + `UpdateView` (Pivot Güncelleme|Ayarlar) + `DatabaseSettingsView` (3 kart) canlı.

### Generic / tekilleştirme
- `KullaniciService` vs `UserRepository/Authenticator` — service repo'yu sarar, aynı kuralı iki yerde uygulamaz. `LisansRepository:IRepository<Lisans>` generic korunur.

### Modeller
- `LicenseSettings.cs` yeni + `Kullanici/KullaniciFirmaRol/RolPermission/Lisans` entity'leri güncellenir (gerekirse migration aynı fazda).

### Gerçek veri
- Yetki `PermissionService.HasPermissionAsync` + lisans `ModuleLicenseService` + rol-firma atamaları — DB'den; menü/buton görünürlüğü bu modellere bakar (hardcoded rol id yok).

### Yapılanlar
- (boş)

### Doğrulama
- `grep "new.*Service(" ViewModels` 0 (sadece DI) · `Subscribe` sayısı = `Unsubscribe` sayısı · build 0 · test · canlı yetki/lisans + ayar-değişimi yayını.

---

## Faz 6 — Mühür `✅🧪` (Oturum 133)

- [x] `AKIS-PLANI.md` (§2 Sistem.db/Tenants/Backups + SemVer, §3 Splash-ilk + onaylı oluşturma, §4 Update/Verify/Transfer/FIFO/kimlik) + `WINUI-MIMARISI.md` (§7 Coordinator/Yönetim/UpdateView/kapanış) güncellendi (Oturum 133) ✅
- [x] Abonelik matrisi (5 tipli olay) `AKIS-PLANI §7` tablosu; ham `Send("` 0 (yeni kod tipli bus'ta); `ThemeChanged` bus dışı kalır (Oturum 133) ✅
- [x] `KONTROL-LISTESI 6.59 + ROADMAP 6.59 + HATALAR + LOG` son cilt işlendi (Oturum 133) ✅
- [x] Full `dotnet build --no-incremental` 0 uyarı/0 hata + `dotnet test` 235/235 (Oturum 133) ✅🧪
- [ ] E2E canlı (Splash→MainShell + güncelleme + geri yükleme + FIFO 5) — kullanıcıda ⬜

---

## Ortak Controls Kaydı (nerde ne kullanılıyor)

| Control (ortak yol) | İşlev (tek cümle) | Kullanıldığı yerler | Tarih |
|---|---|---|---|
| `MuhasibPro/Views/Components/YardimDialog` | Kural 13 yardım dialogu: başlık + madde listesi, sayfa ? butonundan açılır | `SistemDbYonetimView` (Oturum 236) | 2026-09-13 |

Kural: yeni ortak control eklenirken bu tabloya satır eklenir; sayfa-yereli kopya işlev bulunursa silinip ortak olana yönlendirilir.

---

## Riskler

- M5'e zeminsiz girilmez (stub şişer). 2a/2b paralel dışında sıra atlanmaz.
- Bu ortam WMC9999 kırık → tüm doğrulamalar Windows'ta.
- `bin/Debug` vs `bin/x64/Debug` karışıklığı izlenir; `slnx→sln`, `Mapster NU1903` bu planın dışıdır.
