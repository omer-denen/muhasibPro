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
| 6.71 | Refactoring: testler → bug → yıkım → progress → RAF | 🔨 Aktif — madde 1 ✅🧪; madde 3 atlandı; 4/5 sürüyor |
| 6.72 | Çekirdek Stabilizasyon (saf Fluent + Faz B kilidi) | 🔨 Aktif — Dalga 0 ✅🧪; Dalga 1–3 ⬜ |
| 6.73 | Tenant geçiş doğrulaması (WAL checkpoint + dispose + DB log) | 📋 Plan |
| 6.74 | Büyük-iş akış detayı + DB log + log altyapısı | 📋 Plan |
| 6.75 | Windows 11 Fluent geçişi (Mica-uyumlu, tüm ekranlar) | 🔨 Aktif — Splash/Kurulum/token ✅🧪; sırada Login → FirmaShell |
| 6.76 | SistemKurulum→SistemDbYonetim + KurulumSplash akış yeniden tasarımı | ✅🧪 (Adım 1–5; smoke/kalan Fluent alt işleri ⬜) |
| 6.77 | SistemDb Yönetim Operasyonları (yedekle/geri-yükle/güncelle) | 🔨 Aktif — Adım 0–5 ✅🧪; canlı tur |
| 6.78 | Ortak Restore Analizi (çift-veritabanı hükmü) | 🔨 Aktif — Adım 1–3 ✅🧪; sırada Adım 4 canlı tur |
| 6.79 | Sistem.db yaşam-döngüsü güvenliği (açılış yedeği + kapanış WAL) | ✅🧪 |
| 6.80 | Katmanlı sayfa yapısı (zemin → ana border → kartlar) | 🔒 Mühürlü (Katman 2 turu tamam) |
| 6.81 | Sahte transfer alarmı + dialog rötüşü + tek örnek | ✅🧪 |
| 6.82 | Dev-mode (yalnız DEBUG): kimlik/transfer + teşhis araçları | ✅🧪 (kullanıcı onaylı) |
| 6.83 | TenantDatabaseUpdateView canlı doğrulama turu | 🔨 Kısmi — ekran doğrulandı; **6.87'ye devredildi** |
| 6.84 | FirmaShellView seçim deneyimi yeniden tasarımı | ✅🧪 (onaylı "gayet başarılı") |
| 6.85 | Kullanıcı Yönetimi modülü (ayrı MODAL pencere) | 📋 Plan |
| 6.86 | Durum çubuğu + StatusMessage + Notification: redesign + refactor | 🔨 Aktif — **Chunk-1 ✅🧪 onaylı** + **ana pencere çubuğu/aktif bağlam ✅🧪 onaylı (Oturum 272)**; Chunk-2 (In-App message + OS toast kaldırma + progress) sırada |
| 6.87 | Veritabanı Güncelleme sayfası komple yeniden tasarımı | 📋 Plan (Kural 14 araştırması hazır) |

## 4. Karar Logu (Yeni)

> Geçmiş kararlar (2026-08-21 – 2026-09-15): `docs/Arsiv/KARAR-LOGU-ARSIV.md`. Yeni kararlar bu tabloya eklenir.

| Tarih | Karar | Gerekçe |
|---|---|---|
| 2026-09-15 | **Bu proje ana projedir;** eski referans dönemler (master/WebToXaml/viewpackage/InventEase) arşivde | Kullanıcı kararı (Oturum 271) |
| 2026-09-15 | **Doküman/log yapısı sadeleştirildi:** DURUM.md (tek "nerede kaldık" yüzeyi) + kompakt LOG indeksi + eski cilt/dokümanlar arşivde | Kullanıcı kararı — "model 170k token log okuyor" |
| 2026-09-15 | **Faz 6.86 Chunk-1:** durum çubuğu + StatusMessage/StatusBar servis refactor + `ShellStatusBar` sıfırdan Fluent footer | Kullanıcı onayı (Chunk-1 sınıf planı) |
| 2026-09-15 | **Durum çubuğu ana pencere kararı (çözüldü):** ana pencereye `MainShellView` altına tam genişlik footer olarak eklendi; `FirmaShellView`'de yok; `ShellView` (detay) aynen; mesajlar pencere başına ayrı | Kullanıcı kararı (Oturum 272) |
| 2026-09-15 | **Durum çubuğu bağlamı:** sağ blokta aktif firma + mali dönem + tenant bağlantı noktası birincil, Sistem.db ikincil | Kullanıcı kararı (Oturum 272) — SAP status bar bağlam deseni |
| 2026-09-15 | **Notification → In-App message; OS toast terk edildi** (CommunityToolkit paketi + AUMID hack'i kaldırılacak); çökme bildirimi = dialog | Kullanıcı kararı — "kullanmayacaksan onu da sil" |
| 2026-09-15 | **Güncel iş sırası:** 6.86 (aktif) → 6.87 | Kullanıcı kararı (Oturum 270/271) |

## 5. Referanslar
- `docs/DURUM.md`, `docs/LOG.md`, `docs/AKIS-PLANI.md`, `docs/WINUI-MIMARISI.md`, `docs/TASARIM-KURALLARI.md`, `docs/REFERANSLAR.md`
