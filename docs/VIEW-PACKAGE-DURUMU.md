# MuhasibPro — View Package Durumu

`muhasibpro-WebToXaml` tek görsel kaynak — web `src/components/*.tsx` → WinUI3 XAML dönüşümü.
Temel ilke **fonksiyon ↔ stil ayrımı**: mevcut fonksiyonellik (binding/komut/code-behind) korunur, yalnızca görünüm stil paketine çevrilir.

`MuhasibPro-master` ve `muhasibpro-WebtoWinui3-v2` 2026-08-28’de silindi — artık tek kaynak `muhasibpro-WebToXaml`.

| Paket | Durum | Not |
|---|---|---|
| 00-DesignTokens | ✅ Eklendi 🧪 | Bloom token'lar `Styles/DesignTokens.xaml` içinde hibrit (Oturum 15); Acrylic MSB3073 nedeniyle alınmadı |
| 01-Splash | ✅ Eklendi 🧪 | ExtendedSplash açık tema + radial accent (Oturum 16, görsel doğrulandı) — `WebToXaml/Splash/SplashView.xaml:19` referans |
| 02-SistemKurulum | 🔶 Hybrid | 3 kart iskelet + `SistemKurulumViewModel` bağlama (Oturum 17); `WebToXaml/SistemKurulum/` 4 metrik + terminal ile zenginleştirilecek |
| 03-Login | ✅ Eklendi 🧪 | `WebToXaml/Login/` 4 bileşen (BrandBanner+NamePassword+QuickLogin) 3-kolon dark card (Oturum 14/16, 2026-08-28 WebToXaml entegre) |
| 04-FirmaShell | ✅ Eklendi 🧪 | `FirmaShellView` OOBE (`docs/OOBE-TASARIM-SABLONU.md` TEK kaynak — WebToXaml'a bakılmaz). Oturum 69: seçim `SelectionMode="Single"`+VSM (tek-seçim garantisi), `FirmalarListControl` (`KAYITLI FİRMALAR` + `FirmaNo` gradyan tile + arama + düzenle), `MaliDonemlerListControl` (seçili firma şeridi + `ItemsWrapGrid` 2-kol dönem kartı: Veritabanı+boyut/Tablo-Kayıt/Son Yedek + Yedek/Arşivle/Sil + durum pill `Aktif Seçim/AÇIK/KAPALI`), `0 Dönem` fix (`GetFirmalarAsync` projeksiyon `MaliDonemler`) — build 0 hata, görsel resimle uyumlu |
| 05-MainShell | ⬜ Bekliyor | `WebToXaml/MainShell/` NavSidebarControl 240 + TitleBarControl + ShellStatusBar ile yenilenecek |
| 06-AccountingModules | ⬜ Bekliyor | Referans şablonlar dışarıda tutuldu — Aşama B'de modül modül eklenecek |

## Kurallar
- XAML'de `RequestedTheme` zorlanmaz; tema tek kanal `IThemeSelectorService` (varsayılan fallback Light)
- God-class kalıcı olarak yasak: View / code-behind / ViewModel / Service küçük ve tek sorumluluklu kalır; View'da büyüme ihtiyacı düzenli klasör yapısında UserControl ile karşılanır
- Web iskeleti (`vite`/`tailwind`/`package.json`) tekrar eklenmez; tek doğruluk kaynağı `dotnet build`
- Tek stil kaynağı `muhasibpro-WebToXaml` — `muhasibpro-WebtoWinui3-v2` 2026-08-28’de silindi
- Oturum 86: hover basamakları netleştirildi (Light hover `#094D8F`/pressed `#073A6C`, Ghost hover `PrimaryLight`) + eksik 8 tooltip eklendi (Login/FirmaShell/Dönem); token adları değişmedi, tüm ekranlar otomatik devraldı
- Oturum 87: Login tek logo (BrandBanner silindi, sürüm footer) + Ghost hover/pressed 1.5px Primary halka + `DonemYedeklerPanel` (Yedekler Arşiv'den Dönem'e taşındı, modern satırlar) + işlem kartları 4'lü 2×2; Arşiv sekmesi "Arşiv (N)"
- Oturum 88 (InventEase pilotu, WebToXaml dışı): Login yeni dilde — gradyan zemin + buzlu acrylic sol taşıyıcı + beyaz iç kartlar + siyah hap submit + teal vurgu; token ailesi `MuhasibTeal/Ink/Invent/Sage` (Light+Dark); rollout onayı bekliyor
- Oturum 96 (InventEase ANA DİL): dialog chrome butonları setters-only (`InventDialogPrimary/DangerStyle`, `DefaultButton=Primary` yasağı — accent ezmesi) + `FirmaDetails/FirmaCard` çevrildi + ölü `YeniFirmaDialog` silindi; WebToXaml referansı dialog/stil için kapandı, tek kaynak `OOBE-TASARIM-SABLONU.md`
