# MuhasibPro — Faz 6.85: Kullanıcı Yönetimi + RBAC (kullanıcı → modül/alan erişimi) Planı

> **Faz 6.85 (genişletildi).** Kullanıcı bazlı yapı çekirdeğin sonuna bırakıldı; **kim hangi modülü/alanı kullanabilir, nereye girebilir, yetkisiz alan nasıl engellenir** eksik kaldı. Bu faz onu kapatır.
> **Durum:** **K1 ✅🧪 · K2 ✅🧪 (canlı onay bekliyor) (Oturum 285)** · **K2-REDESIGN + K3 🔨 (Oturum 285 devam — YENİ CONTEXT)** · K4-K6 📋. **Öncelikli faz** (kullanıcı kararı: "eksik işleri tamamladıktan sonra öncelikli faz").
> **Sahiplik:** RBAC `Data`+`Business` (motor) · UI `Views`/`ViewModels` · migration (Kural 8) — sınıf bazlı onay.

## Kullanıcı sözü / kapsam
- Kullanıcı atarken **hangi kullanıcı hangi modülü kullanabilsin**, **nerelere giriş yapabilsin**, **giriş yapılmayacak alanlar nasıl engellenecek** — bunlar eksik.
- Kullanıcı Yönetimi modülü + **rol/izin matrisi** + **modül/alan erişim kapısı** tek fazda.
- "Eksik iş kalmasın, temiz bir yapıyla devam."

## Mevcut durum (kod doğrulaması — Oturum 284 taraması)
**Var olan altyapı (Sistem.db):**
| Parça | Durum | Ref |
|---|---|---|
| `Kullanici`, `KullaniciRol`, `RolPermission`, `KullaniciFirmaRol` (KFR) entity'leri | ✅ var | `Domain/Entities/SistemEntity/*` |
| `KullaniciRolTip` enum | ⚠️ yalnız `Yönetici=1`, `Kullanici=2` | `KullaniciRol.cs:17-21` |
| `Permission` enum | ✅ **71 değer** (100…2300) | `Domain/Enum/Permission.cs:7-124` |
| Migration tabloları | ✅ `Initial` (KullaniciRoller/KFR/RolPermission) | `Migrations/20260828170905_Initial.cs:111,323,354` |
| `PermissionService.HasPermissionAsync` | ✅ KFR → `RolPermission` okur | `PermissionService.cs:33-55` |
| `IKullaniciService`/`KullaniciService` | ⚠️ listele/güncelle/aktif/şifre/sil — **create/rol yok** | `IKullaniciService.cs:8-17` |

**Eksikler (kök nedenler):**
1. ❌ **`RolPermission` seed yok** — ne `HasData`, ne runtime; yöneticide bile izin kümesi boş.
2. ❌ **`KullaniciFirmaRol` yazıcısı yok** — `AddAsync` 0 çağıran; kullanıcı kaydı/firma oluşturma/kurulum KFR üretmiyor.
3. ❌ **Seed yöneticiye KFR atanmıyor** (`SeedUser` yalnız Kullanici + 2 rol: `SistemDbContext.cs:76-94`).
4. ❌ **`PermissionService`'te Yönetici bypass'ı yok** (`KullaniciRolTip.Yönetici` okunmuyor); bypass yalnız `AyarYetkiDenetimi`'nde (izin sistemiyle bağlantısız).
5. ❌ **`ClearCache()` prod'da 0 çağıran** — rol değişiminde cache invalidation yok.
6. ❌ **UI yetki kapısı yok** — `Permission.` kullanımı tüm çözümde **tek yerde** (`AsistanSohbetViewModel.cs:180`).
   - Modül menüsü statik (`MainMenuViewModel.cs:31-65`); "Ayarlar" yetkisiz herkese açık (`FirmaShellView.xaml:83`);
   - Denetim nav yalnız DEBUG filtreliyor (`DenetimMasasiViewModel.cs:216-221`).
7. ❌ **Modül lisans kapısı bağlı değil** — `IModuleLicenseService.IsModuleActiveAsync` 0 çağıran; `ModuleLicenseViewModel` hardcoded `firmaId=1`.
8. ❌ **Faz 6.85 UI hiç yok** — `KullaniciYonetimiView/ViewModel` yok; `UserInfoControl` menüsü yalnız logout, **rol rozeti hardcoded "Admin"**.
9. ❌ **`AuthenticationService.Register` UI'dan 0 çağıran**; `Adi/Soyadi` set edilmiyor.
10. ✅ `KullaniciRolTip` iki rol (`Yönetici`/`Kullanıcı`) — **karar (Oturum 284): iki rol yeterli**; özel rol yok. `Kullanıcı` rolünün izin seti K3'te düzenlenebilir.
11. ⚠️ Küçük borç: `Permission.cs:5` yorumu "eşleme Global.db'de" → doğrusu Sistem.db.

## Hedef mimari (RBAC)
```
Kullanici ──(KullaniciFirmaRol)──► Firma  (firma-başına rol)
     Rol (KullaniciRol) ──(RolPermission)──► Permission[]   (71 aksiyon)
                     │
        PermissionService.HasPermissionAsync(Permission)
                     │
        ├─ MainShell modül menüsü (yetkisiz modül gizli/pasif + gerekçe)
        ├─ Denetim Masası bölümleri (yetkisiz bölüm gizli)
        ├─ Sayfa/buton guard'ları (yıkıcı işlemler yetki ister)
        └─ AI asistanı (AiAsistan_Kullan) — mevcut kapı korunur
```
- **Yönetici istisnası net olsun:** `KullaniciRolTip.Yönetici` → tüm izinler (bypass), `PermissionService`'te.
- **Firma-bazlı rol:** KFR `(KullaniciId, FirmaId) → RolId`; firma erişimi **KFR ile** (bugün `KaydedenId` ile — KFR'ye geçilir).
- **Sahiplik kuralı (kullanıcı, Oturum 284):** **"Firmayı oluşturan / uygulamaya giriş yapan kişi = o firmanın sahibi ve Yöneticisi (admin)"** → firma oluşturulunca oluşturana **Yönetici KFR** yazılır.

### KFR nedir? (sözlük)
`KullaniciFirmaRol` = **bir kullanıcının bir firmadaki rolü**. Tablo: `(KullaniciId, FirmaId) → RolId` (PK: KullaniciId+FirmaId). Yani rol "global" değil, **firma başına**dır: aynı kullanıcı A firmasında Yönetici, B firmasında Kullanıcı olabilir. İzinler role bağlıdır (`RolPermission`): `Kullanıcı → KFR(firma) → Rol → Permission[]`. `PermissionService` bu zinciri `(kullaniciId, seciliFirmaId)` ile çözer.

## Kararlar
| Konu | Karar | Kaynak |
|---|---|---|
| Seed yaklaşımı | **3 katman:** (1) migration/`HasData` statik `RolPermission` (Yönetici=tüm izinler), (2) firma oluşturmada oluşturana Yönetici KFR, (3) açılışta idempotent backfill (Yöneticisiz firmaya seed yönetici) | Kullanıcı onayı (Oturum 281) + `REFERANSLAR` 281 |
| Migration | **Kural 8 ✅ ONAYLI (Oturum 284)** — K1 RBAC seed + `Kullanici.SifreDegistirmeliMi` kodlanabilir | AGENTS Kural 8 |
| Firma erişimi | **KFR** — firma oluşturan/giriş yapan sahip = o firmanın **Yöneticisi** | Kullanıcı kararı (Oturum 284) |
| Mevcut firmalar | **Önemsiz (test verisi)** → dev'de sıfırlanıp yeniden oluşturulabilir; backfill yine de prod-güvenli tutulur | Kullanıcı kararı (Oturum 284) |
| Rol matrisi | **İki rol:** `Yönetici` (tüm izinler) + `Kullanıcı` (varsayılan set; yönetici izin matrisinden düzenleyebilir). **Özel rol oluşturma yok.** Modül erişimi rol üzerinden yönetilir (kullanıcıya firma-başına rol atanır) | Kullanıcı kararı (Oturum 284) |

## Faz adımları

### K1 — Seed/atama temeli (RBAC çalışır hâle gelir) ✅🧪 — **Kural 8 ✅ onaylı (Oturum 284)**
- [x] `RolPermission` statik matrisi (Yönetici=tüm `Permission`=76; Kullanıcı=temel görüntüleme=16) → `SeedDataRolPermission` `HasData` + migration `RbacRolPermissionSeed` (**idempotent `INSERT OR IGNORE`**; mevcut/geçici satırlarla çakışmaz).
- [x] Firma oluşturmada (`FirmaKayitService`) oluşturana **Yönetici KFR** (idempotent).
- [x] Açılışta **idempotent backfill**: KFR'siz firmalara seed yönetici Yönetici KFR (`SistemRbacBackfill` + `SistemMigrationManager`).
- [x] `PermissionService`: **Yönetici bypass'ı** ✅ · `ClearCache()` mekanizma hazır; çağrı yerleri **K2/K3** (K1'de giriş sonrası rol/izin yazımı yok; negatif sonuç cache'lenmez).
- [x] `Permission.cs:5` yorum düzeltmesi (Sistem.db).
- [x] Testler: gerçek seed ile `PermissionService` integration (KFR+RolPermission; Yönetici bypass RolPermission sorgulamaz), KFR yazımı, backfill (`RbacK1Tests`, 6 test) — build 0 + **646/646**.

> **Oturum 285 notu:** varsayılan `Kullanıcı` seti = 15 modül `*_Goruntule` + `AiAsistan_Kullan` (default-deny; `Veritabani_*`/`Log_*` ve tüm yazma kapalı; K3'te düzenlenir). Dev DB'de Oturum 284'ten kalan geçici `RolPermission` satırı migration'ı UNIQUE ihlaliyle durduracaktı → migration idempotent yapıldı ve gerçek dev DB kopyasıyla doğrulandı.

### K2-REDESIGN — standart modül deseni (FirmaList/FirmaDetails) 🔨 (Oturum 285 devam — YENİ CONTEXT)
> **Oturum 286 güncellemesi (K2-ÖN ✅):** Desen kaynağı **hazırlandı ve canlı doğrulandı** — `Views/Firmalar/*` Kural 17 + `MuhasibCardStyle` ile yeniden tasarlandı (liste `DataList`+`TableView`, detay `Details`+`FirmalarCard`, master-detail + Pivot) ve FirmaShell'den **"Firma Yönetimi"** butonuyla açılıyor. K2-REDESIGN bu dosyaları birebir referans alır. Detay: `docs/LOG/LOG-261-280.md` Oturum 286 + `KONTROL-LISTESI` "K2-ÖN".
> **Kullanıcı kararı (Oturum 285 devam):** Mevcut K2 view'i **elle yazılmış master-detail** olduğu için **reddedildi** ("bu şekilde bir kullanıcı ekleme sayfası istemiyorum"). İstenen: `FirmaList.xaml` + `FirmaDetails.xaml` (kullanılmayan/örnek) **standart deseni**. K3 izin matrisi **aynı sayfada** olacak. **Kural 8 sınıf onayı ✅ alındı.**
- Silinecek (Kural 4): `Views/KullaniciYonetimi/Components/KullaniciDuzenlePanel.*` + `KullaniciYonetimiViewModel` + `KullaniciDuzenleViewModel` + `KullaniciYonetimiView.xaml(.cs)` (elle yazılan sürüm).
- **Yeni VM'ler** (`ViewModels/ViewModels/KullaniciYonetimi/`): `KullaniciYonetimiViewModel` (orkestratör/composition) · `KullaniciListViewModel : GenericListViewModel<KullaniciModel>` · `KullaniciDetailsViewModel : GenericDetailsViewModel<KullaniciModel>` · `KullaniciRolYetkiViewModel` (K3 izin matrisi).
- **Yeni View'ler** (`Views/KullaniciYonetimi/`): `KullaniciYonetimiView` (Section + Pivot: "Kullanıcılar" + "Roller & İzinler") · `List/KullaniciList` (`DataList`+`BaseConfig`+`TableView` kolonları: Kullanıcı Adı · Ad Soyad · E-posta · Telefon · **Firma Rolü** · Aktif) · `Details/KullaniciDetails` (`Details`+`FormTextBox`/`FluidGrid`) · `Details/KullaniciCard`.
- **Servis/motor korunur:** `IKullaniciService` create/rol (K2) + `IRolYetkiService` (K3).
- **Avatar/profil resmi (kullanıcı isteği, Oturum 285 devam):** firma logosu gibi (`FirmaDetailsViewModel.OnEditPicture` + `IFilePickerService.OpenImagePickerAsync`) **kullanıcı profil resmi** ekleme. `Kullanici` entity'de `Resim`/`ResimOnizleme`, `KullaniciModel`'de `Resim`/`ResimOnizleme`/`ResimSource`/`ResimOnizlemeSource` **zaten var**; `KullaniciCard`'da `PersonPicture` ile gösterilir. **Eksik:** `IKullaniciService.CreateKullaniciAsync`/`UpdateKullaniciAsync` resmi **kalıcılaştırmıyor** (`ToModel` okur ama yazma eşlemesi yok) → create/update'e `Resim`+`ResimOnizleme` yazımı eklenir; `KullaniciDetailsViewModel`'e `EditPictureCommand` + `NewPictureSource` (FirmaDetails deseni).
- **Referans:** `Views/Firmalar/FirmalarView.xaml`, `Views/Firmalar/List/FirmalarList.xaml`, `Views/Firma/Details/FirmaDetails.xaml` + `FirmaCard.xaml`, `FirmaListViewModel`, `FirmaDetailsViewModel`, `GenericListViewModel`, `GenericDetailsViewModel`, `Controls/DataList`, `Controls/Details`. (Araştırma iç desen; dış araştırma gerekmez.)

### K2 — Kullanıcı Yönetimi modal penceresi ✅🧪 (Oturum 285) — ⚠️ VIEW REDESIGN gerekli (yukarı bkz.); motor/servis ✅
- [x] `IKullaniciService`'e create/rol metotları (`CreateKullaniciAsync`, `RolAtaAsync`, `GetKullanicilarWithRolAsync`, `GetRollerAsync`) + `IKullaniciRolRepository` (Scoped) (Kural 5).
- [x] Kapı: `AyarYetkiDenetimi.KullaniciYoneticiMi`. Giriş: `UserInfoControl` admin "Kullanıcı Yönetimi" maddesi.
- [x] **Bug fix (canlı):** `IUserRepository` Singleton→Scoped (pencere-scope context birliği; FK).
- [x] Testler + **Kural 18 canlı** (`ot285_s_kayit`: kullanıcı oluşturma + rol atama çalışıyor).
- [ ] **View redesign** (standart desene) — yeni context.

### K3 — İzin matrisi + rol atama (iki rol) 🔨 (Oturum 285 devam)
- [x] Kullanıcıya firma-bazlı **rol atama** (KFR; `RolAtaAsync`) — K2 motorunda.
- [x] **Servis iskeleti:** `RolIzinModel` + `IRolYetkiService`/`RolYetkiService` (`GetMatrisAsync`, `SetIzinAsync` + `ClearCache`) — **kodlandı, DI kaydı/UI/test YOK** (yeni context).
- [ ] DI kaydı (`AddServicesHostBuilderExtensions` Scoped) + `KullaniciRolYetkiViewModel` + "Roller & İzinler" sekmesi UI (kategori başlıklı 71 izin; `Yönetici` sabit, `Kullanıcı` düzenlenebilir).
- [ ] Testler + canlı (yetkisiz alan kapısı K4).
- [ ] **Özel rol oluşturma YOK** (kullanıcı kararı Oturum 284); `KullaniciRolTip` iki rol kalır.

### K4 — Modül/alan erişim kapısı ⬜
- [ ] `MainShell` modül menüsü: yetkisiz modül **gizli/pasif + gerekçe** (`MainMenuViewModel` + `Permission`).
- [ ] Denetim Masası bölümleri: yetkisiz bölüm gizli (`DenetimMasasiViewModel.MenuGorunurMu`).
- [ ] Sayfa/buton guard'ları: yıkıcı işlemler (sil/güncelle/geri yükle) yetki ister.
- [ ] `IModuleLicenseService` modül lisans kapısı nav'a bağlanır; `ModuleLicenseViewModel` hardcoded `firmaId` düzeltmesi.

### K5 — Hesabım + UserInfoControl menüsü ⬜
- [ ] "Hesabım" yüzeyi: profil (ad/iletişim/avatar) + şifre değiştir (mevcut şifre doğrulamalı).
- [x] `UserInfoControl` menüsü: **"Kullanıcı Yönetimi" (yalnız admin)** maddesi eklendi (K2).
- [ ] **Rol rozeti gerçek** (hardcoded "Admin" kaldırılır, aktif rol adı gösterilir); "Hesabım" maddesi.

### K6 — Doğrulama + doküman ⬜
- [ ] build 0/0 + test (yeni: seed, KFR, PermissionService, Kullanıcı Yönetimi).
- [ ] Kural 18 canlı: yönetici + kısıtlı kullanıcı ile alan erişim matrisi; kanıt + onay.
- [ ] Kural 13 yardım + dokümanlar (bu plan, KONTROL, DURUM, ROADMAP).

## Riskler / açık kararlar
- **Rol matrisi (karar):** iki rol (`Yönetici`/`Kullanıcı`); özel rol yok. `Kullanıcı` rolünün izin seti K3'te düzenlenebilir → modül erişimi rol üzerinden.
- **Firma erişimi = KFR (karar):** `KaydedenId` görünürlük filtresi KFR'ye devredilir; dev verisi sıfırlanacağı için geçiş karmaşası yok. Prod için backfill (var olan firmalara sahibine Yönetici KFR) yine de yazılır.
- **Migration (Kural 8)** onayı olmadan K1 kodlanmaz.
- Bu faz **çekirdeğin parçası**; bitmeden Faz B modülleri kullanıcı bazlı çalışmaz.
