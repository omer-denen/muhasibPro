# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · ilgili faz planı (`docs/CEKIRDEK-MODUL-PLAN.md`, `docs/YARDIM-DB-PLAN.md`).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).

---

## ⭐ SIRADAKİ ADIM (yeni oturum buradan başlar)
1. **6.85 — Kullanıcı Yönetimi + RBAC: ÖNCELİKLİ FAZ, PLAN HAZIR (Oturum 284).** Kullanıcı bazlı yapı yarım kaldı: **hangi kullanıcı hangi modülü/alanı kullanır, nereye girebilir, yetkisiz alan nasıl engellenir** eksik. Bugün RBAC fiilen **çalışmıyor** (kod doğrulaması): `KullaniciFirmaRol` hiç yazılmıyor + `RolPermission` hiç seed edilmiyor + UI'da yetki kapısı yok (tek kullanım AI paneli). Plan: **`docs/KULLANICI-YONETIMI-PLAN.md`** (K1 seed/atama temeli → K2 kullanıcı yönetimi modal → K3 izin matrisi → K4 modül/alan kapısı → K5 Hesabım/menü → K6 doğrulama). **K1 migration = Kural 8 ✅ ONAYLI.** Sıra: **önce hızlı eksikler → 6.85 K1 → 6.95 A1/A2.**
2. **6.95 — Aktivasyon & Modül Kilidi (KEY) + İlk Giriş: PLAN HAZIR + KARARLAR ✅ (Oturum 284).** Akış: Splash → Login (üretici atadığı ilk kullanıcı) → **zorunlu şifre değişikliği** → **Modül Seçici** → **KEY** → onaylı key sonrası **Sistem.db modül erişimi** → FirmaShell. **Kararlar:** lisans kapsamı **kurulum-geneli (global)** · KEY **çevrimdışı imza/checksum** · deneme **anahtarsız 30 gün + kısıtlı set** · ek modül **tüm açık dönemler + erişimde onay/inline göç** · **tenant DB etkin modül şemasıyla**. Plan: `docs/AKTIVASYON-MODUL-PLAN.md` (A1-A6).
3. **6.96 — Çoklu DB Provider Desteği (SQLite varsayılan): PLAN HAZIR (Oturum 284).** Karar: **SQLite varsayılan kalır + PostgreSQL/SQL Server** eklenebilir (provider-soyut Data katmanı). Kod haritası: PRAGMA/`UseSqlite`/ham ADO/dosya-başına-tenant/migration tipleri/yedek=dosya-kopya. Plan: `docs/DB-PROVIDER-PLAN.md` (D1-D8). **Açık kararlar:** sunucu tenant modeli (ayrı catalog vs TenantId) · bağlantı saklama · yedek anlayışı · `AsistanBilgi.db` SQLite-only mu · hangi sağlayıcılar. Öneri: 6.85/6.95 sonrası D1-D3.
4. **Hızlı kapatılabilir eksikler (onay/canlı):** 6.93 Adım 3 onayı (iş bitti, 640/640) · 6.92 Adım 7 onayı · 6.90 uçtan uca canlı (Setup→pack/upload→update) · 6.91-F (UX + dev-mode Tanılama) · 6.69/6.70 "canlı (kullanıcıda)" maddeleri.
5. **6.94 — Tek Yardım Yüzeyi (AI Asistanı):** plan ✅ (`docs/YARDIM-TEK-YUZEY-PLAN.md`, H1-H5); **6.85 sonrası** (H2 içerik paralel yürüyebilir).
6. **Aktif yarım fazlar:** 6.72 **Dalga 1-3** + mühür hükmü · 6.74 (log altyapısı/eşleme) · 6.75 (Fluent: Login→FirmaShell) · 6.76 (kalan smoke/Fluent) · 6.77 canlı tur · 6.78 Adım 4 canlı tur · 6.86 Chunk-2b (progress → 6.88).
7. **Rafta (plan):** 6.88 (MaliDonem+SistemDbYonetim redesign) · 6.89 (Login hatırla/QuickLogin) · "Varsayılan firma".
8. **ÇEKİRDEK planı kalanı:** Faz 2 (M4 SystemDb + M2 Auth) · Faz 4 (M5 Tenant kısmi) · E2E canlı madde.

## 📋 Açık iş envanteri (temiz — Oturum 284)
> Kaynak: `KONTROL-LISTESI` + `ROADMAP` taraması. Kategoriler: (A) öncelikli, (B) gerçekten yarım, (C) onay/canlı bekleyen, (D) plan, (E) bayat/kapandı.
- **A) Öncelikli:** **6.85 Kullanıcı Yönetimi + RBAC** (K1-K6) + **6.95 Aktivasyon & Modül Kilidi (KEY) + İlk Giriş** (A1-A6) — birlikte yürür (modül erişimi = lisans seti ∩ kullanıcı izinleri).
- **B) Gerçekten yarım (kod eksik):** 6.72 Dalga 1-3 · 6.74 · 6.75 (Login→FirmaShell) · 6.76 (smoke/Fluent kalan) · 6.77 canlı tur · 6.78 Adım 4 · 6.86 Chunk-2b · ÇEKİRDEK Faz 2/Faz 4/E2E.
- **C) İş bitti — onay/canlı:** 6.93 Adım 3 · 6.92 Adım 7 · 6.90 uçtan uca · 6.69/6.70 canlı · 6.91-F.
- **D) Plan (başlanmadı):** 6.94 · **6.96 (çoklu DB provider)** · 6.88 · 6.89.
- **E) Bayat/kapandı (doküman temizliği):** **6.73** kapandı (test fix, 280) · **6.83** ❌ (278) · **6.87** ❌ (278) · **"Sıradaki iş kuyruğu (Oturum 180)"** eski backlog (kapanmadı/aktarılmadı) · **6.71 kapı** maddesi · **6.93 "1. Motor"** ana satırı (alt maddeler ✅).

## ⚠️ Blokaj
- **Kural 8 ✅ ONAYLI (Oturum 284):** 6.85 K1 (RBAC seed + KFR + backfill) **ve** `Kullanici.SifreDegistirmeliMi` migration'ı yeni context'te kodlanabilir.
- **Kararlaşanlar (Oturum 284):** firma erişimi = **KFR**; firma oluşturan/giriş yapan sahip = o firmanın **Yöneticisi**; mevcut/dev firmalar **önemsiz** (sıfırlanıp yeniden oluşturulur); **rol matrisi = iki rol** (`Yönetici` tüm izinler + `Kullanıcı` düzenlenebilir set; özel rol yok). Tek kalan: **K1 migration Kural 8 onayı**.
- **6.95 kararları ✅ (Oturum 284):** lisans kapsamı = **kurulum-geneli (Sistem.db global)** · KEY doğrulama = **çevrimdışı imza/checksum** · deneme = **anahtarsız 30 gün + kısıtlı modül seti** · ek modül = **tüm açık dönemler, erişimde onay + inline göç**.
- **Çakışma kuralı:** yardımcı `Data`/`Business`/test dosyalarında; ben `Views`/`ViewModels`/DI/docs/yardim'de. Aynı anda aynı dosyaya yazım yok (ders: `HATALAR` 277/278).

## Aktif iş (özet)
- **Faz 6.85 — Kullanıcı Yönetimi + RBAC (ÖNCELİKLİ):** 📋 plan ✅ (`docs/KULLANICI-YONETIMI-PLAN.md`, Oturum 284) — mevcut durum kod doğrulamalı + hedef mimari + K1-K6 + riskler. **Kod başlamadı** (K1 Kural 8 onayı bekliyor).
- **Faz 6.95 — Aktivasyon & Modül Kilidi (KEY) + İlk Giriş:** 📋 plan ✅ + **kararlar ✅** (`docs/AKTIVASYON-MODUL-PLAN.md`, Oturum 284) — kullanıcı akışı (Login → zorunlu şifre → Modül Seçici → KEY → Sistem.db modül erişimi → FirmaShell) + sektör bulguları (`REFERANSLAR` 284) + A1-A6. Kararlar: kurulum-geneli lisans · çevrimdışı imza · 30 gün deneme · ek modül tüm açık dönemlere. A1 migration = Kural 8 ✅.
- **Faz 6.96 — Çoklu DB Provider (SQLite varsayılan):** 📋 plan ✅ (`docs/DB-PROVIDER-PLAN.md`, Oturum 284) — SQLite bağımlılık haritası (kod ref'li) + soyutlama mimarisi + D1-D8. **Açık kararlar:** sunucu tenant modeli (ayrı catalog vs TenantId) · bağlantı saklama · yedek anlayışı · hangi sağlayıcılar · `AsistanBilgi.db` SQLite-only mu.
- **Faz 6.94 — Tek Yardım Yüzeyi (AI Asistanı):** 📋 plan ✅ (`docs/YARDIM-TEK-YUZEY-PLAN.md`, Oturum 283). View `?` yardım listeleri/dialogu kaldırılır; tek kaynak `docs/yardim/*.md` (tüm uygulama) → `AsistanBilgi.db` → asistan; giriş F1 + statü çubuğu; AI yoksa yalnız kilit gerekçesi. H1-H5 maddeleri `KONTROL-LISTESI`'nde. **6.85 sonrası** (H2 paralel).
- **Faz 6.93 — AI Yardım Bilgi Tabanı (`AsistanBilgi.db` + Hibrit RAG):** plan/sözleşme ✅ (frozen `docs/YARDIM-DB-PLAN.md`) · içerik ✅ (`docs/yardim/*.md` 9 sayfa/60 madde) · **Adım 1 motor ✅** (279) · **Adım 2 UI/entegrasyon ✅🧪** (280) · **Adım 3 canlı S1-S4 ✅🧪 (282, ⏳ onay):** embedding indirildi + 60×1024 vektör; doğru madde cosine #2; semantic cevap; embedding'siz fallback sağlam; içerik +1→panel açılışında 61 + yeni vektör, geri al→60 + orphan 0. **Denetim "Yardım dizini" kartı** wiring bug'ı düzeltildi (`DenetimMasasiViewModel` artık KB'yi çocuk VM'lere geçiriyor; +2 test). · **6.91-G ✅🧪 (280):** saga 4. adım `AI Yardım Dizini` + 4 adımlı şerit. Build 0 + **636/636**.
- **Faz 6.91 — Product-ready güncelleme hattı:** A/B/C/D ✅ + Rev3 ✅ + E ✅ + **G ✅**. **Kalan: F** (UX + dev-mode Tanılama + yardım). Veri kökü `%AppData%\MuhasibPro`; ön-yedek `EnsureUpdateSafetyAsync`; guard `DbSchemaVersions.IsNewerThanSupported`; saga `IPostUpdateDogrulamaService` (**4 adım**) + `GuncellemeSonrasiView`.
- **Faz 6.92 — AI Yardım Asistanı (yerel Foundry):** Adım 1-6 ✅ + UI revizyonu ✅ + Adım 5c ✅. **Kalan: Adım 7 kapanış onayı.**
- **Faz 6.90 — Velopack git güncelleme akışı:** kurulum ✅; **uçtan uca canlı test bekliyor** (6.91'e bağlı).
- **Kapanan/kapatılanlar (278):** 6.83 ❌ · 6.87 ❌ · 6.73 ✅ (test düzeltmesi 280).

## Açık kararlar (kullanıcı bekliyor)
1. **Ayarlar rozeti:** FirmaShell güncelleme bildirimi kapatılınca Ayarlar butonuna güncelleme ikonu; giriş yeri (Ayarlar içi güncelleme yüzeyi mi / Mali Dönem Yönetimi'ne yönlendirme mi).
2. **6.92 Adım 7 kapanışı:** onay verilsin mi (iş bitti sayılsın).
3. **Doküman maddeleri:** (a) `docs/VIEW-BAGIMLILIK.md` dursun/arşive? (b) `KONTROL-LISTESI` kapanan yakın fazlar arşive alınsın mı?

## Son oturum (284, 2026-09-17)
**Temiz yapı + Kullanıcı Yönetimi/RBAC planı (kod yok):**
- **Kullanıcı talebi:** "Eksik iş kalmasın, temiz bir yapıyla devam"; **Kullanıcı Yönetimi modülü öncelikli** — kullanıcı bazlı yapı çekirdeğin sonuna bırakıldı; hangi kullanıcı hangi modülü/alanı kullanır, nereye girebilir, yetkisiz alan nasıl engellenir eksik.
- **RBAC kod doğrulaması (explore):** `KullaniciFirmaRol` yazan 0 kod yolu (`AddAsync` 0 çağıran); `RolPermission` ne seed ne runtime; `PermissionService` yönetici bypass'ı yok; `ClearCache()` prod'da 0 çağıran; UI'da `Permission.` tek kullanım (AI paneli); modül menüsü statik + "Ayarlar" kapısız; `IModuleLicenseService` nav'a bağlı değil; `KullaniciYonetimiView/VM` yok; `UserInfoControl` rol rozeti hardcoded "Admin".
- **Çıktı:** **`docs/KULLANICI-YONETIMI-PLAN.md`** (Faz 6.85 genişletildi: K1 seed/atama temeli → K6 doğrulama) + **`docs/AKTIVASYON-MODUL-PLAN.md`** (Faz 6.95: Splash → Login → zorunlu şifre → Modül Seçici → KEY → Sistem.db modül erişimi → FirmaShell; A1-A6) + DURUM **Açık iş envanteri (temiz)** + SIRADAKİ ADIM yeniden sıralandı (6.85 öncelikli) + ROADMAP Bayat satır fix (6.73/6.83/6.87) + KONTROL 6.85 güncellendi + AGENTS plan referansları.
- **Sektör araştırması:** Sage 50 / MYOB / Blackbaud / QuickBooks aktivasyon+modül kilidi + kullanıcı hakkı → `REFERANSLAR` 284 (5 satır). Kullanıcının akışı **sektör standardıyla birebir** (Blackbaud iki katman: unlock code → kullanıcı hakkı).
- **Kararlar ✅ (kullanıcı, Oturum 284 sonu):** 6.95 — lisans kapsamı **kurulum-geneli (Sistem.db global)** · KEY doğrulama **çevrimdışı imza/checksum** · deneme **anahtarsız 30 gün + kısıtlı set** · ek modül **tüm açık dönemler + erişimde onay/inline göç**. **Kural 8 ✅ ONAYLI** (K1 RBAC seed + `SifreDegistirmeliMi`). **Sıra:** önce hızlı eksikler → **6.85 K1** → 6.95 A1/A2. **Commit:** Oturum 281/282 kod değişiklikleri commit edildi (kullanıcı onayı).

## Son oturum (283, 2026-09-17)
**Faz 6.94 planı oluşturuldu (kod yok):**
- **Karar (kullanıcı):** ayrı yardım sayfası yazmanın anlamı yok → view `?` yardım listeleri/dialogu kaldırılır; **tek kaynak `docs/yardim/*.md`** (tüm uygulama) → `AsistanBilgi.db` → **tek yardım yüzeyi AI asistanı** (F1 + statü çubuğu); AI erişilemezse yalnız kilit gerekçesi; kapsam tüm yapı; aşamalı H1-H5.
- **Çıktı:** `docs/YARDIM-TEK-YUZEY-PLAN.md` (envanter: ~81 view yardım maddesi + silinecek altyapı) · `KONTROL-LISTESI` Faz 6.94 bölümü · `ROADMAP` 6.94 satırı + Karar Logu · `AGENTS` plan referansı.
- **Başlanmadı:** kullanıcının ek soruları sonrası H1 → H2 → H3 → H4 → H5.

## Son oturum (282, 2026-09-17)
**Faz 6.93 Adım 3 canlı S2-S4 + Denetim kartı fix + sohbet paneli durum/tema (ana model):**
- **Canlı (Kural 18, `ot282*`/`ot283*`):** **S1 ✅** (60 madde) · **S3 ✅** (embedding alias geçersiz → "yalnız anahtar kelime"; soruya cevap geldi, çökme yok) · **S2 ✅** (embedding 495 MB indi + 60×1024 vektör; bağımsız prob: doğru madde cosine **#2**; canlı cevap doğru maddeye dayandı) · **S4 ✅** (geçici madde +1 → panel açılışında **61 madde** + yeni vektör; geri al → **60 madde** + orphan 0).
- **Bug (bulundu+düzeltildi):** Denetim "Yardım dizini" kartı "Bilinmiyor." gösteriyordu — `DenetimMasasiViewModel` `IYardimBilgiTabani`'yı çocuk VM'lere geçmiyordu; iletildi + 2 test.
- **Sohbet paneli dürüst durum (kullanıcı tespiti — çelişki):** "Model hazır değil" yazıp cevap veriyordu. `PanelAcildiAsync` (flyout açılışında) → yüklüyse tazele, **indirilmişse ön-yükle → "Model hazır"**, değilse nötr "ilk soruda indirilecek", hata → sebep. +5 test. Canlı: açılışta kısa "hazırlanıyor" → "Model hazır".
- **Popup tema (kullanıcı isteği):** flyout → acrylic + ana-border hairline 1.5px + Padding 24; panel → Kural 17 içerik kartları; kullanıcı balonu accent tint. `REFERANSLAR` 282. Canlı onay **✅** ("çok güzel oldu").
- **Doğrulama:** build **0 hata** + **640/640** (636 + 4 yeni; test-helper `StatusMessageService`/`NotificationService` mock'u). **Dürüst açık uç:** varsayılan `qwen2.5-0.5b` cevap kalitesi zayıf (ilk soruda bağlamı kullanmayabilir; arada uydurma) → 6.92 final model seçimi.
- Detay: `docs/LOG/LOG-261-280.md` Oturum 282.

## Son oturum (281, 2026-09-17)
**Görev devri:** A) motor fix (izin kapısı + `FoundryYoneticiKurulum`) build 0 + 634/634 · B) yetki bug araştırması + 3-katman yaklaşım onayı (kod yok). Detay: `LOG-261-280.md` Oturum 281.

> Önceki oturumlar (280 ve öncesi) için: `docs/LOG.md` indeksi + `docs/LOG/LOG-261-280.md`.

## Kapılar / komutlar
- **Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo`
- **Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build`
- **Canlı (Kural 18):** Debug exe `MuhasibPro\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\MuhasibPro.exe`; giriş `korkutomer` / `Ok241341`; **veri kökü (dev) `MuhasibPro\Databases`**; Foundry model önbelleği `C:\Users\Code\.MuhasibPro\cache\models`; UIA betikleri `C:\Users\Code\AppData\Local\Temp\opencode\` (`ot279_adim2.ps1`, `ot277f_drawer.ps1`).
- **Velopack:** tag push → `.github/workflows/release.yml` (dinamik repo) → GitHub Release; test için `*-Setup.exe` ile kurmak şart.
- **Beklenen test:** **640/640** (6.93 Adım 3 + Denetim wiring + panel durum sonrası; 634/636 üzerine +4). Blokajsız yeşil.
- **Canlı test dersi:** Türkçe metni betikte literal yazma (BOM'suz `.ps1` → PS 5.1 ANSI); ContentDialog butonları `PrimaryButton`/`SecondaryButton` AutomationId ile (`HATALAR` 277/278). Model hazırlığı başarısızsa artık hata bandında gerçek sebep görünür (`HATALAR` 280).

## Son commit'ler
`262a37b` (DURUM sadeleştirme) → **`825f078` (Oturum 280)** → Oturum 281 (motor fix + yetki araştırma) / Oturum 282 (canlı S2-S4 + Denetim fix + panel durum/tema) **henüz commit edilmedi** (kullanıcı onayı sonrası).

## Bilinen açık uçlar / notlar
- **6.93 motor bug'ı:** **KAPANDI** (281 fix + 282 canlı S2-S4 ✅).
- **RBAC / Kullanıcı Yönetimi eksikleri (AÇIK → 6.85):** (1) `RolPermission` seed yok (ne `HasData` ne runtime) → izin kümesi boş; (2) `KullaniciFirmaRol` yazıcısı yok → firma erişimi `Firma.KaydedenId` ile (KFR değil); (3) seed yöneticiye KFR atanmıyor; (4) `PermissionService` yönetici bypass'ı yok + `ClearCache()` prod'da çağrılmıyor; (5) UI yetki kapısı yok (modül menüsü statik, "Ayarlar" kapısız, Denetim nav yalnız DEBUG filtreli); (6) `IModuleLicenseService` nav'a bağlı değil + `ModuleLicenseViewModel` hardcoded `firmaId=1`; (7) `KullaniciYonetimiView/VM` yok, rol rozeti hardcoded "Admin". Detay: `docs/KULLANICI-YONETIMI-PLAN.md`. Dev DB'ye **geçici** satır eklendi. Küçük borç: `Permission.cs:5` yorumu (`Global.db`) yanlış.
- **6.91-F kalanı:** UX ince işi + dev-mode Tanılama + yardım; `App.VelopackYenidenBaslatildi` bayrağı değerlendirilecek.
- **6.91-C UI ince işi (6.88'e devir):** `SistemDbYonetimView` gelecek şemada hâlâ "Geçerli/Güncel" gösteriyor + "Giriş Ekranına Devam Et" görünür (giriş `DbIsReady=false`).
- **`GetCurrentDatabaseVersionAsync` public fallback'i:** yeni guard'lar `ReadStoredSistemVersionAsync`'i kullanmalı (özyineleme dersi — `HATALAR` 274).
- **Bildirimler (6.86):** OS toast yok; in-app InfoBar yalnız `MainShellView` + `ShellView` host'larında, **pencere başına ayrı** (`IInAppMessageService` Scoped). `FirmaShellView`'de host yok.
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25) + `docs/Arsiv/HATALAR-ARSIV.md`. **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` + `docs/Arsiv/KARAR-LOGU-ARSIV.md`.
