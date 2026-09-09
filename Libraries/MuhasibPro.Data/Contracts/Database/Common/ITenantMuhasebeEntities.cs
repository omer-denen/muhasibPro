using Microsoft.EntityFrameworkCore;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Banka;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Cari;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Cek;
using MuhasibPro.Domain.Entities.MuhasebeEntity.DegerlerEntities;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Fatura_Irsaliye;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Kasa;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Personel;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Senet;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Siparis;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Stok;
using MuhasibPro.Domain.Entities.MuhasebeEntity.TaksitOdemeTahsilat;
using MuhasibPro.Domain.Entities.MuhasebeEntity.Teklif;

namespace MuhasibPro.Data.Contracts.Database.Common
{
    /// <summary>
    /// Tenant (dönem) veritabanındaki 69 muhasebe entity'sinin DbSet tanımları.
    /// Faz B "muhasebe modülleri" aşamasına geçildiğinde AppDbContext bu
    /// interface'i implemente eder — böylece entity keşfi çalışması boşa gitmez.
    /// Şimdilik sadece sözleşme (contract) olarak durur; hiçbir sınıf implemente etmez.
    /// </summary>
    public interface ITenantMuhasebeEntities
    {
        // ── Banka ────────────────────────────────────────────────
        DbSet<BankaListesi> BankaListesi { get; set; }
        DbSet<BankaHesaplar> BankaHesaplar { get; set; }
        DbSet<BankaHareket> BankaHareketler { get; set; }

        // ── Cari ─────────────────────────────────────────────────
        DbSet<CariHesap> CariHesaplar { get; set; }
        DbSet<CariHesapDetay> CariHesapDetaylar { get; set; }
        DbSet<CariHareketler> CariHareketler { get; set; }
        DbSet<CariGrup> CariGruplar { get; set; }
        DbSet<CariFaturaBilgi> CariFaturaBilgiler { get; set; }
        DbSet<CariBankaHesap> CariBankaHesaplar { get; set; }
        DbSet<CariBakiyeler> CariBakiyeler { get; set; }

        // ── Çek ──────────────────────────────────────────────────
        DbSet<Cekler> Cekler { get; set; }
        DbSet<CekHesaplari> CekHesaplari { get; set; }
        DbSet<CekOdemeleri> CekOdemeleri { get; set; }
        DbSet<CekTahsilatlari> CekTahsilatlari { get; set; }
        DbSet<CekCirolari> CekCirolari { get; set; }

        // ── Fatura / İrsaliye ────────────────────────────────────
        DbSet<Faturalar> Faturalar { get; set; }
        DbSet<FaturaKalemler> FaturaKalemler { get; set; }
        DbSet<Irsaliyeler> Irsaliyeler { get; set; }
        DbSet<IrsaliyeKalemler> IrsaliyeKalemler { get; set; }

        // ── Kasa ─────────────────────────────────────────────────
        DbSet<Kasalar> Kasalar { get; set; }
        DbSet<KasaHareket> KasaHareket { get; set; }
        DbSet<KasaDurum> KasaDurumlar { get; set; }

        // ── Personel ─────────────────────────────────────────────
        DbSet<Personeller> Personeller { get; set; }
        DbSet<PersonelHareket> PersonelHareketler { get; set; }
        DbSet<PersonelGorev> PersonelGorevler { get; set; }
        DbSet<PersonelBolum> PersonelBolum { get; set; }
        DbSet<PersonelMizan> PersonelMizan { get; set; }
        DbSet<PersonelTopluMaasTahakkuk> PersonelTopluMaasTahakkuk { get; set; }
        DbSet<PersonelTopluOdeme> PersonelTopluOdemeler { get; set; }

        // ── Senet ────────────────────────────────────────────────
        DbSet<Senetler> Senetler { get; set; }
        DbSet<SenetOdemeleri> SenetOdemeleri { get; set; }
        DbSet<SenetTahsilatlari> SenetTahsilatlari { get; set; }
        DbSet<SenetCirolari> SenetCirolari { get; set; }
        DbSet<SenetMahkemeler> SenetMahkemeler { get; set; }

        // ── Sipariş ──────────────────────────────────────────────
        DbSet<Siparisler> Siparisler { get; set; }
        DbSet<SiparisDetay> SiparisDetay { get; set; }
        DbSet<SiparisNotSablonlari> SiparisNotSablonlar { get; set; }

        // ── Stok ─────────────────────────────────────────────────
        DbSet<Stoklar> Stoklar { get; set; }
        DbSet<StokHareketler> StokHareketler { get; set; }
        DbSet<StokGruplar> StokGruplar { get; set; }
        DbSet<StokBirimler> StokBirimler { get; set; }
        DbSet<StokBakiyeler> StokBakiyeler { get; set; }
        DbSet<IstatistikKarZarar> IstatistikKarZarar { get; set; }
        DbSet<BarkodYazdir> BarkodYazdir { get; set; }
        DbSet<Barkod> Barkodlar { get; set; }

        // ── Taksit / Ödeme / Tahsilat ────────────────────────────
        DbSet<Taksitler> Taksitler { get; set; }
        DbSet<TaksitliAlis> TaksitliAlis { get; set; }
        DbSet<TaksitliSatis> TaksitliSatis { get; set; }
        DbSet<TaksitSenet> TaksitSenet { get; set; }
        DbSet<GecikenTaksitler> GecikenTaksitler { get; set; }

        // ── Teklif ───────────────────────────────────────────────
        DbSet<Teklifler> Teklifler { get; set; }
        DbSet<TeklifDetay> TeklifDetaylar { get; set; }
        DbSet<TeklifNotSablonlari> TeklifNotSablonlari { get; set; }

        // ── Değerler / ComboBox ──────────────────────────────────
        DbSet<VarsayilanDegerler> VarsayilanDegerler { get; set; }
        DbSet<TeslimatSablonlar> TeslimatSablonlar { get; set; }
        DbSet<ParaBirimler> ParaBirimler { get; set; }
        DbSet<OdemeTurler> OdemeTurler { get; set; }
        DbSet<Notlar> Notlar { get; set; }
        DbSet<Iller> Iller { get; set; }
        DbSet<HatirlatmaTurler> HatirlatmaTurler { get; set; }
        DbSet<Hatirlatmalar> Hatirlatmalar { get; set; }
        DbSet<BelgeNumara> BelgeNumaralar { get; set; }
        DbSet<Ayarlar> Ayarlar { get; set; }
        DbSet<APP_SayiFormat> APP_SayiFormat { get; set; }
        DbSet<APP_GuidCode> APP_GuidCode { get; set; }
        DbSet<APP_FormPozisyon> APP_FormPozisyon { get; set; }
        DbSet<Ajanda> Ajandalar { get; set; }
    }
}