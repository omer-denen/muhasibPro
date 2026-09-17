# MuhasibPro

> Windows 11 **Fluent** tasarım diline sahip, **çok-kiracılı (multi-tenant)** masaüstü **ön muhasebe** yazılımı.
>
> **DeepSeek V4.1 Flash** ile geliştirilmektedir.

WinUI 3 (Windows App SDK) + .NET 8 üzerinde, katmanlı (Domain / Data / Business / ViewModels / UI) ve MVVM + ServiceLocator mimarisiyle geliştirilir. Amaç: yerel (offline) çalışan, verisi makineden çıkmayan, firma ve mali dönem bazlı yönetilen bir ön muhasebe çözümü.

## Genel Bakış

- **Çok-kiracılı veritabanı modeli:** Tek `Sistem.db` (kullanıcı, firma, mali dönem, ayarlar, lisans) + her firma/mali dönem için ayrı veritabanı dosyası.
- **Firma & mali dönem yönetimi:** Yeni dönem açma, arşivleme, silme; dönem yedeği alma/geri yükleme; bakım (VACUUM/REINDEX/WAL), derin analiz ve sağlık raporları.
- **Yedek ve geri yükleme güvenliği:** Tek kapıdan geçen geri yükleme; yedek analizi (bozuk / yeni sürüm / kayıp kayıt) ve kayıp varsa 6 haneli onay kodu.
- **Güncelleme altyapısı (Velopack):** Git tabanlı güncelleme akışı; güncelleme öncesi zorunlu doğrulanmış yedek; ileri-uyumluluk guard'ı; güncelleme sonrası doğrulama sagası ve kurtarma.
- **Yerel AI yardım asistanı:** Foundry Local ile süreç-içi çalışan, uygulamaya gömülü yardım bilgi tabanı (`AsistanBilgi.db`) üzerinden **hibrit arama** (anahtar kelime + anlamsal embedding + RRF). Veri makineden çıkmaz.
- **Denetim Masası:** Görünüm, güvenlik, firma, veritabanı, mali dönem, güncelleme ve yapay zeka ayarları; DEBUG'da geliştirici araçları (tanılama, modül testleri).
- **Fluent tasarım:** Mica uyumlu pencereler; katmanlı sayfa yapısı (zemin → ana panel → içerik kartları); tema (sistem/açık/koyu) desteği.

## Mimari

```
MuhasibPro.Domain        -> Entity, enum, model (bağımlılıksız)
MuhasibPro.Data          -> EF Core DbContext, repository, migration, yedek servisleri
MuhasibPro.Business      -> Servis sözleşmeleri + implementasyonları (contracts-first)
MuhasibPro.ViewModels    -> MVVM ViewModel'ler (ICommonServices + Business.Contracts)
MuhasibPro (UI)          -> WinUI 3 Views, UserControl, dialog, tema ve stiller
```

- ViewModel/View katmanı EF Core/DbContext bilmez; veriye yalnız `Business.Contracts` üzerinden erişir.
- Sözleşme → servis → repo zinciri korunur; modüler sınırlar mimari testleriyle (`MuhasibPro.Tests`) denetlenir.

## Teknolojiler

| Alan | Teknoloji |
|---|---|
| UI | WinUI 3 / Windows App SDK, CommunityToolkit (SettingsControls), DevWinUI, WinUI.TableView |
| Runtime | .NET 8 (Windows), C# |
| Veri | EF Core + SQLite (varsayılan; çoklu sağlayıcı desteği için yol haritasında) |
| Yapay Zeka | Microsoft Foundry Local (yerel embedding + sohbet), SQLite tabanlı bilgi tabanı |
| Güncelleme | Velopack (GitHub Releases) |

## Derleme ve Test

```powershell
dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo
dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug
```

- Hedef platform: **x64**, Windows 10 19041+.
- Geliştirme veri kökü: `MuhasibPro/Databases` (üretimde `%AppData%\MuhasibPro`).

## Yol Haritası (kısa)

| Faz | Konu | Durum |
|---|---|---|
| 6.85 | Kullanıcı Yönetimi + RBAC (kullanıcı → firma rolü → izin, modül/alan erişim kapısı) | Planlandı (öncelikli) |
| 6.95 | Aktivasyon & Modül Kilidi (KEY) + ilk giriş; tenant şeması | Planlandı |
| 6.96 | Çoklu veritabanı sağlayıcı (SQLite varsayılan + PostgreSQL/SQL Server) | Planlandı |
| B | Muhasebe modülleri (Cari, Stok, Fatura, Kasa/Banka, Çek/Senet, ...) | Devam ediyor |

Güncel durum ve faz detayları için `docs/ROADMAP.md` ve `docs/DURUM.md`.

## Dokümantasyon

| Dosya | İçerik |
|---|---|
| `docs/DURUM.md` | "Nerede kaldık" — aktif faz, açık kararlar, sonraki adım |
| `docs/ROADMAP.md` | Faz tablosu ve karar günlüğü |
| `docs/AKIS-PLANI.md` | Uygulama akışı ve veritabanı yönetimi |
| `docs/WINUI-MIMARISI.md` | Pencere/uygulama mimarisi |
| `docs/TASARIM-KURALLARI.md` | Katmanlı sayfa yapısı ve Fluent tasarım kuralları |
| `docs/LOG.md` | Oturum günlüğü indeksi |
| `docs/HATALAR.md` | Hata ve ders kayıtları |
| `docs/REFERANSLAR.md` | Araştırma kaynakları ve uygulandığı yerler |

## Notlar

- Bu proje bir **ön muhasebe** yazılımıdır; veri yereldir ve yerel AI asistanı veriyi makineden çıkarmaz.
- Geliştirme akışı doküman-öncelikli yürütülür: her oturum `docs/DURUM.md` ile başlar ve güncellenir.
