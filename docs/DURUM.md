# MuhasibPro — Durum (nerede kaldık)

> **TEK "nerede kaldık" yüzeyi.** Her oturum ÖNCE bunu oku. Uzun log okuması gerekmez.
> Detay yalnız gerektiğinde: `docs/LOG.md` (kompakt indeks) · `docs/LOG/LOG-261-280.md` (güncel cilt) · `docs/KONTROL-LISTESI.md` (açık maddeler) · `docs/CEKIRDEK-MODUL-PLAN.md` (faz planı).
> Eski ciltler `docs/LOG/Arsiv/`; arşivlenen dokümanlar `docs/Arsiv/` (okuma yolunda değil, gerektiğinde grep).
> Dokumentasyon yapısı 2026-09-15 (Oturum 271) sadeleştirildi: **bu proje ana projedir;** eski referans dönemler (AI-Studio/WebToXaml/OOBE-şablonu) arşivde.

---

## Aktif iş
- **Faz 6.86 — Durum çubuğu + `StatusMessageService` + `NotificationService` redesign/refactor** (Hibrit: footer + InfoBar + In-App message; OS toast terk).
  - **Chunk-1 ✅ kod + ✅🧪** (Oturum 271): servis refactor (A1-A3) + `ShellStatusBar` sıfırdan + temizlik. **Kullanıcı onayı bekliyor.**
  - **Chunk-2 (sıradaki):** C In-App message/InfoBar hattı + A4 `NotificationService` rewrite (OS toast + `CommunityToolkit.WinUI.Notifications` + AUMID/StartMenu hack'i kaldırılır, `NotificationEnabled` bağlanır) + D progress (5 sabit `IsActive="True"` ring).
- **Sıra:** **6.86 (aktif) → 6.87** (Veritabanı Güncelleme sayfası komple redesign).
- **Rafta:** 6.72 Dalga 1-3, 6.73, 6.74, 6.75 kalan, 6.76, 6.78 Adım 4, 6.85 (Kullanıcı Yönetimi modal).

## Açık kararlar (kullanıcı bekliyor)
1. **Durum çubuğu ana pencere:** Yeni footer yalnız `ShellView`'de; ana pencere akışı `ShellView` kullanmıyor (`LoginViewModel.EnterApplication` → doğrudan `FirmaShellView`). "Ana pencereye de bağla (ShellView)" mi, "DetailsWindow-only kalsın" mı? (Oturum 271: "şimdilik not düş, sonra karar".)
2. **6.86 Chunk-2 kapsam onayı** (In-App message + OS toast/CommunityToolkit kaldırma + progress).
3. **6.87 sınıf planı onayı** (güncelleme sayfası redesign; Kural 14 araştırması `REFERANSLAR` Oturum 269 satırlarında hazır).
4. **Commit:** `docs` yapılandırması + Oturum 271 kod değişiklikleri henüz commitlenmedi.

## Son oturum (271, 2026-09-15)
Faz 6.86 Chunk-1 tamamlandı: `IStatusMessageService`/`IStatusBarService` ölü üye temizliği; `StatusMessageService` 440→4 dosya (`StatusMesajDurumu`/`MesajOtoGizleme`/`MesajYurutucu`/facade; yetim `StatusAutoHideMs` bağlandı); DB bool bug'ı düzeldi (`ShellViewModel` gerçek durum); `ShellStatusBar` sıfırdan Fluent footer; ölü converter temizliği. **Build 0 hata + test 498/498 + canlı Dark/Light** (`Temp/opencode/ot290b_*`, dialog `ot290e_*`). Doküman yapısı arşivlendi/sadeleştirildi. Detay: `docs/LOG/LOG-261-280.md` Oturum 271.

## Kapılar / komutlar
- **Build:** `dotnet build MuhasibPro.sln -p:Platform=x64 -c Debug --nologo`
- **Test:** `dotnet test Libraries/MuhasibPro.Tests/MuhasibPro.Tests.csproj -c Debug --no-build`
- **Canlı (Kural 18):** Debug exe `MuhasibPro\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\MuhasibPro.exe`; giriş `korkutomer` / `Ok241341`; veri `MuhasibPro\Databases`; UIA betikleri `C:\Users\Code\AppData\Local\Temp\opencode\` (sqlrun yardımcısı dahil).
- **Beklenen test sayısı:** 498/498.

## Bilinen açık uçlar / notlar
- **Mimari not:** Durum çubuğu ana pencerede görünmüyor (yukarıdaki açık karar 1). `HATALAR.md`'de kayıtlı.
- **Açık buglar/geçmiş dersler:** `docs/HATALAR.md` (son 25 kayıt) + `docs/Arsiv/HATALAR-ARSIV.md` (eski).
- **Kapanan faz geçmişi:** `docs/Arsiv/KONTROL-ARSIV.md` (Faz 0–6.67) + `docs/Arsiv/KARAR-LOGU-ARSIV.md` (eski kararlar).
