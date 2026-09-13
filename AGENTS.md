# AGENTS

Bu projede çalışmaya başlamadan ÖNCE şu dosyaları oku:

- `docs/ROADMAP.md` — fazlar, mimari kararlar, nerede kaldığımız
- `docs/KONTROL-LISTESI.md` — işaretli/bekleyen maddeler
- `docs/HATALAR.md` — tekrar edilmemesi gereken hatalar
- `docs/AKIS-PLANI.md` — uygulama akışı ve veritabanı yönetimi
- `docs/WINUI-MIMARISI.md` — pencere yönetimi
- `docs/VIEW-PACKAGE-DURUMU.md` — görsel stil paketi entegrasyon durumu (sadece View/XAML işi varsa)
- `docs/OOBE-TASARIM-SABLONU.md` — Login/Splash/SistemKurulum görsel dili, TEK kaynak (sadece View/XAML işi varsa; WebToXaml'a BAKILMAZ)
- `docs/TASARIM-KURALLARI.md` — katmanlı sayfa yapısı (zemin → ana border → kartlar), Kural 17'nin ayrıntısı (her View/XAML işinde okunur)
- `docs/CEKIRDEK-MODUL-PLAN.md` — çekirdek faz planı ve Kesin Kurallar (Kural 7: modelden gelen gerçek veri, örn. yedek saklama `GetManuelKeep()` → `CleanOldBackupsAsync` request model ayardan okur)
- `docs/AYARLAR-PANEL-PLAN.md` — per-view ayar panelleri haritası (varsa; her view kendi ayarını yapar)
- `docs/REFERANSLAR.md` — referans defteri: alınan kaynak + hangi view/özelliğe uygulandığı (Kural 19; yeni view/özellikten önce oku, aynı işi tekrar yapma)

## Her oturumun başında
1. `docs/LOG.md` **indeksini** oku (kitap kapağı — sadece tablo, ~4 KB). Son 2-3 oturumun hangi `docs/LOG/LOG-XX-YY.md` cildinde olduğunu bul, sonra ilgili cildi `Read` ile aç (sadece ilgili oturum başlıkları). `LOG.md`'nin tamamını veya tüm ciltleri okuma — token tasarrufu.
2. **Planı oku:** `docs/CEKIRDEK-MODUL-PLAN.md` (Kesin Kurallar + faz tablosu) ve varsa `docs/AYARLAR-PANEL-PLAN.md` (per-view ayar haritası). Eksik okuma = plan hatası hükmündedir. Token tasarrufu, plan eksik okumayı mazur göstermez — ilgili fazın **Kural 7 örneği** (`yedek saklama` → `GetManuelKeep()` → `CleanOldBackupsAsync` request model ayardan okur) gibi kritik satırları `Read` ile doğrula, ezberden yazma.
3. Yeni oturum için son cilde bak: dolmadıysa (20'den az) oraya ekle, dolduysa yeni `LOG-XX-YY.md` cilt oluştur ve `LOG.md` indekse satır ekle.
4. `docs/KONTROL-LISTESI.md`'deki açık maddelere göre devam et
5. İlgili katmanın mevcut kodunu oku (`Libraries/MuhasibPro.Domain|Data|Business|ViewModels`, `MuhasibPro` [UI])

## Her oturumun sonunda
1. `docs/LOG/LOG-XX-YY.md` son cilde yapılanlar/kararlar/engeller/sonraki adım yaz ve `docs/LOG.md` indekse satır ekle
2. `docs/KONTROL-LISTESI.md`'yi güncelle
3. `docs/ROADMAP.md` faz tablosunu güncelle
4. Çözülen bug varsa `docs/HATALAR.md`'ye ekle
5. View/stil paketi ile ilgili çalışıldıysa `docs/VIEW-PACKAGE-DURUMU.md`'yi güncelle
6. Plan değiştiyse: **işlem öncesi plan → plan durumu → değişiklik sebebi → uygulanan plan sonucu** dörtlüsünü `LOG` cildine ve ilgili plan dosyasına (`CEKIRDEK-MODUL-PLAN.md` / `AYARLAR-PANEL-PLAN.md`) işle — “nerede yanlış yaptım” şüphesi log eksik okumadan kaynaklanmasın
7. Son cilt 20'yi doldurunca yeni `LOG-XX-YY.md` cilt oluştur (eski ciltler arşivdir; `LOG-ARSIV.md`'ye taşıma gerek yok, kalıcı kararlar `ROADMAP.md`'de)
8. **Canlı test + onay (Kural 18):** biten view/özelliğin canlı testini agent yapar, kanıtı (ekran görüntüsü + UIA + referans açıklaması) `LOG`'a işler ve kullanıcı onayına sunar; kanıt/onay olmadan iş kapanmaz
9. **Referans defteri (Kural 19):** internetten/sektörden referans alındıysa `docs/REFERANSLAR.md`'ye satır ekle (kaynak + uygulandığı view/özellik) ve uygulama bitince `Durum`u güncelle

## Marker sözleşmesi
| Marker | Anlam |
|---|---|
| `⬜` | Bekliyor |
| `🔨` | Aktif |
| `✅` | Kod eklendi |
| `🧪` | Derleme/test doğrulandı |
| `⚠️` | Risk |
| `❌` | Hata |

## Genel kurallar
- **Görsel dil TEK kaynak: Windows 11 Fluent.** Tüm ekranlar Fluent tasarım diline geçirilir (Faz 6.75); pencereler Mica-uyumlu olur (Mica backdrop + Fluent malzeme/kontrast). Yeni işler sistem stilleri (`TableView`, `NavigationView`, standart dialog/button stilleri) ile yazılır; özel token/stil yalnız Fluent'te karşılığı yoksa `DesignTokens`/`Styles` altına eklenir. `docs/OOBE-TASARIM-SABLONU.md` ve InventEase kararları yürürlükten kalktı. `muhasibpro-WebToXaml` referans DEĞİL.
- **Tema ayarlanabilir (varsayılan: sistemi takip et):** `ThemeSelectorService` kayıt yoksa `Default` ile başlatılır (OS Light/Dark izlenir); kullanıcı Denetim Masası → Görünüm'den `Default/Light/Dark` seçer (`AppPlatformSettings.ThemeDefault` modelden). **XAML içinde tema zorlanamaz (`RequestedTheme` hardcode yasak — dialog dahil);** dialog'lar uygulama temasını miras alır. **Tema-bağımlı fırça kullanım yerinde `ThemeResource` olur (`StaticResource` load-time kilitlenir, beyaz-beyaz metin yapar — Oturum 230);** `ThemeDictionaries` içinde `StaticResource` kullanılır (MS kuralı).
- Her faz sonu `dotnet build` / `MSBuild` ile 0 uyarı / 0 hata.
- Modüler geliştirme: önce App ayağa kalksın (Aşama A), sonra muhasebe modülleri (Aşama B) tek tek.
- Token tasarrufu: her oturumda log tut; bir sonraki oturum için `LOG.md` tek başına yeterli context sağlamalı.

## Kod kuralları

### 1. Tek sorumluluk (god-class yasak)
Bir sınıf (ViewModel, Service, Manager, code-behind) birden fazla iş yapamaz: seçim + kaydetme + navigasyon + UI-state aynı sınıfta OLMAZ.

**Tetikleyici:** Bir sınıf 150+ satıra ulaştıysa veya "bu sınıf ne yapıyor?" sorusuna tek cümleyle cevap veremiyorsan, hemen böl.

**Örnek:**
```
❌ SistemKurulumViewModel (450 satır: durum + oluşturma + testler + orkestrasyon)

✅ SistemDatabaseStatusViewModel   (sadece durum/refresh)
✅ SistemDatabaseCreationViewModel (sadece oluşturma/güncelleme saga)
✅ SistemDiagnosticsViewModel      (sadece testler)
✅ SistemKurulumViewModel          (orkestratör: composition + navigation + Logs, başka iş yapmaz)
```
Kalıtımla VM birleştirme yasak (`VM : BaşkaVM` yazma). Onun yerine composition kullan (bir VM, diğer VM'leri property olarak içinde barındırır).

### 2. XAML modülerliği
View XAML'i tek monolit blok olarak yazılmaz.

**Tetikleyici:** Aynı görsel blok 2+ yerde tekrar ediyorsa VEYA bir view 300+ satıra ulaştıysa VEYA bir blok kendi başına bağımsız veri/davranış yönetiyorsa → `UserControl`'e böl.

**Örnek:** `QuickLoginPanel`, `NamePasswordControl`, `FirmalarList`, `StatusCard`, `DatabaseOperationsCard`.

### 3. DesignTokens tek kaynak
Renk, köşe yarıçapı, padding, typography → hepsi `Styles/DesignTokens.xaml` üzerinden (`MuhasibPrimaryBrush`, `MuhasibCardStyle`, `MuhasibPillCornerRadius` vb.).

- XAML içine hardcode renk yazma (`#FF...` yasak, ThemeResource hariç).
- İstediğin token yoksa: önce DesignTokens'a ekle, sonra oradan kullan.
- Hover/pressed renkleri de token'dan gelir (`MuhasibPrimaryHoverColor`, `PressedColor`).

### 4. Ölü kod yok
Kullanılmayan metod/interface/command'ı SİL. Yorum satırına alma, `NotImplementedException` ile gizleme.
Kontrol: `grep` ile 0 caller çıkan imza = ölü koddur, silinir.

### 5. ViewModel katman kuralı
ViewModel'e kolayca property/command eklemek yasak. Her yeni property/command şu zinciri izler:

```
ViewModel → Business.Contracts (interface) → Business.Services (implementasyon) → Data
```

ViewModel sadece `ICommonServices` + `Business.Contracts` bilir. `EF Core` / `DbContext` asla ViewModel veya View içine sızmaz.

### 6. SOLID (somut karşılıkları)
- **S**ingle Responsibility → kural 1 ile aynı (150 satır tetikleyicisi).
- **O**pen-Closed → yeni davranış eklerken mevcut sınıfı değiştirme, yeni implementasyon ekle.
- **L**iskov → türetilmiş sınıf, temel sınıfın yerine sorunsuz geçebilmeli; geçemiyorsa kalıtım yanlış.
- **I**nterface Segregation → bir interface'e kullanılmayan metod ekleme, küçük ve amaca özel interface'ler yaz.
- **D**ependency Inversion → somut sınıfa değil `Contracts` interface'ine bağımlı ol (constructor injection).

### 7. Klasörleme
Yeni sınıfı `WINUI-MIMARISI.md` / `AKIS-PLANI.md`'deki yapıya göre doğru klasöre koy:
`ViewModels/Shell`, `Views/Login/Components`, `Database/SistemDatabase`, `Services/SistemServices/AppServices` vb.

### 8. Onay (View ve Kritik Yapı)
View işlemleri (`Views/**/*.xaml` + `.xaml.cs`, `Styles/*`, `Controls/*`) ve kritik yapı değişiklikleri (`Mimari bekçi`yi tetikleyen, `Platform`/`RuntimeIdentifier`, `Package.appxmanifest:13` `Version`, `Database` migration, `DI` yaşam döngüsü) **her sınıfta/dosyada onay alınmadan yapılmaz**. Sınıf bazlı onay alınır, toplu import/push yok (`git diff --stat` her View sınıfı için ayrı gösterilir). Onaysız View/kritik değişiklik `AGENTS.md:8` ihlali sayılır — `dotnet build` 0/0 olsa bile faz kapanmaz.

### 11. Yükleme bildirimi (busy kuralı)
(9-10 `docs/CEKIRDEK-MODUL-PLAN.md` Kesin Kurallar'dadır; bu numara devamıdır.)
Veritabanından/diskten veri çeken **her** dinamik yüzey (sayfa, panel, kart, liste) kendi `Is*Yukleniyor` bayrağını taşır ve `ProgressRing` (`IsActive={Binding}` + `Visibility=TrueToVis`) ile gösterir.
- **Sürekli dönme yasak:** `IsActive="True"` sabitiyle ring konulamaz; her yükleme `try/finally` ile bayrağı kapatır.
- **Sonuç zorunlu:** Her görev kullanıcıya sonuç verir — başarı (`Kaydedildi • HH:mm:ss` / toast), hata (`StatusError` + satır-içi mesaj) veya **zaman aşımı** (asılı kalma yok; analiz tipi işlerde 20sn presedanı `MaliDonemListViewModel:245`, liste/yedek işlerinde makul tavanla).
- **Boş-durum ayrımı:** `IsYukleniyor=true` → ring; `false` + 0 kayıt → "kayıt yok" metni (ikisi aynı anda görünmez).
- Yeni panel/liste ekleyen faz, bayrak + ring + sonuç bildirimi olmadan kapanmaz.

### 12. İşlem-görseli disiplini (tasarım kuralı)

**Tasarım kısmında modern ve güncel akımla uyumlu, kullanıcı dostu, işlem durumu ve sonucu sadece bir bildirimle değil; uzun işlemlerde bir akış planı, kısa işlemlerde bir bekleme animasyonu gösterme gibi tasarımlar kesin bir agent kuralıdır.**

- **Uzun işlem** (yedek, göç, bakım, analiz, indirme — N ile çarpanlı): adım rozetli **akış planı** (`Yedek→Göç→Doğrulama` deseni) + determinate `ProgressBar` (`i/N`, `%`, mevcut iş adı) + bitince **tek** özet bildirimi. Ara adım toast'u yasak; bar geri sarmaz (tek bar ileri gider).
- **Kısa işlem** (tek satır yazımı, tek dosya silme, liste okuma): `ProgressRing` bekleme animasyonu (Kural 11 bayrağı) + sonuç bildirimi (başarı/hata/zaman aşımı).
- **İçerik yüklenirken**: iskelet veya ring; boş-durum metni ring ile aynı anda görünmez (Kural 11).
- **Hata durumu** görselle taşınır (`ShowError`/danger stili); sessiz `catch` + çıplak `return` ile işlem yutulamaz.
- **Bilinmeyen işlemde tasarım yasağı:** yapısı bilinmeyen veya yaptığı işlem tam anlaşılmayan operasyonun tasarımı ezberden çıkarılmaz — **önce internette aratılır, bulunamazsa kullanıcıya sorulur**, ona göre tasarım çıkarılır. Araştırma kaynağı + karar LOG'a işlenir.

### 13. Araştır-önce + yardım sayfası
- **Kritik yapıya başlanmadan veya refactoring yapılmadan ÖNCE** ilgili işlem internette araştırılır (resmi docs/kaynak öncelikli); bulgu + karar LOG'a işlenir, **ondan sonra** işleme başlanır. Bulunamazsa kullanıcıya sorulur (Kural 12).
- **İş bitiminde**, ilgili View'e UI kullanımı için **yardım sayfası** eklenir (nerede ne yapılır, hangi sonuç beklenir, hata ne demek). Yardım içeriği saf Fluent ile, Kural 11/12 görsel disipliniyle yazılır. Yardım sayfası olmadan faz kapanmaz.
- **Desen (Oturum 236 presedanı):** yardım = sayfa başlığındaki **? butonu → yardım dialogu** (ortak `Views/Components/YardimDialog`, sayfa içeriği maddeler halinde). **Kapsam:** yalnız fonksiyon içeren view'ler (buton/işlem/liste olan); Splash/KurulumSplash gibi fonksiyonsuz açılış ekranları muaftır. Her fonksiyonlu view'in ? butonu yoksa o faz kapanmaz.
- **Yardım sayfayla birlikte yaşar:** sayfanın içeriği/fonksiyonu değişen her iş, aynı işin içinde yardım maddelerini de günceller (sonraya/ayrı faza bırakılmaz; güncellenmemiş yardım = eski bilgi = bug hükmündedir).

### 14. View-öncesi içerik + tasarım araştırması
- **Bir view'e dokunulmadan ÖNCE**, o view iki yönüyle internette araştırılır (Oturum 159 presedanı): **içerik** (bu işlemde ne olmalı/olmamalı — MS kılavuzu + sektör) + **tasarım** (nasıl görünmeli — MS Fluent rehberi + Toolkit galerisi öncelikli); kaynak + karar LOG'a işlenir, **ondan sonra** uygulamaya geçilir. Bulunamazsa kullanıcıya sorulur (Kural 12).
- **Araştırma bu uygulamanın sektörüne göre yapılır:** MuhasibPro bir **ön muhasebe yazılımıdır** (masaüstü, multi-tenant, firma/mali dönem bazlı, SQLite yerel veritabanı). Her view'in araştırması "genel WinUI ekranı" değil, **muhasebe/ERP yazılımlarında o işlemin nasıl yapıldığı** bağlamında olur (QuickBooks, Sage, MYOB, Tally, Logo, Luca vb. referans).
- **Araştırma sonucu mevcut sayfa ile uyuşmuyorsa davranış değişikliği yapılır:** araştırmadan elde edilen sektör bulgusu mevcut view'in fonksiyonelliğiyle çelişiyorsa (eksik işlev, gereksiz teknik detay, yanlış akış, kullanıcı beklentisiyle uyumsuz yapı), view sadece görsel değil **fonksiyon ve işleyiş olarak da** araştırma bulgusuna göre yeniden tasarlanır. "Davranış değişikliği yok" kısıtı bu durumda geçerli değildir — sektör gerçeği önceliktir.
- **Sıralama kesindir — kod araştırmadan ÖNCE yazılamaz:** yeni view/sayfa oluşturma veya mevcut view'e dokunma işleminde araştırma adımı atlanırsa, yazılan kod geriye dönük araştırma ile doğrulansa bile **süreç ihlali** sayılır. Araştırma → LOG kaydı → uygulama sırası zorunludur (Oturum 229 presedanı: KurulumSplash araştırmasız yazıldı, geriye dönük düzeltildi — tekrarı kabul edilmez).
- Mica-uyumluluk her geçirilen view'da doğrulanır (backdrop + malzeme + kontrast + Light/Dark).
- **Referans defteri (Kural 19):** alınan her kaynak (site/doküman + bulgu) **uygulamadan önce** `docs/REFERANSLAR.md`'ye, uygulandığı view/özellikle birlikte yazılır; kayıtsız referansla tasarım uygulanmaz ve aynı yer için tekrar araştırma yapılmaz.

### 15. Bilmeden yapma — dürüstçe sor
- **Bilmediğin yapıyı uydurma.** İnternette araştırılmamış, nasıl çalıştığı öğrenilmemiş bir yapının tasarımı yapılmaz, altyapısı kurulmaz, projeye geçirilmez. Uydurulan yapı büyük geri dönüşe, ağır refactoring'e, zaman ve token israfına yol açar.
- **Emin değilsen dur ve söyle.** Araştırma sonuç vermediyse veya konu anlaşılmadıysa kod yazılmaz; kullanıcıya açıkça söylenir ("bunu araştırmadım / bilmiyorum") ve sorulur. Tahminle devam etmek yasaktır.
- **Geriye dönük doğrulama kurtarmaz.** Önce kodlayıp sonra araştırmak süreç ihlalidir (Oturum 229 presedanı). Sıra değişmez: araştırma → LOG → uygulama → doğrulama.

### 16. Tasarım derinliği + bulunabilirlik
- **Araştırma yüzeysel yapılmaz.** Her view'de şunlar internette araştırılır ve LOG'a işlenir: buton yerleşimi (hangi buton nerede), sayfa düzeni (bölüm sırası/hiyerarşi), kullanıcı kısayolları (en sık işlem ekstra tıklamasız). Tasarım Stations: MS CommandBar/Buttons/Settings kılavuzları + sektör (muhasebede QuickBooks/Sage backup deseni) öncelikli.
- **Birincil aksiyon kart başlığında sağdadır** (SettingsCard deseni: ikon + başlık + açıklama solda, aksiyon sağda). Sayfada tek buton varsa içerikle hizalı olur; kap içindeyse sağa yaslanır. Etiket tek kelimelik fiildir, min 120px.
- **Kritik bölüm gömülü kalmaz.** Her yönetim işlemi (yedekle/geri-yükle/güncelle/sil) kendi başlıklı kartında durur; sayaç hapı + son-durum satırı taşır. Kullanıcının "iğne araması" tasarım bug'ıdır.
- **En sık 1-2 işlem her zaman görünür.** Yıkıcı işlem varsayılan olamaz, onaysız çalışamaz.
- Bu maddelerden biri eksikse o view'in tasarımı bitmemiş sayılır — build 0/0 olsa bile faz kapanmaz.

### 17. Katmanlı sayfa yapısı (zemin → ana border → kartlar) — KESİN
**Her yeni view/sayfa ve güncellenen her view bu yapıya uyar. Uymayan sayfa önce bu yapıya taşınır, sonra içerik eklenir.** (Kullanıcı kuralı, Oturum 241; ana border + tint kararı Oturum 248.)

- **Katman 1 — zemin (kök `Border`, tüm sayfayı kaplar):**
  - Sayfanın en dış elemanı **`Border`** olur; kök `Grid` + `Background` kombinasyonu **yasak**.
  - İçinde `Image` (`Stretch="UniformToFill"`, kaynağı temaya göre `light`/`dark`) + üzerine yarı-saydam tint katmanı (`SolidBackgroundFillColorBaseBrush`, **~%35 opak** — zemin görseli görünür kalır). **Ham/net resim gösterilmez — her zaman tint üstte.**
  - Statiktir; sayfa içeriği değişince yeniden oluşturulmaz. (Varlık: `Assets/Images/light.jpg`/`dark.jpg`.)
- **Katman 2 — ana border (ana panel):**
  - Zeminin üzerinde duran, tüm içeriği kapsayan tek çerçeveli panel: `Background="{ThemeResource CardBackgroundFillColorSecondaryBrush}"`, `BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"`, `BorderThickness="1"`, `CornerRadius="{StaticResource OverlayCornerRadius}"`, `Padding="24"`.
  - Elevation: `ThemeShadow` (receiver `Loaded`/`OnPageLoaded` içinde try/catch ile eklenir — **asla ctor'da**).
  - Kenarlardan boşluklu; zemin görseli çevresinde görünür kalır.
- **Katman 3 — içerik kartları (her mantıksal blok ayrı `Border`, ana border içinde):**
  - `Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"`
  - `CornerRadius="{StaticResource ControlCornerRadius}"` (veya `OverlayCornerRadius`)
  - `BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"` + `BorderThickness="1"`
  - Hafif elevation: `ThemeShadow` + `Translation` (Z ekseni); receiver `Loaded` + try/catch (Splash emsali).
  - `Padding` standardı: standart kart `16`; küçük kart `12`; büyük panel `20`.
  - Kart arası boşluk **sabit 12px** (`StackPanel Spacing` / `Grid ColumnSpacing-RowSpacing`).
  - **İç içe kart > 2 seviye yasak.**
- **Dialog zeminleri opak:** dialog `Background`'ı `SolidBackgroundFillColorBaseBrush` (opak) olur; yarı saydam `CardBackgroundFillColorDefaultBrush` dialog zemini olarak **kullanılmaz** (arkası sızar).
- **Renk:** yalnız `{ThemeResource ...}`/`{StaticResource ...}` — hardcode `#RRGGBB` **yasak** (DesignTokens/ThemeDictionaries tanımları hariç).
- **Yasaklar:** kök `Grid` + `Background`; ana border'sız doğrudan zemine kart; **iç içe kart > 2 seviye**; view'dan view'a tutarsızlık.
- **Kapı:** yapıya uymayan view "tasarımı bitmemiş" sayılır — build 0/0 olsa bile faz kapanmaz. Detay: `docs/TASARIM-KURALLARI.md`.

### 18. Canlı testi agent yapar; onay kullanıcıya sunulur — KESİN
**Kullanıcı, agent'ın internetten araştırıp kurduğu tasarımı/akışı görmediği için kör test edemez. Bu yüzden canlı testi agent kendisi yapar; kullanıcı yalnızca onaylar.** (Kullanıcı kuralı, Oturum 245.)

- **Canlı testi agent yürütür:** view/özellik bitince agent uygulamayı açar, akışı (açılış → ekran → buton/etkileşim → sonuç) kendisi sürer; UIA + ekran görüntüsü alır.
- **Kanıt zorunlu:** akış adımları + `Temp/opencode/otXXX-*.png` ekran görüntüsü + (varsa) UIA çıktısı. Kanıt sunulmadan iş "bitti" sayılmaz.
- **"Canlı test kullanıcıda" diyerek kapatma yasak.** Kullanıcı YALNIZCA onay/ret verir; kör deneme yaptırılmaz.
- **Referansı açıkla:** internetten alınan desen/akış kısaca sözlü açıklanır ve canlı sonuç aynı sunumda gösterilir — kullanıcı neyi onayladığını bilir.
- **Engeli dürüstçe yaz:** ortam uygulamayı açamıyorsa agent "canlı test yapılamadı + neden" der; sessizce kullanıcıya yıkmaz. "Kullanıcı test eder" varsayımı yasaktır.
- **Kapı:** build 0/0 + test yeşil olsa bile **canlı test kanıtı + kullanıcı onayı** olmadan faz kapanmaz.

### 19. Referans defteri (araştırma → nereye uygulandı) — KESİN
**Her oturum/agent internetten aldığı tasarım/akış referansını ve onu hangi view/özelliğe uyguladığını `docs/REFERANSLAR.md`'ye yazar. Kayıtsız referansla tasarım uygulanmaz.** (Kullanıcı kuralı, Oturum 246.)

- **Tek defter:** `docs/REFERANSLAR.md` — satır: tarih/oturum · kaynak (site/doküman + kısa bulgu) · **uygulandığı view/özellik** · durum (`✅` uygulandı, `❌` vazgeçildi).
- **Sıra:** araştırma bulgusu **önce** deftere (Kural 13/14 ile birlikte) → sonra uygulama. Uygulama bitince `Durum` güncellenir.
- **Aynı işi tekrar yapma:** yeni bir view/özelliğe dokunmadan önce defter okunur; aynı yer için daha önce referans uygulanmışsa tekrar araştırılmaz, mevcut karar korunur (stabilite).
- **Çelişki:** defterdeki mevcut referansla çelişen yeni bir tasarım gerekiyorsa **kullanıcıya sorulur** (Kural 15); habersiz ezme yasak.
- **Kapı:** referansı kaydedilmemiş (veya `Durum` güncellenmemiş) tasarım işi tamamlanmış sayılmaz; faz kapanmaz.

---

**Bu kurallardan herhangi biri ihlal edilirse:** `dotnet build` 0 uyarı/0 hata verse bile o faz kapanmaz, kod reddedilir. Bu tek cümle tüm kurallar için geçerlidir — her maddede ayrıca tekrar edilmez.

## Mimari bekçi (derleme hatası hükmünde)
- `Libraries/MuhasibPro.Tests/ArchitectureTests.cs` modüler sınırları kaynaktan denetler (ViewModel/View EF bilmez, Business/WinUI bilmez, Data/Business bilmez, Kurulum↔SistemDb ayrık). **Bu testlerden biri kızarırsa modüler tasarım bozulmuş sayılır — `dotnet test` kırmızısı derleme hatası hükmündedir**, faz kapanmaz.
