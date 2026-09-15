# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · `docs/CEKIRDEK-MODUL-PLAN.md` (faz planı).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).
> Dokumentasyon yapısı 2026-09-15 (Oturum 271) sadeleştirildi: **bu proje ana projedir;** eski referans dönemler (AI-Studio/WebToXaml/OOBE-şablonu) arşivde.

---

## Aktif iş
- **Faz 6.86 — Durum çubuğu + `StatusMessageService` + `NotificationService` redesign/refactor** — **KAPANDI (onaylandı, commit `2ff1a6c`).**
  - Chunk-1 ✅ (Oturum 271): `ShellStatusBar` sıfırdan + servis refactor.
  - Ana pencere çubuğu + aktif bağlam ✅ onaylı (Oturum 272): firma/dönem/tenant yalnız yüklü tenant'tan, `BaglamGoster` ile yalnız MainShell'de.
  - **Chunk-2a ✅ onaylı:** in-app InfoBar hattı (`IInAppMessageService` Scoped — pencere başına ayrı), `InAppMessageHost` (MainShellView + ShellView), OS toast/CommunityToolkit/AUMID kaldırıldı, `NotificationEnabled` bağlı.
  - **Chunk-2b:** yalnız Login ringi; kalan 4 sabit ring → **6.88** (view'lar refactoring bekliyor).
- **Sıradaki iş — Faz 6.87: Veritabanı Güncelleme sayfası (`TenantDatabaseUpdateView`) komple redesign** (Oturum 272 planı sunuldu, **Kural 8 onayı bekliyor**).
  - **Envanter (mevcut):** `MuhasibPro/Views/ShellViews/Shell/TenantDatabaseUpdateView.xaml(.cs)` (209 satır, 4 kart alt alta) · `Libraries/MuhasibPro.ViewModels/ViewModels/Shell/Tenant/TenantDatabaseUpdateViewModel.cs` (389 satır) · `TenantDatabaseUpdateCoordinator` · dialog `TenantDatabaseUpdateDialog`.
  - **Mevcut eksikler (altyapı):** `LoadAsync` sırasında yükleme durumu/ring yok (boş→dolu sıçraması); "güncel — işlem gerekmiyor" **kırmızı hata** gösteriyor; adım bazlı **determinate ilerleme yok**; legacy `MuhasibPetrol*` token'ları; VM 389 satır (Kural 1).
  - **Önerilen tasarım:** Katman-2 ana border + header (başlık/bağlam solda, birincil aksiyon sağda, `?`) → **durum hero kartı** (sürüm `1.0→1.1` + göç rozeti + tek hüküm + çalışırken determinate bar) → **iki sütun: sol "İşlem akışı" (adım rozetli stepper) · sağ "Değişiklikler" (headline + tablo değişimleri)** → sonuç InfoBar. Dar ekranda alt alta.
  - **Altyapı:** `IsChecking`+ring; nötr "güncel" durumu; `Yedek ~30 · Göç ~70 · Doğrulama ~100` yüzde; `MuhasibPetrol*` → `{ThemeResource}`; `TenantDatabaseUpdateViewModel` bölme (`TenantUpdateAkisYoneticisi` + facade).
  - **Kural 14/19:** araştırma hazır (`REFERANSLAR` Oturum 269: QB company-file update + Verify/Rebuild, MS progress controls, Fluent stepper; Oturum 272: MS app-settings layout ~1040px/grup). Uygulamadan önce seçilen layout satırı `REFERANSLAR`'a işlenecek.
  - **Kapı:** Kural 8 sınıf onayı → build 0/0 + test + Kural 18 canlı (Yedek→Göç→Doğrulama + hata/oto-geri-alma; `Temp/opencode/sqlrun` kontrollü test dönemi) + onay.

## Çalışma sırası
- **6.87** (Veritabanı Güncelleme redesign — plan hazır, onay bekliyor) → **6.88** (MaliDonemYönetimView + SistemDbYonetimView redesign + RAF panel temizliği + progress) → **6.89** (LoginView "Beni hatırla" + QuickLogin).
- **Rafta:** 6.72 Dalga 1-3, 6.73, 6.74, 6.75 kalan, 6.76, 6.78 Adım 4, 6.85 (Kullanıcı Yönetimi modal), **"Varsayılan firma"** kavramı. (6.71 kapandı.)

## Açık kararlar (kullanıcı bekliyor)
1. **6.87 sınıf planı onayı** (yukarıdaki redesign; onaylanınca uygulanır).
2. **Doküman maddeleri:** (a) `docs/VIEW-BAGIMLILIK.md` dursun/arşive? (b) `KONTROL-LISTESI` kapanan yakın fazlar (6.69/6.70/6.79–6.84) arşive alınsın mı?
3. **Commit durumu:** **temiz** — Oturum 272 işleri commit edildi: `d5310a2` (durum çubuğu) + `2ff1a6c` (Chunk-2a + fazlar).

## Son oturum (272, 2026-09-15)
Faz 6.86 kapandı (onaylı): ana pencere durum çubuğu + aktif firma/dönem/tenant bağlamı (yalnız MainShell, yüklü-tenant kapısı); in-app InfoBar hattı + OS toast/CommunityToolkit/AUMID kaldırma (pencere-başına izolasyon); `FirmaShellViewModel.OnTenantUpdateAvailable` kaldırıldı. Faz 6.71 kapandı (m.3 iptal; m.4→6.86/6.88; m.5→6.88); 6.88 + 6.89 açıldı. 6.87 redesign planı sunuldu. Build 0 hata + test 498/498. Detay: `docs/LOG/LOG-261-280.md` Oturum 272.

## Son önceki oturum (271, 2026-09-15)
Faz 6.86 Chunk-1: `StatusMessageService` 440→4 dosya, `ShellStatusBar` sıfırdan Fluent footer, ölü üye/converter temizliği. Build 0/0 + test 498/498 + canlı Dark/Light. Detay: `docs/LOG/LOG-261-280.md` Oturum 271.

## Kapılar / komutlar
- **Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo`
- **Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build`
- **Canlı (Kural 18):** Debug exe `MuhasibPro\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\MuhasibPro.exe`; giriş `korkutomer` / `Ok241341`; veri `MuhasibPro\Databases`; UIA betikleri `C:\Users\Code\AppData\Local\Temp\opencode\` (sqlrun yardımcısı dahil).
- **Beklenen test sayısı:** 498/498.

## Bilinen açık uçlar / notlar
- **Bildirimler (6.86):** OS toast kaldırıldı; in-app InfoBar yalnız `MainShellView` + `ShellView` host'larında ve **pencere başına ayrı** (`IInAppMessageService` Scoped — yansımaz). `FirmaShellView`'de host yok; `FirmaShellViewModel.OnTenantUpdateAvailable` kaldırıldı (dönem güncelleme sinyali **Mali Dönem listesi** içinde — `MaliDonemListViewModel`).
- **Mimari not:** Durum çubuğu ana pencerede (`MainShellView`) ve detay pencerelerinde (`ShellView`) görünür; `FirmaShellView`'de yok. Ana pencerede modüller ileride `MainShellView`'i değiştirirse çubuk kaybolur — modüller `MainShellView.ContentFrame`'e alınırsa kalıcı olur (Faz B).
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25 kayıt) + `docs/Arsiv/HATALAR-ARSIV.md` (eski).
- **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` (Faz 0–6.67) + `docs/Arsiv/KARAR-LOGU-ARSIV.md` (eski kararlar).
