# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · `docs/CEKIRDEK-MODUL-PLAN.md` (faz planı).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).
> Dokumentasyon yapısı 2026-09-15 (Oturum 271) sadeleştirildi: **bu proje ana projedir;** eski referans dönemler (AI-Studio/WebToXaml/OOBE-şablonu) arşivde.

---

## Aktif iş
- **Faz 6.91 — Product-ready güncelleme hattı (ön-yedek → doğrulama → göç → verify → restore) — 🔨 A/B/C ✅ (Oturum 274)** — ayrıntı: `docs/KONTROL-LISTESI.md` Faz 6.91 + `REFERANSLAR` Oturum 274.
  - **Kullanıcı isteği:** yapmadan önce sektörel derin araştırma: uygulama nerede/nasıl güncellenecek, güncellenen uygulama nasıl doğrulanacak, Sistem.db + dönem DB'leri yeni sürümle uyumlu mu, doğrulama/restore/yedekleme product-ready.
  - **Araştırma bulguları:** paket doğrulama = bütünlük (Velopack checksum); güven kökü = **Authenticode imza**; üretim sırası **staging→doğrula→eskiyi koruyarak aktive→health-check→rollback** (fail-closed); **ileri-uyumluluk guard** (disk şema > binary → reddet, damga migration sonrası); SQLite disiplini (VACUUM INTO, restore'da bayat -wal/-shm, her yedek doğrulanır, bozuk=karantina); sektör (QB/Sage 50/100) doğrulanmış+test edilmiş yedek, göç sonrası mutabakat.
  - **⚠️ Kritik tespit (çözüldü):** veri `%LocalAppData%\MuhasibPro\Databases` idi — Velopack **uninstall'da app kökünü siler** → `%AppData%\MuhasibPro`'ya taşındı (6.91-A).
  - **Özet plan (5 adım):** (1) release hattı + imza; (2) `PrepareForUpdateAsync` gerçek sürüm-etiketli Sistem.db (+ gerekli dönem) yedeği + doğrula; (3) `OnRestarted`/`OnFirstRun` yalnız bayrak; (4) aktivasyon sonrası `IPostUpdateDogrulamaService`: dosya doğrulama → ileri-uyumluluk guard → Sistem.db migrate+verify (hata'da yedekten restore) → dönem tarama (bozuk=restore, pending=rapor) → sonuç InfoBar+log; (5) dev-mode Tanılama + yardım.
  - **TenantDatabaseUpdateView:** kullanıcı kararı (Oturum 274) — **geçersiz → SİL**; göç erişimde onay+inline ilerlemeye taşınır (ayrıntı: 6.91 karar bölümü).
  - **6.91-A ✅ (Oturum 274):** veri kökü `%LocalAppData%\MuhasibPro` → **`%AppData%\MuhasibPro`** + tek seferlik taşıma servisi (`IDataPathRelocationService`, açılışta, fail-closed) + 4 ölü path provider silindi + `release.yml` changelog. Canlı RELEASE taşıma + Login kanıtı (`ot274_reloc_login.png`).
  - **6.91-B ✅ (Oturum 274):** `EnsureUpdateSafetyAsync` (zorunlu doğrulanmış Sistem.db yedeği, fail-closed) + `UpdateSettingsModel` LastUpdate* alanları + `InstallUpdate`→`PrepareForUpdateAsync`→`ApplyWithDatabaseSync` + `OnRestarted` bayrağı. Dönem yedeği göç anında (yeni şema öncesi bilinemez). Build 0 hata, test **518/518**; canlı modül testleri **41/41** + Güncelleme sayfası.
  - **6.91-C ✅ (Oturum 274):** İleri-uyumluluk guard — `DbSchemaVersions.IsNewerThanSupported`; Sistem + dönem state/init'te disk şema > binary ise **göç/yazma yok** (fail-closed) + `DatabaseStatusResult.FutureSchema` mesajı; giriş `DbIsReady=false` ile kapalı. Test **527/527**; canlı `9.9.0` damgalı DB → `target=MigrationRequired`, göç yok. (UI ince işi 6.88'e devredildi: SistemDbYonetim "Güncel" gösterimi + devam butonu.) Sırada **6.91-D** (post-update saga).
- **Faz 6.90 — Velopack git güncelleme akışı (kurulum ✅, uçtan uca canlı test bekliyor → 6.91'e bağlı)** — Oturum 273:
  - **`release.yml` repo adresi artık SABİT DEĞİL:** `${{ github.server_url }}/${{ github.repository }}`; `dotnet publish ... -p:GuncellemeFeedUrl=<repo>`.
  - **Derleme-zamanı varsayılan kaynak:** `MuhasibPro.csproj` → `GuncellemeFeedAdresiEkle` (`BeforeTargets="GetAssemblyAttributes"`), `git config --get remote.origin.url` (veya CI property) → `AssemblyMetadata("GuncellemeFeedUrl")`; `Domain.AppGuncellemeBilgisi` okur + normalize eder.
  - **Servis:** `UpdateService.GetSettingsAsync` boş kaynakta gömülü varsayılanı döndürür. **Admin:** Denetim Masası → Güncelleme kaynak adresi + "Varsayılana sıfırla". **Dev-mode:** Geliştirici Araçları → "Güncelleme Kaynağı" kartı (düzenle + varsayılana sıfırla + "Kaynağı Doğrula").
  - **Dev-mode test entegrasyonu (Oturum 273):** "Modül Entegrasyon Testleri" (donanım POST — `IModulTestCalistirici`: 34 DI çözümü + 5 fonksiyonel probe) + "Tanılama" (sistem testleri 7 + güncelleme öz-testi + kimlik). Amaç: sorun olunca hangi modülün koptuğunu uygulama içinden görme.
  - **Test:** `AppGuncellemeBilgisiTests` (+9) + `ModulTestCalistiriciTests` (+1). Build 0 hata; **509/509**. Canlı (_ot293_*_): modül testleri **41/41**, FeedUrl gömülü varsayılan `github.com/omer-denen/muhasibPro`.
  - **Kapı:** uçtan uca canlı (kurulum Setup.exe → `vpk pack/upload` → uygulama güncellemeyi görür/indirir/uygular) + delta kanıtı + onay.
  - **Sırada (altyapı):** Velopack `OnRestarted` hook'ları (boş) → sistem.db senkronu + tenant pending tespiti; `PostUpdateDatabaseSyncAsync` bağlanmalı.
- **Faz 6.87 — Veritabanı Güncelleme sayfası:** v1 (hero + iki sütun) **kullanıcı reddetti** (Oturum 273); "referansı sil, baştan tasarım" istendi. **Oturum 274 kararı (kullanıcı onayı bekliyor): sayfa GEÇERSİZ → SİL.** Göç işi erişim anına taşınır: dönem seç → pending ise onay dialogu → motor **inline** (determinate ilerleme) → başarıda geçiş. Post-update kontrol sayfada değil, yeni `IPostUpdateDogrulamaService` sagasında. Ayrıntı/silme listesi: `KONTROL-LISTESI` Faz 6.91 "TenantDatabaseUpdateView kararı".
  - **Envanter (mevcut):** `TenantDatabaseUpdateView.xaml(.cs)` · `TenantDatabaseUpdateViewModel` (Akis yöneticisi + facade'a bölündü) · `TenantDatabaseUpdateCoordinator` · dialog.
  - **Kalacak:** göç motoru (`TenantUpdateAkisYoneticisi`) + onay dialogu + inline ilerleme + `TenantDatabaseUpdateCoordinator` (yeniden amaç).
- **Bildirim davranışı (kullanıcı isteği, bekliyor):** FirmaShell güncelleme bildirimi kapatılınca **Ayarlar butonuna güncelleme ikonu** (bir sonraki oturuma kadar). Giriş yeri (Ayarlar içi update vs Mali Dönem Yönetimi) kararı bekliyor.
- **Faz 6.86 — Durum çubuğu + `StatusMessageService` + `NotificationService` redesign/refactor** — **KAPANDI (onaylandı, commit `2ff1a6c`).**
  - Chunk-1 ✅ (Oturum 271): `ShellStatusBar` sıfırdan + servis refactor.
  - Ana pencere çubuğu + aktif bağlam ✅ onaylı (Oturum 272): firma/dönem/tenant yalnız yüklü tenant'tan, `BaglamGoster` ile yalnız MainShell'de.
  - **Chunk-2a ✅ onaylı:** in-app InfoBar hattı (`IInAppMessageService` Scoped — pencere başına ayrı), `InAppMessageHost` (MainShellView + ShellView), OS toast/CommunityToolkit/AUMID kaldırıldı, `NotificationEnabled` bağlı.
  - **Chunk-2b:** yalnız Login ringi; kalan 4 sabit ring → **6.88** (view'lar refactoring bekliyor).

## Çalışma sırası
- **6.91-A ✅ + 6.91-B ✅ + 6.91-C ✅** → **6.91-D** (post-update saga, açılış splash) → **6.91-E** (TenantDatabaseUpdateView silme + erişimde onay/inline göç) → **6.91-F** (UX + dev-mode + yardım) → **6.90 canlı** (Setup → `vpk pack/upload` → güncelle/delta) → **6.88** (MaliDonemYönetimView + SistemDbYonetimView redesign + RAF panel temizliği + progress; + 6.91-C UI ince işi) → **6.89** (LoginView "Beni hatırla" + QuickLogin).
- **Rafta:** 6.72 Dalga 1-3, 6.73, 6.74, 6.75 kalan, 6.76, 6.78 Adım 4, 6.85 (Kullanıcı Yönetimi modal), **"Varsayılan firma"** kavramı. (6.71 kapandı.)

## Açık kararlar (kullanıcı bekliyor)
1. **6.91 kararları ✅ (Oturum 274, onaylandı):** (a) veri yolu → `%AppData%\MuhasibPro`; (b) dönem taraması → açılışta splash adımı (bloklayıcı); (c) ön-yedek → **Sistem.db** (dönem yedeği göç anında — yeni migration'lar öncesi bilinemez); (d) imza yok → bütünlük (checksum) ile başla, imza sonraki tur; (e) TenantDatabaseUpdateView → **SİL** + erişimde onay/inline göç. Uygulama sırası: A veri yolu+release → B ön-yedek+hook → C ileri-uyumluluk guard → D post-update saga → E view silme/refactor → F UX+yardım. **A/B/C ✅ uygulandı (Oturum 274), D/E/F bekliyor.**
2. **Ayarlar rozeti:** FirmaShell güncelleme bildirimi kapatılınca Ayarlar butonuna güncelleme ikonu; giriş yeri (Ayarlar içi güncelleme yüzeyi **mı** / Mali Dönem Yönetimi'ne yönlendirme **mi**).
3. **Doküman maddeleri:** (a) `docs/VIEW-BAGIMLILIK.md` dursun/arşive? (b) `KONTROL-LISTESI` kapanan yakın fazlar (6.69/6.70/6.79–6.84) arşive alınsın mı?
4. **Commit durumu:** **temiz.** Oturum 274: `033eacb` (6.91-A/B/C + araştırma + docs). Önceki: `53d0999` (273 kapanış) · `af77b50` (6.90) · `fde34df` (6.87 v1 — reddedildi).

## Son oturum (274, 2026-09-16)
Güncelleme altyapısı için **derin sektörel araştırma + product-ready plan** + **6.91-A/B/C uygulandı**. Bulgular `REFERANSLAR` 274 (10 satır). **Kritik tespit:** veri app kökünde → Velopack uninstall'da siliniyordu. **6.91-A:** veri `%AppData%\MuhasibPro` + tek seferlik taşıma (fail-closed) + ölü provider temizliği + release changelog. **6.91-B:** zorunlu doğrulanmış Sistem.db ön-yedeği + `UpdateSettingsModel` LastUpdate* + `InstallUpdate`→Prepare→WithDatabaseSync + `OnRestarted` bayrağı. **6.91-C:** ileri-uyumluluk guard (Sistem + dönem; disk şema > binary → göç/yazma yok) + giriş kapısı. Build 0 hata, test **527/527**. **Kararlar:** TenantDatabaseUpdateView **SİL**, tarama açılış splash, imza sonraki tur. Detay: `KONTROL-LISTESI` Faz 6.91 + `docs/LOG/LOG-261-280.md` Oturum 274.

## Son oturum (273, 2026-09-15)
6.87 v1 redesign **kullanıcı tarafından reddedildi** ("referansı sil, baştan tasarım"). İki-katman güncelleme ayrımı netleşti (app update/Velopack ↔ tenant şema migration). **Faz 6.90 kurulumu:** `release.yml` repo adresi dinamik, derleme-zamanı gömülü varsayılan FeedUrl, admin + dev-mode kaynak düzenleme. **Dev-mode test entegrasyonu:** "Modül Entegrasyon Testleri" (donanım POST, 41/41 canlı) + "Tanılama" (sistem 7 + güncelleme öz-testi) + "Kaynağı Doğrula". **Faz 6.91 planı** (güncelleme sonrası doğrulama + kurtarma) yazıldı. Build 0 hata + test **509/509**. Detay: `docs/LOG/LOG-261-280.md` Oturum 273.

## Son önceki oturum (272, 2026-09-15)
Faz 6.86 kapandı (onaylı): ana pencere durum çubuğu + aktif bağlam; in-app InfoBar hattı + OS toast kaldırma. 6.71 kapandı; 6.88 + 6.89 açıldı. Build 0 hata + test 498/498. Detay: `docs/LOG/LOG-261-280.md` Oturum 272.

## Kapılar / komutlar
- **Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo`
- **Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build`
- **Canlı (Kural 18):** Debug exe `MuhasibPro\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\MuhasibPro.exe`; giriş `korkutomer` / `Ok241341`; veri `MuhasibPro\Databases`; UIA betikleri `C:\Users\Code\AppData\Local\Temp\opencode\` (sqlrun yardımcısı dahil).
- **Velopack:** tag push (`v1.1.4`) → `.github/workflows/release.yml` (dinamik repo) → GitHub Release; test için `*-Setup.exe` ile kurmak şart (portable exe güncelleme görmez). FeedUrl varsayılanı derlemede gömülü.
- **Beklenen test sayısı:** 527/527.
- **Canlı test betikleri (Oturum 274):** `ot274_reloc.ps1` (veri taşıma, RELEASE), `ot274b_live.ps1` (modül testleri + Güncelleme), `ot274c2_guard.ps1` (ileri-uyumluluk guard, 9.9.0 damga). Veri dış konumda sentetik kurulum + temizlik yapar.

## Bilinen açık uçlar / notlar
- **Güncelleme hattı (6.91) — A/B/C ✅, D/E/F bekliyor:** veri `%AppData%\MuhasibPro` (Roaming); ön-yedek `EnsureUpdateSafetyAsync`; ileri-uyumluluk guard `DbSchemaVersions.IsNewerThanSupported`. **Sıradaki:** 6.91-D post-update saga (`App.VelopackYenidenBaslatildi` bayrağı ile), 6.91-E TenantDatabaseUpdateView silme + erişimde onay/inline göç, 6.91-F UX.
- **6.91-C UI ince işi (6.88'e devir):** `SistemDbYonetimView` gelecek şemada hâlâ "Geçerli/Güncel" gösteriyor + "Giriş Ekranına Devam Et" görünür (giriş `DbIsReady=false` ile kilitli). 6.88 yeniden tasarımında ele alınacak.
- **`GetCurrentDatabaseVersionAsync` public fallback'i:** yeni guard'lar bunu değil, fallback'siz `ReadStoredSistemVersionAsync`'i kullanmalı (özyineleme dersi — `HATALAR` 274).
- **Bildirimler (6.86):** OS toast kaldırıldı; in-app InfoBar yalnız `MainShellView` + `ShellView` host'larında ve **pencere başına ayrı** (`IInAppMessageService` Scoped — yansımaz). `FirmaShellView`'de host yok; `FirmaShellViewModel.OnTenantUpdateAvailable` kaldırıldı (dönem güncelleme sinyali **Mali Dönem listesi** içinde — `MaliDonemListViewModel`).
- **Mimari not:** Durum çubuğu ana pencerede (`MainShellView`) ve detay pencerelerinde (`ShellView`) görünür; `FirmaShellView`'de yok. Ana pencerede modüller ileride `MainShellView`'i değiştirirse çubuk kaybolur — modüller `MainShellView.ContentFrame`'e alınırsa kalıcı olur (Faz B).
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25 kayıt) + `docs/Arsiv/HATALAR-ARSIV.md` (eski).
- **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` (Faz 0–6.67) + `docs/Arsiv/KARAR-LOGU-ARSIV.md` (eski kararlar).
