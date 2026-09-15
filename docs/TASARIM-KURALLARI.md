# MuhasibPro — Tasarım Kuralları (Katmanlı Sayfa Yapısı)

> **KESİN KURAL (Oturum 241, kullanıcı; ana border + tint kararı Oturum 248):** Her yeni view/sayfa ve güncellenen her view bu yapıya uyar. Uymayan sayfa **önce bu yapıya taşınır**, sonra içerik eklenir. Bu doküman `AGENTS.md` Kural 17'nin ayrıntısıdır — çelişkide Kural 17 esastır.
>
> 🔒 **MÜHÜR (Oturum 254 — kullanıcı kararı):** Bu mimari **kilitlidir.** Bundan sonra oluşturulacak/güncellenecek **her View, UserControl ve Dialog** zemin → ana border (ana kart) → içerik kartları yapısı üzerine inşa edilir. Görünüm işlerinde kapsam yalnızca: **(a)** View içindeki kontrol yerleşimleri (elemanların yeri/düzeni), **(b)** **araştırma sonucu** (Kural 14) eklenen/çıkarılan buton ve kontroller. **Katman 2 ana border ve Katman 3 iç kart stilleri (renk/radius/gölge/padding/hairline/kart arası boşluk) mührün parçasıdır; habersiz değiştirilmez** — değişiklik ancak kullanıcı kararıyla (mühür revizyonu) yapılır.
>
> Kapsam: `MuhasibPro/Views/**/*.xaml` (Page + UserControl + ContentDialog dahil). Görsel dil **Windows 11 Fluent**'tir.

## Genel ilke

Sayfa = **zemin (statik)** + **ana border (ana panel)** + **kartlar (içerik)**. Katmanlar birbirine karışmaz: zemin sayfa ömrü boyunca değişmez; ana border içeriği çerçeveler; kartlar veriyle değişir.

---

## Katman 1 — Zemin

- Sayfanın **en dış elemanı tek `Border`** olur. Kök `Grid` + `Background` **yasak**.
- İçerik: `Image` (`Stretch="UniformToFill"`, kaynağı `UygulamaZeminGorseli` = **tek görsel `light.jpg`**) + üzerine **tema perdesi** (`MuhasibZeminPerdeBrush`, Light `#59F3F3F3` ≈%35 / Dark `#D9202020` ≈%85 — görsel görünür kalır). Ham/net resim gösterilmez.
- Zemin statiktir; sayfa içeriği değişince yeniden oluşturulmaz.

```xml
<Border x:Name="KokZemin" CornerRadius="0">
    <Grid>
        <Image Source="{ThemeResource UygulamaZeminGorseli}" Stretch="UniformToFill" />
        <Border Background="{ThemeResource MuhasibZeminPerdeBrush}" />
        <!-- Katman 2 -->
    </Grid>
</Border>
```

**Tek görsel + tema perdesi (Oturum 251):** `DesignTokens.xaml` → `UygulamaZeminGorseli` her iki temada `light.jpg`; dark görünüm perdeden gelir (iki ayrı zemin görselinden daha stabil kompozisyon).

---

## Katman 2 — Ana border (ana panel)

Zeminin üzerinde duran, **tüm içeriği kapsayan tek çerçeveli panel**:

| Özellik | Değer |
|---|---|
| `Background` | `{ThemeResource CardBackgroundFillColorSecondaryBrush}` |
| `BorderBrush` | `{ThemeResource MuhasibAnaBorderCizgiBrush}` (Light beyaz `#FFFFFFFF` — Oturum 258 mühür revizyonu, kullanıcı kararı / Dark yumuşak gri `#45C9CCD1`) |
| `BorderThickness` | `1.5` (Oturum 268 mühür revizyonu, kullanıcı kararı: "bir tık arttır") |
| `CornerRadius` | `{StaticResource OverlayCornerRadius}` |
| `Padding` | `24` |
| Elevation | `Translation="0,0,32"` + `ThemeShadow` (receiver `Loaded`/`OnPageLoaded` içinde try/catch — **asla ctor'da**) |

- Kenarlardan boşluklu; zemin görseli panelin çevresinde görünür kalır.

---

## Katman 3 — İçerik kartları

Her mantıksal blok (form, liste, özet, araç çubuğu) **ana border içinde ayrı `Border`** olur:

| Özellik | Değer |
|---|---|
| `Background` | `{ThemeResource CardBackgroundFillColorDefaultBrush}` |
| `BorderBrush` | `{ThemeResource CardStrokeColorDefaultBrush}` |
| `BorderThickness` | `1` |
| `CornerRadius` | `{StaticResource ControlCornerRadius}` (veya `OverlayCornerRadius`) |
| `Padding` | standart `16`; küçük kart `12`; büyük panel `20` |
| Elevation | `ThemeShadow` + `Translation` (Z); receiver `Loaded` + try/catch |

- Kartlar arası boşluk **sabit 12px** (`StackPanel Spacing`, `Grid ColumnSpacing/RowSpacing`).
- Gölge deseni: `ThemeShadow` receiver'ı `Loaded`/`OnPageLoaded` içinde try/catch ile eklenir — **asla ctor'da** (Splash pencere-görünmez regresyonu; `HATALAR.md`).

---

## Dialog zeminleri

- Dialog `Background`'ı **opak** `SolidBackgroundFillColorBaseBrush` olur.
- Yarı saydam `CardBackgroundFillColorDefaultBrush` dialog zemini olarak **kullanılmaz** (arkası sızar — Oturum 248 dersi).

---

## Renk kuralı

- Yalnız `{ThemeResource ...}` / `{StaticResource ...}` (Fluent anahtarları).
- Hardcode `#RRGGBB` **yasak** (yalnız `DesignTokens.xaml`/`ThemeDictionaries` tanımlarında).
- Temaya bağlı fırça kullanım yerinde `ThemeResource`'dur (`StaticResource` load-time kilitlenir — `HATALAR.md` Oturum 230); `ThemeDictionaries` içinde `StaticResource` (MS kuralı).

---

## Yasaklar

1. Kök seviyede `Grid` + `Background` — yalnız kök `Border`.
2. Ana border'sız doğrudan zemine kart.
3. İç içe kart — **max 2 seviye derinlik**.
4. View'dan view'a tutarsızlık — her sayfa aynı katmanları tekrarlar.
5. Hardcode renk, ham resim (tintsiz), `RequestedTheme` hardcode.

---

## Kapı (faz kapanmadan geçilmez)

- Yapıya uymayan view → "tasarımı bitmemiş" sayılır; `dotnet build` 0/0 olsa bile faz kapanmaz.
- Geçişte Kural 14 (view-öncesi içerik+tasarım araştırması) + Kural 8 (sınıf bazlı onay) + Kural 13 (? yardım dialogu) birlikte uygulanır.
- Doğrulama: `dotnet build --no-incremental` 0/0 + `dotnet test` + canlı Light/Dark + Mica.
- **Canlı testi agent yapar (Kural 18):** ekran görüntüsü + UIA + referans açıklaması kanıt olarak sunulur; "canlı test kullanıcıda" denerek geçilmez — kullanıcı yalnızca onaylar.

## Uygulama sırası (topolojik — `docs/VIEW-BAGIMLILIK.md`)

Çekirdek akış önce: Splash → Kurulum → Login → FirmaShell → Yönetim → Update → MainShell → alt paneller/dialoglar.
