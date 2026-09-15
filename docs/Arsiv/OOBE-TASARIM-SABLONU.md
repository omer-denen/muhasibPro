# MuhasibPro — OOBE Tasarım Şablonu (Login/Splash/SistemKurulum görsel dili)

> **WebToXaml'a BAKILMAZ.** `muhasibpro-WebToXaml` (özellikle `SistemKurulum`) dark/diagnostik referanstır ve çekirdek ekranlar için **kullanılmaz**. Bu şablon, Login/Splash/SistemKurulum OOBE görsel dilinin TEK kaynağıdır. Yeni bir ekran/panel bu şablona göre yazılır; soru varsa bu dosyaya bak, kullanıcıya tekrar sorma.

## Temel ilkeler
1. **Arka plan:** sayfa `Background="{StaticResource AppBackgroundBrush}"` — `Assets/Images/background_muhasib.png` (warm greige ImageBrush, DesignTokens; Oturum 102'de mavi `app_background.png` yerine geçti).
2. **Hardcode renk YASAK** (AGENTS kural 3): istenen token yoksa önce `Styles/DesignTokens.xaml`'a ekle. `Foreground="White"`/`Background="White"` yerine `{StaticResource MuhasibOnPrimaryBrush}` kullan.
3. **Buton ölçüsü/duygusu TEK kaynak `Styles/Buttons.xaml` (Oturum 103):** `Button` üzerinde `Padding/CornerRadius/Background/Foreground/BorderThickness` YAZILMAZ — base (`InventDark/Secondary/Ghost/Toolbar/Plain`) veya merkezi varyant (`PrimaryCompact/SecondaryCompact/GhostLarge/GhostSmall/GhostTint*/NavActive/Link`, chrome'da `InventDialog*`) seçilir. İstisna: VSM/code-behind sürümlü (segment/sekme) + unik ikon geometrisi + Faz B placeholder.
3. **Kart stilleri TEK kaynak `Styles/Cards.xaml`:** **Ana kartlarda `ModernCard` (Login referans: `CardBackgroundFillColorDefaultBrush` + `CardStrokeColorDefaultBrush` + 12 radius + `CardPadding`; `Translation 0,0,24` + `ThemeShadow`) TEK kaynak.** `MuhasibCardStyle` yardımcı kartlarda kalabilir. `DesignTokens.xaml` **`<Style>` içermez**. **KESİN KOMUT (2026-08-31): bundan sonraki tüm ana kartlar `ModernCard` ile yazılacak.**
4. **Düzen tercihi:** çekirdek ekranlar **çok-panel** düzen kullanır (tek ortalanmış kart sarmalı değil), kullanıcı aksini söylemedikçe. Login/Splash tek kartlıdır (zaten tek akış), SistemKurulum/FirmaShell çok panellidir.

## Token'lar (DesignTokens.xaml)
- Light+Dark `ThemeDictionaries`; semantic renkler dict'lerde, fırçalar `{ThemeResource}` ile tema-reaktif.
- Birincil üstü (beyaz) metin/ikon/ring: **`MuhasibOnPrimaryBrush`** (Light+Dark `#FFFFFF`).
- Hover/pressed: `MuhasibPrimaryHoverBrush` / `MuhasibPrimaryPressedBrush`.
- Yüzeyler: `MuhasibCardBackgroundBrush`, `MuhasibCardBackgroundSubtleBrush`, `MuhasibBorderBrush`, `MuhasibTextPrimary/Secondary/TertiaryBrush`.

## Buton (Styles/Buttons.xaml)
- **`MuhasibPrimaryButtonStyle`** = birincil buton (Kurulumu Başlat, Giriş Ekranına Git, Kaydet vb.): özel `ControlTemplate` (Border root + ContentPresenter), `Normal/PointerOver/Pressed` VisualState'leri → `MuhasibPrimaryHoverBrush`/`MuhasibPrimaryPressedBrush`. `Foreground = MuhasibOnPrimaryBrush`.
- **`Button.Resources` ile `ButtonBackgroundPointerOver` override'ı GÜVENİLMEZ** (ContentDialog `RequestedTheme` scope'unda beyaza düşüyor) → her zaman `MuhasibPrimaryButtonStyle` kullan. (HATALAR.md — 2026-08-31)
- Disabled state tanımlanmaz → buton mavi kalır (içinde beyaz ring görünür, ör. Yedek Al).

## Gölge deseni
- `ThemeShadow x:Name="..."` + element `Translation="0,0,24/32"`.
- Receiver (`Receivers.Add`) **`Loaded`/`OnPageLoaded` içinde, try/catch** — **ASLA ctor'da** (splash pencere-görünmez regresyonu; HATALAR.md).
- Örnek: `LoginCardShadow.Receivers.Add(RootGrid)`, `HeaderShadow.Receivers.Add(RootGrid)`.

## SistemKurulum ekranı (özel düzen — çok panel)
- **Üst bar (Row 0, sabit, scroll dışı):** `MuhasibCardBackgroundBrush` + alt çizgi `MuhasibBorderBrush`; `Padding "32,52,140,16"` (top 52 = title bar, right 140 = pencere kontrol tuşları). İçerik: marka logo tile (BloomBlue→Sky gradyan + AppIcon, BrandBanner deseni) + başlık/subtitle + durum pill (`MuhasibCardBackgroundSubtleBrush`/`MuhasibBorderBrush`) + aksiyon butonu `MuhasibPrimaryButtonStyle`. Gölge: header'a ThemeShadow (Translation 32, receiver RootGrid).
- **Scroll içeriği (`MaxWidth 960`, ortalanmış):**
  - `DatabaseInfoPanel` (kendi bileşeni): **Sistem Durumu kartı** (ikon + StatusMessage + progress pill + "Kurulumu Başlat" butonu + ilerleme barı + **Veritabanı Dosya Yolu TextBox en altta**) → başlık "Global Veritabanı & Depolama Alanı" (GÜNCEL pill YOK) → **2-kolon grid: [Veritabanı İşlemleri] [Sistem Testleri]**.
  - **İşlem Günlüğü** kartı (`MuhasibCardStyle` + `MuhasibTerminalBgBrush` terminal + `MuhasibMonospaceFontFamily`).
- Veritabanı İşlemleri kartı içeriği: "Analiz Et" butonu (outline) + "Veritabanı Güncel" (success) + "Kurulum Gerekli" (danger) + DbExists metni (boyut/versiyon/bileşen satırı).

## Login (tek kart, referans desen)
- `AppBackgroundBrush` zemin + ortalanmış `ModernCard` + ThemeShadow + Translation 24; içinde 3-kolon (marka | divider | form). `BrandBannerControl` (logo tile + marka), `NamePasswordControl`, `QuickLoginPanel`. Giriş butonu `MuhasibPrimaryButtonStyle`'a alınabilir.

## Splash
- `AppBackgroundBrush` zemin + ortalanmış 460px gölgeli kart (ModernCard + ThemeShadow + Translation 32, receiver `OnPageLoaded`). Progress + fade + sürüm.

## Panel dili (Recurra — 2026-09-05, YÜRÜRLÜKTEN KALKTI)
> 2026-09-05 (Oturum 90) kararı: ana dil **InventEase**'dir. Bu bölüm yalnız geçiş bitene dek referanstır; yeni iş InventEase'e göre yazılır.
- Referans: Recurra satış paneli (düz opak zemin + beyaz geniş-radius kartlar + hap kontroller). Mica/cam/widget-masonry YOK.
- **Zemin:** `MuhasibBackgroundBrush` opak (Light `#F3F7FF`). Sayfa kökü saydam yapılmaz.
- **Kart:** `PanelCardStyle` (Border; card bg + border token, radius `MuhasibPanelCornerRadius` 20, padding 20, **gölgesiz flat**). İç mini kutu: `PanelMiniStyle` (subtle bg, radius 12, padding 14).
- **Kart gruplama kuralı (kullanıcı kararı):** ilişkili işlemler tek kartta (marka + form bir bütün); ek/yan işlevler ayrı kartta (durum, hızlı giriş, listeler kendi kartı).
- **Kontroller:** inputlar hap (`PanelMiniStyle` + pill radius sarmalı, iç TextBox transparent/borderless); birincil buton hap (pill radius, `MuhasibPrimaryButtonStyle`); durum hapları ince çerçeveli pastel (mevcut Success/Warning/Danger/Info token'lar).
- **Üst bar + adımlar:** logo + hap + statik adım göstergesi (komutsuz, yalnız görüntü). Gizli menü (`...`) YOK — tüm aksiyonlar görünür buton. Yeni fonksiyon eklenmez, yalnız stil değişir.
- Token'lar `DesignTokens`'ta, stiller `Cards.xaml`'da; hardcode yasak (tanım dışı `#...` 0).

## Güncel dil — Custom* + warm (TEK GEÇERLİ, Oturum 99-102 kararı)
- **Kök:** `Background="{ThemeResource AppBackgroundBrush}"` (`background_muhasib.png` warm resim).
- **Kartlar (`Styles/Cards.xaml`):** `CustomModernCard` ana kart; `CustomCompactCard` iç/input kartı; `CustomGlassPanel` buz taşıyıcı (sayfa başına TEK); `CustomElevatedCard` gölgeli hero. `Style=` her zaman `StaticResource`.
- **Renkler (Light+Dark):** petrol marka/vurgu (`MuhasibPetrolBrush`, koyu uç `MuhasibPetrolDeepBrush`, tint/border); zeytin rozet/sayaç (`MuhasibOlive*`); ink birincil buton (`MuhasibInkBrush` + hover/pressed basamakları); metin/border `MuhasibText*/MuhasibSoftBorderBrush`. Hardcode `#` yasak.
- **Butonlar (`Styles/Buttons.xaml`):** sayfada ölçü/renk yazılmaz — `MuhasibPrimaryButtonStyle` (birincil), `PetrolButtonStyle`/`PetrolTintButtonStyle` (vurgu), `Ghost*` (satır ikonları), `LinkButtonStyle` (metin-içi), dialogda setters-only `InventDialog*` stilleri. `DefaultButton="Primary"` YASAK.
- **Header yasağı sürer:** sayfa-içi başlık bloğu yok; sistem kromu tek yetkili.
- **İkonlar:** `Styles/Icons.xaml` glyph haritası (`IconAdd/IconEdit/IconRefresh` vb.) — yeni ikon önce oraya.

## InventEase frost dili (TARİHÇE — 2026-09-05 mührü, Oturum 99-102'de Custom* dile geçildi)
- Aşağıdaki §51-62 frost-panel ("buz üstünde beyaz hub") dili artık uygulanmaz; yeni iş yukarıdaki Güncel dile göre yazılır. Referans duruyor (eski ekranları okurken bağlam için).
- Referans: `EkranKaydi/` InventEase dashboard PNG'si (yalnız tasarım dili, fonksiyonellik alınmaz). Piksel palet: petrol `#0F6C7A`, lime `#EAEE5B`/`#DEEA74`, zemin `#E7EAEE→#F4F4E6`, kart `#FFFFFF`, ink `#141414`, yazı `#101010`, soluk `#E1E7E9`.
- **Zemin:** `InventEaseBackgroundBrush` (3-stop gradyan, soğuk sol-üst → sıcak sağ). Light+Dark karşılıklı. Sayfa kökü bu gradyan; opak `MuhasibBackgroundBrush` sayfa zemininde kullanılmaz.
- **Kart-için-kart ("buz üstünde beyaz hub"):** içerik kartlarının tamamı **TEK buzlu acrylic kap** içinde (`InventFrostPanelStyle` + `InventFrostBrush`, pale tint 0.55); panel başına ayrı buz YOK (çift-blur yasak) — iç taşıyıcılar saydam. Beyaz kartlar (`InventCardStyle`, radius 24; iç kart 16) buz üstte yüzer. Ayraç çizgisi yok — kartlar `Spacing 12` ile ayrılır. Bölüm başlıkları small-caps (`CharacterSpacing 80`, sage tertiary). Animasyonlu/canlış metin buz üstünde çıplak durur (beyaz hub yok).
- **Header yasağı:** sayfa-içi header/titlebar bloğu YOK (`ShellTitleBar` silindi); sürükleme + pencere tuşları sistem kromunda. Login üst barı (logo + adım hapları) header değil akış göstergesidir, korunur.
- **Hap kuralı:** buzlu/zemin üstü = beyaz kart; **beyaz kart içi = soluk hap** (`InventPillMutedStyle`). Beyaz-üstüne-beyaz hap yasak (görünmez).
- **Aksiyon dili:** birincil = siyah hap (`MuhasibPrimaryButtonStyle` artık ink: hover/pressed ink basamakları, hap radius, metin + `ChevronRight` önerilir); petrol = marka/vurgu (logo, link, hint, avatar, seçili-satır tinti — küçük elemanlarda koyu uç `MuhasibPetrolDeepBrush`); küçük kart-içi aksiyonlar dolu/tint petrol (`PetrolButtonStyle` birincil + `PetrolTintButtonStyle` ikincil — saydam ghost küçük aksiyonda kullanılmaz); zeytin = ikinci vurgu (rozet/sayaç/grafik dolgusu: `MuhasibOliveTintBrush` zemin + `MuhasibOliveBorderBrush` çerçeve + `MuhasibOliveDeepBrush` metin). Satır aksiyonları `GhostPillButtonStyle` (petrol hover tinti + petrol halka) yalnız tablo-satırı ikonlarında. Semantik (Success/Warning/Danger/Info) aynen korunur.
- **Dialog chrome butonları (Oturum 96 kuralı + Oturum 103 kalıbı = QuickDialog):** `ContentDialog` chrome'u — `Background=CardBackgroundFillColorDefaultBrush` + `Border=CardStrokeColorDefaultBrush 1` + `CornerRadius=CardCornerRadius 16`; gövde dış `CustomModernCard Padding 12`; Primary/Close butonlarında SADECE setters-only stiller (`InventDialogPrimary/Danger/Info/SecondaryStyle` — şablonlu stil chrome'da düşer); **`DefaultButton="Primary"` YASAK** (WinUI default butona accent basıp stili ezer); tehlikeli dialogda `DefaultButton="Close"` korunur (Enter=iptal güvenli varsayılan), güvenli dialogda Enter=onay code-behind'de (`EnterOnay` bayrağı emsali). `Style=` her zaman `StaticResource` (`ThemeResource` ile stil verilemez).
- **Lime spec (token ilk kullanımda eklenir — ölü token yasağı):** kapasite segment `#DEEA74`, CTA `#EAEE5B`, üstü yazı `#101010`; sparkline çizgi petrol + dolgu `#C5DFDD`.
- **Gölge (yüzen ana kartlar, opsiyonel):** yumuşak `ThemeShadow` + `Translation 0,0,32`, receiver `Loaded`+try/catch (Splash emsal). Buz üstü iç kartlarda gerekmez.
- **Eski stil haritası (Faz 6.42):** `PanelCardStyle`/`ModernCard` → `InventCardStyle` alias; `MuhasibCardStyle` → iç kart (16); `PanelMiniStyle` → soluk iç kutu; `ElevatedCard` → Invent tokenlar; `GlassPanel` → frost; `CompactCard` silinir.
- **Rollout sırası:** Splash → paylaşılan stiller → SistemKurulum → FirmaShell → MaliDonemYönetim → MainShell → Controls → Dialoglar → süpürme (mavi 0 + `#` 0 + ölü 0).

## Güncelleme kuralı
- Bu şablona dokunulursa (yeni kart stili, buton deseni, ekran düzeni) **güncelle**: oturum sonunda bu dosyayı düzenle + `LOG`/`KONTROL-LISTESI`'ne işle.
