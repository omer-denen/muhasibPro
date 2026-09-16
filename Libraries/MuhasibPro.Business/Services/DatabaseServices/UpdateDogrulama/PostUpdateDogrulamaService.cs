using System.Diagnostics;
using MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.PostUpdateModel;

namespace MuhasibPro.Business.Services.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D (Revizyon 3): güncelleme sonrası doğrulama orkestratörü (Kural 1 — yalnız akış).
    /// Üç adım: uygulama dosyaları → Sistem.db (guard/göç/verify/restore) → dönem taraması.
    /// Sert blok (Sistem.db) damga atmaz → sonraki açılışta tekrar denenir; diğer her durum damgalanır (tek sefer).</summary>
    public class PostUpdateDogrulamaService : IPostUpdateDogrulamaService
    {
        private const string AdUygulama = "Uygulama Dosyaları";
        private const string AdSistemDb = "Sistem Veritabanı";
        private const string AdDonemler = "Mali Dönem Veritabanları";

        private const double UygulamaBaslangic = 5;
        private const double UygulamaBitis = 20;
        private const double SistemBaslangic = 25;
        private const double SistemBitis = 55;
        private const double DonemBaslangic = 60;
        private const double DonemBitis = 95;

        private readonly IUygulamaDosyaDogrulayici _uygulamaDosya;
        private readonly ISistemDbGocDogrulayici _sistemDb;
        private readonly ITenantTaramaDogrulayici _tenantTarama;
        private readonly ILocalSettingsService _localSettings;
        private readonly ISistemLogService _logService;

        public PostUpdateDogrulamaService(
            IUygulamaDosyaDogrulayici uygulamaDosya,
            ISistemDbGocDogrulayici sistemDb,
            ITenantTaramaDogrulayici tenantTarama,
            ILocalSettingsService localSettings,
            ISistemLogService logService)
        {
            _uygulamaDosya = uygulamaDosya;
            _sistemDb = sistemDb;
            _tenantTarama = tenantTarama;
            _localSettings = localSettings;
            _logService = logService;
        }

        /// <summary>Tetik yalnız başarılı ön-yedek sonrası yazılan <see cref="UpdateSettingsModel.PostUpdatePending"/> bayrağıdır
        /// (B2): iptal/başarısız hazırlık bayrağı temizler, böylece yanlış açılış ekranı çıkmaz.</summary>
        public async Task<bool> GerekliMiAsync()
        {
            try
            {
                var ayar = await _localSettings.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey);
                return ayar?.PostUpdatePending == true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PostUpdateDogrulamaSonucu> CalistirAsync(
            IProgress<PostUpdateAdimSonucu>? ilerleme = null,
            CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();
            var sonuc = new PostUpdateDogrulamaSonucu();

            UpdateSettingsModel? ayar = null;
            try { ayar = await _localSettings.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey); }
            catch { /* ayar okunamazsa adımlar yine çalışır */ }

            try
            {
                // (a) Uygulama dosyaları + sürüm
                var uygulama = await AdimCalistirAsync(ilerleme, sonuc, AdUygulama, UygulamaBaslangic,
                    "Uygulama dosyaları doğrulanıyor...", UygulamaBitis,
                    () => _uygulamaDosya.DogrulaAsync(ayar?.LastUpdateToVersion, cancellationToken));
                if (!uygulama.Basarili)
                {
                    KalanlariAtla(ilerleme, sonuc, "Uygulama doğrulanamadığı için atlandı");
                    return await UygulamaBasarisizAsync(sonuc, uygulama.Mesaj, ilerleme, sw);
                }

                // (b/c) Sistem.db — guard + göç + verify (+ gerekirse pre-update yedekten restore)
                var sistem = await AdimCalistirAsync(ilerleme, sonuc, AdSistemDb, SistemBaslangic,
                    "Sistem veritabanı doğrulanıyor...", SistemBitis,
                    () => _sistemDb.DogrulaAsync(ayar?.LastUpdateBackupPath, cancellationToken));
                if (!sistem.Basarili)
                {
                    Atla(ilerleme, sonuc, AdDonemler, DonemBaslangic, "Sistem veritabanı doğrulanamadığı için atlandı");
                    return await SistemBasarisizAsync(sonuc, sistem.Mesaj, ilerleme, sw);
                }

                // (d) Dönem (tenant) taraması
                var donemAdim = AdimBasla(ilerleme, sonuc, AdDonemler, DonemBaslangic, "Dönemler taranıyor...");
                // Alt ilerleme yalnız çubuğu besler (Mesaj boş) — durum metni adım başı/başı mesajlarında kalır;
                // böylece Progress<T>'nin gecikmeli bildirimi terminal durumu/mesajı ezemez (Kural 12).
                var tenantIlerleme = new IlerlemeKopru(p => ilerleme?.Report(
                    new PostUpdateAdimSonucu { Ad = AdDonemler, Durum = PostUpdateAdimDurumu.DevamEdiyor, Mesaj = string.Empty, Yuzde = DonemBaslangic + p * (DonemBitis - DonemBaslangic) / 100 }));
                var tarama = await _tenantTarama.TaraAsync(tenantIlerleme, cancellationToken);
                sonuc.TarananDonemSayisi = tarama.Taranan;
                sonuc.BozukDonemSayisi = tarama.Bozuk;
                sonuc.BekleyenDonemSayisi = tarama.Bekleyen;
                sonuc.GelecekSemaDonemSayisi = tarama.GelecekSema;
                sonuc.RaporSatirlari.AddRange(tarama.RaporSatirlari);

                bool uyariVar = sistem.Uyari || tarama.Taranamadi || tarama.Bozuk > 0 || tarama.GelecekSema > 0;
                AdimBitir(ilerleme, donemAdim,
                    uyariVar ? PostUpdateAdimDurumu.Uyari : PostUpdateAdimDurumu.Basarili,
                    tarama.Mesaj, DonemBitis);

                // (e) Sonuç + doğrulandı damgası
                await BasariliAsync(sonuc, uyariVar, ilerleme, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                KalanlariAtla(ilerleme, sonuc, "Doğrulama hatası nedeniyle atlandı");
                return await SistemBasarisizAsync(sonuc, $"Doğrulama sırasında beklenmeyen hata: {ex.Message}", ilerleme, sw, ex);
            }

            sw.Stop();
            sonuc.SureMs = sw.ElapsedMilliseconds;
            return sonuc;
        }

        private async Task<DogrulamaAdimSonucu> AdimCalistirAsync(
            IProgress<PostUpdateAdimSonucu>? ilerleme,
            PostUpdateDogrulamaSonucu sonuc,
            string ad,
            double baslangicYuzde,
            string baslangicMesaj,
            double bitisYuzde,
            Func<Task<DogrulamaAdimSonucu>> islem)
        {
            var adim = AdimBasla(ilerleme, sonuc, ad, baslangicYuzde, baslangicMesaj);
            var sonucAdim = await islem();
            AdimBitir(ilerleme, adim, AdimDurumu(sonucAdim), sonucAdim.Mesaj, bitisYuzde);
            return sonucAdim;
        }

        private static PostUpdateAdimDurumu AdimDurumu(DogrulamaAdimSonucu s)
        {
            if (s.Bloklayici || !s.Basarili) return PostUpdateAdimDurumu.Hata;
            return s.Uyari ? PostUpdateAdimDurumu.Uyari : PostUpdateAdimDurumu.Basarili;
        }

        private PostUpdateAdimSonucu AdimBasla(
            IProgress<PostUpdateAdimSonucu>? ilerleme, PostUpdateDogrulamaSonucu sonuc,
            string ad, double yuzde, string mesaj)
        {
            var adim = new PostUpdateAdimSonucu { Ad = ad, Durum = PostUpdateAdimDurumu.DevamEdiyor, Mesaj = mesaj, Yuzde = yuzde };
            sonuc.Adimlar.Add(adim);
            ilerleme?.Report(new PostUpdateAdimSonucu { Ad = ad, Durum = PostUpdateAdimDurumu.DevamEdiyor, Mesaj = mesaj, Yuzde = yuzde });
            return adim;
        }

        private static void AdimBitir(
            IProgress<PostUpdateAdimSonucu>? ilerleme, PostUpdateAdimSonucu adim,
            PostUpdateAdimDurumu durum, string mesaj, double yuzde)
        {
            adim.Durum = durum;
            adim.Mesaj = mesaj;
            adim.Yuzde = yuzde;
            ilerleme?.Report(new PostUpdateAdimSonucu { Ad = adim.Ad, Durum = durum, Mesaj = mesaj, Yuzde = yuzde });
        }

        /// <summary>Hiç başlamamış (atlanan) adımı doğrudan terminal "Atlandı" olarak işler.</summary>
        private static void Atla(
            IProgress<PostUpdateAdimSonucu>? ilerleme, PostUpdateDogrulamaSonucu sonuc,
            string ad, double yuzde, string mesaj)
        {
            if (sonuc.Adimlar.Any(a => a.Ad == ad))
                return;
            sonuc.Adimlar.Add(new PostUpdateAdimSonucu { Ad = ad, Durum = PostUpdateAdimDurumu.Atlandi, Mesaj = mesaj, Yuzde = yuzde });
            ilerleme?.Report(new PostUpdateAdimSonucu { Ad = ad, Durum = PostUpdateAdimDurumu.Atlandi, Mesaj = mesaj, Yuzde = yuzde });
        }

        private static void KalanlariAtla(IProgress<PostUpdateAdimSonucu>? ilerleme, PostUpdateDogrulamaSonucu sonuc, string mesaj)
        {
            Atla(ilerleme, sonuc, AdUygulama, UygulamaBaslangic, mesaj);
            Atla(ilerleme, sonuc, AdSistemDb, SistemBaslangic, mesaj);
            Atla(ilerleme, sonuc, AdDonemler, DonemBaslangic, mesaj);
        }

        private async Task<PostUpdateDogrulamaSonucu> UygulamaBasarisizAsync(
            PostUpdateDogrulamaSonucu sonuc, string mesaj,
            IProgress<PostUpdateAdimSonucu>? ilerleme, Stopwatch sw)
        {
            sonuc.Basarili = false;
            sonuc.Bloklayici = false;
            sonuc.SonucTuru = PostUpdateSonucTuru.UygulamaBasarisiz;
            sonuc.Baslik = "Uygulama güncellenemedi";
            sonuc.Ozet = string.IsNullOrWhiteSpace(mesaj) ? "Daha sonra tekrar deneyin." : $"{mesaj} Daha sonra tekrar deneyin.";

            // Damga atılır: ekran tek sefer gösterilir (kullanıcı "Kapat" ile Login'e döner).
            await DamgalaAsync(ilerleme);
            await _logService.SistemLogErrorAsync("Güncelleme Sonrası Doğrulama", "Uygulama doğrulanamadı", sonuc.Ozet);

            sw.Stop();
            sonuc.SureMs = sw.ElapsedMilliseconds;
            return sonuc;
        }

        private async Task<PostUpdateDogrulamaSonucu> SistemBasarisizAsync(
            PostUpdateDogrulamaSonucu sonuc, string mesaj,
            IProgress<PostUpdateAdimSonucu>? ilerleme, Stopwatch sw, Exception? ex = null)
        {
            sonuc.Basarili = false;
            sonuc.Bloklayici = true;
            sonuc.SonucTuru = PostUpdateSonucTuru.SistemBasarisiz;
            sonuc.Baslik = "Sistem veritabanı güncellenemedi";
            sonuc.Ozet = mesaj;

            // Sert blok: damga atılmaz, bayrak temizlenmez → sonraki açılışta tekrar denenir (fail-closed).
            if (ex != null)
                await _logService.SistemLogExceptionAsync("Güncelleme Sonrası Doğrulama", "Bloklayıcı hata", ex);
            else
                await _logService.SistemLogErrorAsync("Güncelleme Sonrası Doğrulama", "Bloklayıcı hata", mesaj);

            sw.Stop();
            sonuc.SureMs = sw.ElapsedMilliseconds;
            return sonuc;
        }

        private async Task BasariliAsync(
            PostUpdateDogrulamaSonucu sonuc, bool uyariVar,
            IProgress<PostUpdateAdimSonucu>? ilerleme, CancellationToken cancellationToken)
        {
            sonuc.Basarili = true;
            sonuc.Bloklayici = false;
            sonuc.SonucTuru = uyariVar ? PostUpdateSonucTuru.Dikkat : PostUpdateSonucTuru.Temiz;
            sonuc.Baslik = "Güncelleme tamamlandı";
            sonuc.Ozet = sonuc.SatirOzeti();

            await DamgalaAsync(ilerleme, cancellationToken);

            if (uyariVar)
                await _logService.SistemLogInformationAsync("Güncelleme Sonrası Doğrulama", "Tamamlandı (dikkat)", sonuc.Ozet, string.Empty);
            else
                await _logService.SistemLogInformationAsync("Güncelleme Sonrası Doğrulama", "Tamamlandı", sonuc.Ozet, string.Empty);
        }

        /// <summary>Başarılı doğrulama damgası: <c>LastUpdateVerifiedAt</c> yazılır, <c>PostUpdatePending</c> bayrağı temizlenir
        /// (best-effort; yazılamazsa sonraki açılışta tekrar denenir).</summary>
        private async Task DamgalaAsync(IProgress<PostUpdateAdimSonucu>? ilerleme = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var ayar = await _localSettings.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey);
                if (ayar == null) return;
                ayar.LastUpdateVerifiedAt = DateTime.Now;
                ayar.PostUpdatePending = false;
                await _localSettings.SaveSettingAsync(UpdateSettingsModel.SettingsKey, ayar);
            }
            catch { /* best-effort: damga atılamazsa sonraki açılışta tekrar denenir */ }
            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>IProgress köprüsü: olayı oluştuğu anda (senkron) iletir — <see cref="Progress{T}"/>'nin
        /// çift-post gecikmesi terminal adım durumunu ezmesin diye (Kural 12 adım izi güvenilirliği).</summary>
        private sealed class IlerlemeKopru : IProgress<double>
        {
            private readonly Action<double> _ileten;
            public IlerlemeKopru(Action<double> ileten) => _ileten = ileten;
            public void Report(double value) => _ileten(value);
        }
    }
}
