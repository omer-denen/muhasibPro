# MuhasibPro — Oturum Günlüğü — İndeks (Kitap)

> **Token tasarrufu için kitap gibi:** Bu dosya **sadece indeks** — 20'şerlik ciltlere bakar, sonra ilgili cilde gider. Her oturumda `LOG.md`'nin son 2-3 oturumu yerine **bu indeksi oku**, sonra `docs/LOG/LOG-XX-YY.md` cildindeki ilgili oturumu aç. `LOG.md` kendisi 96 KB değil, ~4 KB indekstir.

> **Kodlama:** UTF-8 (BOM, 65001). PowerShell'de `chcp 65001` ve `Get-Content -Encoding UTF8`.

---

## Nasıl Okunur (Model için)

1. **Önce `docs/DURUM.md`:** "nerede kaldık" tek yüzey (aktif faz + açık kararlar + sonraki adım). Her oturum buradan başlar; 170k token'lık tüm log okuması gerekmez.
2. **Bu dosya yalnız indekstir:** Gerekirse grep/arama ile ihtiyaç duyulan oturumu bul; tümünü okuma.
3. **Cilt:** Güncel cilt `docs/LOG/LOG-261-280.md`; eski ciltler `docs/LOG/Arsiv/`. Yalnız ilgili oturum başlığını oku.
4. **Arşiv politikası:** Kapanan ciltler `Arsiv/`'e taşınır (okuma yolunda değil, gerektiğinde grep).
5. **Yeni oturum eklerken:** Güncel cilt 20'yi doldurmadıysa oraya ekle; dolduysa yeni cilt oluştur ve bu indekse satır ekle.

---

## Ciltler

| Cilt | Oturum Aralığı | Dosya | Durum |
|------|---------------|-------|-------|
| Cilt 1 | 1-20 | Arsiv/LOG-01-20.md | arsiv |
| Cilt 2 | 21-40 | Arsiv/LOG-21-40.md | arsiv |
| Cilt 3 | 41-60 | Arsiv/LOG-41-60.md | arsiv |
| Cilt 4 | 61-80 | Arsiv/LOG-61-80.md | arsiv |
| Cilt 5 | 81-100 | Arsiv/LOG-81-100.md | arsiv |
| Cilt 6 | 101-120 | Arsiv/LOG-101-120.md | arsiv |
| Cilt 7 | 121-140 | Arsiv/LOG-121-140.md | arsiv |
| Cilt 8 | 141-160 | Arsiv/LOG-141-160.md | arsiv |
| Cilt 9 | 161-180 | Arsiv/LOG-161-180.md | arsiv |
| Cilt 10 | 181-200 | Arsiv/LOG-181-200.md | arsiv |
| Cilt 11 | 201-220 | Arsiv/LOG-201-220.md | arsiv |
| Cilt 12 | 221-240 | Arsiv/LOG-221-240.md | arsiv |
| Cilt 13 | 241-260 | Arsiv/LOG-241-260.md | arsiv |
| Cilt 14 | 261-280 | LOG-261-280.md | aktif |

---

## Oturum İndeksi (274 numaralı oturuma kadar, 14 cilt)

| # | Tarih | Baslik | Durum | Cilt |
|---|-------|--------|-------|------|
| 1 | 2026-08-21 | İskelet + plan | ok | C1 |
| 2 | 2026-08-21 | Faz 1 Domain | ok | C1 |
| 3 | 2026-08-21 | DbContext derin iyileştirme | ok | C1 |
| 4 | 2026-08-21 | Data/Business uyumu | ok | C1 |
| 5 | 2026-08-22 | Web Migration prototip | ok | C1 |
| 6 | 2026-08-22 | Çekirdek + Feature Flags | ok | C1 |
| 7 | 2026-08-22 | Data/Business katmanı | ok | C1 |
| 8 | 2026-08-22 | Entegrasyon | ok | C1 |
| 9 | 2026-08-22 | Derleme doğrulama | ok | C1 |
| 10 | 2026-08-23 | İlk Kurulum + Login görsel doğrulama | ok | C1 |
| 11 | 2026-08-23 | Göz ikonu + FirmaShell + Saga + NU1903 | ok | C1 |
| 12 | 2026-08-23 | Web temizliği — saf WinUI3 | ok | C1 |
| 13 | 2026-08-23 | Splash/DataList orijinale dönüş + DesignTokens | ok | C1 |
| 14 | 2026-08-23 | Login üst şerit + dinamik hızlı giriş + Hello | ok | C1 |
| 15 | 2026-08-23 | Görünüm referansı + fonksiyonellik kararı — tem... | ok | C1 |
| 16 | 2026-08-23 | Yeni renk stili — Splash + Login | ok | C1 |
| 17 | 2026-08-24 | SistemKurulum Bloom güncelleme + migration log ... | ok | C1 |
| 18 | 2026-08-24 | devamı (Migration profesyonel revizyon — aceley... | ok | C1 |
| 19 | 2026-08-24 | SistemKurulum 2. tasarım + Login küçük ekran + ... | ok | C1 |
| 20 | 2026-08-24 | Login küçük ekran + hover static renk | ok | C1 |
| 21 | 2026-08-24 | FirmaShellView yeni tasarım — başlangıç | ok | C2 |
| 22 | 2026-08-24 | FirmaShell god-class önlendi — sınıflar düzenlendi | ok | C2 |
| 23 | 2026-08-24 | FirmaShellControls kart kontrolleri + canlı akı... | ok | C2 |
| 24 | 2026-08-24 | FirmaShellView yeni tasarım — kart mimarisi | ok | C2 |
| 25 | 2026-08-25 | LogService DI + SAGA captive fix — kullanıcı te... | ok | C2 |
| 26 | 2026-08-25 | devamı (AppLog tenant guard — kullanıcı hatırla... | ok | C2 |
| 27 | 2026-08-25 | Çekirdek Tamamlama İlkesi — kullanıcı kararı | ok | C2 |
| 28 | 2026-08-25 | MaliDonemDetails sadeleştirme + progress birleş... | ok | C2 |
| 29 | 2026-08-26 | Stil referans temizliği + akış planı güncelleme | ok | C2 |
| 30 | 2026-08-26 | SistemKurulum — webToWinui3 stil taşıma, Splash... | ok | C2 |
| 31 | 2026-08-26 | Login — webToWinui3 stil taşıma, akışın ikinci ... | ok | C2 |
| 32 | 2026-08-26 | FirmaShell — webToWinui3 stil taşıma, akışın üç... | ok | C2 |
| 33 | 2026-08-26 | MainShell — webToWinui3 stil taşıma, akışın son... | ok | C2 |
| 34 | 2026-08-26 | Splash ince ayar + UpdateService birleşik — çek... | ok | C2 |
| 35 | 2026-08-26 | SistemKurulumViewModel — EF Core katman ihlali ... | ok | C2 |
| 36 | 2026-08-26 | AGENTS Katı Kurallar — Ölü Kod / ViewModel Kola... | ok | C2 |
| 37 | 2026-08-26 | SistemKurulumViewModel 450 satır — god-class bö... | ok | C2 |
| 38 | 2026-08-26 | Görsel İnceleme — “Hazır” Netliği + View Başarı... | ok | C2 |
| 39 | 2026-08-26 | SistemKurulumView DatabaseInfoPanel bölünmesi —... | ok | C2 |
| 40 | 2026-08-26 | SistemKurulum tekrar temizliği + layout düzeltme | ok | C2 |
| 41 | 2026-08-27 | Domain/Data regresyon fix + LOG kitap + SistemKurulum IsBusy + çekirdek testleri | ok | C3 |
| 42 | 2026-08-28 | Tasarım entegrasyonu + eski klasör temizliği | ok | C3 |
| 43 | 2026-08-28 | Değişken tema OOBE + splash light + migration fix | ok | C3 |
| 44 | 2026-08-28 | Business DB testleri + 6 bug fix | ok | C3 |
| 45 | 2026-08-29 | DbContext extensions çekirdek mühürleme + tenant entity sözleşmesi | ok | C3 |
| 46 | 2026-08-29 | LoginView OOBE acrylic + akış mühürleme | ok | C3 |
| 47 | 2026-08-29 | Splash + LoginView — Tamamlandı | ok | C3 |
| 48 | 2026-08-30 | SistemKurulum compaction geri alma + LogsText + PRI263 | ok | C3 |
| 49 | 2026-08-30 | SistemKurulumView final rötüş + Login yönlendirme | ok | C3 |
| 50 | 2026-08-30 | FirmaShell görünüm tam entegrasyon | ok | C3 |
| 51 | 2026-08-30 | Dialog görünüm tamamlama + ölü view temizliği | ok | C3 |
| 52 | 2026-08-30 | DesignTokens Light+Dark + Splash kart + SistemKurulum sadeleştirme | ok | C3 |
| 53 | 2026-08-30 | Görsel OOBE yeni arkaplan + kartlar Cards.xaml'a taşındı + gölge + teşhis dialogu | ok | C3 |
| 54 | 2026-08-30 | ToastService ayrı pencere + bildirim servisleri organizasyonu | devam | C3 |
| 55 | 2026-08-30 | Toast tek-pencere (ToastOverlayWindow) entegrasyonu | ❌ düzelmedi | C3 |
| 56 | 2026-08-30 | Toast kök neden: SystemBackdrop akrilik | ❌ çözülmedi | C3 |
| 57 | 2026-08-31 | ToastOverlayWindow MicaBaseAlt | ✅🧪 | C3 |
| 58 | 2026-08-31 | ToastOverlayWindow silindi → sayfa içi ToastHostControl | ✅🧪 | C3 |
| 59 | 2026-08-31 | MainWindow MainFrame + Beni hatırla animasyonu + **Toast→OS toast (CommunityToolkit)** + **SistemKurulum OOBE** | ✅🧪 | C3 |
| 60 | 2026-08-31 | Tasarim stili ModernCard kesin komut + FirmaDetails modern iskelet + Header normalizasyon | ✅🧪 | C3 |
| 61 | 2026-09-01 | FirmaShell polish + Header UserControl + Saga fix | ✅🧪 | C4 |
| 62 | 2026-09-01 | Login + Pencere kapanış + Startup tek kaynak | ✅🧪 | C4 |
| 63 | 2026-09-02 | FirmaShellViewModel composition — AGENTS god-class kuralı | ✅🧪 | C4 |
| 64 | 2026-09-02 | FirmaShell hata düzeltme — duplicate MaliDonemList + edit/gir butonları | ✅🧪 | C4 |
| 65 | 2026-09-02 | Splash→SistemKurulum→Login akış mühürleme + DB repair/restore | ✅🧪 | C4 |
| 66 | 2026-09-02 | SelectedFirma null fix — mesaj dispatch race condition | ✅🧪 | C4 |
| 67 | 2026-09-02 | Tasarım revizyonu: DesignTokens + glass kart + Splash/Login/SistemKurulum | ✅🧪 | C4 |
| 68 | 2026-09-02 | SagaPipeline dark→OOBE + MaliDonem liste sorunu + SelectedBadge | ❌ kısmi | C4 |
| 69 | 2026-09-02 | FirmaShell seçim onarımı + OOBE yerleşim + arşiv | ✅🧪 | C4 |
| 70 | 2026-09-02 | Kapat onayı fix + ortak ShellTitleBar refactor | ✅🧪 | C4 |
| 71 | 2026-09-02 | MaliDonem kart vitrini gerçek zamanlı fix (Yedek→SonYedek+Boyut) | ✅🧪 | C4 |

| 72 | 2026-09-02 | MaliDonem kart: hover/seçim fix + kart büyütme + bilgi/buton ayraç + DB Durumu rozeti (analiz) | ✅🧪 | C4 |
| 73 | 2026-09-03 | MaliDonem/Firma radio kart + hover/seçim + dark→OOBE + ContentDialog Light | ✅🧪 | C4 |
| 74 | 2026-09-03 | Dialog Light garantisi kodda + XAML RequestedTheme yasağı | ✅🧪 | C4 |
| 75 | 2026-09-03 | MaliDonem kart: boyut 0 + dosya-yok Kurtar/Sil + arşiv onayı/çıkarma | ✅🧪 | C4 |
| 76 | 2026-09-04 | Mali Dönem Yönetim penceresi + kart hafifletme | ✅🧪 | C4 |
| 77 | 2026-09-04 | Şablon Aşama 1: Genel Bakış + bakım + derin analiz | 🔨 yarım | C4 |
| 78 | 2026-09-04 | Şablon Aşama 1 tamamlama: sayfa bağlantısı + build/test | ✅🧪 | C4 |
| 79 | 2026-09-04 | Motor 3 görsel + Aşama A iskelet | ✅🧪⚠️ | C4 |
| 80 | 2026-09-04 | Motor 3-sayfa tasarım (Tümü + Dönem + Analiz) | ✅ kod 🧪❌ | C5 |
| 81 | 2026-09-04 | Tooltip teşhisi + manuel stil XAML'e | ✅🧪 | C5 |
| 82 | 2026-09-05 | Widget görsel dili temel + ölü stil temizliği | ✅🧪 | C5 |
| 83 | 2026-09-05 | Login Mica + glass deney | ✅🧪 | C5 |
| 84 | 2026-09-05 | Panel dili + Login giydirme | ✅🧪 | C5 |
| 85 | 2026-09-05 | Panel dili rollout + canlı bulgular + MaliDonemListView sorunları | 🔨 kısmi | C5 |
| 86 | 2026-09-05 | 3 açık madde fix: hover + tooltip + EF concurrency | ✅🧪 | C5 |
| 87 | 2026-09-05 | Login tek logo + tooltip soruşturması + Yedekler Dönem içine | ✅🧪 | C5 |
| 88 | 2026-09-05 | Tasarım dili → Login pilotu | ✅🧪 | C5 |
| 89 | 2026-09-05 | Login: kartlar TEK acrylic kap içine | ✅🧪 | C5 |
| 90 | 2026-09-05 | Ana tasarım dili + mühür + stiller + Splash refactor | ✅🧪 | C5 |
| 91 | 2026-09-05 | Splash buz anakart + header sökümü + SistemKurulum | ✅🧪 | C5 |
| 92 | 2026-09-05 | FirmaShell çevirme + ShellTitleBar silme | ✅🧪 | C5 |
| 93 | 2026-09-05 | MaliDonemYönetim çevirme (view + 9 panel + Details) | ✅🧪 | C5 |
| 94 | 2026-09-05 | MainShell çevirme + TitleBarControl silme | ✅🧪 | C5 |
| 95 | 2026-09-05 | Controls + Dialog içerikleri + buton gizemi | 🔨 kısmi | C5 |
| 96 | 2026-09-06 | Yarım oturum tamamlama + dialog buton gizemi KAPANDI | ✅🧪 | C5 |
| 97 | 2026-09-06 | GorunumTest + tooltip fare-hover + WindowTitle + segment VSM | ✅🧪 | C5 |
| 98 | 2026-09-06 | Login kart-dışı başlık + genişletme + GERİ ALMA | ↩️ geri alındı | C5 |
| 99 | 2026-09-06 | Cards Custom* stilleri + Splash/Login/FirmaShell background — log kaydı | ✅🧪 | C5 |
| 100 | 2026-09-06 | SistemKurulum Custom* geçişi — senin güncellediğin gibi | ✅🧪 | C5 |
| 101 | 2026-09-06 | FirmaShellView yapısı mühür — AppBackgroundBrush + header ayrı + CustomGlassPanel + CustomModernCard | ✅🧪 | C5 |
| 102 | 2026-09-06 | Arkaplan warm + Teal yeşil + QuickDialog + FirmaShell polish + Radio + canlı | ✅🧪 | C5 |
| 103 | 2026-09-06 | QuickDialog alt-buton kalıbı + buton merkezileştirme | ✅🧪 | C6 |
| 104 | 2026-09-07 | MaliDonemYonetimView tasarım: kart stili + tab birleştirme + LoginView adım kartı | ✅🧪 | C6 |
| 105 | 2026-09-07 | MaliDonemYönetim tooltip soruşturması: kod bug yok + sekme ipuçları | ✅🧪 | C6 |
| 106 | 2026-09-07 | MaliDonemYönetim master-detail yeniden tasarım (tasarım dili) | ✅🧪 | C6 |
| 107 | 2026-09-07 | Yönetim revizyon: scroll + 3 kolon + 2. referans dili | ✅🧪 | C6 |
| 108 | 2026-09-07 | Yönetim revizyon: seçim görseli + birleşik dönem kartı | ✅ kod 🧪❌ | C6 |
| 109 | 2026-09-07 | Birleşik kart sadeleşme: hızlı-yedek çıktı, hero + bilgi şeridi | ✅ kod 🧪❌ | C6 |
| 110 | 2026-09-07 | Teal → mavi-uç petrol: isimlerle rename + değerler | ✅🧪 | C6 |
| 111 | 2026-09-07 | Zeytin ikinci vurgu + küçük butonlar görünür | ✅🧪 | C6 |
| 112 | 2026-09-07 | Yedek listesi boş bug'ı: yazma/okuma desen uyuşmazlığı | ✅🧪 | C6 |
| 113 | 2026-09-07 | Kimlikli yedek/geri-yükleme + kurulum kaydı: ONAYLI PLAN | 📋 plan | C6 |
| 114 | 2026-09-08 | Kimlikli yedek/geri-yükleme + kurulum kaydı + birleşik kart kapanışı | ✅🧪 | C6 |
| 115 | 2026-09-08 | FirmaShellViewModel god-class tespiti — refactoring planı | 🔨 plan | C6 |
| 116 | 2026-09-08 | 6.48 god-class split + güncelleme gerçek yapı | ✅🧪 | C6 |
| 117 | 2026-09-08 | Uygulama güncelleme hattı: manuel feed + UpdateView | ✅🧪 | C6 |
| 118 | 2026-09-08 | DB güncelleme sayfası: ön-dialog + full sayfa | ✅🧪 | C6 |
| 119 | 2026-09-08 | Canlı doğrulama: ön-dialog + sayfa + gerçek göç | ✅ kısmi | C6 |
| 120 | 2026-09-08 | MaliDonemYonetimView canlı revizyon + pagination + Veritabanı Ayarları | 🔨 kısmi | C6 |
| 121 | 2026-09-08 | Ölü/tekrar sınıf süpürmesi + Oturum 120 build fix | ✅🧪 | C7 |
| 122 | 2026-09-08 | Faz 1 M1 + Kurulum/SystemDb ayrımı + mimari bekçi | ✅🧪 | C7 |
| 123 | 2026-09-08 | Faz 2 + ayar yetki kuralı + ortak-altyapı | ✅🧪 | C7 |
| 124 | 2026-09-09 | Faz 3 M3 EntityRegistry | ✅ kod 🧪❌ | C7 |
| 125 | 2026-09-09 | Faz 3 M3 statik doğrulama | ✅ statik 🧪❌ | C7 |
| 126 | 2026-09-09 | Faz 4 M5 Tenant kısmi | ✅ kod 🧪❌ | C7 |
| 127 | 2026-09-09 | Faz 4 M5 derin bağlantı + kalan testler | ✅🧪 | C7 |
| 128 | 2026-09-09 | Faz 5 ilk adım: politika DI + ITenantContext ölümü | ✅🧪 | C7 |
| 129 | 2026-09-09 | Faz 5 devam: LicenseSettings + KaydedenId + E2 | ✅🧪 | C7 |
| 130 | 2026-09-09 | KaydedenId tek sabit | ✅🧪 | C7 |
| 131 | 2026-09-09 | SemVer geçişi (tenant/sistem/app) | ✅🧪 | C7 |
| 132 | 2026-09-09 | Faz 5 kapanışı: Kullanici/Lisans + E2 | ✅🧪 | C7 |
| 133 | 2026-09-09 | Faz 6 Mühür: 0 uyarı + docs | ✅🧪 | C7 |
| 134 | 2026-09-09 | Tasarım brief bakımı | ✅ docs | C7 |
| 135 | 2026-09-09 | Git hazırlığı | ✅ | C7 |
| 136 | 2026-09-09 | Tasarım tek-tip geçiş başladı | 🔨 devam | C7 |
| 137 | 2026-09-09 | MaliDonemYonetimView seçerek alındı | ✅🧪 | C7 |
| 138 | 2026-09-09 | Tenant güncelleme 3 bug loglandı — modüler geçiş sebebi | ✅ docs | C7 |
| 139 | 2026-09-09 | Per-view ayar panelleri planı (her view kendi ayarı) | 📋 plan | C7 |
| 140 | 2026-09-09 | Circular DI + kör bekçi fix + canlı E2E | ✅🧪 | C7 |
| 141 | 2026-09-09 | AI-Yönetim devralma + Bilinmeyen sola + hamburger/blur/row-scroll + B3 kapanışı | ✅🧪 | C8 |
| 142 | 2026-09-09 | Yedek pagination fix + chevron + Expander'sız parametre | ✅🧪 | C8 |
| 143 | 2026-09-09 | MaliDonemYonetim firma-bazlı ayarlar (HANDOFF) | 🔨 devam | C8 |
| 144 | 2026-09-09 | 6.65 kapanış: ayar dialog + pane butonu + docs | ✅🧪 | C8 |
| 145 | 2026-09-10 | Canlı test + dialog v2 + 6.66 planı | 🔨 plan | C8 |
| 146 | 2026-09-10 | Dialog blur (tüm zeminler acrylic) | ✅🧪 | C8 |
| 147 | 2026-09-10 | Dialog chrome tekilleştirme + smoke-blur | ✅🧪 | C8 |
| 148 | 2026-09-10 | Dialog blur artırma | ✅🧪 | C8 |
| 149 | 2026-09-10 | Composition backdrop-blur (DialogHelper tek kapı) | ✅🧪 | C8 |
| 150 | 2026-09-10 | Dialog tek kapı tamam + tutarlılık kuralı | ✅🧪 | C8 |
| 151 | 2026-09-10 | Blur şiddeti + kapanış kasması | ✅🧪 | C8 |
| 152 | 2026-09-10 | Takılı blur fix + minik blur | ✅🧪 | C8 |
| 153 | 2026-09-10 | Blur 3px | ✅🧪 | C8 |
| 154 | 2026-09-10 | Kapanış beklemesi (Closing anı) | ✅🧪 | C8 |
| 155 | 2026-09-10 | Kararma perdesi: Popup yerleşim (canlı) | ✅🧪 | C8 |
| 156 | 2026-09-10 | Takılı blur teşhis enstrümantasyonu | 🔨 teşhis | C8 |
| 157 | 2026-09-10 | Dialog-blur hattı geri alındı | ↩️ geri alma | C8 |
| 158 | 2026-09-10 | 6.66 v2 ayar dialogu uygulaması | ✅ kod 🧪❌ | C8 |
| 159 | 2026-09-10 | Ayar paneli internet araştırması | 📋 bulgu | C8 |
| 160 | 2026-09-10 | 6.67 ayar refactor uygulaması | ✅ kod 🧪❌ | C8 |
| 161 | 2026-09-10 | Dialog zeminleri SubtleBrush | ✅ kod 🧪❌ | C9 |
| 162 | 2026-09-10 | Login sağ kolon başlık+kart | ✅ kod 🧪❌ | C9 |
| 163 | 2026-09-10 | FirmaShell başlık ikon+chip | ✅ kod 🧪❌ | C9 |
| 164 | 2026-09-10 | Radio kart arkaplan uyumu | ✅ kod 🧪❌ | C9 |
| 165 | 2026-09-10 | Dönem durum + 2027 teşhisi | 🔍 teşhis | C9 |
| 166 | 2026-09-10 | Yeni dönem buton görünürlüğü | ✅ kod 🧪❌ | C9 |
| 167 | 2026-09-10 | 2027 elle göç + damga | ✅ veri 🧪❌ | C9 |
| 168 | 2026-09-10 | Seçili-hover yok + buton hover | ✅ kod 🧪❌ | C9 |
| 169 | 2026-09-10 | Ayar dialogu ekran-görüntüsü tasarımı | ✅ kod 🧪❌ | C9 |
| 170 | 2026-09-10 | IconShutdown çöküşü | ❌→✅ | C9 |
| 171 | 2026-09-10 | Dialog Kapat→X | ✅ kod 🧪❌ | C9 |
| 172 | 2026-09-10 | Durum-X aralığı | ✅ kod 🧪❌ | C9 |
| 173 | 2026-09-10 | Toplu log kapanışı | 📋 docs | C9 |
| 174 | 2026-09-10 | Ön muhasebe modül araştırması | 📋 bulgu | C9 |
| 175 | 2026-09-10 | Çekirdek iskelet araştırması | 📋 bulgu | C9 |
| 176 | 2026-09-10 | Güncelleme altyapısı | 📋 bulgu | C9 |
| 177 | 2026-09-10 | View bağımlılık haritası | 📋 docs | C9 |
| 178 | 2026-09-10 | Güncelleme yapısı diagramda mı | ❓ cevap | C9 |
| 179 | 2026-09-10 | Güncelleme altyapı denetimi | ✅ hazır | C9 |
| 180 | 2026-09-10 | Açık faz envanteri | 📋 liste | C9 |
| 181 | 2026-09-10 | İş kuyruğu kaydı | 📋 plan | C9 |
| 182 | 2026-09-10 | 0. Windows borcu (build/test/smoke) | ✅🧪 | C10 |
| 183 | 2026-09-10 | 1. B1 rozet A + 2. Güncelle butonu | ✅🧪 | C10 |
| 184 | 2026-09-10 | 3. B2 tetikleme + 4. BilinmeyenPanel | ✅ | C10 |
| 185 | 2026-09-10 | 5. Continue + 6. Ölü dialog | ✅🧪 | C10 |
| 186 | 2026-09-10 | 7. plan + Faz 0/1 (break-fix, rename) | ✅🧪 | C10 |
| 187 | 2026-09-10 | Faz 2 iskelet + kullanıcı-anahtarı | ✅🧪 | C10 |
| 188 | 2026-09-10 | Faz 3 Giriş Güvenliği + oturum ekleri | ✅🧪 | C10 |
| 189 | 2026-09-10 | Retrofit + avatar | ✅🧪 | C10 |
| 190 | 2026-09-10 | Denetim Masası: menu-driven NavigationView + iç Frame | ✅🧪 | C10 |
| 191 | 2026-09-10 | Win11 Ayarlar dili + kullanıcı-bazlı ayar JSON'u | ✅🧪 | C10 |
| 192 | 2026-09-10 | Menü: Giriş en üst + Güvenlik | ✅🧪 | C10 |
| 193 | 2026-09-10 | Veritabanı Yönetimi ağacı + dashboard | ✅🧪 | C10 |
| 194 | 2026-09-10 | Firma sökümü + başlık tek-butonu | ✅🧪 | C10 |
| 195 | 2026-09-11 | FirmaShell header tek kapsül + ikonlu başlıklar + bugfix | ✅🧪 | C10 |
| 196 | 2026-09-11 | M4 DatabaseSettings altyapısı (provider + 2 bölüm) | ✅🧪 | C10 |
| 197 | 2026-09-11 | Yönetici kilidi bug'ı (seed fallback) | ✅🧪 | C10 |
| 198 | 2026-09-11 | Denetim Güncelleme bölümü (UpdateView gömülü) | ✅🧪 | C10 |
| 199 | 2026-09-11 | Canlı güncelleme testi + seed-hash + donma fix'leri | ✅🧪 | C10 |
| 200 | 2026-09-11 | 1.1.1 canlı tur: 0-byte kurulum + timeout + converter fix + release workflow | ✅🧪 | C10 |
| 201 | 2026-09-12 | Firma/Dönem gerçek bölümler + footer dinamik + AutoMapper silme + UpdateViewModel split | ✅🧪 | C11 |
| 202 | 2026-09-12 | Mali Dönem Yönetim 4 kritik + Kural 11 (busy/result) | ✅🧪 | C11 |
| 203 | 2026-09-12 | Yerleşim düzeltme (sekmeli dialog) + silme sertleştirme + saklama koruması | ✅🧪 | C11 |
| 204 | 2026-09-12 | Refactoring planı + Kural 12 (işlem-görseli disiplini) | 📋 plan | C11 |
| 205 | 2026-09-12 | Faz 6.71 kaydı (kod yok, checklist genişletme) | 📋 faz | C11 |
| 206 | 2026-09-12 | Saf Fluent + stabilizasyon fazı (Faz 6.72, kod yok) | 📋 faz | C11 |
| 209 | 2026-09-12 | Faz 6.73 + 6.74 kaydı + Kural 13 (kod yok) | 📋 faz | C11 |
| 210 | 2026-09-12 | Kapanış + MuseCode devir notu (kod yok) | 📋 devir | C11 |
| 207 | 2026-09-12 | Dalga 0: 6 alanlık bug listesi alındı, teşhis başladı | 🔨 aktif | C11 |
| 208 | 2026-09-12 | Dalga 0 sonuçları + büyük-iş tasarım kararı (akış detayı + DB logu) | 📋 kayıt | C11 |
| 209 | 2026-09-12 | Faz 6.73 + 6.74 kaydı + Kural 13 (kod yok) | 📋 faz | C11 |
| 210 | 2026-09-12 | Kapanış + MuseCode devir notu (kod yok) | 📋 devir | C11 |
| 211 | 2026-09-12 | Faz 6.71/1 test seferberliği (4 dosya ~98 nokta) | ✅ kod 🧪❌ | C11 |
| 212 | 2026-09-12 | GetById_Bulunamadi kırmızısı → ürün fixi | ✅🧪 | C11 |
| 213 | 2026-09-12 | Faz 6.71/2 kritik bug kapanışı (4 madde) | ✅🧪 | C11 |
| 214 | 2026-09-12 | Dalga 0 Kol A: Tenant yedek/silme 7/9 kırık | ✅🧪 | C11 |
| 215 | 2026-09-12 | Dalga 0 Kol B+C: SistemDb + Güncelleme 7/9 | ✅🧪 | C11 |
| 216 | 2026-09-12 | Dalga 0 son 4 kırık: B5+B3+A2+A5 | ✅🧪 | C11 |
| 217 | 2026-09-12 | Dalga 0 mühürleme (doğrulama + kayıt, kod yok) | ✅🧪 | C11 |
| 218 | 2026-09-12 | 6.71/4-D1 bildirim arş. + ShowTagged | ✅🧪 | C11 |
| 219 | 2026-09-12 | Fluent+Mica tam geçiş + Kural 14 + Faz 6.75 | 📋 karar | C11 |
| 220 | 2026-09-12 | 6.71/4-D2 VM gruplama kapanışı | ✅🧪 | C11 |
| 221 | 2026-09-12 | 6.75 araştırması (Mica+backdrop) | 📋 bulgu | C11 |
| 222 | 2026-09-12 | 6.75 ilk view Splash Mica geçişi | ✅🧪 | C11 |
| 223 | 2026-09-12 | 6.75 ikinci view SistemKurulum | ✅🧪 | C11 |
| 225 | 2026-09-12 | Token tek-anahtar süpürmesi | ✅🧪 | C11 |
| 226 | 2026-09-12 | Kapanış + devir notu (kod yok) | 📋 devir | C11 |
| 227 | 2026-09-13 | Fluent token geçişi + Splash/SistemKurulum view turu + Faz 6.76 Adım 1 | ✅🧪 | C12 |
| 228 | 2026-09-13 | Faz 6.76 Adım 3: SistemKurulum→SistemDbYonetim rename (geriye dönük kayıt) | ✅🧪 | C12 |
| 229 | 2026-09-13 | Faz 6.76 Adım 2+4+5: KurulumSplash + içerik daraltma + 3 yol navigasyon | ✅🧪 | C12 |
| 230 | 2026-09-13 | Login zinciri tema onarımı (beyaz-beyaz + ayarlanabilir tema) | ✅🧪 | C12 |
| 231 | 2026-09-13 | SistemDbYonetim sayfa yeniden yazımı (migration/onarım) | ✅🧪 | C12 |
| 232 | 2026-09-13 | DatabaseInfoPanel iki kart tek satır | ✅🧪 | C12 |
| 233 | 2026-09-13 | Faz 6.77 planı: SistemDb yönetim operasyonları | 📋 plan | C12 |
| 234 | 2026-09-13 | Faz 6.77 Adım 1: ApplyPending yedek-önce-göç zinciri | ✅🧪 | C12 |
| 235 | 2026-09-13 | Faz 6.77 Adım 2: sayfa yönetim bölümleri + VM zinciri | ✅🧪 | C12 |
| 236 | 2026-09-13 | Faz 6.77 Adım 3: QuickDialog kaldırıldı | ✅🧪 | C12 |
| 237 | 2026-09-13 | Faz 6.78 planı: ortak restore analizi | 📋 plan | C12 |
| 238 | 2026-09-13 | Gerçek Mica tek kaynak (Base + pane şeffaflığı) | ✅🧪 | C12 |
| 239 | 2026-09-13 | Kuyruk kapanışı: yedek-panel + Adım 0 + 6.78 Adım 1 | ✅🧪 | C12 |
| 240 | 2026-09-13 | Canlı tur: yedek-panel tasarım-uyum denetimi | ✅🧪 | C12 |
| 241 | 2026-09-13 | Faz 6.79: Sistem.db açılış/kapanış güvenliği + Faz 6.80 kuralı | ✅🧪 | C13 |
| 242 | 2026-09-13 | Faz 6.78 Adım 2: tenant hattı ortak restore çekirdeğine bağlandı | ✅🧪 | C13 |
| 243 | 2026-09-13 | Faz 6.78 Adım 3: sistem restore tek kapı (altyapı + Kural-17 zemin + UI) | ✅🧪 | C13 |
| 244 | 2026-09-13 | Denetim Masası FirmaShell'den ayrıldı → ayrı modal pencere (Kural 14+17) | ✅🧪 | C13 |
| 245 | 2026-09-13 | Kural 18: canlı testi agent yapar, kanıt kullanıcı onayına sunulur | 📋 kural | C13 |
| 246 | 2026-09-13 | Kural 19: referans defteri (araştırma → nereye uygulandı) | 📋 kural | C13 |
| 247 | 2026-09-13 | Canlı test devri: Denetim Masası turu yeni oturumda | 📋 devir | C13 |
| 248 | 2026-09-13 | Login pilotu: Fluent 2 + ana border + MVVM yardım + logo | ✅🧪 | C13 |
| 249 | 2026-09-13 | Kural 17 Katman 2: FirmaShellView + Denetim Masası Windows Ayarlar yeniden tasarımı (KISMİ) | ✅🧪 | C13 |
| 250 | 2026-09-13 | Oturum 249 devam 2: Veritabanı tek view + token/tema bugfix + canlı Light/Dark | ✅🧪 | C13 |
| 251 | 2026-09-13 | Denetim Masası Giriş: Win11 karşılaştırma → RichButton + rol/busy/canlı güncelleme denetimi | ✅🧪 | C13 |
| 252 | 2026-09-14 | Sahte transfer uyarısı + dialog tema/tek buton + DEV işareti + çift açılma engeli | ✅🧪 | C13 |
| 253 | 2026-09-14 | Faz 6.80 Katman 2 turu: ana border + hairline + dialog opaklığı + Light/Dark canlı | ✅🧪 | C13 |
| 254 | 2026-09-14 | Ana tasarım dili MÜHÜR + Faz 6.83 (TenantDatabaseUpdateView turu) | 📋 kural | C13 |
| 255 | 2026-09-14 | FirmaShellView seçim deneyimi: araştırma + Faz 6.84 planı | 📋 plan | C13 |
| 256 | 2026-09-14 | Kullanıcı Yönetimi ayrı faz (6.85, modal) + FirmaShellView dashboard yönü — karar + devir | 📋 devir | C13 |
| 257 | 2026-09-14 | Kaldığı yer tespiti + loglanmamış 6.84 başlangıcının kaydı (kurtarma/log, kod yok) | 📋 kayıt | C13 |
| 258 | 2026-09-14 | Mühür revizyonu (Light hairline beyaz) + Login form sol hizası | ✅🧪 | C13 |
| 259 | 2026-09-14 | Login üst hizası düzeltmesi + beyaz çizgi onayı | ✅🧪 | C13 |
| 260 | 2026-09-14 | 6.84 kod sayımı + kalanlar raporu (kod yok) | 📋 rapor | C13 |
| 261 | 2026-09-14 | 6.84 tamamlama: 6 madde + rozet + hyperlink + 10 test | ✅ kod 🧪❌ | C14 |
| 262 | 2026-09-14 | Tema kısayolu: başlıkta 3 segmentli hap | ✅ kod 🧪❌ | C14 |
| 263 | 2026-09-14 | PİLOT: dönem listesi stok stile alındı (kart devrede değil) | 🔨 pilot | C14 |
| 264 | 2026-09-14 | PİLOT devamı: satır büyütme + aralık açma | 🔨 pilot | C14 |
| 265 | 2026-09-14 | PİLOT iptal: kart stiline tam dönüş | ✅ revert | C14 |
| 266 | 2026-09-14 | Tema kısayolu v2: kayar anahtar + ikonlar | ✅ kod 🧪❌ | C14 |
| 267 | 2026-09-15 | Dönem seçim stili: mavi tam-çerçeve kalktı + sol hub + dönen kuyruk | ✅🧪 | C14 |
| 268 | 2026-09-15 | Seçili kart = ana kart rengi (içeride) + hover token + ana border 1.5px; animasyon sınıfları silindi | ✅🧪 | C14 |
| 269 | 2026-09-15 | Faz 6.84 onayı + Faz 6.82 Dev-mode tamamlandı | ✅🧪 | C14 |
| 270 | 2026-09-15 | CircleIconButtonStyle dark tonu: ana border yüzeyiyle uyum | ✅🧪 | C14 |
| 271 | 2026-09-15 | Faz 6.86 Chunk-1: durum çubuğu + StatusMessage/StatusBar refactor | ✅🧪 | C14 |
| 272 | 2026-09-15 | Faz 6.86: durum çubuğu ana pencereye (MainShellView) + aktif firma/mali dönem/tenant bağlamı | ✅🧪 | C14 |
| 273 | 2026-09-15 | 6.87 v1 reddi + Faz 6.90: Velopack git güncelleme akışı (dinamik repo + gömülü varsayılan FeedUrl + admin/dev-mode) | ✅🧪 | C14 |
| 274 | 2026-09-16 | Güncelleme altyapısı: derin sektörel araştırma + 6.91-A/B/C (veri yolu `%AppData%`, zorunlu ön-yedek, ileri-uyumluluk guard) | ✅🧪 | C14 |
| 275 | 2026-09-16 | Faz 6.91-D: güncelleme sonrası doğrulama sagası + `GuncellemeSonrasiView` (soldan sağa adım şeridi) | ✅🧪 | C14 |
| 276 | 2026-09-16 | AI Yardım Asistanı Faz 6.92 Adım 1-6 ✅🧪 (UI drawer'a taşınıyor — 277'ye devir) | 📋 devir | C14 |
| 277 | 2026-09-16 | Faz 6.91-D Revizyon 3: hero + 3 adım + Atlandı + B1/B2 (`PostUpdatePending`) + Light/Dark `ThemeResource` düzeltmesi + çapraz-model build açma | ✅🧪 | C14 |
| 278 | 2026-09-16 | Faz 6.92 Adım 5c (Model yönetimi UI) + Faz 6.91-E (dönem güncelleme sayfası silindi → erişimde onay+inline göç, manuel Güncelle butonları kaldırıldı) + Faz 6.93 plan/sözleşme devri | ✅🧪 | C14 |
| 278b | 2026-09-16 | Faz 6.93 plan/sözleşme (AI yardım bilgi tabanı `AsistanBilgi.db` + hibrit RAG): araştırma 5 satır + `YARDIM-DB-PLAN.md` frozen + motor/veri diğer modele devir (`📨`) | 📋 plan | C14 |
| 279 | 2026-09-16 | Faz 6.93 Adım 1 motor (sözleşme birebir: KB + RRF + depo + Foundry embedding + retrieval swap) — build 0 + 33/33 + full 640/641 (tek kırmızı 6.73'ün, dokunulmadı) | ✅🧪 | C14 |
| 281 | 2026-09-17 | Görev devri (280): A) 6.93 motor fix (izin kapısı + tek Foundry kurulumu) build 0 + 634/634; B) yetki bug araştırması + 3-katman yaklaşım onayı (kod yok) | ✅🧪 | C14 |
| 280 | 2026-09-17 | Faz 6.93 Adım 2 (eski RAG v1 sökümü + DI + panel dizin durumu + Denetim kartı + öz-test) · 6.91-G (saga 4. adım `AI Yardım Dizini` + 4 adımlı şerit) · 6.73 testi düzeltildi (641/641) — canlı S1 ✅, S2 motor bug'ına takıldı (devir) | ✅🧪 | C14 |
| 282 | 2026-09-17 | Faz 6.93 Adım 3 canlı S2-S4 (anlamsal retrieval + embedding'siz fallback + içerik güncelle→yeniden indeks) + Denetim "Yardım dizini" DI wiring fix + sohbet paneli dürüst model durumu/flyout ön-yükleme + popup Kural 17 tema hizalaması — build 0 + 640/640; popup onayı ✅ | ⏳ onay | C14 |
| 283 | 2026-09-17 | Faz 6.94 planı (Tek Yardım Yüzeyi = AI Asistanı): view `?` yardım kaldırılır, tek kaynak `docs/yardim` → `AsistanBilgi.db` → asistan (F1 + statü çubuğu) + H1-H5 maddeleri — **kod yok** | 📋 plan | C14 |
| 284 | 2026-09-17 | Temiz yapı + 3 plan (kod yok): **6.85** Kullanıcı Yönetimi+RBAC (`KULLANICI-YONETIMI-PLAN.md` K1-K6) · **6.95** Aktivasyon&Modül Kilidi+tenant şema (`AKTIVASYON-MODUL-PLAN.md` A1-A6) · **6.96** Çoklu DB Provider/SQLite varsayılan (`DB-PROVIDER-PLAN.md` D1-D8) + DURUM açık iş envanteri + ROADMAP bayat fix (6.73/6.83/6.87) + kararlar + **Kural 8 onayı** | 📋 plan | C14 |
| 285 | 2026-09-17 | Faz 6.85 **K1** (RBAC temeli): `RolPermission` HasData (Yönetici 76 + Kullanıcı 16) + idempotent migration `RbacRolPermissionSeed` + `KullaniciRolSabitleri`/`PermissionVarsayilanlari` · firma oluşturana Yönetici KFR + açılış backfill (`SistemRbacBackfill`) · `PermissionService` Yönetici bypass · `Permission.cs` yorum fix — build 0 + **646/646**; dev DB kopyasına migration kanıtı | ✅🧪 | C14 |

---

