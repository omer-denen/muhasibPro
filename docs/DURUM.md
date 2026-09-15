# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · `docs/CEKIRDEK-MODUL-PLAN.md` (faz planı).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).
> Dokumentasyon yapısı 2026-09-15 (Oturum 271) sadeleştirildi: **bu proje ana projedir;** eski referans dönemler (AI-Studio/WebToXaml/OOBE-şablonu) arşivde.

---

## Aktif iş
- **Faz 6.91 — Güncelleme sonrası doğrulama + kurtarma (uçtan uca) — 📋 PLAN (kod yok; sonraki context)** — ayrıntı: `docs/KONTROL-LISTESI.md` Faz 6.91 + `REFERANSLAR` Oturum 273.
  - **Kullanıcı isteği:** güncelleme sonrası uygulama dosyaları + Sistem.db + MaliDönem (tenant) DB doğrulama, kurtarma/restore — uçtan uca.
  - **Özet plan:** (1) pre-update gerçek Sistem.db yedeği + sürüm damgası; (2) `OnRestarted`/`OnFirstRun` yalnız bayrak; (3) aktivasyon sonrası async `IPostUpdateDogrulamaService`: dosya doğrulama → Sistem.db migrate+verify (hata'da yedekten restore) → tenant tarama (bozuk=son yedekten restore, pending=rapor, oto-migrate yok) → sonuç InfoBar+log; (4) dev-mode Tanılama'da görünür.
- **Faz 6.90 — Velopack git güncelleme akışı (kurulum ✅, uçtan uca canlı test bekliyor → 6.91'e bağlı)** — Oturum 273:
  - **`release.yml` repo adresi artık SABİT DEĞİL:** `${{ github.server_url }}/${{ github.repository }}`; `dotnet publish ... -p:GuncellemeFeedUrl=<repo>`.
  - **Derleme-zamanı varsayılan kaynak:** `MuhasibPro.csproj` → `GuncellemeFeedAdresiEkle` (`BeforeTargets="GetAssemblyAttributes"`), `git config --get remote.origin.url` (veya CI property) → `AssemblyMetadata("GuncellemeFeedUrl")`; `Domain.AppGuncellemeBilgisi` okur + normalize eder.
  - **Servis:** `UpdateService.GetSettingsAsync` boş kaynakta gömülü varsayılanı döndürür. **Admin:** Denetim Masası → Güncelleme kaynak adresi + "Varsayılana sıfırla". **Dev-mode:** Geliştirici Araçları → "Güncelleme Kaynağı" kartı (düzenle + varsayılana sıfırla + "Kaynağı Doğrula").
  - **Dev-mode test entegrasyonu (Oturum 273):** "Modül Entegrasyon Testleri" (donanım POST — `IModulTestCalistirici`: 34 DI çözümü + 5 fonksiyonel probe) + "Tanılama" (sistem testleri 7 + güncelleme öz-testi + kimlik). Amaç: sorun olunca hangi modülün koptuğunu uygulama içinden görme.
  - **Test:** `AppGuncellemeBilgisiTests` (+9) + `ModulTestCalistiriciTests` (+1). Build 0 hata; **509/509**. Canlı (_ot293_*_): modül testleri **41/41**, FeedUrl gömülü varsayılan `github.com/omer-denen/muhasibPro`.
  - **Kapı:** uçtan uca canlı (kurulum Setup.exe → `vpk pack/upload` → uygulama güncellemeyi görür/indirir/uygular) + delta kanıtı + onay.
  - **Sırada (altyapı):** Velopack `OnRestarted` hook'ları (boş) → sistem.db senkronu + tenant pending tespiti; `PostUpdateDatabaseSyncAsync` bağlanmalı.
- **Faz 6.87 — Veritabanı Güncelleme sayfası redesign:** v1 (hero + iki sütun) **kullanıcı reddetti** (Oturum 273); "referansı sil, baştan tasarım" istendi. Ayrıca "tenant DB update sayfası gerekli mi, gerçek update ile ilgilenelim" sorusu açık. Karar: önce **6.90 (gerçek update)**, sonra 6.87 yeniden.
  - **Envanter (mevcut):** `TenantDatabaseUpdateView.xaml(.cs)` · `TenantDatabaseUpdateViewModel` (Akis yöneticisi + facade'a bölündü) · `TenantDatabaseUpdateCoordinator` · dialog.
  - **Açık karar:** (a) sayfa kalsın mı / kaldırılıp erişimde otomatik migration + hafif onay mı, (b) yeni tasarım yönü.
- **Bildirim davranışı (kullanıcı isteği, bekliyor):** FirmaShell güncelleme bildirimi kapatılınca **Ayarlar butonuna güncelleme ikonu** (bir sonraki oturuma kadar). Giriş yeri (Ayarlar içi update vs Mali Dönem Yönetimi) kararı bekliyor.
- **Faz 6.86 — Durum çubuğu + `StatusMessageService` + `NotificationService` redesign/refactor** — **KAPANDI (onaylandı, commit `2ff1a6c`).**
  - Chunk-1 ✅ (Oturum 271): `ShellStatusBar` sıfırdan + servis refactor.
  - Ana pencere çubuğu + aktif bağlam ✅ onaylı (Oturum 272): firma/dönem/tenant yalnız yüklü tenant'tan, `BaglamGoster` ile yalnız MainShell'de.
  - **Chunk-2a ✅ onaylı:** in-app InfoBar hattı (`IInAppMessageService` Scoped — pencere başına ayrı), `InAppMessageHost` (MainShellView + ShellView), OS toast/CommunityToolkit/AUMID kaldırıldı, `NotificationEnabled` bağlı.
  - **Chunk-2b:** yalnız Login ringi; kalan 4 sabit ring → **6.88** (view'lar refactoring bekliyor).

## Çalışma sırası
- **6.91** (güncelleme sonrası doğrulama + kurtarma — plan hazır) → **6.90 canlı** (Setup → `vpk pack/upload` → güncelle/delta) → **6.87 v2** (update sayfası: kaldır mı/kalsın mı + baştan tasarım) → **6.88** (MaliDonemYönetimView + SistemDbYonetimView redesign + RAF panel temizliği + progress) → **6.89** (LoginView "Beni hatırla" + QuickLogin).
- **Rafta:** 6.72 Dalga 1-3, 6.73, 6.74, 6.75 kalan, 6.76, 6.78 Adım 4, 6.85 (Kullanıcı Yönetimi modal), **"Varsayılan firma"** kavramı. (6.71 kapandı.)

## Açık kararlar (kullanıcı bekliyor)
1. **6.87 v2:** tenant update sayfası kalsın mı (baştan tasarım), yoksa kaldırılıp "erişimde otomatik migration + hafif onay/progress" mi?
2. **Ayarlar rozeti:** FirmaShell güncelleme bildirimi kapatılınca Ayarlar butonuna güncelleme ikonu; giriş yeri (Ayarlar içi güncelleme yüzeyi **mı** / Mali Dönem Yönetimi'ne yönlendirme **mi**).
3. **Doküman maddeleri:** (a) `docs/VIEW-BAGIMLILIK.md` dursun/arşive? (b) `KONTROL-LISTESI` kapanan yakın fazlar (6.69/6.70/6.79–6.84) arşive alınsın mı?
4. **Commit durumu:** **temiz (Oturum 273 commit edildi):** `af77b50` (6.90 Velopack + dev-mode testleri + 6.91 planı) + `fde34df` (6.87 v1 — reddedildi/geçici). Önceki: `d5310a2` + `2ff1a6c`.

## Son oturum (273, 2026-09-15)
6.87 v1 redesign **kullanıcı tarafından reddedildi** ("referansı sil, baştan tasarım"). İki-katman güncelleme ayrımı netleşti (app update/Velopack ↔ tenant şema migration). **Faz 6.90 kurulumu:** `release.yml` repo adresi dinamik, derleme-zamanı gömülü varsayılan FeedUrl, admin + dev-mode kaynak düzenleme. **Dev-mode test entegrasyonu:** "Modül Entegrasyon Testleri" (donanım POST, 41/41 canlı) + "Tanılama" (sistem 7 + güncelleme öz-testi) + "Kaynağı Doğrula". **Faz 6.91 planı** (güncelleme sonrası doğrulama + kurtarma) yazıldı. Build 0 hata + test **509/509**. Detay: `docs/LOG/LOG-261-280.md` Oturum 273.

## Son önceki oturum (272, 2026-09-15)
Faz 6.86 kapandı (onaylı): ana pencere durum çubuğu + aktif bağlam; in-app InfoBar hattı + OS toast kaldırma. 6.71 kapandı; 6.88 + 6.89 açıldı. Build 0 hata + test 498/498. Detay: `docs/LOG/LOG-261-280.md` Oturum 272.

## Kapılar / komutlar
- **Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo`
- **Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build`
- **Canlı (Kural 18):** Debug exe `MuhasibPro\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\MuhasibPro.exe`; giriş `korkutomer` / `Ok241341`; veri `MuhasibPro\Databases`; UIA betikleri `C:\Users\Code\AppData\Local\Temp\opencode\` (sqlrun yardımcısı dahil).
- **Velopack:** tag push (`v1.1.4`) → `.github/workflows/release.yml` (dinamik repo) → GitHub Release; test için `*-Setup.exe` ile kurmak şart (portable exe güncelleme görmez). FeedUrl varsayılanı derlemede gömülü.
- **Beklenen test sayısı:** 509/509.

## Bilinen açık uçlar / notlar
- **Bildirimler (6.86):** OS toast kaldırıldı; in-app InfoBar yalnız `MainShellView` + `ShellView` host'larında ve **pencere başına ayrı** (`IInAppMessageService` Scoped — yansımaz). `FirmaShellView`'de host yok; `FirmaShellViewModel.OnTenantUpdateAvailable` kaldırıldı (dönem güncelleme sinyali **Mali Dönem listesi** içinde — `MaliDonemListViewModel`).
- **Mimari not:** Durum çubuğu ana pencerede (`MainShellView`) ve detay pencerelerinde (`ShellView`) görünür; `FirmaShellView`'de yok. Ana pencerede modüller ileride `MainShellView`'i değiştirirse çubuk kaybolur — modüller `MainShellView.ContentFrame`'e alınırsa kalıcı olur (Faz B).
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25 kayıt) + `docs/Arsiv/HATALAR-ARSIV.md` (eski).
- **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` (Faz 0–6.67) + `docs/Arsiv/KARAR-LOGU-ARSIV.md` (eski kararlar).
