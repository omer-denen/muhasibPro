# MuhasibPro — Yol Haritası

> **Bu proje ana projedir** (tek repo). Eski referans projeler/dönemler (master, WebToXaml, viewpackage, InventEase) kaldırıldı ve `docs/Arsiv/`'tedir. Görsel dil: **Windows 11 Fluent** (Mica-uyumlu).
> **Aktif durum:** `docs/DURUM.md` · **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` + `docs/Arsiv/KARAR-LOGU-ARSIV.md` · **Oturum indeksi:** `docs/LOG.md`.

## 1. Genel Bakış

```
Mimari:
  Libraries/MuhasibPro.Domain    (Entity + Enum + Common)
  Libraries/MuhasibPro.Data      (DbContext + Repository + Database)
  Libraries/MuhasibPro.Business  (Contracts + Services + Validators)  ← UIContracts ayrı değil
  Libraries/MuhasibPro.ViewModels(Infrastructure + ViewModels)
  MuhasibPro                     (WinUI3 App, MuhasibPro namespace)
```

```
Veritabanı (AKIS-PLANI.md):
  %LocalAppData%/MuhasibPro/Databases/ (RELEASE) | Databases/ (DEBUG)
  ├── Sistem.db              → Kullanici, KullaniciRol, KullaniciFirmaRol, RolPermission, Firma, MaliDonem meta, Lisans, AuditLog
  ├── Tenants/{DatabaseName}.db → tenant (TenantDatabaseVersiyonlar: SemVer + kimlik damgası)
  └── Backups/{DatabaseName}_{zaman}_{guid}.backup
```

### Zorunlu kavramlar (korunan)
- `IFirmaWithMaliDonemSelectedService` seçim aynası + `IAppDbContextFactory` (tenant bağlantıları)
- Rol **Kullanici+Firma** (`KullaniciFirmaRol`) — `Kullanici.RolId` kaldırıldı
- Aksiyon bazlı `Permission` + `IPermissionService` + `IModuleLicenseService` + `ILisansService`
- `IDatabaseManagementService` ayrı yönetim penceresi (ileride bağımsız process'e taşınabilir)
- Yedek `VACUUM INTO`, backup-önce-migrate, SemVer şema sürümleri (`DbSchemaVersions`)

## 2. Öncelik — Modüler

### Aşama A — Çekirdek (önce app ayağa kalksın)
Sırasıyla: **Altyapı → GlobalDb migrate+seed → Login → Firma Seç → Mali Dönem Seç → MainShell + UpdateService birleşik (Velopack + SistemMigrationManager)**. Bu fazlar 0–6. **🔒 çekirdek kilidi:** Çekirdek en az kusurda ve güncelleme altyapısı dahil tamamlanmadan **Faz B açılmayacak** (LOG Oturum 25).

### Aşama B — Muhasebe (app ayakta sonra, tek tek)
Her modül çekirdek üzerine bağımsız eklenir: **Cari (B1) → Stok (B2) → Fatura/İrsaliye (B3) → Kasa/Banka (B4) → Çek/Senet (B5) → Sipariş/Teklif (B6) → Personel (B7) → Rapor/Dönem Sonu (B8)**.

## 3. Faz Tablosu

| Faz | Açıklama | Durum |
|---|---|---|
| 0–6 | Dokümantasyon + Domain/Data/Business/ViewModels + WinUI App + Entegrasyon (Aşama A temeli) | ✅🧪 |
| 6.5–6.24 | İlk kurulum/Login, OOBE, DesignTokens Light+Dark, Splash, SistemKurulum, Business DB testleri | ✅🧪 (ayrıntı arşiv) |
| B | Muhasebe modülleri (B1–B8) | ✅🧪 |
| 6.25–6.70 | Tasarım/seçim onarımları, dijital log iskeletleri, kimlikli yedek, ayar panelleri, Denetim Masası | ✅🧪 (ayrıntı arşiv) |
| 6.71 | Refactoring: testler → bug → yıkım → progress → RAF | ✅ Kapandı (Oturum 272) — m.1/m.2 ✅🧪; m.3 **iptal** (zaten yapıldı); m.4 bildirim→**6.86**, progress→**6.88**; m.5 (RAF panel)→**6.88** |
| 6.72 | Çekirdek Stabilizasyon (saf Fluent + Faz B kilidi) | 🔨 Aktif — Dalga 0 ✅🧪; Dalga 1–3 ⬜ |
| 6.73 | Tenant geçiş doğrulaması (WAL checkpoint + dispose + DB log) | ✅🧪 **KAPANDI** — kod mevcut; `TenantCheckpointTests` `ReadWriteCreate` düzeltmesi (Oturum 280) |
| 6.74 | Büyük-iş akış detayı + DB log + log altyapısı | 📋 Plan |
| 6.75 | Windows 11 Fluent geçişi (Mica-uyumlu, tüm ekranlar) | 🔨 Aktif — Splash/Kurulum/token ✅🧪; sırada Login → FirmaShell |
| 6.76 | SistemKurulum→SistemDbYonetim + KurulumSplash akış yeniden tasarımı | ✅🧪 (Adım 1–5; smoke/kalan Fluent alt işleri ⬜) |
| 6.77 | SistemDb Yönetim Operasyonları (yedekle/geri-yükle/güncelle) | 🔨 Aktif — Adım 0–5 ✅🧪; canlı tur |
| 6.78 | Ortak Restore Analizi (çift-veritabanı hükmü) | 🔨 Aktif — Adım 1–3 ✅🧪; sırada Adım 4 canlı tur |
| 6.79 | Sistem.db yaşam-döngüsü güvenliği (açılış yedeği + kapanış WAL) | ✅🧪 |
| 6.80 | Katmanlı sayfa yapısı (zemin → ana border → kartlar) | 🔒 Mühürlü (Katman 2 turu tamam) |
| 6.81 | Sahte transfer alarmı + dialog rötüşü + tek örnek | ✅🧪 |
| 6.82 | Dev-mode (yalnız DEBUG): kimlik/transfer + teşhis araçları | ✅🧪 (kullanıcı onaylı) — **Ek (Oturum 273):** güncelleme kaynağı düzenleme + **Modül Entegrasyon Testleri** (donanım POST) + Tanılama koşucusu |
| 6.83 | TenantDatabaseUpdateView canlı doğrulama turu | ❌ **GEÇERSİZ/KAPATILDI** (Oturum 278) — view 6.91-E'de silindi |
| 6.84 | FirmaShellView seçim deneyimi yeniden tasarımı | ✅🧪 (onaylı "gayet başarılı") |
| 6.85 | Kullanıcı Yönetimi + RBAC (kullanıcı → modül/alan erişimi) | **K1 ✅🧪 + K2 ✅🧪 (Oturum 285):** RBAC motoru (seed/KFR/backfill/Yönetici bypass) **ve** Kullanıcı Yönetimi modalı (FirmaShell kullanıcı menüsü → admin; liste/rol + yeni/düzenle/aktif/şifre/sil) canlı. Canlıda FK bug'ı bulundu+düzeltildi (`IUserRepository` Scoped). build 0 + 655/655; Kural 18 `ot285_s_kayit`. **K3-K6 📋** (`docs/KULLANICI-YONETIMI-PLAN.md`): K3 izin matrisi → K4 modül/alan kapısı → K5 Hesabım → K6 doğrulama. Karar: firma erişimi KFR, iki rol |
| 6.86 | Durum çubuğu + StatusMessage + Notification: redesign + refactor | ✅🧪 KAPANDI (onaylı; commit `2ff1a6c`) — Chunk-1 + ana pencere çubuğu/bağlam + Chunk-2a (in-app InfoBar + OS toast kaldırma); Chunk-2b kalan ring'ler **6.88** |
| 6.87 | Veritabanı Güncelleme sayfası komple yeniden tasarımı | ❌ **GEÇERSİZ/KAPATILDI** (Oturum 278) — sayfa silindi; göç erişimde onay+inline (6.91-E) yönü seçildi |
| 6.88 | MaliDonemYönetimView + SistemDbYonetimView yeniden tasarımı (+ RAF panel temizliği + 6.71 progress) | 📋 Plan (kullanıcı kararı — bu view'lara ara iş yapılmayacak; 6.71/m.3 envanteri + m.5 RAF panel + m.4 progress burada birleşti) |
| 6.89 | LoginView — "Beni hatırla" + QuickLogin (hızlı giriş) incelenmesi | 📋 Plan (kullanıcı kararı — Oturum 272) |
| 6.90 | Velopack git güncelleme akışı (repo adresi dinamik + gömülü varsayılan FeedUrl + admin/dev-mode düzenleme) | 🔨 Kurulum ✅ — `release.yml` dinamik repo, `AssemblyMetadata` gömülü kaynak, admin + dev-mode düzenleme; uçtan uca canlı test (Setup → pack/upload → güncelle) bekliyor |
| 6.91 | Güncelleme altyapısı (product-ready): ön-yedek → göç → doğrulama → restore; app dosyaları + Sistem.db + dönem DB | 🔨 Aktif (Oturum 274/275/277) — **A ✅** veri kökü `%AppData%` + tek seferlik taşıma (uninstall koruması) · **B ✅** zorunlu doğrulanmış Sistem.db ön-yedeği + `OnRestarted` bayrağı · **C ✅** ileri-uyumluluk guard (disk şema > binary → fail-closed) · **D ✅** post-update doğrulama sagası + `GuncellemeSonrasiView`; **Revizyon 3 ✅** (Oturum 277) 3 adım + Atlandı + 4 sonuç hâli (temiz/dikkat/app-fail/sistem-fail) + B1/B2 (`PostUpdatePending`) + Light/Dark `ThemeResource` düzeltmesi. **E ✅ (Oturum 278)** TenantDatabaseUpdateView silme + erişimde onay/**inline** göç (manuel Güncelle butonları kaldırıldı, build 0 + 614/614) → **F** UX + dev-mode + yardım → **G ✅ (Oturum 280)** post-update saga 4. adım `AI Yardım Dizini` (`HazirlaAsync(modelIndirmeyeIzin:false)`, bloklamaz) + `GuncellemeSonrasiView` 4 adımlı şerit; build 0 + 633/633 |
| 6.92 | AI Yardım Asistanı (sürüm-paketli: Profesyonel/Kurumsal + Deneme açık; yerel Foundry süreç-içi, RAG v1) | 🔨 Adım 1-6 ✅ (Oturum 276 kod + Oturum 277 Windows doğrulama: build 0 + **604/604** + C2/C3/C4/C5/C6/C7/C8/C11 ✅). **UI revizyonu (277):** statü çubuğu sağda 🤖 → sağa hizalı **Flyout** (MainShell yerleşimi dışı), panelde gizle ikonu, **model indirme panel içinde** (Denetim'den kaldırıldı). **Sahiplik:** UI 277'de; **servis/contract** (model listele/sil/unload/alias-değişimi) diğer modelde → **Adım 5c ✅ (Oturum 278):** servis→UI bağlandı (Denetim "Model yönetimi" liste+disk+Sil onaylı + alias `Uygula` + indirme yönergeleri; sohbet alias uyuşmazlığı düzeltmesi), build 0 + **617/617** + canlı S1/S2/S3 + restore (`ot278*`). Sırada **Adım 7 kapanış onayı** (veya 6.91-E) |
| 6.93 | AI Yardım Bilgi Tabanı — içeriği view'lardan ayır, `AsistanBilgi.db` + **hibrit RAG** (lexical + Foundry embedding `qwen3-embedding-0.6b` + RRF; FTS5 yok) | 🔨 Plan/sözleşme ✅ (Oturum 278): `docs/YARDIM-DB-PLAN.md` frozen + `REFERANSLAR` 278 (5 satır). **Karar:** yalnız AI · içerik `docs/yardim/*.md` · hibrit · ayrı `%AppData%\MuhasibPro\AsistanBilgi.db`. **Sahiplik:** motor+veri diğer modelde, UI/entegrasyon/doğrulama bende. **Adım 1 motor ✅ (Oturum 279).** **Adım 2 UI/entegrasyon ✅🧪 (Oturum 280):** eski RAG v1 yolu söküldü (Kural 4), DI + panel dizin durumu/ilerlemesi + Denetim `Yardım dizini` kartı + öz-test. **Motor fix ✅ (Oturum 281):** izin kapısı + tek `FoundryYoneticiKurulum`; build 0 + 634/634. **Adım 3 canlı S1-S4 ✅🧪 (Oturum 282):** S1 (60 madde) · S3 (embedding'siz fallback) · S2 (embedding 495 MB + 60×1024 vektör; doğru madde cosine #2 + doğru cevap) · S4 (içerik +1 → 61 + yeni vektör; geri al → 60); Denetim "Yardım dizini" DI wiring fix; sohbet paneli dürüst model durumu + popup Kural 17 tema; build 0 + **640/640**; popup onayı ✅ — **6.93 Adım 3 kapanış onayı bekleniyor** |
| 6.94 | Tek Yardım Yüzeyi (AI Asistanı) — view `?` yardım listeleri/dialogu kaldırılır; tek kaynak `docs/yardim/*.md` (tüm uygulama) → `AsistanBilgi.db` → asistan; giriş F1 + statü çubuğu | 📋 Plan oluşturuldu (Oturum 283): `docs/YARDIM-TEK-YUZEY-PLAN.md` + KONTROL maddeleri (H1-H5). **Kararlar:** yardım kitabı ❌ iptal · `?` yardım kaldırılır · tek kaynak `docs/yardim` · F1 + statü çubuğu · AI yoksa yalnız kilit gerekçesi · eski altyapı silinir (Kural 4) · kapsam tüm yapı. **Sahiplik:** içerik+UI ana modelde, motor diğer modelde. **Kullanıcının ek soruları sonrası başlanacak** |
| 6.95 | Aktivasyon & Modül Kilidi (KEY) + İlk Giriş — Splash → Login (üretici atadığı ilk kullanıcı) → zorunlu şifre değişikliği → Modül Seçici → KEY → Sistem.db modül erişimi → FirmaShell | 📋 Plan (Oturum 284): `docs/AKTIVASYON-MODUL-PLAN.md` (A1-A6) + `REFERANSLAR` 284 (Sage/MYOB/Blackbaud/QuickBooks). Mevcut: `IModuleLicenseService` firma-bazlı (bağlı değil), `Lisans`/`LisansTuru`, seed admin var; KEY UI + zorunlu şifre bayrağı yok. **Tenant DB = etkin modüllerin şeması** (yeni dönem oluşturma + ek modülde dönem DB'lerine göç). **Açık kararlar:** lisans kapsamı + KEY doğrulama (offline/online) + deneme seti + ek modülde hangi dönemler. **6.85 ile birlikte** (modül erişimi = lisans seti ∩ kullanıcı izinleri) |
| 6.96 | Çoklu Veritabanı Sağlayıcı Desteği — **SQLite varsayılan** kalır; PostgreSQL/SQL Server eklenebilir (provider-soyut Data katmanı) | 📋 Plan (Oturum 284): `docs/DB-PROVIDER-PLAN.md` (D1-D8) + kod doğrulamalı SQLite bağımlılık haritası (`SistemDbContext` PRAGMA, DI `UseSqlite`, ham ADO, dosya-başına-tenant, migration tipleri, yedek=dosya kopya, paket sürüm dağınıklığı). **Açık kararlar:** sunucu tenant modeli (ayrı catalog vs TenantId) · bağlantı saklama · yedek anlayışı · hangi sağlayıcılar · `AsistanBilgi.db` SQLite-only mu. Sıra: 6.85/6.95 sonrası D1-D3 |

## 4. Karar Logu (Yeni)

> Geçmiş kararlar (2026-08-21 – 2026-09-15): `docs/Arsiv/KARAR-LOGU-ARSIV.md`. Yeni kararlar bu tabloya eklenir.

| Tarih | Karar | Gerekçe |
|---|---|---|
| 2026-09-15 | **Bu proje ana projedir;** eski referans dönemler (master/WebToXaml/viewpackage/InventEase) arşivde | Kullanıcı kararı (Oturum 271) |
| 2026-09-15 | **Doküman/log yapısı sadeleştirildi:** DURUM.md (tek "nerede kaldık" yüzeyi) + kompakt LOG indeksi + eski cilt/dokümanlar arşivde | Kullanıcı kararı — "model 170k token log okuyor" |
| 2026-09-15 | **Faz 6.86 Chunk-1:** durum çubuğu + StatusMessage/StatusBar servis refactor + `ShellStatusBar` sıfırdan Fluent footer | Kullanıcı onayı (Chunk-1 sınıf planı) |
| 2026-09-15 | **Durum çubuğu ana pencere kararı (çözüldü):** ana pencereye `MainShellView` altına tam genişlik footer olarak eklendi; `FirmaShellView`'de yok; `ShellView` (detay) aynen; mesajlar pencere başına ayrı | Kullanıcı kararı (Oturum 272) |
| 2026-09-15 | **Durum çubuğu bağlamı:** sağ blokta aktif firma + mali dönem + tenant bağlantı noktası birincil, Sistem.db ikincil | Kullanıcı kararı (Oturum 272) — SAP status bar bağlam deseni |
| 2026-09-15 | **Faz 6.88 açıldı:** `MaliDonemYönetimView` + `SistemDbYonetimView` yeniden tasarlanacak; bu iki view'a **ara iş yapılmayacak** (Kural 11 sabit ring düzeltmeleri dahil ertelendi) | Kullanıcı kararı (Oturum 272) |
| 2026-09-15 | **Faz 6.89 açıldı:** `LoginView` "Beni hatırla" + QuickLogin (hızlı giriş) incelenecek | Kullanıcı kararı (Oturum 272) |
| 2026-09-15 | **6.71 kapandı + maddeleri birleştirildi:** m.3 (View yıkım/yeniden tasarım) **iptal** — zaten yapıldı; m.4 bildirim/InfoBar kısmı **6.86 Chunk-2a**'da tamamlandı, progress kısmı **6.88**'e; m.5 (RAF panel temizliği, 16 kart) **6.88**'e (aynı `MaliDonem/Yonetim/.../YonetimAyarlar*` ağacı) | Kullanıcı kararı (Oturum 272) |
| 2026-09-15 | **Bildirim kanalı pencere-başına:** `IInAppMessageService` Scoped; in-app InfoBar yalnız tetikleyen/aktif pencerede görünür, diğer pencerelere yansımaz; `FirmaShellView`'de host **yok** (yönlendirme/listeleme paneli) | Kullanıcı kararı (Oturum 272) |
| 2026-09-15 | **Notification → In-App message; OS toast terk edildi** (CommunityToolkit paketi + AUMID hack'i kaldırılacak); çökme bildirimi = dialog | Kullanıcı kararı — "kullanmayacaksan onu da sil" |
| 2026-09-15 | **Güncel iş sırası:** 6.86 (aktif) → 6.87 | Kullanıcı kararı (Oturum 270/271) |
| 2026-09-15 | **Faz 6.90 açıldı + kuruldu:** Velopack git güncelleme akışı — `release.yml` repo adresi dinamik (`${{ github.repository }}`); uygulama derlemede gömülü git remote'u varsayılan FeedUrl yapar; admin + dev-mode kaynak adresini değiştirebilir | Kullanıcı kararı (Oturum 273) — "git adresi değişebilir, varsayılan mevcut git; dev-mode + admin değiştirebilir" |
| 2026-09-15 | **6.87 v1 (hero + iki sütun redesign) reddedildi;** "referansı sil, baştan tasarım" + "tenant update sayfası gerekli mi, gerçek update ile ilgilenelim" | Kullanıcı kararı (Oturum 273) |
| 2026-09-15 | **İki-katman güncelleme ayrımı:** app update (Velopack, binary) ↔ tenant şema migration (EF Core, erişimde runtime: yedek→göç→doğrula→oto-geri-al). App update tenant DB'yi migrate etmez | Araştırma bulgusu (Kural 14) — Oturum 273 |
| 2026-09-15 | **Dev-mode'a "Modül Entegrasyon Testleri" (donanım POST):** her modülün DI'da kayıtlı/çözülebilir + kritik akışının çalışır olduğunu tek listede doğrular; ayrıca Tanılama (sistem testleri) + güncelleme kaynağı öz-testi. xUnit paketi uygulamada koşmaz (CI) | Kullanıcı kararı (Oturum 273) — "her modül entegre edildiğinde çalışıyor mu; sorun olunca nerede olduğunu buradan gör" |
| 2026-09-15 | **Faz 6.91 açıldı (plan):** güncelleme sonrası uçtan uca doğrulama + kurtarma — pre-update yedek, `OnRestarted` bayrak, async doğrulama (dosya/Sistem.db/tenant) + bozukta yedekten restore. FastCallback'e DB işi konmaz | Kullanıcı kararı (Oturum 273) — "güncelleme sonrası dosya/DB doğrulama ve restore'u uçtan uca yapılandır" |
| 2026-09-16 | **Güncelleme altyapısı kararları (Oturum 274):** veri kökü **`%AppData%\MuhasibPro`** (Velopack uninstall veriyi silmesin); dönem taraması **açılış splash adımı**; ön-yedek **Sistem.db** (+ dönem yedeği göç anında, çünkü yeni migration'lar güncelleme öncesi bilinemez); kod imzası **yok → bütünlük (Velopack checksum) ile başla**, Authenticode sonraki tur | Kullanıcı onayı (5 soru) — sektörel araştırma bulguları (`REFERANSLAR` 274) |
| 2026-09-16 | **TenantDatabaseUpdateView GEÇERSİZ → SİL:** app update (Velopack) ↔ tenant şema göçü ayrımı; post-update kontrol yeni sagada, göç erişimde **onay + inline ilerleme**. Motor (`TenantUpdateAkisYoneticisi`) + dialog + progress kalır | Kullanıcı kararı (Oturum 274) — "geçersizse sil, post-update kontrol için gerekliyse kullan" → analiz: post-update kontrol için gerekli değil |
| 2026-09-16 | **İleri-uyumluluk guard (fail-closed):** disk şema SemVer > binary desteklediği ise Sistem.db + dönem DB **açılmaz/yazılmaz** (göç yok), giriş kapalı. Velopack Setup "repair/reinstall" ile eski binary yeni DB'ye binebileceği için zorunlu | Araştırma (raindex/memtomem/quilltap/chio) + kullanıcı onayı (Oturum 274) |
| 2026-09-16 | **AI asistan = sürüm paketi (Profesyonel + Kurumsal; Deneme'de açık):** `SurumKatalogu` ürün sabiti + `ISurumOzellikService` kapısı; modül-anahtarı değil | Kullanıcı kararı (Oturum 276) — "Enterprise/Pro/Lite gibi sürüm katmanı" |
| 2026-09-16 | **AI çalıştırma = Foundry Local süreç-içi (WinML); IChatClient kullanılmadı:** WinML windows-TFM → impl App katmanında; öz `IAsistanSohbetService` sözleşmesi | Araştırma (NuGet TFM + örnek kod) + "uygula" onayı (Oturum 276) |
| 2026-09-17 | **Faz 6.94 açıldı — Tek Yardım Yüzeyi:** view `?` yardım listeleri/dialogu kaldırılır; tek kaynak `docs/yardim/*.md` → `AsistanBilgi.db` → asistan; giriş **F1 + statü çubuğu**; AI yoksa yalnız kilit gerekçesi; eski altyapı Kural 4 ile silinir; kapsam tüm yapı. Yardım "kitabı" fikri iptal | Kullanıcı kararı (Oturum 283) — "ayrı ayrı yardım sayfası yazmanın anlamı yok, tek yerden güncelleme; asistan varken kullanıcı açıp yardım okumaz" + "asistanı niye koyuyoruz o zaman" |
| 2026-09-17 | **Faz 6.85 genişletildi + ÖNCELİKLİ:** Kullanıcı Yönetimi modülüne ek olarak **RBAC** (kullanıcı→firma rolü→izin) + **modül/alan erişim kapısı** (yetkisiz alan gizli/pasif + gerekçe). Sıra: **önce hızlı eksikler → sonra 6.85**. Plan `docs/KULLANICI-YONETIMI-PLAN.md` | Kullanıcı kararı (Oturum 284) — "eksik iş kalmasın, temiz bir yapıyla devam; kullanıcı bazlı yapı çekirdeğin sonuna bırakıldı, hangi kullanıcı hangi modülü/alanı kullanır, nereye girer, yetkisiz alan nasıl engellenir eksik" |
| 2026-09-17 | **Faz 6.95 açıldı — Aktivasyon & Modül Kilidi (KEY):** akış Splash → Login (üretici atadığı ilk kullanıcı) → zorunlu şifre değişikliği → Modül Seçici → KEY → Sistem.db modül erişimi → FirmaShell; iki katman (modül kilidi=KEY, kullanıcı hakkı=RBAC 6.85) | Kullanıcı kararı (Oturum 284) — "genel olarak muhasebe uygulamaları böyle çalışmıyor mu" → araştırma onayladı (Sage 50 New Key, MYOB lisans dosyası, Blackbaud Unlock Optional Modules + kullanıcı hakkı; `REFERANSLAR` 284) |
| 2026-09-17 | **Faz 6.95 kararları:** lisans kapsamı **kurulum-geneli (Sistem.db global)** · KEY **çevrimdışı imza/checksum** · deneme **anahtarsız 30 gün + kısıtlı set** · ek modül **tüm açık dönemler + erişimde onay/inline göç** · **tenant DB etkin modül şemasıyla** | Kullanıcı kararı (Oturum 284) |
| 2026-09-17 | **Faz 6.96 açıldı — Çoklu DB Provider (SQLite varsayılan):** SQLite ağ/çok-kullanıcı için sınırlı; **SQLite varsayılan kalır**, Data katmanı provider-soyut yapılır (PostgreSQL/SQL Server eklenebilir) | Kullanıcı kararı (Oturum 284) — "SQLite varsayılan olarak açılsın, çoklu provider desteği de ekleyelim" |
| 2026-09-17 | **Faz 6.85 K1 uygulandı (RBAC temeli):** `RolPermission` seed (Yönetici=76 tüm izin / Kullanıcı=16 temel görüntüleme+AI) · firma oluşturana Yönetici KFR · açılış backfill · `PermissionService` Yönetici bypass · migration idempotent (`INSERT OR IGNORE`; dev DB'deki geçici satırla çakışmaz) | Kullanıcı onayı (Oturum 284 Kural 8) + Oturum 285 uygulaması |
| 2026-09-17 | **Faz 6.85 K2 uygulandı (Kullanıcı Yönetimi modalı):** `UserInfoControl` (admin) → Kullanici Yonetimi penceresi; liste + rol + yeni/düzenle/aktif/şifre/sil; `IKullaniciService` create/rol; canlıda FK bug'ı → **`IUserRepository` Singleton→Scoped** (pencere-başına DI scope; ServiceLocator) | Kullanıcı onayı (K2 sınıf + DI lifetime Kural 8) + Oturum 285 canlı testi |

## 5. Referanslar
- `docs/DURUM.md`, `docs/LOG.md`, `docs/AKIS-PLANI.md`, `docs/WINUI-MIMARISI.md`, `docs/TASARIM-KURALLARI.md`, `docs/REFERANSLAR.md`
