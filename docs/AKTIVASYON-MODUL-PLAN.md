# MuhasibPro — Faz 6.95: Aktivasyon & Modül Kilidi (KEY) + İlk Giriş Akışı Planı

> **Faz 6.95.** Uygulama akışı: **Splash → Login (üretici tarafından atanan ilk kullanıcı) → [zorunlu şifre değişikliği] → Modül seçici → seçilen modüller için KEY girişi → onaylı key sonrası Sistem.db'de erişilecek modüllerin güncellenmesi → FirmaShellView.**
> **Durum:** 📋 Plan (Oturum 284, kod yok). Sektör araştırması ✅ (`REFERANSLAR` 284).
> **İlişki:** **6.85** = kullanıcı hakları (RBAC) · **6.95** = modül kilidi/aktivasyon (lisans). İki katman birlikte çalışır (Blackbaud deseni).

## Kullanıcı akışı (hedef)
```
Splash
  └─► Login  (üretici tarafından atanan ilk kullanıcı = seed yönetici)
        └─► [Zorunlu şifre değişikliği]        (üretici şifre atadıysa ilk girişte)
              └─► Modül Seçici                 (hangi modüller kullanılacak)
                    └─► KEY girişi             (seçilen modüller için aktivasyon anahtarı)
                          └─► Sistem.db güncelle (erişilecek modüller kaydedilir)
                                └─► FirmaShellView
                                      └─► (yeni kullanıcı oluşturma | atanan şifre → zorunlu değişiklik)
```

## Sektör bulgusu (iki katmanlı desen — `REFERANSLAR` 284)
1. **Modül kilidi = KEY/aktivasyon** (Sage 50 "New Key", MYOB lisans dosyası, Blackbaud "Unlock Optional Modules", QuickBooks license/product number).
2. **Kullanıcı hakları = RBAC** (Blackbaud: unlock sonrası "Set up System Security" ile kullanıcılara modül hakkı; MYOB/Sage: supervisor kullanıcı yönetimi).
3. **Zorunlu ilk şifre değişikliği** ERP'lerde standart güvenlik pratiği.

## Mevcut altyapı (kod — Oturum 284 taraması)
| Parça | Durum | Ref |
|---|---|---|
| `IModuleLicenseService` / `ModuleLicenseService` | ✅ var — **firma-bazlı** modül seti (`GlobalAyarlar:Firma_{id}_ActiveModules`), varsayılan set (Cari/Fatura/Kasa/Banka/Raporlama), `ConcurrentDictionary` cache | `ModuleLicenseService.cs:29-160` |
| `ModuleType` enum | ✅ 12 modül (Cari…Raporlama) | `Domain/Enum/ModuleType.cs` |
| `Lisans` + `LisansTuru` | ✅ `Deneme/Standart/Profesyonel/Kurumsal`, `LisansAnahtari`, tarihler | `Lisans.cs` |
| `SurumKatalogu` | ⚠️ yalnız AI özelliği (`AiAsistanIcerirMi`) | `SurumKatalogu.cs:10-11` |
| `ModuleLicenseViewModel` | ⚠️ **hardcoded `firmaId=1`**; nav'a bağlı değil | `ModuleLicenseViewModel.cs:66,105,115` |
| Seed yönetici | ✅ `KullaniciSabitleri.SeedYoneticiId` (üretici atadığı ilk kullanıcı) | `KullaniciSabitleri.cs` |
| **Zorunlu şifre değişikliği** | ❌ `Kullanici`'da bayrak yok; ilk giriş akışı yok | — |
| **KEY girişi/doğrulama** | ❌ UI yok; `LisansService.KaydetLisansAsync(tur, anahtar, …)` var (doğrulama algoritması yok) | `LisansService.cs:74-98` |
| Modül kapısı (nav) | ❌ `IsModuleActiveAsync` **0 çağıran** | — |

## Kararlar (kilitli — Oturum 284)
| Konu | Karar |
|---|---|
| İki katman (KEY=modül kilidi + RBAC=kullanıcı hakkı) | ✅ **KEY/aktivasyon = modül kilidi; RBAC (6.85) = kullanıcı hakkı** |
| İlk kullanıcı | ✅ **Üretici atar** (seed yönetici); ilk girişte **zorunlu şifre değişikliği** |
| **Lisans kapsamı** | ✅ **Kurulum-geneli (Sistem.db global)** — tüm firmalar aynı modül seti; akış FirmaShell'den önce |
| **KEY doğrulama** | ✅ **Çevrimdışı (imza/checksum)** — gömülü açık anahtar; internet gerekmez |
| **Deneme (trial)** | ✅ **Anahtarsız süreli deneme (30 gün) + kısıtlı modül seti**; süre bitince KEY istenir |
| Zorunlu şifre bayrağı | ✅ `Kullanici.SifreDegistirmeliMi` — **Kural 8 onaylı** (yeni context'te kodlanabilir) |
| Tenant DB = etkin modül şeması | ✅ Dönem DB'si etkin modüllerle oluşturulur |
| **Ek modül → tenant DB güncelleme** | ✅ **Tüm açık dönemler; erişimde onay + inline göç** (ön-yedek→göç→doğrulama; 6.91-E) |

## Tenant (mali dönem) veritabanı ve modüller (kullanıcı kararı — Oturum 284)
- **Her mali dönem veritabanı, etkin modüllerin şemasını içerecek şekilde oluşturulur.** Yeni dönem açılışında o an **etkin (KEY'li) modül seti**nin tabloları/migration'ları uygulanır.
- **Ek modül istenirse (yeni KEY) → mali dönem veritabanı güncellenir:** ilgili modülün migration'ları mevcut dönem DB'lerine uygulanır. Bu, mevcut göç hattını kullanır: **ön-yedek → göç → doğrulama** (+ 6.91-E "erişimde onay + inline göç"); başarısızsa yedekten geri dönüş.
- **Modül → migration eşlemesi** ürün sabiti olarak tanımlanır (hangi `ModuleType` hangi tenant migration grubunu getirir). Bugün tenant şeması yalnız `TenantDatabaseVersiyonlar` + `AppLogs` (`AppDbContext`); modül tabloları (Cari/Stok/Fatura…) Faz B ile eklenir.
- **Mevcut dönemler (karar):** yeni modül eklendiğinde **tüm açık dönemler** güncellenir; kullanıcı dönemi seçince **onay dialogu + inline göç** (ön-yedek→göç→doğrulama; 6.91-E hattı) çalışır.

## Faz adımları
### A1 — Model/sözleşme (kararlar ✅ — kod ⬜)
- [x] Lisans kapsamı (kurulum-geneli) + KEY doğrulama (çevrimdışı imza/checksum) + deneme (30 gün, kısıtlı set) kararları — ✅ Oturum 284
- [ ] `Kullanici.SifreDegistirmeliMi` + (gerekirse) `Lisans` doğrulama alanları → migration (**Kural 8 ✅ onaylı**)
- [ ] `REFERANSLAR` derinleştirme (imza/checksum deseni — kod öncesi araştırma, Kural 14)

### A2 — Aktivasyon/KEY altyapısı (Data+Business) ⬜
- [ ] `ILisansService`/`IModuleLicenseService` genişletme: `AktivasyonAnahtariDogrula`, `ModulleriEtkinlestir`
- [ ] Anahtar → modül seti eşlemesi (ürün sabiti; kurulum-geneli veya firma)
- [ ] Seed varsayılanları + doğrulama testleri

### A3 — Modül Seçici + KEY ekranı (UI) ⬜
- [ ] Login sonrası **Modül Seçici** + **KEY girişi** ekranı (Kural 17 katmanlı yapı, Kural 11/12 durum/ilerleme)
- [ ] Onaylı key → Sistem.db modül erişimi güncellenir → FirmaShell'e geçiş
- [ ] Deneme/kısıtlı mod seti (varsa)

### A4 — Zorunlu şifre değişikliği akışı ⬜
- [ ] İlk girişte şifre değiştirme ekranı (mevcut şifre doğrulamalı, güç kuralı `IIdentitySettingsProvider`)
- [ ] Yeni kullanıcı oluşturma akışı (admin) + atanan şifreyle ilk girişte zorunlu değişiklik

### A5 — Modül erişim kapısı + tenant şeması ⬜
- [ ] `MainShell` modül menüsü `IModuleLicenseService.IsModuleActiveAsync` ile (yetkisiz modül gizli/pasif + gerekçe)
- [ ] `ModuleLicenseViewModel` hardcoded `firmaId` → aktif firma
- [ ] **Modül → tenant migration eşlemesi** (ürün sabiti)
- [ ] **Yeni dönem oluşturma:** etkin modül setinin şemasıyla
- [ ] **Ek modül (yeni KEY):** mevcut dönem DB'lerine göç (ön-yedek → göç → doğrulama; 6.91-E erişimde onay+inline)

### A6 — Doğrulama + doküman ⬜
- [ ] build 0/0 + testler + Kural 18 canlı (aktivasyon→FirmaShell, zorunlu şifre, modül kapısı)
- [ ] Kural 13 yardım + dokümanlar

## Sıra / ilişki
- **6.85** (RBAC/kullanıcı hakları) ile **6.95** (modül kilidi) birbirini tamamlar; ortak karar: **modül erişimi = lisans seti ∩ kullanıcı izinleri**.
- Önerilen: **6.95 A1/A2** kararları → **6.85 K1** (RBAC temeli) → A3-A5 ile kapılar.

## Riskler
- **KEY doğrulama seçimi** (offline/online) ürün güvenliğini ve dağıtımı etkiler; araştırma + kullanıcı kararı şart.
- Mevcut `ModuleLicenseService` firma-bazlı; kapsam kararı değişirse `GlobalAyarlar` anahtarı ve migration etkilenir.
- **Tenant şeması modül başına büyür:** ek modül = mevcut dönem DB'lerine göç → geri dönüşsüz veri riski; bu yüzden mevcut **ön-yedek → göç → doğrulama** hattı (6.91) zorunlu, doğrudan migration çalıştırılmaz.
- Zorunlu şifre bayrağı migration (Kural 8) — onay olmadan kodlanmaz.
