# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · ilgili faz planı (`docs/CEKIRDEK-MODUL-PLAN.md`, `docs/YARDIM-DB-PLAN.md`).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).

---

## ⭐ SIRADAKİ ADIM (yeni oturum buradan başlar)
0. ✅ **Kullanıcı Yönetimi çökmesi ÇÖZÜLDÜ (Oturum 291):** kök neden `FormComboBox.UpdateVisualState` NRE (özel şablon kaldırılınca `Background` parçası null) → null-güvenli fix; canlıda iki sekme de açılıyor. Detay: `HATALAR` 291.
0b. 🔨 **AI asistan — Faz 6.97 "AI Yardımcı Modülü Geliştirme" (TEK SAHİP: ana model).** Yardımcı-model dönemi **kapandı** (Oturum 293); arşiv `docs/Arsiv/YARDIMCI-MODEL-ARSIV.md`, plan `docs/AI-ASISTAN-MODUL-PLAN.md`. **Bitenler:** S1 (model sabit) · S2 (temperature=0 + tohum) · D2/D3 (donanım servisi + test) · **A1 (Oturum 293):** prompt sertleştirme + ilgi eşiği (lexical<3 / vektör<0.35) + madde kırpma; +2 test → **687/687**. **SIRADAKİ:** **A2** (RRF ağırlık/K/top-k + offline değerlendirme seti "soru→beklenen madde", S4) → **A3/A4** (D2/D3 **DI kaydı** + D4 panel uyarı bandı + D5 Denetim "Bu bilgisayar") → **A5/A6** (S3/S4) → **A7** model araştırması (gerekirse). **A8:** 6.94 kilit senaryosu canlı. Not: model tabanı `qwen2.5-0.5b` zayıf; iyileştirme prompt+retrieval ile kovalanıyor.
1. ✅ **6.85 K4 — izin kapıları (Oturum 293).** K1 ✅🧪 + K2 ✅🧪 + K2-REDESIGN ✅ + K3 ✅. **K4 ✅ (kod):** firma-bağımsız `KullaniciYetkisiVarMiAsync` · `MainShell` navbar yalnız gerçek hedefler (Kural 21) + yetkisiz **gizli** · Denetim bölüm kapıları · FirmaShell "Firma Yönetimi" kapısı · yıkıcı guard'lar: **servis** (Firma sil→`Firma_Yonet`, Kullanıcı sil→`Kullanici_Yonet`, **mali dönem sil→`MaliDonem_Yonet`**) + **VM** (yedekten geri yükle→`Veritabani_GeriYukle`, arşiv/kapat→`MaliDonem_Yonet`, yedek sil→`Veritabani_Sil`, log sil→`Log_Sil`). Build 0 + **685/685**. **Kalan (küçük):** yıkıcı butonların görsel `IsEnabled` pasif + tooltip gerekçesi. **Devir:** `IModuleLicenseService` kapısı + hardcoded `firmaId` → **6.95** (View nav'a kayıtlı değil). → **K5 → K6**. Plan: `docs/KULLANICI-YONETIMI-PLAN.md`; detay `LOG-261-280` Oturum 293.
2. **6.95 — Aktivasyon & Modül Kilidi (KEY) + İlk Giriş: PLAN HAZIR + KARARLAR ✅ (Oturum 284).** Akış: Splash → Login (üretici atadığı ilk kullanıcı) → **zorunlu şifre değişikliği** → **Modül Seçici** → **KEY** → onaylı key sonrası **Sistem.db modül erişimi** → FirmaShell. **Kararlar:** lisans kapsamı **kurulum-geneli (global)** · KEY **çevrimdışı imza/checksum** · deneme **anahtarsız 30 gün + kısıtlı set** · ek modül **tüm açık dönemler + erişimde onay/inline göç** · **tenant DB etkin modül şemasıyla**. Plan: `docs/AKTIVASYON-MODUL-PLAN.md` (A1-A6).
3. **6.96 — Çoklu DB Provider Desteği (SQLite varsayılan): PLAN HAZIR (Oturum 284).** Karar: **SQLite varsayılan kalır + PostgreSQL/SQL Server** eklenebilir (provider-soyut Data katmanı). Kod haritası: PRAGMA/`UseSqlite`/ham ADO/dosya-başına-tenant/migration tipleri/yedek=dosya-kopya. Plan: `docs/DB-PROVIDER-PLAN.md` (D1-D8). **Açık kararlar:** sunucu tenant modeli (ayrı catalog vs TenantId) · bağlantı saklama · yedek anlayışı · `AsistanBilgi.db` SQLite-only mu · hangi sağlayıcılar. Öneri: 6.85/6.95 sonrası D1-D3.
4. **Hızlı kapatılabilir eksikler (onay/canlı):** 6.93 Adım 3 onayı (iş bitti, 640/640) · 6.90 uçtan uca canlı (Setup→pack/upload→update) · 6.91-F (UX + dev-mode Tanılama) · 6.69/6.70 "canlı (kullanıcıda)" maddeleri. **6.92 Adım 7 ✅ kapandı (kullanıcı onayı, Oturum 291).**
5. ✅ **6.94 — Tek Yardım Yüzeyi (AI Asistanı):** **H1 ✅** (AGENTS Kural 13 + YARDIM-DB-PLAN v1.2 + REFERANSLAR) · **H2 ✅** (`docs/yardim` 14 sayfa/119 madde) · **H3 ✅** (10 view `?` + 8 VM üye + `YardimDialog`/`YardimMaddesi`/`YardimMaddesiDto`/`ShowYardimAsync` silindi; **F1** → asistan) · **H5 ✅ kısmi** (build 0 + 687/687; **canlı:** `?` yok · F1 → asistan paneli `ot298_*`). **Kalan:** kilit senaryosu canlı + kullanıcı onayı. Motor işi → **Faz 6.97**.
6. **Aktif yarım fazlar:** 6.72 **Dalga 1-3** + mühür hükmü · 6.74 (log altyapısı/eşleme) · 6.75 (Fluent: Login→FirmaShell) · 6.76 (kalan smoke/Fluent) · 6.77 canlı tur · 6.78 Adım 4 canlı tur · 6.86 Chunk-2b (progress → 6.88).
7. **Rafta (plan):** 6.88 (MaliDonem+SistemDbYonetim redesign) · 6.89 (Login hatırla/QuickLogin) · "Varsayılan firma".
8. **ÇEKİRDEK planı kalanı:** Faz 2 (M4 SystemDb + M2 Auth) · Faz 4 (M5 Tenant kısmi) · E2E canlı madde.

## 📋 Açık iş envanteri (temiz — Oturum 284)
> Kaynak: `KONTROL-LISTESI` + `ROADMAP` taraması. Kategoriler: (A) öncelikli, (B) gerçekten yarım, (C) onay/canlı bekleyen, (D) plan, (E) bayat/kapandı.
- **A) Öncelikli:** **Faz 6.97 AI Yardımcı Modülü Geliştirme** (A1 ✅ → A2-A8) + **6.85 K4 kalanı** (K5-K6: Hesabım/rol rozeti) + **6.95 Aktivasyon & Modül Kilidi** (A1-A6).
- **B) Gerçekten yarım (kod eksik):** 6.72 Dalga 1-3 · 6.74 · 6.75 (Login→FirmaShell) · 6.76 (smoke/Fluent kalan) · 6.77 canlı tur · 6.78 Adım 4 · 6.86 Chunk-2b · ÇEKİRDEK Faz 2/Faz 4/E2E.
- **C) İş bitti — onay/canlı:** 6.93 Adım 3 · 6.90 uçtan uca · 6.69/6.70 canlı · 6.91-F · **6.94 H5 kilit senaryosu canlı + onay** · K4 görsel buton pasif.
- **D) Plan (başlanmadı):** **6.96 (çoklu DB provider)** · 6.88 · 6.89. **(6.94 tamam; 6.97 açıldı.)**
- **E) Bayat/kapandı (doküman temizliği):** **6.73** kapandı (test fix, 280) · **6.83** ❌ (278) · **6.87** ❌ (278) · **"Sıradaki iş kuyruğu (Oturum 180)"** eski backlog (kapanmadı/aktarılmadı) · **6.71 kapı** maddesi · **6.93 "1. Motor"** ana satırı (alt maddeler ✅) · **yardımcı-model yapısı** (Oturum 293 arşivlendi).

## ⚠️ Blokaj
- **✅ Kullanıcı Yönetimi çökmesi kapandı (Oturum 291):** `FormComboBox.UpdateVisualState` NRE (özel şablon kaldırılınca `Background` parçası null) → null-güvenli fix; canlıda iki sekme de açılıyor. Detay: `HATALAR` 291. Ayrıca "Roller & İzinler" yeniden tasarlandı (K3, onaylı).
- **Kural 8 ✅ ONAYLI (Oturum 284):** 6.85 K1 (RBAC seed + KFR + backfill) **✅ kodlandı (Oturum 285)**; `Kullanici.SifreDegistirmeliMi` migration'ı (6.95 A1) hâlâ onaylı ve bekliyor. **K2+ (View/VM) için sınıf bazlı Kural 8 onayı gerekir.**
- **Kararlaşanlar (Oturum 284):** firma erişimi = **KFR**; firma oluşturan/giriş yapan sahip = o firmanın **Yöneticisi**; mevcut/dev firmalar **önemsiz** (sıfırlanıp yeniden oluşturulur); **rol matrisi = iki rol** (`Yönetici` tüm izinler + `Kullanıcı` düzenlenebilir set; özel rol yok). **K1 tamam → sıra K2-K6.**
- **6.95 kararları ✅ (Oturum 284):** lisans kapsamı = **kurulum-geneli (Sistem.db global)** · KEY doğrulama = **çevrimdışı imza/checksum** · deneme = **anahtarsız 30 gün + kısıtlı modül seti** · ek modül = **tüm açık dönemler, erişimde onay + inline göç**.
- **Sahiplik (Oturum 293 — kapanış):** AI asistan işinin **tamamı tek sahipte (ana model)** — motor/veri/model + içerik + UI/DI + doğrulama. "Yardımcı model / Muse Spark" ayrımı, dosya sahipliği ve çakışma kuralı **kaldırıldı**; tarihsel kayıt `docs/Arsiv/YARDIMCI-MODEL-ARSIV.md`. Tüm AI işleri **Faz 6.97 — AI Yardımcı Modülü Geliştirme** (`docs/AI-ASISTAN-MODUL-PLAN.md`) altında. Ders: `HATALAR` 277/278/288.

## Aktif iş (özet)
- **Faz 6.97 — AI Yardımcı Modülü Geliştirme (ÖNCELİKLİ, tek sahip):** plan ✅ `docs/AI-ASISTAN-MODUL-PLAN.md` (A1-A8). **A1 ✅** prompt sertleştirme + ilgi eşiği + madde kırpma (+2 test). **Sıradaki A2** (RRF + offline değerlendirme seti/S4) → A3/A4 (D2/D3 DI + D4/D5 UI) → A5/A6 (S3/S4) → A7 model → A8 canlı. Model tabanı `qwen2.5-0.5b` (zayıf; prompt+retrieval ile kovalanıyor). Build 0 + **687/687**.
- **Faz 6.85 — Kullanıcı Yönetimi + RBAC (ÖNCELİKLİ):** K1 ✅🧪 (Oturum 285, `213c3b4`) · K2 ✅🧪 · K2-ÖN ✅🧪 (296) · **K2-REDESIGN + K3 ✅ (Oturum 286-291; K3 yeniden tasarım onaylı)** · **K4 ✅ kod (Oturum 293):** izin kapıları (menü gizle, Denetim bölüm, FirmaShell buton; yıkıcı Firma/Kullanıcı/mali dönem sil + VM geri-yükle/arşiv/yedek-sil/log-sil). **Kalan (küçük):** buton görsel pasif; modül lisans kapısı → 6.95. **K5-K6.** Build 0 + **687/687**.
- **Faz 6.95 — Aktivasyon & Modül Kilidi (KEY) + İlk Giriş:** 📋 plan ✅ + **kararlar ✅** (`docs/AKTIVASYON-MODUL-PLAN.md`, Oturum 284) — kullanıcı akışı (Login → zorunlu şifre → Modül Seçici → KEY → Sistem.db modül erişimi → FirmaShell) + sektör bulguları (`REFERANSLAR` 284) + A1-A6. Kararlar: kurulum-geneli lisans · çevrimdışı imza · 30 gün deneme · ek modül tüm açık dönemlere. A1 migration = Kural 8 ✅.
- **Faz 6.96 — Çoklu DB Provider (SQLite varsayılan):** 📋 plan ✅ (`docs/DB-PROVIDER-PLAN.md`, Oturum 284) — SQLite bağımlılık haritası (kod ref'li) + soyutlama mimarisi + D1-D8. **Açık kararlar:** sunucu tenant modeli (ayrı catalog vs TenantId) · bağlantı saklama · yedek anlayışı · hangi sağlayıcılar · `AsistanBilgi.db` SQLite-only mu.
- **Faz 6.94 — Tek Yardım Yüzeyi (AI Asistanı) ✅ (motor → 6.97):** plan ✅ (`docs/YARDIM-TEK-YUZEY-PLAN.md`). **H1 ✅ H2 ✅ H3 ✅ H5 ✅ kısmi.** View `?` ve tüm eski yardım altyapısı silindi (Kural 4); tek kaynak `docs/yardim/*.md` (14 sayfa) → `AsistanBilgi.db` → asistan; giriş **F1 + statü çubuğu**; AI yoksa yalnız kilit gerekçesi. Canlı `ot298_*`. Kalan: kilit senaryosu canlı + onay.
- **Faz 6.93 — AI Yardım Bilgi Tabanı (`AsistanBilgi.db` + Hibrit RAG):** plan/sözleşme ✅ (frozen `docs/YARDIM-DB-PLAN.md`) · içerik ✅ (`docs/yardim/*.md` 9 sayfa/60 madde) · **Adım 1 motor ✅** (279) · **Adım 2 UI/entegrasyon ✅🧪** (280) · **Adım 3 canlı S1-S4 ✅🧪 (282, ⏳ onay):** embedding indirildi + 60×1024 vektör; doğru madde cosine #2; semantic cevap; embedding'siz fallback sağlam; içerik +1→panel açılışında 61 + yeni vektör, geri al→60 + orphan 0. **Denetim "Yardım dizini" kartı** wiring bug'ı düzeltildi (`DenetimMasasiViewModel` artık KB'yi çocuk VM'lere geçiriyor; +2 test). · **6.91-G ✅🧪 (280):** saga 4. adım `AI Yardım Dizini` + 4 adımlı şerit. Build 0 + **636/636**.
- **Faz 6.91 — Product-ready güncelleme hattı:** A/B/C/D ✅ + Rev3 ✅ + E ✅ + **G ✅**. **Kalan: F** (UX + dev-mode Tanılama + yardım). Veri kökü `%AppData%\MuhasibPro`; ön-yedek `EnsureUpdateSafetyAsync`; guard `DbSchemaVersions.IsNewerThanSupported`; saga `IPostUpdateDogrulamaService` (**4 adım**) + `GuncellemeSonrasiView`.
- **Faz 6.92 — AI Yardım Asistanı (yerel Foundry) ✅ (motor → 6.97):** Adım 1-6 ✅ + UI revizyonu ✅ + Adım 5c ✅ + Adım 7 ✅. **6.92-D kalıcı model:** `qwen3-1.7b` ❌ · `qwen2.5-1.5b` ❌ · `phi-3.5-mini` ❌ · `qwen3-0.6b` ❌ · `qwen3.5-0.8b` ❌ → **varsayılan `qwen2.5-0.5b` sabit** (S1). Donanım kapısı D1-D6 planı onaylı (D2/D3 servis ✅; DI+UI → 6.97). Ders: üretici ≠ proje bilgisi (bilgi RAG'den).
- **Faz 6.90 — Velopack git güncelleme akışı:** kurulum ✅; **uçtan uca canlı test bekliyor** (6.91'e bağlı).
- **Kapanan/kapatılanlar (278):** 6.83 ❌ · 6.87 ❌ · 6.73 ✅ (test düzeltmesi 280).

## Açık kararlar (kullanıcı bekliyor)
1. **Ayarlar rozeti:** FirmaShell güncelleme bildirimi kapatılınca Ayarlar butonuna güncelleme ikonu; giriş yeri (Ayarlar içi güncelleme yüzeyi mi / Mali Dönem Yönetimi'ne yönlendirme mi).
2. **6.92 Adım 7 ✅ onaylandı (Oturum 291).**
3. **Doküman maddeleri:** (a) `docs/VIEW-BAGIMLILIK.md` dursun/arşive? (b) `KONTROL-LISTESI` kapanan yakın fazlar arşive alınsın mı?

## Son oturum (293, 2026-09-18 — S1 UI + 6.85 K4 izin kapıları )
- **Bağlam:** AI asistan servis katmanı (S2 + S1 + D2/D3) modeli sabitleyince `YapayZekaAyarlarViewModel`'i hâlâ model seçtiriyordu → 2 test kırmızı (673/675).
- **S1 UI (Kural 8 onaylı):** `YapayZekaAyarPaneli.xaml` → "Model" bölümü (TextBox + `Uygula` + `Varsayılan` + ilerleme) kaldırıldı; "Durum" içine salt-okunur **Model** kartı eklendi; disk yönetimi korundu. `YapayZekaAyarlarViewModel` → `ModelAlias` salt-okunur, `ModelAliasUygulaAsync`/`ModelAliasSifirlaAsync`/`ModelAliasUygulaCommand`/`ModelAliasSifirlaCommand`/`IsModelUygulaniyor`/`UygulamaYuzde`/`UygulamaAsamasi`/`IsYonetici` ve kullanılmayan `_auth`/ctor `auth` parametresi silindi (Kural 4); yardım maddeleri güncellendi. `DenetimMasasiViewModel` çağrısı uyarlandı.
- **Test:** `Yetkisiz_Alias_StatusError` + `AliasUygula_Onayla_Uygular` (kaldırılan davranış) silindi; `ModelAlias_Sabit_Kalir` eklendi. Build **0 hata** + **683/683** (684 − 2 + 1).
- **6.85 K4 izin kapıları (kod):** yıkıcı guard'lar tamamlandı — **servis:** `MaliDonemService.DeleteMaliDonemAsync` + `TenantSQLiteDatabaseService.DeleteTenantDatabaseAsync` → `MaliDonem_Yonet`; **VM:** `DonemYedeklerViewModel` (geri yükle→`Veritabani_GeriYukle`, yedek sil→`Veritabani_Sil`), `SistemYedekViewModel` (geri yükle), `ArsivDonemlerViewModel` (kapat/aç→`MaliDonem_Yonet`), `SistemLogListViewModel`/`SistemLogDetailsViewModel` (sil→`Log_Sil`). Gerekçe bildirimle gösterilir. +2 test → **685/685**.
- **Devir:** `IModuleLicenseService` kapısı + `ModuleLicenseViewModel` hardcoded `firmaId` → **6.95** (View nav'a kayıtlı değil, Kural 21).
- **Not (uyarı):** Test projesinde 28 nullable/analyzer uyarısı HEAD'de de mevcuttu (incremental build'de gizleniyor); S1 UI'dan bağımsız, temizlik ayrı iş.
- **6.94 H2 içerik ✅ TAM:** gerçek UI envanteri (explore, kaynak-doğrulanmış) + 9 mevcut sayfa derinleştirildi/gerçeğe çekildi (03 sahte modüller kaldırıldı; 08 sabit model) + **5 yeni sayfa** (`10-denetim-masasi`, `11-sistem-veritabani-yonetimi`, `12-guncelleme-sonrasi`, `13-diyaloglar`, `14-acilis-kurulum`) → `docs/yardim` **9 → 14 sayfa / 60 → 119 madde**. Build 0 + **685/685** (içerik testleri mock'lu). **Onay bekliyor.** Sonra H1 → H3 → H5; D2/D3 DI + D4/D5 UI → Faz 6.97. H4 motor → **Faz 6.97**.
- **Kural 21 temizliği (onaylı — View/Kural 8):** içerik envanterinde bulunan sahte yüzeyler kaldırıldı — `MainShellView` "Snapshot Yedek" (Command'siz) + statik tenant header ("WAL Aktif") + placeholder ContentFrame metinleri ("Faz B modülleri..."); `NamePasswordControl` "Şifremi Unuttum?" (işlevsiz); `MainShellViewModel.YardimMaddeleri` gerçeğe çekildi. Build 0 + 685/685.
- **6.94 H3 UI sadeleştirme (onaylı — Kural 8):** 10 view'den `?` butonu + 8 VM'den `YardimMaddeleri`/`YardimCommand`/`YardimAnahtari`/`YardimBasligi` + `YardimDialog`/`YardimMaddesi`/`YardimMaddesiDto`/`ShowYardimAsync` tümden silindi (Kural 4). **F1** → `ShellStatusBar.AsistanPaneliAc()` (statü çubuğu "Asistan" ile aynı yol; `MainShellView` `KeyboardAccelerator`). `TenantDatabaseUpdateView` ölü kod **yok**. Build 0 + 685/685. **Kalan: H5 canlı.**
- **Push:** commit'ler origin/main'e gönderildi.
- **6.94 H5 canlı (Kural 18):** uygulama açıldı → UIA: `YardimButton` **yok**, `AsistanToggleButton` **var**, "Şifremi Unuttum?" **yok**, **F1 → AI Yardım Asistanı paneli açıldı**. Kanıt `Temp/opencode/ot298_0_mainshell.png` + `ot298_1_f1_asistan.png`. **Kalan:** kilit senaryosu canlı + kullanıcı onayı.

## Son oturum (291, 2026-09-18 — Kullanıcı Yönetimi çökme fix + K3 redesign + sahiplik )
- **Çökme ÇÖZÜLDÜ:** kök neden `FormComboBox.UpdateVisualState` NRE (Oturum 287'de özel şablon kaldırılınca `GetTemplateChild("Background")` null; `OnApplyTemplate` → `FluidGrid.MeasureOverride` zincirinde patlıyordu) → null-güvenli `ZeminUygula` fix + `App`'e geçici first-chance tanısı (kaldırıldı). Build 0/0 + 665/665 + canlı (iki sekme).
- **K3 "Roller & İzinler" yeniden tasarım (kullanıcı onaylı):** konteynerli master-detail; modül başına `x/y` hapı; üstte `N/71 izin` + ilerleme + `N değişiklik var`; **Hazır ayar** presetleri (Tümünü ver / Yalnız görüntüleme / Temizle); alttaki yayılı özet kaldırıldı; `ItemTemplate` erişilebilirlik adı fix (test yakaladı). Kural 14/19 araştırması `REFERANSLAR` 291 (3 satır, ✅).
- **Kararlar (kullanıcı):** **6.92 Adım 7 ✅ kapandı** · **K4 = menüde gizle / butonda pasif+gerekçe** (standart hibrit) · **sahiplik kapatıldı (tek sahip, Oturum 293)**.
- **K4 (izin kapıları) başladı (Oturum 291):** firma-bağımsız `IPermissionService.KullaniciYetkisiVarMiAsync` + **`MainShell` navbar gerçek** (Kural 21: sahte modüller kaldırıldı; yalnız "Genel Bakış/Firma Yönetimi/Sistem Kayıtları") + yetkisiz menü öğesi **gizli** (butonlarda pasif + gerekçe) + Denetim bölüm kapıları + FirmaShell "Firma Yönetimi" kapısı + **yıkıcı işlem guard'ları (kısmi: Firma sil→`Firma_Yonet`, Kullanıcı sil→`Kullanici_Yonet`; servis katmanı)**. Build 0/0 + **675/675** + canlı `ot297_*`. **Kalan K4:** DB geri-yükle/arşiv/sil + log sil + mali dönem sil guard'ları + `IModuleLicenseService` kapısı.
- **Yeni kural:** **AGENTS Kural 21** — UI'da projede gerçekten var olmayan hiçbir modül/buton/veri gösterilmez (kullanıcı kararı, tüm proje).
- **Kalan:** 6.85 K4 kalanı → K5 → K6; 6.94 H1-H5 + H4 → Faz 6.97; 6.95/6.96.

## Yardımcı model dönemi (Oturum 275-292) — ARŞİV

Bu oturumların sahiplik/devir/çakışma yapısı ile imzalı özetleri **kaldırıldı**; tek sahip (ana model) dönemi başladı (Oturum 293). Tarihsel kayıt: `docs/Arsiv/YARDIMCI-MODEL-ARSIV.md`. AI işleri: **Faz 6.97 — AI Yardımcı Modülü Geliştirme** (`docs/AI-ASISTAN-MODUL-PLAN.md`).
## Son oturum (287, 2026-09-18 — K3 revizyonu + AÇIK çökme)
- **K3 "Roller & İzinler" yeniden tasarım** (modül listesi + aksiyon kartı + toplu ata + yetki özeti) + ComboBox beyazlaşma fix (WinUI3 varsayılan şablon). Build 0 + **665/665**.
- ⚠️ **Kullanıcı Yönetimi açılışında uygulama ÇÖKÜYOR** (`HATALAR` en üst, AÇIK). Canlı açılış testi edilemedi.

## Son oturum (286, 2026-09-17)
**Firma modülü yeni tasarım (K2-REDESIGN ön koşulu) + FirmaShell "Firma Yönetimi" (canlı):**
- **Kullanıcı kararı:** Kullanıcılar sayfası "FirmaList gibi" tasarlanacağı için önce `Views/Firmalar/*` (eski tasarım) yeni tasarıma çekildi + FirmaShell'den **"Firma Yönetimi"** butonuyla erişilir yapıldı; `Views/Firma/*` hızlı düzenleme korundu. **Kural 8 ✅.**
- **Yapılanlar:** `FirmalarView` Kural 17 (zemin→ana border→kartlar) + master-detail (liste + detay/mali dönem Pivot) + başlık/`IsBusy` ring + Yenile/Yeni Firma; `FirmalarList` kolon "Kimlik"→"Firma Kodu"; `FirmalarDetails` `GlassPanel`→`MuhasibCardStyle`; `FirmalarCard` `ElevatedCard`→`MuhasibCardStyle` (logo avatar); `FirmaMaliDonemler` başlık; `FirmaShellView` "Firma Yönetimi" butonu + `FirmaShellViewModel.FirmaYonetimiCommand`; ölü `OpenInNewView*` silindi (Kural 4).
- **Kararlar:** ham kalıcı **InfoBar yok** → sonuçlar oto-kapanan kanaldan (`INotificationService`, 6 sn); **yeni sayfada `?` yardım yok** (sayfa yardımları kaldırıldı kabulü; AGENTS Kural 13/6.94 H1 kapsam dışı).
- **Kural 14:** MS list/details + Sage 50 Company Information + QuickBooks company file → `REFERANSLAR` 286 (3 satır ✅).
- **Doğrulama:** build 0/0 · **655/655** · **Kural 18 canlı** (`ot286_*`: FirmaShell → "Firma Yönetimi" → liste `F-0001/Korkut Mermer` → satır seç → detay kartı + Pivot). **⏳ onay bekliyor.** **Açık uç:** açılışta ilk satır otomatik seçilmiyor (satır seçince gelir; `FirmaListViewModel` eski davranışı — istenirse VM fallback'i).
- **Commit yok** (onay sonrası).
- **Ek (aynı oturum, devam):** **DEV-ONLY otomatik migration** — `SplashTarget.DevMigration` + `SplashRoutingService.DevOttomatikGoc` (`IDevModeProvider` kapısı) → `SistemMigrationView`/`SistemMigrationViewModel` (motor `ApplyPendingSistemMigrationsAsync`: ön-yedek→göç→doğrulama; başarıda Login, hatada Tekrar Dene/Veritabanı Yönetimi); DI + 2 test → **657/657**. Canlı yol: dev DB'de bekleyen göç yok → üretilemedi (birim testli). Ayrıca Firma rötuşları: detay formu yalnız gerekli/güncellenebilir alanlara indirildi, `ListToolbar` New butonu etiketi düzeltildi (`PrimaryButtonStyle`→`ModernToolBarButtonStyle`), avatar `Initials`. **AGENTS Kural 20** (iş sırası) eklendi.
- **Ek (devam-2):** **K2-REDESIGN + K3 ✅** (Kullanıcı Yönetimi Firma deseni: Kural 17 + Pivot sekmeleri, liste/detay/kart, K3 izin matrisi + `IRolYetkiService` DI, avatar kalıcılığı; eski panel/VM silindi) → **664/664**. **Master-detail expander** (detay büyütme; liste kapanır) `FirmalarView`+`KullaniciYonetimiView`'de; kural `AGENTS` Kural 17 + `TASARIM-KURALLARI`. Canlı kanıt `ot287_*`; Kullanıcı Yönetimi canlı turu + onay bekliyor.

## Son oturum (285, 2026-09-17)
**Faz 6.85 K1 + K2 — RBAC temeli + Kullanıcı Yönetimi modalı (kod + canlı):**
- **K1:** `RolPermission` seed (Yönetici 76/Kullanıcı 16) + idempotent migration `RbacRolPermissionSeed`; firma oluşturana Yönetici KFR + açılış backfill (`SistemRbacBackfill`); `PermissionService` Yönetici bypass; `Permission.cs` yorum fix. Dev DB kopyasında migration doğrulandı.
- **K2 (Kural 8 onaylı):** `IKullaniciRolRepository` (Scoped) + `IKullaniciService` create/rol metotları; `KullaniciYonetimiViewModel`/`KullaniciDuzenleViewModel`/`KullaniciYonetimiView`/`KullaniciDuzenlePanel` (Kural 17); `UserInfoControl` menüsüne admin "Kullanıcı Yönetimi" girişi; DI + nav kaydı.
- **Canlı bulgu + fix (Kural 8 onaylı):** kaydetmede **FK ihlali** → kök neden `IUserRepository` Singleton + `ServiceLocator` pencere-başına DI scope (kullanıcı ve KFR farklı `SistemDbContext`'e yazılıyordu) → **`IUserRepository` Scoped** yapıldı; iki-scope testi kanıtladı (`SingletonRepo(SaveChanges=0) ScopedRepo(SaveChanges=1)`). `NavigationHelper` sessiz catch → `Debug.WriteLine` (Kural 12).
- **Doğrulama:** build **0/0**; **655/655** (K1+K2 yeni testler + DI regresyon). **Kural 18 canlı (`ot285_*`):** kullanıcı menüsü → "Kullanıcı Yönetimi" → liste (Ömer Korkut/**Yönetici**) → yeni kullanıcı → **"Kullanıcı oluşturuldu."** + liste (Test Kullanici/**Kullanıcı**). **Onay bekliyor.**
- **Kullanıcı kararı (redesign):** K2 view'i (elle yazılmış master-detail) **reddedildi**; standart `FirmaList`/`FirmaDetails` deseni isteniyor + **K3 izin matrisi aynı sayfada** + **kullanıcıya avatar/profil resmi** (firma logosu gibi). **Kural 8 sınıf onayı ✅.** K3 motor iskeleti (`RolIzinModel`/`IRolYetkiService`/`RolYetkiService`) kodlandı (DI/UI/test yok, commit yok) → **yeni context'te tamamlanacak.**
- **K1 commit:** `bcffa7b`. **K2 commit:** `213c3b4`. **K2-REDESIGN + K3 iskeleti:** commit'lenmedi.
- Detay: `docs/LOG/LOG-261-280.md` Oturum 285 (+ DEVİR).

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
**Faz 6.93 Adım 3 canlı S2-S4 + Denetim kartı fix + sohbet paneli durum/tema:**
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
- **Beklenen test:** **687/687** (Oturum 293; blokajsız yeşil).
- **Canlı test dersi:** Türkçe metni betikte literal yazma (BOM'suz `.ps1` → PS 5.1 ANSI); ContentDialog butonları `PrimaryButton`/`SecondaryButton` AutomationId ile (`HATALAR` 277/278). Model hazırlığı başarısızsa artık hata bandında gerçek sebep görünür (`HATALAR` 280).

## Son commit'ler
`825f078` (280) → `7979a7e` (281-284) → `bcffa7b` (285 K1) → `213c3b4` (285 K2) → **`74391fc`** (286-293 birikimi + S1 UI) → **`bd8d164`** (K4 guard'lar) → **`09e4e7a`+`33939d3`** (6.94 H2) → **`f938b0a`** (Kural 21 temizliği) → **`b49b573`** (H1) → **`8ed3da8`** (H3) → **`bf36572`** (H5 canlı) → **bu oturum (293 devam): H4 A1 + yardımcı-model kapatma + Faz 6.97** (aşağıdaki commit).

## Bilinen açık uçlar / notlar
- **6.93 motor bug'ı:** **KAPANDI** (281 fix + 282 canlı S2-S4 ✅).
- **RBAC / Kullanıcı Yönetimi (K1-K3 ✅ → K4 🔨):** motor + kullanıcı CRUD + izin matrisi UI ✅ (canlı). **K4 kısmi ✅:** firma-bağımsız `KullaniciYetkisiVarMiAsync` · navbar gerçek+gizle (Kural 21) · Denetim bölüm kapıları · FirmaShell "Firma Yönetimi" · yıkıcı Firma/Kullanıcı sil (servis). **Kalan K4:** DB geri-yükle/arşiv/sil + log sil + mali dönem sil guard'ları + `IModuleLicenseService` kapısı (`ModuleLicenseViewModel` hardcoded `firmaId`). **K5:** `UserInfoControl` rol rozeti hardcoded "Admin" → gerçek + "Hesabım". Plan: `docs/KULLANICI-YONETIMI-PLAN.md`.
- **6.91-F kalanı:** UX ince işi + dev-mode Tanılama + yardım; `App.VelopackYenidenBaslatildi` bayrağı değerlendirilecek.
- **6.91-C UI ince işi (6.88'e devir):** `SistemDbYonetimView` gelecek şemada hâlâ "Geçerli/Güncel" gösteriyor + "Giriş Ekranına Devam Et" görünür (giriş `DbIsReady=false`).
- **`GetCurrentDatabaseVersionAsync` public fallback'i:** yeni guard'lar `ReadStoredSistemVersionAsync`'i kullanmalı (özyineleme dersi — `HATALAR` 274).
- **Bildirimler (6.86):** OS toast yok; in-app InfoBar yalnız `MainShellView` + `ShellView` host'larında, **pencere başına ayrı** (`IInAppMessageService` Scoped). `FirmaShellView`'de host yok.
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25) + `docs/Arsiv/HATALAR-ARSIV.md`. **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` + `docs/Arsiv/KARAR-LOGU-ARSIV.md`.
