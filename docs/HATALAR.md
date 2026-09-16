# MuhasibPro — Hata Günlüğü

Format: `## Başlık` → Belirti / Sebep / Çözüm / Tarih

## `SettingsExpander` varsayılan kapalı gelince liste/aksiyon görünmüyordu ("iğne araması") (ÇÖZÜLDÜ - 2026-09-16 - Oturum 278)
- **Belirti:** Denetim "Model yönetimi" kartında `SettingsExpander` başlık + disk özetini gösteriyor, ama indirilmiş model satırı ve `Sil` butonu canlıda görünmüyordu (UIA'da `Sil` yok). Kullanıcı yönetim listesini görmek için expander'ı elle açmak zorundaydı.
- **Sebep:** Toolkit `SettingsExpander.IsExpanded` varsayılanı **false**; `ItemsSource` satırları yalnız açılınca realize edilir (Kural 16: "kritik bölüm gömülü kalmaz / iğne araması tasarım bug'ı").
- **Çözüm:** `IsExpanded="True"` verildi; liste ve satır aksiyonları ilk açılışta görünür. Ders: yönetim listesi taşıyan `SettingsExpander` varsayılan açık başlar; alt-ayar/ileri düzey expander'larda kapalı kalabilir.
- Tarih: 2026-09-16

## Canlı test betiğinde ContentDialog onayı sayfa butonuyla karıştı (isimle arama) (ÇÖZÜLDÜ - 2026-09-16 - Oturum 278)
- **Belirti:** `Uygula` sonrası açılan `ShowConfirmationAsync` dialogu, betik `Name='Uygula'` eşleşmesiyle **sayfadaki** butonu tıkladığı için kapanmıyordu; işlem başlamamış gibi göründü.
- **Sebep:** Dialog hem metniyle hem sayfa butonuyla aynı ada sahip; ağaçta ilk/Son eşleşme sayfa butonuna denk geldi.
- **Çözüm:** ContentDialog butonları **AutomationId** ile seçilir: `PrimaryButton` (onay) / `SecondaryButton` (vazgeç) — dump ile doğrulandı. Ders: dialog etkileşiminde isim değil AutomationId kullan.
- Tarih: 2026-09-16

## Canlı test betiğinde Türkçe metin bozuk gitti (BOM'suz .ps1 → PS 5.1 ANSI okur) (ÇÖZÜLDÜ - 2026-09-16 - Oturum 277)
- **Belirti:** UIA ile AI modeline sorulan soru ("mali dönem nasıl arşivlenir?") modele bozuk (mojibake) karakterlerle ulaştı; canlı streaming testinin girdisi geçersizdi (cevap değil, **girdi** bozuk).
- **Sebep:** PowerShell 5.1, BOM'suz `.ps1` dosyalarını ANSI (Windows-1254) olarak okur; `write` aracı BOM'suz UTF-8 yazar → script içindeki Türkçe string sabitleri bozulur (`ValuePattern.SetValue("...")`).
- **Çözüm:** Türkçe metni dosya kodlamasından bağımsız kur (`"mali d" + [char]0x00F6 + "nem nas" + [char]0x0131 + "l ar" + [char]0x015F + "ivlenir?"`), **veya** panoya yazıp (`Set-Clipboard`, UTF-16) kutuya Ctrl+V ile yapıştır, **veya** betiği UTF-8 **BOM'lu** kaydet. Element/ailesi adı eşleşmelerinde de Türkçe literal yerine ASCII/indeks kullan.
- Tarih: 2026-09-16

## `Progress<T>` kuyruğundaki geç bildirimler monotonik guard'a takılıp adım rozetleri terminal duruma gelmiyordu (ÇÖZÜLDÜ - 2026-09-16 - Oturum 277)
- **Belirti:** Güncelleme sonrası doğrulamada Sistem.db adımı hatayla bitmesine rağmen adım rozeti dönmeye devam ediyor, ondan sonraki adım "Bekliyor" kalıyordu (hero doğruydu).
- **Sebep:** `Progress<T>.Report` mesajı asenkron post eder; saga senkron ilerlediğinde bildirimler `CalistirAsync` dönene kadar kuyrukta bekler. VM `ApplyResult` içinde `ProgressValue = 100` yazınca kuyruktaki tüm bildirimler (`Yuzde < 100`) monotonik guard'a takılıp atılıyordu → adım görünümleri hiç terminal duruma geçmiyordu.
- **Çözüm:** `ApplyResult`, adım rozetlerini `sonuc.Adimlar`'dan **doğrudan** yazar (canlı ilerleme yalnız çalışma sırasında); `ProgressValue=100` kuyruktaki geç bildirimleri güvenle etkisiz kılar. Ders: terminal durumu geç bildirime bırakma — sonucu kaynaktan yaz.
- Tarih: 2026-09-16

## Converter'da `Application.Current.Resources` tema-bağımsız brush çözüyordu (Light'ta koyu zemin) (ÇÖZÜLDÜ - 2026-09-16 - Oturum 277)
- **Belirti:** Kırmızı/amber hero ve adım rozetleri Light temada da koyu kalıyor, koyu metin koyu zemin üstünde okunmuyordu (Dark doğruydu).
- **Sebep:** Renk converter'ları brush'ı `Application.Current.Resources[...]` ile çözüyordu. Tema pencere kökünde `rootElement.RequestedTheme` ile uygulanıyor; `Application.Current.RequestedTheme` (sistem teması) değişmediğinden `ThemeDictionaries` **sistem** temasına göre çözülüyordu.
- **Çözüm:** Renk converter'ları kaldırıldı (`PostUpdateAdimDurumuConverter`, `PostUpdateSonucTuruConverter`); durum renkleri View'da **tema-farkında `ThemeResource` overlay Border'ları** + bool görünürlüklerle seçiliyor. Ders: tema-bağımlı brush `ThemeResource` ile element üzerinde uygulanır; converter'da `Application.Current.Resources` ile çözülmez.
- Tarih: 2026-09-16

## Çok-modelli çalışma: eşzamanlı 6.92 dosyaları derlemeyi bloke ediyordu (ÇÖZÜLDÜ - 2026-09-16 - Oturum 277)
- **Belirti:** 6.91 Revizyon 3 doğrulamasında build, başka modelin aktif yazdığı Faz 6.92 dosyalarındaki eksik `using`/API hatalarıyla (`ObservableObject`, `ISurumOzellikService`, `DataContextChangedEventArgs.OldValue`) düştü.
- **Sebep:** Aynı çalışma kopyasında iki model paralel; 6.92 UI dosyaları yarım durumdaydı.
- **Çözüm:** Kullanıcı onayıyla yalnızca build'i açan minimal dokunuş yapıldı (eksik using'ler + `DataContextChanged` abone takibi + yinelenen using temizliği); 6.92 tasarımına/akışına müdahale edilmedi. Ders: sahiplik sınırı korunur, yalnız derleme açacak asgari düzeltme yapılır ve LOG'a "çapraz-model build açma" olarak işlenir.
- Tarih: 2026-09-16

## `edit_file` çok-satırlı eşleşme CRLF dosyada tutmaz (ÇÖZÜLDÜ — 2026-09-16 — Oturum 276)
- **Belirti:** `LoginViewModel.cs` (karışık CRLF/LF) üzerinde çok-satırlı `find` "no exact match" ile düştü; LF dosyalarda aynı desen çalıştı.
- **Sebep:** Eşleşme bayt-bayttir; `\n` ile yazılan find, CRLF satır sonlarıyla eşleşmez.
- **Çözüm:** CRLF/karışık dosyada tek-satır find kullan ya da dosyayı tümden oku+yaz (LF; `text=auto` + index LF olduğundan `git diff` temiz kalır) + BOM'u koru. Ders: düzenlemeden önce `file` + `git show HEAD:` ile satır-sonu/BOM karşılaştır.
- Tarih: 2026-09-16

## `Progress<T>` alt-ilerleme bildirimi terminal adım durumunu/mesajını eziyordu (ÇÖZÜLDÜ — 2026-09-16 — Oturum 275)
- **Belirti:** Güncelleme sonrası doğrulama ekranında iş bitmiş olmasına rağmen durum satırı "Dönemler taranıyor..." olarak kalıyordu (adım rozetleri ise terminal/yeşil).
- **Sebep:** `IProgress<T>.Report` (Progress<T>) mesajı **asenkron post** eder; tenant alt-ilerlemesi iç içe `Progress<double>` ile ikinci bir post turu oluşturuyordu → gecikmeli "devam ediyor" bildirimi, sonradan gelen terminal bildirimini **geçip** mesajı geriye sardı (sıra bozuldu).
- **Çözüm:** (a) Alt-ilerleme için senkron `IlerlemeKopru : IProgress<double>` + `Mesaj = string.Empty` (yalnız çubuk beslenir; durum metni adım başı/başlangıç mesajında kalır), (b) VM'de monotonik guard: `adim.Yuzde < ProgressValue` ise stale rapor atlanır; terminal adım "DevamEdiyor"a dönmez. Canlıda doğrulandı (`ot275_b_08.png`). Ders: ilerleme bildirimini tek post turunda tut; terminal durumu geri saran geç bildirimlere karşı monotonik guard koy.
- Tarih: 2026-09-16

## İleri-uyumluluk guard'ı özyineleme riski taşıyordu — sürüm okuması fallback'li metottan (ÇÖZÜLDÜ — 2026-09-16 — Oturum 274)
- **Belirti:** 6.91-C guard'ı disk sürümünü `GetCurrentDatabaseVersionAsync()` ile okuyordu; bu metot kayıt yoksa `GetSistemDatabaseStateAsync()`'e düşüyor → guard tekrar çağrılıyor → **sonsuz özyineleme (yığın taşması)** riski. Canlıya çıkmadan, uygulama sırasında fark edildi.
- **Sebep:** Ortak metot "kayıt yoksa state'e düş" fallback'i içeriyordu; guard bu metodu kullanınca çağrı yolunun içine geri döndü.
- **Çözüm:** Guard ve `InternalInitializeAsync` sürümü **fallback'siz** `ReadStoredSistemVersionAsync()` ile doğrudan okur (`GetCurrentDatabaseVersionAsync` public kaldı). Ders: bir guard/kontrol, çağrılma yolunun içine dönen fallback'li metodu kullanmamalı.
- Tarih: 2026-09-16

## Muhasebe verisi Velopack app kökünde tutuluyordu — uninstall veriyi siliyordu (ÇÖZÜLDÜ — 2026-09-16 — Oturum 274)
- **Belirti:** RELEASE veri yolu `%LocalAppData%\MuhasibPro\Databases`; Velopack `%LocalAppData%\{AppId}` (app kökü) **uninstall'da silinir** ve binary klasörü `current` her güncellemede değişir → kaldırma/servis müdahalesi muhasebe verisini götürür.
- **Sebep:** `ApplicationPaths.GetAppDataFolderPath` `SpecialFolder.LocalApplicationData` kullanıyordu; Velopack app köküyle çakıştı.
- **Çözüm:** Kök **`%AppData%\MuhasibPro`** (Roaming) yapıldı + tek seferlik taşıma (`IDataPathRelocationService`, açılışta, fail-closed) + eski yolu taşıyan ölü `Paths/*` provider'lar silindi. Canlı kanıt: sentetik eski kurulum taşındı, Login açıldı (`ot274_reloc_login.png`).
- Tarih: 2026-09-16

## Ön-yedek planı "göç gerektiren dönemler"i güncelleme ÖNCESİ belirleyemez (TASARIM DÜZELTMESİ — 2026-09-16 — Oturum 274)
- **Belirti:** Plan, güncelleme öncesi yalnız "göç gerektiren dönem DB'leri"nin yedeklenmesini öngörüyordu.
- **Sebep:** Hangi dönemin göç gerektireceği **yeni migration'lara** bağlı; yeni migration'lar yeni binary'de olduğundan güncelleme öncesi bilinemez.
- **Çözüm:** Güncelleme öncesi yalnız Sistem.db yedeklenir; dönem yedekleri göç anında (mevcut Yedek→Göç saga'sı) + post-update taramada (6.91-D) alınır. Ders: güncelleme öncesi bilinemeyen bir kümeyi plan şartı yapma; bilgi ancak yeni sürüm çalışırken elde edilir.
- Tarih: 2026-09-16

## Durum çubuğu yalnız `ShellView`'de — ana pencere akışında hiç görünmüyor (AÇIK — MİMARİ NOT — 2026-09-15 — Oturum 271)
- **Belirti:** Yeni Fluent durum çubuğu (`ShellStatusBar`) DetailsWindow'larda görünüyor ama ana pencerede (FirmaShell/MainShell) hiç görünmüyor.
- **Sebep:** `ShellStatusBar` yalnız `ShellView` içinde. Ana pencere akışı `ShellView`'i kullanmıyor: `SplashNavigator` → `MainFrame.Navigate(LoginView)`, `LoginViewModel.EnterApplication` → `NavigationService.Navigate<FirmaShellViewModel>(...)` yani **doğrudan `FirmaShellView`**. `ShellView` yalnız `NavigationHelper.SetupContentAsync` → `frame.Navigate(typeof(ShellView), args)` ile **DetailsWindow**'larda kullanılıyor. (Chunk-1'den önce de böyleydi.)
- **Çözüm (bekliyor):** Karar — (a) ana pencere akışı `ShellView` üzerinden geçsin (durum çubuğu uygulama geneli görünür) veya (b) durum çubuğu DetailsWindow-only kalsın. Kullanıcı Oturum 271: "şimdilik not düş, sonra karar". Böylece `ShellViewModel`'in DB durum beslemesi de yalnız DetailsWindow shell'lerinde çalışıyor.
- Tarih: 2026-09-15

## `IsSistemDatabaseConnection` hiç `true` olmuyordu — sistem DB göstergesi daima gizli (ÇÖZÜLDÜ — 2026-09-15 — Oturum 271)
- **Belirti:** Durum çubuğundaki sistem veritabanı göstergesi hiç görünmüyordu.
- **Sebep:** `StatusBarService.IsSistemDatabaseConnection` yalnız `SetSistemDatabaseStatus` ile set ediliyor; bu metotun çağıranı yoktu (`Startup.InitializeSistemDatabase` ölü koddu, `DatabaseConnectionMessage` hep null → bölüm `Collapsed`).
- **Çözüm:** `ShellViewModel` ctor'una `ISistemDatabaseService` eklendi; `LoadAsync` navigasyonu geciktirmeden `GetSistemDatabaseStateAsync` ile **gerçek durum** `SetSistemDatabaseStatus`'a yazılıyor (Kural 7). Canlı kanıt: `ot290b_statusbar_*.png` → "Sistem veritabanı bağlı" + yeşil nokta; DB bool `true`. Ayrıca `ShellViewModel`'deki `UserName` çift-atama bug'ı düzeltildi (önce kullanıcı adı, sonra ad-soyad yazılıyordu).
- Tarih: 2026-09-15

## `StatusMessageService` dispatch fiilen ölü — `Initialize` 0 çağıran (ÇÖZÜLDÜ — 2026-09-15 — Oturum 271)
- **Belirti:** Durum mesajları UI dışı iş parçacıklarından gelirse çapraz-iş parçacığı riski; ama pratikte dispatch hep "null → satır içi çalıştır" yoluna düşüyordu.
- **Sebep:** `_dispatcherQueue` yalnız `Initialize(object dispatcher)` ile atanıyordu; bu metot 0 çağırandı → `_dispatcherQueue` hep null.
- **Çözüm:** `StatusMessageService`/`StatusBarService` dispatch tek kaynaktan `App._dispatcherQueue` (null veya UI iş parçacığında satır içi, değilse `TryEnqueue`) kullanıyor; ölü `Initialize` üyeleri silindi (Kural 4).
- Tarih: 2026-09-15

## Opak pencere-tabanı dolgusu dark'ta "siyah nokta" yapar (ÇÖZÜLDÜ — 2026-09-15 — Oturum 270)
- **Belirti:** `CircleIconButtonStyle` (tüm `?` butonları) Dark'ta ana border üstünde neredeyse siyah bir daire gibi görünüyordu; kullanıcı: "çok siyah oldu, tema rengiyle uyumlu olsun, göze batmasın".
- **Sebep:** 269'da yarı saydamlık sorununa çözüm olarak seçilen opak `SolidBackgroundFillColorBaseBrush` **Dark'ta `#202020`** (pencere tabanı) değerini verir; ana border yüzeyi ise görsel-tintli `CardBackgroundFillColorSecondary` (Dark'ta ≈`#333A3E`) olduğundan buton zeminden belirgin koyu kalıyordu. Opaklık gerekliydi (arkadaki 1.5px çizgi görünmesin) ama taban rengi yanlıştı.
- **Çözüm:** Butona özel **tema-farkında opak** token (`DesignTokens.xaml`: `MuhasibCircleIconArkaplanColor` Light `#F3F3F3` / Dark `#333A3E`; hover `#3F464B`, pressed `#2A3033`) — Dark değeri ana border yüzeyiyle hizalı. Kural: üstüne binen opak kontrol, altındaki yüzeyin tema rengiyle eşleşmeli (kör sistem tabanı seçilmez). Canlı kanıt: `ot289e_firmashell_dark.png` + `ot289f_btn_dark.png` (buton `#333A3E` ≈ çevre `#383F43`).
- Tarih: 2026-09-15

## Yarı saydam sistem dolgusu üst üste binen kontrolde arkadaki border'ı gösterir (ÇÖZÜLDÜ — 2026-09-15 — Oturum 269)
- **Belirti:** `?` yardım butonu ana border köşesine bindiğinde (Login/FirmaShell/Güncelleme) ana border'ın 1.5px çizgisi butonun arkasından net görünüyordu.
- **Sebep:** `CircleIconButtonStyle` dolgusu `ControlFillColorDefaultBrush` — WinUI sistem kontrol dolgusu **yarı saydamdır** (`#B3FFFFFF` gibi ~%70). Üstüne bindiği XAML içeriği (ana border çizgisi) dolgudan geçip görünür. `CardBackgroundFillColorDefaultBrush` de yarı saydamdır; `Transparent` ise hiç kapatmaz.
- **Çözüm:** Üstüne binen kontrolde **tam opak** sistem dolgusu kullan: `SolidBackgroundFillColorBaseBrush` (normal) / `SolidBackgroundFillColorTertiaryBrush` (hover) / `SolidBackgroundFillColorSecondaryBrush` (pressed). Kural: bir kontrol başka bir yüzeyin (border/çizgi) üstüne biniyorsa `Card*`/`ControlFillColor*` yerine `SolidBackgroundFillColor*` kullan. Canlı kanıt: `ot288_yardim.png` + `ot288_login.png`.
- Tarih: 2026-09-15

## Dev araçları Installation modülüne konulamaz — mimari bekçi kırmızısı (ÇÖZÜLDÜ — 2026-09-15 — Oturum 269)
- **Belirti:** `dotnet test` → `ArchitectureTests.Kurulum_Ile_SistemDb_Yonetimi_Ayrik` kırmızı; `Services/Installation` taraması "DatabaseServices" metnini yasaklıyor, yeni `DevAraclariService.cs` ihlal olarak bulundu.
- **Sebep:** Kurulum modülü (kimlik + global ayar) tenant/sistem servislerine yaslanamaz (Kural 10 + bekçi). Geliştirici araçları kimlik/transfer **teşhis** işi yaptığı için `ITenantSQLiteDatabaseService` gerektiriyor; sınıf yanlış klasöre konunca bekçi yakaladı.
- **Çözüm:** `IDevAraclariService` + `DevAraclariService`, `Contracts/SistemServices/DevServices` + `Services/SistemServices/DevServices` altına taşındı (bekçi kapsamı dışı, yaslanma yönü doğru). Ders: yeni sınıf `Installation`/`SistemDatabaseService`/`TenantDatabaseService` klasörlerine konmadan önce hangi modüllere yaslandığı kontrol edilir (bekçi kırmızısı derleme hatası hükmündedir).
- Tarih: 2026-09-15

## StaticResource tema fırçası — çalışma-zamanı tema geçişinde beyaz-beyaz (ÇÖZÜLDÜ — 2026-09-14 — Oturum 253)
- **Belirti:** Uygulama Dark açılıp Denetim Masası'ndan Light'a geçilince `FirmaShell` sol firma kartı beyaz-beyaz oldu ("KAYITLI FİRMALAR", "Korkut Mermer", "Yetkili/İletişim" okunmaz); açılıştan Light başlatınca sorun yoktu.
- **Sebep:** `FirmalarListControl` (22 nokta) + `UserInfoControl` + `NavSidebarControl` + `AnimatedInfoBorder` tema fırçalarını `{StaticResource TextFillColor*/SystemFillColor*}` ile alıyordu; `StaticResource` **load-time** çözülür ve Dark değerine kilitlenir — canlı tema değişiminde güncellenmez (Oturum 230 dersinin FirmaShell çocuklarındaki tekrarı).
- **Çözüm:** İlgili tüm tema fırçaları `{ThemeResource}`'a çevrildi (4 dosya); ayrıca aynı süpürme `TenantDatabaseUpdateView`/`UpdateView`/`MainShellView`'da yapıldı. Canlı kanıt: runtime switch sonrası `ot253h_firmashell_light.png` okunur; açılıştan Light `ot253i_firmashell_light.png`. Kural: tema-bağımlı fırça **kullanım yerinde** `ThemeResource` (TASARIM-KURALLARI).
- Tarih: 2026-09-14

## Gömülü modda zemin gizleme — KokZemin tüm içeriği sarıyor (ÇÖZÜLDÜ — 2026-09-14 — Oturum 253)
- **Belirti:** Denetim Masası → Güncelleme sayfası (gömülü `UpdateView`) tamamen **boş** açıldı (yalnız kabuk başlığı görünüyor).
- **Sebep:** Katman-2 dönüşümünde `UpdateView`'un kök `KokZemin` Border'ı hem zemin katmanını (Image + perde) hem de tüm içeriği sarıyor; `GomuluUygula` "zemin gizle" niyetiyle `KokZemin.Visibility=Collapsed` yapınca içerik de gizlendi.
- **Çözüm:** Gömülü modda yalnız zemin **katmanları** gizlenir (`ZeminGorseli` + `ZeminPerdesi` x:Name'leri); panel + içerik görünür kalır. Ders: kök Border'a Visibility ile "zemin gizleme" yapılmaz — katman ayrı adlandırılır.
- Tarih: 2026-09-14

## Sahte "Taşınmış Veri" uyarısı — kurulum kimliği yeniden üretildi (ÇÖZÜLDÜ — 2026-09-14 — Oturum 252)
- **Belirti:** Uygulama her açılışta "Taşınmış Veri Tespit Edildi" dialogu gösteriyordu; kullanıcı aynı makinede publish→uninstall sonrası bunun neden çıktığını sordu.
- **Sebep:** Kurulum kimliği `LocalSettings.json` + Sistem.db `GlobalAyarlar` aynasında tutuluyor; Oturum 251'de pilot tema temizliği için `LocalSettings.json` silinince ayna da yoktu → `KurulumKayitService.GetOrCreateAsync` **yeni bir `KurulumId` üretti** (`8ae5…`, 13.09 22:24:34). Dönem damgaları eski kimlikteydi (`9e67…`) ve `MakineId` aynıydı; `TenantVersionReader` makine/kurulum ayrımı yapmadan "farklı kurulum" saydı → sahte transfer alarmı.
- **Çözüm:** `CheckTransferAsync` ayrımı — makine farklı → gerçek transfer (dialog + olay); makine aynı → kimlik kaybı, sessiz onarım: tek eski kimlik `UpdateKurulumIdAsync` ile damgadan geri alınır (eski yedeklerle uyum), karışık kimlik `ReAlignTenantKurulumIdsAsync` ile güncel kimliğe eşitlenir. Uyarı dialogu yalnız DEBUG. Canlı kanıt: GlobalAyarlar + LocalSettings `9e67…`'ye döndü, dialog çıkmadı (`ot252_*`).
- Tarih: 2026-09-14

## ContentDialog açık temada dark açılıyor — popup katmanı tema mirası (ÇÖZÜLDÜ — 2026-09-14 — Oturum 252)
- **Belirti:** OS Dark + uygulama Light iken transfer dialogu Dark (akrilik koyu) açıyordu; içerik fırçaları `ThemeResource` olmasına rağmen.
- **Sebep:** `ContentDialog` popup katmanında render edilir; kök `Frame`'e yazılan `RequestedTheme` (tema servisi) popup'a miras gitmiyor — dialog `Application.RequestedTheme`/sistem temasına düşüyor. `DialogHelper` yalnız XamlRoot atıyordu, tema yazmıyordu.
- **Çözüm:** `DialogHelper.ApplyAppTheme(dialog)` — `IThemeSelectorService.Theme` runtime'dan `dialog.RequestedTheme`'e yazılır; `ShowCenteredAsync` + `DialogService.CreateDialog` aynı kapı. XAML'de hardcode yok (kural korunur). Canlı kanıt: `ot252_transfer_dialog.png` açık tema.
- Tarih: 2026-09-14

## PowerShell toplu metin değişimi XAML encoding'ini bozar — Türkçe mojibake (ÇÖZÜLDÜ — 2026-09-13 — Oturum 251 devam)
- **Belirti:** 9 XAML dosyasında tek satırlık perde değişimi sonrası `git diff` tüm dosyayı değişmiş gösterdi; Türkçe karakterler `BaÅŸlÄ±k`, `VeritabanÄ±`, em-dash `â€”` oldu; dosya başına BOM eklendi.
- **Sebep:** `Get-Content -Raw` (BOM'suz UTF-8 dosyayı Windows-1254 olarak okur) + `Set-Content -Encoding UTF8` (BOM'lu yazar) roundtrip'i çift-encoding üretti. Edit tool encoding'i korur; script'li replace korumaz.
- **Çözüm:** `git checkout --` ile 9 dosya geri alındı; değişiklik `edit` tool ile dosya başına uygulandı. Kural: XAML/metin dosyalarında toplu değişim script'le YAPILMAZ (edit tool) veya `[IO.File]::ReadAllText/WriteAllText(UTF8Encoding($false))` kullanılır.
- Tarih: 2026-09-13

## Denetim Masası Giriş'te firma "0 dönem" — dönem projeksiyonu eksik (ÇÖZÜLDÜ — 2026-09-13 — Oturum 251)
- **Belirti:** Giriş sayfası firma kartı "0 dönem" gösteriyordu; aynı firma FirmaShell'de 3 dönemli (`2025/2026/2027`).
- **Sebep:** `FirmaListelemeService.GetFirmalarWithUserId` `CreateFirmaModelAsync` sonrası `MaliDonemler` listesini modele **haritalamıyordu** (sorgu dönemleri getiriyordu; `GetFirmalarPageAsync` haritalıyordu, bu yol atlanmıştı).
- **Çözüm:** `model.MaliDonemler = FirmaServiceExtensions.ToHafifDonemListesi(item.MaliDonemler)`; canlı kanıt: "3 dönem • 2025, 2026, 2027" (`ot251l_home.png`).
- Tarih: 2026-09-13

## Paralel Sistem.db analizi — paylaşımlı singleton context çakışması (ÇÖZÜLDÜ — 2026-09-13 — Oturum 251)
- **Belirti:** Giriş sayfası Sistem.db kartı "Kontrol gerekli" döndü; log: `SistemMigrationManager: Database analizi başarısız` + `Failed executing DbCommand ... __EFMigrationsHistory` (1 ms).
- **Sebep:** `ISistemDatabaseService`/`ISistemMigrationManager` **Singleton** ve tek `SistemDbContext` paylaşılıyor; analiz, güncelleme denetimiyle `Task.WhenAll` ile **paralel** koşunca eşzamanlı context erişimi komutu düşürdü (eşzamanlı ikinci analiz kaynağı logla kesinleşmedi; kanıt: paralel sürüm düştü, sıralı sürüm düzeldi).
- **Çözüm:** `GirisDashboardViewModel.YukleAsync` sıralı `await` (UI kilitlenmez; ring'ler ayrı bayrakta). Kural: Sistem.db analizi/operasyonları paralel çağrılmaz. Canlı: `ot251k`/`ot251l` "Bağlı • Güncel • 1,04 MB".
- Tarih: 2026-09-13

## NavigationView'de seçim vurgusu düşüyor — kaynak yenileme + veri kaynaklı SelectedItem (ÇÖZÜLDÜ — 2026-09-13 — Oturum 251)
- **Belirti:** Denetim Masası'nda başlık "Giriş" olmasına rağmen sol nav'da hiçbir madde seçili görünmüyordu (UIA: 7 maddede de `IsSelected=False`).
- **Sebep:** `MenuItemsSource` + `MenuItemTemplate` ile veri kaynaklı `NavigationView`'de başlangıç `SelectedItem` görsel seçime dönüşmüyor; üstüne `GorunurMenuleriTazele` her çağrıda `Clear` + aynı örnekleri yeniden ekliyordu → görsel seçim sıfırlanıyordu (aynı örnek olduğu için binding yeniden atamıyor).
- **Çözüm:** VM: liste yalnız gerçekten değiştiyse `Clear` (`SequenceEqual`); View: `NavSeciminiAynala` — aynı örnekte dahi `null → hedef` atamasıyla tazeleme (`SeciliMenu` bildiriminde de). Canlı UIA: `Giriş=True`, pill + accent çubuk görünür.
- Tarih: 2026-09-13

## Seed yönetici rolü boş — kullanıcı kartında rol satırı yok (ÇÖZÜLDÜ — 2026-09-13 — Oturum 251)
- **Belirti:** Denetim Masası kullanıcı kartında ad vardı, rol satırı boştu (VM "Yerel Hesap" fallback'ine düşüyordu).
- **Sebep:** Seed'de firma-rol (KFR) satırı üretilmez (firma-bağımlı); `AuthenticationService.CreateKullaniciModel` `Rol`'ü yalnız KFR varsa dolduruyordu — bootstrap "seed yöneticidir" kuralı sadece yetki denetiminde (`AyarYetkiDenetimi`) vardı.
- **Çözüm:** `AuthenticationService` seed yöneticisi (`KullaniciSabitleri.SeedYoneticiId`) için `Rol = Yönetici` sentetik model (yetki denetimiyle aynı kural); VM fallback'i "Kullanıcı". Canlı: pane "Yönetici" (`ot251l_home.png`).
- Tarih: 2026-09-13

## Geçersiz Fluent renk anahtarları — XamlParse çöküşü (ÇÖZÜLDÜ — 2026-09-13 — Oturum 249 devam)
- **Belirti:** Denetim Masası paneli açılırken pencere **siyah/boş** kalıyor; log'da `XamlParseException: Cannot find a Resource with the Name/Key TextFillColorTertiaryColor`. Ayrıca Mali Dönem Yönetim ayar dialogunda aynı aileden `ControlFillColorSecondaryColor`.
- **Sebep:** Oturum 225 "token tek-anahtar süpürmesi" `MuhasibTextTertiaryColor` → `TextFillColorTertiaryColor` yazmış; WinUI'de doğru Color anahtarı `TextFillColorTertiary`'dir (`...Color` soneki yok; brush `TextFillColorTertiaryBrush`). Aynı süpürme `ControlFillColorSecondary` → `ControlFillColorSecondaryColor` bozmuş. Derleme yakalamaz (ThemeResource lazy), çalışma anında patlar.
- **Çözüm:** `ToolBar.xaml` → `TextFillColorTertiary`; `YonetimAyarlarDialog.xaml` → `ControlFillColorSecondary`. Kural: sistem renk anahtarları `generic.xaml`'den doğrulanır (Color vs Brush ayrımı), ezberden `...Color` yazılmaz.
- Tarih: 2026-09-13

## Tema ayarı sahte — kullanıcı-bazlı kaydediliyor, açılışta global okunuyor (ÇÖZÜLDÜ — 2026-09-13 — Oturum 249 devam)
- **Belirti:** Denetim Masası → Görünüm → Tema = "Açık" seçilip kaydediliyor (`LocalSettings.json` içinde `AppPlatformSettings:U5413300800.ThemeDefault="Light"` kanıtlı) ama uygulama açılışta ve oturumda **Dark** kalıyordu (Light hiç yansımıyor). Oturum 249 notundaki "Light render yansımadı" bulgusunun kök nedeni.
- **Sebep:** İki katmanlı uyuşmazlık: (1) `AppPlatformSettingsProvider.Anahtar()` giriş yapılmışken `...:U{id}` döndürür; ama `ThemeSelectorService.LoadPlatformDefaultAsync()` **açılışta** (giriş öncesi) `AppPlatformSettings` **global** anahtarını okur — orada değer yok → `Default` → sistem (Dark). (2) `AppPlatformAyarlarViewModel.ThemeDefault` setter'ı `IThemeSelectorService`'i çağırmaz → canlı uygulama yok.
- **Çözüm:** `ThemeSelectorService` artık `IEventBus.Subscribe<AppSettingsChangedEvent>` ile `AppPlatformSettings.SettingsKey` olayını dinler; kayıtta platform temasını okuyup `SetThemeAsync` ile **canlı** uygular (tüm pencereler) ve `AppBackgroundRequestedTheme` global anahtarına yazar → **açılışta da** okunur. Canlı kanıt: Görünüm'de "Açık" seçilince pencere anında Light; yeniden başlatmada Light korunuyor (`ot249f_after_light.png`, `ot249g_denetim_startup.png`). Build 0/0, test 481/481.
- Tarih: 2026-09-13

## Açılış yedeği + kapanış WAL aktarımı hiç inşa edilmemişti (ÇÖZÜLDÜ — 2026-09-13 — Oturum 241)
- **Belirti:** Kullanıcı sordu: "açılışta Sistem.db yedekliyor musun, kapanışta WAL dosyalarını DB'ye aktarıyor musun?" — ikisi de YOK.
- **Sebep:** `EnsureWeeklyBackupAsync` yalnız arşiv/LOG kaydında kaldı (kodda yok); `WeeklyBackupDays`/`HaftalikButunlukKontrolu` modelleri 0 tüketici; kapanış kolu (`TryTakeExitBackupAsync`) yalnız tenant'ı sarıyor + varsayılan kapalı — Sistem.db checkpoint/oto-yedek hattı hiç kurulmamıştı (Oturum 208 taramasında da "HİÇ İNŞA EDİLMEMİŞ" hükmü vardı).
- **Çözüm:** `ISistemYasamDongusuService` (açılış: WAL checkpoint + eşik-aşan oto-yedek; kapanış: checkpoint her zaman + ayarlıysa yedek) + `ISistemBackupManager.CheckpointWalAsync` + 2 App kancası (DbTest / WindowHelper) + 6 test. Eşik/limit modelden. Build 0 hata, test 468/468.
- Tarih: 2026-09-13

## Yedek limiti listede uygulanmıyordu — "3 yazıyor 5 listeli" (ÇÖZÜLDÜ — 2026-09-13 — Oturum 239)
- **Belirti:** Panel "En fazla 3 yedek saklanır" yazarken listede 5 yedek görünüyordu (canlı tur).
- **Sebep:** `SistemYedekViewModel.YukleIcAsync` limiti hiç uygulamıyordu — `CleanOldBackupsAsync` yalnız `YedekAlAsync` sonunda çalışıyordu. Limit Denetim Masası'nda düşürülünce (veya yedek birikince) liste limit üstü kalıyordu. Üstüne `YedekAlAsync` temizlik sonucunu okumuyordu (sessiz; Oturum 202 dersi aynı aile). Denetim Masası↔panel aynı provider/modeli kullanıyordu — tutarsızlık davranıştaydı, veride değil.
- **Çözüm:** Yüklemede limit-aşımı budanır + sonuç bildirilir (`Limit (3) aşıldığı için 2 eski yedek temizlendi` / temizlenemezse `Danger`); `YedekAlAsync` temizlik sonucunu okur. Regresyon: `Yukle_LimitAsilinca_Budar` + `YedekAl_TemizlikBasarisizsa_Uyarir`. Build 0 hata, test 462/462.
- Tarih: 2026-09-13

## Tanımsız CardCornerRadius — 10 dialogda XamlParse çöküşü (ÇÖZÜLDÜ — 2026-09-13 — Oturum 236)
- **Belirti:** Listeden seçimde `XamlParseException: Cannot find a Resource with the Name/Key CardCornerRadius` (CustomContentDialog üzerinden). Derleme yeşil, çalışma anında patlar.
- **Sebep:** `CardCornerRadius` anahtarı hiçbir sözlükte tanımlı değil (sistemde `Control/OverlayCornerRadius`, bizde `MuhasibCardCornerRadius` var). Ezberden yazım; 10 dialog chrome'unda aynı mayın (QuickDialog Oturum 230'da tekildi, tarama eksikti).
- **Çözüm:** 10 dosya `ControlCornerRadius`; çıplak `CardCornerRadius` grep 0 (kalanlar tanımlı `Muhasib*`). Kural pekişti: CornerRadius anahtarı sözlükten kopyalanır + yeni dialog eklenince chrome anahtarları grep'lenir. Build 0 hata, test 446/446.
- Tarih: 2026-09-13

## Yedek saati UTC — kullanıcı saatiyle uyuşmuyor (ÇÖZÜLDÜ — 2026-09-13 — Oturum 236)
- **Belirti:** Yedekleme saati kullanıcının saatinden 3 saat geride (TR UTC+3).
- **Sebep:** Görüntülenen tarihler UTC ile üretiliyordu (`DateTime.UtcNow`, `LastWriteTimeUtc`, `CreationTimeUtc`; tenant listede `LastAccessTimeUtc` — okuyunca bile değişir).
- **Çözüm:** Görüntüye çıkan 6 nokta local'e alındı (`Now`, `LastWriteTime`, `CreationTime`; sistem + tenant). DB'de saklanan kayıtlar (`OperationTime`, versiyon damgaları) UTC kaldı — doğru pratik. Kural: kullanıcıya gösterilen saat her zaman local, saklanan UTC.
- Tarih: 2026-09-13

## TwoWay seçim setter'ında Notify → StackOverflow çöküşü (ÇÖZÜLDÜ — 2026-09-13 — Oturum 236)
- **Belirti:** Yedek listesinde satıra tıklayınca `System.StackOverflowException` (stack izlenemez) + uygulama ölür. VS ile 3 yedek alındı, liste doldu, tıklama çökertti.
- **Sebep:** `SistemYedekViewModel.SeciliYedek` setter'ı `Set()` sonrası fazladan `NotifyPropertyChanged(SeciliYedek)` çağırıyordu (`YükleKomutDurumu`) — `SelectedItem TwoWay` ile ListView↔VM sonsuz gidip-gelme. `Set` zaten bildirir; setter içinde aynı property'e tekrar bildirim yasaktır.
- **Çözüm:** Fazladan bildirim + ölü `YükleKomutDurumu` silindi (grep 0). Kural: TwoWay bağlı property'nin setter'ında aynı property'e `NotifyPropertyChanged` YOK (`Set` yeter).
- Tarih: 2026-09-13

## Login zinciri beyaz-beyaz metin — StaticResource tema kilidi (ÇÖZÜLDÜ — 2026-09-13 — Oturum 230)
- **Belirti:** OS Dark iken login zincirinde tüm metinler görünmez (beyaz zemin + beyaz yazı).
- **Sebep:** Tema-bağımlı fırçalar kullanım yerinde `{StaticResource}` ile bağlanmıştı (~60 nokta) — StaticResource load-time kilitlenir (Dark beyazına), `{ThemeResource}` app temasına geçer (Light beyazı). MS kuralı: kullanım yerinde ThemeResource, ThemeDictionaries içinde StaticResource.
- **Çözüm:** Login zinciri sweep (Static→Theme + Muhasib alias→sistem: Petrol→AccentFill, Tint→AttentionBackground, Deep/Border→AttentionBrush) + tema ayarlanabilir (varsayılan sistemi takip, Denetim Masası'ndan Sistem/Açık/Koyu) + dialog Light zorlaması kalktı. Kural AGENTS.md'ye işlendi. Build 0 hata, test 436/436.
- Tarih: 2026-09-13

## QuickSistemDbDiagDialog tanımsız CardCornerRadius — dialog açılınca patlar (ÇÖZÜLDÜ — 2026-09-13 — Oturum 230)
- **Belirti:** `QuickSistemDbDiagDialog.xaml:12` `CornerRadius="{StaticResource CardCornerRadius}"` — bu anahtar hiçbir sözlükte tanımlı değil (sistemde Control/OverlayCornerRadius, bizde MuhasibCardCornerRadius var).
- **Sebep:** Ezberden yazım; derleme yakalamaz, çalışma anında XAML parse patlar (teşhis dialogu hiç açılamıyordu).
- **Çözüm:** `ControlCornerRadius` (sistem). Kural: CornerRadius anahtarları `DesignTokens`/sistem sözlüğünden kopyalanır, ezberden yazılmaz.
- Tarih: 2026-09-13

## KurulumSplash Kural 14 ihlali — araştırmasız view oluşturuldu (SÜREÇ İHLALİ — 2026-09-13 — Oturum 229)
- **Belirti:** KurulumSplash (yeni view+VM) araştırma yapılmadan kodlandı. Kod çalışıyor ama Kural 14 sırası ihlal edildi.
- **Sebep:** Hız baskısı — "zaten Splash tarzı basit bir sayfa" varsayımıyla araştırma atlandı, doğrudan kodlamaya geçildi. Kullanıcı uyardı.
- **Çözüm:** Geriye dönük araştırma yapıldı (QuickBooks/TallyPrime/Logo Tiger ilk kurulum deneyimi + MS Fluent progress kılavuzu). Bulgular mevcut kodu doğruladı (kod değişikliği gerekmedi) ama süreç ihlali kaydedildi. AGENTS.md Kural 14'e presedans eklendi: "Oturum 229 presedanı: KurulumSplash araştırmasız yazıldı, geriye dönük düzeltildi — tekrarı kabul edilmez."
- **Ders (AGENTS kuralı):** Kod araştırmadan ÖNCE yazılamaz — yeni view/sayfa veya mevcut view dokunuşunda araştırma → LOG → uygulama sırası zorunlu. Geriye dönük doğrulansa bile süreç ihlali hükmünde.
- Tarih: 2026-09-13

