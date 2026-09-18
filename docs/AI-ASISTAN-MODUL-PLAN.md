# MuhasibPro — Faz 6.97: AI Yardımcı Modülü Geliştirme Planı

> **Faz 6.97** — Yerel AI yardım asistanının **tek sahipli** olarak incelenmesi, düzeltilmesi ve geliştirilmesi. "Yardımcı model / ana model" ayrımı kapatıldı (Oturum 293); tüm AI işi tek sahipte (ana model). Tarihsel devir kaydı: `docs/Arsiv/YARDIMCI-MODEL-ARSIV.md`.
> **Durum:** 🔨 Açıldı (Oturum 293) — A1 başladı.
> **Kapsam:** motor/veri/model (Foundry sohbet, RAG, prompt/retrieval, embedding, `AsistanBilgi.db`, donanım kapısı) + içerik (`docs/yardim`) + UI/DI + doğrulama.

## Plan revizyonu (dörtlü — Oturum 293)
- **İşlem öncesi plan:** AI işi "yardımcı model (motor/veri/model)" + "ana model (UI/DI/içerik/docs)" katman ayrımıyla yürüyordu (Oturum 288/291).
- **Plan durumu:** Yardımcı-model dönemi tamamlandı (S1/S2/D2/D3); H4 ve D4/D5/DI yarım; kullanıcı H4'ü zayıf buldu.
- **Değişiklik sebebi:** Kullanıcı kararı (Oturum 293) — "yardımcı modelin yaptığı tüm işlemleri devral; görevlerini kendin üstlen". Tek sahip = hız + netlik.
- **Uygulanan plan sonucu:** Yardımcı-model yapısı arşivlendi (`docs/Arsiv/YARDIMCI-MODEL-ARSIV.md`); tüm AI işi **tek sahipte (ana model)** ve bu fazda (A1-A8) toplandı. A1 (prompt + eşik) uygulandı.

## Neden açıldı?
- Yardımcı-model döneminde yapılan AI işleri tek sahipte toplanıp **uçtan uca incelenip düzeltilecek**.
- Kullanıcı tespiti: **H4 (motor/prompt/retrieval) hâlâ zayıf** — cevap kalitesi düşük.
- Yarım kalan işler tek fazda kapatılacak: D2/D3 DI, D4/D5 UI, S3/S4, H4.

## Mevcut durum (Oturum 293 başı, incelendi)
- **S1** ✅ model sabit (`VarsayilanModelAlias`; `Clamp` + `AliasDegisiminiUygulaAsync` kilidi). **S2** ✅ `Temperature=0` + sabit tohum.
- **D2/D3** ✅ servis (`IDonanimUygunlukService`/`DonanimUygunlukService` + testler) — **DI kaydı yok**.
- **H4** 🔨 başladı: prompt sertleştirildi (sadakat + `[n]` atıf + madde kırpma 400) + ilgi eşiği (lexical `<3`, vektör `<0.35`) → boşsa "yardım maddesi yok".
- **Zayıflıklar:** RRF ağırlıkları (0.4/0.6, K=60) içerik 60→119 olduktan sonra ayarlanmadı; değerlendirme seti yok; model `qwen2.5-0.5b` Türkçesi zayıf.

## Görevler

### A1 — Prompt + retrieval eşiği (H4 çekirdek) 🔨
- [x] Prompt sertleştirme: "yalnız maddeleri kullan; ekleme/tahmin yok; ≤3 cümle; `[n]` atıf; yoksa 'Bu konuda yardım maddesi yok.'"
- [x] İlgi eşiği: lexical `<3` ve vektör kosinüs `<0.35` elenir; lexical-only + zayıf → boş döner.
- [x] Madde gövdesi 400 karaktere kırpılır (bağlam bütçesi).
- [x] Testler: `AsistanPromptKurucuTests` (+3: yok-cümlesi, kırpma, `[1]`) → **687/687**.

### A2 — RRF/retrieval ayarı + değerlendirme seti (S4) ⬜
- [ ] RRF ağırlık (lexical/vektör) + K + top-k, 119 maddelik derlemle ölçülerek ayarlanır.
- [ ] Offline değerlendirme seti: "soru → beklenen madde" (~15-20 soru); retrieval beklenen maddeyi ilk N'de getirmeli (xUnit).
- [ ] Regresyon kapısı: prompt/alias/ağırlık değişimi setten geçmeden sabitlenmez.

### A3 — Donanım kapısı DI + panel uyarı bandı (D2/D3 DI + D4) ⬜
- [ ] `IDonanimUygunlukService` DI kaydı (Singleton — saf okuma).
- [ ] Asistan panelinde uyarı bandı (`DonanimUyarsiniGizle`, kilit yok); Sınırda/Yetersiz'de bilgi + yavaş kullanım serbest.
- [ ] İlk model indirmeden önce onay (boyut + "yavaş olabilir").

### A4 — Denetim "Bu bilgisayar" kartı (D5) ⬜
- [ ] Denetim Masası > Yapay Zeka: RAM/disk/çekirdek + hüküm (Uygun/Sınırda/Yetersiz) kartı.
- [ ] `DonanimUyarsiniGizle` ayarı (provider + VM + UI).

### A5 — Öğretme = RAG, prompt sadakati (S3) ⬜
- [ ] İçerik değişiminde retrieval kalitesi korunur; sadakat kuralı (madde dışı cevap yok) canlıda doğrulanır.

### A6 — 6-soru regresyon kapısı (S4) ⬜
- [ ] A2 seti uygulama-içi (dev-mode) "asistan öz-testi" ile birleştirilir; alias/prompt değişimi setten geçmeden varsayılan olamaz.

### A7 — Model tabanı (gerekirse) ⬜
- [ ] S1-S4 yetmezse ayrı araştırma: ≤1.5B adaylar / fine-tune (raf koşulu). Onaysız indirme yok.

### A8 — 6.94 kalan + doğrulama ⬜
- [ ] H5 canlı: kilit senaryosu (lisans/yetki/asistan kapalı → yalnız gerekçe) + her ekrandan projeye özgü cevap + onay.

## Kapılar
- Kural 8 (View/DI sınıf onayı) · Kural 7 (gerçek veri) · Kural 4 (ölü kod) · build 0/0 · test yeşil · Kural 18 canlı + kanıt + onay.
- Sözleşme değişikliği (`IAsistanSohbetService`, `IYardimBilgiTabani`) gerekirse Kural 15 + onay + `YARDIM-DB-PLAN` revizyonu.
