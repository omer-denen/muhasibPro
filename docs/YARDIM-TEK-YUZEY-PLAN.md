# MuhasibPro — Faz 6.94: Tek Yardım Yüzeyi (AI Asistanı) Planı

> **Faz 6.94** — Yardımı view'lere dağıtan `?` yardım dialogu/listeleri kaldırılır; **tek yardım kaynağı** `docs/yardim/*.md` olur ve **tek yardım yüzeyi** AI asistanıdır (F1 + durum çubuğu düğmesi).
> **Durum:** 📋 Plan oluşturuldu (Oturum 283). **Kod başlamadı** — kullanıcının ek soruları sonrası başlanacak.
> **Sahiplik (Oturum 293 — tek sahip):** motor/veri/model + prompt/retrieval (H4) + içerik (H2) + UI sadeleştirme (H1/H3) + doğrulama (H5) = **ana model** (yardımcı-model dönemi kapandı).

## Kullanıcı kararları (Oturum 283 — kilitli)
| Konu | Karar |
|---|---|
| Yardım kitabı | ❌ **İptal** — "Asistanı niye koyuyoruz o zaman" |
| View `?` yardım listeleri/dialogu | ❌ **Kaldırılır** — "ayrı ayrı yardım sayfası yazmanın anlamı yok" |
| Tek içerik kaynağı | ✅ `docs/yardim/*.md` (repo) → `AsistanBilgi.db` → asistan |
| Yardım girişi | ✅ **F1** asistanı açar + durum çubuğu **"Asistan"** düğmesi kalır |
| AI erişilemezse (lisans/yetki/kapalı/model yok) | ✅ **Yalnız kilit gerekçesi** gösterilir (statik yardım yok) |
| Eski yardım altyapısı | ✅ **Tümden silinir** (Kural 4) |
| Kapsam | ✅ **Tüm yapı:** kullanıcı ekranları + Denetim Masası + diyaloglar |
| Yürütme | ✅ Aşamalı: **H1 → H2 → H3 → H4 → H5** |

## Gerekçe
- Yardım içeriği bugün **iki ayrı yerde**: (a) ~11 view'de `YardimMaddeleri()` listeleri (~81 madde, `?` dialogu), (b) AI için `docs/yardim/*.md` (9 sayfa/60 madde). Çift bakım → drift (Kural 13 "yardım sayfayla birlikte yaşar").
- Asistan zaten kullanıcıya doğal dilde cevap veriyor; kullanıcı ayrı yardım sayfası **açıp okumaz**. Tek yerden güncelleme yeterli.
- "Kullanıcının ne soracağını bilemezsin" → AI **tüm yapıyı** bilmeli; içerik tüm ekranları kapsar.

## Mevcut durum (envanter — Oturum 283)
**Kaldırılacak yardım kaynakları (view `?`):**
| Kaynak | Madde |
|---|---|
| `LoginViewModel.YardimMaddeleri` | 5 |
| `FirmaShellViewModel.YardimMaddeleri` | 9 |
| `MainShellViewModel.YardimMaddeleri` | 6 |
| `MaliDonemYonetimViewModel.YardimMaddeleri` | 7 |
| `DatabaseSettingsViewModel.YardimMaddeleri` | 5 |
| `UpdateViewModel.YardimMaddeleri` | 5 |
| `GuncellemeSonrasiViewModel` | 4 |
| `YapayZekaAyarlarViewModel.YardimMaddeleri` | 9 |
| `GelistiriciAraclariViewModel.YardimMaddeleri` | 12 |
| `DenetimMasasiView.xaml.cs` | 9 |
| `SistemDbYonetimView.xaml.cs` | 6 |
| `SistemDbYonetimViewModel` / `SistemDatabaseStatusViewModel` / `MaliDonemlerListControl` / `YeniDonemDialog` / `PostUpdateAdimGorunum` | 5 |
| **Toplam** | **~81** |

**Altyapı (silinecek):** `Views/Components/YardimDialog.xaml(.cs)` + `YardimMaddesi` (view record) · `IDialogService`/`DialogService.ShowYardimAsync` · `Business/DTOModel/SistemModel/YardimMaddesiDto.cs` · `?` butonları (LoginView, MainShellView, FirmaShellView, MaliDonemYonetimView, DatabaseSettingsView, UpdateView, SistemDbYonetimView, DenetimMasasiView, GelistiriciAraclariPaneli, YapayZekaAyarPaneli).

**AI tarafı (korunur + genişler):** `docs/yardim/*.md` (9 sayfa/60 madde) → `AsistanBilgi.db` (`Madde`/`Vektor`/`Meta`) → `IYardimBilgiTabani` → `FoundryAsistanSohbetService` → sohbet paneli.

**Envanter notu:** `Views/ShellViews/Shell/TenantDatabaseUpdateView.xaml` listede çıktı; 6.91-E'de silinmiş olması gerekiyordu → **ölü kod doğrulaması** (Kural 4) H3'te.

## Mimari (hedef)
```
docs/yardim/*.md  (tüm uygulamayı kapsayan tek içerik)
        │  (build: EmbeddedResource — MuhasibPro.csproj:49)
        ▼
GomuluYardimIcerikKaynagi → YardimBilgiTabaniService.HazirlaAsync
        ▼
AsistanBilgi.db (Madde + Vektor + Meta)  →  IYardimBilgiTabani.AraAsync (hibrit RRF)
        ▼
F1 / durum çubuğu "Asistan"  →  AsistanSohbetPaneli  →  cevap
```

## Fazlar

### H1 — Karar / sözleşme (ben) — ⬜
- [ ] `AGENTS.md` **Kural 13** revizyonu: "her fonksiyonlu view'de `?` yardım dialogu" → **"tek yardım yüzeyi AI asistanı (F1 + statü çubuğu)"**; "yardım sayfayla birlikte yaşar" → "`docs/yardim` içeriği değişiminde güncellenir".
- [ ] `AGENTS.md` okuma listesine bu plan dosyasını ekle (`YARDIM-TEK-YUZEY-PLAN.md`).
- [ ] `docs/YARDIM-DB-PLAN.md` sözleşme revizyonu (v1.2): kapsam "yalnız AI, view'lardan bağımsız" → "**tek yardım kaynağı = tüm uygulama; UI `?` yardımı kaldırıldı**".
- [ ] `REFERANSLAR.md`: araştırma satırları — (a) MS F1/help girişi + uygulama-içi AI yardım konumlandırması, (b) sektör örneği (uygulama içi AI yardım). Kaynak + karar yazılır.
- [ ] `ROADMAP.md` Karar Logu satırı (bu fazın açılması).
- **Kapı:** kullanıcı onayı (Kural 8/15); kod yok.

### H2 — İçerik genişletme (ben) — ⬜
- [ ] 78 XAML view + ~81 mevcut yardım metni taranır; ekran/buton envanteri çıkarılır.
- [ ] Mevcut 9 sayfa/60 madde **buton/işlem düzeyinde** derinleştirilir (projeye özgü; genel anlatım yasak).
- [ ] Yeni sayfalar eklenir:
  - [ ] `10-denetim-masasi.md` (Görünüm/Güvenlik/Firma Kayıt/Veritabanı/Mali Dönem/Güncelleme/Yapay Zeka bölümleri)
  - [ ] `11-sistem-veritabani-yonetimi.md` (analiz/onar/yedek/güncelleme/tanı)
  - [ ] `12-guncelleme-sonrasi.md` (4 adım + sonuç hâlleri)
  - [ ] `13-diyaloglar.md` (Yeni Firma/Dönem, göç onayı, geri yükleme onay kodu, yedek sil, transfer, kimlik sıfırlama)
  - [ ] `14-acilis-kurulum.md` (Splash/KurulumSplash)
- [ ] Yazım standardı: **ekran → buton adı → adım sırası → sonuç/onay/hata**; `Etiket:` satırı ekran+buton adlarıyla.
- [ ] Kaynak: gerçek XAML/VM metinleri + mevcut yardım maddeleri (uydurma yok — Kural 15).
- **Kapı:** içerik gözden geçirme (kullanıcı); `AsistanBilgi.db` otomatik tazelenir.

### H3 — UI sadeleştirme (ben, Kural 8 — dosya bazlı onay) — ⬜
- [ ] `?` butonları kaldırılır (yukarıdaki 10 view).
- [ ] `YardimMaddeleri()`/`YardimGoster`/`YardimCommand` (~81 madde) + `YardimAnahtari`/`YardimBasligi` alanları silinir.
- [ ] `YardimDialog` + `YardimMaddesi` (view) + `YardimMaddesiDto` + `IDialogService/DialogService.ShowYardimAsync` silinir.
- [ ] Global **F1** kısayolu → asistan panelini açar (`MainShellView`/`ShellStatusBar` `KeyboardAccelerator` veya pencere tuş işleyicisi).
- [ ] Kilitliyken F1 → panel kilit gerekçesini gösterir (mevcut `KilitMetni`).
- [ ] `TenantDatabaseUpdateView` ölü kod doğrulaması (varsa sil).
- **Kapı:** build 0/0 + test yeşil; Kural 8 onayları.

### H4 — Motor (Faz 6.97) — 🔨
- [ ] Genişleyen derlem için retrieval ayarı (top-k / etiket ağırlığı).
- [ ] Prompt sertleştirme: yalnız yardım maddelerine dayan, genel tavsiye verme, "yardım maddesi yok" fallback'i korunur.
- [ ] Değerlendirme seti: örnek "soru → beklenen madde" (offline) + canlı ölçüm.
- [ ] Sözleşme değişikliği gerekirse **Kural 15 + onay + `YARDIM-DB-PLAN` revizyonu**.

### H5 — Doğrulama + doküman (ben) — ⬜
- [ ] build 0/0 + test (silinen yardım testleri temizlenir).
- [ ] **Canlı (Kural 18):** F1 → asistan; her ekrandan projeye özgü cevap; kilit senaryosu (lisans/yetki/kapalı) yalnız gerekçe gösterir; kanıt + kullanıcı onayı.
- [ ] LOG/DURUM/ROADMAP/KONTROL/REFERANSLAR güncel.

## Riskler / notlar
- **Yardım artık AI'ya bağımlı:** lisans (Profesyonel/Kurumsal; Deneme açık, Standart kapalı), `AiAsistan_Kullan` yetkisi, "Asistan etkin" anahtarı ve model yüklenebilirliği. Bu koşullar sağlanmazsa kullanıcı yardım alamaz (kullanıcı kararı kabul).
- **İçerik hacmi:** 60 → ~200 madde; embedding süresi ve retrieval top-k motor tarafında ayarlanmalı.
- **Kural 13 değişikliği:** AGENTS/plan güncellenmeden H3 yapılırsa kural ihlali sayılır.
