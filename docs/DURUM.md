# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · ilgili faz planı (`docs/CEKIRDEK-MODUL-PLAN.md`, `docs/YARDIM-DB-PLAN.md`).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).

---

## ⭐ SIRADAKİ ADIM (yeni oturum buradan başlar)
1. **6.93 motor fix (diğer modele devredildi — BLOKAJ):** canlı S2 "Asistan hazır değil" verdi. İki kök neden: (a) `HazirlaAsync(modelIndirmeyeIzin:true)` embedding'i **indirmiyor** (`YardimBilgiTabaniService.cs:142-149` `OnbellekteMiAsync` yanlış dalda — sözleşme ihlali); (b) iki servis ayrı `FoundryLocalManager.CreateAsync` çağırıyor (doğrulanacak). Brief: `docs/KONTROL-LISTESI.md` → "Görev devri A".
2. **6.93 Adım 3 canlı S2-S4 (bende, motor fix sonrası):** anlamsal soru→doğru madde, embedding'siz fallback, içerik güncelle→yeniden indeks + Denetim "Yardım dizini" kartı kanıtı + kullanıcı onayı.
3. **Seed yönetici yetki bug'ı (diğer modele devredildi — bağımsız):** `PermissionService` yöneticide bile tüm yetkileri `false` (KFR hiç yazılmıyor + `RolPermission` hiç seed edilmiyor). Brief: "Görev devri B". **Geçici önlem (dev):** dev `Sistem.db`'ye yönetici KFR + `RolPermission(2300)` **geçici** eklendi (canlı tur için; motor/6.93 bitince geri alınacak).
4. **6.91-F** (UX + dev-mode Tanılama + yardım) · **6.92 Adım 7** onayı · **6.90** uçtan uca canlı (Setup→pack/upload→update).
5. **Rafta (eski açık fazlar):** 6.76, 6.74, 6.78 Adım 4, 6.85, 6.72, 6.75, "Varsayılan firma".

## ⚠️ Blokaj
- **6.93 canlı S2-S4:** motor bug'ına bağlı (yukarıdaki devir A). Kod/test tarafı hazır.
- **Çakışma kuralı:** yardımcı `Data`/`Business`/test dosyalarında; ben `Views`/`ViewModels`/DI/docs/yardim'de. Aynı anda aynı dosyaya yazım yok (ders: `HATALAR` 277/278).

## Aktif iş (özet)
- **Faz 6.93 — AI Yardım Bilgi Tabanı (`AsistanBilgi.db` + Hibrit RAG):** plan/sözleşme ✅ (frozen `docs/YARDIM-DB-PLAN.md`) · içerik ✅ (`docs/yardim/*.md` 9 sayfa/60 madde) · **Adım 1 motor ✅** (yardımcı, 279) · **Adım 2 UI/entegrasyon ✅🧪 (280, bende):** eski RAG v1 yolu söküldü (Kural 4: `YardimIcerikToplayici`/`IYardimIcerikSaglayici`/`YardimliSayfaDto`/`MesajlariKur`/`IlgiliMaddeleriBul` silindi), `FoundryAsistanSohbetService` ctor injection, DI 3 Singleton, panel dizin durumu/ilerlemesi, Denetim `Yardım dizini` kartı, öz-test sayımı · **6.91-G ✅🧪 (280):** saga 4. adım `AI Yardım Dizini` + 4 adımlı şerit. Build 0 + **633/633**; canlı S1 ✅ / S2 ❌ (motor).
- **Faz 6.91 — Product-ready güncelleme hattı:** A/B/C/D ✅ + Rev3 ✅ + E ✅ + **G ✅**. **Kalan: F** (UX + dev-mode Tanılama + yardım). Veri kökü `%AppData%\MuhasibPro`; ön-yedek `EnsureUpdateSafetyAsync`; guard `DbSchemaVersions.IsNewerThanSupported`; saga `IPostUpdateDogrulamaService` (**4 adım**) + `GuncellemeSonrasiView`.
- **Faz 6.92 — AI Yardım Asistanı (yerel Foundry):** Adım 1-6 ✅ + UI revizyonu ✅ + Adım 5c ✅. **Kalan: Adım 7 kapanış onayı.**
- **Faz 6.90 — Velopack git güncelleme akışı:** kurulum ✅; **uçtan uca canlı test bekliyor** (6.91'e bağlı).
- **Kapanan/kapatılanlar (278):** 6.83 ❌ · 6.87 ❌ · 6.73 ✅ (test düzeltmesi 280).

## Açık kararlar (kullanıcı bekliyor)
1. **Ayarlar rozeti:** FirmaShell güncelleme bildirimi kapatılınca Ayarlar butonuna güncelleme ikonu; giriş yeri (Ayarlar içi güncelleme yüzeyi mi / Mali Dönem Yönetimi'ne yönlendirme mi).
2. **6.92 Adım 7 kapanışı:** onay verilsin mi (iş bitti sayılsın).
3. **Doküman maddeleri:** (a) `docs/VIEW-BAGIMLILIK.md` dursun/arşive? (b) `KONTROL-LISTESI` kapanan yakın fazlar arşive alınsın mı?

## Son oturum (280, 2026-09-17)
**Faz 6.93 Adım 2 + 6.91-G (bende) + iki blokaj:**
- **6.73 testi düzeltildi:** `TenantCheckpointTests` `Mode=ReadWrite` → `ReadWriteCreate` (var olmayan dosya Error 14) → **641/641**.
- **6.93 Adım 2:** eski RAG v1 yolu tamamen söküldü (Kural 4) + DI + `BilgiTabani` ctor'a + panel "Yardım dizini" durumu/ilerlemesi + Denetim `Yardım dizini` kartı + öz-test sayımı + Kural 13 metinleri + `REFERANSLAR` 278 `Durum=✅`.
- **6.91-G:** post-update saga 4. adım **`AI Yardım Dizini`** (`HazirlaAsync(modelIndirmeyeIzin:false)`, bloklamaz → Dikkat) + `GuncellemeSonrasiView` 4 adımlı şerit (150→120px) + 2 test.
- **Doğrulama:** build 0 hata + **633/633**. **Canlı (Kural 18, `ot279*`):** S1 ✅ (`Yardım dizini hazır: 60 madde`, panel açık) · S2 ❌ "Asistan hazır değil" → **motor bug'ı devredildi**.
- Detay: `docs/LOG/LOG-261-280.md` Oturum 280.

## Son oturum (279, 2026-09-16)
**6.93 motor Adım 1 teslimi (yardımcı):** sözleşme birebir + retrieval swap (o zaman nullable property + fallback) + `EmbeddingModelAlias` + 23 yeni test. Build 0 hata + `Yardim*|Rrf` 33/33. Detay: `LOG-261-280.md` Oturum 279 (`📨` teslim satırı).

> Önceki oturumlar (278 ve öncesi) için: `docs/LOG.md` indeksi + `docs/LOG/LOG-261-280.md`.

## Kapılar / komutlar
- **Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo`
- **Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build`
- **Canlı (Kural 18):** Debug exe `MuhasibPro\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\MuhasibPro.exe`; giriş `korkutomer` / `Ok241341`; **veri kökü (dev) `MuhasibPro\Databases`**; Foundry model önbelleği `C:\Users\Code\.MuhasibPro\cache\models`; UIA betikleri `C:\Users\Code\AppData\Local\Temp\opencode\` (`ot279_adim2.ps1`, `ot277f_drawer.ps1`).
- **Velopack:** tag push → `.github/workflows/release.yml` (dinamik repo) → GitHub Release; test için `*-Setup.exe` ile kurmak şart.
- **Beklenen test:** **633/633** (6.93 Adım 2 sonrası; legacy RAG testleri −10, 6.91-G +2). Blokajsız yeşil.
- **Canlı test dersi:** Türkçe metni betikte literal yazma (BOM'suz `.ps1` → PS 5.1 ANSI); ContentDialog butonları `PrimaryButton`/`SecondaryButton` AutomationId ile (`HATALAR` 277/278). Model hazırlığı başarısızsa artık hata bandında gerçek sebep görünür (`HATALAR` 280).

## Son commit'ler
`262a37b` (DURUM sadeleştirme) → **`825f078` (Oturum 280: 6.93 Adım 1 motor + Adım 2 + 6.91-G + 6.73 test düzeltmesi + dokümanlar)**. Working tree temiz (yalnız 6.93 canlı S2-S4 motor fix'i bekliyor).

## Bilinen açık uçlar / notlar
- **6.93 motor bug'ı (AÇIK, devir A):** `modelIndirmeyeIzin:true` embedding indirmiyor + olası çift `FoundryLocalManager.CreateAsync` → canlı S2-S4 bekliyor.
- **Seed yönetici yetki bug'ı (AÇIK, devir B):** `KullaniciFirmaRol` hiç yazılmıyor + `RolPermission` hiç seed edilmiyor; yöneticide bile yetkiler `false`. Dev DB'ye **geçici** satır eklendi. `Permission.cs:5` yorumu (`Global.db`) yanlış → düzeltilmeli.
- **6.91-F kalanı:** UX ince işi + dev-mode Tanılama + yardım; `App.VelopackYenidenBaslatildi` bayrağı değerlendirilecek.
- **6.91-C UI ince işi (6.88'e devir):** `SistemDbYonetimView` gelecek şemada hâlâ "Geçerli/Güncel" gösteriyor + "Giriş Ekranına Devam Et" görünür (giriş `DbIsReady=false`).
- **`GetCurrentDatabaseVersionAsync` public fallback'i:** yeni guard'lar `ReadStoredSistemVersionAsync`'i kullanmalı (özyineleme dersi — `HATALAR` 274).
- **Bildirimler (6.86):** OS toast yok; in-app InfoBar yalnız `MainShellView` + `ShellView` host'larında, **pencere başına ayrı** (`IInAppMessageService` Scoped). `FirmaShellView`'de host yok.
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25) + `docs/Arsiv/HATALAR-ARSIV.md`. **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` + `docs/Arsiv/KARAR-LOGU-ARSIV.md`.
