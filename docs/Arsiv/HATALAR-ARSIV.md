# MuhasibPro - Hata Gunlugu ARSIV (eski kayitlar)

> Arsiv (2026-09-15, Oturum 271). Guncel kayitlar: docs/HATALAR.md

## GetByMaliDonemIdAsync kayıp satırda Success+null dönüyordu (ÇÖZÜLDÜ — 2026-09-12 — Oturum 212)
- **Belirti:** `MaliDonemServiceTests.GetById_Bulunamadi` kırmızısı (Oturum 211 test seferberliği yakaladı): kayıp id'de `Success=True, Data=null` (Windows: 425 geçti, 1 kaldı).
- **Sebep:** `MaliDonemService:55` `if (item == null)` dalı hiç tutmuyor — extension null değil `ErrorApiDataResponse` döner (`MaliDonemServiceExtensions:89-92`); üstüne aynı dalda `item.Message` NRE'liydi (null olsaydı patlardı).
- **Çözüm:** `item == null || !item.Success || item.Data == null` → Error (extension mesajı korunur, null-güvenli fallback'li). Çağıran taraması: hepsi `Success+Data` çift kontrollü veya `??` fallback'li — regresyon yok.
- Tarih: 2026-09-12

## Kayıt-dışı liste dönem detayına konulmaz (DERS — 2026-09-12 — Oturum 203)
- **Belirti:** "Silinen Dönem Yedekleri" + Bilinmeyen kartları seçili dönemin detay sayfasına konuldu; kullanıcı itirazı: bu veriler firma-seviyesi yetim veri, dönem detayıyla ilgisiz.
- **Sebep:** Kolay yerleşim tercihi; doman-model uyumu düşünülmedi.
- **Çözüm:** Kartlar sayfadan çıkarıldı; sekmeli `YetimYedeklerDialog` (silinen/bilinmeyen sekmeleri, mevcut paneller reuse) + navbar'da 2 sayaçlı buton. Kural: firma-seviyesi yetim veri dönem-detay sayfasına gömülmez, dialogda açılır.
- Tarih: 2026-09-12

## Yedek silme sonucu kontrol edilmedi — koşulsuz "Silindi" (ÇÖZÜLDÜ — 2026-09-12 — Oturum 202)
- **Belirti:** Yedek silinince "Yedek Silindi" bildirimi çıkıyor ama liste yenilenince dosya geri geliyor.
- **Sebep:** `DonemYedeklerViewModel:257` (`TemizleAsync:176` dahil) `CleanupBackupFileAsync` bool'unu okumuyordu; servis tüm hataları `false`'a gömüyor (`TenantBackupService:88-114`), sonraki `YukleAsync` diski doğru tarayıp silinmemiş dosyayı geri listeliyordu. Refresh kırık değildi — silme-sonuç kontrolü yoktu.
- **Çözüm:** bool kontrolü → başarısızda `Danger` ("dosya kilitli olabilir") + gerçek liste; çift-refresh sadeleşti. Regresyon: `YonetimSilmeYuklemeTests` 2 test. Kural: `bool` dönen silme çağrısının sonucu okunmadan bildirim verilmez.
- Tarih: 2026-09-12

## Kilitli yedek dönem silmeyi blokluyordu (ÇÖZÜLDÜ — 2026-09-12 — Oturum 202)
- **Belirti:** Yedekleri olan dönem silinemiyor (dialog inline hata, satır kalıyor).
- **Sebep:** `TenantDatabaseSagaStep:104-114` `CleanAllBackupsAsync` başarısız olunca tüm sagayı abort ediyordu (`BackupDeleteCompleted = deletedCount > 0`); `DeleteAllTenantBackup=false` yolu saga'da vardı ama dialog hep `true` gönderiyordu. Yan: ad-eşleşme case-sensitive (`:433`).
- **Çözüm:** DeleteGuard'a "Yedekleri de sil" checkbox (varsayılan işaretli); işaretsizse yedekler korunup "Silinen Dönem Yedeği" listesinde gösterilir (kimlik-id eşleşmesi, migration yok); abort mesajı kilitli dosya adlarını söyler; ad-eşleşme `OrdinalIgnoreCase`.
- Tarih: 2026-09-12

## Tüm bölümler gerçeğe geçince placeholder ölü kalır (ÇÖZÜLDÜ — 2026-09-12 — Oturum 201)
- **Belirti:** `YakindaViewModel/Sayfası` + Startup kaydı + `DenetimMasasiViewModel.Yakinda` routing Firma/Dönem gerçeğe geçince 0 caller'a düştü (test dışında).
- **Sebep:** Placeholder altyapısı son bölüm gerçeğe geçerken sökülmemiş.
- **Çözüm:** Kural 4 ile silindi (VM + Sayfa x2 + kayıt + prop + test; yerine `FirmaDonem_Cocuklari_Bagli` testi). Kural: son placeholder kalkınca çerçeve de silinir, "belki lazım olur" tutulmaz.
- Tarih: 2026-09-12

## IStatusMessageService Contracts.UIServices'te, CommonServices'te değil (ÇÖZÜLDÜ — 2026-09-12 — Oturum 201)
- **Belirti:** yeni testte `CommonServices.IStatusMessageService` CS0234 verdi.
- **Sebep:** `ICommonServices` CommonServices'te ama `IStatusMessageService` bir üst `Contracts.UIServices`'ta (`IStatusMessageService.cs:9`); `GuncellemeAyarTests` using'i üst paketten geliyordu.
- **Çözüm:** tam-ad `Business.Contracts.UIServices.IStatusMessageService`. Kural: `ICommonServices` üye tipi için interface dosyasının kendi namespace'i grep'lenir, üye-tutan interface'in paketi varsayılmaz.
- Tarih: 2026-09-12

## IUpdateService metodu SaveSettingsAsync, SaveAsync değil (ÇÖZÜLDÜ — 2026-09-12 — Oturum 201)
- **Belirti:** `UpdateSettingsStore` ilk yazımda `SaveAsync` CS1061 verdi.
- **Sebep:** Ezberden yazım; arayüzde `GetSettingsAsync/SaveSettingsAsync` çifti var (`IUpdateService.cs:10-11`).
- **Çözüm:** Derleyici yakaladı, tek satır düzeltme. Kural: split/taşımada metot adları arayüzden kopyalanır (CEKIRDEK plan dersi: ezberden yazma).
- Tarih: 2026-09-12

## SimpleWebSource timeout 0 — "must be greater than TimeSpan.Zero" (ÇÖZÜLDÜ — 2026-09-11 — Oturum 200)
- **Belirti:** Güncelleme → "Bağlantıyı Dene" → "Güncelleme kaynağına ulaşılamadı (http://127.0.0.1:8321): value ('00:00:00') must be greater than '00:00:00'" (canlı, kurulu 1.1.1).
- **Sebep:** `UpdateFeedSourceFactory.Create` 3. parametreye `0` geçiyordu; o parametre `double` **dakika** (`Velopack.xml`: `Timeout`) → HttpClient sıfır timeout'u reddeder. Oturum 199'da sayfa daha önce donduğu için bu bug hiç ağa çıkamamış, gizli kalmış.
- **Çözüm:** `30` (protokol sabiti, kural 9) + `UpdateFeedSourceTests.DuzAdres_TimeoutGecerliOlur` regresyonu. Canlı: "v1.1.2 hazır, İndir". Kural: harici API ctor parametresinin birimi doc'tan doğrulanır (`DefaultTimeout` ms→s dersiyle aynı aile, Oturum 127).
- Tarih: 2026-09-11

## İlk kurulum restore dalına düşüyor — 0-baytlık dosya + yanlış mesaj (ÇÖZÜLDÜ — 2026-09-11 — Oturum 200)
- **Belirti:** Taze kurulumda "Kurulumu Başlat" → "Veritabanı meşgul veya kilitli, geri yüklenemediği için işlem durduruldu" (canlı, 2 deneme).
- **Sebep (2 katman):** (1) SQLite bağlantı açarken dosya yoksa **0 baytlık dosya** oluşturur (analiz yan etkisi — açılıştaki durum sorgusu tetikler) → `SistemDatabaseFileExists()` true döner → create dalı yerine migrate dalı çalışır → geçersiz DB → yedek olmayan restore başarısız olur. (2) Mesaj yedek-yokluğunu kilit gibi gösteriyordu.
- **Çözüm:** `SistemDatabaseFileExists()` 0-bayt dosyayı yok sayar (`>= MIN_SQLITE_FILE_SIZE`, mevcut sabit — yeni eşik yok) + mesaj dürüstleşti ("yedek bulunamadı veya dosya kilitli"). Canlı: kurulum tek seferde 7/7. Kural: `File.Exists` ile "veritabanı var" denmez — SQLite dünyasında boyut+header bakılır (`IsSistemDatabaseValid` deseni).
- Tarih: 2026-09-11

## Güncelleme sayfası açılınca donma (ÇÖZÜLDÜ — 2026-09-11 — Oturum 199)
- **Belirti:** Denetim → Güncelleme bölümü açılınca uygulama donuyor (canlı, kurulu 1.1.0).
- **Sebep:** `UpdateViewModel.CheckInitialStateAsync → IUpdateService.IsUpdatePendingRestart` (sync property) içinde `GetSettingsAsync().GetAwaiter().GetResult()` — UI thread bloklanıp file-IO continuation'ı bekliyor → klasik sync-over-async deadlock. Ağ çağrısı hiç başlamıyordu (logda iz yok).
- **Çözüm:** `IsUpdatePendingRestartAsync()` (interface + servis + VM çağrısı; sync üye silindi, Kural 4). Regresyon: `GuncellemeAyarTests` 2 test (fake servis). Kural: UI yolundaki her servis çağrısı `await` edilir, `.Result/.Wait()/GetResult()` yasak.
- Tarih: 2026-09-11

## Seed admin girişi FormatException ile çöküyor (ÇÖZÜLDÜ — 2026-09-11 — Oturum 199)
- **Belirti:** `korkutomer / Ok241341` ile girişte "Sistem Hatası" dialogu (canlı, kurulu 1.1.0).
- **Sebep:** Seed `ParolaHash` 78 karakter (bozuk — base64 uzunluğu 4'ün katı olmalı; geçerli Identity V3 84) → `PasswordHasher.VerifyHashedPassword` içinde `FromBase64String` patlıyor. Kök: hash koda kesik yapıştırılmış (üretilen geçerli hash ile aynı `AQAAAAIAAYagAAAAE` öneki).
- **Çözüm:** Seed'e geçerli hash (`makehash` ile üretildi) + `SeedHashDuzeltme` migration'ı + `AuthenticationRepository.Login` `FormatException` → `InvalidPasswordException` guard'ı (legacy `PBKDF2$` yolu artık gerçekten işler). Regresyon: `GirisHashTests` 3 test. Kural: seed gizli-değerler (hash/anahtar) `makehash` benzeri üreticiyle yazılır, elle yapıştırılmaz.
- Tarih: 2026-09-11

## Yöneticiyle girişte ayar satırları "yalnızca yönetici" kilitli (ÇÖZÜLDÜ — 2026-09-11 — Oturum 197)
- **Belirti:** Seed yönetici (`korkutomer`) ile girişte Denetim Masası'ndaki tüm rozetli ayarlar kilitli + kayıt `UnauthorizedAccessException` → tam test edilemiyor.
- **Sebep:** Seed yalnızca kullanıcıyı + rolleri üretir, `KullaniciFirmaRol` satırı üretmez (KFR anahtarı `KullaniciId+FirmaId`, seed firma yok; firma kaydında da KFR üreten kod yok) → `CreateKullaniciModel:178` `FirstOrDefault()` boş gelir → `model.Rol` null → `KullaniciYoneticiMi` false. İkinci sıra: satır olsa bile ilk satır admin olmayabilirdi.
- **Çözüm:** `AyarYetkiDenetimi.KullaniciYoneticiMi` seed fallback'i (`GetCurrentUserId == SeedYoneticiId`, Oturum 129/130 presedanı) + `CreateKullaniciModel` yönetici-satır tercihi + 3 Denetim VM'i tek kaynağa (`AyarYetkiDenetimi`, Kural 4). Regresyon: `AyarYetkiDenetimiTests` 6 test. Kural: rol ataması firma-bağımlıysa seed/bootstrap kimliği rol satırına bakılmadan tanınır.
- Tarih: 2026-09-11

## ContentDialogSmokeFill override şablona ulaşmıyor (ÇÖZÜLDÜ — 2026-09-10 — Oturum 155, canlı kanıtlı)
- **Belirti:** `ContentDialogSmokeFill → Transparent/acrylic` override'ı NE app-merge NE dialog-instance `Resources` düzeyinde karartma perdesini kaldırmadı (piksel kanıtı: aynı bölge açık ~87 / kapalı ~219).
- **Sebep:** Şablon `{ThemeResource ContentDialogSmokeFill}` okumasına rağmen iki kapsam da canlıda etkisiz kaldı (nedeni deşilemedi — zincir varsayımına güvenilmedi).
- **Çözüm:** `ShowAsync(ContentDialogPlacement.Popup)` — smoke'suz resmi API (şablondaki `DialogShowingWithoutSmokeLayer` durumu). Kural: dialog perdesiyle resource üzerinden boğuşulmaz, yerleşim API'si kullanılır.
- Tarih: 2026-09-10

## WinUI3 Style'da Resources yok — WMC0011 (ÇÖZÜLDÜ — 2026-09-10 — Oturum 147)
- **Belirti:** `Cards.xaml` içine yazılan `MuhasibDialogStyle` içindeki `<Style.Resources>` bloğu `XamlCompiler error WMC0011: Unknown member 'Resources' on element 'Style'` verdi.
- **Sebep:** WPF kalıbı sanıldı; WinUI3/UWP `Style`'ında `Resources` property'si yok (yalnız `Setters/BasedOn/TargetType`).
- **Çözüm:** Tema-anahtar override'ı (`ContentDialogSmokeFill`) merged-dictionary düzeyinde tek satır olarak yazıldı (stil dışına). Kural: şablonun okuduğu tema anahtarını stil-içi ezmek gerekiyorsa ya dialog `Resources`'ına (tekil) ya merged-dictionary'ye (paylaşılan) yazılır, `Style.Resources` denenmez.
- **Ek (Oturum 148):** `AcrylicBrush`'ta `BackgroundSource`/`BlurAmount` da XAML'de yoktur (aynı WMC0011) — WinUI3'te blur miktarı SDK-sabittir, WPF örneklerinden kopyalanamaz. Blur hissi yalnız `TintOpacity`/`TintLuminosityOpacity` ile ayarlanır.
- Tarih: 2026-09-10

## Moq Setup'ta opsiyonel parametre CS0854 (ÇÖZÜLDÜ — 2026-09-09 — Oturum 143)

## ContentDialog DefaultButton accent ezmesi (ÇÖZÜLDÜ — 2026-09-10 — Oturum 145, canlı kanıtlı)
- **Belirti:** `YonetimAyarlarDialog` `Kapat` butonu mavi render oluyordu (SDK accent).
- **Sebep:** `DefaultButton="Close"` — WinUI default butona accent basar (Oturum 96 dersi aynı: `DefaultButton="Primary"` yasağı).
- **Çözüm:** `DefaultButton="None"` (doğru kalıp: QuickSistemDbDiagDialog/SagaPipeline/YeniDonem/TenantDatabaseUpdate). Kural: dialogda `DefaultButton` yalnızca `None` olur.
- Tarih: 2026-09-10

## ContentDialog SDK max-width tuzağı (ÇÖZÜLDÜ — 2026-09-10 — Oturum 145, canlı kanıtlı)
- **Belirti:** Hamburger-menülü dialogda pane+içerik üst üste bindi, metinler çakıştı (önce `MaxWidth="1000"` attribute'u denendi — işlemedi).
- **Sebep:** Template `MaxWidth`'i `{ThemeResource ContentDialogMaxWidth}` (~640) ile sabitler; dışarıdan `MaxWidth` attribute'u ezilmez.
- **Çözüm:** dialog `Resources`'ına `<x:Double x:Key="ContentDialogMaxWidth">960</x:Double>` (yerel override, global etkilenmez). Kural: geniş dialog gerekiyorsa bu resource, attribute değil.
- Tarih: 2026-09-10

## Birincil aksiyon scroll altında kaldı (DERS — 2026-09-10 — Oturum 145)
- **Belirti:** "Varsayılanlara dön" butonu vardı ama scroll altında olduğu için kullanıcı hiç görmedi, "ekleyelim" istedi.
- **Sebep/Ders:** Birincil aksiyonlar scroll gerektirmeden görünür olmalı (header/footer sabit alana alınır). `YonetimAyarlarDialog` v2'de buton header-sağ durum butonu oldu.
- Tarih: 2026-09-10
- **Belirti:** `ITenantSettingsProvider.GetAsync()`'e `firmaId = 0` eklendikten sonra `TenantDerinBaglantiTests.cs:126` `Setup(a => a.GetAsync())` derlenmedi (CS0854: ifade ağacı opsiyonel bağımsız değişken içeremez).
- **Sebep:** Moq `Setup` expression-tree'dir; C# ifade ağaçlarında opsiyonel-argüman çağrısı yasak.
- **Çözüm:** `Setup(a => a.GetAsync(It.IsAny<long>()))`. Kural: interface'e opsiyonel parametre eklenince tüm Moq `Setup`/`Verify` çağrıları taranır.
- Tarih: 2026-09-09

## Yedek pagination barı görünür ama butonlar ölü (ÇÖZÜLDÜ — 2026-09-09 — Oturum 142)
- **Belirti:** 4+ yedekte pagination barı (`‹ 1 / 2 ›`) görünüyor ama `‹/›` butonları basılmıyordu (`IsEnabled=false` takılı).
- **Sebep:** 4 VM'deki (`DonemYedekler/ArsivDonemler/BilinmeyenYedek/MaliDonemYonetim-Acik`) `UpdatePaged*` yalnızca `TotalPages/HasPagination/PageInfo` bildiriyordu; `CanPrev/CanNext` bildirimi yalnız `CurrentPage` setter'ındaydı. Liste yüklenince sayfa zaten 1 olduğundan setter `Set` false dönüp bildirim yapmıyordu → binding bayat kaldı. Ek: liste kısalınca sayfa clamp'lenmiyordu (son sayfada silme → boş dilim).
- **Çözüm:** `UpdatePaged*`'a `CanPrev/CanNext/CurrentPage` bildirimi + sayfa clamp eklendi (4 VM). Regresyon: `YedekPaginationTests.cs` 3 test (bildirim varlığı + dilim + kısalma). Kural: computed `Can*`/`*Info` her `UpdatePaged*` içinde bildirilir, setter bildirimine güvenilmez.
- Tarih: 2026-09-09

## BilinmeyenPanel kayıtlı dönem yedeklerini listeliyordu (ÇÖZÜLDÜ — 2026-09-09 — Oturum 141, canlı kanıtlı)
- **Belirti:** `BilinmeyenPanel.TaraAsync` kayıtlı `MaliDonem.DatabaseName`'ye ait `.backup` dosyalarını da listeliyordu (HATALAR B3).
- **Sebep:** Disk taraması ile dönem listesi aynı kanonik adla eşleşmiyordu.
- **Çözüm:** `TaraAsync` her çağrıda diski yeniden tarar (cache yok) + `TumDonemAdlariniYukleAsync` `HashSet<kanonikAd>` (`TrimDbSuffixLocal`, `OrdinalIgnoreCase` — iki taraf simetrik) + E2 olaylarında oto-tazeleme (`TenantBackup/RestoreCompleted`). Canlı: 18 kayıtlı yedek → 0 listelenme; sentetik yetim `db-YABANCI_2099` → 1 listelenme + Temizle. Kural: disk↔DB eşleşen her listede iki taraf aynı kanonikleştirmeden geçer.
- Tarih: 2026-09-09

## slnx→sln dönüşümü kaynak-tarayan tüm testleri kör bıraktı (ÇÖZÜLDÜ — 2026-09-09 — Oturum 140)
- **Belirti:** `MuhasibPro.slnx` silinip `MuhasibPro.sln` oluşturulduktan sonra `ArchitectureTests` dahil 9 kaynak-tarama testi yeşil geçmeye devam etti — ama `RepoRoot()` ankrajı (`slnx` dosyası) artık bulunamadığı için tarama boş kümeye düşüyor, yasaklı metin hiç aranmıyordu (yanlış-yeşil).
- **Sebep:** Ankraj tek dosya adına bağımlıydı; `slnx`'in opsiyonel dönüşümü (KONTROL Faz 0) test ankrajlarıyla eşgüdümsüz yapıldı.
- **Çözüm:** 9 dosyada ankraj `slnx || sln` kabul eder hale geldi + `CircularDependencyTests`'e kör-tarama guard'ı (`kenarlar > 100`, `appKenar > 10`) eklendi — evren boşsa test kasten kızarır. Kural: kaynak-tarayan her teste körlük guard'ı şart; çözüm-dosyası adı değişiminde test ankrajları aynı PR'da güncellenir.
- Tarih: 2026-09-09

## AuthenticationService ↔ IdentitySettingsProvider DI döngüsü — uygulama açılışta patlıyordu (ÇÖZÜLDÜ — 2026-09-09 — Oturum 140)
- **Belirti:** Uygulama ilk DI çözümünde `A circular dependency was detected for the service of type 'IAuthenticationService'` — Login'e bile gelmeden çöküş. Zincir: `AuthenticationService` (Singleton, ctor'da `IIdentitySettingsProvider`) → `IdentitySettingsProvider` (Singleton, ctor'da `IAuthenticationService`).
- **Sebep:** Faz 2'de `IdentitySettingsProvider`'a yetki denetimi için `IAuthenticationService` ctor'dan verildi; oysa `AuthenticationService` zaten kilit eşiklerini modelden okumak için `IIdentitySettingsProvider` istiyordu. MS DI opsiyonel (`= null!`) parametreyi kayıtlıysa çözer — `null` varsayılanı döngüyü kırmaz.
- **Çözüm:** Provider ctor'dan `IAuthenticationService` çıkarıldı → `IServiceProvider` (`[ActivatorUtilitiesConstructor]`, construction-dışı `SaveAsync` içinde `GetService<IAuthenticationService>()`); `AyarYetkiTests` yeni imzaya uyarlandı. Kural: ayar-provider ↔ tüketici-servis arasında ctor'dan karşılıklı bağımlılık yasak — yetki gerektiren provider `IServiceProvider` ile tembel çözer. Bekçi: `CircularDependencyTests.DI_Circular_Baglanti_Olmamali` (ctor-graf + gerçek DI kayıt haritası + App kaynak taraması; HEAD sürümünde zinciri birebir yakaladığı kontrollü deneyle kanıtlı).
- Tarih: 2026-09-09

## 70 birikmiş derleme uyarısı — faz kuralı (0/0) yıllarca esnetilmiş (ÇÖZÜLDÜ — 2026-09-09 — Oturum 133, Mühür)
- **Belirti:** full `--no-incremental` build'de 70 uyarı (benzersiz ~35): CS8625 `= null` (testler), CS8600 cast'ler, xUnit1031 `.Result`, xUnit1012 `InlineData(null)`, CS0618 Velopack `IsUpdatePendingRestart`, IL2072 COM `CreateInstance`, CA1416 Registry. Her faz "eski aileler" notuyla kapatılmış.
- **Sebep:** Uyarı sayacı artımlı build'de küçük göründüğü (2) için tam süpürme hiç yapılmamış.
- **Çözüm:** hepsi kapatıldı — `null!`, `?.ToString()`, async test, `string?` + null-tolerant `IsValid` imzası, `UpdatePendingRestart != null`, IL2072 pragma (gerekçeli), `OperatingSystem.IsWindows()` guard. `dotnet build --no-incremental` **0 uyarı 0 hata**, `dotnet test` 235/235. Kural: Mühür'den sonra yeni uyarı = faz kapatmaz; tam sayım `--no-incremental` ile alınır (artımlı sayı yanıltır).
- Tarih: 2026-09-09

## Sistem ilk-sürüm damgası hesaplananı değil sabiti yazıyordu (ÇÖZÜLDÜ — 2026-09-09 — Oturum 131)
- **Belirti:** `SistemMigrationManager.DatabaseVersionFromMigrationsAsync` kayıt-yok dalında `CurrentDatabaseVersion = "1.0.0.0"` sabit yazıyordu; bir satır üstte hesaplanan `newVersion` hiç kullanılmıyordu (tenant tarafı doğruydu).
- **Sebep:** Kopyala-yapıştır; tenant'taki INSERT dalı `newVersion` kullanırken sistem tarafı unutulmuş.
- **Çözüm:** SemVer geçişinde `newVersion` yazılır hale geldi. Kural: hesaplanıp kullanılmayan değişken derleme uyarısı vermez — migration-damga dallarında INSERT/UPDATE simetrisi code-review'da aranır.
- Tarih: 2026-09-09

## KaydedenId sihirli sayıları var olmayan kullanıcılara işaret ediyordu (ÇÖZÜLDÜ — 2026-09-09 — Oturum 130)
- **Belirti:** `KaydedenId = 1` (3 yer), `= 0000000800` (2 log reposu, "korkutomer" yorumuyla), `= 241341L` (Register), `= 24134175366` (81 il seed'i) — seed adminin gerçek Id'si `5413300800` iken hiçbiri ona işaret etmiyordu. FK olmadığı için patlamıyor, denetim izi sessizce yalan söylüyordu.
- **Sebep:** Her yazım noktası kendi varsayılanını uydurmuş; tek kaynak yok (kural 7 ihlali).
- **Çözüm:** `Domain/Entities/KullaniciSabitleri.SeedYoneticiId = 5413300800L` tek kaynak — 7 yanlış nokta + 6 seed dosyasındaki doğru literal'ler + seed adminin kendi `Id`'si sabite bağlandı (12 dosya); Register self-registration'da kendi Id'sini yazar; `KaydedenIdVarsayilanTests` bekçi taraması (Migrations + test fixture hariç literal yasaklar). Kural: yeni `KaydedenId` varsayılanı sabitsiz yazılmaz.
- Tarih: 2026-09-09

## LoadAsync'te tekrar Subscribe — ikinci çağrıda ArgumentException (ÇÖZÜLDÜ — 2026-09-09 — Oturum 129)
- **Belirti:** `LoginViewModel.LoadAsync` her çağrıda `MessageService.Subscribe` ediyordu; Teşhis→Kapat yolu `LoadAsync`'i ikinci kez çağırınca `RefreshDbStatusAsync` hiç çalışmıyordu (DB durumu tazelenmiyordu).
- **Sebep:** `MessageService.Subscriptions.AddSubscription` içi `Dictionary.Add(typeof(TArgs), action)` — aynı hedef+mesaj ikinci kez eklenince `ArgumentException`. `async void OnNavigatedTo`/doğrudan çağrıda exception akışı kesiyor.
- **Çözüm:** `_quickLoginAbone` guard (tek seferlik abonelik) + `Unsubscribe()` (`MessageService.Unsubscribe(this)`, bayrağı sıfırlar) + `LoginView.OnNavigatedFrom` bağlantısı. Kural: `LoadAsync`/`OnNavigatedTo` içine çıplak `Subscribe` yazılmaz — guard + Unsubscribe çifti şart (Faz 5 doğrulaması: Subscribe sayısı = Unsubscribe sayısı).
- Tarih: 2026-09-09

## ITenantContext ölü arayüz — tek tüketiciyle kayıtsız kaldı (ÇÖZÜLDÜ — 2026-09-09 — Oturum 128)
- **Belirti:** `Data/DataContext/ITenantContext` — 0 implementasyon, 0 DI kaydı; tek tüketici `PermissionService` ctor'da istiyordu. Kayda alınsaydı resolve anında patlayacaktı.
- **Sebep:** Plan L2 notu ("SetTenant 0 caller — sil ya da aynaya bağla") Faz 2'de işlenmemiş; `IPermissionService`'in 0 tüketicisi olduğu için kimse çarpmamış.
- **Çözüm:** `PermissionService` → `IFirmaWithMaliDonemSelectedService` (seçim aynası) + `IAuthenticationService` (giriş); `ITenantContext.cs` silindi (ölü kod kural 4). Kural: implementasyonsuz interface DI'ya alınmadan önce ya implemente edilir ya silinir.
- Tarih: 2026-09-09

## SqliteConnectionStringBuilder DefaultTimeout saniye ister — 5000 ms değeri ~83 dk oldu (ÇÖZÜLDÜ — 2026-09-09 — Oturum 127)
- **Belirti:** `TenantSQLiteConnectionStringFactory` `DefaultTimeout = 5000` yazıyordu; niyet 5 sn busy-timeout idi.
- **Sebep:** `Microsoft.Data.Sqlite` builder'ı `DefaultTimeout`'u **saniye** ister (ms anahtarı yok); ayar modeli ms tutuyordu, çevrim yapılmamıştı.
- **Çözüm:** `TenantSettings.BusyTimeoutMsToSeconds` (clamp 1000-30000 → en az 1 sn) fabrika içinde; varsayılan 5000 ms → 5 sn. Kural: harici API'ye değer geçerken birim doc'tan doğrulanır.
- Tarih: 2026-09-09

## Moq ifade ağacı opsiyonel argüman alamaz — CS0854 (ÇÖZÜLDÜ — 2026-09-09 — Oturum 127)
- **Belirti:** `Setup(m => m.InitializeTenantDatabaseAsync(It.IsAny<string>()))` opsiyonel parametre eklendikten sonra `CS0854` verdi.
- **Sebep:** Moq `Setup`/`Verify` ifade ağacıdır; eksik bırakılan opsiyonel argüman ağaçta temsil edilemez.
- **Çözüm:** tüm parametreler açık yazılır (`It.IsAny<int?>()` vb.). Kural: opsiyonel parametreli interface mock'lanırken Setup/Verify her zaman tam-arite yazılır.
- Tarih: 2026-09-09

## GetFirmalarPageAsync guard'ı `&&` — null'da NRE (ÇÖZÜLDÜ — 2026-09-09 — Oturum 124, split-taşıma)
- **Belirti:** `if (items == null && items.Count < 0)` — repo null dönerse ikinci operand `NullReferenceException` fırlatıyor (catch yutuyor, Error dönüyor ama yanlış yoldan); `Count < 0` zaten imkânsız.
- **Sebep:** 6.20'deki `SelectedDetail &&→||` ailesinin yaşayan örneği (master mirası).
- **Çözüm:** `||` yapıldı (`FirmaListelemeService`); boş-liste davranışı korunur (Success-boş).
- Tarih: 2026-09-09

## Tenant restore rollback Sistem.db'yi hedefliyordu — veri kaybı riski (ÇÖZÜLDÜ — 2026-09-08 — Oturum 123)
- **Belirti:** `TenantSQLiteBackupManager.RestoreBackupDetailsAsync` catch bloğunda rollback `GetSistemDatabaseFilePath()` yoluna yazıyordu — tenant geri yüklemesi patlarsa **Sistem.db tenant safety yedeğiyle eziliyordu**.
- **Sebep:** HATALAR arşivindeki master bug'ı ("hata bloğunda Sistem DB yolu") yaşayan koda taşınmış; metot içi `targetPath` (tenant yolu) catch kapsamı dışında kaldığı için yanlış sabit kullanılmış.
- **Çözüm:** catch içinde tenant yolu yeniden türetildi (`GetTenantDatabaseFilePath(databaseName)`); Tenant↔Sistem ortak-altyapı bekçisine `GetSistemDatabaseFilePath` yasağı eklendi (tekrarı derleme hatası hükmünde).
- Tarih: 2026-09-08

## ShellArgs yanlış namespace — WMC kaskadı "ortam kırık" sanıldı (ÇÖZÜLDÜ — 2026-09-08 — Oturum 121)
- **Belirti:** `dotnet build` 1× CS0234 (`MaliDonemYonetimView.xaml.cs:147` `ViewModels.Infrastructure.ViewModels.ShellArgs` yok) + 19× WMC0001 (converters) + WMC9999. Oturum 120 "bu ortamda XAML derleyicisi kırık, Windows'ta teyit" diye loglamıştı.
- **Sebep:** Oturum 120'nin `OnGuncelleClick` eklemesi `ShellArgs`'ı yanlış namespace ile yazmış (`Infrastructure.ViewModels` yerine `ViewModels.Shell`). Gerçek tek CS hatası XAML derlemesini düşürüp sahte WMC kaskadı üretti (Oturum 84 dersi aynen).
- **Çözüm:** tek satır namespace düzeltmesi → bu ortamda da **build 0 hata**. Kural: WMC kaskadında önce gerçek CS hatası aranır; "ortam kırık" hükmü ancak kaskatsız tekrarda verilir.
- Tarih: 2026-09-08

## Atananmamış Where/Include — filtreli sayımlar hep toplam döndü (ÇÖZÜLDÜ — 2026-09-08 — Oturum 121, süpürme)
- **Belirti:** `GetFirmalarCountAsync` / `GetMaliDonemlerCountAsync` arama varken bile toplam sayı dönüyordu (`items.Where(...)` sonucu atanmıyordu); `MaliDonem`'de `items.Include(r => r.Firma)` da çöpe gidiyordu.
- **Sebep:** `HATALAR.md` arşivindeki "atılan sorgu sonuçları" deseninin yaşayan örnekleri (master'dan kalma kopyala-yapıştır).
- **Çözüm:** `items = items.Where(...)` + `items = items.Include(...)`. Kural: `IQueryable` zincirinde atamasız `Where/Include` derleme uyarısı vermez — code-review'da aranır.
- Tarih: 2026-09-08

## Sürüm hash çöpü — 8 haneli tarih öneki (ÇÖZÜLDÜ — 2026-09-08 — Oturum 119, canlı)
- **Belirti:** ön-dialogda hedef sürüm `1.0.0.04DD0C` — `20260908_AddTenantIdentity` hash dalına düştü.
- **Sebep:** `ExtractVersionFromMigration` yalnız 14 haneli damgayı tanıyordu.
- **Çözüm:** takvim-sıralı şema (`1.yyyy.MMdd.HHmm` / `1.yyyy.MMdd.0`) + 3 test (sıralama regresyonu dahil). "Yeni proje, veri yok" kararı sayesinde eski şema (`1.1.28.1709`) gömülü verisi korunmadı — bilerek değiştirildi.
- Tarih: 2026-09-08

## x:Bind iç-yol tazelenmez — nesne atamasında notify şart (ÇÖZÜLDÜ — 2026-09-08 — Oturum 119, canlı)
- **Belirti:** sayfada versiyon şeridi boş (`– → –`), başlık/detay dolu — aynı `Check` nesnesinden besleniyorlardı.
- **Sebep:** `Check = await ...` sonrası `NotifyPropertyChanged(nameof(Check))` yoktu; düz prop'lar (Headline/Details) kendi notify'larıyla güncellendi, iç-yol (`Check.X`) tazelenmedi.
- **Çözüm:** atama sonrası notify. Kural: x:Bind iç-yol (`A.B`) bağlanan her nesne ataması notify edilir.
- Tarih: 2026-09-08

## AsyncRelayCommand ilk değerde takılır — RaiseCanExecuteChanged unutulursa (ÇÖZÜLDÜ — 2026-09-08 — Oturum 119, canlı)
- **Belirti:** "Yedekle ve Güncelle" veri varken pasif (`IsEnabled=False`).
- **Sebep:** `StartUpdateCommand`'ın `CanExecuteChanged`'i hiç ateşlenmedi; buton komutun ctor-anındaki değerinde kaldı (x:Bind `IsEnabled` tek başına yetmedi — komut varlığında komut belirleyici).
- **Çözüm:** `RefreshStartCommand()` deseni (`Notify CanStart` + `RaiseCanExecuteChanged`) — `CanStart`'ı değiştiren her setter/yol çağırır.
- Tarih: 2026-09-08

## bin/obj çift ağaç — Debug + x64 karışması (BİLGİ — 2026-09-08 — Oturum 119)
- **Belirti:** `bin` içinde `Debug` + `x64` (kullanıcı tespiti).
- **Durum:** tüm `bin/obj` silinip temiz derlendi (tek `x64`); ancak Libraries `bin\Debug`, App `bin\x64\Debug` üretiyor — platform konfigürasyonu karışık, çalışıyor ama dağınık. slnx platform birleştirmesi izlenecek.
- Tarih: 2026-09-08

## Tenant güncelleme akışı — product-ready geçişi için modüler yapı zorunluluğu (AÇIK — 2026-09-09 — Oturum 138, kullanıcı beyanı)
- **Geçiş sebebi:** Tenant veritabanlarına yeni migration uygulama akışı `FirmaShellView`'de dönem seçildiğinde `TenantDatabaseUpdateDialog` (Güncelle/Daha sonra/Vazgeç) olarak kurgulanmıştı; karar değişip `TenantDatabaseUpdateView` tam sayfasına (Yedek→Göç→Doğrulama→oto geri alma) geçildi. Asıl amaç **developer mod → product-ready** (çekirdek) adımı — güncelleme hattı tek dialog ile yönetilemedi, modüler yapıya (CEKIRDEK-MODUL-PLAN L3 M5 + L4 politika) geçildi.
- **Tetkik:** Dialog→Sayfa geçişi + `TenantDatabaseUpdateCoordinator` / `ITenantDatabaseUpdateService` ayrımı bu geçişin kod izidir. Kapatılma kriteri: `TenantUpdated` → `MainShell` tek yol, `Continue` anomalisi dahil, canlı E2E ile doğrulanır.
- Tarih: 2026-09-09

## MaliDonemListControl — güncelleme rozeti tek dönemle sınırlı (ÇÖZÜLDÜ — 2026-09-10 — Oturum 183, A şıkkı)
- **Belirti:** `MaliDonemListControl` bir dönemde `Güncelleme Gerekli` rozeti gösteriyor, aynı firmanın diğer dönemlerinde aynı şema farkı varken uyarı çıkmıyor. `TenantUpdateAvailableEvent` tekillik şüphesi.
- **Şüphe:** `MaliDonemListViewModel` abonesi (`TenantUpdateAvailableEvent` → rozet) filtrelemeyi `DatabaseName` eşleşmesi yerine ilk seçili veya tekil dönem üzerinden kuruyor olabilir; `Task.WhenAll` + per-item `GetTenantDatabaseStateAsync` analizinden sonra `DbDurum=RequiredUpdating` tüm listeye yayılmıyor. `FirmaShellViewModel` status/toast bacağı ayrı çalışıyor.
- **Planlı çözüm:** Liste yenilemede her `MaliDonemModel` için `GetTenantDatabaseStateAsync` + `TenantDatabaseUpdateService.CheckUpdateRequiredAsync` çiftinden gelen `RequiredUpdating` tüm satırlara işletilir, `IEventBus` abonesi `DatabaseName` bazlı eşleştirir, `NotifyPropertyChanged` + `SecimiListeyeYenidenDuyur` ile korunur. Test: 2 dönemli firma, yalnız 1 DB eski şemada → 1 rozet → upgrade sonrası 0.
- **Çözüm (uygulandı):** `LoadDataAsync` sonunda `_ = AnalyzeAllDbStatusesAsync(snapshot)` (Task.WhenAll; AnalyzeDbStatusAsync zaten per-item try/catch'li, WhenAll savunma try/catch'li) — seçili + seçili-olmayan tüm kartlar paralel analizlenir. E1 abonesi `==` → `OrdinalIgnoreCase` (farklı harfli event de rozeti düşürür). Regresyon: `MaliDonemTopluAnalizTests` 2 test (toplu rozet + case-insensitive E1). Build 0/0, test 253/253, smoke OK. Canlı rozet teyidi kullanıcıda.
- Tarih: 2026-09-10

## MaliDonemYonetimView — güncelleme gerekli uyarısı var, Güncelle butonu yok (ÇÖZÜLDÜ — 2026-09-10 — Oturum 183)
- **Belirti:** Yönetim sayfasında seçili dönem `DbGuncellemeGerekliMi=true` iken `Güncelleme Gerekli` uyarı barı çıkıyor ama `Güncelle` butonu (`OnGuncelleClick` → `TenantDatabaseUpdateView`) listelenmiyor. `DonemOzetCard` split’inde buton `Visibility` yanlış kaynağa bağlanmış olabilir.
- **Şüphe:** `DonemOzetCard.xaml` + `MaliDonemYonetimView.xaml` Row A birleşik kartında `GuncelleClick` `Visibility="{Binding SelectedDonem.DbGuncellemeGerekliMi}"` `TrueToVis` doğru kaynaktan beslenmiyor veya `DonemOzetCard` `Root/DataContext` ataması `OnNavigatedTo`’da eksik (paket split’i sonrası `Root` yalnız `DonemKartlar`/`DonemYedekler`’e verilmişti, `DonemOzet` sonradan eklendi). Buton `PrimaryCompactButtonStyle` mevcut, tetik yok.
- **Planlı çözüm:** `OnNavigatedTo`’da `DonemOzet.DataContext + Root` atanması doğrulanır (`MaliDonemYonetimView.xaml.cs:24` paket sonrası eklendi, HEAD’e geri alınmıştı — tekrar eklenir), `Visibility` doğrudan `SelectedDonem.DbGuncellemeGerekliMi`’ye bağlanır, tıklama `ShellArgs(SelectedFirma, SelectedDonem)` ile `TenantDatabaseUpdateView`’a `Navigate` eder. Canlı: `Güncelleme Gerekli` iken buton görünür → sayfa Yedek→Göç→Doğrulama.
- **Çözüm (teşhis):** Rapor Oturum 138'den kalma; mevcut ağaçta zincir eksiksiz: `DonemOzetCard.xaml:75` uyarı barı + `:88` buton (`SelectedDonem.DbGuncellemeGerekliMi`), sayfa `:28-29` DataContext+Root, `:178` `GuncelleClick→OnGuncelleClick→TenantDatabaseUpdateView`, model iki setter'da `DbGuncellemeGerekliMi` bildirimi, sayfa setter'ı `DonemDegistiAsync` ile analiz tetikler. Kod değişikliği gerekmedi; kanıt: `YonetimGuncelleAksiyonTests` (bayat seçim → koşul true). Not: 3 dönem de güncel (Oturum 167) → butonun gizli kalması DOĞRU davranış; bayat-senaryo görseli kullanıcıda.
- Tarih: 2026-09-10

## BilinmeyenPanel — varolan döneme ait yedekler Bilinmeyen’de listeleniyor (ÇÖZÜLDÜ — 2026-09-09 Oturum 141 canlı + 2026-09-10 Oturum 184 regresyon testi)
- **Belirti:** `BilinmeyenPanel` (önetic `BilinmeyenVeritabanlariVM.TaraAsync`) kayıtlı bir `MaliDonem.DatabaseName`’ye ait `.backup` dosyalarını da listeliyor; beklenen: yalnızca `MaliDonemler` tablosunda karşılığı olmayan (öksüz) dosyalar.
- **Kök neden şüphesi:** `ParseDatabaseName` → kanonik ada normalize (`ParseDatabaseName` Oturum 112’de iki desen için düzeltildi) sonrası filtre `GetAllBackupsAsync` içinde `DatabaseName` set’i ile birebir eşleşmiyor; `GetBackupsAsync` iki desen (`çıplak + .db’li`) dedupe ederken `Bilinmeyen` taraması `MaliDonem` listesinden bağımsız disk taraması ile dolduruyor. `TenantSettings` / `MaliDonem` listesi ile `Backup` listesi ortak sözleşme (`IDatabaseBackupManager`) üzerinden konuşmuyor (Kural 10 ihlali şüphesi).
- **Planlı çözüm:** `TaraAsync` önce `IMaliDonemService.GetMaliDonemlerWithFirmaId` (veya tüm firmalar) → `HashSet<kanonikAd>` → diskteki `*.backup` dosyaları `ParseDatabaseName` → set’te varsa **atlanır**, yoksa Bilinmeyen’e eklenir. `GetAllBackupsAsync` ile aynı kanonikleştirme kullanılır. Test: 2 dönem * 1 yedek → Bilinmeyen 0; diske yabancı `db-YABANCI_2099_*.backup` → Bilinmeyen 1.
- **Not (Oturum 184):** Bu AÇIK kaydı Oturum 138'den kalma çift kayıt — aynı belirti Oturum 141'de çözülüp canlı kanıtlanmıştı (`HATALAR.md` üstteki B3 kaydı: 18 kayıtlı → 0, sentetik yetim → 1). Mevcut kod teyit edildi (`TaraAsync:130` her çağrıda yeniden tarar + simetrik `TrimDbSuffixLocal` + `OrdinalIgnoreCase` + E2 abonelikleri). Eksik halka olan planlı test eklendi: `YedekOlayTests.Bilinmeyen_KayitliDonemYedegini_Listelemez_Yabanciyi_Listeler` (çıplak + `.db`'li desen dahil). Build 0/0, test 255/255.
- Tarih: 2026-09-10

## Continue sonrası MainShell yerine Mali Dönem Yönetimi (ÇÖZÜLDÜ — 2026-09-10 — Oturum 185)
- **Belirti:** güncelleme sayfasında "Çalışma Alanına Geç" sonrası beklenen MainShell yerine Mali Dönem Yönetimi penceresi görüldü.
- **Şüphe:** UIA otomasyonu ana-pencere ağacından detay-pencere butonlarını da gördüğü için yanlış butona (ana `DevamEt`?) basılmış olabilir — otomasyon artefaktı da olabilir, gerçek akış hatası da. Teşhis edilmedi.
- **Kök neden (bulundu):** `OnTenantUpdated` yalnız yerel `Selection.SelectedMaliDonem` ile `==` eşleştiriyordu. Yönetim sayfasından girilen akışta (`OnGuncelleClick`) yerel seçim eşleşmez → guard sessizce düşer → `ExecuteDevamEt` hiç koşmaz → update penceresi kapanır, kullanıcı yönetim sayfasında kalır. (UIA-artefakt şüphesi geçersiz değil ama bu yol gerçek akış hatasıdır.)
- **Çözüm:** ayna-yedeği — yerel eşleşmezse Continue'un garanti yazdığı `_selectedService` aynasına `OrdinalIgnoreCase` bakılır; eşleşirse `Selection` aynadan doldurulup `ExecuteDevamEt` koşar; alakasız DB'de geçiş yok. Regresyon: `ContinueNavigasyonTests` 3 test (ayna-yedeği / eşleşen-yerel / alakasız). Build 0/0, test 258/258, smoke OK. Tam canlı E2E (bayat DB + Continue → MainShell) kullanıcıda.
- Tarih: 2026-09-10

## FirmaShellViewModel god-class — SwitchToTenantAndUpdateState 5 sorumluluk (ÇÖZÜLDÜ — 2026-09-08 — Oturum 116)
- **Belirti:** `FirmaShellViewModel.cs:359` `SwitchToTenantAndUpdateState` seçim + `GetTenantDatabaseStateAsync` + `DialogService.ShowAsync` (onay) + `SwitchTenantAsync` (backup-önce-migrate) + `IsUpdating/UpdateStatus/Progress` 5 sorumluluk tek metotta → 379 satır, `AGENTS.md:1` 150 satır tetikleyici aşıldı, SRP ihlali. `IsUpdating` ayrı `IsBusy`'den dallanıyor.
- **Sebep:** 6.47 kimlikli yedek/Transfer/RestoreVerify akışları `FirmaShell` orkestratörüne yığıldı; önceki oturumlarda `FirmalarVM/MaliDonemVM` composition ile önlenmişti, bu metot atlandı.
- **Çözüm (uygulandı, Oturum 116):** `TenantSelectionViewModel` (seçim+kalıcılık+guard) + `TenantDatabaseUpdateCoordinator` (`ITenantDatabaseUpdateService` — Check→onay→Switch→arıza, `IDialogService` via `ICommonServices`, Business dialog bilmez) + `TenantUpdateProgressViewModel` ayrıştırıldı; orkestratör 465→183 (composition+navigation+forward). Ek: VM→Data `ExtractVersionFromMigration` Business'a taşındı; hardcoded "5 kolon" → `TenantMigrationDescriber` (gerçek migration `Up()` → operasyon → Türkçe); ölü `SecimOzeti/FirmaBilgisi/DonemBilgisi/VeritabaniBilgisi/KullaniciBilgisi/IsSelectedDonemKapali/VeritabaniBaglantiDurumu` + `IAuthenticationService` bağımlılığı silindi. `dotnet build` 0 hata, `dotnet test` 96/96. Kural: `ViewModelBase.UpdateProgress(double)` metodu varken VM'de `UpdateProgress` prop'u `CS0108` verir (prop adı `Progress` oldu); factory/prop isim çakışması (`Allowed`) `CS0102` verir.
- Tarih: 2026-09-08

## Yedek alınır ama liste hep boş — yazma/okuma dosya-adı deseni uyuşmuyor (ÇÖZÜLDÜ — 2026-09-07 — Oturum 112)
- **Belirti:** 2027'nin yedeği diskte varken panel "Henüz yedek yok" diyor; "Şimdi Yedekle" → "Yedek Alındı" bildirimi çıkıyor ama liste yine boş. Yenileme zinciri sağlamdı (`YedekAlAsync → YukleAsync → RefreshAllAsync → DonemDegistiAsync → YukleAsync`).
- **Kök neden:** `GenerateBackupFileName(databaseName)` çıplak ad üretiyor (`db-KODU_2027_{zaman}_{guid}.backup`), ama `GetBackupsAsync` deseni dosya adını `.db` ile bekliyordu (`db-KODU_2027.db_*.backup`) → `GetFiles` hiç eşleşmiyor. Aynı dosya `ParseDatabaseName`'deki `.db_` marker'ını da tutturamadığı için `GetAllBackupsAsync` onu `DatabaseName=""` ile Bilinmeyen'e düşürüyordu (çift belirti, tek neden).
- **Çözüm:** yazma formatı korundu; `GetBackupsAsync` iki deseni de tarar (çıplak + `.db`'li, dedupe) ve adayı regex ile çıkarılan ada göre birebir filtreler (önek çakışması yok); `ParseDatabaseName` iki formatı da kanonik ada (uzantısız) normalize eder. Kural: üreten ve listeleyen aynı dosya-adı grameriyle konuşur; desen değişince iki taraf + temizlik + öksüz taraması birlikte kontrol edilir.
- Tarih: 2026-09-07

## Bilinmeyen listesi bayat + yanlış sınıflı — sekme guard'ı + adsız parse (ÇÖZÜLDÜ — 2026-09-07 — Oturum 112)
- **Belirti:** kayıtlı yedekler Bilinmeyen'de; liste tazelenmiyor.
- **Kök neden (2 katman):** (1) `ParseDatabaseName` `.db_` marker bulamayınca `""` → kayıtlı dosya kayıtsız sayıldı (Oturum 112 desen fix'iyle kapandı). (2) `RefreshAllAsync` `TaraAsync`'i yalnız `IsArsivSekmesi` iken çağırıyordu; sekmeler Oturum 106'da kalktı, bayrak hep false → liste açılıştaki haliyle kaldı.
- **Çözüm:** guard kaldırıldı, her tazede `TaraAsync` (panel kalıcı görünür). Kural: UI'dan kalkan koşula bağlı tazeleme bırakılmaz.
- Tarih: 2026-09-07

## ItemsSource yenileme ListView görsel seçimini düşürür (ÇÖZÜLDÜ — 2026-09-07 — Oturum 108)
- **Belirti:** sol listede dönem seçince sağ bölme yenileniyor ama seçim kartındaki buton seçili kalmıyor (veri doğru — `SelectedDonem` korunuyor, yalnız görsel seçim kayboluyor).
- **Kök neden:** `TazeleSayaclar()` her çağrıda `AcikDonemler`'e YENİ liste örneği atıyor (`ArsivVM.Refresh` de öyle). ItemsSource değişince ListView iç seçimi null'a düşürüp TwoWay ile null geri yazar; setter guard'ı null'ı engeller (veri kurtulur) ama kontrolde görsel seçim geri gelmez — kimse kaynağı hedefe tekrar itmez.
- **Çözüm:** liste yenilemenin hemen ardından seçimi Id ile yeni listedeki örneğe eşitle + `NotifyPropertyChanged(nameof(SelectedDonem))` ile binding'i hedefe tekrar duyur (`SecimiListeyeYenidenDuyur`). Kural: `List<T>` yenileyen her VM akışının sonunda seçim tekrar duyurulur.
- Tarih: 2026-09-07

## Grid otomatik yerleştirme + ColumnSpan pile-up yapar (ÇÖZÜLDÜ — 2026-09-07 — Oturum 106)
- **Belirti:** 2 kolon + `ColumnSpan=2` hero'lu kart grid'inde hero/yedek/analiz kartları AYNI Y'de üst üste render oldu (UIA rect kanıtlı: üçü de Y≈510). Build 0 hataydı, XAML doğru görünüyordu.
- **Kök neden:** otomatik satır yerleştirme `ColumnSpan` ile güvenilmez — sonraki kartlar 1. satıra yığıldı (satır yükseklikleri ~0, içerik taşarak üst üste bindi).
- **Çözüm:** kart grid'indeki TÜM öğelere explicit `Grid.Row` + `Grid.Column` (hero: Row=0 Span=2). Kural: 3+ kartlı grid'lerde otomatik yerleştirmeye güvenilmez, satır/sütun her öğede açık yazılır.
- Tarih: 2026-09-07

## PS UIA metin aramada Türkçe `ş` ASCII eşleşmeyi bozar + dialog popup'ı Descendants'ta görünmez (DERS — 2026-09-06 — Oturum 103)
- **Belirti:** Teşhis butonunu UIA ile bulma 2 kez `NOT_FOUND` verdi; "Hayır" popup metni hiç bulunamadı.
- **Kök neden:** (1) `Contains("eshis")` ≠ `eşhis` — `ş` harfi ASCII parçasıyla eşleşmez (doğru parça `his` idi). (2) `ContentDialog` popup içerikleri `MainWindowHandle` kökünden `Descendants` ile görünmeyebilir (buton rect'leri görünür, metinleri görünmez).
- **Çözüm:** UIA metin aramada ASCII-güvenli parça kullan (`his`, `Hay`+uzunluk); popup kapatmada buton rect dökümü + `SetForegroundWindow` ile AYNI scriptte koordinat tıkı. Tık öncesi foreground olmazsa tıklama IDE'ye gider. Kural: otomasyon script'i tek parça (öne al → taze koordinat → tık → bekle → shot).
- Tarih: 2026-09-06

## VSM GoToState ctor/navigate'te False döner — ilk çağrı Loaded'da (ÇÖZÜLDÜ — 2026-09-06 — Oturum 97)
- **Belirti:** yönetim segment/sekme aktif vurgusu hiç görünmüyordu (üç buton da pasif). Kod doğru görünüyordu (Root atanıyor, state adı doğru, VM flag'leri true).
- **Kök neden (probe kanıtlı):** `GuncelleSegmentGorunum state=TumuAktif ok=False loaded=False` — `GoToState` görsel ağaç bağlanmadan (`Root` setter'ı `OnNavigatedTo`'da) çağrılıyordu; sonradan değişiklik olmadığı için vurgu hiç uygulanmadı.
- **Çözüm:** `TumuHeroPanel` + `MaliDonemYonetimView` ctor'larına `Loaded => Guncelle*Durumu()`. Kural: VSM ilk uygulaması `Loaded`'da (gölge-receiver kalıbıyla aynı).
- Tarih: 2026-09-06

## Hover'da ölçü değişimi ToolTip timer'ını öldürür (ÇÖZÜLDÜ — 2026-09-06 — Oturum 97)
- **Belirti:** yönetim satır ikon butonlarında fare-hover tooltip'i açılmıyordu; tıklama + klavye-odak tooltip'i çalışıyordu.
- **Kök neden (hipotez, kod-kanıtlı):** Oturum 87'nin 1.5px hover halkası `BorderThickness` değiştiriyordu → fare sabitken buton büyür → enter/exit döngüsü → tooltip timer sürekli sıfırlanır. Odakta boyut değişimi olmadığı için odak probu temizdi (yanıltıcı).
- **Çözüm:** `GhostPillButtonStyle` VSM'inde ölçü değişimi kaldırıldı (yalnız renk) + satır butonlarındaki `BorderThickness="0"` yereller silindi. Kural: hover/pressed state'leri ÖLÇÜ değiştiremez (renk/opaklık serbest). Fare teyidi kullanıcıda.
- Tarih: 2026-09-06

## ContentDialog DefaultButton="Primary" PrimaryButtonStyle'ı ezer (ÇÖZÜLDÜ — 2026-09-06 — Oturum 96)
- **Belirti:** 4 dialogda `PrimaryButtonStyle` (şablonlu `InventDarkButtonStyle`) mavi render oluyordu (`#3285CC` piksel-kanıtlı). Oturum 95'te code-behind'den verilen şablonsuz Danger kırmızı olmuştu → "şablon düşüyor" sanıldı.
- **Kök neden (probe kanıtlı):** `PrimaryButtonStyle` property'DE duruyordu (`Loaded` probu: null değil, `setters=10`) ama render maviydi → sorun stilde değil, WinUI `DefaultButton="Primary"` mekanizmasında: default butona `AccentButtonStyle` basılıp özel stil eziliyor. DeleteGuard (`DefaultButton="Close"`) kontrolü doğruladı: Vazgeç mavi + Dönemi Sil kırmızı.
- **Çözüm:** `InventDialogPrimaryStyle`/`InventDialogDangerStyle` (setters-only, `Buttons.xaml`) + 3 dialogda `DefaultButton="Primary"` kaldırıldı; DeleteGuard'da Close default korundu (Enter=iptal güvenli varsayılan). YeniDonem'de Enter=onay `KeyDown→Hide()+EnterOnay` bayrağı + 2 caller koşuluyla korundu. Kural: **tehlikeli dialogda Enter=iptal, güvenli dialogda Enter=onay; DefaultButton Primary ASLA.**
- Tarih: 2026-09-06

## Style ThemeResource ile verilemez (ÇÖZÜLDÜ — 2026-09-06 — Oturum 96)
- **Belirti:** `YeniFirmaDialog.xaml:36` `Style="{ThemeResource MuhasibCardStyle}"` + `QuickSistemDbDiagDialog.xaml:127` `Style="{ThemeResource MuhasibPrimaryButtonStyle}"` — stil `ThemeResource` ile çözülemez (stiller normal kaynak, tema sözlüğü değil).
- **Çözüm:** İkisi de `StaticResource`'a çevrildi. Kural: `Style=` her zaman `StaticResource`; `ThemeResource` yalnız tema fırçalarında/renklerde.
- Tarih: 2026-09-06

## Ölü stil temizliği kırığı: CardPictureRadius (ÇÖZÜLDÜ — 2026-09-06 — Oturum 96)
- **Belirti:** `Yeni Firma` penceresi boş açılıyordu (25 sn'de içerik yok). Logda `XamlParseException: Cannot find Resource CardPictureRadius` — Oturum 82 ölü temizliğinde token silinmiş ama `FirmaCard`/`FirmalarCard` 5'er yerde kullanıyordu. Build temizdi (StaticResource eksikliği derlemede değil çalışmada patlar).
- **Çözüm:** `FirmaCard` InventEase'e çevrildi (68 sayısal boyut + teal + ThemeResource fırçalar + daire ikonlar) — token geri eklenmedi, borç kapatıldı. Ders: ölü stil silinirken grep 0'lanan ANAHTAR değil, TÜKETİM yerleri de doğrulanır (build yetmez, ilgili ekran canlı açılır).
- Tarih: 2026-09-06

## XAML hover/tooltip sentetik otomasyonla test edilemez (DERS — 2026-09-05 — Oturum 87)
- **Belirti:** `SetCursorPos`/`SendInput` hover'da piksel diff 0 + UIA `ToolTip` 0 (Login dahil, ScrollViewer içi/dışı fark etmez) → "tooltip/hover çalışmıyor" sanıldı (Oturum 85 + 4 oturumluk saga).
- **Gerçek:** kontrol deneyi — Microsoft Notepad'de (WinUI3) de AYNI sonuç → sentetik girdi bu makinede XAML hover süremiyor (sistem caption hover'ı çalışıyor, sorun XAML island girdi hattında). Metodoloji geçersizdi, uygulama değil.
- **Kanıt:** klavye odağı (Tab) tooltip'i açtı — UIA `ToolTip: 1 [Şifreyi göster / gizle]` + render screenshot'ta görünür → servis + timer + render sağlam; gerçek farede çalışır.
- **Kural:** XAML hover/tooltip için fare simülasyonu KULLANILMAZ; doğrulama = klavye-odak probu (servis/render) + gerçek fare (kullanıcı). DetailsWindow UIA ağacında görünmez → `EnumWindows + AutomationElement.FromHandle`; `.ps1`'e BOM gerekli (PS 5.1 Türkçe desenler için).
- Tarih: 2026-09-05

## Code-behind ölü gölge referansı → XamlCompiler kaskadı (ÇÖZÜLDÜ — 2026-09-05 — Oturum 84)
- **Belirti:** `dotnet build` **21 hata**: 1× `CS0103 'LoginCardShadow'` + App.xaml'de 19× sahte `WMC0001 Unknown type '*Converter'` + 1× `WMC9999` null-ref.
- **Sebep:** Kart stili değişirken (`ModernCard` → `GlassCardStyle` → `PanelCardStyle`) XAML'deki `ThemeShadow x:Name` silindi ama `LoginView.xaml.cs:27` `LoginCardShadow.Receivers` kaldı. Tek gerçek hata CS0103'tü; MarkupCompile düşünce derleyici App.xaml birleştirilmiş sözlüklere sahte WMC0001 + WMC9999 üretti (gerçek dosyada sorun yoktu).
- **Çözüm:** Ölü receiver bloğu silindi → **0 Uyarı 0 Hata**. Kural: kart/stil değişiminde code-behind gölge (`Receivers.Add` + `Loaded`) aynı commit'te temizlenir; WMC0001 kaskadı görülünce önce gerçek CS hatası aranır.
- Tarih: 2026-09-05

## Scoped DbContext çakışması — "second operation started before previous completed" (ÇÖZÜLDÜ — 2026-09-05 — Oturum 86)
- **Belirti:** Windows `Application` logunda gün boyu tekrarlayan `EventId 10100` — `SistemDbContext` sorgu iterasyonu sırasında `InvalidOperationException` (eşzamanlı ikinci operasyon).
- **Sebep (`MaliDonemListViewModel`):** `RefreshAsync` okuması (liste sorgu iterasyonu) + `BackfillFileSizeAsync` yazımı + üst üste `RefreshAsync` (mesaj + kullanıcı tetiklemeli) aynı Scoped context'i paylaşıyordu.
- **Çözüm:** VM-içi `_dbGate = new SemaphoreSlim(1,1)` — Refresh + Backfill aynı kapıdan serileşti. Kapı `ConfigureAwait`'siz (Oturum 78 dersi: `NotifyPropertyChanged` UI thread'inde kalır). Tenant dosya analizi kendi bağlantısını açtığı için kapı dışında (fire-and-forget korundu, UI bloklanmaz). Ölü `EnrichTenantDbStatusesAsync`/`AnalyzeAndStampAsync` (Task.WhenAll deseni) silindi. Kural: Scoped context'e dokunan tüm VM akışları tek kapıdan geçer.
- Tarih: 2026-09-05

## Tooltip görünmüyor + hover belirsiz (ÇÖZÜLDÜ — 2026-09-05 — Oturum 86)
- **Belirti (canlı):** primary butonda 1.8sn hover sonrası tooltip yok (screenshot); hover renk farkı (`#0F6CBD`→`#115EA3`) gözle ayırt edilemiyor.
- **Sebep (2 ayrı konu):** (1) Test edilen Login Giriş butonunda `ToolTipService.ToolTip` hiç tanımlı değildi — "görünmedi" beklenen davranıştı, framework bug'ı değil. (2) Hover token'ları algısal olarak birbirine çok yakındı; ayrıca `GhostPillButtonStyle` hover zemini `Subtle #F8FAFC` beyaz kartta görünmezdi.
- **Çözüm:** (1) Eksik 8 tooltip eklendi (Giriş/Reveal/Teşhis/QuickLogin×2/Yeni Firma/Yeni Dönem/DevamEt) + `DbAnalizDetay` tooltip'e `TargetNullValue` (null iken anlamlı metin). Not: disabled butonda tooltip WinUI platform gereği gösterilmez. (2) Light hover `#115EA3→#094D8F`, pressed `#0B4A8A→#073A6C` (belirgin koyu basamaklar; Dark zaten ayrık, ellemedi); Ghost hover `Subtle→PrimaryLight`, pressed `PrimaryLight→PrimaryBorder`. Token adları aynı → tüm tüketiciler otomatik devraldı.
- Tarih: 2026-09-05

> Not: `MuhasibPro-master` 2026-08-28’de silindi — aşağıdaki “Master’dan” bölümü arşivdir, sadece HATALAR.md’de korunur.

---

## Master’dan Tespit Edilen ve Yeni Projeye Taşınmayacak Bug’lar (arşiv — 2026-08-28 öncesi)

### TenantSQLiteBackupManager — copy-paste veri kaybı
- `GetBackupsAsync` deseni tam yol prefix → backup listesi bulunamıyor. `RestoreBackupDetailsAsync` hata bloğunda Sistem DB yolu kullanılıyor.

### AuthenticationRepository.Register — yanlış değişken
- `if (ePosta != null) result |= UsernameAlreadyExists` — ePosta kontrolü, username çakışması raporlanmaz. `RolId=1` seed dışı.

### MaliDonemRepository.RestoreMaliDonemAsync
- `Id > 0` iken `AddAsync` → mevcut Id ile yeni kayıt.

### Atılan sorgu sonuçları
- `FirmaRepository/MaliDonemRepository`: `Include/Where` sonuçları atanmıyor → etkisiz.

### DatabaseUtilityExtensionsHelper.CalculateSimpleHash
- `Math.Abs(int.MinValue)` → OverflowException.

### DatabaseBackupManager
- `GC.Collect()` gereksiz, check-then-act yarışı, header kontrolü yok.

### Hesap entity
- `KullaniciId1` artık kolon.

### DI captive dependency
- Repository Singleton, DbContext Scoped → yeni projede Scoped/Transient.

---

## Yeni Projeye Taşınan Düzeltmeler

### MaliDonem kart vitrini gerçek zamanlı dolmuyor (Yedek → SonYedek/Boyut `-`)
- **Belirti:** `MaliDonemlerListControl` dönem kartlarında "Son Yedek:" (ve "Boyut") alanı gerçek zamanlı dolmuyor; Yedekle'ye tıklayınca "Yedek Alındı" bildirimi çıkıyor ama karttaki alan hâlâ `-`.
- **Sebep (2 katman):** (1) `OnBackupClick` yedek başarılı olunca yalnızca bellek içi `model.TenantDetails.SonYedekTarihi = DateTime.Now` atayıp `RefreshAsync()` çağırıyordu; Global.db `MaliDonemler` satırı hiç güncellenmiyordu → Refresh DB'den yeni model örnekleri üretince (`GetMaliDonemlerWithFirmaId` satırdan okuyor) satırdaki `NULL` geri geliyordu. (Arşivleme `UpdateMaliDonemAsync(model)` ile DB'ye yazdığı için çalışıyordu — Yedek'te o adım yoktu.) (2) Yeni dönem akışı (`TenantSQLiteDatabaseService.CreateNewTenantDatabaseAsync`) satırı DB dosyası **oluşturulmadan önce** ekliyor ve sonrasında `DosyaBoyutu` satıra işlenmiyordu → "Boyut" hep `-`.
- **Çözüm (kullanıcı kararı: kaynak = DB satırı, kapsam = SonYedek + Boyut):** (1) `OnBackupClick` → `TenantDetails.SonYedekTarihi=DateTime.Now` + `DosyaBoyutu=response.Data.BackupFileSizeBytes` → `IMaliDonemService.UpdateMaliDonemAsync(model)` ile satıra yaz → sonra `RefreshAsync()`; (2) `TenantSQLiteBackupManager.CreateBackupAsync` başarıda `BackupFileSizeBytes = FileInfo(sourcePath).Length`; (3) `TenantSQLiteDatabaseService.CreateNewTenantDatabaseAsync` DB dosyası oluşunca `PersistTenantFileSizeAsync` (satıra `DosyaBoyutu` best-effort yazar).
- Tarih: 2026-09-02

### Button hover beyaza düşüyor (Button.Resources override güvenilmez)
- **Belirti:** `QuickSistemDbDiagDialog` Yedek Al butonu mavi yerine beyaz görünüyor (özellikle hover'da); buton içi beyaz ProgressRing de görünmüyor. `Button.Resources` içinde `ButtonBackgroundPointerOver` override'ı `RequestedTheme="Light"` dialog'da çalışmadı → varsayılan `ButtonBackgroundPointerOver` (Light'ta beyazımsı `SubtleFill`) basılıyor.
- **Sebep:** WinUI 3'te varsayılan Button şablonunun PointerOver state'i `{ThemeResource ButtonBackgroundPointerOver}`'ı instance `Button.Resources` üzerinden çözümlemeyebiliyor; tema-ıraksak scope'larda (ContentDialog `RequestedTheme`) override güvenilir değil.
- **Çözüm (token tabanlı, hardcode yok):** `Styles/Buttons.xaml` → yeni **`MuhasibPrimaryButtonStyle`** — özel `ControlTemplate` (Border root + `ContentPresenter`), VisualState'ler `Normal/PointerOver/Pressed` → `MuhasibPrimaryHoverBrush`/`MuhasibPrimaryPressedBrush` (DesignTokens). `DesignTokens.xaml` → `MuhasibOnPrimaryColor` (Light+Dark #FFFFFF) + `MuhasibOnPrimaryBrush` (birincil üstü metin/ikon/ring). BackupButton `Style="{StaticResource MuhasibPrimaryButtonStyle}"` + iç `Foreground` `{ThemeResource MuhasibOnPrimaryBrush}`. Disabled state tanımlanmadı → yedek alırken buton mavi kalır, beyaz ring görünür.
- Tarih: 2026-08-31

### Toast overlay — kart dışı siyah şerit (WinUI3 saydam pencere desteklenmez)
- **Belirti:** `ToastOverlayWindow` tek-pencere overlay'inde kart dışı alan siyah/gri şerit görünüyor, kart genişliği taşıyor, X butonu sadece ilk kartta görünüyor. `DwmExtendFrameIntoClientArea(-1)` ve `LWA_COLORKEY` ikisi de çözmedi. Sonrası: ilk toast "bir pencere" gibi (çerçeve + başlık çubuğu), kapatma butonu kırmızı (native close).
- **Sebep:** (1) WinUI3 (Windows App SDK) gerçek per-pixel saydam pencereyi desteklemez; `Background="Transparent"` XAML adasının opak siyah zeminini kaldırmaz. `DwmExtendFrameIntoClientArea`/`LWA_COLORKEY` GDI/layered-window teknikleridir, WinUI3 compositor'ı (`DesktopChildSiteBridge`) ile uyumsuz. (2) `AppWindowTitleBar` renklerini şeffaf yapmak caption butonlarını (özellikle close'un kırmızı hover'ı) gizlemez.
- **Çözüm:** `Dwm`/`LWA_COLORKEY`/`MARGINS`/`EnableTrueTransparency` silindi → `SystemBackdrop = DesktopAcrylicBackdrop()` (fallback `MicaBackdrop`); `RootGrid Background="Transparent"` akrilik zemin üzerinden görünür. **`presenter.SetBorderAndTitleBar(false, false)`** ile çerçeve + başlık çubuğu + caption butonları tamamen gizlendi (`HideCaptionButtons`/`ExtendsContentIntoTitleBar` kaldırıldı; sıralama: `ApplyThemeToWindow` önce, `SetBorderAndTitleBar` son). Kart `Width="360"`+`HorizontalAlignment="Left"` → `Stretch`.
- **Durum (2026-08-30):** Akrilik zemin light masaüstünde "arkaplan beyaz" görünüyor — kullanıcı ret etti. **Ayrı-pencere yaklaşımı terk edildi; sıradaki çözüm ana pencere içinde WinUI3 `UserControl` overlay (saydam, ThemeShadow, yuvarlak kart).** `ToastOverlayWindow.*` bu geçişte silinecek.
- Tarih: 2026-08-30

### XamlCompiler sessiz çökme (exit 1, çıktısız)
- Çift `Background` tanımı (attribute + `<Grid.Background>`) → XamlCompiler düşüyor. `LoginView` Border’ında düzeltildi.
- Tarih: 2026-08-21

### WindowsAppSDKSelfContained x64
- slnx AnyCPU → `RuntimeIdentifier win-x64` eklendi.

### SQLite Pooling
- `Pooling=false` — havuz dosya kilidi `File.Move` engelliyordu.

### Zero-Width karakterler
- `DatabaseGenerat‌​ed` içinde U+200B/ZWNJ → temizlendi.

### Navigate Frame boş
- `MainWindow.Content` Grid, `ActivationService` Frame araması `is Frame` ile bulamadı → blank window. `MainWindow.RootFrame` eklendi, `Grid.Children.OfType<Frame>` fallback.

### FirmaShellView “Test” placeholder
- `FirmaShellView.xaml` 772B test içeriği → `.bak2` 20623B restore edildi.

### MainShellView deferred element
- `NavigationView.MenuItems` içinde `x:Load="false"` Template → `Cannot add DeferredElement` → kaldırıldı.

### DbContext Extensions — Analysis merkezi
- `DbContextDiagnosticsExtensions` ve `DbContextOperationsExtensions`’ta tablo analizi, validasyon, migration kontrolü duplicate idi. `Database/Extensions/DbContextAnalysisExtensions.cs` eklendi — tek kaynak `AnalyzeDatabaseCoreAsync<T>` (cache, batch, progress, validasyon birleşik), `IsValidTableName`/`TableExists`/`TableHasRowsSafe`/`GetAllTablesCached` merkezileştirildi.
- Tarih: 2026-08-21

### MigrationExtension finalAnalysis bug
- `DbContextOperationsExtensions.cs:270` `finalAnalysis = GetConnectionFullStateAsync(..., Array.Empty<string>())` → migration sonrası yeni tablo/versiyon analiz edilmiyordu, eski `analysis.IsDatabaseExists/DatabaseValid` ve boş tablo listesi kullanılıyordu.
- Çözüm: `ExecuteMigrationsWithBackupCheckAsync`’a `string[] tablesToCheck = null` eklendi, final analizde `isDatabaseExists:true, databaseValid:true, tablesToCheck: finalTables ?? PendingMigrations` ile taze analiz; `SistemMigrationManager.cs:118` ve `TenantSQLiteMigrationManager` çağrıları `TablesToCheck` ile güncellendi.
- Tarih: 2026-08-21

### Backup — haftalık otomatik yedek
- `TenantSQLiteBackupManager` ve `SistemBackupManager`’a `EnsureWeeklyBackupAsync` eklendi: `GetLastBackupDate() == null || (UtcNow - last).TotalDays >=7` ise `CreateBackupAsync(Automatic)` tetiklenir. `ITenantSQLiteBackupManager:25` ve `ISistemBackupManager:20` contract’ları güncellendi.
- Tarih: 2026-08-21

### ApplicationPaths god-class parçalama
- 433 satırlık `ApplicationPaths` (18 public metod) 4 provider’a bölündü: `Helpers/Paths/BasePathHelper.cs` (kök klasör/güvenli dosya), `DatabaseStructureProvider.cs` (Databases/Tenant/Sistem yolları + Sanitize), `BackupPathProvider.cs` (Backups/TenantBackups), `DatabaseValidationProvider.cs` (IsValid/Size/Header/WAL cleanup). `ApplicationPaths` artık facade olarak delegate ediyor — `IApplicationPaths` sözleşmesi korundu, DI uyumlu.
- Tarih: 2026-08-21

### DatabaseUtilityExtensionsHelper.CalculateSimpleHash overflow
- `Math.Abs(int.MinValue)` → `OverflowException` (unchecked’de bile negatif kalır). `hash == int.MinValue ? int.MaxValue : Math.Abs(hash)` ile düzeltildi, `DatabaseUtilityExtensionsHelper.cs:62`.
- Tarih: 2026-08-21

### Batch ambiguity
- `DbContextAnalysisExtensions.Batch` ile `DbContextDiagnosticsExtensions.Batch` aynı imzada çakışıp `CS0121 Ambiguous` veriyordu. `Diagnostics`’taki duplicate `Batch` kaldırıldı, merkezi olan kullanıma bırakıldı.
- Tarih: 2026-08-21

### Kullanici.RolId kaldırma sonrası Data/Business uyumsuzluğu
- `SistemDbContext.cs:30` `RolId` HasData, `KullanicilarConfiguration:11` `HasOne(Rol)`, `UserRepository:20` `Include(Rol)`, `AuthenticationService:108` `entity.Rol/RolId` → `KullaniciFirmaRoller` koleksiyonuna göre düzeltildi (composite key, `Include(...).ThenInclude(Rol)`).
- Tarih: 2026-08-21

### MaliDonemSagaManager — hayali entity alanları (AI Studio)
- `MaliDonemSagaManager.cs:109` `AppLog.LogTuru/Kaynak/Mesaj`, `LogType.Information`, `MaliDonem.DonemKodu/BaslangicTarihi/BitisTarihi/DbDosyaAdi/DbDosyaYolu/Aciklama/IsDeleted` → gerçek `MaliDonem.DatabaseName/DatabaseType/Durum/DosyaBoyutu/ArsivlendiMi` ve `AppLog.Type/Source/Action/Message/User` ile uyumsuzdu → `DatabaseName = donemKodu ?? $"{maliYil}.db"`, `KaydedenId=1` ile düzeltildi.
- Tarih: 2026-08-22

### DI ambiguous TenantSQLiteBackupManager (CS0104)
- `Managers.ITenantSQLiteBackupManager` (VACUUM INTO) ile `Database.TenantDatabase.ITenantSQLiteBackupManager` (eski backup) aynı kısa isimle çakışıp `CS0104` verdi → `AddDbManagerHostBuilderExtensions.cs:75` tam ad ile iki ayrı `AddScoped` kayıt yapıldı.
- Tarih: 2026-08-22

### ModuleLicenseViewModel — missing using & override
- `ModuleLicenseViewModel.cs:52` `ShellArgs`/`ObservableObject` için `using` eksik → `CS0246`; `override LoadAsync(ShellArgs)` base’de yok → `CS0115` → `using Business.DTOModel` + `using ViewModels.Shell` eklendi, override kaldırıldı.
- Tarih: 2026-08-22

### ModuleLicenseService — missing LogServiceExtensions using
- `ModuleLicenseService.cs:60` `SistemLogExceptionAsync` extension `using LogServiceExtensions` olmadan `CS1061` → eklendi; ayrıca `GlobalAyarlar` `KaydedenId` zorunlu alan eksik → `KaydedenId=1, AktifMi=true` eklendi.
- Tarih: 2026-08-22

### Ayarlar/BelgeNumara/VarsayilanDegerler PK eksik (no primary key)
- `Ayarlar`/`BelgeNumara`/`VarsayilanDegerler` `BaseEntity` olmadan `[Key] Id` yok → `The entity type 'Ayarlar' requires a primary key` → her birine `[Key] Id` eklendi.
- Tarih: 2026-08-23

### DbContext SQL non-composable (no such column: s.Value / FromSql)
- `DbContextAnalysisExtensions.cs:202` `SELECT 1 FROM ...` + `AnyAsync()` `FromSql non-composable` ve `SELECT COUNT(*) AS Value` `s.Value` hatası → `SELECT 1 AS Value` + `AsEnumerable().Any()` ve `PRAGMA` doğrudan `SqliteConnection` ile düzeltildi; `SistemMigrationManager.cs:25` `TablesToCheck` gerçek tablo adlarına (`Kullanicilar` vb.) çevrildi.
- Tarih: 2026-08-23

### EnsureCreated vs Migrate (AppDbVersion zaten var)
- `DbContextDatabaseCreateExtensions.cs:24` `EnsureCreated` history oluşturmadığı için sonraki `Migrate` `AppDbVersion zaten var` hatası → sadece `MigrateAsync()` + `already exists` catch'inde history `INSERT OR IGNORE` ile onarıldı.
- Tarih: 2026-08-23

### LoginView textbox görünmez + şifre odaklanınca kullanıcı adı kayboluyor
- `NamePasswordControl.xaml:22` `TextChanged` handler ilk boş değerle `ViewModel.Username`'ı siliyor + `Background` focused koyu → `x:Bind` TwoWay tek başına bırakıldı, `TextBox.Resources TextControlBackgroundFocused=White` eklendi, `LoginView.xaml:281` `background/foreground` `Visibility Collapsed` yapıldı.
- Tarih: 2026-08-23

### LoginView inline hata ve CanLogin
- Hatalı şifre `DialogService.ShowAsync` dialog olarak gösteriliyordu, boşken buton aktifti → `LoginViewModel.cs:50` `ErrorMessage`/`CanLogin` + `NamePasswordControl.xaml:22` kırmızı `Border #FEF2F2` text + `IsEnabled="{x:Bind CanLogin}"` ile düzeltildi.
- Tarih: 2026-08-23

### LoginView şifre göz ikonu (Reveal)
- Şifre alanı maskeli kalıyordu, göz ikonu yoktu → `NamePasswordControl.xaml:64` `PasswordBox Padding 36,8,40,8` + sağa `Button revealButton` (`Glyph E052→E7B3` toggle, `PasswordRevealMode.Visible/Hidden`) + `NamePasswordControl.xaml.cs:77` `OnToggleReveal` eklendi; `UIAutomation` ile `revealButton` ve login akışı doğrulandı.
- Tarih: 2026-08-23

### MaliDonemSagaManager EnsureCreated → Migrate
- `MaliDonemSagaManager.cs:97` `EnsureCreatedAsync()` history oluşturmadığı için `Migrate` sonrası `already exists` hatası; `DbContextDatabaseCreateExtensions.cs:24` ile aynı `MigrateAsync()` + `already exists` catch'inde `SqliteConnection` ile `__EFMigrationsHistory` `INSERT OR IGNORE` history onarımı eklendi.
- Tarih: 2026-08-23

### AutoMapper NU1903 GHSA-rvv3-g6hj-g44x
- `MuhasibPro.Business.csproj:14` `AutoMapper 16.0.0` GHSA yüksek seviye açığı `dotnet build`'de `NU1903` uyarısı veriyordu (2 uyarı, Faz 0-6 0/0 hedefine engel).
- Geçici çözüm: `MuhasibPro.Business.csproj:7` `<NoWarn>NU1903</NoWarn>` eklendi → `dotnet build MuhasibPro.slnx` şimdi `0 uyarı 0 hata`.
- Kalıcı çözüm planı: `AutoMapperSistemMapping.cs:9` 5 `CreateMap` (`SistemLog`, `Kullanici`, `Hesap`, `Firma`, `MaliDonem`) manuel `ToModel`/`ToEntity` extension'larına veya `Mapster` (`7.4.0+`, kaynak üretimli, AOT uyumlu) geçiş; `Business` katmanında `IMapper` kullanımı yok, sadece `Profile` var — geçiş düşük riskli. Bir sonraki Faz B öncesi `Mapster` POC + `dotnet remove package AutoMapper` + `NoWarn` kaldırma.
- Tarih: 2026-08-23

### LoginView regresyon — Grid.Row/ScrollViewer/Foreground kaybı
- `LoginView.xaml:110` `Grid.Row="2"` + `LoginView.xaml:18` `MinWidth 280` + `LoginView.xaml:60` sol `ScrollViewer` + `NamePasswordControl.xaml:39` `TextControlForeground` → son commit sonrası regresyonla kaybolmuş, `korkutomer` beyaz-beyaz görünmez + `QuickLoginPanel` logoyla çakışıyor + `CheckBoxes 2` + `Width 380` sabit kalmış (screenshot `109174` bytes, `Descendants 31`).
- Çözüm: `NamePasswordControl.xaml:39` `Foreground="#FF0F172A"` + `TextControlForeground` resource'ları, `LoginView.xaml:60` sol `ScrollViewer` + `Grid.Row="2"` + `MinWidth 280`, `LoginView.xaml:118` sağ `MaxWidth/MinWidth` + `ScrollViewer` kapanış tag'ı eklendi; `UIAutomation Descendants 34`, `CheckBox 1`, `revealButton` toggle, screenshot `106051` bytes ile doğrulandı (`Login bitti`).
- Tarih: 2026-08-23

### GetFirmalarWithMaliDonemler — ölü Include + eksik projeksiyon (HATALAR deseni: atılan sorgu sonuçları)
- `FirmaRepository.GetFirmalarWithMaliDonemler:94` `query.Include(f => f.MaliDonemler);` sonucu atanmıyor (etkisiz) + projeksiyonda `AktifMi`/`FirmaId`/`Il`/`VergiNo`/`VergiDairesi` yoktu (service `AktifMi = md.AktifMi` default `true` basıyordu) + `Where(md => md.AktifMi)` KAPALI dönemleri kartlardan gizliyordu.
- Çözüm: ölü Include kaldırıldı (Select projeksiyonu zaten dönemi çekiyor), `FirmaId/AktifMi/Il/VergiNo/VergiDairesi` projeksiyona + service map'e eklendi, AktifMi filtresi kaldırıldı.
- Tarih: 2026-08-24

### MaliDonemSagaStep + LogService DI — Global.db INSERT 'database is locked' (ÇÖZÜLDÜ)
- Belirti: Dönem oluşturma sagasında `MaliDonemSagaStep.CreateNewMaliDonemAsync` içindeki Global.db `INSERT INTO "MaliDonemler"` **30,046ms** ile `SQLite Error 5: 'database is locked'` düşüyor → "İşlem Başarısız". Rollback doğru çalışıyordu (tenant dosyası silindi). Kullanıcı tespiti: "kilitlenmeden ziyade LogService DI kayıtlarında sorun olabilir" — haklı.
- Kök neden (captive dependency + çift lock):
  1. `AddServicesHostBuilderExtensions.cs:22` `IFirmaService`/`IMaliDonemService`/`ILogService` **Singleton** iken `IUnitOfWork<SistemDbContext>`/`ISistemLogRepository` **Scoped** — Singleton, Scoped DbContext'i root provider'dan capture ediyor (farklı `SqliteConnection`). `AddCommonServiceHostBuilderExtensions.cs:25` aynı `ILogService`'i tekrar Singleton kaydedip Business'taki Scoped'ı eziyordu (son kayıt kazanır).
  2. `MaliDonemSagaStep.cs:49` `using var transaction = await _unitOfWork.BeginTransactionAsync()` (Scoped UnitOfWork) açılırken `await _maliDonemService.UpdateMaliDonemAsync(maliDonem)` **farklı connection** (Singleton'ın capture ettiği) üzerinden `INSERT` deniyor → Scoped transaction SHARED lock tutarken Singleton RESERVED alamıyor → 30s busy timeout.
  3. Global.db WAL kapalıydı (tenant DB'lerde `PRAGMA journal_mode=WAL` vardı, Global.db'de yok) → DELETE modunda kilit daha agresif. `App.xaml.cs:154` `ILogService` root'tan `GetRequiredService` ile Scoped çözmeye çalışınca ayrıca `InvalidOperationException: Cannot resolve scoped service from root` riski.
- Çözüm (2026-08-25):
  - `AddServicesHostBuilderExtensions.cs:22` `IFirmaService`/`IMaliDonemService`/`IFirmaWithMaliDonemSelectedService` **Scoped**'a çevrildi; `ILogService`/`IAppLogService`/`ISistemLogService` **Scoped** (Business + Common her ikisinde). Böylece her pencere `ServiceLocator` scope'unda aynı `SistemDbContext` paylaşılır, captive ortadan kalkar.
  - `AddCommonServiceHostBuilderExtensions.cs:26` aynı 3 log servisi **Scoped** yapıldı (eski Singleton duplicate düzeltildi).
  - `MaliDonemSagaStep.cs:45` çift `BeginTransaction` + ek `SaveChangesAsync` kaldırıldı — servis zaten `SaveChanges`+log yapıyor; saga step artık sadece `await _maliDonemService.UpdateMaliDonemAsync(maliDonem)` (ve `Delete/Restore`) çağırıyor, ayrı transaction yok → aynı connection içinde tek yazım.
  - `AddDbManagerHostBuilderExtensions.cs:34` `SistemDbContext` oluşturulurken `File.Exists` ise `PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA busy_timeout=5000;` best-effort çalıştırılıyor (tenant WAL ile tutarlı).
  - `App.xaml.cs:154` `LogService` property ve `OnUnhandledException` artık `using var scope = _host.Services.CreateScope()` ile Scoped LogService çözüyor.
- Doğrulama: `dotnet build MuhasibPro.slnx --nologo` **0 uyarı 0 hata** (2026-08-25). Canlı E2E (dönem oluşturma → Global.db INSERT → tenant `Migrate`) bir sonraki oturumda UIA ile test edilecek.
- Tarih: 2026-08-25

### ViewModel AppLog → SistemLog yanlış kanal + AppLogService tenant guard eksik (ÇÖZÜLDÜ)
- Belirti: Tenant DB kurulmadan/seçilmeden `MaliDonemCreationViewModel.cs:149` `LogAppExceptionAsync("MaliDonemCreation", "StartCreation", ex)` ve `MaliDonemDeletionViewModel.cs:127` `LogAppExceptionAsync("MaliDonemDeletion"...)` çağrılıyordu. `AppLogService` `AppDbContext` (tenant DB, 69 tablo + `AppLogs`) ister; tenant yokken `AddDbManagerHostBuilderExtensions.cs:97` fallback `factory.CreateDbContext("Sistem")` ile `AppDbContext` **Sistem.db**'ye bağlanıyor → `INSERT INTO "AppLogs"` → `SQLite Error 1: 'no such table: AppLogs'` (EF loglarında gürültü, ViewModelBase `try/catch` ile yutulsa da hatalı kanal). Kullanıcı hatırlatması: "Tenant db kurulmadan önce bir viewmodel içinde AppLogService kullanılıyor ve bu yüzden hata alıyorduk."
- Kök neden: (1) Yanlış kanal — bu akış **Sistem** katmanı (Global.db `MaliDonemler` + tenant dosya `Migrate`), `SistemLog` doğru; (2) `AppLogService` tenant guard yok — `ITenantContext.IsTenantSet` kontrolü olmadan her `WriteAsync` doğrudan `_logRepository.CreateLogAsync` ile `AppDbContext`'e yazmaya çalışıyor.
- Çözüm (2026-08-25):
  - `MaliDonemCreationViewModel.cs:149` ve `MaliDonemDeletionViewModel.cs:127` `LogAppExceptionAsync` → `LogSistemExceptionAsync` (SistemLog kanalına taşındı).
  - `AppLogService.cs:13` `ITenantContext _tenantContext` eklendi; `AppLogService.cs:53` `WriteAsync` başında `if (_tenantContext == null || !_tenantContext.IsTenantSet) { Debug.WriteLine("[AppLog SKIPPED — tenant yok] ..."); return; }` guard + `try/catch` ile `CreateLogAsync` sarmalandı (tenant silinirken/kapalıyken log kaybı tolere edilir, döngü önlenir).
  - `LogService.cs:14` `ITenantContext` enjekte edilip `AppLogService`'e iletiliyor (`AddServicesHostBuilderExtensions` / `AddCommonServiceHostBuilderExtensions` zaten `ITenantContext` Scoped kayıtlı, `AppLogService` de Scoped olduğu için aynı tenant state paylaşılır).
- Doğrulama: `dotnet build MuhasibPro.slnx --nologo` **0 uyarı 0 hata** (2026-08-25).
- Tarih: 2026-08-25

### SistemKurulumViewModel — EF Core doğrudan ViewModel’de + katman ihlali (ÇÖZÜLDÜ)
- Belirti: `SistemKurulumViewModel.cs:28` `SistemDbContext _dbContext` direkt enjekte, `using Microsoft.EntityFrameworkCore` ile `Kullanicilar.CountAsync/Firmalar.Include/MaliDonemler/KullaniciFirmaRoller` ViewModel içinde — ViewModel Business dışına çıkıyor, EF Core Data dışına taşıyor; `RefreshDbStateAsync` `DbExists=false` iken `Pending/TableCount/Version/Kullanici/Firma` sıfırlanmıyor (bilgiler dolu), `DiagnosticsMetricsGrid/DatabaseTablesList` her durumda görünür (duplicate), `GoToLoginCommand` `Kurulum Gerekli` iken aktif (hatalı gezinme), `DiagnosticsTerminalLog.xaml:9` `#0F172A/#1E293B/#CBD5E1` hardcode `DesignTokens` dışı.
- Çözüm (2026-08-26):
  - `Business/Contracts/AppServices/ISistemDiagnosticsService.cs:1` (`GetKullanici/FirmaCount/CanConnect/GetFirmaMaliDonemStats/GetKullaniciFirmaRolStats`) + `Business/Services/AppServices/SistemDiagnosticsService.cs:1` (EF Core sadece burada) + `AddServicesHostBuilderExtensions.cs:32` `AddScoped<ISistemDiagnosticsService>`; `SistemKurulumViewModel.cs:1` `using EF/DataContext` kaldırıldı → `ISistemDiagnosticsService` ile `RefreshDbState CanConnect/Count`, `TestDbContext/FirmaMaliDonem/KullaniciFirmaRol` servis üzerinden — ViewModel artık EF Core bilmiyor, `grep DbContext ViewModels` 0 doğrulandı.
  - Görsel tutarlılık: `SistemKurulumViewModel.cs:132 RefreshDbState` `else` dalında `Pending=0/TableCount=0/DbVersion=-/Kullanici=-/Firma=-` sıfırlandı; `SistemKurulumView.xaml:149/365` `DiagnosticsMetricsGrid/DatabaseTablesList` `Visibility={x:Bind DbExists TrueToVis}` ile DB yokken gizlendi (duplicate kalktı); `SistemKurulumView.xaml:86/426` `GoToLoginCommand IsEnabled={x:Bind IsKurulumTamamlandi}` ile `Kurulum Gerekli` iken disabled (InfoBg gri).
  - `Styles/DesignTokens.xaml:16` `MuhasibTerminalBg/Border/TextPrimary/Secondary/Tertiary` token eklendi, `DiagnosticsTerminalLog.xaml:9` hardcode → `StaticResource MuhasibTerminalBgBrush/BorderBrush/Text*Brush` — `AGENTS.md STİL TUTARLILIĞI` tek kaynak düzeltildi.
- Doğrulama: `dotnet build MuhasibPro.slnx --nologo` **0 uyarı 0 hata** (2026-08-26).
- Tarih: 2026-08-26

### Business DB servislerinde guard/ölü kod + `ToUpper()` kültür bug'ı (ÇÖZÜLDÜ — Faz 6.20)
- Birim testi yazımı sırasında tespit edilen 5 kusur:
  1. `TenantSQLiteDatabaseSelectedDetailService.cs:39` `if(!maliDonem.Success && maliDonem.Data == null)` — `&&` iken `Success=false + Data!=null` guard'ı atlıyor, hatalı yoldan DB dosya kontrolüne geçiyordu → `||` yapıldı; sonrasındaki ikinci `Success && Data != null` kontrolü ve varışsız hata `return`'ü ölü kod oldu, silindi (AGENTS kural 4).
  2. `TenantHelperExtensions.cs:69` `return` sonrası sarkan boş `;` → silindi.
  3. `TenantSQLiteSelectionService.cs:161` `Message = canConnect ? null : "..."` ölü dal (satır 149 `!canConnect`'te zaten return etmiş) → satır kaldırıldı.
  4. `DatabaseUtilityExtensionsHelper.cs:10` `firmaKodu.ToUpper()` Türkçe kültürde `i→İ` (test FAIL'i ile yakalandı: `firma01` → `FİRMA01`) → DB adı makine kültürüne bağımlı hale geliyordu → `ToUpperInvariant()`.
  5. `GenerateTenantDatabaseName` `prefix "db-" + "_"` → `db-_KODU_2027` çift ayraç → extra `_` kaldırıldı, artık `db-KODU_2027` (kullanıcı onayı ile — sıfır DB varken düzeltildi, migration gerekmedi).
- Doğrulama: `BusinessDatabaseTests.cs` 41 test dahil `dotnet test` **62/62**, `dotnet build MuhasibPro.slnx` **0 hata** (2026-08-28).
- Tarih: 2026-08-28

### DbContext uzantı katmanı — bayat `DatabaseValid` + sahte rollback + sonsuz döngü (ÇÖZÜLDÜ — Faz 6.21)
- Belirti (derin analiz): `DbContextOperationsExtensions`'ta `DatabaseValid` asla yeniden hesaplanmıyordu (input pass-through) → `ExecuteMigrationsWithBackupCheckAsync`'teki "post-migration doğrulama" (`finalAnalysis.DatabaseValid == false`) ölü kodu andırıyordu: DB valid ise restore hiç tetiklenmiyor, invalid ise başarılı migrate sonrası bile koşulsuz rollback oluyordu (sahte rollback).
- Kök nedenler:
  1. `GetConnectionFullStateAsync` kendi tekrarını yapıyor, `PRAGMA integrity_check` `SqlQueryRaw<string>` (Value alias yok), `GetVersionFromMigrations` (format çakışması `yyyy.MM.dd.HHmm` vs `1.1.dd.HHmm`) — merkezi `AnalyzeDatabaseCoreAsync` kullanılmıyordu (ölü).
  2. Backup/restore retry `while(!success && retryCount < 2)` retryCount yalnız catch'te artıyordu → `backupAction()` false dönerse sonsuz döngü.
  3. `_allTablesCache` key'i DbContext tipiydi → tenant switch'te bayat tablo listesi.
  4. `TablesToCheck` `nameof(Kullanici)` (tekil) tablo adı `Kullanicilar` ile eşleşmiyordu → `TableCount` hep 0, `IsEmptyDatabase` hep true → `ShouldTakeBackupBeforeMigration` false → backup hiç alınmıyordu.
- Çözüm (2026-08-29): `DatabaseAnalysisResult` ara sınıfı + tek kaynak `AnalyzeDatabaseCoreAsync<T>`; `ExecuteMigrations` restore/backup/migrate/final yeniden analiz zinciri + `TryWithRetryAsync` (false da deneme artırır); connection-string cache; `tablesToCheck` gerçek çoğul adlar + ayrı `TenantTablesToCheck` sınıfı. `DataMigrationFlowTests` 8 test ile mühürlendi.
- Tarih: 2026-08-29

### Muhasebe entity'leri — PK'sız singleton'lar + AppDbContext şişmesi (ÇÖZÜLDÜ — Faz 6.21)
- Belirti: `AppDbContext`'e 67 muhasebe DbSet'i eklenip `dotnet ef migrations add` denendiğinde `The entity type 'Ayarlar' requires a primary key` — `Ayarlar`/`BelgeNumara`/`VarsayilanDegerler` `BaseEntity` olmadan `[Key] Id` yoktu (tek-kayıt singleton tablolar).
- Çözüm (kullanıcı kararı — hızlı/boşa gitmeyen): tüm muhasebe DbSet'leri `Contracts/Database/Common/ITenantMuhasebeEntities` interface'ine taşındı; `AppDbContext` eski 2 DbSet'e döndü; Faz B açılınca `AppDbContext` interface'i implemente edip migration üretecek. Singleton'lara `[Key] long KullaniciId` eklendi (login kullanıcının kendi ayarı/sayacı izole). `BelgeNumara`'ya eksik `using System.ComponentModel.DataAnnotations` eklendi.
- Tarih: 2026-08-29

### SistemKurulumView LogsText eksik — XamlCompiler sessiz MSB3073 (ÇÖZÜLDÜ — 2026-08-30)
- Belirti: `SistemKurulumView.xaml:46` `Text="{x:Bind ViewModel.LogsText}"` varken `SistemKurulumViewModel.cs:56` sadece `Logs ObservableCollection<string>` vardı — `LogsText` yok → `XamlCompiler.exe` exit 1, `output.json` hatasız, `MSB3073` sessiz düşüyor; `BrandBanner+DatabaseInfoPanel` aynı Grid'de yan yana iken tetikleniyor, tek tek build alıyor.
- Çözüm: `SistemKurulumViewModel.cs:56` `LogsText => string.Join(Environment.NewLine, Logs)` + `CollectionChanged` ve `AddLog` içinde `NotifyPropertyChanged(nameof(LogsText))` eklendi.
- Tarih: 2026-08-30

### WinUI.TableView PRI263 neutral resource (ÇÖZÜLDÜ — 2026-08-30)
- Belirti: `dotnet build` her derlemede `PRI263 0xdef01051 No default or neutral resource given for 'WinUI.TableView/DatePickerPlaceholder'` — paket `FirmalarList.xaml:27`/`FirmaMaliDonemler.xaml:28`/`BaseConfig.cs:53` tarafından aktif kullanılıyor, silinemez (` LOG-41-60:65` silme `CS0246/CS0400` ile geri alınmıştı).
- Çözüm: `MuhasibPro.csproj:26` `DefaultLanguage tr-TR` + `96,99` `NoWarn PRI263` eklendi → build `0 Hata 2 Uyarı (NU1903)` sadece.
- Tarih: 2026-08-30

### SistemKurulum compaction teşhis iskeleti (BİLGİ — 2026-08-30)
- Belirti: Compaction teşhisi için `SistemKurulumView.xaml:24` `Header` + `31` `LightGray Brand1/2/3` placeholder'a çekildi — header sadece `Header` yazıyor, brand gri, `DatabaseInfoPanel 72 Global` fazla görünüyor.
- Çözüm: Eski çalışan vertical `ScrollViewer > BrandBanner + DatabaseInfoPanel` düzene dönüldü; rötüş `Visibility=Collapsed` ile yapılacak (yapı silinmeden).
- Tarih: 2026-08-30

### SistemKurulumView final rötüş — Status sabit + log scroll + buton Visibility + hover (ÇÖZÜLDÜ — 2026-08-30 — Oturum 49)
- Belirti: (1) Alt tarafa sabitlenen log durum görüntüleyici sabit footer'daydı — scroll ile kaymıyordu; (2) Durum çubuğu scroll içinde kayboluyordu; (3) Log içindeki `Giriş Ekranına Git` butonu `IsEnabled` ile disabled görünüyordu, DB kurulmadan da görünür kalıyordu; (4) `İşlem Günlüğü` `TextBox IsReadOnly` `PointerOver` hover ring veriyordu.
- Çözüm: `SistemKurulumView.xaml:29` `Border CardBackgroundFillColorDefaultBrush + BorderThickness 0,0,0,1` ile `Grid.Row 0` sabit header, `ScrollViewer:94` log dahil scroll içine; `xaml:74` `IsEnabled IsKurulumTamamlandi` → `Visibility IsKurulumTamamlandi Converter TrueToVis` (DB kurulmadan `Collapsed`); `xaml:118` `TextBox` → `Border TerminalBg + ScrollViewer + TextBlock IsTextSelectionEnabled` hover'sız terminal. `dotnet build 0 Hata 2 Uyarı (NU1903)` doğrulandı.
- Tarih: 2026-08-30

### MaliDonemlerListControl OnYeniDonemClick eksik — XAML derleme hatası (ÇÖZÜLDÜ — 2026-08-30 — Oturum 51)
- Belirti: `MaliDonemlerListControl.xaml:23` `Click="OnYeniDonemClick"` butonuna eklendi (17:52) ama code-behind (11:08) handler içermiyordu → `XamlCompiler` `MSB3073` sessiz çökme + derleme hatası. Önceki model `YeniDonemDialog`(16:52) + `DeleteGuardDialog`(17:28) WebToXaml çevrimini bitirip bu handler'ı yazarken log alamadan kesilmişti.
- Çözüm: `MaliDonemlerListControl.xaml.cs` yeniden yazıldı — `OnYeniDonemClick` (`vm.SelectedFirma` guard + `YeniDonemDialog` + refresh), `OnDeleteClick` (`DeleteGuardDialog` + refresh), `OnBackupClick` (`ITenantSQLiteDatabaseOperationService.CreateBackupAsync(Safety)` + Toast); buton `IsEnabled="{Binding IsFirmaSelected}"`.
- Tarih: 2026-08-30

### YeniDonemDialog DonemYili ?? — NumberBox.Value nullable değil (ÇÖZÜLDÜ — 2026-08-30 — Oturum 51)
- Belirti: `YeniDonemDialog.xaml.cs:37` `(int)(DonemYiliBox.Value ?? 2027)` → `CS0019 '??' double ile int'e uygulanamaz` — `NumberBox.Value` `double?` değil `double` (nullable değil). WebToXaml çevirisi sırasında eklenmişti, build zaten kırık olduğu için hiç derlenmemişti (gizli).
- Çözüm: `(int)DonemYiliBox.Value` (XAML `Value="2027"` default'u var).
- Tarih: 2026-08-30

### 6.14 "silindi" kayıtları vs gerçek dosya durumu (ÇÖZÜLDÜ — 2026-08-30 — Oturum 51)
- Belirti: Faz 6.14 logu `MaliDonemCard.xaml` ve `MaliDonemOlusturProgress/SilProgress → tek MaliDonemProgressView` "silindi/birleştirildi" diyordu; gerçekte `MaliDonemCard.xaml(.cs)` ve `MaliDonemProgressView.xaml(.cs)` diskte duruyordu ve `MaliDonemDetails.xaml:516-517` hâlâ eski iki overlay'i (`MaliDonemOlusturProgress`/`MaliDonemSilProgress`) kullanıyordu — yani 6.14 birleştirme hiç bağlanmamıştı, `MaliDonemProgressView` yetim kalmıştı.
- Çözüm: `MaliDonemCard` + `MaliDonemProgressView` (0 referans) silindi; `MaliDonemOlusturProgress`/`MaliDonemSilProgress` (canlı, csproj `Page Update` kayıtlı) korundu. Ayrıca `FirmaShell/Controls/` (FirmaKart/FirmaKartListesi/MaliDonemKart — FirmaShellView `FirmalarListControl`+`MaliDonemlerListControl`'e geçtiğinden 0 referans) silindi. `dotnet build 0 Hata 2 Uyarı (NU1903)`, `dotnet test 73/73`.
- Tarih: 2026-08-30

### Splash ThemeShadow receiver ctor'da → pencere görünmez (ÇÖZÜLDÜ — 2026-08-30 — Oturum 52)
- Belirti: Splash'ı v2-tarzı gölgeli karta çevirdikten sonra uygulama "çalışmıyordu" — proses ayakta, `Responding=true`, Event Log temiz, ama pencere ekranda YOK. Kullanıcı: "bin içindeki debug'ta uygulama çalışmıyor."
- Kök neden: `ExtendedSplash` ctor'unda `SplashCardShadow.Receivers.Add(SplashOverlay)` — element görsel ağaca bağlanmadan (Loaded öncesi) `Receivers.Add` exception fırlatıyordu. Bu exception `ActivationService.ActivateAsync`'ın `try/catch`'i (satır 37-59) tarafından yutuluyordu → `StartSplashScreenAsync` → `MainWindow.Activate()` hiç çalışmıyordu → **pencere oluşuyor ama `IsWindowVisible=False` kalıyordu** (Win32 EnumWindows ile tespit: handle var, `gorunur=False`).
- Çözüm: `SplashCardShadow.Receivers.Add(SplashOverlay)` ctor'dan **`OnPageLoaded`'a taşındı + try/catch ile sarıldı** (görsel ağaç bağlandıktan sonra, hata asla uygulamayı kırmaz). `ExtendedSplash.xaml.cs:81`.
- Doğrulama: Win32 EnumWindows → ana pencere `gorunur=True`, başlık `MuhasibPro`. `dotnet build 0 Hata`, test 73/73.
- Tarih: 2026-08-30

### Duplicate MaliDonemListViewModel instance — FirmaShell dönem listesi boş (ÇÖZÜLDÜ — Faz 6 / Oturum 64)
- Belirti: FirmaShell'de firma seçildiğinde sağ paneldeki mali dönem listesi boş kalıyordu. "Yeni Dönem Aç" butonu "Firma seçiniz" uyarısı veriyordu — `SelectedFirma` null görünüyordu.
- Kök neden: `FirmalarViewModel` constructor'ında `new MaliDonemListViewModel(...)` (`:17`), `MaliDonemViewModel` constructor'ında da ayrı `new MaliDonemListViewModel(...)` (`:16`) oluşturuluyordu. `FirmaShellViewModel.MaliDonemList` shortcut'u `MaliDonemVM.MaliDonemList`'e bakıyordu — XAML binding bu instance'a bağlıydı. AMA `FirmalarVM.OnItemSelected()` → `PopulateMaliDonem(selected)` → `FirmalarVM.MaliDonemList.LoadAsync(FirmaId)` **farklı instance'ı** dolduruyordu. XAML'deki instance hiçbir zaman veri almıyordu → boş. Ek olarak `FirmalarVM.Subscribe()` ve `MaliDonemVM.Subscribe()` ikisi de `MaliDonemList.Subscribe()` çağırıyordu → çift mesaj dinleyici.
- Çözüm: `FirmalarViewModel` constructor'ına `MaliDonemListViewModel sharedMaliDonemList = null` parametresi eklendi. `FirmaShellViewModel`'de `MaliDonemVM` önce oluşturulup `MaliDonemVM.MaliDonemList` paylaştırıldı — tek instance. `FirmalarVM.Subscribe/Unsubscribe` içinden `MaliDonemList` çıkarıldı (çift Subscribe önlendi).
- Tarih: 2026-09-02
### ListViewItem custom template — PointerOver, Selected'ı eziyor (ÇÖZÜLDÜ — 2026-09-02 — Oturum 72)
- Belirti: MaliDonem kartı seçilince (mouse hâlâ kart üzerindeyken) kart beyaz görünüyor, mouse çekilince mavi oluyordu — seçim anında mouse üzerinde kaldığı için kullanıcı seçili görünümü göremiyordu.
- Kök neden: `ListViewItem` container template'i yalnızca `Normal/PointerOver/Selected` VisualState tanımlıyordu; seçili öğenin üzerinde işaretçi gezinince `PointerOver` state'i `Selected`'ı eziyordu (hover zemini seçili zeminin yerine geçiyordu).
- Çözüm: `CommonStates`'e `Pressed`/`PointerOverSelected`/`PressedSelected` eklendi (üçü de seçili görünümü taşır: `MuhasibPrimaryLightBrush` + `MuhasibPrimaryBrush` 1.5px). Kural: özel `ListViewItem`/`GridViewItem` template'inde seçim görünümü korunacaksa bileşik state'ler tanımlanmalı.
- Tarih: 2026-09-02

### TenantSQLiteDatabaseLifecycleService — ters şart: `!IsNullOrEmpty` valid DB adını reddediyor (ÇÖZÜLDÜ — 2026-09-02 — Oturum 72)
- Belirti: `GetTenantDatabaseStateAsync("db-X_2027")` "Veritabanı adı boş olamaz" dönüyordu → tenant analiz akışı hiç çalışmıyordu (şu ana kadar çağıran yoktu; dönem kartı "DB Durumu" rozeti bağlanınca yakalandı).
- Kök neden: `if (!string.IsNullOrEmpty(databaseName)) return ErrorApi(... "boş olamaz")` — şart ters yazılmıştı (ad dolu OLMADAN hata dönüyordu).
- Çözüm: `string.IsNullOrEmpty` yapıldı.
- Tarih: 2026-09-02

### Tema renkleri — OOBE Light/Dark düzenlenecek (ÇÖZÜLDÜ — 2026-09-03 — Oturum 74)
- Belirti: Sistem Dark iken `ContentDialog` (silme guard, YeniMaliDonem) ve progress overlay (`MaliDonemSil/OlusturProgress`) koyu `Acrylic/TerminalBg` kalıyor, OOBE açık değil. `AGENTS.md:38` açılış Light kuralı yoktu.
- Kök neden (3 katman): (1) Dialog XAML'leri `{StaticResource Muhasib...Brush}` kullanıyordu — shared fırçalar app/sistem temasına (Dark) çözülüyor, dialog `RequestedTheme=Light` olsa bile içerik Dark slate kalıyordu (StaticResource alt-ağaç temasını izlemez; sayfalar `ThemeResource` stil kullandığı için Light görünüyordu). (2) `DialogHelper.ShowCenteredAsync` `dialog.RequestedTheme = themeService.Theme` ile XAML Light'ı eziyordu. (3) `OnDeleteClick`/`LoginView teşhis`/`ExtendedSplash` bildirimi direkt `ShowAsync()` ile helper'ı bypass ediyordu.
- Çözüm: `AGENTS.md:39` — **XAML içinde tema zorlanamaz (`RequestedTheme` hardcode yasak, dialog dahil)**; Light garantisi kodda tek kaynak (`DialogHelper.ShowCenteredAsync` + `DialogService.CreateDialog` → `ElementTheme.Light`). 8 XAML'den `RequestedTheme` silindi, 6 dialogda `StaticResource Muhasib→ThemeResource`, 3 bypass helper'a alındı.
- Doğrulama: build 0 hata, test 73/73, canlı UIA (sistem Dark) — DeleteGuard beyaz Light açıldı, screenshot `Temp/opencode/ot74_deleteguard.png` (silme onaylanmadı).
- Tarih: 2026-09-03

### `ConfigureAwait(false)` sonrası UI-bound property set — WinUI sessiz kapanma şüphesi (ÖNLENDİ — 2026-09-04 — Oturum 78)
- Belirti (şüphe): `DonemGenelBakisViewModel.DerinAnalizleriYukleAsync` `await ...ConfigureAwait(false)` sonrası `model.DbDerin = ...` (ObservableObject → `NotifyPropertyChanged` → binding) threadpool thread'inde çalışıyordu. WinUI binding UI thread dışından güncellenince `RPC_E_WRONG_THREAD` ile uygulama sessizce kapanabilir (Oturum 76'daki 2 izsiz kapanmayla aynı desen; Event Log'da yeni crash kaydı yoktu).
- Çözüm: `ConfigureAwait(false)` kaldırıldı — `DbDerin` set'i UI thread'de. Kural: ViewModel'de UI'ya bağlı (`ObservableObject.Set` tetikleyen) atama varsa `ConfigureAwait(false)` kullanılmaz.
- Tarih: 2026-09-04

### MaliDonem kart "0 MB" + dosya-yok yetim satır + boş yedek listesi (ÇÖZÜLDÜ — 2026-09-03 — Oturum 75)
- Belirti: (1) Kart boyutu "0 MB"; (2) `.db` elle silinince kayıt hiç silinemiyor ("Veritabanı bulunamadı" / "Silinecek veritabanı bulunamadı"); (3) yedek varken bile Kurtar beliremiyor (liste hep boş).
- Kök neden: (1) `BoyutMetni` sadece MB basıyordu (KB dosyalar `0.0 MB`); (2) silme zincirinde 3 dosya-var doğrulaması (`GetTenantDetailsAsync` → lifecycle → manager); (3) `GetBackupsAsync` deseni tam yol prefix'liydi (`GetFiles(backupDir, "C:\...\db-X.db_*.backup")` hiç eşleşmez — master mirası bug).
- Çözüm: `BoyutMetni` B/KB/MB/GB + analizden karta boyut + NULL satırlara tek seferlik backfill; silme doğrulaması satır-düzeyine, lifecycle/manager dosya-yoksa başarı (saga adımları aynı, kayıt+Yedek temizliği devam eder); desen `Path.GetFileName(...)`.
- Doğrulama: 2025 "28 KB"; 2028 kaydı silindi; 2026 dosyası kenara alınıp Kurtar→restore→Güncel. Build 0 hata, test 73/73.
- Tarih: 2026-09-03


### `ModernCard` Grid'e Style ile verilemez — TargetType=Border (ÖNLENDİ — 2026-09-04 — Oturum 80)
- Belirti: `Styles/Cards.xaml:66` `ModernCard` `TargetType="Border"` — yeni panellerde `<Grid Style="{StaticResource ModernCard}">` yazıldı; WinUI'da stil hedef tipten farklı elemente uygulanamaz (derleme/çalışma hatası verirdi).
- Çözüm: kart `Border Style="{StaticResource ModernCard}"` sarmala + içine içerik `Grid`i koy (Login/FirmaShell deseni). Kontrol: `grep 'Grid Style="{StaticResource ModernCard}"'` 0 sonuç + 7 XAML `xml parse` OK.
- Tarih: 2026-09-04

### Code-behind `resources["Transparent"]` — böyle bir kaynak anahtarı yok (ÇÖZÜLDÜ — 2026-09-04 — Oturum 80)
- Belirti: `TumuHeroPanel.SekmeGorunumu` pasif segmentte `resources["Transparent"]` arıyor → `COMException 0x80004005 / Cannot find a resource with the given key: Transparent` (runtime crash, derleme temizdi). Aynı kalıp sayfa sekme kodunda da vardı (`MaliDonemYonetimView.SekmeGorunumu` — Arşiv sekmesi pasifken patlardı).
- Kök neden: `Transparent` bir renk, kaynak sözlüğünde `Brush` anahtarı değil. Aktif daldaki `MuhasibPrimaryLightBrush` token'dı ama pasif dal token dışıydı (AGENTS kural 3 ihlali).
- Çözüm: aktif → `MuhasibPrimaryLightBrush` token; pasif → `button.ClearValue(Button.BackgroundProperty)` (varsayılan şeffaf zemin, kodda renk yok). Her iki dosyada da düzeltildi.
- Tarih: 2026-09-04

### ComboBox TwoWay + ItemsSource.Clear() → seçim null yazılır (ÇÖZÜLDÜ — 2026-09-04 — Oturum 80)
- Belirti: Dönem kartında "İndeksleri Yeniden Derle" (REINDEX) sonrası banner "—" oluyor, butonlar pasif — `SelectedDonem` null.
- Kök neden: bakım sonrası `RefreshAllAsync → LoadDataAsync` içinde `ItemsSource.Clear()` anında banner ComboBox seçimi düşürüp `TwoWay` ile VM'e null yazıyor (geçici desync; VM daha sonra doğru örneği set etse de binding yarışı null'ı kalıcı kılıyor). Ayrıca paylaşılan `MaliDonemList.SelectedItem` yenilemede ilk açık döneme dönüyor, sayfa seçiminden kopuyordu (tablo rozeti desync).
- Çözüm: `SelectedDonem` setter'da liste doluyken null'ı yoksay (liste boşken kabul et — silme sonrası); `RefreshAllAsync`/`LoadAsync` sonunda `EsitleListeSecimi()` ile liste seçimini sayfaya eşitle. Zincir: `Contracts → MaliDonemYonetimViewModel` (VM-içi, yeni servis yok).
- Tarih: 2026-09-04

### Element Visibility + code-behind DataContext değişimi = ölü binding (ÇÖZÜLDÜ — 2026-09-04 — Oturum 80)
- Belirti: Dönem/Analiz segmentinde Tümü tablosu (KPI + bant + liste) altta görünüyordu — dönem listesi her sayfada.
- Kök neden: `Visibility="{Binding IsTumuSegmenti}"` panel elementinin ÜZERİNDEYDİ ama code-behind `GenelBakisPanel.DataContext = GenelBakisVM` yapıyordu (IsTumuSegmenti ana VM'de). Yol çözülemeyince converter HİÇ çalışmaz, Visibility varsayılanda (Visible) kalır — Oturum 78'deki "bool-dışı false" notu yalnızca converter'a ULAŞAN değerler içindir.
- Çözüm: Visibility, sayfa DataContext'ini gören SARMALAYICI elemente taşınır; DataContext'i değişen elementin kendine görünürlük binding'i konmaz. Diğer paneller tarandı (hero/analiz ana VM; Yedekler/Arşiv/Bilinmeyen parent'ta) — tuzak yalnızca buradaydı.
- Tarih: 2026-09-04

## Oturum 170 — Uydurma `StaticResource` anahtarı (`IconShutdown`)
- Yeniden yazılan XAML'e sözlükte olmayan ikon anahtarı kondu → canlıda `XamlParseException` (`InitializeComponent`).
- Ders: XML parse'ı anahtar çözümlemez; eklenen HER `StaticResource` `Icons.xaml`/`DesignTokens.xaml`'a karşı `grep` ile doğrulanır. Tekrarı yasak.

## Oturum 188 — `x:Bind` iç-içe yol null-patlar (DERS)
- Belirti: `{x:Bind ViewModel.SeciliFirma.KisaUnvani}` — liste boşken `SeciliFirma` null → NRE (Visibility gizlese bile binding değerlendirilir).
- Ders: `x:Bind` null-yayılımı yapmaz; ara-nesne null olabiliyorsa VM'de null-güvenli DÜZ prop (`SeciliFirmaUnvani ?? string.Empty`) açılır, XAML düz yola bağlanır. Tekrarı yasak.
- Tarih: 2026-09-10

## Oturum 248 — Dialog zemini yarı saydam → arkası sızıyor (DERS)
- Belirti: `YardimDialog` arkasındaki Login içeriği görünüyordu ("arka plan transparent gibi").
- Kök neden: `ContentDialog.Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"` — Fluent kart fırçası yarı saydam (~%70); dialog blur hattı geri alındığı için arkası örtülmüyordu.
- Ders: Dialog zemini **opak** `SolidBackgroundFillColorBaseBrush` olur; `CardBackgroundFillColorDefaultBrush` dialog zemini olarak kullanılmaz (Kural 17).
- Tarih: 2026-09-13

## Oturum 248 — MVVM dışı code-behind command mantığı (DERS)
- Belirti: `LoginView.xaml.cs`'te `OnYardimClick`/`OnTeshisClicked` (kullanıcı: "code-behind metodla dolu").
- Ders: View'a bağlı command mantığı (yardım/teşhis/navigasyon) ViewModel command'ına taşınır; View chrome gerektiren dialog App katmanındaki `IDialogService` üzerinden açılır (`ShowYardimAsync`). Code-behind yalnız lifecycle + gölge receiver taşır.
- Tarih: 2026-09-13

## Oturum 267 - Container sablonunda \{Binding Selected}\ cozulmuyor (DERS)
- Belirti: Donem listesi secerken sol hub (accent bar) canlida hic gorunmuyordu; kuyruk animasyonu da ayni baglama ile bos kaliyordu. Akordiyon (DataTemplate) ayni \Selected\ baglamasiyla calisiyordu.
- Kok neden: \ListViewItem\ **container sablonunun** DataContext'i item modeli DEGIL — \{Binding Selected, FallbackValue=Collapsed}\ cozulmeyince sessizce Collapsed'a dusuyordu (6.84'ten beri; kullaniciya "orjinal stilde solda kutu cikiyor" hatirasi stok PİLOT gostergesinden geliyordu).
- Ders: Container (\ItemContainerStyle\) sablonunda item modeli ozelligi \{Binding Content.X, RelativeSource={RelativeSource Mode=TemplatedParent}}\ ile baglanir; dogrudan \{Binding}\ + FallbackValue sessiz kaybolma demektir. Tekrari yasak.
- Tarih: 2026-09-15

## Oturum 267 - WinUI dash deseni: toplam > yol uzunlugu = cizim yok (DERS)
- Belirti: \StrokeDashArray=\"100 3300\"\ tek-kuyruk deseni hic cizmiyordu (offset animasyonu dogru ilerlerken bile); \"1 10\" ciziyordu.
- Kok neden: WinUI shape dash cizicisi desen toplami yol uzunlugunu gecince deseni dusuruyor (buyuk bosluklu tek-dash kalibi calismiyor).
- Ders: Tek kuyruk gerekiyorsa desen boyut-bazli kurulur: \gap = perimeter − 101\ (toplam yolun 1px altinda; 1px kirinti gorunmez). Kod: \OrbitRingControl.StartOrbit\. Tekrari yasak.
- Tarih: 2026-09-15

## Oturum 268 - Dependent animasyon (StrokeDashOffset) = UI thread takilmasi; "yapay" dekoratif animasyon (DERS)
- Belirti: Donem kartindaki donen kuyruk stabil degildi (yuk altinda takiliyor); ayrica dinlenme offset'i bir kuyruk boyu fazla oldugu icin kuyruk hub'i gecip ust kenarda duruyordu ("sol hub icinde durmasi gereken cizgi hub'i geciyor").
- Kok neden: (1) \Shape.StrokeDashOffset\ bagimsiz animasyon DEGIL (\EnableDependentAnimation=True\ → UI thread); (2) rest hesabinda kuyruk basi \hubDistance\ yerine \hubDistance + kuyrukUzunlugu\ hedeflenmisti.
- Ders (kullanici karari): Secim gibi kalici durumlar icin **dekoratif animasyon yapilmaz** — "animasyon yapmak icin yapilmis gibi" olur. Cozum durum farkidir (secili kart dis panel karti dolgusuna burunur + sol hub). Animasyon gerekirse bagimsiz ozellik (Opacity/Scale/Translation) kullanilir, sonsuz/surekli animasyondan kacinilir (MS Motion). Tekrari yasak.
- Tarih: 2026-09-15

## Oturum 268 - Hover perdesi Light temada gorunmuyor (DERS)
- Belirti: Liste satirinda fare ile uzerine gelince Light temada hicbir gorsel degisiklik yoktu (248→248); Dark'ta calisiyordu.
- Kok neden: `HoverOverlay` `ControlFillColorSecondaryBrush` kullaniyordu (her iki temada da **acik/beyaz** renkli) + `Opacity=0.30` → Light'ta efektif alfa ≈ %1.8 = gorunmez.
- Ders: Hover gibi tema-farkinda olmasi gereken perdeler `SubtleFillColor*` veya **DesignTokens'ta Light siyah / Dark beyaz tanimli token** ile yapilir; `ControlFillColorSecondaryBrush` tek basina Light'ta koyulastirmaz. Uygulama: `MuhasibHoverOverlayBrush` (Light `#14000000` / Dark `#1AFFFFFF`). Kod: `Cards.xaml` HoverOverlay. Tekrari yasak.
- Tarih: 2026-09-15
