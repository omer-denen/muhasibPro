# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · `docs/CEKIRDEK-MODUL-PLAN.md` (faz planı).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).
> Dokumentasyon yapısı 2026-09-15 (Oturum 271) sadeleştirildi: **bu proje ana projedir;** eski referans dönemler (AI-Studio/WebToXaml/OOBE-şablonu) arşivde.

---

## Aktif iş
- **Faz 6.86 — Durum çubuğu + `StatusMessageService` + `NotificationService` redesign/refactor** (Hibrit: footer + InfoBar + In-App message; OS toast terk).
  - **Chunk-1 ✅ kod + ✅🧪** (Oturum 271): servis refactor (A1-A3) + `ShellStatusBar` sıfırdan + temizlik. **Onaylandı.**
  - **Oturum 272 ✅ kod + ✅🧪 + ✅ onay:** ana pencereye çubuk (`MainShellView` tam genişlik `ShellStatusBar`); sağ blokta aktif firma + mali dönem + tenant bağlantı noktası (birincil), Sistem.db ikincil; **bağlam yalnız gerçek yüklü tenant (`ConnectedTenantDb.IsLoaded`) varken gösterilir**, **MainShell girişinde `RefreshAktifBaglam`** ile aktarılır ve **host kapısı `BaglamGoster` ile yalnız MainShell'de gösterilir** (Ayarlar/Yeni Firma göstermez); mesajlar pencere başına ayrı (Scoped DI). Kullanıcı onayı: "yapı oturdu, onaylıyorum".
  - **Chunk-2a ✅ kod + ✅🧪 (Oturum 272):** in-app InfoBar hattı — `IInAppMessageService` **Scoped (pencere başına ayrı; yansımaz)**, `InAppMessageHost` (`MainShellView` + `ShellView`; `FirmaShellView`'de yok); `NotificationService` rewrite (**OS toast + `CommunityToolkit.WinUI.Notifications` + AUMID/kısayol hack'i tamamen kaldırıldı**; `NotificationEnabled` bağlı); `App.xaml.cs` `Initialize()` kaldırıldı; csproj paket kaldırıldı; `FirmaShellViewModel.OnTenantUpdateAvailable` (tekrar) kaldırıldı. Canlı kanıt: `ot292f_detay_bildirim.png` (Yeni Firma → Kaydet → ShellView InfoBar). **Kullanıcı onayı bekliyor.**
  - **Chunk-2b 🔨 kısmi (tam test edilmedi):** `NamePasswordControl` (Login) ring `IsActive` bayrağa bağlandı; diğer **4 sabit ring** (`SistemYedekPanel`, `SistemGuncellemePanel`, `DatabaseInfoPanel`, `DonemIslemKartlariPanel`) **test edilemedi** — bu iki view (MaliDonemYönetimView + SistemDbYonetimView) **refactoring istediği için** ertelendi → **Faz 6.88**.
- **Sıra:** **6.86 (aktif) → 6.87** (Veritabanı Güncelleme sayfası komple redesign) **→ 6.88** (MaliDonemYönetimView + SistemDbYonetimView redesign) **→ 6.89** (LoginView "Beni hatırla" + QuickLogin incelenmesi).
- **Rafta:** 6.72 Dalga 1-3, 6.73, 6.74, 6.75 kalan, 6.76, 6.78 Adım 4, 6.85 (Kullanıcı Yönetimi modal), **"Varsayılan firma"** kavramı (tek firma = varsayılan; Oturum 272 kullanıcı isteği). (**6.71 kapandı** — m.3 iptal; m.4 bildirim→6.86, progress→6.88; m.5 RAF panel→6.88.)

## Açık kararlar (kullanıcı bekliyor)
1. **6.86 Chunk-2a onayı** (in-app InfoBar hattı + OS toast/CommunityToolkit kaldırma; canlı kanıt `ot292f_detay_bildirim.png`).
2. **6.87 sınıf planı onayı** (güncelleme sayfası redesign; Kural 14 araştırması `REFERANSLAR` Oturum 269 satırlarında hazır).
3. **Küçük doküman maddeleri:** (a) `docs/VIEW-BAGIMLILIK.md` dursun/arşive? (b) `KONTROL-LISTESI` kapanan yakın fazlar (6.69/6.70/6.79–6.84) arşive alınıp yalnız açık maddeler mi kalsın?
4. **Commit durumu:** Oturum 272 ilk parti commitlendi (`d5310a2`); **Chunk-2a/2b değişiklikleri + revertler commitlenmedi (ağaç kirli).**

## Son oturum (272, 2026-09-15)
Faz 6.86: durum çubuğu **ana pencereye** eklendi (`MainShellView` tam genişlik footer) + sağ blokta **aktif firma · mali dönem · tenant bağlantı noktası** (birincil), Sistem.db ikincil; mesajlar pencere başına ayrı (Scoped DI). `MainShellViewModel.LoadAsync` (base'siz) besleme + yardım maddesi; `IStatusBarService`/`StatusBarService` `IFirmaWithMaliDonemSelectedService` aboneliği. Build 0 hata + test 498/498 + canlı Light/Dark (`Temp/opencode/ot292_*`, `ot292b_*`). Detay: `docs/LOG/LOG-261-280.md` Oturum 272.

## Son önceki oturum (271, 2026-09-15)
Faz 6.86 Chunk-1 tamamlandı: `IStatusMessageService`/`IStatusBarService` ölü üye temizliği; `StatusMessageService` 440→4 dosya (`StatusMesajDurumu`/`MesajOtoGizleme`/`MesajYurutucu`/facade; yetim `StatusAutoHideMs` bağlandı); DB bool bug'ı düzeldi (`ShellViewModel` gerçek durum); `ShellStatusBar` sıfırdan Fluent footer; ölü converter temizliği. **Build 0 hata + test 498/498 + canlı Dark/Light** (`Temp/opencode/ot290b_*`, dialog `ot290e_*`). Doküman yapısı arşivlendi/sadeleştirildi. Detay: `docs/LOG/LOG-261-280.md` Oturum 271.

## Kapılar / komutlar
- **Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo`
- **Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build`
- **Canlı (Kural 18):** Debug exe `MuhasibPro\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\MuhasibPro.exe`; giriş `korkutomer` / `Ok241341`; veri `MuhasibPro\Databases`; UIA betikleri `C:\Users\Code\AppData\Local\Temp\opencode\` (sqlrun yardımcısı dahil).
- **Beklenen test sayısı:** 498/498.

## Bilinen açık uçlar / notlar
- **Mimari not:** Durum çubuğu artık ana pencerede (`MainShellView`) ve detay pencerelerinde (`ShellView`) görünür; `FirmaShellView`'de yok. Ana pencerede modüller ileride `MainShellView`'i değiştirirse çubuk kaybolur — modüller `MainShellView.ContentFrame`'e alınırsa kalıcı olur (Faz B).
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25 kayıt) + `docs/Arsiv/HATALAR-ARSIV.md` (eski).
- **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` (Faz 0–6.67) + `docs/Arsiv/KARAR-LOGU-ARSIV.md` (eski kararlar).
