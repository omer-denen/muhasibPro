# MuhasibPro — Akış Planı

Bu doküman, `MuhasibPro-Akis-Plani.md` ve `MuhasibPro-Veritabani-Yonetimi.md` kullanıcı dokümanlarının
master referanslı yeni projeye uygulanmış halidir.

## 1. Mimari Katmanlar

```
WinUI3 app (Faz 6)
  → MuhasibPro.Business   (servisler, permission, auth, DTO, validator)   [Faz 4]
    → MuhasibPro.Data     (DbContext'ler, repository, manager'lar)        [Faz 2-3]
      → MuhasibPro.Domain (entity'ler, enum'lar, common, exceptions)      [Faz 1]
```

## 2. Veritabanı Yapısı

```
Sistem.db  (SistemDbContext)
  Kullanici, KullaniciRol, KullaniciFirmaRol, RolPermission, Firma,
  MaliDonem (meta + durum), AuditLog, Lisans, GlobalAyarlar, OturumKaydi, SistemLog, Hesap,
  AppDbVersiyonlar (CurrentDatabaseVersion = SemVer, örn 1.1.0)

{MaliDonem}.db  (AppDbContext, her dönem ayrı dosya)
  TenantDatabaseVersiyonlar (CurrentTenantDbVersion = SemVer + kimlik damgası:
  FirmaId/MaliDonemId/OlusturanKullaniciId(rol)/MakineId/KurulumId) + AppLogs
```

Klasör ağacı (RELEASE: `%LocalAppData%/MuhasibPro/Databases/`, DEBUG: `Databases/`):

```
Databases/
  ├── Sistem.db
  ├── Tenants/{DatabaseName}.db      → örn db-KODU_2027.db (düz liste, firma-alt-klasörü yok)
  └── Backups/{DatabaseName}_{yyyyMMdd_HHmmss}_{guid}.backup
```

Şema sürümü SemVer'dir (`DbSchemaVersions` haritası: Initial→1.0.0, AddTenantIdentity→1.1.0;
`SemanticVersion` sayısal karşılaştırır). Yeni migration = haritaya bir satır.

## 3. Uygulama Açılış Akışı (Aşama A)
```
Splash (SplashRoutingService.DecideRouteAsync → Sistem hazır mı?)
  → Sistem.db YOKSA: SistemKurulum (onaylı ilk oluşturma: kullanıcı başlatır, saga + damga)
  → Transfer şüphesi (CheckTransferAsync: damga tara) → TransferDialog (onay/ret)
  → Backfill (eksik kimlik damgaları, best-effort)
  → Login (Sistem.db, PBKDF2, IdentitySettings kilit eşikleri, 5 deneme/5dk)
  → Firma seç (KullaniciFirmaRol'e göre yetkili firmalar; rol firma seçiminde netleşir → IPermissionService)
  → Dönem seç (CheckUpdateRequiredAsync → güncelleme varsa ön-dialog [Güncelle/Daha sonra/Vazgeç]
     → TenantDatabaseUpdateView [Yedek→Göç→Doğrulama→oto geri alma] → SwitchTenantAsync + TenantUpdated)
  → MainShell (yetkiye göre menü + lisans türü, DetailsWindow ile modüller)
```

## 4. Veritabanı Yaşam Döngüsü
| İşlem | Akış |
|---|---|
| **Dönem oluştur** | Sistem.db kayıt (Durum=Açık) → dosya oluştur → migrate → sürüm damgası + kimlik damgası (Firma/Dönem/Rol/Makine/Kurulum) |
| **Yedekle** | `VACUUM INTO` → `Backups/*.backup` + SonYedekTarihi/Boyut satıra yazılır + `TenantBackupCompletedEvent` (liste+tarama tazelenir); FIFO limiti `DatabaseSettingsModel`'den |
| **Geri yükle** | Tek kapı `RestoreVerifyDialog` (hüküm motoru: eski→Warning+auto-migrate / yeni→Block / kimliksiz→Warning) → `TenantRestoreCompletedEvent` |
| **Arşivle** | Bayrak (`Durum=Arsivlenmis`, dosya taşıma YOK) + onaylı Kapat / Arşivden Çıkar |
| **Sil (soft)** | Kayıt silme saga korumalı + audit. Kalıcı sil ayrı yetki (`Veritabani_KalıcıSil`) + şifre onayı |
| **Güncelle (şema)** | `TenantUpdateAvailableEvent` → ön-dialog → sayfa (Yedek→Göç→Doğrulama→oto geri alma→`ValidateAsync`) |
| **Transfer** | Açılış taraması (kurulum/makine damga karşılaştırma) → `TransferDetectedEvent` + `TransferDialog` |

## 5. Silme Guard-Clause’ları
1. Dönem aktif/seçili mi? → engelle
2. Başka kullanıcı kullanıyor mu? (OturumKaydi/heartbeat) → engelle
3. Bağımlı sonraki dönem var mı? → uyar/engelle
4. Yetki (`Veritabani_Sil`) var mı?
5. Yedek alındı mı? → sor + logla
6. Onay kodu doğrulaması → yoksa başlamaz

## 6. Yetki Matrisi (Veritabanı Yönetimi)
| Permission | Admin | Kıdemli | Denetçi | Muhasebeci |
|---|---|---|---|---|
| Veritabani_Goruntule | ✅ | ✅ | ✅ | ❌ |
| Veritabani_YedekAl | ✅ | ✅ | ❌ | ❌ |
| Veritabani_GeriYukle/Arsivle/Sil | ✅ | ❌ | ❌ | ❌ |
| Veritabani_KalıcıSil | ✅ (+şifre) | ❌ | ❌ | ❌ |
Buton aktif = yetki AND dönem "Açık" AND kilitli değil.

## 7. Olay Abonelik Matrisi (tipli bus — `IEventBus`, Faz 6 mührü)
| Olay | Yayıncı | Abone |
|---|---|---|
| `AppSettingsChangedEvent` | AppPlatformSettingsProvider, DatabaseSettingsVM, UpdateVM | — (sözleşme; abone Faz B'de) |
| `TransferDetectedEvent` | SplashRoutingService | — (dialog senkron akışta açılır; olay sözleşme) |
| `TenantUpdateAvailableEvent` | TenantDatabaseUpdateService | MaliDonemListVM (rozet), FirmaShellVM (durum+toast) |
| `TenantBackupCompletedEvent` | TenantSQLiteDatabaseOperationService | DonemYedeklerVM (liste), BilinmeyenVM (tarama) |
| `TenantRestoreCompletedEvent` | TenantSQLiteDatabaseOperationService | DonemYedeklerVM (liste), BilinmeyenVM (tarama) |

Kurallar: yeni kod ham string `Send` kullanmaz (tipli bus); `ThemeChanged` C# event'idir, bus dışı kalır; Subscribe = Unsubscribe (kapanışta scope dispose + VM Unsubscribe). Eski string-bus akışları (QuickLogin, ItemSelected/Changed, TenantUpdated) grandfathered — Faz B'de tipliye taşınır.

