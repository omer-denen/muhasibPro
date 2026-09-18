# AGENTS

Bu projede çalışmaya başlamadan ÖNCE şu dosyaları oku:

- **`docs/DURUM.md` — İLK OKU:** "nerede kaldık" tek yüzey (aktif faz + açık kararlar + sonraki adım + kapılar/komutlar). Uzun log okuması gerekmez.
- `docs/ROADMAP.md` — fazlar + güncel kararlar (geçmiş kararlar `docs/Arsiv/KARAR-LOGU-ARSIV.md`)
- `docs/KONTROL-LISTESI.md` — açık maddeler (kapanan fazlar `docs/Arsiv/KONTROL-ARSIV.md`)
- `docs/HATALAR.md` — son 25 hata dersi (eskiler `docs/Arsiv/HATALAR-ARSIV.md`)
- `docs/AKIS-PLANI.md` — uygulama akışı ve veritabanı yönetimi
- `docs/WINUI-MIMARISI.md` — pencere yönetimi
- `docs/TASARIM-KURALLARI.md` — katmanlı sayfa yapısı (zemin → ana border → kartlar), Kural 17'nin ayrıntısı (her View/XAML işinde okunur)
- `docs/CEKIRDEK-MODUL-PLAN.md` — çekirdek faz planı ve Kesin Kurallar (Kural 7: modelden gelen gerçek veri, örn. yedek saklama `GetManuelKeep()` → `CleanOldBackupsAsync` request model ayardan okur)
- `docs/AYARLAR-PANEL-PLAN.md` — per-view ayar panelleri haritası (varsa; her view kendi ayarını yapar)
- `docs/YARDIM-DB-PLAN.md` — Faz 6.93 AI Yardım Bilgi Tabanı (`AsistanBilgi.db` + hibrit RAG) **dondurulmuş sözleşmesi** + motor/UI dosya sahipliği
- `docs/YARDIM-TEK-YUZEY-PLAN.md` — Faz 6.94 Tek Yardım Yüzeyi (AI asistanı) planı: view `?` yardım kaldırılır, tek kaynak `docs/yardim/*.md` → `AsistanBilgi.db` → asistan (F1 + statü çubuğu)
- `docs/KULLANICI-YONETIMI-PLAN.md` — Faz 6.85 Kullanıcı Yönetimi + RBAC planı (kullanıcı → firma rolü → izin + modül/alan erişim kapısı) **ÖNCELİKLİ FAZ**; mevcut durum kod doğrulamalı + K1-K6
- `docs/AKTIVASYON-MODUL-PLAN.md` — Faz 6.95 Aktivasyon & Modül Kilidi (KEY) + ilk giriş; tenant (mali dönem) DB'si etkin modül şemasıyla + ek modülde göç; A1-A6
- `docs/DB-PROVIDER-PLAN.md` — Faz 6.96 Çoklu DB Provider (SQLite varsayılan + PostgreSQL/SQL Server); SQLite bağımlılık haritası + D1-D8
- `docs/REFERANSLAR.md` — referans defteri: alınan kaynak + hangi view/özelliğe uygulandığı (Kural 19; yeni view/özellikten önce oku, aynı işi tekrar yapma)

> **Arşiv:** `docs/Arsiv/` + `docs/LOG/Arsiv/` okuma yolunda DEĞİL — yalnız grep/arama gerekirse açılır. **Bu proje ana projedir;** eski referans dönemler (AI-Studio/WebToXaml/OOBE-şablonu vb.) yürürlükten kalktı ve arşivdedir.

## Her oturumun başında
1. **`docs/DURUM.md`'yi oku** — "nerede kaldık" tek yüzey. Ardından yalnız göreve gereken dosyayı oku (grep/ilgili bölüm); tüm log/cilt/KONTROL'ü baştan okuma (token tasarrufu).
2. **Planı oku:** `docs/CEKIRDEK-MODUL-PLAN.md` (Kesin Kurallar + ilgili faz) ve varsa `docs/AYARLAR-PANEL-PLAN.md`. Eksik okuma = plan hatası hükmündedir. İlgili fazın **Kural 7 örneği** (`yedek saklama` → `GetManuelKeep()` → `CleanOldBackupsAsync` request model ayardan okur) gibi kritik satırları `Read` ile doğrula, ezberden yazma.
3. **Geçmiş gerekirse:** `docs/LOG.md` kompakt indeksinden ilgili oturumu bul; güncel cilt `docs/LOG/LOG-261-280.md`, eski ciltler `docs/LOG/Arsiv/`. Yalnız ilgili oturum başlığını oku.
4. Yeni oturum kaydı: güncel cilt 20'yi doldurmadıysa oraya ekle, dolduysa yeni `LOG-XX-YY.md` cilt oluştur + `LOG.md` indekse satır ekle; kapanan ciltleri `docs/LOG/Arsiv/`'e taşı.
5. `docs/KONTROL-LISTESI.md`'deki açık maddelere göre devam et
6. İlgili katmanın mevcut kodunu oku (`Libraries/MuhasibPro.Domain|Data|Business|ViewModels`, `MuhasibPro` [UI])

## Her oturumun sonunda
1. `docs/LOG/LOG-XX-YY.md` son cilde yapılanlar/kararlar/engeller/sonraki adım yaz ve `docs/LOG.md` indekse satır ekle
2. `docs/KONTROL-LISTESI.md`'yi güncelle
3. `docs/ROADMAP.md` faz tablosunu güncelle
4. Çözülen bug varsa `docs/HATALAR.md`'ye ekle
5. **`docs/DURUM.md`'yi güncelle** — aktif faz + açık kararlar + sonraki adım + bilinen açık uçlar
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
- **Görsel dil TEK kaynak: Windows 11 Fluent.** Tüm ekranlar Fluent tasarım diline geçirilir (Faz 6.75); pencereler Mica-uyumlu olur (Mica backdrop + Fluent malzeme/kontrast). Yeni işler sistem stilleri (`TableView`, `NavigationView`, standart dialog/button stilleri) ile yazılır; özel token/stil yalnız Fluent'te karşılığı yoksa `DesignTokens`/`Styles` altına eklenir. Eski OOBE/InventEase dönemi yürürlükten kalktı (arşivde); tek kaynak bu kurallar.
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

### 13. Araştır-önce + tek yardım yüzeyi
- **Kritik yapıya başlanmadan veya refactoring yapılmadan ÖNCE** ilgili işlem internette araştırılır (resmi docs/kaynak öncelikli); bulgu + karar LOG'a işlenir, **ondan sonra** işleme başlanır. Bulunamazsa kullanıcıya sorulur (Kural 12).
- **Tek yardım yüzeyi = AI asistanıdır (Oturum 293 revizyonu).** Ayrı view `?` yardım dialogu/sayfaları **yoktur/kaldırılır**; yardım girişi **F1** ve statü çubuğundaki **"Asistan"** düğmesidir (aynı panel). İçerik tek kaynaktan gelir: `docs/yardim/*.md` → `AsistanBilgi.db` → asistan. AI erişilemezse (lisans/yetki/kapalı/model yok) yalnız **kilit gerekçesi** gösterilir; statik yardım yok. Kararlar/ayrıntı: `docs/YARDIM-TEK-YUZEY-PLAN.md` (Oturum 283).
- **Eski yardım altyapısı silinir (Kural 4):** `YardimMaddeleri()`/`YardimGoster`/`YardimCommand` + `YardimAnahtari`/`YardimBasligi`, `YardimDialog`/`YardimMaddesi`, `YardimMaddesiDto`, `ShowYardimAsync` ve view `?` butonları.
- **İçerik ekranla birlikte yaşar:** bir ekranın içeriği/fonksiyonu değişen her iş, aynı işin içinde `docs/yardim` ilgili sayfasını da günceller (sonraya/ayrı faza bırakılmaz; güncellenmemiş içerik = eski bilgi = bug hükmündedir). İçerik değişiminde `AsistanBilgi.db` otomatik tazelenir.

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
🔒 **MÜHÜR (Oturum 254 — kullanıcı kararı):** Bu mimari **kilitlidir** — ana tasarım dili `zemin → ana border (ana kart) → içerik kartları`. Bundan sonra oluşturulacak/güncellenecek **her View, UserControl ve Dialog bu yapı üzerine inşa edilir.** Görünüm işlerinde kapsam yalnızca: (a) View içindeki kontrol yerleşimleri (elemanların yeri/düzeni), (b) **araştırma sonucu** (Kural 14) eklenen/çıkarılan buton ve kontrollerdir. **Katman 2 ana border ve Katman 3 iç kart stilleri (renk/radius/gölge/padding/hairline/kart arası boşluk) mührün parçasıdır; habersiz değiştirilmez** — değişiklik ancak kullanıcı kararıyla (mühür revizyonu) yapılır.

- **Katman 1 — zemin (kök `Border`, tüm sayfayı kaplar):**
  - Sayfanın en dış elemanı **`Border`** olur; kök `Grid` + `Background` kombinasyonu **yasak**.
  - İçinde `Image` (`Stretch="UniformToFill"`, kaynağı `UygulamaZeminGorseli` = **tek görsel `light.jpg`**) + üzerine **tema perdesi** (`MuhasibZeminPerdeBrush`: Light `#59F3F3F3` ≈%35, Dark `#D9202020` ≈%85 — zemin görünür kalır). **Ham/net resim gösterilmez — her zaman perde üstte.**
  - Statiktir; sayfa içeriği değişince yeniden oluşturulmaz. (Oturum 251 kararı: tek görsel + tema perdesi = iki ayrı görselden daha stabil.)
- **Katman 2 — ana border (ana panel):**
  - Zeminin üzerinde duran, tüm içeriği kapsayan tek çerçeveli panel: `Background="{ThemeResource CardBackgroundFillColorSecondaryBrush}"`, `BorderBrush="{ThemeResource MuhasibAnaBorderCizgiBrush}"` (Light beyaz `#FFFFFFFF` — Oturum 258 mühür revizyonu, kullanıcı kararı / Dark yumuşak gri `#45C9CCD1`), `BorderThickness="1.5"` (Oturum 268 mühür revizyonu, kullanıcı kararı: "bir tık arttır"), `CornerRadius="{StaticResource OverlayCornerRadius}"`, `Padding="24"`, `Translation="0,0,32"`.
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
  - **Master-detail sayfalarda (liste + detay):** detay panelinin sağ üstünde, yukarı açılan bir **büyüt/küçült (expander)** butonu olur; büyütünce **liste kapanır** ve detay tüm içerik alanını kaplar (`FirmalarView`/`KullaniciYonetimiView` emsali; sonraki master-detail tasarımlarında zorunlu).
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

### 20. İş sırası — elindeki işi bitir, yenisini sıraya al (KESİN)
**Bir kez başlanan iş, yeni talimat gelse bile yarım bırakılmaz.** (Kullanıcı kuralı, Oturum 286.)
- Elindeki (başlanmış) iş **tamamlanmadan** yeni talimata geçilmez; yeni talimat **sıraya alınır**.
- Kullanıcı iş sırasında yeni bir şey yazarsa sıra: **(1)** elindeki işi bitir → **(2)** sırada bekleyeni yap. Ara vermek zorundaysan (ör. kullanıcı onayı/kararı gerekiyorsa) bunu **açıkça söyle** ve işi "yarım" olarak kaydet.
- **İstisna — aynı iş:** Kullanıcının yazdığı şey **üzerinde çalışılan işle ilgiliyse** (geri bildirim/düzeltme/ayrıntı), bu "yeni iş" sayılmaz; sıraya alınmaz, **doğrudan mevcut işe uygulanır** (iş kesilmez).
- **Söyle:** Yeni (farklı) bir talimat geldiğinde agent, "şu an X işini bitiriyorum, senin yazdığın Y'yi sıraya aldım / X bitince Y'ye geçeceğim" der. Sessizce konu değiştirmek yarım iş bırakır — kabul edilmez.
- **Kapı:** Elindeki iş build 0/0 + test + (gerekiyorsa) canlı/kanıt ile kapanmadan yeni işe başlamak süreç ihlalidir.

### 21. Sahte/gösterimlik içerik yasak — UI'da yalnız gerçekten var olan (KESİN)
**Hiçbir yüzeyde (navbar/menü, buton, liste, rozet, başlık, kart) projede gerçekten var olmayan modül/özellik/veri gösterilmez.** (Kullanıcı kuralı, Oturum 291.)
- Projede karşılığı olmayan menü öğesi/buton/modül **konmaz**; "örnek/mock/placeholder/yakında" görsel giriş yasaktır.
- Sahte sabit veri (uydurma firma/şirket adı, sabit kullanıcı/rol/dönem) gösterilmez; gösterilecekse **gerçek veriye bağlanır** (Kural 7).
- Bir özellik henüz yoksa arayüzde **hiç görünmez**; boş/pasif bırakıp varmış gibi gösterilmez.
- Erişim/izin kapıları yalnız **gerçek** yüzeylere bağlanır; olmayan modül için kapı yazılmaz.
- **Kapı:** UI'da projede olmayan bir giriş bulunursa o iş `dotnet build` 0/0 olsa bile kapanmaz; önce kaldırılır (Kural 4 ile birlikte).

---

**Bu kurallardan herhangi biri ihlal edilirse:** `dotnet build` 0 uyarı/0 hata verse bile o faz kapanmaz, kod reddedilir. Bu tek cümle tüm kurallar için geçerlidir — her maddede ayrıca tekrar edilmez.

## Mimari bekçi (derleme hatası hükmünde)
- `Libraries/MuhasibPro.Tests/ArchitectureTests.cs` modüler sınırları kaynaktan denetler (ViewModel/View EF bilmez, Business/WinUI bilmez, Data/Business bilmez, Kurulum↔SistemDb ayrık). **Bu testlerden biri kızarırsa modüler tasarım bozulmuş sayılır — `dotnet test` kırmızısı derleme hatası hükmündedir**, faz kapanmaz.
