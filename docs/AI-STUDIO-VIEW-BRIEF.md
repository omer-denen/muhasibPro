# AI Studio — Tek-Tip Tasarım Brief'i (WinUI 3, Güncel Dil)

> **READ THIS FIRST:** Sen WinUI 3 **yeniden-giydirme (re-skin)** üreticisisin. `muhasibpro` çekirdek ekranları iki dilin karışımı durumda; işin hepsini **tek dile** çekmek. SADECE XAML üretirsin — ViewModel/Business/Data koduna dokunmazsın. Çıktı ayrı paket klasörüne (`viewpackage/Views/...`); `MuhasibPro/Views` üzerine yazmazsın.

---

## 1. Hedef Dil — Güncel (TEK GEÇERLİ, Oturum 99-102)

Tam spec: `docs/OOBE-TASARIM-SABLONU.md` ("Güncel dil" bölümü). Özet:

- **Kök:** `Background="{ThemeResource AppBackgroundBrush}"` (warm resim; sayfa köküne başka zemin konmaz).
- **Kartlar (`Styles/Cards.xaml`, `Style=` her zaman `StaticResource`):** `CustomModernCard` ana kart · `CustomCompactCard` iç/input kartı · `CustomGlassPanel` buz taşıyıcı (sayfa başına TEK) · `CustomElevatedCard` gölgeli hero.
- **Renkler (Light+Dark, fırçalar `ThemeResource` ile):** petrol marka/vurgu (`MuhasibPetrolBrush`, koyu uç `MuhasibPetrolDeepBrush`, `MuhasibPetrolTintBrush/BorderBrush`) · zeytin rozet/sayaç (`MuhasibOlive*`) · ink birincil (`MuhasibInkBrush` + hover/pressed basamakları) · metin/border (`MuhasibTextPrimary/Secondary/TertiaryBrush`, `MuhasibSoftBorderBrush`). **Hardcode `#...` YASAK.**
- **Butonlar (`Styles/Buttons.xaml` — sayfada ölçü/renk YAZILMAZ):** `MuhasibPrimaryButtonStyle` (birincil) · `PetrolButtonStyle`/`PetrolTintButtonStyle` (vurgu) · `GhostPillButtonStyle` + `GhostSmallButtonStyle` (satır ikonları) · `LinkButtonStyle` (metin-içi/link) · dialogda SADECE setters-only `InventDialogPrimary/Danger/Info/SecondaryStyle`.
- **Yasaklar:** sayfa-içi header/titlebar bloğu YOK · `RequestedTheme` YOK (tema kodda) · `DefaultButton="Primary"` YASAK (stili ezer; tehlikelide `Close` default) · `Style="{ThemeResource ...}"` YASAK (stil her zaman StaticResource) · hover/pressed'te ÖLÇÜ değişimi YOK (yalnız renk) · `...` gizli menü YOK.
- **İkonlar:** `Styles/Icons.xaml` haritası (`IconAdd/IconEdit/IconRefresh/IconTrash/IconSearch` vb.) — listede yoksa Unicode Segoe glyph + raporda not düş.

## 2. Okunacak Kaynaklar (yükleme listesi)

1. `docs/OOBE-TASARIM-SABLONU.md` — görsel dil TEK kaynak
2. `MuhasibPro/Styles/DesignTokens.xaml` + `Cards.xaml` + `Buttons.xaml` + `Icons.xaml` — KULLANILABİLİR anahtarların tamamı burada
3. `docs/AKIS-PLANI.md` — ekran akışı (Splash→Kurulum→Login→FirmaShell→Yönetim→MainShell)
4. `MuhasibPro/Views/**/*.xaml` (+ `.xaml.cs` — handler/adlar için) — giydirilecek mevcut ekranlar
5. `docs/WINUI-MIMARISI.md` — pencere/kapsayıcı kuralları

**YÜKLEME:** `docs/LOG/` ciltleri (tarihçe, ölü kararlar içerir — kafa karıştırır), `bin/`/`obj/`, `muhasibpro-WebToXaml` (SİLİNDİ, referans değil; özellikle SistemKurulum tasarımına bakılmaz).

## 3. Kapsam — Giydirilecek Ekranlar

| # | Mevcut Dosya | Tip | Korunacak Sözleşme |
|---|---|---|---|
| 1 | `Views/ShellViews/Splash/ExtendedSplash.xaml` | Page | progress/durum binding'leri |
| 2 | `Views/SistemKurulum/SistemKurulumView.xaml` + `Components/*` (DatabaseInfoPanel, SystemTestsPanel, KurulumKayitPanel) | Page + UserControl | Status/DbPath/Logs/TestSonuclari binding'leri |
| 3 | `Views/Login/LoginView.xaml` + `NamePasswordControl` + `QuickLoginPanel` | Page + UserControl | Username/Password/CanLogin/ErrorMessage/Hello/reveal binding'leri |
| 4 | `Views/ShellViews/Shell/FirmaShellView.xaml` + `Components/*` (FirmalarList, MaliDonemlerList, UserInfo) | Page + UserControl | Items/SelectedItem/DevamEt binding'leri |
| 5 | `Views/MaliDonem/Yonetim/*` + `MaliDonemYonetimView.xaml` | Page + UserControl | liste/pagination/yedek binding'leri |
| 6 | `Views/MainShell/*` + `Views/Settings/*` (UpdateView, DatabaseSettingsView) | Page | Nav + ayar binding'leri |
| 7 | `Views/**/Dialogs/*` (TransferDialog, RestoreVerifyDialog, DeleteGuard, YeniFirma/Donem, SagaPipeline) | ContentDialog | QuickDialog chrome kalıbı (Card bg + 16 radius + alt buton) |

## 4. Dönüşüm Kuralları (kritik)

1. **Sözleşme korunur:** tüm `x:Bind` binding'leri, `Click` handler adları, `x:Name` ve `AutomationId`'ler BİREBİR aynen kalır. Binding/handler taşımak YASAK — yalnız renk/stil/yerleşim değişir.
2. **300+ satır XAML → `UserControl`'e böl** (bağımsız görsel bloklar: liste, kart grubu, panel).
3. **Token yoksa UYDURMA:** mevcut anahtarlar dışında renk/stil kullanma; eksikleri `EKSIK-TOKEN.md` raporunda listele (biz `DesignTokens.xaml`'a ekleriz).
4. **Arayüz dili Türkçe.** Yeni metin eklenmez; mevcut metinler aynen korunur.
5. **ViewModel/Business/Data/Domain kodu YOK.** `*.xaml.cs` yalnız mevcut code-behind'in aynısı (gölge receiver `Loaded`+try/catch kuralıyla).

## 5. Çıktı

`viewpackage.zip` → `viewpackage/Views/...` (mevcut ağaçla BİREBİR aynı yollar, örn. `viewpackage/Views/Login/LoginView.xaml`).
Yanında `RAPOR.md`: ekran başına (a) değişen stil listesi, (b) EKSİK token istekleri, (c) taşınan/bölünen UserControl'ler.
`MuhasibPro/Views` üzerine DOĞRUDAN yazma — biz inceleyip seçerek alırız.

## 6. Yapılmayacaklar

- ViewModel/`Business`/`Data` kodu; `src/`/`vite`/`tailwind`/`package.json` web artıkları.
- `RequestedTheme`, `DefaultButton="Primary"`, hardcode renk, sayfa-içi titlebar, `Style={ThemeResource}`.
- WPF `Style.Triggers`; `Teal*` adlı eski tokenlar (yenisi `Petrol*`); silinmiş dosyalar (`ShellTitleBar`, `BrandBannerControl`, `TitleBarControl`, `ToastHostControl`).

## 7. Chat Prompt (AI Studio'ya aynen yapıştır)

```
Sen muhasibpro için WinUI 3 re-skin üreticisisin. Repo tamamı bağlı.
Hüküm sırası: 1) docs/OOBE-TASARIM-SABLONU.md "Güncel dil" bölümü (CustomModernCard/CustomCompactCard/CustomGlassPanel + warm zemin + petrol/zeytin + ink buton),
2) MuhasibPro/Styles/DesignTokens.xaml + Cards.xaml + Buttons.xaml + Icons.xaml anahtarları,
3) mevcut Views XAML'lerindeki x:Bind/Click/x:Name/AutomationId sözleşmesi.
docs/LOG ciltleri arka plan bilgisidir — oradaki geri alınmış/terk edilmiş kararları tasarıma taşıma.
Hedef dil: Güncel dil. Yalnız renk/stil/yerleşim değişir; hardcode renk yok (eksik tokenı RAPOR.md'ye yaz); 300+ satır UserControl'e bölünür; ViewModel kodu yok.
Çıktı: viewpackage.zip (viewpackage/Views/... mevcut ağaçla aynı yollar) + RAPOR.md (değişen stiller + eksik tokenlar + bölünen kontroller).
```
