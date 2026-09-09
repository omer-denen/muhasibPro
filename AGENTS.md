# AGENTS

Bu projede çalışmaya başlamadan ÖNCE şu dosyaları oku:

- `docs/ROADMAP.md` — fazlar, mimari kararlar, nerede kaldığımız
- `docs/KONTROL-LISTESI.md` — işaretli/bekleyen maddeler
- `docs/HATALAR.md` — tekrar edilmemesi gereken hatalar
- `docs/AKIS-PLANI.md` — uygulama akışı ve veritabanı yönetimi
- `docs/WINUI-MIMARISI.md` — pencere yönetimi
- `docs/VIEW-PACKAGE-DURUMU.md` — görsel stil paketi entegrasyon durumu (sadece View/XAML işi varsa)
- `docs/OOBE-TASARIM-SABLONU.md` — Login/Splash/SistemKurulum görsel dili, TEK kaynak (sadece View/XAML işi varsa; WebToXaml'a BAKILMAZ)

## Her oturumun başında
1. `docs/LOG.md` **indeksini** oku (kitap kapağı — sadece tablo, ~4 KB). Son 2-3 oturumun hangi `docs/LOG/LOG-XX-YY.md` cildinde olduğunu bul, sonra ilgili cildi `Read` ile aç (sadece ilgili oturum başlıkları). `LOG.md`'nin tamamını veya tüm ciltleri okuma — token tasarrufu.
2. Yeni oturum için son cilde bak: dolmadıysa (20'den az) oraya ekle, dolduysa yeni `LOG-XX-YY.md` cilt oluştur ve `LOG.md` indekse satır ekle.
3. `docs/KONTROL-LISTESI.md`'deki açık maddelere göre devam et
4. İlgili katmanın mevcut kodunu oku (`Libraries/MuhasibPro.Domain|Data|Business|ViewModels`, `MuhasibPro` [UI])

## Her oturumun sonunda
1. `docs/LOG/LOG-XX-YY.md` son cilde yapılanlar/kararlar/engeller/sonraki adım yaz ve `docs/LOG.md` indekse satır ekle
2. `docs/KONTROL-LISTESI.md`'yi güncelle
3. `docs/ROADMAP.md` faz tablosunu güncelle
4. Çözülen bug varsa `docs/HATALAR.md`'ye ekle
5. View/stil paketi ile ilgili çalışıldıysa `docs/VIEW-PACKAGE-DURUMU.md`'yi güncelle
6. Son cilt 20'yi doldurunca yeni `LOG-XX-YY.md` cilt oluştur (eski ciltler arşivdir; `LOG-ARSIV.md`'ye taşıma gerek yok, kalıcı kararlar `ROADMAP.md`'de)

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
- **Görsel dil TEK kaynak `docs/OOBE-TASARIM-SABLONU.md`.** `muhasibpro-WebToXaml` artık referans DEĞİL — özellikle SistemKurulum dark/diagnostik tasarımı için oraya BAKILMAZ. Yeni ekran/panel OOBE şablonuna göre yazılır (çok-panel düzen, `AppBackgroundBrush`, `MuhasibCardStyle`, `MuhasibPrimaryButtonStyle`, `MuhasibOnPrimaryBrush`, Loaded gölge deseni). `MuhasibPro-master` ve `muhasibpro-WebtoWinui3-v2` 2026-08-28’de silindi (artık referans değil).
- **Tema varsayılan Light:** Uygulama açılışında `RequestedTheme Light` seçili olmalı (sistem Dark olsa bile OOBE Light). `ThemeSelectorService` Light ile başlatılır. **XAML içinde tema zorlanamaz (`RequestedTheme` hardcode yasak — dialog dahil);** `ContentDialog` Light garantisi kodda tek kaynakta verilir (`DialogHelper.ShowCenteredAsync` / `DialogService.CreateDialog`).
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

---

**Bu 7 kuraldan herhangi biri ihlal edilirse:** `dotnet build` 0 uyarı/0 hata verse bile o faz kapanmaz, kod reddedilir. Bu tek cümle tüm kurallar için geçerlidir — her maddede ayrıca tekrar edilmez.

## Mimari bekçi (derleme hatası hükmünde)
- `Libraries/MuhasibPro.Tests/ArchitectureTests.cs` modüler sınırları kaynaktan denetler (ViewModel/View EF bilmez, Business/WinUI bilmez, Data/Business bilmez, Kurulum↔SistemDb ayrık). **Bu testlerden biri kızarırsa modüler tasarım bozulmuş sayılır — `dotnet test` kırmızısı derleme hatası hükmündedir**, faz kapanmaz.
