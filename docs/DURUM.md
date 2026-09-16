# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · ilgili faz planı (`docs/CEKIRDEK-MODUL-PLAN.md`, `docs/YARDIM-DB-PLAN.md`).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).

---

## ⭐ SIRADAKİ ADIM (yeni oturum buradan başlar)
1. **Yardımcının 6.93 motor teslimini bekle** (`📨` LOG'da: `### 📨 DİĞER MODEL OKUSUN — Faz 6.93 sözleşme DONDURULDU` + v1.1 notu). **Şu an paylaşılan ağaçta build KIRIK** (yardımcının yarım işi) → teslimde önce **build 0 + test yeşil** yap, `753f4c1`'deki 6.73 testini doğrula.
2. **6.93 Adım 2 — UI/entegrasyon (bende):** asistan paneli "yardım dizini hazırlanıyor" durumu, Denetim dizin durum satırı, `GelistiriciAraclari` öz-test sayımı → `IYardimBilgiTabani`, DI, **`YardimIcerikToplayici` + `IYardimIcerikSaglayici` silme** (birlikte, Adım 2) → Kural 18 canlı S1-S4.
3. **6.91-G** (`AsistanBilgi.db` post-update tazeleme adımı) — 6.93 motoruna bağlı.
4. **6.91-F** (UX + dev-mode Tanılama + yardım) · **6.92 Adım 7** onayı · **6.90** uçtan uca canlı (Setup→pack/upload→update).
5. **Rafta (eski açık fazlar):** 6.76, 6.74, 6.78 Adım 4, 6.85, 6.72, 6.75, "Varsayılan firma".

## ⚠️ Blokaj (Oturum 278 sonu)
- Yardımcının 6.93 motor ara işi (`AsistanPromptKurucu` overload + `AsistanPromptKurucuTests` + `FoundryAsistanSohbetService` refactor) **derlemeyi kırıyor** → bu yüzden `753f4c1` (6.73 Data testi) **henüz koşulamadı**. Yardımcı `📨` verince yeşillenecek.
- **Çakışma kuralı:** yardımcı bende olan dosyalara yazmaz; ben onun yeni dosyalarına yazmam. Aynı anda aynı dosyaya yazım yok (ders: `HATALAR` 277/278).

## Aktif iş (özet)
- **Faz 6.91 — Product-ready güncelleme hattı:** A/B/C/D ✅ + **Revizyon 3 ✅** (277) + **E ✅** (278). **Kalan: F** (UX + dev-mode Tanılama + yardım) ve **G** (`AsistanBilgi.db` tazeleme, 6.93'e bağlı). Veri kökü `%AppData%\MuhasibPro`; ön-yedek `EnsureUpdateSafetyAsync`; ileri-uyumluluk guard `DbSchemaVersions.IsNewerThanSupported`; post-update saga `IPostUpdateDogrulamaService` + `GuncellemeSonrasiView`.
  - **6.91-E (278):** `TenantDatabaseUpdateView` (+VM+yardım) **silindi**; `TenantDatabaseUpdateCoordinator` motoru **inline** çalıştırır (`check → dialog → Yedek→Göç→Doğrulama(+oto-restore) → tek bildirim`); FirmaShell'e determinate ilerleme + adım rozetleri; manuel "Güncelle" butonları (Mali Dönem Yönetimi + İncele) + InfoBar aksiyonu kaldırıldı (bilgilendirici flyout); `OnTenantUpdated` aboneliği silindi. Tandem: `docs/yardim/07` yeni akışa güncellendi; `YardimIcerikToplayici` 8→7 sayfa.
- **Faz 6.92 — AI Yardım Asistanı (yerel Foundry):** Adım 1-6 ✅ + UI revizyonu ✅ + **Adım 5c servis→UI (278) ✅**. Denetim "Yapay Zeka"da **Model yönetimi** (liste/disk/`Yenile`/`Sil` onaylı) + **`Uygula`** (alias değişimi); sohbet paneli statü çubuğunda 🤖 → sağa hizalı Flyout. **Kalan: Adım 7 kapanış onayı** (build/test/canlı ✅).
- **Faz 6.93 — AI Yardım Bilgi Tabanı (`AsistanBilgi.db` + Hibrit RAG):** plan/sözleşme ✅ (frozen: `docs/YARDIM-DB-PLAN.md`; v1.1 güncelleme entegrasyonu). **İçerik ✅ (bende, `docs/yardim/*.md` 9 sayfa/60 madde).** **Motor/veri yardımcıda (WIP).** Kararlar: yalnız AI · Markdown içerik · hibrit (lexical + Foundry `qwen3-embedding-0.6b` + RRF, FTS5 yok) · ayrı `%AppData%\MuhasibPro\AsistanBilgi.db`.
- **Faz 6.90 — Velopack git güncelleme akışı:** kurulum ✅; **uçtan uca canlı test bekliyor** (6.91'e bağlı).
- **Kapanan/kapatılanlar (278):** 6.83 ❌ geçersiz (sayfa silindi) · 6.87 ❌ geçersiz · 6.73 ✅ (kod mevcuttu + Data testi eklendi). 6.86 kapandı (`2ff1a6c`).

## Açık kararlar (kullanıcı bekliyor)
1. **Ayarlar rozeti:** FirmaShell güncelleme bildirimi kapatılınca Ayarlar butonuna güncelleme ikonu; giriş yeri (Ayarlar içi güncelleme yüzeyi mi / Mali Dönem Yönetimi'ne yönlendirme mi).
2. **6.92 Adım 7 kapanışı:** onay verilsin mi (iş bitti sayılsın).
3. **Doküman maddeleri:** (a) `docs/VIEW-BAGIMLILIK.md` dursun/arşive? (b) `KONTROL-LISTESI` kapanan yakın fazlar arşive alınsın mı?

## Son oturum (278, 2026-09-16)
**Üç iş yapıldı (hepsi commit'li):**
- **6.92 Adım 5c — servis→UI bağlama** (`6d328d4`): Denetim "Model yönetimi" (`SettingsExpander` liste + disk + `Yenile` + onaylı `Sil`) + `Uygula` (alias) + sohbet panelinde alias uyuşmazlığı düzeltmesi. Build 0 + **617/617**; canlı S1/S2(gerçek sil)/S3 + restore (`ot278*`). **Canlı bug:** `SettingsExpander` kapalı → `IsExpanded="True"` (`HATALAR` 278).
- **Faz 6.93 plan/sözleşme + içerik** (`da33c04`,`04264a5`,`ced364a`,`d2f62a5`): `docs/YARDIM-DB-PLAN.md` (frozen), `REFERANSLAR` 278 (5 satır), `docs/yardim/*.md` 9 sayfa. **6.91-G** eklendi (post-update `AI Yardım Dizini` adımı; `HazirlaAsync(modelIndirmeyeIzin:false)` + `OnbellekteMiAsync`). **Cross-model derleme riski kapatıldı:** `IYardimIcerikSaglayici` silme Adım 2'ye alındı.
- **6.91-E + eski faz kapanışları** (`54fecd9`,`8158fda`,`753f4c1`): inline göç (yukarıda) + 6.83/6.87 kapatıldı + 6.73 kapatıldı (+`TenantCheckpointTests`). Build 0 + **614/614** (6.91-E sonrası); canlı normal erişim ✅ (`ot278e*`); **pending→inline yolu bu makinede üretilemedi** (3 dönem güncel) — birim testli.

> Önceki oturumlar (277 ve öncesi) için: `docs/LOG.md` indeksi + `docs/LOG/LOG-261-280.md`.

## Kapılar / komutlar
- **Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo`
- **Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build`
- **Canlı (Kural 18):** Debug exe `MuhasibPro\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\MuhasibPro.exe`; giriş `korkutomer` / `Ok241341`; **veri kökü (dev) `MuhasibPro\Databases`**; Foundry model önbelleği `C:\Users\Code\.MuhasibPro\cache\models`; UIA betikleri `C:\Users\Code\AppData\Local\Temp\opencode\`.
- **Velopack:** tag push → `.github/workflows/release.yml` (dinamik repo) → GitHub Release; test için `*-Setup.exe` ile kurmak şart. FeedUrl varsayılanı derlemede gömülü.
- **Beklenen test:** 6.91-E sonrası **614**; + 6.73 testi **3** = **617** (yardımcının 6.93 testleri eklenince artar).
- **Canlı test betikleri:** `ot278_model.ps1`, `ot278b_sil.ps1`, `ot278c_dogrula.ps1`, `ot278e_erisim.ps1`, `ot278f_pending.ps1`, `ot278g_yonetim.ps1` + eski `ot274_*`/`ot275_*`/`ot277_*`. **Ders:** Türkçe metni betikte literal yazma (BOM'suz `.ps1` → PS 5.1 ANSI); ContentDialog butonları `PrimaryButton`/`SecondaryButton` AutomationId ile tıklanır (`HATALAR` 277/278).

## Son commit'ler
`6d328d4` (6.91-D Rev3 + 6.92 + Adım 5c) → `da33c04` → `04264a5` → `ced364a` → `d2f62a5` (6.93 plan/içerik) → `54fecd9` (6.91-E) → `8158fda` (6.83/6.87 kapatma) → `753f4c1` (6.73 + test). **Uncommitted:** yardımcının 6.93 motor WIP'i (11 yeni + 5 düzenleme) — teslimde incelenip commit'lenecek.

## Bilinen açık uçlar / notlar
- **AI kritik bulgu (açık):** seed admin'de `KullaniciFirmaRol` satırı **yok** → `PermissionService` tüm yetkileri false döner (AI paneli yöneticide bile kilitli) — ayrı ele alınmalı.
- **6.91-C UI ince işi (6.88'e devir):** `SistemDbYonetimView` gelecek şemada hâlâ "Geçerli/Güncel" gösteriyor + "Giriş Ekranına Devam Et" görünür (giriş `DbIsReady=false`).
- **`App.VelopackYenidenBaslatildi` bayrağı:** saga tetikleyicisi değil (tetik `PostUpdatePending`); 6.91-F'de değerlendirilecek.
- **`GetCurrentDatabaseVersionAsync` public fallback'i:** yeni guard'lar `ReadStoredSistemVersionAsync`'i kullanmalı (özyineleme dersi — `HATALAR` 274).
- **Bildirimler (6.86):** OS toast yok; in-app InfoBar yalnız `MainShellView` + `ShellView` host'larında, **pencere başına ayrı** (`IInAppMessageService` Scoped). `FirmaShellView`'de host yok.
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25) + `docs/Arsiv/HATALAR-ARSIV.md`. **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` + `docs/Arsiv/KARAR-LOGU-ARSIV.md`.
