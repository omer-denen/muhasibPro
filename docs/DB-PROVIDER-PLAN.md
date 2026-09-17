# MuhasibPro — Faz 6.96: Çoklu Veritabanı Sağlayıcı Desteği (SQLite varsayılan) Planı

> **Faz 6.96.** **SQLite varsayılan** kalır; veri katmanı **provider-soyut** hâle getirilir ve **PostgreSQL / SQL Server** gibi sunucu sağlayıcıları eklenebilir olur.
> **Durum:** 📋 Plan (Oturum 284, kod yok). Kaynak: SQLite bağımlılık haritası (kod doğrulamalı).
> **Karar (kullanıcı, Oturum 284):** "SQLite varsayılan olarak açılsın, çoklu provider desteği de ekleyelim."

## Neden
- Tek makine (masaüstü) bugünkü hedef → SQLite **doğru ve yeterli** (RBAC, modül kilidi, dosya-başına-tenant, yedek).
- Ancak **LAN'da çok kullanıcı** / ileride sunucu ihtiyacı doğarsa SQLite ağ paylaşımında riskli (WAL = tek yazıcı, SMB kilidi güvenilmez).
- Erken karar ucuz: provider soyutlaması şimdi kurulursa sunucuya geçiş veri kaybetmeden yapılabilir.

## Mevcut durum (SQLite bağımlılık haritası — Oturum 284)
| Alan | Durum | Ref |
|---|---|---|
| `SistemDbContext` ctor PRAGMA (WAL) kancası | **SQLite'a bağımlı** | `DataContext/SistemDbContext.cs:30-48` |
| DI `AddDbContext` + `UseSqlite` + elle connection string | **SQLite'a bağımlı** | `Business/HostBuilder/AddDbManagerHostBuilderExtensions.cs:35-51` |
| `AppDbContextFactory` (`UseSqlite`, Error 14/26) | **SQLite'a bağımlı** | `Data/Database/Common/AppDbContextFactory.cs:46-109` |
| Design-time factory'ler | **SQLite'a bağımlı** | `DataContext/Factories/DesignTime*Factory.cs:18` |
| Tenant connection factory (`SqliteConnectionStringBuilder`, Mode/Cache/ForeignKeys) | **SQLite'a bağımlı** | `Data/Database/TenantDatabase/TenantSQLiteConnectionStringFactory.cs:34-120` |
| Ham ADO (`SqliteConnection`, PRAGMA, `sqlite_master`, `ClearAllPools`) | **SQLite'a bağımlı** | `TenantSQLiteDatabaseManager`, `DatabaseBackupManager`, `SistemBackupManager`, `SistemSnapshotReader`, `DbContextAnalysisExtensions`, `YardimVektorDeposu` |
| Migration'lar SQLite tip sistemi (`TEXT/INTEGER/BLOB`) | **SQLite'a özgü** | `Migrations/**`, `Migrations/AppDb/**` |
| Dosya-başına-tenant (yol/ad/magic-byte/WAL-SHM/File.Delete) | **dosya bağımlı** | `ApplicationPaths.cs:154-437`, `TenantSQLiteDatabaseManager.cs:159-302` |
| Yedek/restore = dosya kopya + `File.Replace` + `.lock` | **dosya bağımlı** | `DatabaseBackupManager.cs:68-145,329-425` |
| `ITenantSQLite*` arayüz adları | provider adı koda gömülü | `ITenantSQLiteSelectionManager` vb. |
| Paket sürümleri dağınık | ⚠️ uyumsuz | `Data`: EFCore 9.0.11 + `Microsoft.Data.Sqlite.Core 10.0.2`; UI: `EFCore.Design 9.0.12`; `SQLitePCLRaw` 2.1.12 (Business/VM/Tests) vs 3.53.3 (UI) |
| `appsettings.json` `ConnectionStrings` | ölü yapılandırma | `MuhasibPro/appsettings.json:6-10` |

## Hedef mimari (soyutlama)
```
IDatabaseProvider (Sqlite | PostgreSql | SqlServer)
   ├─ OptionsFactory  → UseSqlite/UseNpgsql/UseSqlServer + PRAGMA/interceptor
   ├─ IConnectionFactory (Sistem + Tenant)
   ├─ ITenantStore  (exists/create/delete/checkpoint/scan)  → SQLite: dosya+WAL | Server: CREATE/DROP DATABASE
   ├─ IMigrationSet  (provider başına ayrı migrations assembly/klasör)
   ├─ IDatabaseDiagnostics (integrity + tablo listesi)      → PRAGMA/sqlite_master | information_schema
   └─ IBackupStrategy (yedek/restore)                       → dosya kopya | pg_dump/BACKUP DATABASE
```

## Açık kararlar (kullanıcıya)
1. **Sunucu provider'da tenant fiziksel modeli:** (a) **dönem başına ayrı database/catalog** (mevcut dosya-başına-DB modelinin karşılığı; yönetim/backup daha karmaşık) mi, (b) **tek DB + `TenantId` discriminator** (global query filter + repository değişikliği) mi?
2. **Bağlantı bilgisi saklama:** sunucu/port/kullanıcı/parola nerede + güvenli saklama (DPAPI/credential)?
3. **Yedek anlayışı:** dosya kopya yerine native backup mu; yedek listeleme metadata'sı nereden?
4. **`AsistanBilgi.db`** (cache) → **SQLite-only kalsın mı?** (öneri: evet)
5. **Veri taşıma** (`DataPathRelocationService`) → SQLite-only mı kalacak?
6. **Hangi sağlayıcılar** desteklenecek: PostgreSQL mi, SQL Server mı, ikisi mi?

## Adımlar
### D1 — Kararlar + sürüm/paket hizalama ⬜
- [ ] Açık kararlar (1-6) + `REFERANSLAR` araştırması (EF provider stratejisi, offline imza ile ilgisiz)
- [ ] Paket sürümlerini hizala; SQLite paketlerini tek "provider" noktasına topla; server paketlerini ekle

### D2 — Provider soyutlaması (Data) ⬜
- [ ] `IDatabaseProvider` + `DbContextOptions` factory; `UseSqlite` çağrılarını tek noktaya topla
- [ ] PRAGMA kancasını `IDbConnectionInterceptor`'a taşı (`SistemDbContext` ctor'dan çıkar)
- [ ] `IConnectionFactory` (Sistem + Tenant); `ITenantSQLite*` adlarını provider-nötr yap

### D3 — Tenant depolama soyutlaması ⬜
- [ ] `ITenantStore` (exists/create/delete/checkpoint/scan); SQLite impl = dosya+WAL, server impl = catalog
- [ ] Dönem/kimlik damgası kontrolünü `File.Exists`'ten bağımsızlaştır

### D4 — Migration seti (provider başına) ⬜
- [ ] `MigrationsAssembly` yapılandır + `Migrations/Sqlite|PostgreSql|SqlServer` klasörleri
- [ ] Mevcut migration'ları SQLite seti olarak koru; diğer provider'lar için yeniden üret

### D5 — Diagnostics + yedek stratejisi ⬜
- [ ] `IDatabaseDiagnostics` (integrity/tablo/row) soyutla
- [ ] `IBackupStrategy` (SQLite: kopya+WAL; server: native backup/restore)

### D6 — Ayar/UI ⬜
- [ ] `DatabaseSettingsModel`/`AppPlatformSettings`'e `Provider` + bağlantı alanları; Denetim → Veritabanı bölümünde seçim + bağlantı testi
- [ ] Varsayılan = SQLite; provider değişimi uyarı + ön-yedek akışı

### D7 — Test altyapısı ⬜
- [ ] Provider-agnostic fixture (SQLite temp, PostgreSQL testcontainer, SQL Server LocalDB)
- [ ] Mimari testine provider sızıntı denetimi

### D8 — Doğrulama + doküman ⬜
- [ ] build 0/0 + test + Kural 18 canlı (SQLite varsayılan; opsiyonel server smoke)
- [ ] Dokümanlar + `REFERANSLAR`

## Sıra / ilişki
- **6.96 bağımsız bir refactor'dur**; 6.85 (RBAC) ve 6.95 (aktivasyon) SQLite varsayılanla yürür.
- **K1 (RBAC migration)** SQLite setine yazılır; diğer provider migration'ları D4'te üretilir.
- Öneri: **6.85 K1 → 6.95 A1/A2 → 6.96 D1-D3** (soyutlama erken kurulursa sonraki işler provider-nötr ilerler).

## Riskler
- **Ağ/çok kullanıcı** ihtiyacı netleşmeden server provider'a geçmek gereksiz karmaşıklık yaratır → D1 kararları netleşmeden D2+ kodlanmaz.
- **Dosya-başına-tenant** ile **tek-DB+TenantId** arasındaki seçim, tüm repository ve yedek akışını etkiler (en kritik karar).
- Migration'ların provider başına yeniden üretimi dikkat ister (mevcut SQLite migration'ları korunmalı).
- Paket sürüm uyumsuzlukları şimdi hizalanmazsa çoklu provider derlemesi kırılır.
