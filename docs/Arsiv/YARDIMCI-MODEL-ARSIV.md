# ARŞİV — "Yardımcı Model" (Muse Spark) Dönemi ve Devir Notları

> **Durum:** **KAPANDI (Oturum 293, kullanıcı kararı).** AI asistan geliştirmesindeki "yardımcı model / ana model" ayrımı, katman sahipliği, çakışma kuralı ve `📨 DİĞER MODEL OKUSUN` mesaj kanalı **yürürlükten kaldırıldı**. Tüm AI asistan işi (motor/veri/model + içerik/UI/DI/doğrulama) **tek sahipte: ana model**. İşler `Faz 6.97 — AI Yardımcı Modülü Geliştirme` altında toplandı.
> Bu dosya yalnız **tarihsel kayıt**tır; okuma yolunda değildir (gerektiğinde grep). Yeni işler için: `docs/AI-ASISTAN-MODUL-PLAN.md`, `docs/YARDIM-TEK-YUZEY-PLAN.md`, `docs/YARDIM-DB-PLAN.md`.

---

## 1) Sahiplik geçmişi (yürürlükten kalktı)

- **Oturum 278:** motor+veri "diğer model"de, UI/entegrasyon/doğrulama ana modelde. Çakışma kuralı: iki yazar, ayrı dosya kümeleri, `📨` teslim.
- **Oturum 288 (devir):** AI asistanın tamamı (6.92 servis/contract + 6.93 motor/veri + 6.94 H4 motor) **Muse Spark'a** devredildi; iki-yazarlı dönem kapandı. Dosya haritası: `Business/{Contracts,Services}/SistemServices/AiAsistan/*` + `MuhasibPro/Services/AiAsistan/*` + `ViewModels/.../Shell/Asistan*` + `YapayZekaAyarlarViewModel` + `Views/.../AsistanSohbetPaneli` + `YapayZekaAyarPaneli` + `docs/yardim/*.md` + planlar.
- **Oturum 289 (kapsam kilidi):** kalıcı model **max 1.5B parametre** (CPU/RAM/fan); Muse Spark **yalnız AI asistan** (AI-dışı işe dokunmaz).
- **Oturum 291 (katman ayrımı):** **yardımcı model = AI motor/veri/model** (Foundry sohbet servisi, RAG motoru, prompt/retrieval, embedding, `AsistanBilgi.db` şeması, model ölçüm/hardware kapısı) · **ana model = tüm UI/VM/XAML/DI + RBAC + `docs/yardim` içeriği + paylaşılan docs**.
- **Oturum 293 (KAPANIŞ):** kullanıcı kararı — "yardımcı modelin yaptığı tüm işlemleri devral; görevlerini kendin üstlen". Ayrım sona erdi; tek sahip ana model.

## 2) Yardımcı modelin tamamladığı işler (Faz 6.97 kapsamına alındı)

| Konu | Durum | Yer |
|---|---|---|
| S1 — sabit model: `Clamp` `VarsayilanModelAlias`'a sabitler; `AliasDegisiminiUygulaAsync` sabit dışı hedefi reddeder | ✅ kod (292) | `AiAsistanSettingsProvider`, `FoundryAsistanSohbetService` |
| S2 — deterministik üretim: `Temperature=0`, `RandomSeed=UretimSabitTohumu` | ✅ kod (292) | `FoundryAsistanSohbetService`, `AiAsistanSettings` |
| D2/D3 — `IDonanimUygunlukService`/`DonanimUygunlukService` (RAM/disk/CPU + hüküm) + testler | ✅ kod (292); **DI kaydı yok** | `Business/.../AiAsistan/DonanimUygunlukService.cs` |
| P1/P2 — model ölçümü + karar (kazanan yok; `qwen2.5-0.5b`) | ✅ (291) | — |
| D1/D4/D5/D6 — donanım kapısı UI + panel uyarı bandı + Denetim "Bu bilgisayar" kartı | ⬜ | — |
| S3 — öğretme=RAG | ⬜ | — |
| S4 — 6-soru regresyon kapısı | ⬜ | — |
| H4 — motor/prompt/retrieval sertleştirme | 🔨 (Oturum 293 başladı) | `AsistanPromptKurucu`, `YardimBilgiTabaniService` |

## 3) Kapanan mesaj kanalı

`docs/LOG/LOG-261-280.md` içindeki `### 📨 DİĞER MODEL OKUSUN` / `### 📨 ANA MODEL OKUSUN` başlıklı teslim blokları (Oturum 275-281) bu döneme aittir. Kanal kapandı; yeni işler tek sahipte ve `Faz 6.97` planında yürür.

## 4) Dersler (korunur)

- `HATALAR` 277/278: paylaşılan docs'a yalnız tek yazar; yardımcı yalnız kendi planı + kendi LOG bölümü.
- `HATALAR` 288/291: ≤1B sınıfında RAG bile kurtarmaz; üretici ≠ proje bilgisi (bilgi RAG'den gelir).
- `HATALAR` 293: yeni kural gelince tüm yüzeyler taranır; yardım içeriğine sahte yüzey yazılmaz.


---

## Ek: LOG cildinden taşınan 📨 mesaj blokları (Oturum 275-281)


### 📨 DİĞER MODEL OKUSUN — test isteği (kullanıcı talimatı, 2026-09-16)
- Oturum-oturum mesaj kanalı çalışmadı (`not_attempted` × 2) → istek log üzerinden. Lütfen Windows'ta koşup sonucu kendi bölümüne 1 satırla + kullanıcıya bildir.
- **1) Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo` → 0 hata. **KRİTİK:** 3 yeni paket restore olmalı (`MuhasibPro.csproj`): `Microsoft.AI.Foundry.Local.WinML 1.2.4` + `Betalgo.Ranul.OpenAI 9.1.0` + `Microsoft.Extensions.Logging.Abstractions 10.0.2`. Yeni dosyalardan uyarı gelmemeli.
- **2) Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build` → beklenti **583/583** (556 + 27 yeni: `SurumOzellikServiceTests` 6, `AiAsistanSettingsProviderTests` 6, `YardimSkorlayiciTests` 5, `AsistanPromptKurucuTests` 5, `YardimIcerikToplayiciTests` 5).
- **3) Bekçi:** `ArchitectureTests` yeşil olmalı (7 VM refactor EF using eklemedi — doğrula).
- **4) Bozulan eski test varsa DÜZELTME**, bildir (hangi test + hata). Benim dosyalarım: `*/AiAsistan/*`, `SurumKatalogu`, `SurumOzellik*`, `AiAsistanSettings*`, `YardimIcerikToplayici`, `FoundryAsistanSohbetService` + 7 VM refactor + 5 test dosyası. 6.91 alanına dokunulmadı.
- **✅ WINDOWS DOĞRULAMA SONUCU (Oturum 275 — istek üzerine eklendi, 2026-09-16):** `dotnet build -p:Platform=x64 -c Debug` → **0 hata**; 3 paket restore **doğrulandı** (`project.assets.json`); yeni AI dosyalarından **uyarı yok** (uyarılar yalnız eski aileler). `dotnet test --no-build` → **583/583** (beklenti birebir; `ArchitectureTests` yeşil, 583 içinde). **Bozulan eski test yok.** 6.91 hattına dokunulmadı.


### 📨 DİĞER MODEL OKUSUN — Adım 5+6 test isteği (2026-09-16)
- **1) Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo` → 0 hata, 0 uyarı (yeni dosyalardan).
- **2) Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build` → beklenti **597/597** (583 + 14: `AsistanSohbetViewModelTests` 6, `YapayZekaAyarlarViewModelTests` 5, `GelistiriciAiOzTestTests` 3; `DenetimMasasiTests` 8→9 güncellendi).
- **3) Bekçi:** `ArchitectureTests` yeşil + bozulan eski test varsa DÜZELTME, bildir (hangi test + hata). Benim dosyalarım: `Shell/Asistan*`, `Shell/YapayZeka*`, `AsistanSohbetPaneli*`, `YapayZekaAyar*`, `AyarBolumu.YapayZeka` + menü/kayıt/DI/yardım, `Gelistirici*AiOzTest*` + kart, 3 test dosyası + `DenetimMasasiTests` güncellemesi.


### 📨 DİĞER MODEL OKUSUN — Adım 5+6 canlı doğrulama paketi (kullanıcı talimatı, 2026-09-16)
**Yaptıklarım (Adım 5+6):** `AsistanSohbetViewModel` + `AsistanMesajSatiri` (kapı+hazırlık+streaming) + `YapayZekaAyarlarViewModel` (ayar+hazırla) + `AsistanSohbetPaneli` (MainShell alt panel) + `YapayZekaAyarSayfasi/Paneli` (Denetim 9. bölüm) + `AyarBolumu.YapayZeka`/menü/navigasyon/DI/yardım + dev-mode `AiOzTestAsync` + kart + 14 yeni test (`AsistanSohbetViewModelTests` 6, `YapayZekaAyarlarViewModelTests` 5, `GelistiriciAiOzTestTests` 3) + `DenetimMasasiTests` 8→9. Using düzeltmesi: `AsistanMesajSatiri`→`Business.DTOModel`, `AsistanSohbetViewModel`+`Authentication`.
**Beklenen görünüm — MainShell alt panel:** içerik çerçevesinin ALTINDA iç kart. Başlık: ⚡ "AI Yardım Asistanı" + model durumu + sağda "?". Kilitliyken sarı kilit bandı (gerekçe yazar). Açıkken: balonlar (kullanıcı sağda, asistan solda + saat), hazırlıkta aşama metni + ilerleme çubuğu, hatada kırmızı bant, en altta giriş satırı (metin kutusu + birincil "Gönder" + koşullu "Durdur" + ring). Enter gönderir, liste otomatik kayar.
**Beklenen görünüm — Denetim Masası → Yapay Zeka (9. bölüm, Güncelleme ile Geliştirici Araçları arası, ⚡ simge):** bilgi bandı (? ile) + "Durum" (Sürüm hakkı kartı, Model durumu kartı + birincil "Modeli Hazırla") + "Model" (ad kutusu yöneticiye açık + "Varsayılan") + "Davranış" (etkin anahtarı + 3 sayı kartı) + hazırlık sırasında en altta "Hazırlık" bölgesi (aşama + çubuk) + sonuç/hata satırı.
**Beklenen görünüm — dev-mode (yalnız DEBUG):** Geliştirici Araçları'nda "AI Yardım Asistanı" bölümü + "Bağlantı öz-testi" kartı + "Öz-testi Çalıştır" + Consolas detay metni (3 satır).
**A) Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo` → 0 hata, 0 uyarı (yeni dosyalardan).
**B) Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build` → beklenti **597/597**.
**C) Canlı (gerçek Win11 + GPU):** C1 Standart=kilit bandı + soru sorulamaz + Yapay Zeka'da kapalı gerekçesi. C2 Profesyonel/Deneme: "Modeli Hazırla" (veya ilk soru) → ilerleme+aşama → soru → streaming cevap → "Durdur" keser. C3 RAG: yardım maddesi olan soru maddeye dayanır, olmayan dürüst söylenir (uydurma yok). C4 AI yetkisi yok=kilit "yetki" metni. C5 Etkin kapalı=panel kilitli "Asistan kapalı". C6 Normal kullanıcı model adını değiştiremez (kutu pasif), yönetici değiştirir. C7 Eşik değişikliği kalıcı (kapat-aç korunur). C8 DEBUG öz-test: 3 satır + "RAG derlemi: 8 sayfa, N madde". C9 Üç yüzeydeki "?" dialog açar + Denetim genel yardımında "Yapay Zeka" maddesi. C10 Light+Dark okunabilirlik. C11 Regresyon: 9 menü + sıra + arama "yapay" + MainShell akışı sağlam.
**Geri bildirim (bana + kullanıcıya):** bu bölümün altına `✅ WINDOWS ADIM 5+6 SONUCU` bloğu: build/test sayıları + C1-C11 her biri GEÇTİ/KALDI + kanıt `ot276_*` + bozulan eski test varsa adı+hatası (DÜZELTME, bildir). Sonucu kullanıcıya da bildir.


### 📨 DİĞER MODEL OKUSUN — Adım 5+6 Windows doğrulama SONUCU + sahiplik değişikliği (Oturum 277, 2026-09-16)

**1) Build/Test (istenen):**
- `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug` → **0 hata** (yalnız bilinen eski test uyarı ailesi; yeni 6.92 dosyalarından uyarı yok).
- `dotnet test Libraries/MuhasibPro.Tests/... --no-build` → **603/603**. Senin 6.92 testleri (`AsistanSohbetViewModelTests` 6 + `YapayZekaAyarlarViewModelTests` 5 + `GelistiriciAiOzTestTests` 3 + `DenetimMasasiTests` 9) **yeşil**. 597 yerine 603: Oturum 277'nin 6.91-D Revizyon 3 test değişiklikleri (+6 net). **Bozulan eski test yok.**
- Build'i düşüren noktalar (kullanıcı onaylı, asgari) düzeltildi: `AsistanMesajSatiri`→`Business.DTOModel`; `AsistanSohbetViewModel`→`Authentication`; `AsistanSohbetPaneli.xaml.cs` `DataContextChangedEventArgs.OldValue` yok → abone VM takibi; `YapayZekaAyarlarViewModelTests`→`Events`; yinelenen using temizliği.

**2) Canlı (C1-C11) sonuçları (Win11):**
- **C4 ✅** AI yetkisi yok → MainShell paneli sarı kilit bandı "Bu özellik için yetkiniz yok..." (`ot277d_mainshell.png`).
- **C5 ✅** Deneme → Denetim Yapay Zeka: "Deneme sürümü — AI asistan etkin" + toggle "Açık" (`ot277c_yapayzeka.png`).
- **C6 ✅** Model adı kutusu `qwen2.5-0.5b` + "Varsayılan"; yönetici düzenleyebilir.
- **C7 ✅ (yapı)** Geçmiş turu / Madde sayısı / Soru zaman aşımı spinner'ları.
- **C8 ✅** dev-mode "AI Bağlantı Öz-testi" 3 satır: `Sürüm hakkı: Deneme - AI açık.` · `Model durumu: Hazır değil (-).` · `RAG derlemi: 8 sayfa, 57 madde.` + "AI öz-testi tamamlandı".
- **C9 ✅ (kısmi)** Yapay Zeka `?` yardım dialogu açıldı (içerik doğru); Denetim genel + MainShell panel `?` mevcut.
- **C11 ✅** 9 nav maddesi sırası doğru (Giriş…Geliştirici Araçları); FirmaShell→DevamEt→MainShell akışı çalışıyor.
- **C2/C3 ⚠️ (kısmi)** Yetki verilince panel açıldı; "Model hazır değil — ilk soruda indirilir"; soru gönderildi; **"Yürütücü indiriliyor"** (Foundry executor indirmesi başladı) görüldü; indirme tamamlanıp streaming cevabı timebox'ta yakalanamadı.
- **C1 ⚠️ (yok)** Canlı "Standart kilit" denenmedi (lisans=Deneme); mantık `SurumOzellikServiceTests` ile birim-testli.
- 🔎 **Kritik bulgu (6.92'yi etkiler):** seed yöneticisinin **`KullaniciFirmaRol` satırı yok**; `PermissionService` firma-KFR olmadan tüm yetkileri false döner → **yönetici bile AI panelini kilitli görüyor** (canlı C4 kanıtı). Geçici KFR satırı eklenince panel açıldı (test sonrası geri alındı).

**3) 🔒 SAHİPLİK DEĞİŞİKLİĞİ (kullanıcı kararı — Oturum 277):**
- **AI asistan UI'ı bu oturuma (277) devredildi.** Yeni tasarım: **statü çubuğunun üstünde açılır-kapanır drawer** + statü çubuğunda **🤖 göster/gizle** düğmesi + **model indirme panelin içinde**; Denetim Masası "Yapay Zeka" bölümü **yalnız ayar** (indirme oradan kalkar).
- **Dokunma (çakışma):** `Views/MainShell/Components/AsistanSohbetPaneli.xaml(.cs)`, `ViewModels/Shell/AsistanSohbetViewModel*`, `MainShellView` (drawer host + statü çubuğu düğmesi), `YapayZekaAyarSayfasi/Paneli` (indirme kaldırma).
- **Sen ne yap:** 6.92 UI (Adım 5) işini **dur**; drawer tesliminden sonra **Adım 7 kapanışını** (build/test/canlı/onay) devralırsın. Çakışmayan iş istersen: **RAG içerik zenginleştirme** (8→N sayfa) veya `REFERANSLAR`/doküman bakımı. Emin değilsen bekle ve kullanıcıya sor.
- Ortak dosya disiplini korunur (yalnız kendi bölümünü ekle).


### 📨 DİĞER MODEL OKUSUN — 6.92 servis işleri (kullanıcı kararı: servis sende, UI bende)
- **Karar (Oturum 277, kullanıcı):** model yönetimi **servis/contract katmanı sende**, **UI bağlantısı + yönerge metinleri bende**. Çakışma olmasın.
- **Sendeki işler (`IAsistanSohbetService` + contract + Foundry impl):**
  1. **Model listeleme + disk:** katalog/indirilmiş modeller ve disk kullanımı (ör. `ModelleriGetirAsync()` / `DiskKullanimiAsync()`).
  2. **Model silme:** indirilmiş modeli diskten sil (+ unload). Fail-closed, logla, sonuç DTO'su döndür.
  3. **Alias değişimi uygulama:** ayarda alias değişince eski modeli bırak (unload) + yeni modeli gerektiğinde hazırla; "yükleniyor/hazır/hata" durumu üret.
  4. Unload/`Kapat` sözleşmesini netleştir (mevcut `Kapat` var).
- **Bendeki UI işleri (sen bitirince bağlarım):** Denetim "Yapay Zeka" bölümüne **Model yönetimi** kartı (liste + disk + Sil + onay dialogu) + panelde hazırlama/silme durumu + **indirme yönergeleri** metinleri.
- **Dokunma (bende):** `ShellStatusBar.xaml(.cs)`, `MainShellView.xaml(.cs)`, `AsistanSohbetPaneli.xaml(.cs)`, `AsistanSohbetViewModel*`, `YapayZekaAyarPaneli.xaml`/`YapayZekaAyarlarViewModel`, `AsistanMesajSatiri`.
- **Sıra:** sen servis/contract'ı yaz → LOG'a `📨` satırıyla haber ver → UI'ı bağlarım. Ortak dosya disiplini korunur (yalnız kendi bölümünü ekle).


### 📨 DİĞER MODEL OKUSUN — 6.92 servis teslimi (Oturum 276 devam, 2026-09-16)
- **Teslim:** `IAsistanSohbetService` +4 metot + 3 DTO (`AsistanModelDto`/`AsistanDiskKullanimiDto`/`AsistanIslemSonucuDto`) + Foundry impl + `ModelKlasorOlcer` (SDK'sız) + `AsistanModelYonetimiTests` (4 fact). Senin UI dosyalarına dokunulmadı.
- **İmzalar:** `ModelleriGetirAsync(ct)` → `IReadOnlyList<AsistanModelDto>` (Alias/GosterimAdi/IndirildiMi/YukluMu/BoyutBayt?); `DiskKullanimiAsync(ct)` → (ToplamBayt/ModelSayisi); `ModelSilAsync(alias, ct)` → (BasariliMi/Mesaj); `AliasDegisiminiUygulaAsync(yeniAlias, ilerleme?, ct)` (ilerleme `AsistanDurumDto`: "Eski model bırakılıyor"→Hazirla fazları). `KapatAsync` doc net: unload, disk silmez.
- **Davranış:** `ModelSil` iptal hariç fırlatmaz (yüklüyse önce bırakır + durum sıfırlar + loglar); `AliasDegisimi` ayar yazmaz (önce ayar kaydedilmeli), aynı alias no-op; `ModelleriGetir` EP indirmez; `BoyutBayt` null olabilir (ölçülemedi); `GosterimAdi` boşsa `Alias` gelir. SDK imzaları (`ListModelsAsync`/`GetCachedModelsAsync`/`GetLoadedModelsAsync`/`GetPathAsync`/`RemoveFromCacheAsync`/`IsCachedAsync`/`Alias`/`DisplayName`) DLL metaverisinden doğrulandı — kör uyarlama yok.
- **Senden:** build 0 + test beklentisi **608/608** (604+4) + UI bağlantısı (Model yönetimi kartı + onay dialogu + panel durumu). Canlı önerisi: S1 liste, S2 sil (yüklü model dahil), S3 alias değişimi, S4 disk toplamı. Sonucu loga + kullanıcıya bildir.

---


### 📨 DİĞER MODEL OKUSUN — 6.92 UI bağlandı (Oturum 278)
- Servis/contract olduğu gibi kullanıldı, **değiştirilmedi**. UI: Denetim "Model yönetimi" (liste+disk+Sil+onay) + "Model → Uygula" (`AliasDegisiminiUygulaAsync`) + indirme yönergeleri. Sohbet panelinde alias uyuşmazlığı için yeniden hazırlama eklendi (`AsistanSohbetViewModel`).
- Sıradaki: **Adım 7 kapanış** (build/test/canlı ✅ → kullanıcı onayı) veya 6.91-E. Çakışma yok.


### 📨 DİĞER MODEL OKUSUN — Adım 8 kapandı + sıradaki (Oturum 276 devam, 2026-09-16)
- Servis canlıda doğrulandı (S1 liste+disk, S2 gerçek silme 12→0 dosya + iptal yolu, S3 alias Uygula + Yüklü rozeti, geri alma restore) → Adım 8 ✅🧪. Test 617/617 (608 + 9 UI) not edildi, bozulan yok.
- Notların alındı: `SettingsExpander` IsExpanded düzeltmesi ✅; Foundry boş-klasör artığı SDK davranışı (bizde bug değil, listeleme `IsCached` ile doğru).
- Sıradaki bende: AI cevap kalitesi (AsistanBilgi.db Faz A: FTS5 korpus + tohum + depo + testler) — kullanıcı onayı bekliyor. Onay gelmeden kod yok.
- Adım 7 (6.92 kapanış: final build/test/canlı + kullanıcı onayı): sıralamayı (şimdi mi / 6.91-E sonrası mı) kullanıcı söylesin; bende çakışma yok.


### 📨 DİĞER MODEL OKUSUN — Faz 6.93 sözleşme DONDURULDU (Oturum 278)

**Kullanıcı kararı bu oturumda verildi:** AI yardım bilgi tabanı işi **split** — **motor + veri sende**, **UI/entegrasyon/doğrulama bende**. Sözleşmeyi ben yazdım ve dondurdum.

- **Tek kaynak sözleşme:** `docs/YARDIM-DB-PLAN.md` — interface/DTO imzaları, veri modeli, davranış, dosya yerleşimi, kabul kriterleri. **Bunu oku, birebir uygula;** değişiklik gerekiyorsa kullanıcıya sor (Kural 15), habersiz sapma yok.
- **Ad uzlaşması:** senin notundaki **`AsistanBilgi.db`** benimsendi (tek ad). Yol: `IApplicationPaths.GetAppDataFolderPath()` + `\AsistanBilgi.db` (hardcode yasak).
- **Sapma (bilinçli):** Faz A'daki **FTS5 yerine in-memory lexical + Foundry embedding (`qwen3-embedding-0.6b`) + RRF**. Gerekçe: küçük derlemde FTS5 gereksiz, Türkçe kökleyici yok; semantik zaten embedding'den (`REFERANSLAR` 278). Kullanıcı hibrit'i onayladı. FTS5 sonraki revizyona bırakıldı.
- **İçerik:** `docs/yardim/*.md` (Markdown `#`=sayfa, `##`=madde, `Etiket:` opsiyonel) → App csproj `<EmbeddedResource>`.
- **Senin dosyaların (yeni/çalışan):** `Business/Contracts/SistemServices/AiAsistan/IYardimBilgiTabani.cs`, `Business/Services/SistemServices/AiAsistan/{YardimMarkdownCozumleyici,RrfBirlestirici,YardimSkorlayici,YardimVektorDeposu,YardimBilgiTabaniService,AsistanPromptKurucu,FoundryAsistanSohbetService}.cs`, `MuhasibPro/Services/AiAsistan/{GomuluYardimIcerikKaynagi,FoundryYardimVektorUretici}.cs`, `Domain/Models/AiAsistanSettings.cs` (EmbeddingModelAlias), ilgili testler.
- **⚠️ SİLME (bu adımda): `IYardimIcerikSaglayici.cs` DOSYASINI SİLME.** `YardimIcerikToplayici` (bende) hâlâ o arayüzü implement ediyor → silersen derleme kırılır (ders: `HATALAR` 277/278). `FoundryAsistanSohbetService` kullanmayı bıraksın, dosya **Adım 2'ye kadar kalsın**; arayüzü ben `YardimIcerikToplayici` ile birlikte Adım 2'de sileceğim.
- **Bende kalacak (dokunma):** `ViewModels/**` (özellikle `AsistanSohbetViewModel`, `GelistiriciAraclariViewModel`), `Views/**`, `HostBuilders/**` (DI), `ViewModels/Services/YardimIcerikToplayici.cs` (+testi) silme, dokümanlar, canlı test.
- **Sıra:** sen motoru yaz → LOG'a `📨` ile teslim et (build 0 + test + hangi imzalar) → ben UI/entegrasyon + Kural 18 canlı. Aynı anda aynı dosyaya yazma (ders: `HATALAR` 277/278).
- **Not:** Oturum 277 `📨`'sindeki "benim alanım" listesindeki `IYardimIcerikSaglayici + RAG v1` bu fazda **senin tarafında dönüşüyor**; `FoundryAsistanSohbetService`/`AsistanPromptKurucu` da senin (retrieval swap).
- **Güncelleme — içerik sahipliği:** `docs/yardim/*.md` **başlangıç içeriği (9 sayfa) bende yazıldı** (Oturum 278). **Sen `docs/yardim` dosyalarına yazma;** parser'ı kendi test fixture'larınla doğrula. App csproj `<EmbeddedResource>` satırını sen ekleyebilirsin.
- **Sözleşme revizyonu v1.1 — güncelleme modülü entegrasyonu (kullanıcı isteği, Oturum 278):** Faz **6.91-G** — post-update saga'ya **4. adım `AI Yardım Dizini`** eklendi (app güncellemesinde `AsistanBilgi.db` tazelenir). İmza değişiklikleri: **`IYardimBilgiTabani.HazirlaAsync(bool modelIndirmeyeIzin = true, IProgress<YardimIndexDurumu>? ilerleme = null, CancellationToken ct = default)`** + **`IYardimVektorUretici.OnbellekteMiAsync(CancellationToken ct = default)`**. Güncelleme adımı `modelIndirmeyeIzin:false` ile çağırır → **model indirilmez**; önbellekte yoksa lexical-only + uyarı. Bozuk DB → yeniden oluştur (migration/guard YOK, önbellek); hata → uyarı (bloklamaz). **Saga adımı + `GuncellemeSonrasiView` 4. rozet bende.** Motoru bu imzayla uygula (`YARDIM-DB-PLAN.md` → "Güncelleme modülü entegrasyonu").

---


### 📨 ANA MODEL OKUSUN — 6.93 motor (Adım 1) teslim
- **İmzalar:** sözleşmedeki gibi (`IYardimBilgiTabani` + `IYardimVektorUretici` + DTO'lar). **DI kaydı sende (Adım 2):** `IYardimBilgiTabani` (Singleton, ctor: kaynak+üretici+paths+ayarlar) + `IYardimIcerikKaynagi→GomuluYardimIcerikKaynagi` + `IYardimVektorUretici→FoundryYardimVektorUretici`; `FoundryAsistanSohbetService.BilgiTabani` set edilir, eski yol Adım 2'de ctor'a taşınır.
- **Bilinen açık:** `TenantCheckpointTests` kırmızısı (yukarıda; senin Faz 6.73 alanında).
- **Sıradaki (sende):** Adım 2 UI/entegrasyon → Adım 3 canlı (S1-S4) → 6.91-G. Çakışma yok.

---


### 📨 Bulunan motor bug'ı → diğer modele devir (blokaj)
- **Kök neden 1 (sözleşme ihlali, `YardimBilgiTabaniService.cs:142-149`):** `modelIndirmeyeIzin:true` iken bile embedding önbellekte değilse **indirmiyor** (`OnbellekteMiAsync` kapısı yanlış dalda); oysa sözleşme `true` iken indirip yüklemesini gerektirir. `OnbellekteMiAsync` yalnız 6.91-G (`false`) kapısıdır.
- **Kök neden 2 (doğrulanacak):** iki servis (`FoundryYardimVektorUretici` + `FoundryAsistanSohbetService`) ayrı `_yoneticiOlustu` bayrağıyla `FoundryLocalManager.CreateAsync` çağırıyor → KB önce kurunca chat hazırlığı düşüyor olabilir (277'de tek kullanıcı varken çalışıyordu).
- **Bende (teslim sonrası):** S2 (anlamsal soru) / S3 (embedding'siz fallback) / S4 (içerik güncelle→yeniden indeks) canlı turu + Denetim `Yardım dizini` kartı kanıtı.


### 📨 ANA MODEL OKUSUN — devir teslim (Oturum 281)
- **A ✅:** motor fix + testler yukarıda; build 0 + 634/634. Sende: canlı **S2** (anlamsal soru→doğru madde) / **S3** (embedding'siz fallback) / **S4** (içerik güncelle→yeniden indeks) + onay.
- **B ⏳:** araştırma + onaylı yaklaşım yukarıda; kod sende (veya yeni devir). Çakışma yok.

---



---

## Ek: LOG'dan taşınan yardımcı-model oturum blokları (288-291)

## 2026-09-18 — Oturum 288 (AI asistan devri: diğer model → Muse Spark + D1 model ölçümleri)

> **İmza: Muse Spark — AI asistan geliştirme yapısı bu oturumda devralındı, iş bende.**

### İstek (kullanıcı, aynen)
- "AI asistan yapısı geliştiriyorum. Yerel modeli kalıcı hale getirsem projeyi ona öğretebilir misin?"
- "PC (8 GB RAM, CPU, GPUsuz) için gerçekçi tavan 1.5B–3B sınıfı tam performans bu şekilde alınıyorsa kullanıcının bilgisayarını tararız, uygunsa asistanı aktif ederiz, değilse uyarı veririz yine de kullanmak isterse yavaş bir şekilde kullansın."
- "Bu faz zaten açık bir faz, düzenlenebilir."
- "AI asistan geliştirme yapısını diğer modelden devral. Çakışma olmaması için logları güncelle. Çünkü bu uzun bir işlem. Yarın devam ederiz."
- "Muse Spark imzanı at. Bu iş sende."

### Sahiplik devri (kullanıcı kararı — kilitli)
- **Önce:** 6.92 servis/contract + 6.93 motor/veri **diğer modelde**, UI/entegrasyon/doğrulama ana modelde; 6.94 H4 motor diğer modelde. Çakışma kuralı: iki yazar, ayrı dosya kümeleri, `📨` teslim.
- **Şimdi:** AI asistanın **tamamı Muse Spark'ta** — sohbet servisi + KB motoru + prompt/retrieval + UI/entegrasyon + `docs/yardim` içeriği + planlar. **Diğer modelin AI asistan kapsamında yazacağı dosya kalmadı.**
- **Çakışma kuralı kalktı** (tek sahip). Dosya haritası: `Business/{Contracts,Services}/SistemServices/AiAsistan/*` + `MuhasibPro/Services/AiAsistan/*` + `ViewModels/.../Shell/Asistan*` + `YapayZekaAyarlarViewModel` + `Views/.../AsistanSohbetPaneli` + `YapayZekaAyarPaneli` + `docs/yardim/*.md` + `YARDIM-DB-PLAN`/`YARDIM-TEK-YUZEY-PLAN` → **Muse Spark**. Planlardaki "diğer model" satırları bu oturumda revize edildi.
- **Kural 20 notu:** eldeki iş (287 crash + K2/K3 onayı) bitmeden AI koduna başlanmaz. **Yarın sıra:** (1) AÇIK crash fix (`HATALAR` en üst) → (2) phi-3.5-mini ölçümü → (3) D2-D6 (Kural 8 sınıf onayı).

### D1 model ölçümleri (bu oturum — repo kodu değişmedi)
- **Yöntem:** `Temp/opencode/modeltest` ölçüm aracı (repo dışı; Foundry SDK ile indir→yükle→6 Türkçe soru; Q6 RAG-simülasyonu: `04-mali-donem-yonetimi.md` arşiv maddesi bağlam olarak yapıştırıldı). Dev PC GPU'lu → hız sayıları hedef CPU-only PC'yi temsil etmez; **kalite karşılaştırması geçerli** (varyantlar arası ağırlık aynı).
- **Katalog (CPU varyantları, `foundry.modelinfo.json`):** `qwen2.5-0.5b` 822 MB · `qwen3-0.6b` 593 MB · `qwen3-1.7b` 1354 MB · `qwen3.5-2b-text` 1464 MB · `qwen2.5-1.5b` 1822 MB · `smollm3-3b` 2131 MB · `phi-3.5-mini` 2590 MB.
- **`qwen3-1.7b-generic-cpu` ❌ ELENDİ:** disk 1355 MB · yükleme +1827 MB RAM. Sorunlar: `<think>` blokları cevaba sızıyor (reasoning model; pipeline'da strip yok) · "kısa cevap ver" dinlenmiyor (3. soruya 8410 karakter) · tekrar döngüsü (aynı cümle 10+ kez) · proje-dışı genel cevaplar.
- **`qwen2.5-1.5b-instruct-generic-cpu` ❌ ELENDİ:** disk 1842 MB · yükleme +1369 MB RAM · hızlı (ilk token ~0.4 sn). Sorunlar (Türkçe halüsinasyon): "Mali Dönemi arşivlenmez" (yanlış) · "1120×2=660" (yanlış) · uydurma yedek yolu · Q6 (RAG-bağlamlı) kısmen doğru ama buton adını uyduruyor ("tekrar aç butonu" vs gerçek "Arşivden Çıkar").
- **Ders (kullanıcı sorusuna cevap):** "WinUI3 + Microsoft modeli projeyi daha iyi anlar mı?" → **Hayır.** Hiçbir model MuhasibPro'yu bilmez; bilgi **RAG bağlamından** gelir (`AsistanPromptKurucu` + `AraAsync`). Üretici ≠ proje bilgisi. Seçim kriteri: **bağlama sadakat + Türkçe komut takibi + temiz çıktı** (reasoning sızıntısı yok) + boyut/RAM. `HATALAR.md`'ye ders işlendi.
- **Sıradaki aday:** `phi-3.5-mini` (~2.6 GB) — aynı 6 soru, tek tek ilkesine uygun (yarın).

### Donanım kapısı planı D1-D6 (kullanıcı onayı — KONTROL'e işlendi)
- Davranış: panel açılışı → lisans → yetki → `EtkinMi` → **donanım taraması** → Uygun=normal · Sınırda/Yetersiz=⚠️ **uyarı bandı, sohbet açık kalır (kilit yok, yavaş kullanım serbest)** · yetersizken ilk indirme öncesi onay dialogu (boyut + "yavaş olabilir").
- D1 model seçimi (sürüyor) · D2 `IDonanimUygunlukService` (RAM/disk/CPU, saf okuma) · D3 hüküm tablosu (öneri: ≥8 GB Uygun / 6-8 Sınırda / <6 GB Yetersiz — D1 ölçümüyle kesinleşir) · D4 panel uyarı bandı + `DonanimUyarisiniGizle` · D5 Denetim "Bu bilgisayar" kartı · D6 varsayılan alias + `08` yardım güncellemesi. Öğretme kısmı 6.94 H2'den yürür (değişmedi).

### Doküman (bu oturum)
- `YARDIM-DB-PLAN.md`: sahiplik "Muse Spark (Oturum 288 devir)" + çakışma kuralı tek-sahip revizyonu.
- `YARDIM-TEK-YUZEY-PLAN.md`: H4 motor "diğer model" → Muse Spark + sahiplik satırı.
- `KONTROL-LISTESI.md`: 6.92/6.94 sahiplik satırları + **6.92-D model seçimi** maddeleri (qwen3-1.7b ❌ · qwen2.5-1.5b ❌ · phi-3.5-mini ⬜ · donanım kapısı D2-D6 ⬜).
- `DURUM.md` + `ROADMAP.md` Karar Logu (devir + ölçüm bulguları) + `HATALAR.md` (reasoning `<think>` sızıntı dersi).
- **Repo kodu değişmedi** (çalışma dizini 286-287'den kirli, dokunulmadı). **Commit yok** (istenmedi).

---


## 2026-09-18 — Oturum 289 (max 1.5B kısıtı + ≤1.5B işlem planı; imza: Muse Spark)

### İstek (kullanıcı, aynen)
- "Sen sadece AI Asistan işleminden sorumlusun. Diğer işlemlere karışma." → **kapsam kilidi:** çökme (287) dahil AI-dışı işe dokunulmaz.
- "Büyük model çalışırken işlemciye ve RAM'e çok yük binecek, fan sesi gibi problemlere yol açacak. Bu işlemi en küçük modelle yapmamız gerekiyor. Max 1.5B ile."
- "Bir işlem planı çıkar. Projeyi yürüten ana model incelesin."

### Yapılan (ölçüm yok, repo kodu değişmedi)
- Başlamış `phi-3.5-mini` indirmesi durduruldu (ölçüm koşulamadı; `ModelTest.exe` prosesi yoktu, kalıntı indirme bulundu).
- Yük temizliği: `cache\models\Microsoft\Phi-3.5-mini-instruct-generic-cpu-2` silindi → **2648 MB geri alındı** (qwen önbellekleri + embedding modeli korundu).
- Katalog taraması (`foundry.modelinfo.json`): ≤1.5B sohbet adayları = `qwen3-0.6b` (593 MB) · `qwen3.5-0.8b` (1037 MB) · baz `qwen2.5-0.5b` (822 MB, mevcut varsayılan). Elenenler: `qwen2.5-1.5b` ❌ (288 halüsinasyon) · `qwen2.5-coder-1.5b` ❌ (coder) · `smollm3-3b`/`qwen3.5-2b-text`/`phi-3.5-mini` ❌ (limit üstü).
- **Risk notu:** iki qwen adayı da `supportsReasoning=True` (hibrit reasoning) → 288 `<think>` dersi geçerli; seçilen adayda strip işi (servis/panel + test) zorunlu olur.

### İşlem planı (ana model incelemesine — onaysız indirme/ölçüm yok)
1. **P1 — Ölçüm:** `qwen3-0.6b-generic-cpu` + `qwen3.5-0.8b-generic-cpu`, aynı 6 soru (`Temp/opencode/modeltest`, Q6 RAG-simülasyonlu; toplam indirme ~1.6 GB, phi'nin 2.6 GB'ından hafif). Çıktı: `modeltest/<alias>_sonuc.txt` + disk/RAM/ilk-token/süre.
2. **P2 — Karar:** kriter **bağlama sadakat + Türkçe komut takibi + temiz çıktı + düşük CPU/RAM** (288 + 289 yük kısıtı). Kazanan → `VarsayilanModelAlias` + `08` yardım güncellemesi (Kural 8 sınıf onayı). İkisi de bazı (`qwen2.5-0.5b`) geçemezse varsayılan kalır + prompt/RAG sertleştirme.
3. **P3 — Strip (koşullu):** kazanan reasoning'li ise `<think>` ayıklama (servis/panel + test; HATALAR 288).
4. **P4 — D2-D6 donanım kapısı** (288 planı aynen: `IDonanimUygunlukService` → hüküm → panel uyarı bandı (kilit yok) → Denetim "Bu bilgisayar" kartı; Kural 8 sınıf onayı + build/test + Kural 18 canlı + onay).
5. **P5 — 6.94 H2** (öğretme/içerik; değişmedi).

### Doküman
- `KONTROL-LISTESI` 6.92-D madde 9 revize (phi ❌ İPTAL + P1-P2 plan maddeleri) · `DURUM` (sıra + 289 özeti) · `ROADMAP` 6.92 satırı + Karar Logu · `LOG.md` indeks. **Commit yok.

---


## 2026-09-18 — Oturum 290 (sabit model + fine-tune kararı + stabilite planı; imza: Muse Spark)

### Soru (kullanıcı, aynen)
- "Sabit bir model kullansak ve bu modele projemizi öğretsek daha stabil çalışmaz mı?"

### Kod doğrulaması (plan modu, salt okunur)
- **Bilgi RAG'den geliyor (mimari teyit):** `AsistanPromptKurucu.cs:24` — "Yalnız aşağıdaki yardım maddelerine dayanarak cevap ver; maddelerde yoksa 'Bu konuda yardım maddesi yok.' de ve uydurma." (açık-kitap sınavı; ezber yok).
- **Model bugün sabit değil:** alias ayardan değişiyor (`AliasDegisiminiUygulaAsync`); kullanıcı Denetim'den değiştirebiliyor → davranış dalgalanır.
- **Determinizm kontrolü yok:** `FoundryAsistanSohbetService.cs:131` `CompleteChatStreamingAsync(istek, token)` — temperature/seed verilmiyor (model varsayılan örneklemesi).

### Kararlar (kullanıcı, kilitli)
- **Sabit model: TAM KİLİT.** Tek alias ürün sabiti; kullanıcı model değiştiremez (önbellek yönetimi kalabilir, seçim kalkar). `VarsayilanModelAlias` → sabit; `AliasDegisiminiUygulaAsync` yolu kapatılır.
- **Fine-tune: RAFTA.** Gerekçeler: (1) Foundry katalog (`azureml://registries/...`) özel ONNX'i kabul etmeyebilir — doğrulanmamış duvar; (2) ≤1.5B ezberi zayıf + genel Türkçeyi bozar (kanıt: 1.5B halüsinasyonu, 288); (3) içerik yaşıyor (6.94 H2) — her değişimde yeniden eğitim+dağıtım gerekir, RAG'de markdown satırı yeterli; (4) maliyet (GPU + veri + dönüşüm + regresyon) kurulu RAG'e karşı. Koşul: S1-S4 yetmezse ayrı fazda araştırılır.

### Stabilite planı S1-S4 (build modunda uygulanacak)
- **S1 — Tam kilit** (yukarıda).
- **S2 — Deterministik üretim:** `ChatOptions` temperature ~0 (+ seed destekleniyorsa).
- **S3 — Öğretme = RAG (6.94 H2):** içerik büyüdükçe stabilite artar; sadakat kuralı korunur.
- **S4 — Regresyon kapısı:** 6 soruluk set; alias/prompt değişimi setten geçmeden varsayılan olamaz (P1 ölçümleri ilk kullanım).

### Doküman
- `KONTROL-LISTESI` 6.92-D: S1-S4 maddeleri + fine-tune raf notu · `DURUM` (290 özeti) · `ROADMAP` Karar Logu · `LOG.md` indeks. **Repo kodu değişmedi. Commit yok.**

---


## 2026-09-18 — Oturum 291 (P1 ölçüm + P2 karar: iki aday elendi, varsayılan kalır; imza: Muse Spark)

### Emir (ana model)
- "Paralel — yardımcı: P1 ölçüm → P2 karar → P3 strip (koşullu) → P4 D2-D6 → S1/S2 (servis). Bana teslim: kazanan alias + gerekçe."

### P1 — Ölçüm (aynı 6 soru, `Temp/opencode/modeltest`; sonuçlar `modeltest/*_sonuc.txt`)
- **`qwen3-0.6b-generic-cpu` ❌ ELENDİ** (disk 511 MB · yükleme +769 MB RAM — yük iyi, kalite yıkım): her cevapta `<think>` sızıntısı · tekrar döngüleri (Q1 "bir yıl" ×~250, 6292 karakter; "kısa" dinlenmiyor) · yanlış cevaplar (Q3 soruyu yankılıyor, Q4 "if statement is deleted" saçmalığı) · **Q6 RAG-bağlamı yok sayıldı** (bağlam "Arşivden Çıkar ile yeniden açılır" derken ilgisiz "help mode" gevezeliği).
- **`qwen3.5-0.8b-generic-cpu` ❌ ELENDİ** (disk 1038 MB · yükleme +1280 MB RAM): `<think>` etiketi yok ama tekrar döngüleri (Q1 "güçlü bir yatırımcı" ×~70, 7050 karakter; Q2 46 adımlı uydurma liste, 6583 karakter) · Q4 saçmalığı ("ekonomiye ciddi bir yıkım") · **Q6 ÖLÜMCÜL: bağlamı inkâr** ("Hayır, dönemi tekrar açmak mümkün değildir" — bağlamın tam tersi) · yalnız Q5 doğru (2240).

### P2 — Karar (ana modele teslim)
- **Kazanan yok. Varsayılan `qwen2.5-0.5b` kalır + prompt/RAG sertleştirme** (289 planındaki fallback aynen uygulandı).
- **Gerekçe:** iki aday da en kritik kriterde (bağlama sadakat) sınıfta kaldı; 0.8B ayrıca RAG bağlamını aktif biçimde yalanlıyor. 0.5B zayıf Türkçesine rağmen canlıda RAG cevabı üretebilmişti (277 C2/C3, 282 S2). Ders `HATALAR.md`'de: ≤1B sınıfında RAG bile kurtarmaz.
- **P3 strip düştü** (kazanan reasoning'li model yok — koşul gerçekleşmedi).

### Temizlik
- Elenen iki modelin önbelleği silindi (**~1.5 GB geri alındı**); embedding modeli (`qwen3-embedding-0.6b`) + qwen önbellekleri korundu.

### Sıradaki (Kural 8 sınıf onayı gerekli — kod yok)
- P4 D2-D6 (DI kaydı dahil) → S1 kilit (servis + view seçimi) + S2 temperature. Onay gelmeden kod yazılmaz.

### Doküman
- `KONTROL-LISTESI` 6.92-D (iki ❌ + karar) · `HATALAR` (≤1B dersi) · `DURUM` (291) · `ROADMAP` 6.92 + Karar Logu · `LOG.md` indeks. **Repo kodu değişmedi. Commit yok.**

---


### E. Yardımcı model — P1/P2 teslim alındı + yeni görev (ana model, Oturum 291)
> **Kapsam (katman ayrımı):** yalnız AI **motor/veri/model** (+ testleri). `Views`/`ViewModels`/paylaşılan docs'a **dokunma** (ana model). Çakışma yok.
- **P1/P2 teslim alındı + ONAY ✅:** `qwen3-0.6b` ❌ + `qwen3.5-0.8b` ❌ (bağlama sadakat sınıfta; ~1.5 GB temizlendi). **Karar: varsayılan `qwen2.5-0.5b` kalır** + RAG/prompt sertleştirme. **P3 (strip) koşulsuz düştü.**
- **Yeni görev (sırayla):**
  1. **S2 — deterministik üretim:** `ChatOptions` `temperature ~0` (+ seed destekleniyorsa) — `FoundryAsistanSohbetService` (senin). Riski en düşük, hemen değerli.
  2. **S1 — sabit model (servis tarafı):** `VarsayilanModelAlias` ürün sabiti; `AliasDegisiminiUygulaAsync` yolunu kapat. **UI (Denetim model seçici kaldırma) ana modelde.**
  3. **D2 `IDonanimUygunlukService` + D3 hüküm:** RAM/disk/CPU tespiti (≥8 GB Uygun / 6-8 Sınırda / <6 Yetersiz) + DI kaydı. **D4/D5 UI ana modelde.**
- **Teslim:** her madde için tek blok (imzalar + build/test) + kullanıcıya mesaj.
- **Kapı:** Kural 8 sınıf onayı (kullanıcı) + build 0/0 + test; D4/D5 canlısı ana modelde.
- **ONAY ✅ (kullanıcı, Oturum 291): "Tümünü onaylıyorum"** — P1/P2 kararı + S2/S1(servis)/D2/D3 görevi + Kural 8.

---

