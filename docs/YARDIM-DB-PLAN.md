# MuhasibPro — AI Yardım Bilgi Tabanı (AsistanBilgi.db + Hibrit RAG) Planı

> **Faz 6.93** — AI asistanının yardım kaynağını view statiklerinden ayırıp tek bir **yardım bilgi tabanına** taşır.
> **Sahiplik (Oturum 293 — tek sahip):** AI asistanın tamamı (motor + veri + UI/entegrasyon/doğrulama) **ana modelde**. Yardımcı-model dönemi kapandı → `docs/Arsiv/YARDIMCI-MODEL-ARSIV.md`; işler `Faz 6.97` (`docs/AI-ASISTAN-MODUL-PLAN.md`).
> **Bu dosya sözleşmedir:** interface/DTO imzaları ve davranış buradan değişmez; değişiklik gerekiyorsa kullanıcıya sorulur (Kural 15) ve sözleşme revizyonu olarak LOG'a işlenir.
>
> **Uzlaşma notu:** Diğer modelin LOG notunda geçen ad **`AsistanBilgi.db`** benimsendi (tek ad). Faz A'da önerilen **FTS5 yerine** küçük derlem için **in-memory lexical + embedding + RRF** tercih edildi (araştırma: `REFERANSLAR` 278 — vektör zaten var, FTS5'in Türkçe kökleyicisi yok, corpus küçük). FTS5 yalnız derlem büyürse sonraki revizyonda düşünülür.

## Kararlar (kullanıcı onayı — Oturum 278)
| Konu | Karar |
|---|---|
| Kapsam (v1.2 — Oturum 293) | **Tek yardım kaynağı = tüm uygulama.** `docs/yardim/*.md` uygulamanın tamamını kapsar; UI'daki ayrı `?` yardım dialogları **kaldırılır** (tek yüzey AI asistanı: F1 + statü çubuğu). Bkz. `YARDIM-TEK-YUZEY-PLAN.md`. |
| İçerik kaynağı | **Repoda Markdown** (`docs/yardim/*.md`), build'de uygulamaya gömülür (EmbeddedResource) |
| Arama | **Hibrit:** lexical (FTS'siz, in-memory) + **embedding (Foundry)** → **RRF** füzyon |
| Embedding modeli | **Evet** — `qwen3-embedding-0.6b` (~495 MB, bir kez iner; Foundry katalog) |
| DB konumu | **Ayrı** `%AppData%\MuhasibPro\AsistanBilgi.db` (Sistem.db migration/yedek hattına girmez) |

## Araştırma özeti (kod öncesi — `REFERANSLAR` Oturum 278)
- Foundry Local .NET SDK'da **yerel embedding** var (`GetEmbeddingClientAsync()`), MS resmî RAG örneği küçük derlemde **kaba kuvvet cosine** kullanıyor ("small collections → vector DB gereksiz").
- Sektör/araştırma uzlaşması: küçük + alana özel derlemde FTS5/BM25 ≈ embedding; **hibrit (BM25/lexical + embedding, RRF)** en iyi pratik; vektör DB aşırı.
- **Türkçe:** FTS5'te Türkçe kökleyici yok (porter İngilizce); ek yapısı yüzünden salt anahtar-kelime zayıf → embedding'in ana katkısı Türkçe ek/sinonim yakalamak.
- Türkçe (TQuAD) dahil on-device retrieval karakterizasyonu: hibrit tutarlı kazanıyor.

## İçerik formatı (repoda)
`docs/yardim/` altında **bir dosya = bir yardım sayfası**:

```markdown
# Mali Dönem Yönetimi

## Mali dönem nasıl arşivlenir?
Dönem listesinde ilgili dönemi seçip "Arşivle" ...
Etiket: arşiv, dönem

## Yeni mali dönem nasıl açılır?
...
```

- `#` → **sayfa başlığı**; `##` → **madde başlığı**; sonraki `##`'a kadar olan metin → **madde gövdesi**.
- `##` başlığından sonra gelen `Etiket: a, b` satırı (opsiyonel) etiketleri verir; gövdeye dahil edilmez.
- İçerik **yalnız AI** içindir; view `?` yardım metinlerinden bağımsız yazılır.
- **Başlangıç kapsamı:** mevcut 8 görünümün temel işlemleri (login, firma/dönem seçimi, mali dönem yönetimi, dönem güncelleme, veritabanı/yedek, güncelleme, AI ayarları, geliştirici araçları). İçerik kısıtlı ve kontrollü tutulur (Kural 12/13).

## Veri modeli — `AsistanBilgi.db` (SQLite, EF migration YOK)
Konum: `Path.Combine(IApplicationPaths.GetAppDataFolderPath(), "AsistanBilgi.db")` — **hardcode yol yasak** (Kural 7).
**FTS5 yok:** lexical sıralama bellekte (yüklü kayıtlar üzerinde); DB yalnız `dataset + vektör` tutar.
Erişim: `Microsoft.Data.Sqlite` (Business, raw ADO). Açılışta `SQLitePCL.Batteries_V2.Init()` güvencesi.

```sql
CREATE TABLE Meta   (Anahtar TEXT PRIMARY KEY, Deger TEXT NOT NULL);
-- IcerikSurumu (SHA256), VektorModel, VektorBoyut, DizinTarihi

CREATE TABLE Madde (
  Id INTEGER PRIMARY KEY,
  Anahtar TEXT NOT NULL,        -- {dosyaAdi}#{sira} (stabil)
  Sayfa TEXT NOT NULL,
  Baslik TEXT NOT NULL,
  Icerik TEXT NOT NULL,
  Etiketler TEXT NOT NULL DEFAULT '',   -- virgülle
  IcerikHash TEXT NOT NULL      -- SHA256(Baslik + "\n" + Icerik)
);
CREATE UNIQUE INDEX UX_Madde_Anahtar ON Madde(Anahtar);

CREATE TABLE Vektor (
  MaddeId INTEGER NOT NULL REFERENCES Madde(Id) ON DELETE CASCADE,
  ModelAlias TEXT NOT NULL,
  Boyut INTEGER NOT NULL,
  Vektor BLOB NOT NULL,          -- float32 little-endian
  PRIMARY KEY (MaddeId, ModelAlias)
);
```

- İçerik sürümü (`IcerikSurumu`) değişince: `Madde` yeniden yazılır, değişmeyen `IcerikHash`'lerin vektörü **korunur**, yalnız değişen/yeni maddeler yeniden gömülür.
- `VektorModel`/`VektorBoyut` farklıysa tüm vektörler yeniden üretilir.
- Dosya bozuksa/şema uymuyorsa: dosya **yeniden oluşturulur** (bu bir önbellek; kullanıcı verisi değil).

## Kontratlar (Business.Contracts — `SistemServices/AiAsistan`)
```csharp
public class YardimKaydi
{
    public string Anahtar { get; set; } = "";
    public string Sayfa { get; set; } = "";
    public string Baslik { get; set; } = "";
    public string Icerik { get; set; } = "";
    public IReadOnlyList<string> Etiketler { get; set; } = [];
}

public class YardimAramaSonucu
{
    public string Anahtar { get; set; } = "";
    public string Sayfa { get; set; } = "";
    public string Baslik { get; set; } = "";
    public string Icerik { get; set; } = "";
    public double Skor { get; set; }
    public string Yontem { get; set; } = "";   // "lexical" | "vektor" | "hibrit"
}

public class YardimIndexDurumu
{
    public bool HazirMi { get; set; }
    public int MaddeSayisi { get; set; }
    public bool VektorVarMi { get; set; }
    public string Asama { get; set; } = "";
    public double? IlerlemeYuzde { get; set; }
    public string Mesaj { get; set; } = "";
}

/// <summary>AI yardım bilgi tabanı: içerik + (varsa) semantik indeks üzerinden hibrit arama.</summary>
public interface IYardimBilgiTabani
{
    Task<YardimIndexDurumu> DurumGetirAsync(CancellationToken ct = default);
    /// <summary>İçeriği okur, DB'yi tazeler, eksik vektörleri üretir. Embedding yoksa lexical'e düşer (fırlatmaz).
    /// <paramref name="modelIndirmeyeIzin"/> false ise embedding modeli İNDİRİLMEZ; yalnız önbellekte varsa kullanılır (güncelleme adımı 6.91-G).</summary>
    Task HazirlaAsync(bool modelIndirmeyeIzin = true, IProgress<YardimIndexDurumu>? ilerleme = null, CancellationToken ct = default);
    Task<IReadOnlyList<YardimAramaSonucu>> AraAsync(string soru, int enFazla, CancellationToken ct = default);
    /// <summary>Yüklü tüm kayıtlar (dev-mode öz-testi sayımı için).</summary>
    IReadOnlyList<YardimKaydi> TumKayitlar();
}

/// <summary>Metni vektöre çevirir (Foundry embedding). Model indirilemezse FIRLATIR — çağıran lexical'e düşer.</summary>
public interface IYardimVektorUretici
{
    string ModelAlias { get; }
    Task<int> VektorBoyutuAsync(CancellationToken ct = default);
    /// <summary>Embedding modeli önbellekte/yüklenebilir mi (İNDİRME YAPMAZ). Güncelleme adımı bunu kontrol eder.</summary>
    Task<bool> OnbellekteMiAsync(CancellationToken ct = default);
    Task<IReadOnlyList<float[]>> UretAsync(IReadOnlyList<string> metinler, IProgress<double>? ilerleme = null, CancellationToken ct = default);
}

public class YardimHamKaynak { public string DosyaAdi { get; set; } = ""; public string HamMetin { get; set; } = ""; }

/// <summary>Gömülü yardım içeriği kaynağı (Markdown).</summary>
public interface IYardimIcerikKaynagi { IReadOnlyList<YardimHamKaynak> KaynaklariGetir(); }
```

## Davranış (net)
- **`HazirlaAsync(modelIndirmeyeIzin, ilerleme, ct)`:** içerik oku → parse → hash karşılaştır → Madde yaz → eksik vektörleri `IYardimVektorUretici.UretAsync(..., IProgress<double>)` ile üret (ilerleme `YardimIndexDurumu`'na eşlenir) → meta yaz.
  - `modelIndirmeyeIzin=false` ve `IYardimVektorUretici.OnbellekteMiAsync()` false ise **vektör adımı atlanır** (model indirilmez) → `VektorVarMi=false` + lexical-only.
  - Embedding adımı **hata verirse iptal dışında fırlatmaz**; `VektorVarMi=false` + `Mesaj` (sebep) ile biter (lexical fallback canlı kalır).
- **`AraAsync`:** soru jetonla → **lexical** skor (başlık ağırlıklı; büyük/küçük + diakritik normalizasyonu; token "starts-with" eşleşmesi) → `VektorVarMi` ise sorguyu göm + **cosine** ile sırala → **RRF** (`k=60`, `w_lex=0.4`, `w_vec=0.6`; vektör yoksa `w_lex=1`) → top `enFazla` → `Yontem` etiketi.
- **Eşzamanlılık:** `SemaphoreSlim` (hazırla/ara yarışı yok). Boş/tek-kelime soruda lexical yeterli; cosine için ham skor eşiği uygulanmaz (küçük derlem).
- **İlk kullanımda indirme:** embedding modeli yoksa `IYardimVektorUretici` (App/Foundry impl) modeli indirip yükler (ilerleme verir); başarısızsa `AraAsync` lexical sonuç döndürür, kullanıcı hata görmez (yalnız durum satırında "semantik indeks yok" bilgisi).
- **Prompt:** `AsistanPromptKurucu` artık `YardimAramaSonucu` listesi alır (uyarı-strip, bağlam, geçmiş kırpma davranışı **aynı**).

## Dosya yerleşimi + sahiplik (Oturum 293 — tek sahip: ana model)
**Ana model (hepsi — motor + veri + UI/entegrasyon/doğrulama):**
- `Business/Contracts/SistemServices/AiAsistan/IYardimBilgiTabani.cs` (yukarıdaki DTO+interface'ler)
- `Business/Services/SistemServices/AiAsistan/YardimMarkdownCozumleyici.cs` (saf)
- `Business/Services/SistemServices/AiAsistan/YardimSkorlayici.cs` (mevcut, geliştirilir: Türkçe normalizasyon + starts-with)
- `Business/Services/SistemServices/AiAsistan/RrfBirlestirici.cs` (saf)
- `Business/Services/SistemServices/AiAsistan/YardimVektorDeposu.cs` (SQLite, `AsistanBilgi.db`)
- `Business/Services/SistemServices/AiAsistan/YardimBilgiTabaniService.cs` (orkestratör, Singleton)
- `MuhasibPro/Services/AiAsistan/GomuluYardimIcerikKaynagi.cs` (EmbeddedResource okuyucu)
- `MuhasibPro/Services/AiAsistan/FoundryYardimVektorUretici.cs` (Foundry embedding; `FoundryAsistanSohbetService` deseni)
- `Domain/Models/AiAsistanSettings.cs` → `EmbeddingModelAlias` + `VarsayilanEmbeddingModelAlias = "qwen3-embedding-0.6b"` + `GetEmbeddingModelAlias()`
- App csproj: `<EmbeddedResource Include="..\docs\yardim\**\*.md" Link="Yardim\%(Filename)%(Extension)" />` (**içerik dosyalarını BENTE** — parser'ı kendi test fixture'ınla test et, `docs/yardim/*.md` dosyalarına yazma)
- `MuhasibPro/Services/AiAsistan/FoundryAsistanSohbetService.cs` → retrieval'ı `IYardimBilgiTabani.AraAsync`'e çevir
- `Business/Services/SistemServices/AiAsistan/AsistanPromptKurucu.cs` → `YardimAramaSonucu` overload
- **SİLME (bu adımda):** `IYardimIcerikSaglayici.cs` **silinmez** — `FoundryAsistanSohbetService` onu kullanmayı bırakır ama dosya **Adım 2'ye kadar kalır** (aşağıdaki sıra notu).
- **Test:** `YardimMarkdownCozumleyiciTests`, `RrfBirlestiriciTests`, `YardimBilgiTabaniTests` (temp-dir SQLite + sahte `IYardimVektorUretici`), `YardimSkorlayiciTests` güncelle

> **SIRA NOTU (derleme güvenliği):** `YardimIcerikToplayici` (bende) `IYardimIcerikSaglayici`'yi implement eder. Bu yüzden arayüzü **Adım 1'de silmek derlemeyi kırar** (ders: `HATALAR` 277/278). Sıra: **Adım 1** (yardımcı) yeni KB'yi yazar + `FoundryAsistanSohbetService`'i çevirir, **eski arayüz/dosya KALIR**; **Adım 2** (ben) öz-testi `IYardimBilgiTabani`'na çevirir → sonra `YardimIcerikToplayici.cs` **+** `IYardimIcerikSaglayici.cs` **+** DI kaydı birlikte silinir.

**Ana model (içerik + UI/entegrasyon/doğrulama — sözleşme dondurulduktan sonra):**
- **İçerik:** `docs/yardim/*.md` — başlangıç seti (9 sayfa) Oturum 278'de yazıldı; gözden geçirme/genişletme bende (Kural 13/14/19)
- **Sil (Kural 4 — Adım 2, birlikte):** `ViewModels/Services/YardimIcerikToplayici.cs` + `Tests/YardimIcerikToplayiciTests.cs` + `Business/Contracts/SistemServices/AiAsistan/IYardimIcerikSaglayici.cs` + DI kaydı (`AddSingleton<IYardimIcerikSaglayici, …>`) — öz-test `IYardimBilgiTabani`'na çevrildikten sonra
- `ViewModels/ViewModels/Shell/GelistiriciAraclariViewModel.cs` → öz-test "RAG derlemi" satırı `IYardimBilgiTabani.TumKayitlar().Count`
- `ViewModels/ViewModels/Shell/AsistanSohbetViewModel.cs` + `Views/MainShell/Components/AsistanSohbetPaneli.xaml` → **"Yardım dizini hazırlanıyor"** durumu/ilerlemesi (Kural 11/12)
- `Views/DenetimMasasi/**` (Yapay Zeka paneli) → dizin durum satırı (madde sayısı + semantik indeks var/yok)
- `MuhasibPro/HostBuilders/AddCommonServiceHostBuilderExtensions.cs` → DI (`IYardimBilgiTabani` Singleton, `IYardimIcerikKaynagi`, `IYardimVektorUretici` Singleton) + eski kayıt temizliği
- Kural 13/14/19: `?` yardım metinleri (AI paneli) + `REFERANSLAR` + `docs/yardim` içeriğinin gözden geçirilmesi/genişletilmesi (motor başlangıç içeriğini bırakır)
- Build/test + **Kural 18 canlı** + dokümanlar (LOG/KONTROL/DURUM/ROADMAP)

**Sahiplik (Oturum 293 — tek sahip):** AI motor/veri/model + UI/VM/XAML/DI/içerik/doğrulama ve paylaşılan docs = **ana model**. Sınır sözleşmelerle çizilir (`IAsistanSohbetService`, `IYardimBilgiTabani`).

## Kabul kriterleri
- Build 0 hata; test yeşil (beklenen ~617 + yeni testler).
- `docs/yardim/*.md` güncellenince `AsistanBilgi.db` otomatik tazelenir; yalnız değişen maddeler yeniden gömülür.
- Aynı soru **embedding varken** semantik eşleşmeyle (ör. "arşivleme" ↔ "arşiv" / ek farkı) doğru maddeyi bulur; **embedding yokken** lexical ile çalışmaya devam eder (fırlatmaz).
- `AsistanBilgi.db` bozulursa uygulama çökmez, yeniden oluşturur.
- AI panelinde dizin durumu görünür ve ring kapanır (Kural 11).
- Canlı: (S1) içerik/indeks kurulumu, (S2) anlamsal soru → doğru madde, (S3) embedding'siz fallback, (S4) içerik güncelle→yeniden indeks.
- `REFERANSLAR`/`LOG`/`KONTROL-LISTESI`/`DURUM` güncel; kullanıcı onayı.

## Güncelleme modülü entegrasyonu (Faz 6.91-G — kullanıcı isteği, Oturum 278)
`AsistanBilgi.db` uygulama güncellemesinde **tazelenmelidir** (gömülü içerik sürümü değişir). Güncelleme modülüne (6.91 post-update sagası) **4. adım** olarak eklenir:

- **Ad adı:** `AI Yardım Dizini`. Sıra: Uygulama → Sistem.db → Dönemler → **AI Yardım Dizini** (en son, kritik olmayan).
- **Davranış:** `IYardimBilgiTabani.HazirlaAsync(ilerleme, ct)` çağrılır.
  - İçerik sürümü aynıysa ve DB geçerliyse → hızlı **Atlandı/Güncel** (yeniden gömme yok).
  - İçerik değiştiyse → yeni/değişen maddeleri yaz; vektörleri **yalnız embedding modeli önbellekteyse** üret (güncelleme sırasında **model İNDİRİLMEZ**); yoksa lexical-only + uyarı ("semantik indeks yok").
  - Bozuk/uyumsuz `AsistanBilgi.db` → dosya **yeniden oluşturulur** (önbellek; migration/restore YOK, ileri-uyumluluk guard'ı uygulanmaz).
- **Hata politikası:** bloklamaz. Hata/uyarı → adım `Uyari` (sonuç türü **Dikkat**); uygulama açılışı **bloklanmaz** (Sistem.db'den farklı: bu bir önbellek).
- **Sahiplik:** saga adımı + `GuncellemeSonrasiView` 4. adım rozeti (**bende**, Kural 8 onayı) — motor API'si (`HazirlaAsync`) **diğer modelin** (frozen sözleşme).
- **Bağımlılık:** 6.93 motoru (Adım 1) bitmeden 6.91-G kodlanamaz (arayüz gerekir).
- **Tetik:** sagayı tetikleyen mevcut `UpdateSettingsModel.PostUpdatePending` aynen geçerli; ek bayrak yok.

## Açık noktalar (motor çalışırken netleşecek)
- Embedding modeli `AsistanBilgi.db` küçük hacimli — `DiskKullanimiAsync` sayımına dahil olur (Denetim "İndirilmiş modeller"de görünür).
- İçerik dilbilgisi ileride genişlerse (JSON/ofset) sözleşme revizyonu gerekir; v1 Markdown.
- `IYardimVektorUretici` için model seçimi ayarı (`EmbeddingModelAlias`) Denetim UI'ında ayrı gösterilecek mi → 6.93 UI adımında karar.
