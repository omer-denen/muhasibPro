# MuhasibPro — Kontrol Listesi (Yeni)

Marker: `⬜` bekliyor · `🔨` aktif · `✅` kod eklendi · `🧪` derleme doğrulandı

---

## Sıradaki iş kuyruğu (Oturum 180 envanteri → Oturum 181 kaydı; ilk açılışta bu sıra)
- [x] **0. Windows borcu:** build 0/0 + test 251/251 + smoke (20 sn, çökme yok) — Oturum 182 ✅🧪; CS8625 `null!` fix; KONTROL'deki "beklenti 247/247" notu gerçek 251 ile düzeltildi sayılır
- [x] **0-canlı piksel tur (kullanıcıda):** 161-172 rötuşları + 6.66/6.67 — kullanıcı onayı ✅ (Oturum 182)
- [x] **1. B1 rozet:** A seçildi (kullanıcı) — `LoadDataAsync` artık `AnalyzeAllDbStatusesAsync` (Task.WhenAll, Oturum 72 deseni) + E1 abonesi `OrdinalIgnoreCase`; `MaliDonemTopluAnalizTests` 2 test — build 0/0, test 253/253, smoke OK ✅🧪 (Oturum 183)
- [x] **2. Güncelle butonu yok:** zincir tam kuruluymuş (rapor-öncesi kalmış) — DonemOzetCard uyarı barı + buton (`DbGuncellemeGerekliMi`), DataContext/Root, `GuncelleClick→OnGuncelleClick→UpdateView`; `YonetimGuncelleAksiyonTests` 1 test — build 0/0, test 254/254, smoke OK ✅🧪 (Oturum 183)
- [x] **3. B2 güncelleme tetikleme:** wiring Oturum 141'den ✅'ydi; B1-A ile tetikleme genişledi (her liste yüklemede tüm dönemler + seçimde sayfa analizi) ve testli (`MaliDonemTopluAnalizTests` + `YonetimGuncelleAksiyonTests`) — kod değişikliği yok; canlı tetik eski-şemalı DB ister (3 dönem güncel) → kullanıcıda ✅ (Oturum 184)
- [x] **4. BilinmeyenPanel yanlış listeleme:** Oturum 138'den kalma çift kayıt — Oturum 141'de çözülmüştü; kod teyit + planlı filtre testi eklendi (`YedekOlayTests` 6/6) — build 0/0, test 255/255 ✅🧪 (Oturum 184)
- [x] **5. Continue anomalisi** (MainShell yerine Yönetim): kök neden bulundu — yönetim sayfasından girilen akışta yerel seçim eşleşmeyip guard sessizce düşüyordu; fix: ayna-yedeği + `OrdinalIgnoreCase`; `ContinueNavigasyonTests` 3 test — build 0/0, test 258/258, smoke OK ✅🧪 (Oturum 185)
- [x] **6. Ölü dialog:** `TenantDatabaseUpdateDialog` ölü DEĞİL — `Coordinator:57` ön-dialog (`ShowTenantUpdateConfirmAsync` → Güncelle/Daha sonra/Vazgeç → UpdateView), testli (`TenantDatabaseUpdateTests:279,309`); silinmedi, bağ doğrulandı ✅ (Oturum 185)
- [x] **7. 6.61 diğer view ayar panelleri** ✅🧪 — **Oturum 201: Firma/Dönem GERÇEK bölümler** (`FirmaKayitAyarlarViewModel` desen+durum-enum+sayfa/strict + `DonemAyarlarViewModel` 8 eşik + Denetim composition + 2 Panel + 2 Sayfa + routing + Startup + `DenetimFirmaDonemTests` 6 test; `YakindaViewModel/Sayfası` Kural-4 silindi) — build 0/0, test 312/312, smoke OK; canlı bölüm turu kullanıcıda
- [x] **8. Mapster (NU1903)** ✅🧪 — **Oturum 201: AutoMapper SİLİNDİ** (`AutoMapperSistemMapping` Profile + klasör + 4 csproj referansı; `IMapper` kullanımı 0'dı, Mapster'a gerek kalmadı) — build 0/0
- [x] **9. Legacy 6 dosya → Faz B** ✅ — **Oturum 201: 6 dosya da CANLI** (Startup Register + host caller'lı) → silinemez; kullanıcı kararı "Faz olarak ekle" — Faz B kapsamına faz-kaydı işlendi
- [ ] **Kullanıcı sepeti (paralel):** E2E saga, splash/kurulum, hover-tooltip, login tıklama, sayaç-grafik, 6.64/6.65 görselleri ⬜

## Faz 6.69 — Mali Dönem Yönetim revizyonu (4 kritik sorun + Kural 11 — Oturum 202, ✅🧪)
- [x] **AGENTS Kural 11:** busy bayrağı + ring + sonuç + zaman aşımı (sürekli dönme yasak) — `AGENTS.md:11` ✅ Eklendi
- [x] **1. Açılış lazy-load:** `DurumAnalizleriniYukleAsync` + `DerinAnalizleriYukleAsync` bulk döngüleri silindi (ölü kod); `YenileAsync` filtre+KPI'ya indi; `TaraAsync` açılıştan çıktı (fire-and-forget + sayaç tazeleme); KPI derin-yokken dürüst metin ✅ Eklendi 🧪 Test edildi
- [x] **2. Dönem silme:** DeleteGuard "Yedekleri de sil" checkbox (varsayılan işaretli) + `DeleteAllTenantBackup=false` yolu bağlandı; "Silinen Dönem Yedeği" ayrı listesi (kimlik-id, migration yok); ad-eşleşme `OrdinalIgnoreCase`; kilitli-dosya-adlı abort mesajı ✅ Eklendi 🧪 Test edildi
- [x] **3. Yedek silme:** `YedekSilAsync` + `TemizleAsync` bool kontrolü (başarısızda Danger + liste gerçeği); çift-refresh sadeleşti (ağır `RefreshAllAsync` → vitrin sayacı + `TazeleSayaclar`) ✅ Eklendi 🧪 Test edildi
- [x] **4. Sayfa düzeni:** sol pane yalnız dönem listesi; sağ sıklık sırası (yedek+parametre → işlemler → silinen+bilinmeyen → analiz → ayarlar); ölü sekme sistemi silindi ✅ Eklendi 🧪 Test edildi
- [x] **Kural 11 uygulaması:** 6 busy bayrağı + 4 boş-durum bayrağı + 30sn tarama/yükleme tavanı; `YonetimSilmeYuklemeTests` 4 test — build 0/0, test 316/316, smoke OK ✅ Eklendi 🧪 Test edildi
- [ ] **Canlı (kullanıcıda):** açılış hızı + dönem silme (checkbox'lı) + yedek-silme başarısızlığı + panel sırası/ring'ler ⬜

## Faz 6.70 — Yerleşim düzeltme + silme sertleştirme + saklama koruması (Oturum 203, ✅🧪)
- [x] **Sekmeli dialog:** `YetimYedeklerDialog` (Silinen/Bilinmeyen sekmeleri + sayaçlı başlıklar, mevcut paneller reuse) + navbar'da 2 sayaçlı buton; Row-3 kartları sayfadan çıkarıldı ✅ Eklendi 🧪 Test edildi
- [x] **Silme sertleştirme:** `TryDeleteBackupFileAsync` (neden-plumbing + salt-okunur temizliği + 5 deneme); Danger nedeni söyler; temp-dosya regresyon testi ✅ Eklendi 🧪 Test edildi
- [x] **Saklama koruması:** sayı `GetManuelKeep` altına düşecekse `YedekSilOnayDialog` yazılı onayı (kod = dosya adı); `IDialogService.ShowBackupDeleteGuardAsync` + 3 test ✅ Eklendi 🧪 Test edildi
- [x] **Doğrulama:** build 0/0, test 320/320, smoke OK ✅🧪
- [ ] **Canlı (kullanıcıda):** dialog sekmeleri + yazılı onay + silme sonucu (hiçbir şey mi / neden-mesajı mı) ⬜

## Faz 6.71 — Refactoring: testler → bug → yıkım → progress → RAF (Oturum 204 plan, Oturum 205 faz kaydı)
- [x] **Plan + Kural 12:** 4 salt-okur tarama (sağlık/bildirim/gerçeklik/envanter) + `AGENTS.md:12` işlem-görseli disiplini 📋
- [x] **1. Test seferberliği (davranış mührü):** ✅🧪 (Oturum 211: 4 dosya ~98 nokta; Oturum 212: GetById guard fixi; Windows `dotnet test` 426/426)
  - [x] `TenantSilmeServisTests` (yeni): `DeleteTenantDatabaseAsync` varyantları (id≤0, ad-null, satır-ad uyuşmazlığı, case-farklı ad kabulü, `IsDeleteDatabase=false`, before-backup true/false, yedek-korulu, kısmi silme) + `CleanAllBackupsAsync` (kısmi/0/dizin-yok/boş-liste) + `TryDelete` retry yolu ✅ kod eklendi (Oturum 211; 🧪 Windows'ta)
  - [x] `TenantOperationTests` (yeni): `RestoreBackupAsync`/`CreateBackupAsync` hata dalları, `BakimCalistirAsync`, `GetDerinAnalizAsync` (Hata-dolu dal dahil), `CleanOldBackupsAsync` FIFO (`keepLast<1`, valid≤keep, `deletedCount≤0`), `GetBackupHistoryAsync`/`GetAllBackupsAsync`, `GetLastBackupDate`, `RestoreFromLatestBackupAsync`, `DeleteBackupDatabaseAsync` ✅ kod eklendi (Oturum 211; 🧪 Windows'ta)
  - [x] `MaliDonemServiceTests` (yeni): `GetByMaliDonemIdAsync`, `GetMaliDonemlerPageAsync`, `IsMaliDonemAny/ExistsAsync`, `GetMaliDonemlerCountAsync`, `CreateNewMaliDonemForFirmaAsync`, `UpdateMaliDonemAsync`, `RestoreMaliDonemAsync`, `DeleteMaliDonemAsync` guard'ları, `GetMaliDonemlerWithFirmaId` ✅ kod eklendi (Oturum 211; 🧪 Windows'ta)
  - [x] `YonetimViewModelTests` (yeni): `LoadAsync`/`RefreshAllAsync` (sayaç+seçim koruma), `AyarSonrasiTazeleAsync`, `DerinAnalizYukleAsync` (null/busy/exception), `BakimCalistirAsync` (TUMU/tek/kısmi-ok), `DonemSeciliMi`, `TazeleSayaclar`, `YedekAlAsync` (FIFO dahil), `GeriYukleAsync` (null/boş/onay-false/fail), `Arsivle/ArsivdenCikar` (null/onay-false/fail), `Temizle/SilinenTemizle` (null/onay-false/fail), `YenileAsync`/`Filtrele`/KPI dalları, `TopluYedekle/TopluTest/TopluBakim` ✅ kod eklendi (Oturum 211; 🧪 Windows'ta)
- [x] **2. Kritik bug kapanışı:** ✅🧪 (Oturum 213: 4 madde + 4 test; 430/430)
  - [x] Yazılı-onay reti Info toast'ı + OrdinalIgnoreCase (YedekSilOnayDialog + DonemYedeklerViewModel)
  - [x] LoadAsync/RefreshAllAsync 30sn timeout + Warning toast "mevcut veriler gösterilmektedir"
  - [x] KPI hibrit-c: analizliDurum=0 → "Durum analizi bekleniyor" (%0 yalanı kapandı); TopluAnalizTamamlandi olayı + KPI tazeleme
  - [x] Bayat-exe derleme zamanı footer'a (AppSurumBilgisi.DerlemeZamani — exe GetLastWriteTime)
- [x] **3. View yıkım + yeniden tasarım — İPTAL (kullanıcı kararı, Oturum 272):** Zaten yaptık; silme/yıkım yerine hedefli onarım uygulandı ve **MaliDonemYönetimView redesign Faz 6.88**'e devredildi.
  - [x] → **6.88 envanteri — 16 XAML ünitesi (32 dosya):** `MaliDonemYonetimView.xaml(+cs)` + 9 UserControl (`DonemSecimListesi`, `DonemOzetCard`, `TopluIslemlerPanel`, `DonemYedeklerPanel`, `DonemParametrePanel`, `DonemIslemKartlariPanel`, `DerinAnalizPanel`, `BilinmeyenPanel`, `SilinenDonemYedekleriPanel`) + 3 dialog (`YetimYedeklerDialog`, `YonetimAyarlarDialog`, `YedekSilOnayDialog`) + 3 ayar alt-paneli; `MaliDonemView/Details` + Shell dialogları (`DeleteGuard`, `RestoreVerify`) KAPSAM DIŞI
  - [x] → **6.88:** Kayıtlar: `Startup.cs:63` Yonetim kaydı silinir; `FirmalarListControl:51` + `GirisDashboard:120` caller'ları yeni hedefe (sessiz `catch` teşhisli yapılır); `DialogService:148` `YedekSilOnayDialog` bağı taşınır/korunur; `AddAppViewModelHostBuilderExtensions:40-41` VM kayıtları kalır (VM'ler korunur)
  - [x] → **6.88:** Yeni tasarım + Kural-12 akış-planı/bekleme-animasyonu + Kural-8 sınıf-bazlı onaylar
- [x] **4. Progress/bildirim disiplini (Kural 12) — birleştirildi (Oturum 272):** D1 ✅🧪 (Oturum 218) + D2 ✅🧪 (Oturum 220); **bildirim/InfoBar kısmı Faz 6.86 Chunk-2a ile tamamlandı**; kalan **progress (D3-View + yüzdeler) Faz 6.88**'e devredildi.
  - [ ] → **6.88:** Uzun işlemler (`TopluYedekle/Test/Bakim`, `YedekAl`, `BakimCalistir`, derin analiz, göç): `Is*Calisiyor` + determinate (`i/N`, `%`, mevcut iş) + bitince TEK özet; ara-adım toast'u kaldırılır
  - [ ] → **6.88:** Silme `MaliDonemDeletionViewModel` yoluna bağlanır (`ProgressPercentage` reuse); `TenantDatabaseUpdateViewModel` adım rozetine yüzde (Yedek ~30, Göç ~70, Doğrulama ~100)
  - [x] `NotificationService`: `tag/group` replace + tip-ikon/severity ayrımı → **6.86 Chunk-2a** (`IInAppMessageService` tag+grup ezme + `NotificationTypeToInfoBarSeverityConverter`)
- [x] **5. RAF panel temizliği — Faz 6.88'e birleştirildi (Oturum 272; RAF panel = `MaliDonem/Yonetim/.../YonetimAyarlar*` aynı view ağacı):** AppPlatform 5 (Splash gecikmesi, status süresi, Notification ikizi, klasör ikilisi) · Identity 3 (Pbkdf2 **önce provider'a bağlanır** — güvenlik; SessionTimeout + SonGiris rafta) · Database 7 (kapanış-yedeği, haftalık-kontrol, aralık, VACUUM flag, M4 busy/journal/sync) · License CheckIntervalDays 1
- [x] **Faz 6.71 KAPANDI (Oturum 272):** m.1 ✅🧪 · m.2 ✅🧪 · m.3 **İPTAL** · m.4 bildirim→**6.86 Chunk-2a**, progress→**6.88** · m.5→**6.88**.
- [ ] **Kapılar (her faz):** build 0/0 + test yeşil + smoke + docs ⬜

## Faz 6.72 — Çekirdek Stabilizasyon (saf Fluent + Faz B kilidi — Oturum 206, 📋 plan)
- [x] **Kararlar:** görsel dil = saf WinUI Fluent (AGENTS + ROADMAP karar logu); önce çekirdek, sonra Faz B (mühür = build 0/0 + test + smoke + canlı E2E) 📋
- [x] **Dalga 0 — Bug kuyruğu (öncelik: crash/veri-kaybı → yanlış-veri → UX):** ✅ Eklendi 🧪 Test edildi (Oturum 214 Kol A 7/9 + Oturum 215 Kol B 3/5 + Kol C 4/4 + Oturum 216 son 4 → **18/18 kapandı**; Oturum 217: build 0 hata + test 429/429 + 25sn smoke; canlı E2E kullanıcıda)
  - [ ] HATALAR AÇIK: product-ready geçiş kaydı (Oturum 138 süreci — kapanış kriteri `TenantUpdated → MainShell` canlı E2E)
  - [ ] Sepet: E2E saga etkileşimli akışı (Yeni Firma → Yeni Dönem pipeline → kart seçimi → DevamEt → MainShell) + silme guard dialog
  - [ ] Sepet: splash/kurulum görsel turu + hover-tooltip gerçek-fare teyidi + login tıklama + sayaç-grafik + 6.64/6.65 görselleri
  - [ ] 6.69/6.70 canlı sepeti: açılış hızı + checkbox'lı dönem silme + yedek-silme sonucu + panel sırası/ring'ler + dialog sekmeleri + yazılı onay
  - [ ] **Kullanıcı "her yerde bug" listesi (Oturum 207 — ALINDI, Oturum 208 teşhis TAMAMLANDI):** ✅
    - [x] Güncelleme (akışı) — 4 KIRIK (Apply kurmuyor, LastCheckTime ezilmesi, sahte Geri Al + yedeksiz geçiş, sessiz catch + versiyon kumarı)
    - [x] SistemVeritabanı açılış — 3 KIRIK (splash timeout'suz, routing fail-open, history onarımı yok) + Sistem conn-string çıplak
    - [x] Kapanış yedeği — HİÇ İNŞA EDİLMEMİŞ (okuyucu 0, exit handler yok) + haftalık yedek de yok
    - [x] Güncelleme altyapısı — yukarıdaki 4 + feed-null imza riski
    - [x] Tenant yedekleme altyapısı — 6 KIRIK (compensate yolu, create log yalanı, desen uyumsuzluğu, FIFO körlüğü, derin-analiz sahte Success)
    - [x] Tenant silme altyapısı — 5 KIRIK (`DeleteBackupDatabaseAsync` isim-yalanı + ölü, safety no-op, outer ölü dal, yetim + sahte Success, CompensateAll yok)
- [ ] **Dalga 1 — Kritik akış mührü (saf Fluent dokunuşu yok, davranış):** Splash→Kurulum→Login→FirmaShell→dönem→MainShell→güncelleme→yedek/geri-yükleme; her halka build+test+smoke+canlı ⬜
- [ ] **Dalga 2 — Denetim gerçekliği:** 16 RAF kartı panelden kaldırılır (Faz 6.71/5 ile aynı liste); Pbkdf2 provider'a bağlanır ⬜
- [ ] **Dalga 3 — Bildirim/progress disiplini:** Faz 6.71/4 ile aynı kapsam (çekirdek ekranlarına uygulanır) ⬜
- [ ] **Mühür hükmü (kullanıcıda):** canlı E2E onayı → Faz B (Cari pilotu, saf Fluent) açılır ⬜

## Faz 6.73 — Tenant geçiş doğrulaması: WAL checkpoint + dispose + DB log (Oturum 209 — ✅ KOD MEVCUT, Oturum 278 kapanış)
- [x] **Data ✅ (kod mevcut):** `ITenantSQLiteDatabaseManager.CheckpointAndReleaseAsync(db)` — `PRAGMA wal_checkpoint(TRUNCATE)` + `SqliteConnection.ClearAllPools()` + `(ok, message)` kanıtı (busy>0 → ok=false + çerçeve sayıları). `TenantSQLiteDatabaseManager.CheckpointAndReleaseAsync`.
- [x] **Business ✅ (kod mevcut):** `TenantSQLiteSelectionService.SwitchTenantAsync` + `DisconnectCurrentTenantAsync` eski-tenantı kapatır (`EskiTenantıKapatAsync` → `CheckpointAndReleaseAsync`); geçişi engellemez.
- [x] **Kullanıcı değişimi notu ✅:** `Logout()` artık **yok** (ölü yol temizlenmiş) — not geçersizleşti.
- [x] **Testler:** Business testleri mevcut (`BusinessDatabaseTests` Switch/Disconnect; `TenantDerinBaglantiTests`). **Ek (Oturum 278):** `TenantCheckpointTests` — gerçek dosya + WAL modu → checkpoint başarı + WAL boşaltma; dosya-yok/boş-ad hata. ⚠️ **Kırmızı (Oturum 279, 6.93 dışı):** `Checkpoint_WalModluDosya…` — `Mode=ReadWrite` ile **olmayan** dosyayı açıyor (Error 14); yardımcı kodu çıkınca da kızarıyor. Adım 2 öncesi düzeltilmeli.
- [ ] **Kapı:** build 0/0 ✅ (279) + test — `TenantCheckpointTests` düzeltmesi ⬜

## Faz 6.75 — Windows 11 Fluent geçişi (Mica-uyumlu, tüm ekranlar — Oturum 219, 📋 plan)
- [x] **Kararlar:** görsel dil = Windows 11 Fluent tüm proje (AGENTS genel kural güncellendi; "toplu re-skin yok" kalktı) + Mica-uyumluluk zorunlu + **Kural 14** (view-öncesi tasarım araştırması) 📋
- [x] **Araştırma (Kural 13/14):** MS Mica + System-backdrop bulgusu LOG-221'de ✅ (pencereler Mica'lı, 10 view opak `AppBackgroundBrush` kapatıyor; XAML+code-behind çift-set Kural-4 adayı)
- [ ] **Geçiş sırası:** `VIEW-BAGIMLILIK.md` topolojik sırayla, view view (çekirdek akış önce: Splash→Kurulum→Login→FirmaShell→Yönetim→Update→MainShell) ⬜
- [ ] **Her view'da kapı:** Kural 14 araştırması + Kural-8 sınıf onayı + build 0/0 + test + canlı (Mica + Light/Dark) + Kural 13 yardım sayfası ⬜
- [ ] **Kapsam dışı:** davranış değişikliği yok (yalnız tasarım;-style token'ları `DesignTokens`/`Styles` tek kaynağında) ⬜
- [x] **Token tek-anahtar süpürmesi (Oturum 225):** 19 anahtar→sistem, 61 tanım silindi, eski-grep 0 — 0 hata + 435/435 + smoke ✅🧪
- [x] **Login zinciri tema onarımı (Oturum 230):** 8 dosya Static→Theme + alias→sistem eşlemesi + `CardCornerRadius` kırık anahtar fix + CTA'lar AccentButton + Buttons/Cards sistem-renkleri + tema ayarlanabilir (Default=sistem) + dialog miras — 0 hata + 436/436 ✅🧪 (canlı tur kullanıcıda; downstream kendi turunda)
- [x] **Gerçek Mica tek kaynak (Oturum 238):** 2 pencerede `<MicaBackdrop Kind="Base" />` açık yazım + code-behind `MicaController/DesktopAcrylic` bloğu silindi (XAML tek kaynak, Kural 4) + Yönetim SplitView pane acrylic→Transparent — 0 hata + 446/446 ✅🧪 (canlı Light/Dark görseli kullanıcıda)
- [ ] **Sıradaki (Oturum 227 devri):** SK alt bileşenler (DatabaseInfoPanel/SystemTestsPanel/KurulumKayitPanel) → Login → FirmaShell; her view muhasebe yazılımı bağlamında araştırılır ⬜

## Faz 6.76 — SistemKurulum→SistemDbYonetim + KurulumSplash akış yeniden tasarımı (Oturum 227, onaylı plan)

**Bağlam:** Kural 14 sektör araştırması — muhasebe yazılımlarında ilk kurulum = firma+dönem sihirbazı; DB işlemleri kullanıcıya gösterilmez. Mevcut SistemKurulum hem ilk kurulum hem güncelleme hem teşhis — hepsini kapsıyor. Hedef: ilk kurulum Splash'te otomatik, güncelleme ayrı ekranda.

### Adım 1 — Business: 3 yollu karar mantığı ✅🧪 (Oturum 227)
- [x] `SplashTarget` enum'a `FirstSetup` + `MigrationRequired` ekle (mevcut: `SetupRequired` + `Login`) ✅ Eklendi 🧪 Test edildi
- [x] `SplashRoutingService.DecideRouteAsync` 3 yollu: DB yok → `FirstSetup`, migration var → `MigrationRequired`, hazır → `Login` ✅ Eklendi 🧪 Test edildi
- [x] Mevcut testler güncelle + yeni karar testleri (5→8 test) ✅ Eklendi 🧪 Test edildi

### Adım 2 — KurulumSplash (yeni View + VM) ✅🧪 (Oturum 229)
- [x] `KurulumSplashViewModel` (composition: ISistemDatabaseService — otomatik RunSetupAsync, progress, hata yönetimi) ✅ Eklendi 🧪 Test edildi
- [x] `KurulumSplashView.xaml` (Splash tarzı tek kart, Fluent: logo + "İlk Kurulum" + SplashStatusControl + determinate ProgressBar + % pill + hata paneli) ✅ Eklendi 🧪 Test edildi
- [x] `KurulumSplashView.xaml.cs` (OnPageLoaded → VM.RunSetupAsync → başarı: Login navigate; başarısızlık: ErrorPanel + Tekrar Dene) ✅ Eklendi 🧪 Test edildi
- [x] Startup.cs + DI kaydı (Transient + NavigationService.Register) ✅ Eklendi 🧪 Test edildi

### Adım 3 — Rename: SistemKurulum → SistemDbYonetim (15 dosya + 2 klasör) ✅🧪 (Oturum 228, geriye dönük kayıt)
- [x] View klasörü: `Views/SistemKurulum/` → `Views/SistemDbYonetim/` ✅ Eklendi 🧪 Test edildi
- [x] View: `SistemKurulumView.xaml(.cs)` → `SistemDbYonetimView.xaml(.cs)` ✅ Eklendi 🧪 Test edildi
- [x] ViewModel: `SistemKurulumViewModel.cs` → `SistemDbYonetimViewModel.cs` ✅ Eklendi 🧪 Test edildi
- [x] VM klasörü: `ViewModels/Sistem/SistemKurulum/` → `ViewModels/Sistem/SistemDbYonetim/` ✅ Eklendi 🧪 Test edildi
- [x] 4 alt VM namespace güncelle ✅ Eklendi 🧪 Test edildi
- [x] 5 Component namespace güncelle ✅ Eklendi 🧪 Test edildi
- [x] Harici referanslar güncelle (Startup, DI, SplashNavigator, LoginView, ExtendedSplash) ✅ Eklendi 🧪 Test edildi

### Adım 4 — SistemDbYonetim içerik daraltması ✅🧪 (Oturum 229)
- [x] `_firstSetupMode` bayrağı + `SetFirstSetupMode()` kaldırıldı (ilk kurulum artık KurulumSplash'te) ✅ Eklendi 🧪 Test edildi
- [x] Yeni `SetMigrationMode()` metodu + `UpdateStatusMessage` dali güncellendi ✅ Eklendi 🧪 Test edildi
- [x] Kalan: RepairDatabaseCommand + TestSystemClassesCommand + GoToLoginCommand + DatabaseInfoPanel + KurulumKayitPanel (migration/onarım bağlamında korundu) ✅ Eklendi

### Adım 5 — Navigasyon: SplashNavigator 3 yol ✅🧪 (Oturum 229)
- [x] `SplashNavigator.NavigateToNextAsync` → `FirstSetup` → KurulumSplash, `MigrationRequired` → SistemDbYonetimView, `Login` → LoginView ✅ Eklendi 🧪 Test edildi
- [x] SistemDbYonetimView: `SetFirstSetupMode()` → `SetMigrationMode()` güncellendi ✅ Eklendi 🧪 Test edildi

### Kapı
- [x] `dotnet build` 0/0 ✅ Eklendi 🧪 Test edildi
- [x] `dotnet test` tüm testler yeşil (436/436) ✅ Eklendi 🧪 Test edildi
- [x] **Sayfa yeniden yazımı (Oturum 231):** Kural 14 arş + komple rewrite (ilk-kurulum hero/adım göstergesi 0, durum hapı + BodyStrong bölümler) + titlebar Light kalıntısı temizlendi ✅ Eklendi 🧪 Test edildi
- [ ] Smoke: Splash→KurulumSplash→Login (ilk kurulum) + Splash→Login (normal) + Splash→SistemDbYonetim (migration) akışı ⬜
- [ ] Fluent geçişi: KurulumSplash + SistemDbYonetim Mica-uyumlu ⬜

## Faz 6.74 — Büyük-iş akış detayı + DB log + log altyapısı (Oturum 208 kararı, 📋 plan)
- [ ] **Akış detayı:** migration/yedek/sil/güncelle işlerine adım rozetli plan + determinate bar + tek özet (Kural 12); `DeleteGuardDialog`'a akış planı UI'ı ⬜
- [ ] **Hata→DB eşleşmesi:** tenant-hatası tenant DB (`AppLogs`), sistem-hatası Sistem.db — kural tablosu `AKIS-PLANI §7`-tipi dokümana ⬜
- [ ] **Log süpürmesi:** sessiz `catch{}`/`continue` envanteri + `SistemLogExceptionAsync` kapsama denetimi (bekçi adayı) + seviye disiplini (info/warning/error) ⬜
- [ ] **Kapı:** build 0/0 + test + smoke + docs ⬜

## Faz 6.77 — SistemDb Yönetim Operasyonları: yedekle / geri-yükle / güncelle (Oturum 233, 📋 plan)

**Bağlam (kullanıcı tespiti):** SistemDbYonetim sayfası dashboard gibi — durum/analiz/log var, yönetim operasyonu yok. Yedek yalnızca Login→Teşhis dialogunda (code-behind direkt `ISistemBackupManager`), restore'un sistem tarafında UI yolu 0, bekleyen göçü uygulayan UI yok.

**Araştırma bulguları (Oturum 233, Kural 13/14):**
- Sage 50: Yedekle (tarihli ad, üzerine-yazma uyarısı, otomatik-hatırlatıcı) → Geri yükle (dosya seç + Üzerine-yaz/Yeni-kopya + seçenek demeti + özet-onay + bitince aç) → yükseltmede önce Verify + yedek, hata→yedekten dön.
- QuickBooks: company-file update = hazırla (Verify, konum notu, tüm kullanıcılar çıkış) → otomatik yedek → Update Now → Done; hata→yedeği geri yükle.
- SQLite resmi: canlı kopya = Online Backup API (`Microsoft.Data.Sqlite BackupDatabase`) veya `VACUUM INTO` (küçük, tutarlı snapshot); restore = dosya-değişimi + `integrity_check` doğrulama; `VACUUM` öncesi yedek şart.
- MS Fluent: uzun iş = determinate bar + adım + TEK özet (Kural 12); yıkıcı iş = çift onay.

**Mevcut envanter:**
| Operasyon | Data | Contract (UI yolu) | UI |
|---|---|---|---|
| Durum/analiz/test | ✅ | ✅ `ISistemDatabaseService` | ✅ paneller + log |
| Kurulum/onarım (sil+kur) | ✅ | ✅ | ✅ DatabaseInfoPanel |
| Yedek al/listele/temizle | ✅ `SistemBackupManager` | ❌ (dialog code-behind direkt erişim — Kural 5 ihlali) | 🔶 yalnız QuickDialog |
| Geri yükle | ✅ manager'da | ❌ | ❌ 0 caller |
| Bekleyen göçü listele | ✅ `GetPendingMigrationsAsync` | ✅ servis | ❌ UI yok |
| Bekleyen göçü uygula | ❌ (yalnız initialize akışında otomatik) | ❌ | ❌ |

**Adımlar:**
- [x] **0. SON İŞ (Sorun 2 — Oturum 239):** yönlendirme 3-yollu doğrulandı (kısayol kodda yok); `SplashRouteDecision.KararOzeti` (exists/ready/pending/target) + `SplashNavigator` kararı `ShellArgs.Parameter` ile sayfaya taşır + `SetMigrationMode(ozet)` sayfa günlüğüne `[YÖNLENDİRME]` satırı düşer + 2 karar-izi testi — build 0 hata, test 462/462 ✅ Eklendi 🧪 Test edildi
- [x] **5. Yedek-panel revizyonu (Oturum 239, canlı tur):** KeepLast yüklemede uygulanır (HATALAR kaydı) + ListView→ItemsControl satır-butonlu (seçim/hover karmaşası bitti, `SeciliYedek`/`GeriYukleCommand` silindi) + tarih-birincil satır (`YedekTarihMetni`, dosya adı tooltip) + satır danger "Geri Yükle" + boş-durum kartı + yardım maddesi güncellendi + 4 test — build 0 hata, test 462/462 ✅ Eklendi 🧪 Test edildi (canlı tur kullanıcıda)
- [x] **6.77 kapısı (Oturum 239):** build 0 hata + test 462/462 + Adım 0 içinde; smoke/canlı tur kullanıcıda ✅🧪
- [x] **1. Business zinciri (Oturum 234):** `ISistemDatabaseService.ApplyPendingSistemMigrationsAsync` (contract + impl: bekleyen-kontrol → manuel yedek → `Initialize` göç → doğrulama; `ISistemBackupManager` ctor'a, Singleton-güvenli) + `SistemDatabaseGuncellemeTests` 4 test (yok/yedek-patlar/göç-patlar/başarı) — build 0 hata, test 440/440 ✅ Eklendi 🧪 Test edildi (not: `ISistemDatabaseOperationService` zaten vardı — QuickDialog onu bypass ediyor, VM taşıma Adım 2'de)
- [x] **2. View (Oturum 235):** `SistemYedekViewModel` (liste + al + FIFO `SistemKeepLast` + seçili-geri-yükle onaylı) + `SistemGuncellemeViewModel` (bekleyen liste + akış-planlı uygula: 1/3→3/3 + determinate) + orkestratör composition + `SistemYedekPanel`/`SistemGuncellemePanel` + sayfa "Yedekleme ve güncelleme" bölümü + QuickDialog VM'e bağlandı (code-behind direkt `ISistemBackupManager`/`LocalSettings` 0) + 6 VM testi — build 0 hata, test 446/446 ✅ Eklendi 🧪 Test edildi (canlı tur kullanıcıda)
- [x] **3. QuickDialog kararı (Oturum 236):** "Dialog kalksın" (kullanıcı) — `QuickSistemDbDiagDialog.xaml(+cs)` silindi (Kural 4), Login Teşhis doğrudan SistemDbYonetim sayfasına gider, ölü `SistemYedekViewModel` DI kaydı kaldırıldı — build 0 hata, test 446/446 ✅ Eklendi 🧪 Test edildi (canlı tur kullanıcıda)
- [x] **4. Yardım dialogu (Oturum 236, Kural 13):** AGENTS Kural 13'e desen+kapsam işlendi (? dialogu, yalnız fonksiyonlu view; splash muaf); ortak `Views/Components/YardimDialog` (Kural 8 kaydı) + sayfa başlığında ? butonu + 6 maddelik içerik — build 0 hata, test 446/446 ✅ Eklendi 🧪 Test edildi (canlı tur kullanıcıda)
- [ ] **Kapı (kalan):** 6.77 canlı tur ✅ (Oturum 240: panel tasarım-uyumu 5/5 + 5→3 budama canlı kanıtlı) → sonra 6.78 Adım 2 (tenant bağlama) ⬜

## Faz 6.78 — Ortak Restore Analizi: geri-yüklemeden önce çift-veritabanı hükmü (Oturum 237, 🔨 aktif)

**Bağlam (kullanıcı):** Sistem.db geri-yükleme basit dialogla olmamalı — kritik işlem. Yedekteki MaliDonem kayıtları güncelden eski olabilir (yedekten sonra açılan dönemler kaybolur), tenant dosyaları yedekte yoktur. İki veritabanı da analiz edilmeli. Altyapı tenant restore'unu da kapsamalı (ortak analiz → sonraki işler kolaylaşır).

**Araştırma (Kural 13/14):**
- QuickBooks: Verify → hasar varsa Rebuild (**yedek zorunlu**) → tekrar Verify → temizse devam; son çare restore. Restore öncesi mevcut dosyanın yedeği alınır.
- Mevcut: `RestoreVerdictEvaluator` (tenant, saf fn: Block/Warning/RequireCode/Allow + kod + audit + `RestoreVerifyDialog` tek kapı) — sistem tarafında hüküm YOK (direkt restore).

**Analiz ekseni (2 katman):**
1. **Yedek-dosya analizi:** `integrity_check` + tablo sayımı + `__EFMigrationsHistory` sürümü + boyut/tarih (yedek dosya salt-okunur açılır).
2. **Mevcut↔yedek farkı (sistem):** MaliDonem/firma/kullanıcı satır farkı (yedekten sonra açılanlar kaybolur listesi) + tenant DB dosya varlığı + kurulum/makine kimliği.

**Adımlar:**
- [x] **1. Ortak çekirdek (Oturum 239):** `RestoreAnaliz` DTO'ları (dosya+fark+girdi) + `RestoreAnalizDegerlendirici` (saf fn, `RestoreVerdictKind` reuse) + `IDatabaseBackupManager.AnalyzeBackupFileAsync` (salt-okunur: integrity+sayım+history) + `RestoreAnalizTests` 11 test (matris 8 + gerçek-dosya 3) — build 0 hata, test 462/462 ✅ Eklendi 🧪 Test edildi
- [x] **2. Tenant hattı (Oturum 242):** `RestoreVerdictEvaluator` ince adaptöre indirildi — firma/dönem/ad kıyası `RestoreAnalizGirdisi.KimlikUyusmazlik*`'e yazılır, hüküm `RestoreAnalizDegerlendirici.Degerlendir`'den alınır; ortak sıra tenant önceliğine çekildi (dosya → kimliksiz → kimlik → sürüm → kayıp → kurulum/makine); eski gömülü sürüm/kurulum dalları silindi (Kural 3/4). `RestoreAnalizTests` +3, `RestoreVerdictTests` 10 değişmeden yeşil — 0 hata + 471/471 ✅ Eklendi 🧪 Test edildi
- [x] **3. Sistem hattı (Oturum 243):** `ISistemSnapshotReader` (salt-okunur snapshot) + `ISistemRestoreAnalizService` (dosya analizi + mevcut↔yedek farkı → ortak çekirdek hüküm) + `SistemRestoreAnalizSonuc` DTO; `SistemYedekViewModel.GeriYukleAsync` tek kapı (analiz → hüküm dialogu → onay/kod → restore, Block'ta durur); `ISistemDbYonetimView` Kural-17 Katman 1 + `SistemRestoreVerifyDialog` + `IDialogService.ShowSistemRestoreVerifyAsync`; yardım maddesi güncellendi — 0 hata + 482/482 ✅ Eklendi 🧪 Test edildi
- [ ] **4. UI (Kural 8/11/12):** 🔨 fark listesi + hüküm rozeti + 6-haneli kod alanı + Block'ta pasif Primary + güvenli Vazgeç **dialog içinde** karşılandı (Oturum 243); busy bayrağı/tek özet VM'de var; kalan: canlı Light/Dark + Mica turu ve gerekirse determinate bar ⬜
- [ ] **Kapı:** build 0/0 + test (hüküm matris testleri) + smoke (sistem + tenant restore turu) + docs ⬜

---

## Faz 6.79 — Sistem.db yaşam-döngüsü güvenliği: açılış yedeği + kapanış WAL aktarımı (Oturum 241, ✅🧪)

**Bağlam (kullanıcı sorusu):** "Açılışta Sistem.db yedekliyor musun, uygulama kapanışında WAL'i DB'ye aktarıyor musun gibi kritik işlemleri yapıyor musun?"
**Tarama bulgusu (dürüst kayıt):** Açılışta oto-yedek çağrısı **0**; kapanışta WAL checkpoint **0** (kapanış yedeği vardı ama yalnız tenant + kapalı varsayılan); `EnsureWeeklyBackupAsync` kodda **yok** (yalnız arşiv kaydı); `WeeklyBackupDays` modeli var ama **0 tüketici**.

**Araştırma (Kural 13/14):**
- **Sage 50:** otomatik yedek "dosyayı kapatınca" tetiklenir (gün-ilk) + saklama aralığı ayarlı; tüm kullanıcılar çıkmalı.
- **QuickBooks:** "Save backup copy automatically when I close my company file" + kaç yedek saklanacağı (retention) + "Complete verification".
- **SQLite resmi:** WAL checkpoint TRUNCATE kapanışta önerilir (pasif kapanış WAL bırakabilir); açılış/kapanışta busy-timeout + integrity.

**Uygulanan (onaylı: "Hepsi"):**
- **Data:** `ISistemBackupManager.CheckpointWalAsync` (+impl, `ExecuteWalCheckpointAsync` TRUNCATE reuse) — best-effort, fırlatmaz.
- **Business:** `ISistemYasamDongusuService` + `SistemYasamDongusuService` (tek sorumluluk, Kural 1): `EnsureStartupSafetyAsync` (WAL checkpoint + `GetLastBackupDate` eşik-aşımı → `CreateBackupAsync(Automatic)` + FIFO `GetSistemKeep`) / `EnsureShutdownSafetyAsync` (checkpoint her zaman + ayarlıysa yedek). Eşik/keep/limit **modelden** (Kural 7: `WeeklyBackupDays`, `GetSistemKeep`).
- **DI:** `AddSingleton<ISistemYasamDongusuService, SistemYasamDongusuService>` (yalnız Singleton'lara yaslanır — döngü yok).
- **App kancaları:** açılış → `StartupApplicationExtensions.DbTest` (DB geçerliyse, splash adımı); kapanış → `WindowHelper.TryTakeExitBackupAsync` (Sistem kolu her zaman + ayarlıysa tenant kolu korunur).
- **Ayarlar:** kapanış toggle metinleri "açık dönem + sistem" (2 dosya) — ayar adı korundu (migration yok, Kural 6).
- **Test:** `SistemYasamDongusuTests` 6 test (açılış bayat/taze/checkpoint-fail/yedek-patlar + kapanış kapalı/açık) — **0 hata + 468/468**.

**Kapı:** `dotnet build --no-incremental` 0 hata (25 uyarı: bilinen eski aileler) + `dotnet test` 468/468 ✅🧪; canlı WAL/backup turu kullanıcıda.

---

## Faz 6.80 — Katmanlı sayfa yapısı (zemin → ana border → kartlar, tema-resimli zemin) (Oturum 241; ana border + tint kararı Oturum 248; **🔒 mühür Oturum 254** — mimari kilitli, yeni View/UserControl/Dialog bu yapı üzerine; ana kart + iç kart stilleri mühürlü)

**Kullanıcı kuralı (kesin, Oturum 248 güncel):** Her yeni/güncellenen view 3 katmana oturur: (1) **zemin** — kök `Border` (tüm sayfa; `Image` UniformToFill + tema tonlu tint `SolidBackgroundFillColorBaseBrush` **~%35**, ham resim gösterilmez, statik), (2) **ana border** — zeminin üzerinde tüm içeriği kapsayan çerçeveli panel (`CardBackgroundFillColorSecondaryBrush` + `CardStrokeColorDefaultBrush` 1px + `OverlayCornerRadius` + `Padding 24` + `ThemeShadow`, kenarlardan boşluklu), (3) **içerik kartları** — ana border içinde ayrı `Border` (`CardBackgroundFillColorDefaultBrush` + `ControlCornerRadius`/`OverlayCornerRadius` + `CardStrokeColorDefaultBrush` 1px + `ThemeShadow` + `Translation` + `Padding` 16/12/20, kart arası 12px, iç içe >2 seviye yasak). **Dialog zemini opak** (`SolidBackgroundFillColorBaseBrush`). Sadece Fluent ThemeResource; hardcode hex yasak. Yasak: kök Grid+Background; ana border'sız zemine kart.
- [x] Kaynak assetler: `Assets/Images/light.jpg` + `dark.jpg` (kullanıcı ekledi) ✅
- [x] Kural kaydı: `AGENTS.md` Kural 17 (Oturum 248 güncel: zemin→ana border→kartlar + tint ~%35 + opak dialog) + `docs/TASARIM-KURALLARI.md` ✅
- [x] **LoginView pilotu TAM** (Oturum 248): zemin(0.35)→ana border→kartlar; marka yatay + login sağda + `AppIcon.png` + `?` ana border sağ üst köşesinde; `?`/Teşhis VM command (`IDialogService.ShowYardimAsync`); `YardimDialog` opak; canlı kanıt (`ot248_login_v3_1440.png`/`ot248_login_light.png`/`ot248_help2.png`) ✅🧪
- [x] Tüm view'larda tint 0.35 (11 dosya) ✅
- [x] **KurulumSplash/SistemDbYonetim/FirmaShell/DenetimMasasi/ExtendedSplash** zemin + tint 0.35 ✅🧪
- [x] **FirmaShellView ana border paneli** (Oturum 249): tüm içerik tek `AnaBorder` paneline alındı (`Margin 24,48,24,24` + `CardBackgroundFillColorSecondaryBrush` + stroke 1px + `OverlayCornerRadius` + `Padding 24` + `ThemeShadow`, Loaded receiver); `CustomGlassPanel`/`SecimGorunumu` kaldırıldı + ölü `CustomGlassPanel` stili silindi; `?` `YardimCommand` (6 madde) ana border sağ üst köşesinde; Sage 50 + MS Fluent araştırması → `REFERANSLAR.md` 2 satır; 0 hata + 482/482 + canlı Dark kanıt (`ot249_firmashell.png`/`ot249_help.png`); Light teyit bekliyor ✅🧪

### Denetim Masası → Windows 11 Ayarlar yeniden tasarımı (Oturum 249 devam + devam 2, ✅🧪)
- [x] **Kararlar:** Mica zemin (Kural 17 Mica istisnası) + resmi **CommunityToolkit `SettingsCard`** + içerik/metin komple yeniden tasarım + **sahte ayar & "Kaydedildi" metni yasak** ✅
- [x] **Paket:** `DevWinUI 10.0.0` (net8; 10.4.1 net10 olduğu için çalışmıyordu) + `CommunityToolkit.WinUI.Controls.SettingsControls 8.2.251219` (namespace `using:CommunityToolkit.WinUI.Controls`) ✅
- [x] **Kabuk (`DenetimMasasiView`):** kök `Border` (transparent/Mica) + `NavigationView` Left (kenardan kenara; `AutoSuggestBox` pane'de) + büyük başlık + `?` + `Frame` (max 1000) ✅
- [x] **Giriş = Windows Ayarlar Home:** hero + kategori kartları ızgarası (`SettingsCard IsClickEnabled`) + Firmalarım + firma-yok ✅
- [x] **Paneller:** `AppPlatform`/`Identity`/`FirmaKayit`/`Donem` + **Veritabanı tek view** → `SettingsCard` + bölüm başlıkları + yeni metinler; sahte satırlar (Oturum süresi/Son giriş/Haftalık bütünlük) kaldırıldı ✅
- [x] **Veritabanı tek view (devam 2):** nav tek `Veritabanı`; yeni `VeritabaniAyarlarPaneli` iki `SettingsExpander` (`Sistem Veritabanı` + `Mali Dönem Veritabanları`) + `VeritabaniAyarSayfasi`; eski 4 dosya silindi (Kural 4); arama açıklamada da eşleşir; MS SettingsExpander + Sage/QB yedek araştırması → REFERANSLAR 5 satır ✅
- [x] **Bugfix (canlı testte):** `ToolBar.xaml` `TextFillColorTertiaryColor`→`TextFillColorTertiary` + `YonetimAyarlarDialog.xaml` `ControlFillColorSecondaryColor`→`ControlFillColorSecondary` (XamlParse → boş/siyah pencere) ✅
- [x] **Tema sahte→gerçek:** `ThemeSelectorService` `AppSettingsChangedEvent` aboneliği → canlı uygula + `AppBackgroundRequestedTheme`'e yaz (açılışta okunur) ✅
- [x] **Canlı test (Kural 18 — agent):** Dark (`ot249c_veritabani.png`/`ot249d_top.png`/`ot249e_bottom.png`) + Light canlı (`ot249f_after_light.png`) + açılışta Light (`ot249g_denetim_startup.png`) + Veritabanı Light (`ot249g_veritabani_light.png`); UIA dökümleri; **kullanıcı onayı ALINDI (2026-09-13)** ✅
- [x] **Borç temizliği:** `KaydedildiMetni` (6 VM + testler) + `Identity.SessionTimeoutMin`/`SonGirisBilgisiniGoster` (sahte) + `DenetimMasasiView` yardım maddeleri güncellendi ✅

### Denetim Masası Giriş sayfası yeniden tasarımı + canlı denetimler (Oturum 251, ✅🧪)
- [x] **Karşılaştırma (Kural 14/18):** Gerçek Windows 11 25H2 Ayarlar Giriş canlı ekran görüntüsü alındı (`ot251_winsettings.png`) + Denetim Masası ile fark listesi çıkarıldı; REFERANSLAR'a işlendi ✅
- [x] **Kabuk:** pane üstü **hesap kartı** (PersonPicture + ad + rol; `NavigationView.PaneHeader`) + **nav seçim vurgusu fix** (VM `SequenceEqual` tazeleme + `NavSeciminiAynala`; canlı UIA `Giriş=True`) + arama pane'de kaldı (`AutoSuggestBox`) ✅
- [x] **Giriş sayfası:** "Ayarlar" bölüm kartları kaldırıldı (ayar içinde ayar yok — kullanıcı kararı); üst satır **DevWinUI `RichButton`** (kart kabı yok, Win11 dili): MuhasibPro (ReadOnly) · Sistem Veritabanı (Bağlı • Güncel • boyut) · Güncelleme (son denetim); altında **Kullanıcıya Ait Firmalar** kartı — her firma `SettingsExpander`, içinde mali dönem listesi (+ Gelişmiş Yönetim) ✅
- [x] **Paket:** `App.xaml`'e `ms-appx:///DevWinUI/Themes/Generic.xaml` merge (RichButton stil kaynağı; paket Win2D bağımlılığı getirir) — REFERANSLAR kaydı ✅
- [x] **Rol:** seed yöneticisinin KFR satırı yok → `AuthenticationService` bootstrap kuralıyla `Rol=Yönetici` doldurur (canlı "Yönetici"); VM fallback "Kullanıcı" ✅
- [x] **Busy + dinamik denetim:** Sistem.db analizi `ProgressRing` + "Analiz ediliyor…"; güncelleme denetimi **her Denetim Masası girişinde** (pencere başına 1), UI kilitlenmez (`IsGuncellemeDenetleniyor` + ring), **20 sn tavan**, her durumda sonuç (kaynak yok / denetlenemedi / güncel / güncelleme var / zaman aşımı) ✅
- [x] **Veri bug fix:** `GetFirmalarWithUserId` dönem projeksiyonu (`0 dönem` → gerçek dönemler) — HATALAR kaydı ✅
- [x] **Doğrulama:** build 0 hata + test **481/481**; canlı kanıt `ot251l_home.png` (+ `ot251i`/`ot251j`/`ot251k`, UIA dökümleri); **kullanıcı onayı ALINDI (2026-09-13)** ✅

### Kural 17 hairline + tek görsel/tema perdesi (Oturum 251 devam, ✅🧪)
- [x] **Hairline token** (`MuhasibAnaBorderCizgiBrush`): Light `#E6DFE1E4` / Dark `#45C9CCD1` (yumuşak gri; beyaz ilk deneme "patlıyor" geri bildirimiyle revize) — pilot `ExtendedSplash` + `LoginView` `AnaBorder`; `Translation Z 32` + `ThemeShadow`; canlı Light/Dark + **kullanıcı onayı ("budur")** ✅ + **Oturum 258 mühür revizyonu:** Light → `#FFFFFFFF` (Dark `#45C9CCD1` aynen) ✅
- [x] **Zemin:** tek görsel `light.jpg` + tema perdesi `MuhasibZeminPerdeBrush` (Light `#59F3F3F3` ≈%35 = eski tint birebir; Dark `#D9202020` ≈%85) — `Login`/`ExtendedSplash` canlı; 9 view token swap (`MainShell`/`MaliDonemYonetim`/`SistemDbYonetim`/`Update`/`DatabaseSettings`/`TenantDatabaseUpdate`/`ShellView`/`KurulumSplash`/`FirmaShell`) — build 0/0 ✅
- [x] Kural metinleri güncellendi: `AGENTS.md` Kural 17 + `docs/TASARIM-KURALLARI.md` (perde brush + hairline token + Translation Z) ✅
- [x] Ders kaydı: PowerShell toplu replace XAML encoding bozar → `HATALAR.md` (edit tool kuralı) ✅
- [x] Kullanıcı kararı kaydı: **365 tasarımı İPTAL** + "gri yön yok, mevcut kartlarımızı kullanacağız" (LOG/ROADMAP) ✅
- [x] Ana border hairline kalan ana panellere (FirmaShell `AnaBorder`, KurulumSplash kartı) — **Oturum 253:** `MuhasibAnaBorderCizgiBrush` + FirmaShell'e eksik `Translation="0,0,32"`; canlı Dark (`ot253a_firmashell_dark.png`) + Light (`ot253i_firmashell_light.png`) ✅🧪
- [x] `dark.jpg` asset kaldırıldı (tek görsel+perde sonrası kullanılmıyor) + `light.jpg` 5000×3333 → 2560×1706 (RAM kazancı) — **Oturum 253** (ffmpeg q2, 435 KB→56 KB) ✅

- [x] **Ana border paneli** kalan view'lara tek tek — **Oturum 253:** `MaliDonemYonetim` (CustomElevatedCard→AnaBorder), `TenantDatabaseUpdate`, `Update` (+gömülü mod zemin-gizleme fix), `SistemDbYonetim` (`?` panel köşesine), `DatabaseSettings`, `MainShell` (sağ içerik paneli; sidebar zeminde + latent DataContext fix); `ShellView` pencere chrome'u → panel eklenmedi (kullanıcı kararı), `DenetimMasasi` Mica istisnası sabit; **dialog opaklığı:** 10 dialog kökü `SolidBackgroundFillColorBaseBrush`; **`?` panel köşesi:** 5 yeni VM `YardimCommand`; `InventFrostPanelStyle` silindi (Kural 4); canlı Dark/Light `ot253a-i_*` ✅🧪
- [x] Kalan 3 logo (`ExtendedSplash`/`KurulumSplash`/`SistemDbYonetim`) canlı doğrulama — **Oturum 253:** `SistemDbYonetim` logosu canlı Light (`ot253i_sistemdb_light.png`); `ExtendedSplash`/`KurulumSplash` logosu önceki pilot turlarında görünür (Oturum 248/251-252) ✅
- [x] FirmaShell Light/Mica canlı teyidi — **Oturum 253:** `ot253i_firmashell_light.png` (açılıştan Light) + runtime switch `ot253h_firmashell_light.png`; Static→Theme süpürmesi sonrası beyaz-beyaz giderildi ✅🧪
- [x] Sıradaki view'lar topolojik sıra: `MaliDonemYonetimView` → `TenantDatabaseUpdateView` → `UpdateView` → `MainShellView` → `ShellView` → `DatabaseSettingsView` — **Oturum 253 tamamlandı** (TenantDatabaseUpdate canlı turu: göç bekleyen dönem yok → erişilemedi, dürüst kayıt; DatabaseSettingsView canlı rota yok → ölü görünüm notu) ✅🧪

> **Oturum 253 kalan notu:** `DatabaseSettingsView` + `DatabaseSettingsViewModel` canlı rotaya sahip değil (yalnız `Startup.Register` + `AyarYayinTests` bağı) — Kural 4 temizlik kararı ayrı oturumda (test `YedekSaklamaAyarlarViewModel`'e taşınabilir). `Cards.xaml` `CustomCompactCard` 0 caller (eski "rezerve" kaydı Fluent yönünde geçersiz) — temizlik adayı.

---

## Faz 6.81 — Sahte transfer alarmı + dialog rötüşü + tek örnek (Oturum 252, ✅🧪)
- [x] **Kök neden (kanıtlı):** `LocalSettings.json` silinince (Oturum 251 pilot temizlik) kurulum kimliği yeniden üretildi → aynı makinedeki dönem damgaları "farklı kurulum" göründü → sahte "Taşınmış Veri" alarmı; `MakineId` iki tarafta aynıydı ✅
- [x] **Business/Data ayrımı:** `CheckTransferAsync` — makine farklı → gerçek transfer (dialog + `TransferDetectedEvent`); makine aynı → kimlik kaybı sessizce onarılır (tek kimlik → `UpdateKurulumIdAsync` ile damgadan geri alma; karışık → `ReAlignTenantKurulumIdsAsync` ile güncel kimliğe eşitleme); `TransferCheckResult` + `AlignedCount`/`AdoptedKurulumId` ✅ Eklendi 🧪 Test edildi
- [x] **Dialog:** yalnız `#if DEBUG` (release'de bilgi geri-yükleme hükmünde); tek "Kapat" butonu; profesyonel içerik + "yalnız geliştirme derlemesinde" notu ✅ Eklendi 🧪 Test edildi
- [x] **Tema:** `DialogHelper.ApplyAppTheme` — dialog `RequestedTheme`'i uygulama temasından yazılır (açık temada dark dialog fix; XAML hardcode yok); `DialogService.CreateDialog` de aynı kapıdan ✅ Eklendi 🧪 Test edildi
- [x] **DEV işareti:** `AppSurumBilgisi.Metin` DEBUG'da `DEV • v…` (Login/FirmaShell/SistemDbYonetim footer'ları) ✅ Eklendi 🧪 Test edildi
- [x] **Çift açılma:** yeni `SingleInstanceGuard` (named Mutex + 3 sn tolerans + "zaten açık" `#32770` uyarısı + pencereyi öne getirme) + `App` ctor kapısı ✅ Eklendi 🧪 Test edildi
- [x] **Test:** `SplashRoutingTests` +2 — build 0 hata + **483/483** ✅🧪
- [x] **Canlı kanıt (Kural 18):** `ot252_login_light.png` (DEV + onarım sonrası dialog yok), DB `KurulumId=9e67…`, `ot252_transfer_dialog.png` (açık tema + tek buton), `ot252_ikinci_ornek_uyari.png`, `ot252_final_clean.png` ✅
- [x] **Kullanıcı onayı:** canlı kanıtlar sunuldu — **ALINDI** (Oturum 270, commit onayı ile) ✅

## Faz 6.82 — Dev-mode (yalnız DEBUG): kimlik/transfer + teşhis araçları (Oturum 252 kullanıcı kararı — ✅ TAMAMLANDI + kullanıcı onaylı, Oturum 269/270)

- [x] `IDevModeProvider` (Business `Contracts/UIServices`; DEBUG'da `true`, Release'de `false`) + `DevModeProvider` — sürüm/derleme kapısı, kullanıcı ayarı DEĞİL. **Sapma (Kural 4):** ayrı `NullProvider` sınıfı yerine tek sınıf + derleme sabiti. DI: `AddBusinessServices` Singleton. ✅ Eklendi 🧪 Test edildi
- [x] Denetim Masası'nda yalnız DEBUG'da görünen **Geliştirici Araçları** bölümü (`AyarBolumu.GelistiriciAraclari` + `AyarlarNavigationMenu` 8. satır + `MenuGorunurMu` DEBUG kapısı) + `?` yardım dialogu (Kural 13) + her aksiyonda onay; işlemler `ISistemLogService`'e `DEV` kaynak damgasıyla yazılır (`DevAraclariService`). ✅ Eklendi 🧪 Test edildi
- [x] Yetenekler: kurulum kimliği **onarım** (`ReAlignTenantKurulumIdsAsync`, yalnız makine-aynı) / **sıfırlama + damga** (yeni kimlik + makine-aynı dönemleri yeniden damgalama), **transfer taramasını elle tetikleme** (`ISplashRoutingService.CheckTransferAsync`), **dönem şema damgaları** görünümü (`ITenantVersionReader.GetStampsAsync`), **ayrıntılı log seviyesi** (`ILogSeviyesiYoneticisi` + `FileLoggerProvider.IsEnabled` eşik), **log/veri klasörü açma** (`IYolAciciService` → App `YolAciciService`). ✅ Eklendi 🧪 Test edildi
- [x] Kapsam dışı korundu (kullanıcı kararı): restore hüküm Block/RequireCode bypass + zorla göç + silme/saklama kilidi bypass — uygulanmadı. ✅
- [x] **Ek (Oturum 273) — Güncelleme kaynağı + Modül Entegrasyon Testleri:** (a) Geliştirici Araçları'na **"Güncelleme Kaynağı"** kartı (repo/feed adresi düzenleme + varsayılana sıfırla + "Kaynağı Doğrula" öz-testi); (b) **"Modül Entegrasyon Testleri"** (donanım POST): her modülün DI'da kayıtlı/çözülebilir olduğunu + kritik akışının salt-okunur çalıştığını doğrular (`IModulTestCalistirici`/`ModulTestCalistirici`, 34 DI çözümü + 5 fonksiyonel probe); (c) **"Tanılama"** (mevcut `SistemDiagnosticsViewModel` 7 test + güncelleme öz-testi + kimlik özeti). Build 0 hata; test **509/509**; **canlı: "Modül testleri 41/41 geçti" + FeedUrl gömülü varsayılan** (`ot293_modul_test`/`ot293_tanilama`). ✅🧪
- [x] Kapı: Kural 14 araştırması (MS "Settings for developers" + Dialog controls → `REFERANSLAR` ✅) + Kural 8 sınıf onayı (kullanıcı "tamamını onayla") + build 0 hata + `dotnet test` **498/498** (6 yeni test) + Kural 18 canlı kanıt (`Temp/opencode/ot269_s1..s7`, DEV SistemLog kaydı) + kullanıcı onayı. ✅🧪
- **Not (mimari bekçi):** `DevAraclariService` ilk denemede `Services/Installation`'a konuldu → `ArchitectureTests.Kurulum_Ile_SistemDb_Yonetimi_Ayrik` kırmızı; tenant servisine yaslandığı için `Services|Contracts/SistemServices/DevServices` altına taşındı. Ders `HATALAR.md`'de.

## Faz 6.83 — TenantDatabaseUpdateView canlı doğrulama turu — ❌ GEÇERSİZ (kapatıldı, Oturum 278)
> **KAPANIŞ (Oturum 278):** Bu fazın tüm maddeleri **`TenantDatabaseUpdateView`** canlı turu içindi; sayfa **Faz 6.91-E'de silindi** (göç erişim anında onay + inline). Dolayısıyla faz **geçersiz — kapatıldı**; canlı kanıt ihtiyacı 6.91-E'nin kendi canlı turuna taşındı.
> **Neden ayrı faz (tarihsel):** Oturum 253'te Katman-2 dönüşümü yapıldı (AnaBorder + `?` + gömülü değil, tenant `Güncelle` akışı) ancak **göç bekleyen dönem olmadığı için ekran canlı açılamadı**; Kural 18 canlı kanıtı eksik — kullanıcı "ayrı faz aç, unutmayalım" dedi.
- [x] ~~(a) Kontrollü test ortamı~~ — geçersiz (sayfa yok)
- [x] ~~(b) Canlı akış~~ — geçersiz (sayfa yok); yeni akış 6.91-E
- [x] ~~(c) Kanıt + kayıt~~ — geçersiz
- [x] ~~**Kapı**~~ — geçersiz

## Faz 6.84 — FirmaShellView seçim deneyimi yeniden tasarımı (Oturum 255 kullanıcı talebi — ✅ TAMAMLANDI + kullanıcı onaylı "gayet başarılı", Oturum 268; commit atıldı)

> **Kapanış (Oturum 268):** Seçili dönem/firma satırı ana kart rengini alır (içeride/çukur), seçimsiz saydam, seçim sol accent hub; hover tema-farkında token `MuhasibHoverOverlayBrush`; ana border 1→1.5 (mühür revizyonu); animasyon sınıfları tümüyle silindi. x64 0 hata + 492/492 + canlı Dark/Light kanıt (`ot275_*`/`ot276_*`). **Kullanıcı onayı alındı, faz kapandı.**

**Araştırma (Kural 14/19 → REFERANSLAR):** Sage 50 Company Selection (tam liste + ad/versiyon/VKN/mali yıl/veri yolu kolonları + satır aksiyonları + Add Company) · Tally Gateway (F3 → şirket listesi; yüklü şirket bilgisi ekranda görünür kalır) · Oracle Retail Select Company (liste üstü arama; filtre tüm sonuçlarda) · Deltek Select a Company (arama + sonuç sayısı) · MS Radio Buttons ("seçenekler bağlama göre değişiyorsa liste kontrolü kullan") · MS AutoSuggestBox (yazarken filtre + zengin ItemTemplate + SuggestionChosen) · MS ComboBox (çok satırlı/zengin içerik ComboBox'ta değil listede).

**Kesinleşen kararlar (Oturum 256 — onaylı; FirmaShellView modüler dashboard çekirdeği):**
1. **Header/dashboard:** en solda logo; en sağda **simgeli `Ayarlar`** butonu + `UserInfoControl`; orta alan dashboard başlığına ayrılır. "Denetim Masası" görünür metinleri **"Ayarlar"** olur (kod sınıf adları aynı): `FirmaShellView` buton+tooltip, pencere başlığı, `FirmaShellViewModel`/`DenetimMasasiView`/`SistemDbYonetimView` yardım metinleri.
2. **Firma seçimi:** **seçici buton + Flyout** (içinde arama; **sonuç yoksa "Firma bulunamadı"**); seçim → flyout kapanır; altında **seçili firma kartı** (detay + `N dönem`; aksiyonlar ayrı konforlu satır: `Düzenle` → `FirmaDetailsViewModel` düzenleme yolu, `Mali Dönem İşlemleri` → mevcut yönetim penceresi).
3. **Mali dönemler:** önce **liste**; **seçilen satır açılıp kart gibi durur** (accordion); seçim işareti accent; **`RadioButton` stili tamamen kalkar** (mühür revizyonu).
4. **Busy + tenant güvenliği:** her dönem kendi `IsDbAnalyzing` ring'i (20sn tavan, sonuç zorunlu); tenant geçişi tek kapı (`UpdateCoordinator`) + `_dbGate` serileştirme; hata → seçim geri + hata bandı; kartta Açık/Kapalı/Güncelleme gerekli/DB yok bilgisi.
5. **Güncelleme bildirimi (araştırma: MS InfoBar):** bloklamayan **InfoBar** — tek dönemde doğrudan `TenantDatabaseUpdateView`; çok dönemde "N dönemde güncelleme hazır → İncele" listesi (satır başı Güncelle → ilgili sayfa).
6. **CTA:** alt bar solda `Firma • Yıl • durum` özeti, sağda `Çalışma Alanına Geç` (uygunsa aktif; değilse disabled + neden tooltip).
7. **Renk/token sözleşmesi:** ink siyah "Aktif Seçim" hapı kalkar → accent seçim göstergesi; olive/petrol alias yığını view'dan çıkar; ikonlar TextFill ikincil; durumlar `SystemFillColorSuccess/Caution/Critical/Attention`.
8. **UX kabul kriteri:** klavye (ok/Enter/Esc), flyout kapanınca odak dönüşü, boş-durumlar ring ile çakışmaz, UIA adları, tooltip kapsamı; **dashboard çekirdeği modüler bloklar** (Firma seçici / Dönem listesi / Güncelleme InfoBar'ı / CTA özeti).
9. **Davranış korunur (regresyon yasak):** `Selection.SelectFirma/SelectMaliDonem`, `DevamEt` guard'ları, koordinatör, son-seçim kaydı, Kural 13 yardım.

**Kapsam (sınıf sınıf — Kural 8):** `Cards.xaml` (seçimli liste stili; `MuhasibCardRadioButtonStyle` 0 caller kalınca silinir) → `MaliDonemlerListControl` → `FirmalarListControl` → `FirmaShellView.xaml(+.cs)` → VM'ler (`FirmaShellViewModel`, `FirmaListViewModel`, `MaliDonemListViewModel`) → "Ayarlar" metin süpürmesi → yardım.
**Kapı:** Kural 8 sınıf onayları + Kural 14 araştırma kaydı (✅ defterde) + build 0/0 + test + **Kural 18 canlı (Light/Dark, busy + tenant geçişi)** + yardım güncel + kullanıcı onayı.

**Oturum 257 durum notu (kirli ağaç, commit yok):** son oturumdaki model uygulamaya loglanmadan başlamış — `Cards.xaml` seçimli liste stili + `FirmalarListControl` seçici Flyout/"Firma bulunamadı"/seçili kart + `MaliDonemlerListControl` InfoBar + `FirmaShellViewModel` `SecimOzeti`/`DevamEtNedenTooltip` + `MaliDonemListViewModel` `Guncelleme*` bildirimi + yardım metinleri ("Ayarlar" dahil); build 0/0 + test 483/483 doğrulandı (yeni test yok). Eksik: accordion detay kartı tamamlama, "Ayarlar" süpürmesi, klavye/odak/UIA kriterleri, canlı tur + onay.

**Oturum 260 sayımı (kod yok, rapor):** 12 dosya satır-sayımıyla doğrulandı — accordion detay **kodda tamam** (DB yok uyarısı + Veritabanı/Boyut + DB Durumu + Son Yedek + Yedek; yalnız canlı doğrulama bekliyor); "Ayarlar" süpürmesi **tamam** (görünür "Denetim Masası" 0, pencere başlığı "Ayarlar"); radio referansı 0, Petrol/Olive/Ink 0, ölü handler 0. Kalan kod (6 küçük): (1) flyout açılışında arama `Focus()` yok, (2) ok-tuşu gezinmesi seçimi anında değiştiriyor (canlıda değerlendir), (3) firma-seçili + 0-dönem boş-durumu yok, (4) bayat "soldan" metni + kapalı-dönem yorumu, (5) `FirmaRepository` count `ToLower` tutarsızlığı (pre-existing), (6) sessiz `catch {}` ×3. Kalan doğrulama: Kural 18 canlı tur + onay, Kural 8 sınıf onayları, 6.84 testi, REFERANSLAR `Durum` ✅.

**Oturum 261 tamamlama (kod ✅, doğrulama Windows'ta):** (1) flyout odak ✅, (2) ok-tuşu canlı-takip KORUNDU (ComboBox + eski-radio paritesi, kod yok) ✅, (3) 0-dönem kartı + `BosDonemBaslik` VM'den (firma adıyla) + `HyperlinkButton` → mevcut saga ✅, (4) bayat metinler ✅, (5) count tek-kaynak (`Firma` + `MaliDonem` reposu) ✅, (6) sessiz catch 0 + ölü using silindi ✅; **(7) "Son çalışılan" rozeti** (Sage/QB araştırması → REFERANSLAR; `SonCalisilanMi` + `SonKayitliDonemId` + satır rozeti + yardım) ✅; `FirmaShellSecimDeneyimiTests` 10 fact (beklenti 493/493) ✅. Kararlar (tool-onaylı): ek yönlendirme YOK, varsayılan bayrağı YOK, MRU sıralama YOK (yıl sırası korunur). Kalan: Windows'ta build 0/0 + test + smoke + Kural 18 canlı tur + onay + REFERANSLAR ✅ + commit.

**Oturum 262 ek kapsam (kod ✅):** başlıkta tema kısayolu — Ayarlar'ın solunda 3 segmentli hap (Sistem/Açık/Koyu), model üzerinden kayıt (Görünüm ile aynı kaynak), `ThemeChanged` çift-yön senkronu, hata-bildirimi, fail-soft. Yardım maddesi kullanıcı kararıyla YOK (Kural 13 istisnası). Birim test yok (UI kablolemesi, katman engeli — LOG 262'de gerekçeli). Canlı tur kapsamına eklendi (segment tıklama + Görünüm tutarlılığı).

**Oturum 263 PİLOT:** dönem listesi stok stile alındı (`ItemContainerStyle` kaldırıldı, tek satır); stil firma flyout'unda yaşıyor, geri dönüş hazır. Kart ↔ stok kararı canlı görüntüye göre verilecek.

**Oturum 264 PİLOT devamı (canlı geri bildirim):** satır içeriği büyütüldü (yıl 17, rozet 28, hap/metin +1pt) + satır aralığı 8px (stok template koruyan minimal stil). Karar yine canlı görüntüye göre.

**Oturum 265 PİLOT iptal (canlı hüküm: "olmadı"):** kart stiline + orijinal ölçülere tam dönüş doğrulandı. Stok denemesi kapatıldı, 6.84 kart tasarımı geçerli.

**Oturum 266 tema v2 (kod ✅):** 3 segment → kayar anahtar (sol güneş=Açık, sağ yarım-daire=Koyu; E706/E7A1 MS-teyitli). Sistem ikiliye sığmaz → efektif tema gösterilir, dönüş Görünüm'de. Canlı tur kapsamına eklendi.

**Oturum 267 seçim stili (kod ✅, onay bekliyor):** kullanıcı talebi — mavi tam-çerçeve kaldırıldı, sol dikey hub (3px, mevcut ölçü) korundu, seçilince **hub'dan çıkıp kartı dolanıp hub'da duran kuyruk** eklendi (`MuhasibDonemSelectionListItemStyle` + yeni `OrbitRingControl`, storyboard; Kural 14/19 araştırmalı). Canlı teşhiste 3 bug çözüldü: (a) container şablonunda `{Binding Selected}` çözülmüyor → `TemplatedParent Content.Selected` (hub 6.84'ten beri canlıda yoktu — HATALAR), (b) kuyruk VSM ile sürülünce seçim değişiminde **tüm kartlarda animasyon** → model.Selected bağlaması (kullanıcı raporuyla kapatıldı), (c) WinUI dash desen toplamı > yol uzunluğu = çizim yok → desen boyut-bazlı (`gap=perimeter−101`) — HATALAR. x64 build 0 hata + test 492/492 + UIA/piksel kanıtlı canlı (hub görünür, kuyruk hub'da dinlenip saat yönünde dönüyor, seçimsiz kartta 0). Kalan: kullanıcı onayı + Light tema turu + REFERANSLAR durum ✅ + commit.

**Oturum 268 seçim animasyonu (kod ✅, ONAYLI "gayet başarılı"):** orbit **kaldırıldı** (kuyruk hub'ı geçiyordu + `StrokeDashOffset` bağımlı → takılma); `SelectionWaveControl` nabzı denendi, **kullanıcı reddetti** → **animasyon sınıfları tümüyle silindi** (Kural 4). NİHAİ: **seçili dönem/firma satırı ana kart rengini alır** (`CardBackgroundFillColorSecondaryBrush` → panel üstüne binince içeride/çukur), **seçimsiz saydam**, seçim yalnız sol accent hub; **seçili firma kartı** (sol panel) da aynı; **hover tema-farkında token** `MuhasibHoverOverlayBrush` (Light `#14000000` / Dark `#1AFFFFFF`) ile iki temada görünür (eski `ControlFillColorSecondaryBrush` Light'ta etkisizdi — HATALAR). Ayrıca **ana border `1→1.5`** (10 Katman-2 view; mühür revizyonu — AGENTS Kural 17 + TASARIM-KURALLARI). Kural 14/19: MS Motion → REFERANSLAR güncel. x64 **0 hata + 492/492**; canlı Dark/Light kanıt (`ot275_*`/`ot276_*`: Light panel 250/seçili 248/hover 251→231; Dark panel 76/seçili 82/hover 77→95). Commit yapıldı.

## Faz 6.85 — Kullanıcı Yönetimi + RBAC (kullanıcı → modül/alan erişimi) — **ÖNCELİKLİ FAZ** (Oturum 284 plan 📋; Oturum 256 ilk karar)

> **Plan:** `docs/KULLANICI-YONETIMI-PLAN.md` (kod doğrulamalı mevcut durum + hedef mimari + K1-K6 + riskler).
> **Kullanıcı talebi (Oturum 284):** "Eksik iş kalmasın, temiz bir yapıyla devam"; kullanıcı bazlı yapı çekirdeğin sonuna bırakıldı → **kim hangi modülü/alanı kullanır, nereye girer, yetkisiz alan nasıl engellenir** eksik. **Sahiplik:** RBAC `Data`+`Business`, UI `Views`/`ViewModels`; migration **Kural 8**.

**Araştırma (Kural 14/19 → REFERANSLAR):** QuickBooks Desktop "Users & Roles" (yalnız admin kullanıcı oluşturur/yönetir; kullanıcı + rol + aktive; self hesap ayrı) · MYOB "Manage Users/Roles" (rol bazlı yetki; kullanıcı yönetimi rol yönetiminden ayrı) · MS InfoBar + mevcut DenetimMasasi modal pencere deseni · (RBAC seed: `REFERANSLAR` 281 — EF seeding + sektör admin deseni).

### K1 — Seed/atama temeli (RBAC çalışır hâle gelir) ✅🧪 (Oturum 285) — **Kural 8 (migration) ✅ onaylı**
- [x] `RolPermission` statik matrisi (Yönetici=tüm izinler 76, Kullanıcı=temel görüntüleme 16) → migration `RbacRolPermissionSeed` (`HasData` + **idempotent `INSERT OR IGNORE`**)
- [x] Firma oluşturmada oluşturana **Yönetici KFR** (idempotent) — `FirmaKayitService`
- [x] Açılışta **idempotent backfill** (KFR'siz firmaya seed yönetici Yönetici KFR) — `SistemRbacBackfill` + `SistemMigrationManager`
- [x] `PermissionService`: **Yönetici bypass'ı** ✅ (RolTip=Yönetici → tüm izinler; RolPermission sorgulanmaz) · `ClearCache()` **mekanizma hazır**; K1'de giriş sonrası rol/izin yazımı yok (KFR firma oluşturmada, backfill açılışta) → çağrı yerleri **K2/K3**'te (negatif sonuç cache'lenmez)
- [x] `Permission.cs:5` yorum düzeltmesi (Global.db → Sistem.db)
- [x] Testler: gerçek seed ile `PermissionService` integration + KFR yazımı + backfill (`RbacK1Tests`, 6 test)


### K2 — Kullanıcı Yönetimi modal penceresi ⬜
- [ ] `KullaniciYonetimiViewModel` + `KullaniciYonetimiView` (ayrı `Views/KullaniciYonetimi/`; `DenetimMasasi` modal/boyut deseni; `CreateNewViewAsync`)
- [ ] Liste + yeni kullanıcı (`Adi/Soyadi` dahil; `Register` duplicate kontrolü) + düzenle + aktif/pasif + şifre belirle + sil (guard'lı). Kapı: yönetici (`AyarYetkiDenetimi.KullaniciYoneticiMi` + `Permission`)
- [ ] `IKullaniciService`'e eksik create/rol metotları (Kural 5 zinciri: VM → Contracts → Services → Data)

### K3 — İzin matrisi + rol atama (iki rol) ⬜
- [ ] **İzin matrisi UI** (71 izin, kategori başlıklı): `Yönetici` sabit (tüm izinler), **`Kullanıcı` düzenlenebilir** → modül/aksiyon erişimi buradan
- [ ] Kullanıcıya **firma-bazlı rol atama** (KFR yazımı; Yönetici/Kullanıcı)
- [ ] **Özel rol oluşturma YOK** (kullanıcı kararı Oturum 284)

### K4 — Modül/alan erişim kapısı ⬜
- [ ] `MainShell` modül menüsü: yetkisiz modül **gizli/pasif + gerekçe** (`MainMenuViewModel` + `Permission`)
- [ ] Denetim Masası bölümleri: yetkisiz bölüm gizli (`DenetimMasasiViewModel.MenuGorunurMu`)
- [ ] Sayfa/buton guard'ları (yıkıcı işlemler: sil/güncelle/geri yükle)
- [ ] `IModuleLicenseService` modül lisans kapısı nav'a; `ModuleLicenseViewModel` hardcoded `firmaId` düzeltmesi

### K5 — Hesabım + UserInfoControl menüsü ⬜
- [ ] "Hesabım": profil (ad/iletişim/avatar) + şifre değiştir (mevcut şifre doğrulamalı)
- [ ] `UserInfoControl` menüsü: Hesabım / Yönetim[admin] / Oturumu Kapat; **rol rozeti gerçek** (hardcoded "Admin" kaldırılır)

### K6 — Doğrulama + doküman ⬜
- [ ] build 0/0 + test (seed, KFR, PermissionService, Kullanıcı Yönetimi)
- [ ] Kural 18 canlı: yönetici + kısıtlı kullanıcı ile **alan erişim matrisi**; kanıt + onay
- [ ] Kural 13 yardım + dokümanlar

**Kapı:** Kural 8 (migration/K1) + Kural 14 araştırma + Kural 4 (ölü kod yok) + build 0/0 + test + Kural 18 canlı + onay.

---

## Faz 6.86 (GENİŞLETİLMİŞ) — Durum çubuğu + StatusMessage + Notification: komple redesign + refactor (Oturum 270 kullanıcı kararı — 🔨 aktif; sınıf onayı + yeni context)

> **Kullanıcı kararı (Oturum 270):** (a) `StatusBarService` incelenip modernize edilecek; (b) status bar **kalır ama Fluent'e modernize** ("yakışıklı", sade) — **mevcut tasarım yokmuş gibi sıfırdan**; (c) **status bar + `StatusMessageService` + `NotificationService`** hepsi komple redesign + refactor; (d) `StatusMessageService` (uygulama-içi alt bar) ve `NotificationService` (OS toast) **ayrı sorumluluklarda ayrı kalır** — ikisi arasında bağımlılık kurulmaz. Bu faz; eski 6.86 + **6.71/m.4** (progress+bildirim) + **6.72 Dalga 3**'ü tek hatta toplar.

**Yön (onaylı — Hibrit):** status bar = kalıcı/ambiyans + sessiz arka plan ilerlemesi · **InfoBar** = olay/state mesajı (güncelleme var / göç gerekli / bağlantı koptu) · **In-App message (InfoBar tabanlı)** = olay/state + arka plan iş sonucu (**OS toast terk edildi** — Oturum 270 kullanıcı kararı) · **ContentDialog** = bloklayan onay. Kaynak: MS "Structure a modern WinUI 3 app" + MS "WPF→WinUI3" (*StatusBar → InfoBar + dedicated footer*) + InfoBar/Progress/VS Code status bar kılavuzları (Kural 14/19; `REFERANSLAR` Oturum 270).

**Envanter / borç (teşhis — Oturum 270):**
- `IStatusBarService`/`StatusBarService` (139 satır): **ölü üyeler** `MaliDonem` (0 caller), `IsTenantDatabaseConnection`/`SetTenantDatabaseStatus` (0 caller), `Initialize` (0 caller), `KullaniciAdiSoyadi` (yalnız yazılır). **Bug:** `IsSistemDatabaseConnection` hiç `true` yapılmıyor (`Startup.cs:89` yalnız mesaj yazıyor) → DB göstergesi daima "bağlı değil".
- `IStatusMessageService`/`StatusMessageService` (**440 satır, Kural-1 ihlali**): mesaj+tip+ikon+**hardcode `StatusColorHex` (5 hex)**+progress+auto-hide+3 async+dispatch+INPC. **Bug:** `StatusColorHex` `Theme == ElementTheme.Light` karşılaştırması `Default` (sistemi takip) durumunu yok sayıyor → renk sapması. **Ölü:** `LastUpdateTime`/`LastUpdateTimeText`, `Initialize`. **Yetim ayar:** `StatusAutoHideMs` bu servise bağlı değil.
- `ShellStatusBar.xaml(.cs)` (160/98): code-behind'de `Colors.LimeGreen/OrangeRed` hardcode + ölü wrapper'lar (`StatusGlyph`, `ShowProgressBarVisibility`, `ShowUserInfoVisibility`, `ShowDatabaseInfoVisibility`, `DatabaseIconBrush`); `DatabaseStatusToBrushConverter` (`Colors.*`) + `StringToBrushConverter` kullanımı.
- `NotificationService` (200): `ShowTagged`+`NotificationGroups` var; tip-ikon ayrımı/severity görseli yok.
- **Kural-11 ihlali:** 5 yerde sabit `IsActive="True"` ProgressRing (`NamePasswordControl`, `SistemYedekPanel`, `SistemGuncellemePanel`, `DatabaseInfoPanel`, `DonemIslemKartlariPanel`).

**Sınıf/dosya planı (Kural 8 onayı):**
- **A. Servis refactor:** (1) `IStatusMessageService` ölü üyeler silinir (`StatusColorHex`/`StatusIconGlyph`/`ShowStatusIcon`/`LastUpdateTime`/`LastUpdateTimeText`/`Initialize`); (2) `StatusMessageService` bölünür → `StatusMesajDurumu` (durum+INPC) · `MesajOtoGizleme` (auto-hide, `StatusAutoHideMs` bağlanır) · `MesajYurutucu` (async) · facade; renk View'a taşındığı için `Default` bug'ı kökten kalkar; dispatch `DispatcherQueue.GetForCurrentThread()`; (3) `IStatusBarService`/`StatusBarService` ölü üyeler silinir + **DB bool bug'ı** `Startup` ile düzeltilir; (4) **`NotificationService` → In-App message kanalı:** OS toast (`ToastNotificationManager`/`ToastContentBuilder` + **`CommunityToolkit.WinUI.Notifications 7.1.2` paketi** + elle AUMID registry/Start-Menu kısayolu hack'i) **tamamen kaldırılır**; bildirim in-app host'a (InfoBar tabanlı) iletilir; `ShowTagged`/`NotificationGroups` in-app'e uyarlanır (tag/group = mesaj kimliği + tekrar ezme) + tip-ikon (Success/Warning/Danger). **Trade-off:** uygulama arka planda/minimize iken bildirim görünmez — kritik arka plan işleri için ayrıca değerlendirilecek.
- **B. View redesign:** `ShellStatusBar.xaml(.cs)` **sıfırdan** Fluent footer (sol = durum+severity ikon+progress; sağ = kullanıcı · DB noktası · saat); **tüm renk `{ThemeResource}`** (`SystemFillColorSuccess/Caution/Critical`, `TextFillColor*`, `DividerStrokeColorDefaultBrush`); hardcode `Colors.*` + `StringToBrush`/`DatabaseStatusToBrush` kaldırılır; `IconCheckCircle/IconAlertTriangle/IconError`; `AutomationProperties.LiveSetting`; `IsActive` binding.
- **C. In-App message hattı (OS toast yerine):** `ShellView` içeriğinin üstüne ortak **in-app mesaj host'u** (InfoBar tabanlı: severity + başlık + metin + opsiyonel aksiyon + kapatılabilir; flaş/tekrar önleme; kuyruk). `INotificationService` bu kanala bağlanır; OS toast kaldırıldığı için `App.xaml.cs` bildirim kaydı + AUMID registry/kısayol kodu da temizlenir. **Çökme/başlatma bildirimi (kullanıcı kararı — Oturum 270):** OS toast yerine **dialog** ile bildirilir (`App.xaml.cs` unhandled-exception → hata dialogu); shell/in-app host henüz yokken de çalışan **bağımsız** yol (OS toast tamamen kalkar).
- **D. Progress disiplini (Kural 12):** uzun işlemler determinate (`i/N`,`%`,`iş adı`) + bitince **tek** özet; göç adımlarına yüzde (~30/70/100); `IsBusy` (`ObservableObject.Set`) tek çatı; 5 sabit `IsActive="True"` düzeltilir.
- **E. Kural-4 temizlik:** ölü converter'lar (`StringToBrushConverter`, `DatabaseStatusToBrushConverter`) + App.xaml kayıtları; ölü code-behind wrapper'ları.

**Kapı:** Kural 8 sınıf onayları + build 0/0 + `dotnet test` yeşil + Kural 18 canlı (Light/Dark + uzun işlem + hata + toast) + Kural 13 yardım (gerekiyorsa) + onay.

**Devir notu (yeni context):** Faz tek oturumda bitmez; **A → B → C → D** sırasıyla ilerle, her adımda build. Başlangıç okuması: `docs/REFERANSLAR.md` (Oturum 270 satırları) + bu bölüm + `LOG-261-280.md` (Oturum 270 envanteri). Dosyalar: `MuhasibPro/Services/UIService/{StatusMessageService,StatusBarService,NotificationService}.cs`, `Libraries/MuhasibPro.Business/Contracts/UIServices/{IStatusMessageService,IStatusBarService,INotificationService}.cs`, `MuhasibPro/Views/ShellViews/Shell/{ShellStatusBar.xaml,ShellStatusBar.xaml.cs,ShellView.xaml}`, `MuhasibPro/Configurations/Startup.cs`, `Libraries/MuhasibPro.ViewModels/ViewModels/Shell/ShellViewModel.cs`.

**Oturum 271 — Chunk-1 TAMAM (kod ✅ build 0/0 + test 498/498 + Kural 18 canlı kanıt):**
- **A1:** `IStatusMessageService` ölü üyeler silindi (`StatusColorHex`/`StatusIconGlyph`/`ShowStatusIcon`/`LastUpdateTime`/`LastUpdateTimeText`/`Initialize`); `ShowMessage`/`Execute*` imzalarında `int?` autoHide → null ise `StatusAutoHideMs`'ten okunur (yetim ayar bağlandı; caller explicit değerleri korunur).
- **A2:** `StatusMessageService` 440→4 dosya: `StatusMesajDurumu` (durum+INPC) · `MesajOtoGizleme` (auto-hide, ayar) · `MesajYurutucu` (3 async executor) · facade (~150 altı). Dispatch `App._dispatcherQueue` (eski `Initialize` ölü olduğu için dispatch fiilen yoktu — latent bug kapandı). `Default` tema rengi bug'ı kökten kalktı (renk View'a taşındı).
- **A3:** `IStatusBarService`/`StatusBarService` ölü üyeler silindi (`MaliDonem`/`IsTenantDatabaseConnection`/`SetTenantDatabaseStatus`/`Initialize`/`KullaniciAdiSoyadi`); **DB bool bug'ı** düzeltildi — `ShellViewModel` `ISistemDatabaseService.GetSistemDatabaseStateAsync` ile gerçek durumdan besliyor (Drawer: status çubuğu DB göstergesi artık "Sistem veritabanı bağlı"). `ShellViewModel` çift-atama bug'ı (UserName iki kez yazılıyordu) düzeltildi; `LoadAsync` navigasyonu geciktirmiyor.
- **B:** `ShellStatusBar.xaml(.cs)` sıfırdan Fluent footer — sol severity ikon (4 `FontIcon`, enum→görünürlük) + mesaj + ring/percent bar; sağ kullanıcı · sistem DB noktası · saat; **tüm renk `{ThemeResource}`** (`SystemFillColor*`/`TextFillColor*`/`DividerStrokeColorDefaultBrush`), `AutomationProperties.LiveSetting`, `IsActive` binding. Hardcode `Colors.*` 0.
- **E:** `StringToBrushConverter` + `DatabaseStatusToBrushConverter` silindi + `App.xaml` kayıtları kaldırıldı; ShellStatusBar ölü wrapper'ları (StatusGlyph/ShowProgressBarVisibility/ShowUserInfo/DatabaseInfo/DatabaseIconBrush) kaldırıldı.
- **Canlı (Kural 18):** `Temp/opencode/ot290b_statusbar_current.png` (Dark) + `ot290b_statusbar_toggled.png` (Light) — `Hazır` + yeşil nokta **"Sistem veritabanı bağlı"** + saat; `ot290e_already_open.png` (SingleInstanceGuard "zaten açık" dialogu — task-kill yapılmadı).
- **NOT (Oturum 272'de çözüldü):** Durum çubuğu yalnız `ShellView` içindeydi; ana pencere akışı `ShellView` kullanmıyor. **Kullanıcı kararı (Oturum 272):** çubuk **MainShellView altına tam genişlik footer** olarak eklendi; `FirmaShellView`'de yok; `ShellView` (detay pencereleri) aynen. Mesajlar pencere başına ayrı (Scoped DI) — iletim yok.
- **Chunk-2 (sıradaki):** C in-app mesaj hattı + A4 `NotificationService` rewrite (OS toast + `CommunityToolkit.WinUI.Notifications` paketi + AUMID/StartMenu hack'i kaldırılır, `NotificationEnabled` ayarı bağlanır) + D progress (5 sabit `IsActive="True"`). `App.xaml.cs` çökme bildirimi zaten dialog (`ShowNotificationAsync`) — yalnız `NotificationService.Initialize()` çağrısı kalkacak.
- **Oturum 272 — ana pencere çubuğu + aktif bağlam (kod ✅🧪):** `MainShellView` footer (`ShellStatusBar`); `MainShellViewModel.LoadAsync` (base'siz) kullanıcı+DB beslemesi + yardım maddesi; `ShellViewModel` iki besleme metodu `protected`; `IStatusBarService`/`StatusBarService` → `FirmaAdi`/`MaliDonemAdi`/`IsTenantDatabaseConnection`/`TenantDatabaseMessage` (`IFirmaWithMaliDonemSelectedService` aboneliği + `IDisposable` + `RefreshAktifBaglam`); **bağlam yalnız gerçek yüklü tenant (`ConnectedTenantDb.IsLoaded`) varken gösterilir** (ham seçim yetmez — çoklu firma/dönemde eski seçim görünmez) ve **MainShell girişinde tazelenir**; `ShellStatusBar` sağ blok: Sistem.db ikincil · firma · dönem · tenant noktası · kullanıcı · saat (SAP status bar referansı); **host kapısı `BaglamGoster`** → firma/dönem yalnız `MainShellView`'de, Ayarlar/Yeni Firma pencerelerinde gösterilmez. Build 0/0 + test 498/498 + canlı Light/Dark (`ot292*`/`ot292b*`/`ot292c*`). **Kullanıcı onayı ALINDI** ("yapı oturdu, onaylıyorum").
- **Yeni gereksinim (kullanıcı — rafla):** Firma tarafına **"Varsayılan firma"** kavramı eklenecek (tek firma varsa zaten varsayılan sayılır); FirmaShell ilk seçim/çoklu firma davranışı buna göre.

---

## Faz 6.87 — Veritabanı Güncelleme sayfası (`TenantDatabaseUpdateView`) komple yeniden tasarımı — ❌ GEÇERSİZ (kapatıldı, Oturum 278)
> **KAPANIŞ (Oturum 278):** Kullanıcı kararı "sayfa geçersiz → SİL" olarak uygulandı (**Faz 6.91-E**): `TenantDatabaseUpdateView` silindi, göç erişim anında onay + inline ilerlemeye taşındı. Yeniden tasarım gerekmiyor — faz **geçersiz, kapatıldı**. (Aşağıdaki v1/kapı notları tarihseldir.)

> **Oturum 273 güncellemesi:** v1 redesign (hero + iki sütun + InfoBar) kullanıcı tarafından **reddedildi** ("Güncelleme sayfasından bahsediyorum; referansı sil, baştan tasarım"). Ayrıca "tenant DB update sayfası gerekli mi, gerçek update ile ilgilenelim" kararı açık. v1 XAML'e geri dönülecek; yön: (a) sayfa kalsın + baştan tasarım, veya (b) kaldır → erişimde otomatik migration + hafif onay/progress. **Karar bekliyor.**

> **Kullanıcı isteği:** 6.83 canlı turunda ekran görüldükten sonra — "güncelleme sayfası komple yeniden tasarlanacak".

**Mevcut ekran (6.83 turunda doğrulandı):** `Sürüm` şeridi (mevcut → güncellenecek + `N göç` hapı), `Değişiklik` tablosu, `İşlem adımları` (Yedek→Göç→Doğrulama, durum metinli), sağ üst `?`, başlıkta `Geri` + `Yedekle ve Güncelle`. Canlı kanıt: `Temp/opencode/ot286_upd.png` (gerçek pending göç: 1.0.0→1.1.0, `TenantDatabaseVersiyonlar` tablosu).

**Kapsam:** sayfa komple yeni tasarım — Fluent + **Kural 17** (zemin → ana border → kartlar; mühür), muhasebe/ERP sektör deseni (Kural 14: QuickBooks/Sage update/verify akışı), `?` yardım (Kural 13), Kural 11/12 (busy + sonuç + akış planı), Light/Dark; **davranış korunur** (Yedek→Göç→Doğrulama + hata/oto-geri-alma).

**Ayrıca (aynı oturum — UI rötüş):** `CircleIconButtonStyle` (tüm `?` yardım butonları + ShellView BackButton): opak sistem dolguları — Normal `SolidBackgroundFillColorBaseBrush`, hover `SolidBackgroundFillColorTertiaryBrush`, pressed `SolidBackgroundFillColorSecondaryBrush` (**tam opak; üstüne bindiği ana border çizgisi görünmez**) + border accent'ten çıkarıldı (`ControlStrokeColorDefaultBrush`). Denemeler elendi: `CardBackgroundFillColorDefaultBrush`, `Transparent`, `ControlFillColor*` (yarı saydam → arkayı gösteriyor). Build 0/0; canlı kanıt `ot288_yardim.png` (FirmaShell) + `ot288_login.png` (LoginView).

**Rötüş v2 (Oturum 270 — kullanıcı isteği):** 269'un opak `SolidBackgroundFillColorBaseBrush`'ı **Dark'ta `#202020`** olduğu için ana border yüzeyi üstünde "siyah nokta" gibi kalıyordu ("çok siyah, tema rengiyle uyumlu olsun, göze batmasın"). `DesignTokens.xaml`'e butona özel tema-farkında opak token eklendi: `MuhasibCircleIconArkaplanColor` (**Light aynen** `#F3F3F3` / **Dark** `#333A3E`) + hover `#3F464B` + pressed `#2A3033`; `Buttons.xaml` `CircleIconButtonStyle` bu token'lara bağlandı (opaklık korunur → arkadaki 1.5px ana border çizgisi yine görünmez). Build **0 hata / 0 uyarı**; canlı Dark kanıt: buton `#333A3E` ≈ çevre ana border `#383F43` (`ot289e`/`ot289f`/`ot289g`). **Kullanıcı onayı ALINDI** (Oturum 270 — "çok güzel oldu, onaylıyorum").

**Kapı:** Kural 14 araştırması (REFERANSLAR) + Kural 8 sınıf onayları + build 0/0 + test + Kural 18 canlı (Light/Dark + başarı + hata/oto-geri-alma) + onay.

---

## Faz 6.88 — MaliDonemYönetimView + SistemDbYonetimView yeniden tasarımı (Oturum 272 kullanıcı kararı — 📋 plan)

> **Kullanıcı kararı (Oturum 272):** Bu iki view yeniden tasarlanacak; **bu view'lara ara iş yapılmayacak** ("burada işlem yapma, sadece faz aç sonra hatırlar"). Kural 11 sabit ring düzeltmeleri de bu faza ertelendi.

**Kapsam:** `MaliDonem/Yonetim/*` (MaliDonemYonetimView + alt paneller) ve `SistemDbYonetim/*` (SistemDbYonetimView + alt paneller) komple yeniden tasarım — Fluent + Kural 17 (zemin → ana border → kartlar), Kural 16 (birincil aksiyon kart başlığında sağda), Kural 11/12 (busy ring + sonuç + akış), Kural 13 (`?` yardım), Kural 14 araştırma (muhasebe/ERP dönem yönetimi + DB bakım desenleri), Light/Dark.

**Devredilen / birleştirilen maddeler:**
- **6.71/m.5 — RAF panel temizliği (16 kart):** RAF panel = `MaliDonem/Yonetim/.../YonetimAyarlar*` (aynı view ağacı) → bu fazda. Model+provider+test kalır, panel kalkar: AppPlatform 5 · Identity 3 (Pbkdf2 **önce provider'a bağlanır**) · Database 7 · License 1.
- **6.71/m.3 — View yıkım/yeniden tasarım envanteri (16 XAML ünitesi, 32 dosya):** İPTAL edildi, hedefli onarım uygulandı; envanter bu fazın kapsamı.
- **6.71/m.4 — progress kısmı:** uzun işlemler determinate + tek özet; silme yüzdesi; `TenantDatabaseUpdateViewModel` adım rozetine yüzde (bildirim/InfoBar kısmı **6.86 Chunk-2a**'da tamamlandı).
- **Kural 11 sabit `IsActive="True"` ring:** `DonemIslemKartlariPanel.xaml:84`, `DatabaseInfoPanel.xaml:151`, `SistemGuncellemePanel.xaml:60`, `SistemYedekPanel.xaml:87` (bayrağa bağlanacak).
- Not: `FirmaShellViewModel.OnTenantUpdateAvailable` 6.86'da kaldırıldı (tekrar yok).

**Kapı:** Kural 14 araştırması + Kural 8 sınıf onayları + build 0/0 + test + Kural 18 canlı + onay.

---

## Faz 6.89 — LoginView: "Beni hatırla" + QuickLogin (hızlı giriş) incelenmesi (Oturum 272 kullanıcı kararı — 📋 plan)

> **Kullanıcı kararı (Oturum 272):** `LoginView` içindeki **"Beni hatırla"** ve **QuickLogin (hızlı giriş)** incelenecek; davranış/güvenlik/tasarım gözden geçirilecek.

**Kapsam (incelenecek):** `Views/Login/LoginView.xaml(.cs)`, `Views/Login/QuickLoginPanel.xaml(.cs)`, `Views/Login/NamePasswordControl.xaml`, `LoginViewModel`/`QuickLoginAccountsViewModel` + ilgili servisler — "Beni hatırla" bayrağının kalıcılığı/şifre saklama niyeti, QuickLogin hesap listesi davranışı, Kural 13 yardım metinlerinin güncelliği.

**Kapı:** Kural 14 araştırması (Kural 19 defteri) + Kural 8 sınıf onayları + build 0/0 + test + Kural 18 canlı + onay.

---

## Faz 6.90 — Velopack git güncelleme akışı (repo adresi dinamik + gömülü varsayılan kaynak) (Oturum 273 kullanıcı kararı — 🔨 kurulum ✅)

> **Kullanıcı isteği (Oturum 273):** "Bu işlemi git üzerinden komple kur. Git adresi değişebilir; varsayılan olarak mevcut git'i kullana. Dev-mode + admin güncelleme adresini (repo adresini) değiştirebilir."

**Yapılan (✅ kod + test):**
- `release.yml`: repo adresi sabit değil — `${{ github.server_url }}/${{ github.repository }}`; `dotnet publish ... -p:GuncellemeFeedUrl=<repo>`.
- `MuhasibPro.csproj`: `GuncellemeFeedAdresiEkle` hedefi (`BeforeTargets="GetAssemblyAttributes"`) → `git config --get remote.origin.url` (veya CI property) → `AssemblyMetadata("GuncellemeFeedUrl")`.
- `MuhasibPro.Domain.AppGuncellemeBilgisi`: metadata okur + normalize eder (`git@`/`ssh://`/`.git` → https).
- `UpdateService.GetSettingsAsync`: boş kaynakta gömülü varsayılanı döndürür.
- Admin (`UpdateView` Ayarlar): kaynak adresi + "Varsayılana sıfırla" + gömülü varsayılan gösterimi.
- Dev-mode (`GelistiriciAraclariPaneli`, yalnız DEBUG): "Güncelleme Kaynağı" kartı (anında kayıt + sıfırla).
- Test: `AppGuncellemeBilgisiTests` (+9). Build 0 hata; **507/507**.

**Kalan (kapı):**
- [ ] Uçtan uca canlı: Setup.exe ile kur → `vpk pack/upload` (tag) → uygulama "güncelleme var" görür → indir/kur (delta kanıtı) → onay.
- [ ] Velopack `OnRestarted` hook (boş) → sistem.db senkronu + tenant pending tespiti; `PostUpdateDatabaseSyncAsync` bağlanmalı. **→ Faz 6.91'e taşındı.**

---

## Faz 6.91 — Product-ready güncelleme hattı: ön-yedek → doğrulama → göç → verify → gerekirse restore (uçtan uca) (Oturum 273 istek + Oturum 274 derin araştırma — 📋 plan)

> **Kullanıcı isteği:** "Güncelleme alt yapısına başlamadan detaylı araştırma: nerede/nasıl uygulama güncellenecek, güncellenen uygulama doğrulanacak, veritabanı güncellemesi (hem sistem hem dönem) yeni güncellemeyle uyumlu mu kontrol edilip; doğrulama, restore, yedekleme product-ready yapılacak."

### İki katman model (Oturum 273'te netleşti)
- **Katman A — App binary update (Velopack):** feed = GitHub Releases (HTTPS); `vpk pack` full+delta üretir (uygulama yalnız değişen dosyaları indirir, `MaximumDeltasBeforeFallback=10`); atomik sürüm klasörü + Setup.exe rollback. App update tenant DB'yi migrate **etmez**.
- **Katman B — Şema (Sistem.db + dönem/tenant DB):** EF Core migration; yedek→göç→doğrula→oto-geri-al (mevcut tenant saga'sı) + post-update doğrulama.

### Araştırma bulguları (Kural 14/19 → `REFERANSLAR` Oturum 274)
- **Paket doğrulama:** Velopack `VerifyPackageChecksumAsync` hash doğrular (`ChecksumFailedException`) — bu **bütünlük** kontrolüdür, **güven kökü değil**; güven kökü = **Authenticode kod imzası** (Velopack `vpk --signParams`/Azure Trusted Signing), dağıtımdan önce zorunlu.
- **Veri yolu (⚠️ kritik tespit):** Velopack `%LocalAppData%\{AppId}\current` klasörünü her güncellemede **tümüyle değiştirir**; `%LocalAppData%\{AppId}` **uninstall'da silinir**. Uygulama şu an `%LocalAppData%\MuhasibPro\Databases` kullanıyor → **uninstall muhasebe verisini siler.** Veri app kökü dışına (ör. `%AppData%\MuhasibPro` — uninstall'dan sağ kalır) taşınmalı ya da uninstall koruması kurulmalı.
- **Rollback:** `AllowVersionDowngrade` ile bozuk sürümden geri dönüş; auto-apply asla downgrade yapmaz, explicit apply şart; downgrade daha yeni yerel paketleri siler.
- **Üretim güncelleme sırası (fail-closed):** metadata doğrula → **staging'e indir** → hash/size/imza doğrula → **eskiyi koruyarak aktive** → ilk açılışta **health-check** → yanlışsa **rollback**. Doğrulama düşerse **devam etme**.
- **İleri-uyumluluk guard (yeni, zorunlu):** şema sürümü damgalanır; **disk sürümü > binary sürümü ise reddet**; damga **başarılı migration SONRASI** atılır (başarısız migration damgalamaz). Velopack Setup her seferinde "repair/reinstall" yaptığı için eski Setup, yeni DB'nin üstüne kurulabilir → guard bunu yakalar.
- **SQLite disiplini:** canlı dosya kopyası **torn copy** üretir; güvenli yol `VACUUM INTO` (mevcut) ; restore öncesi bayat `-wal`/`-shm` temizliği (mevcut `CleanupSqliteWalFiles`) ; **her yedek doğrulanır**; bozuk dosya **karantinaya** alınır (silinmez).
- **Sektör (QB/Sage 50/100):** doğrulanmış + **test edilmiş** yedek; önce **kopya üzerinde test**; yarıda kalırsa yarım dosya açılmaz → yedek **temiz klasöre** restore + tekrar dene; göç sonrası **mutabakat**; sürüm-etiketli yedek adı.
- **EF Core çok-kiracılı:** her tenant DB ayrı migrate; erişimde runtime migrate kabul (tek örnek/EF9 lock).

### Product-ready hedef akış (mevcut koda eşleme)
1. **Release hattı (kök güven):** `release.yml` → `vpk pack` + **Authenticode imza** (`--signParams`/Azure Trusted Signing); SemVer + `DbSchemaVersions.CurrentSchemaVersion` birlikte bump; `releasenotes.md` (`--releaseNotes`) → Velopack `ReleaseNotesHtml`.
   - *Dosya:* `.github/workflows/release.yml`, `MuhasibPro.csproj`.
2. **Kontrol + gösterim:** `UpdateViewModel.CheckForUpdatesAsync` → sürüm `eski→yeni` + changelog; `UpdateState` akışı korunur.
3. **Ön-yedek (pre-update) — `PrepareForUpdateAsync` gerçek yedek alır:**
   - WAL checkpoint (`TRUNCATE`) + Sistem.db **gerçek `VACUUM INTO` yedeği** (sürüm-etiketli: `pre-v{from}→v{to}`) + yedeği `integrity_check` ile doğrula.
   - Şema değişimi **varsa** (`GetPendingMigrationsAsync` / tenant pending taraması) → etkilenen **dönem DB'lerinin** yedeği (yalnız göç gerektirenler; maliyet sınırlı) + doğrula.
   - `UpdateSettingsModel`'e kaydet: `LastUpdateFromVersion`, `LastUpdateToVersion`, `LastUpdateBackupPath`, `LastUpdateStartTime` (yeni alanlar).
   - Yedek alınamazsa **güncellemeyi başlatma** (fail-closed).
4. **Hook (yalnız bayrak):** `App.VelopackInitialize()` → `OnRestarted`/`OnFirstRun` yalnız localSettings `PostUpdatePending=true` + from/to yazar; UI/ağır iş yok. (FastCallback'e **DB işi konmaz** — headless, 15-30sn, kill.)
5. **Post-update saga — `IPostUpdateDogrulamaService` (Business.Scoped, DI), aktivasyon sonrası:**
   - (a) **Uygulama dosyaları:** çalışan sürüm == paket sürümü; kritik dosyalar + migration assembly mevcut; hash/`application_id` tutarlı.
   - (b) **İleri-uyumluluk guard:** Sistem.db **ve** her dönem DB'si için `stored SchemaVersion > CurrentSchemaVersion` ise **fail-closed** (kullanıcıya "daha yeni sürümle oluşturulmuş, uygulamayı güncelleyin"); yazma yok.
   - (c) **Sistem.db:** state (connect/valid/pending) → gerekirse migrate + yeniden **verify** (`integrity_check` + tablo/satır mutabakatı); **başarısızsa** 3'teki pre-update yedekten restore + restore'u da verify et; restore de başarısızsa **blokla** + yönlendir.
   - (d) **Dönem (tenant) DB'leri:** listeyi tara; her biri `GetTenantDatabaseStateAsync` (connect/valid/pending):
     - **bozuk** → son **doğrulanmış** yedekten restore (+verify), yoksa karantina + rapor;
     - **pending** → raporla (**oto-migrate YOK** — erişimde mevcut saga zaten yedek→göç→verify→oto-restore).
   - (e) **Sonuç:** adım rozetli `PostUpdateSonucu` → in-app InfoBar + `ISistemLogService`; bayrağı temizle. Göç sonrası **taze yedek** + sürüm damgası.
6. **UX (Kural 11/12/16/17):** ön-yedek/göç/verify için **determinate** ilerleme + adım rozeti, bitince **tek** özet; hata `ShowError` ile; `?` yardım sayfası maddeleri güncel (Kural 13).
7. **Dev-mode:** sonuç **"Tanılama"** ile görünür (mevcut `ModulTestCalistirici`/`SistemDiagnosticsViewModel` hattı).

### Hata matrisi (fail-closed)
| Durum | Davranış |
|---|---|
| Ön-yedek alınamadı/doğrulanamadı | Güncellemeyi başlatma, kullanıcıya bildir |
| Uygulama kritik dosyası eksik | Block + yeniden kurulum yönlendirmesi (Velopack Setup) |
| Sistem.db migrate hatası | Pre-update yedekten restore → tekrar verify; restore da düşerse blokla + yönlendir |
| Disk şema sürümü > binary | **Reddet** (yazma yok) + "uygulamayı güncelle" |
| Dönem DB bozuk, yedek var | Son doğrulanmış yedekten restore + verify |
| Dönem DB bozuk, yedek yok | Karantina (silme) + rapor; o dönem açılmaz |
| Dönem DB pending | Rapor; erişimde mevcut yedek→göç→verify akışı |

### TenantDatabaseUpdateView kararı (Oturum 274 — kullanıcı isteği: "geçersizse sil, post-update kontrol için gerekliyse kullan")
**Verdict: sayfa geçersiz — SİL; göç işi erişim anına (onay + inline ilerleme) taşınır.** Post-update kontrol için **gerekli değil** (onu yeni `IPostUpdateDogrulamaService` yapıyor).
- **Neden geçersiz:** Sayfa, Katman B (tenant şema göçü) için ayrı bir "seçim→onay→sayfa→başlat→Devam" akışı kuruyor. Yeni modelde (a) app update = Velopack, (b) uyumluluk/doğrulama/bozuk-restore = post-update saga, (c) pending göç = **erişimde** zaten var olan yedek→göç→verify→oto-restore motoru. Sayfa bu motorun üstüne gereksiz bir ekran katmanı; bilgi rozeti zaten `MaliDonemListVM`'de (`TenantUpdateAvailableEvent`).
- **SİLİNECEK (yalnız ekran katmanı):** `Views/ShellViews/Shell/TenantDatabaseUpdateView.xaml(.cs)` · `ViewModels/.../Shell/Tenant/TenantDatabaseUpdateViewModel.cs` · `TenantDatabaseUpdateYardim.cs` · `Startup.cs:52` nav kaydı · `AddAppViewModelHostBuilderExtensions.cs:25` (`AddTransient<TenantDatabaseUpdateViewModel>`) · nav çağrıları `MaliDonemYonetimView.xaml.cs:159`, `MaliDonemlerListControl.xaml.cs:189`, `TenantDatabaseUpdateCoordinator.cs:62` · `FirmaShellViewModel` `OnTenantUpdated` aboneliği (`:202`,`:242`) · ilgili testler (`TenantDatabaseUpdateTests` sayfa bölümü).
- **KALACAK (göç motoru + girdi sözleşmeleri):** `ITenantDatabaseUpdateService`/`TenantDatabaseUpdateService` · `TenantUpdateAkisYoneticisi` (yedek→göç→verify→oto-restore + adım/determinate) · `TenantUpdateProgressViewModel` (inline ilerleme) · `TenantDatabaseUpdateDialog` (onay, `SetState(check)`) · `TenantDatabaseUpdateCoordinator` (yeniden amaç: onay → moturu **inline** çalıştır → başarıda seçim + `TenantEvents.Updated` → MainShell; sayfa yok) · `TenantDatabaseUpdateArgs` · `TenantUpdateCheckResult`.
- **Erişim akışı (yeni):** dönem seç → `CheckUpdateRequiredAsync` → pending ise `TenantDatabaseUpdateDialog` (özet + "Şimdi Güncelle / Daha Sonra / Vazgeç") → onayda motur inline + determinate ilerleme → başarıda seçim + geçiş; hata/oto-restore sonucu tek bildirim.
- **Post-update kontrol:** yeni saga tenant'ları tarar; **bozuk** → son doğrulanmış yedekten restore, **pending** → rozet/InfoBar raporu. Yani kontrol **yeni sagada**, sayfada değil.

### Kararlar (Oturum 274 — kullanıcı onayladı ✅)
1. **TenantDatabaseUpdateView:** **SİL** + göç erişimde onay/inline ilerleme (yukarıdaki karar bölümü).
2. **Veri yolu:** `%AppData%\MuhasibPro`'ya **taşınacak** (uninstall veriyi silmesin; Velopack `%AppData%`'yı korur).
3. **Dönem taraması zamanı:** **açılışta splash adımı (bloklayıcı)** — kurtarma penceresi açılıştır; adım rozeti + progress.
4. **Ön-yedek kapsamı:** **Sistem.db + göç gerektiren dönem DB'leri** (hedefli).
5. **Kod imzası:** **sertifika yok → bütünlük (Velopack checksum) + HTTPS/GitHub ile başla**, Authenticode imza sonraki tur.
6. **Faz bölünmesi (uygulama sırası):**
   - **6.91-A ✅ (Oturum 274):** Veri yolu taşıma (`%AppData%`) + release hattı (changelog/fetch-depth) — ayrıntı aşağıda.
   - **6.91-B ✅ (Oturum 274):** Ön-yedek + `UpdateSettingsModel` alanları + `InstallUpdate` → WithDatabaseSync + hook bayrağı — ayrıntı aşağıda.
   - **6.91-C ✅ (Oturum 274):** İleri-uyumluluk guard (Sistem + dönem; disk şema > binary → fail-closed) — ayrıntı aşağıda.
   - **6.91-D:** `IPostUpdateDogrulamaService` saga (dosya + Sistem.db migrate/verify/restore + dönem tarama/rapor) — açılış splash adımı.
   - **6.91-E ✅ (Oturum 278):** TenantDatabaseUpdateView silme + erişimde onay/**inline** göç. Sayfa (view+VM+yardım) silindi; `TenantDatabaseUpdateCoordinator` motoru **inline** çalıştırır (`check → dialog → engine → tek bildirim`); `FirmaShellView`'e determinate ilerleme + adım rozetleri eklendi; manuel "Güncelle" butonları (Mali Dönem Yönetimi + İncele listesi) ve InfoBar aksiyonu **kaldırıldı** (informational flyout); `TenantEvents`/`OnTenantUpdated` aboneliği silindi. Build 0 + test **614/614** (3 `ContinueNavigasyonTests` silindi; saga/coordinator testleri inline motora göre yeniden yazıldı). Canlı: normal erişim (FirmaShell → Devam Et → MainShell) ✅; **pending yol canlı üretilemedi** (3 dönem de güncel) — dialog+inline akış birim testli (`Coordinator_SimdiGuncelle_InlineGocCalisir` + saga). Tandem: `docs/yardim/07` yeni akışa güncellendi, `YardimIcerikToplayici` girdisi düştü.
   - **6.91-F:** UX (determinate + adım rozeti + tek özet) + dev-mode Tanılama + yardım.
   - **6.91-G ⬜ (kullanıcı isteği, Oturum 278):** `AsistanBilgi.db` (AI yardım bilgi tabanı) post-update **tazeleme** adımı — saga'ya 4. adım `AI Yardım Dizini` (`IYardimBilgiTabani.HazirlaAsync`; içerik sürümü değiştiyse yeniden indeks, embedding önbellekte değilse lexical-only + uyarı; bozuk DB yeniden oluşturulur; **bloklamaz → Dikkat**). İleri-uyumluluk guard'ı/migration UYGULANMAZ (önbellek). UI: `GuncellemeSonrasiView` adım şeridi 4 adım (Kural 8 onayı). **Bağımlılık:** 6.93 motoru (Adım 1). Ayrıntı: `docs/YARDIM-DB-PLAN.md` → "Güncelleme modülü entegrasyonu".

### Uygulama durumu — 6.91-A ✅ (Oturum 274)
- **Veri kökü:** `%LocalAppData%\MuhasibPro` → **`%AppData%\MuhasibPro`** (Roaming). `ApplicationPaths.GetAppDataFolderPath` + dev fallback güncellendi; uninstall artık veriyi silmez.
- **Tek seferlik taşıma:** yeni `IDataPathRelocationService`/`DataPathRelocationService` (Data, Singleton) — eski `%LocalAppData%\MuhasibPro\Databases` varsa yeni köke `Directory.Move` (başarısızsa kopyala+doğrula), doğrulanınca eskiyi siler; **başarısızsa eski veri korunur + fail-closed** (boş kuruluma düşmez). Açılışta `DatabaseValidation` adımının başında çalışır. Geliştirme modunda atlanır (dev verisi proje klasöründe).
- **Ölü kod (Kural 4):** `Paths/BasePathHelper·DatabaseStructureProvider·BackupPathProvider·DatabaseValidationProvider` (0 caller, eski yolu taşıyan artıklar) **silindi**.
- **Release hattı:** `release.yml` → `fetch-depth: 0` + sürüm notları üretimi (`git describe/log`) + `vpk pack --releaseNotes` (feed'de changelog). Bütünlük Velopack checksum ile (karar: imza sonraki tur).
- **Doğrulama:** build 0 hata · test **514/514** (`+5` `DataPathRelocationTests`). **Canlı (Kural 18):** RELEASE exe ile sentetik eski kurulum (`%LocalAppData%\...\Databases\Sistem.db`) → taşıma sonrası `legacyDb=False`, `newSistem=True`, uygulama **Login** ekranına ulaştı (v1.1.3, Sistem.db Hazır/WAL); kanıt `Temp/opencode/ot274_reloc_login.png`. Test verisi temizlendi.

### Uygulama durumu — 6.91-B ✅ (Oturum 274)
- **Ön-yedek (fail-closed):** `ISistemYasamDongusuService.EnsureUpdateSafetyAsync()` — WAL checkpoint + `DatabaseBackupType.Migration` Sistem.db yedeği (oluşturucu zaten `IsValidBackupFile` ile doğrular) + `GetSistemKeep()` ile eski yedek temizliği. Sistem.db yok/geçersizse yedek gerekmez (engellemez); yedek **alınamazsa `basarili=false`** ve güncelleme **başlatılmaz**. `SistemYasamDongusuService`'e `IApplicationPaths` eklendi.
- **Servis:** `UpdateService.PrepareForUpdateAsync` artık gerçek yedeği alır (eski hâli yalnız durum okuyordu) + `LastUpdateFromVersion/ToVersion/BackupPath/StartTime` yazar (`UpdateSettingsModel` yeni alanlar); `TryGetCurrentVersion` = Velopack current → assembly fallback.
- **Akış:** `UpdateViewModel.InstallUpdate` → `async InstallUpdateAsync`: `PrepareForUpdateAsync` **başarısızsa hata + iptal**; başarılıysa `UpdateCheckCoordinator.ApplyWithDatabaseSync()` (kullanılmayan `Apply()` silindi).
- **Hook:** `App.VelopackInitialize` → `OnRestarted` yalnız `App.VelopackYenidenBaslatildi = true` (oturum-içi bayrak; ağır iş post-update sagasında — 6.91-D). `OnFirstRun` no-op.
- **Not (dönem ön-yedeği):** Yeni şemanın hangi dönemleri etkileyeceği **uygulama güncellenmeden bilinemez** (yeni migration'lar yeni binary'de). Bu yüzden dönem yedekleri **erişimdeki göç anında** (mevcut Yedek→Göç saga'sı) + post-update taramada (6.91-D) alınır. Pre-update adımda yalnız Sistem.db yedeklenir.
- **Doğrulama:** build 0 hata · test **518/518** (`+4` `SistemYasamDongusuTests`). **Canlı (Kural 18):** giriş → Denetim Masası → Geliştirici Araçları → modül testleri **41/41** (DI sağlam) + Güncelleme sayfası yüklendi; kanıt `Temp/opencode/ot274b_modul_test.png`, `ot274b_guncelleme.png`. (Uçtan uca yedek→uygula→hook→post-update **6.90/6.91 kapanışında** gerçek Setup ile.)

### Uygulama durumu — 6.91-C ✅ (Oturum 274)
- **Tek kaynak:** `DbSchemaVersions.IsNewerThanSupported(onDisk)` (`SemanticVersion` sayısal karşılaştırma) — disk şema SemVer'i `CurrentSchemaVersion`'dan büyükse fail-closed.
- **Durum modeli:** `DatabaseAnalysisResult.IsFutureSchema` + `DatabaseStatusResult.FutureSchema` + `GetStatus/GetStatusMessage` (net mesaj: "daha yeni sürümle (X) oluşturulmuş — güncelleyin").
- **Sistem.db (Data):** `SistemMigrationManager.GetSistemDatabaseStateAsync` (saklanan SemVer `AppDbVersiyonlar.CurrentDatabaseVersion` üzerinden guard) + `InternalInitializeAsync` (disk yeni ise **göç/yazma YOK**).
- **Dönem DB (Data):** `TenantSQLiteMigrationManager.GetTenantDatabaseStateAsync` (saklanan `TenantDatabaseVersiyonlar.CurrentTenantDbVersion`) + `InitializeTenantDatabaseAsync` (yeni ise göç YOK). `TenantDatabaseUpdateService.CheckUpdateRequiredAsync` yeni şemada `CheckSucceeded=false` (güncelleme sunmaz).
- **App fail-closed:** `LoginViewModel.DbIsReady = ... && !HasError && DatabaseValid` → guard sonrası **giriş kapalı**; Sistem.db sonsuz canlı (Kurulum'a düşmez, yazılmaz).
- **Doğrulama:** build 0 hata · test **527/527** (`+9` `IleriUyumlulukGuardTests`). **Canlı (Kural 18):** sentetik gelecek damgalı Sistem.db (`9.9.0`) ile RELEASE exe → `Splash: exists=True ready=False pending=0 target=MigrationRequired`, **SÜRÜM=9.9.0**, göç/oluşturma yok; kanıt `Temp/opencode/ot274c2_guard.png`. Test verisi temizlendi.
- **6.88'e devir (UI ince işi):** `SistemDbYonetimView` gelecek şemada "Geçerli/Güncel" gösteriyor ve "Giriş Ekranına Devam Et" butonu görünür kalıyor; giriş yine de `DbIsReady=false` ile kilitli. Bu yüzeyin (durum + buton koşulu) 6.88 yeniden tasarımında ele alınması gerekir.

### Uygulama durumu — 6.91-D ✅ (Oturum 275)
- **Karar değişikliği (kullanıcı):** tarama `ExtendedSplash` adımına **sıkıştırılmadı** → **yeni view'e yönlendirildi** (`GuncellemeSonrasiView`); 30sn timeout riski + daha iyi UX. Tenant future-schema → **sistem bloklar, dönem raporlanır**. Tetik **kalıcı** (`UpdateSettingsModel.LastUpdateVerifiedAt`; null + `LastUpdateToVersion` dolu → çalışır, crash dayanıklı).
- **Domain:** `PostUpdateAdimDurumu`/`PostUpdateAdimSonucu`/`PostUpdateDogrulamaSonucu` + `UpdateSettingsModel.LastUpdateVerifiedAt`.
- **Business (`DatabaseServices/UpdateDogrulama/`, nötr klasör):** `IPostUpdateDogrulamaService`+impl (orkestrasyon, Kural 1) + `IUygulamaDosyaDogrulayici` (kritik dosya/sürüm) + `ISistemDbGocDogrulayici` (guard→göç+verify→pre-update yedeğinden restore+tekrar verify→blok) + `ITenantTaramaDogrulayici` (bozuk→yedekten restore, pending→rapor, future→dokunma). DI Singleton. **Mimari not:** çapraz orkestratör `SistemDatabaseService`/`TenantDatabaseService` klasörlerinin dışında (`ArchitectureTests.Tenant_Ile_SistemDb...` bekçisi iki modül klasörünü tarar; bekçi yeşil).
- **Routing:** `SplashTarget.PostUpdateVerification` + `SplashRouteDecision.PostUpdateGerekli` (öncelik: FirstSetup → PostUpdate → MigrationRequired → Login).
- **UI:** `Views/ShellViews/Splash/GuncellemeSonrasiView` + `GuncellemeSonrasiViewModel` + `PostUpdateAdimGorunum` + `PostUpdateAdimDurumuConverter` + `SplashNavigator` branch + `AddAppViewModel` kaydı. Splash ailesi → `?` yardım muaf.
- **View revizyonu (kullanıcı isteği):** dikey adım listesi → **soldan sağa adım şeridi** (daire+bağlayıcı+etiket; `PostUpdateAdimDurumuConverter` renk/ikon); sonuç/blok panellerine ikon+başlık (`SonucBaslik`); uyarı mesajları sonuç paneli satırı; tooltip. Araştırma: DevWinUI `StepBar` adım-başı durum veremez → özel stepper (`REFERANSLAR` 275).
- **Kural 4 temizlik:** `IUpdateService.PostUpdateDatabaseSyncAsync` (+`UpdateService` kullanılmayan `ISistemDatabaseService` bağımlılığı) silindi.
- **Bugfix (canlı):** `Progress<T>` sıra/gecikme sorunu → tenant alt-ilerlemesi senkron `IlerlemeKopru` + `Mesaj=empty` (yalnız çubuk) + VM monotonik guard (stale rapor atlanır; terminal adım geri dönmez).
- **Doğrulama:** build 0 hata · test **556/556** (+29: `PostUpdateDogrulamaTests` 27 + `SplashRoutingTests` 2). **Canlı (Kural 18):** A uyumlu sürüm → otomatik Login + `LastUpdateVerifiedAt` damga; B sürüm farklı → adım şeridi + "Uyarılarla tamamlandı" + "Devam Et"; Light turu. Kanıt `Temp/opencode/ot275_a_final.png`, `ot275_b_08.png`, `ot275_light_final.png` + UIA.
- **Sırada:** **6.91-E** (TenantDatabaseUpdateView silme + erişimde onay/inline göç) → 6.91-F (UX + dev-mode Tanılama + yardım).

#### Revizyon 3 — Güncelleme Doğrulaması UX (Oturum 275 sonu onaylı plan → **Oturum 277'de ✅ UYGULANDI**)
> **Araştırma (Kural 19 → `REFERANSLAR` 275):** PatternFly Progress stepper (durumlar + içerik kuralı: fiil/geçmiş zaman, "başarısız" deme spesifik söyle), Red Hat progress stepper (inactive/active/**warn**/**fail**), Figr error-state (genel mesaj yerine spesifik sebep).
- **Sonuç = hero (üstte), adım değil.** Adım şeridi **3**: `Uygulama` → `Sistem.db` → `Dönemler`. Aktif adımda dönen `ProgressRing`; ✓ / ⚠ / ✕ / **Atlandı** (gri + ileri-ok).
- **Hâller:** 🟢 "Güncelleme tamamlandı" → **Devam Et** (temizse otomatik) · 🟡 "Güncelleme tamamlandı" + "Dikkat gerektiren durumlar:" listesi → **Devam Et** · 🔴 **"Uygulama güncellenemedi"** + "Daha sonra tekrar deneyin." → **Kapat → Login** (damga atılır; tek sefer) · 🔴 **"Sistem veritabanı güncellenemedi"** + spesifik sebep → **Veritabanı Yönetimi** (hard-block; damga atılmaz).
- **Atlanan adım:** başarısız adımdan sonra çalışmayan adımlar `Atlandi` (saga bildirir).
- **Sürüm karşılaştırması sağlamlaştırma:** ön-sürüm/metadata (`1.1.4-beta`, `+build`) yok sayılır; yalnız `major.minor.patch` (`SemanticVersion` ön-temizliği). Uyuşmazlık → "Uygulama güncellenemedi".
- **B1/B2 bug fix:** (B1) `UpdateService.PrepareForUpdateAsync` → `LastUpdateVerifiedAt = null`; (B2) tetik `LastUpdateToVersion` yerine **başarılı prepare sonrası yazılan `UpdateSettingsModel.PostUpdatePending`**; iptal/fail'de `Pending=false`+`ToVersion=null`; saga sonunda `Pending=false` (iptal edilen güncellemede yanlış ekran/sarı çıkmaz).
- **Kapsam (Kural 8 sınıf listesi):** `UpdateSettingsModel` (+`PostUpdatePending`), `PostUpdateAdimDurumu` (Atlandı görseli), `UygulamaDosyaDogrulayici` (sürüm normalizasyonu), `SistemDbGocDogrulayici`/`TenantTaramaDogrulayici` (sade mesaj), `PostUpdateDogrulamaService` (4→3 adım + hero verisi + `Pending` temizliği + `GerekliMi` bayrağı), `GuncellemeSonrasiViewModel` (hero durumları + buton görünürlükleri), `PostUpdateAdimGorunum`, `PostUpdateAdimDurumuConverter`, `GuncellemeSonrasiView.xaml(.cs)`, `UpdateService.cs`, testler.
- **Kapı:** build 0/0 + test + **Kural 18 canlı** (temiz / dikkat / uygulama-fail "Kapat→Login" / sistem-fail) + onay + demo `LocalSettings` yedeğini geri yükle.
- **✅ Sonuç (Oturum 277):** Uygulandı. Domain: `PostUpdateSonucTuru` (Temiz/Dikkat/UygulamaBasarisiz/SistemBasarisiz) + `UpdateSettingsModel.PostUpdatePending` (B2 tetik); saga **3 adım** (Sonuç pseudo-adımı kalktı) + başarısız adımdan sonrakiler **Atlandı**; app hatası = damga at + Pending temizle (tek sefer), Sistem.db hatası = sert blok (damga yok, Pending kalır); `SemanticVersion` ön-sürüm/metadata temizliği; `UygulamaDosyaDogrulayici` sürüm uyuşmazlığı artık **hata**. **Ek bulgu/düzeltme:** (a) `ApplyResult` adım durumlarını sonuçtan doğrudan yazar (geç `Progress<T>` bildirimi düşmesi — `HATALAR` 277); (b) renk converter'ları **silindi** (Light'ta tema-bağımsız brush — `HATALAR` 277) → View'da `ThemeResource` overlay'ler. Build 0 hata, test **603/603**. **Canlı (Kural 18):** A temiz→otomatik Login (`ot277_a_temiz_16.png`), B uygulama-fail→"Kapat" (`ot277_b_uygulama_final.png`), C sistem-fail→"Veritabanı Yönetimi"+adımlar Hata/Atlandı (`ot277_c_sistem_final.png`), D dikkat→"Devam Et"+uyarı listesi (`ot277_d_dikkat_final.png`); Light B/D (`ot277_lightB_final.png`, `ot277_lightD_final.png`). Pending/Verified geçişleri: A/B/D damga+Pending temiz, C dokunulmadı. Test verisi geri alındı (Sistem 1.0.0, tenant 1.1.0, LocalSettings yedeği).
- **⚠️ Ortam notu (çözüldü):** `LocalSettings.json` demo yaması geri yüklendi; özgün yedek (`LocalSettings.json.ot275inspect.bak`) yanında duruyor.

**Kapı:** Kural 8 sınıf onayları + build 0/0 + test + **Kural 18 canlı** (kontrollü: `vpk pack` → Setup ile kur → güncelle → `OnRestarted` doğrulama; Sistem.db ön-yedek + restore + ileri-uyumluluk guard denenir) + `REFERANSLAR` durum güncellemesi + onay.

---

## Faz 6.92 — AI Yardım Asistanı (sürüm-paketli, yerel Foundry) (Oturum 276+277 — Adım 1-6 ✅🧪; UI revizyonu ✅ (277); **sahiplik: UI 277'de, servis/contract diğer modelde**; Adım 7 servis+UI sonrası)

> **Sahiplik (Oturum 277 güncel):** AI **UI** (statü çubuğu flyout + panel + Denetim paneli) **Oturum 277'de**; **servis/contract** (model listele/sil/unload, alias-değişimi uygulama) **diğer modelde** — UI bağlantısı servis bitince. Çakışma yasak (LOG `📨`).

- [x] **1. Araştırma + REFERANSLAR ✅ (kod öncesi):** Foundry .NET SDK akışı (CreateAsync→EP→katalog→indir/yükle→streaming→unload) + NuGet TFM bulgusu (WinML=windows-TFM → impl App'te) + MEAI ret (öz sözleşme) + Xero JAX salt-okunur ilkesi + katalog (alias ayardan, varsayılan `qwen2.5-0.5b` geçici) + Phi Silica ret — `REFERANSLAR` 276 (6 satır) ✅
- [x] **2. Sürüm katmanı ✅ (kod):** `SurumKatalogu.AiAsistanIcerirMi` (Domain ürün sabiti) + `ISurumOzellikService`/`SurumOzellikService` (Scoped, fail-closed) + `Permission.AiAsistan_Kullan=2300` + DI + `SurumOzellikServiceTests` (6 fact: 4 tür × geçerli/geçersiz + fail) ✅🧪
- [x] **3. Business çekirdek ✅ (kod):** `AiAsistanSettings` (ModelAlias yönetici + EtkinMi/Gecmis/Madde/ZamanAsimi + Get* clamp) + provider (makine geneli anahtar + yetki + olay) + `IAsistanSohbetService` (Durum/Hazirla-determinate/SorStreaming/Kapat) + `FoundryAsistanSohbetService` (App, örnek-kod akışı, sessiz catch yok) + 3 NuGet (WinML 1.2.4 + Betalgo 9.1.0 + Logging.Abstractions 10.0.2) + DI + `AiAsistanSettingsProviderTests` (6 fact) ✅🧪
- [x] **4. RAG v1 ✅ (kod):** `IYardimIcerikSaglayici` + `YardimIcerikToplayici` (ViewModels, 8 sayfa statiklerinden okur) + 7 VM refactor (içerik → `YardimMaddeleri()` statiği, davranış aynı) + `YardimSkorlayici` (başlık×3/açıklama×1) + `AsistanPromptKurucu` (sistem + bağlam + madde + uydurma-yasağı + geçmiş kırpma) + 3 test dosyası (5+5+5 fact) ✅🧪
- [x] **5. UI yüzeyi ✅🧪 (kod + Windows: build 0 + 603/603 + C1-C11 kısmi):** `AsistanSohbetViewModel` (kapı+hazırlık+streaming+Durdur/Temizle, 6 fact) + `AsistanSohbetPaneli` (MainShell alt panel iç kartı: balonlar/ilerleme/hata/girdi+Enter/otomatik kaydırma) + `YapayZekaAyarSayfasi/Paneli` (Denetim Masası 9. bölüm: durum/model/davranış/hazırlık-alt-bölge) + DI + navigasyon + 9. menü + yardım maddeleri; Kural 8: yeni sınıf yok (mevcut SettingsCard/UserControl/Style/Converter) ✅
- [x] **6. Ayar + Tanılama ✅🧪 (kod + Windows):** Denetim AI paneli (Adım 5'te) + dev-mode "AI Bağlantı Öz-testi" (`AiOzTestCommand`: sürüm hakkı + model durumu + RAG derlemi, salt-okunur) + `GelistiriciAiOzTestTests` (3 fact) ✅
- [x] **5b. UI revizyonu ✅🧪 (Oturum 277, kullanıcı kararı):** asistan **statü çubuğunun en sağında 🤖 → sağa hizalı Flyout** (`TopEdgeAlignedRight`; MainShell yerleşimine girmez, popup); panel başlığında `?` → **gizle**; **model indirme panel içinde** (Denetim'deki "Modeli Hazırla" + hazırlık bölümü kaldırıldı, yalnız ayar + ipucu); ölü `HazirlaCommand`/panel `YardimCommand` temizlendi (Kural 4); yardım maddeleri güncellendi. Canlı: kapalı/açık/gizle (`ot277f_*`), **C2/C3 ✅** (model indi+yüklendi, düzgün Türkçe soru → RAG cevabı `ot277f_soru_19.png`). Build 0 + **604/604** ✅
- [x] **5c. Servis/contract ✅ (diğer model, Adım 8) + UI bağlantısı ✅ (Oturum 278):** model **listele + disk**, **sil** (+unload), **alias-değişimi uygulama** bitti. UI: Denetim "Model yönetimi" (`SettingsExpander` liste + disk özeti + `Yenile` + `Sil` onay dialogu) + "Model → Uygula" (`AliasDegisiminiUygulaAsync` + determinate ilerleme) + indirme yönergeleri; sohbet panelinde alias uyuşmazlığı yeniden-hazırlama. Canlı S1/S2/S3 + restore (`ot278*`), test **617/617** ✅🧪
- [ ] **7. Kapanış:** build 0/0 ✅ + test 617/617 ✅ + Kural 18 canlı (model liste/sil/alias-uygula ✅; Standart=kilit + streaming önceki oturumlarda) → **kullanıcı onayı bekliyor** ⬜
- [x] **8. Model yönetimi servisi ✅🧪 (kod + Windows/canlış, 277 isteği):** contract +4 metot (`ModelleriGetir`/`DiskKullanimi`/`ModelSil`/`AliasDegisiminiUygula`) + 3 DTO + Foundry impl (SDK imzaları DLL metaverisinden doğrulandı) + `ModelKlasorOlcer` + 4 fact; UI 278 bağladı, canlı S1-S4 ✅, test 617/617 ✅

**Kapı:** Kural 8 (Adım 5 view'ları) + build 0/0 + test + Kural 18 canlı + onay.
**Windows doğrulama paketi ✅🧪 (Oturum 275, 2026-09-16):** restore doğrulandı (`project.assets.json`) + build 0 hata (yeni dosyalardan uyarı yok) + test **583/583** (bekçi yeşil) + bozulan eski test yok.
**Windows doğrulama paketi Adım 5+6 ✅🧪 (Oturum 277, 2026-09-16):** build 0 hata + test **604/604** (597 + Rev3/UI değişiklikleri) + C2/C3/C4/C5/C6/C7/C8/C11 ✅ (C1 birim-testli; lisans=Deneme) + kritik bulgu (seed `KullaniciFirmaRol` yok → admin tüm yetkilerde kilitli) + UI revizyonu (statü çubuğu sağda flyout).

---

## Faz 6.93 — AI Yardım Bilgi Tabanı (AsistanBilgi.db + Hibrit RAG) (Oturum 278 plan ✅ → Oturum 279 motor ✅ → Oturum 280 Adım 2 + 6.91-G ✅)

> **Plan/sözleşme:** `docs/YARDIM-DB-PLAN.md` (dondurulmuş kontrat + veri modeli + davranış + dosya sahipliği).
> **Sahiplik (kullanıcı kararı, Oturum 278):** **motor + veri diğer modelde**, **UI/entegrasyon/doğrulama bende**.
> **Kararlar:** yalnız AI kapsamı · içerik repoda `docs/yardim/*.md` · **hibrit** (lexical + Foundry embedding + RRF; FTS5 yok) · ayrı `%AppData%\MuhasibPro\AsistanBilgi.db`.

- [x] **0. Araştırma + sözleşme ✅ (Oturum 278):** `REFERANSLAR` 278 (5 satır: Foundry RAG/embedding, BM25↔vektör/hibrit, FTS5 Türkçe sınırı, vstash RRF, on-device Türkçe karakterizasyon) + `docs/YARDIM-DB-PLAN.md` (frozen) ✅
- [x] **0b. İçerik başlangıç seti ✅ (Oturum 278, bende):** `docs/yardim/*.md` **9 sayfa / 60 madde** (giriş, firma-dönem seçimi, çalışma alanı, mali dönem yönetimi, veritabanı/yedekleme, uygulama güncelleme, dönem şema göçü, AI asistanı, geliştirici araçları) — format `#` sayfa / `##` madde / `Etiket:` ✅
- [x] **1. Motor ✅🧪 (Oturum 279 + fix 281):** `IYardimBilgiTabani` + DTO'lar, `YardimMarkdownCozumleyici`, `RrfBirlestirici`, `YardimSkorlayici` (TR normalizasyon), `YardimVektorDeposu` (SQLite), `YardimBilgiTabaniService`, `GomuluYardimIcerikKaynagi`, `FoundryYardimVektorUretici`, `AiAsistanSettings.EmbeddingModelAlias`, `docs/yardim/*.md`, retrieval `FoundryAsistanSohbetService`/`AsistanPromptKurucu`. Üstteki "1a" + "1b" alt maddeleri teslim (motor fix 281; canlı S2-S4 282).
  - [x] **1a. Motor Adım 1 ✅🧪 (Oturum 279):** sözleşme DTO/interface'leri + `YardimMarkdownCozumleyici` + `RrfBirlestirici` + `YardimSkorlayici` TR ext + `YardimVektorDeposu` + `YardimBilgiTabaniService` + `GomuluYardimIcerikKaynagi` + `FoundryYardimVektorUretici` + `EmbeddingModelAlias` + csproj `EmbeddedResource` + retrieval swap (`BilgiTabani` nullable property + fallback, `AramaSonuclariylaKur`). Build 0 hata (solution x64) + yeni dosyalardan uyarı yok + `Yardim*|Rrf` **33/33** ✅. `IYardimIcerikSaglayici` SİLİNMEDİ (sıra notu) ✅. **Kalan (Adım 2, bende):** DI kaydı + eski yolun ctor'a taşınması + `IYardimIcerikSaglayici`/`YardimIcerikToplayici` silme.
- [x] **2. UI/entegrasyon ✅🧪 (Oturum 280, bende):** DI (`IYardimIcerikKaynagi`/`IYardimVektorUretici`/`IYardimBilgiTabani` Singleton) + `FoundryAsistanSohbetService` ctor injection + eski RAG v1 yolu söküldü (`YardimIcerikToplayici` + `IYardimIcerikSaglayici` + `YardimliSayfaDto` + `MesajlariKur`/`IlgiliMaddeleriBul` silindi, Kural 4) + asistan paneli dizin durumu/ilerlemesi (Kural 11/12) + Denetim "Yapay Zeka" `Yardım dizini` kartı + `GelistiriciAraclari` öz-test sayımı + Kural 13 metinleri + `REFERANSLAR` 278 `Durum=✅`. Build 0 hata + **633/633** ✅
- [x] **3. Doğrulama ✅🧪 (bende, Oturum 282):** build 0 hata + **640/640**. **Kural 18 canlı:** **S1 ✅** (60 madde) · **S3 ✅** (embedding alias geçersiz → "yalnız anahtar kelime"; cevap geldi, çökme yok) · **S2 ✅** (embedding 495 MB + 60×1024 vektör; bağımsız prob doğru madde cosine **#2**; canlı cevap doğru maddeye dayandı) · **S4 ✅** (içerik +1 → panel açılışında **61 madde** + yeni vektör; geri al → **60 madde** + orphan 0). **Ek fix (canlı bulgu):** Denetim "Yardım dizini" DI wiring (çocuk VM'lere `IYardimBilgiTabani` iletimi, +2 test) · sohbet paneli **dürüst model durumu** + flyout ön-yükleme (çelişki giderildi, +4 test) · popup Kural 17 tema (onay ✅). **Kalan: kullanıcı onayı (6.93 kapanış) ⬜**
- [x] **4. Güncelleme modülü entegrasyonu ✅🧪 (Oturum 280, bende — Faz 6.91-G):** post-update saga 4. adım **`AI Yardım Dizini`** (`HazirlaAsync(modelIndirmeyeIzin:false)`, embedding indirilmez; bloklamaz, eksik semide → `Uyari`/Dikkat) + yüzde dağılımı (Dönem 60-88, Dizin 90-98) + `GuncellemeSonrasiView` 4 adımlı şerit (150→120px); 2 yeni test. Build 0 + 633/633
- [x] **1b. Motor fix ✅🧪 (Oturum 281):** kapı restructure (`true`→doğrudan `UretAsync`, `false`→`OnbellekteMiAsync`) + `FoundryYoneticiKurulum` tek kapı (ikiz bayraklar silindi) + testler. Build 0 + `Yardim*|Rrf` 24/24 + **634/634**; canlı S2-S4 (Oturum 282) ✅

**Kapı:** build 0/0 ✅ + test ✅ (640/640) + Kural 18 canlı (S1-S4) ✅ + `REFERANSLAR`/doküman güncel ✅ + kullanıcı onayı ⬜ (popup ✅)

---

## Faz 6.94 — Tek Yardım Yüzeyi (AI Asistanı) (Oturum 283 plan 📋 → kullanıcının ek soruları sonrası başlar)

> **Plan:** `docs/YARDIM-TEK-YUZEY-PLAN.md` (kilitli kararlar + envanter + H1-H5 + riskler).
> **Sahiplik:** içerik `docs/yardim/*.md` + UI sadeleştirme (Views/ViewModels) **ana modelde**; motor/prompt/retrieval **diğer modelde**.
> **Karar özeti:** yardım kitabı ❌ iptal · view `?` yardım listeleri/dialogu ❌ kaldırılır · tek kaynak `docs/yardim/*.md` → `AsistanBilgi.db` → asistan · giriş **F1 + statü çubuğu "Asistan"** · AI erişilemezse yalnız kilit gerekçesi · eski altyapı silinir (Kural 4) · kapsam tüm yapı.

### H1 — Karar / sözleşme (ben) ⬜
- [ ] `AGENTS.md` Kural 13 revizyonu: `?` yardım dialogu → **tek yüzey AI asistanı**; "yardım sayfayla yaşar" → "`docs/yardim` güncellenir"
- [ ] `AGENTS.md` okuma listesine `YARDIM-TEK-YUZEY-PLAN.md` eklenir
- [ ] `docs/YARDIM-DB-PLAN.md` sözleşme revizyonu (v1.2): "yalnız AI" → "tek yardım kaynağı = tüm uygulama; UI `?` kaldırıldı"
- [ ] `REFERANSLAR.md`: MS F1/help + uygulama-içi AI yardım araştırması (kaynak + karar)
- [ ] Kullanıcı onayı (Kural 8/15)

### H2 — İçerik genişletme (ben) ⬜
- [ ] 78 XAML view + ~81 mevcut yardım metni taraması (ekran/buton envanteri)
- [ ] Mevcut 9 sayfa/60 madde buton/işlem düzeyinde derinleştirme (projeye özgü; genel anlatım yasak)
- [ ] Yeni sayfalar: `10-denetim-masasi` · `11-sistem-veritabani-yonetimi` · `12-guncelleme-sonrasi` · `13-diyaloglar` · `14-acilis-kurulum`
- [ ] Yazım standardı: ekran → buton → adım → sonuç/onay/hata; `Etiket:` ekran+buton adlarıyla
- [ ] Kaynak: gerçek XAML/VM + mevcut yardım maddeleri (uydurma yok)
- [ ] İçerik gözden geçirme (kullanıcı); DB otomatik tazelenir

### H3 — UI sadeleştirme (ben, Kural 8) ⬜
- [ ] `?` butonları kaldırılır (Login, MainShell, FirmaShell, MaliDonemYonetim, DatabaseSettings, Update, SistemDbYonetim, DenetimMasasi, GelistiriciAraclariPaneli, YapayZekaAyarPaneli)
- [ ] `YardimMaddeleri()`/`YardimGoster`/`YardimCommand` + `YardimAnahtari`/`YardimBasligi` silinir (~81 madde)
- [ ] `YardimDialog` + `YardimMaddesi` (view) + `YardimMaddesiDto` + `ShowYardimAsync` (interface+impl) silinir
- [ ] Global **F1** → asistan paneli (statü çubuğu düğmesiyle aynı yol); kilitliyse gerekçe
- [ ] `TenantDatabaseUpdateView` ölü kod doğrulaması (Kural 4)
- [ ] Build 0/0 + test yeşil

### H4 — Motor (diğer model) ⬜
- [ ] Geniş derlem için retrieval ayarı (top-k / etiket ağırlığı)
- [ ] Prompt sertleştirme (yalnız maddelere dayan; genel tavsiye verme; "yardım maddesi yok" fallback)
- [ ] Değerlendirme seti: "soru → beklenen madde" (offline) + canlı ölçüm
- [ ] Sözleşme değişikliği gerekirse Kural 15 + onay + `YARDIM-DB-PLAN` revizyonu

### H5 — Doğrulama + doküman (ben) ⬜
- [ ] Build 0/0 + test (silinen yardım testleri temizliği)
- [ ] Kural 18 canlı: F1 → asistan; her ekrandan projeye özgü cevap; kilit senaryosu; kanıt + onay
- [ ] LOG/DURUM/ROADMAP/KONTROL/REFERANSLAR güncel

**Kapı:** H1 onayı → H2 içerik → H3 Kural 8 onayları + build/test → H4 motor → H5 canlı + onay.

---

## Faz 6.95 — Aktivasyon & Modül Kilidi (KEY) + İlk Giriş (Oturum 284 plan 📋)

> **Plan:** `docs/AKTIVASYON-MODUL-PLAN.md` (A1-A6). **Sektör araştırması:** `REFERANSLAR` 284.
> **Akış:** Splash → Login (üretici atadığı ilk kullanıcı) → **zorunlu şifre değişikliği** → **Modül Seçici** → seçilen modüller için **KEY** → onaylı key sonrası **Sistem.db modül erişimi güncellenir** → FirmaShell.
> **İki katman:** **6.95** = modül kilidi (lisans/KEY) · **6.85** = kullanıcı hakkı (RBAC). Modül erişimi = lisans seti ∩ kullanıcı izinleri.

### A1 — Model/sözleşme (kararlar ✅ — kod ⬜)
- [x] **Lisans kapsamı = kurulum-geneli (Sistem.db global)** ✅
- [x] **KEY doğrulama = çevrimdışı imza/checksum** ✅
- [x] **Deneme = anahtarsız 30 gün + kısıtlı modül seti** ✅
- [x] **Ek modül = tüm açık dönemler + erişimde onay/inline göç** ✅
- [ ] `Kullanici.SifreDegistirmeliMi` (+ gerekirse `Lisans` alanları) → migration **Kural 8 ✅ onaylı**
- [ ] `REFERANSLAR` derinleştirme (imza/checksum deseni — kod öncesi, Kural 14)

### A2 — Aktivasyon/KEY altyapısı (Data+Business) ⬜
- [ ] `ILisansService`/`IModuleLicenseService` genişletme: `AktivasyonAnahtariDogrula`, `ModulleriEtkinlestir`
- [ ] Anahtar → modül seti eşlemesi (ürün sabiti)
- [ ] Seed varsayılanları + doğrulama testleri

### A3 — Modül Seçici + KEY ekranı (UI) ⬜
- [ ] Login sonrası **Modül Seçici** + **KEY girişi** ekranı (Kural 17 + Kural 11/12 durum/ilerleme)
- [ ] Onaylı key → Sistem.db modül erişimi güncellenir → FirmaShell

### A4 — Zorunlu şifre değişikliği + yeni kullanıcı ⬜
- [ ] İlk girişte şifre değiştirme ekranı (mevcut şifre doğrulamalı; güç kuralı `IIdentitySettingsProvider`)
- [ ] Yeni kullanıcı oluşturma (admin) + atanan şifreyle ilk girişte zorunlu değişiklik

### A5 — Modül erişim kapısı + tenant şeması ⬜
- [ ] `MainShell` modül menüsü `IModuleLicenseService.IsModuleActiveAsync` ile (yetkisiz modül gizli/pasif + gerekçe)
- [ ] `ModuleLicenseViewModel` hardcoded `firmaId=1` → aktif firma
- [ ] **Modül → tenant migration eşlemesi** (ürün sabiti)
- [ ] **Yeni dönem oluşturma:** etkin modül setinin şemasıyla
- [ ] **Ek modül (yeni KEY):** mevcut dönem DB'lerine göç (ön-yedek → göç → doğrulama; hangi dönemler kararı ⬜)

### A6 — Doğrulama + doküman ⬜
- [ ] build 0/0 + test + Kural 18 canlı (aktivasyon→FirmaShell, zorunlu şifre, modül kapısı)
- [ ] Kural 13 yardım + dokümanlar

**Kapı:** lisans kapsamı + KEY doğrulama kararı ✅ → A1 migration (Kural 8 ✅) → A2-A5 → A6 canlı + onay.

---

## Faz 6.96 — Çoklu Veritabanı Sağlayıcı Desteği (SQLite varsayılan) (Oturum 284 plan 📋)

> **Plan:** `docs/DB-PROVIDER-PLAN.md` (D1-D8) + kod doğrulamalı SQLite bağımlılık haritası.
> **Karar:** SQLite **varsayılan** kalır; PostgreSQL/SQL Server eklenebilir (provider-soyut Data katmanı).

### D1 — Kararlar + sürüm/paket hizalama ⬜
- [ ] Açık kararlar: sunucu tenant modeli (ayrı catalog vs `TenantId`) · bağlantı bilgisi saklama · yedek anlayışı · hangi sağlayıcılar · `AsistanBilgi.db` SQLite-only mu
- [ ] Paket sürümlerini hizala (Data EFCore 9.0.11 + `Sqlite.Core 10.0.2`; UI `EFCore.Design 9.0.12`; `SQLitePCLRaw` 2.1.12 vs 3.53.3); SQLite paketlerini tek noktaya topla; server paketleri ekle

### D2 — Provider soyutlaması (Data) ⬜
- [ ] `IDatabaseProvider` + `DbContextOptions` factory (tek `UseSqlite` noktası)
- [ ] PRAGMA kancasını interceptor'a taşı (`SistemDbContext` ctor'dan çıkar)
- [ ] `IConnectionFactory` (Sistem+Tenant); `ITenantSQLite*` adlarını provider-nötr yap

### D3 — Tenant depolama soyutlaması ⬜
- [ ] `ITenantStore` (exists/create/delete/checkpoint/scan); dosya-başına-tenant varsayımını izole et
- [ ] Kimlik damgası kontrolünü `File.Exists`'ten bağımsızlaştır

### D4 — Migration seti (provider başına) ⬜
- [ ] `MigrationsAssembly` + `Migrations/Sqlite|PostgreSql|SqlServer`; mevcutlar SQLite seti; diğerleri yeniden üretilir

### D5 — Diagnostics + yedek stratejisi ⬜
- [ ] `IDatabaseDiagnostics` (integrity/tablo/row)
- [ ] `IBackupStrategy` (SQLite: kopya+WAL; server: native backup/restore)

### D6 — Ayar/UI ⬜
- [ ] Ayar modeline `Provider` + bağlantı alanları; Denetim → Veritabanı'nda seçim + bağlantı testi; varsayılan SQLite

### D7 — Test altyapısı ⬜
- [ ] Provider-agnostic fixture (SQLite temp / PostgreSQL testcontainer / SQL Server LocalDB); mimari teste provider sızıntı denetimi

### D8 — Doğrulama + doküman ⬜
- [ ] build 0/0 + test + Kural 18 canlı (SQLite varsayılan; opsiyonel server smoke) + dokümanlar

**Kapı:** D1 kararları (özellikle tenant modeli) netleşmeden D2+ kodlanmaz.

---

## Görev devri — diğer model (Oturum 280)
> **Çakışma kuralı:** bu görevler `Data`/`Business`/test dosyalarında; **`Views/**`, `ViewModels/**`, DI kayıtları, `docs/yardim/*.md` bu oturumda bende — dokunulmaz.**
> **Sonuç (Oturum 281):** A ✅ (aşağıda) · B araştırma + yaklaşım onayı ✅ (kod yok — kod onayı ayrı).

### A) 6.93 motor fix (blokaj — canlı S2-S4'ü açıyor)
- **Belirti (canlı):** AI panelinde soru → "Asistan hazır değil. Önce HazirlaAsync çağırın."; dizin `60 madde • yalnız anahtar kelime` (semantik indeks hiç oluşmuyor).
- **Kök neden 1 (sözleşme ihlali):** `YardimBilgiTabaniService.cs:142-149` — `modelIndirmeyeIzin:true` iken `OnbellekteMiAsync` ile kapı kapatılıyor; model önbellekte değilse **indirilmiyor**. Sözleşme (`docs/YARDIM-DB-PLAN.md`, `IYardimVektorUretici` notu): `true` → indir/yükle; `OnbellekteMiAsync` yalnız `false` (6.91-G) kapısıdır.
- **Kök neden 2 (doğrulanacak):** `FoundryYardimVektorUretici` + `FoundryAsistanSohbetService` ayrı `_yoneticiOlustu` bayraklarıyla `FoundryLocalManager.CreateAsync` çağırıyor; ikinci çağrı atıyor olabilir (277'de tek kullanıcı varken çalışıyordu). SDK `CreateAsync` ikinci çağrı davranışını doğrula/güvene al.
- **Kapsam:** `Business/Services|Contracts/SistemServices/AiAsistan` + `MuhasibPro/Services/AiAsistan` + testler. Arayüz/DTO **değişmez**.
- **Test:** "önbellek yok → `UretAsync` çağrılmaz" davranışını doğrulayan test varsa düzelt (sözleşme: `true` iken çağrılır, `false` iken çağrılmaz); KB sonrası chat hazırlığının başarılı olduğunu doğrulayan test ekle. `Yardim*|Rrf` yeşil + build 0 hata.
- **Teslim sonrası bende:** canlı S2/S3/S4 + onay.
- **✅ Sonuç (Oturum 281):** kapı restructure edildi (`true`→direkt `Uret`, `false`→`OnbellekteMi` kapısı) + `FoundryYoneticiKurulum` tek kapı (ikiz bayraklar silindi) + testler güncellendi (yeni: `Hazirla_IzinTrue_OnbellekYoksa_IndiripGomer`). Build 0 + `Yardim*|Rrf` 24/24 + full **634/634**. Sende: canlı S2/S3/S4.

### B) Seed yönetici yetki bug'ı (DURUM açık ucu — bağımsız)
- **Belirti:** `PermissionService` yöneticide bile tüm yetkileri `false` döner (AI paneli kilitli); canlı testte ancak dev DB'ye **geçici** KFR + `RolPermission(2300)` satırı eklenerek açılabiliyor.
- **Kök neden (koddan doğrulandı):** (1) hiçbir kod yolu `KullaniciFirmaRol` yazmıyor (`PermissionService.cs:47-49` KFR yoksa `RolPermission`'a bakmadan `false`); (2) `RolPermission` hiç seed edilmiyor (production'da `new RolPermission`/`AddAsync` 0 caller) → `:51-54` yine boş; (3) `AuthenticationService.cs:181-194` bootstrap'ı yalnız görüntü modeline sentetik `Rol=Yönetici` yazar; PermissionService bunu okumaz. Seed yalnız `Kullanici` + 2 `KullaniciRol` üretir (`SistemDbContext.cs:76-94`, `SeedDataKullaniciRol.cs:9-33`); firma seed'i de yok.
- **Kapsam:** `Libraries/MuhasibPro.Data` + `Libraries/MuhasibPro.Business` + `MuhasibPro.Tests`. **DI/View/ViewModel yasak.**
- **Sıra (Kural 13/14/15):** araştır (rol→izin tohumlama + firma oluşturmada rol atama deseni; `REFERANSLAR`'a yaz) → yaklaşımı (seed migration / idempotent onarım / firma-oluşturmada KFR) sun, onay al, sonra kodla (migration = kritik yapı → Kural 8). Mevcut DB'ler için **idempotent backfill** şart.
- **Test:** gerçek seed/DB ile integration testi (mevcut `PolitikaTests` mock'lu olduğu için bug'ı yakalamıyor) + firma→KFR + yönetici izni.
- **Not (küçük borç):** `Permission.cs:5` yorumu "eşleme Global.db'de" diyor; tablo `SistemDbContext` (Sistem.db) içinde → yorum düzeltilmeli.
- **✅ Sonuç (Oturum 281):** araştırma yapıldı (`REFERANSLAR` 281, 2 satır) + **yaklaşım onaylandı (kullanıcı): 3 katman** — (1) migration'da statik `RolPermission` seed, (2) firma oluşturmada oluşturana Yönetici KFR (idempotent), (3) açılışta idempotent backfill (**kural: Yöneticisiz firmaya seed yönetici atanır**). Kod yok — migration (Kural 8) + backfill + integration testi için kod onayı ayrı.
