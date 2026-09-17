namespace MuhasibPro.Domain.Enum;

/// <summary>
/// Aksiyon bazlı yetki tanımları (modül bazlı değil, aksiyon bazlı).
/// Rol → Permission eşlemesi Sistem.db'deki RolPermission tablosunda tutulur.
/// </summary>
public enum Permission
{
    // ── Cari ────────────────────────────────────────────────
    Cari_Goruntule = 100,
    Cari_Ekle = 101,
    Cari_Duzenle = 102,
    Cari_Sil = 103,

    // ── Stok ────────────────────────────────────────────────
    Stok_Goruntule = 200,
    Stok_Ekle = 201,
    Stok_Duzenle = 202,
    Stok_Sil = 203,

    // ── Fiş ─────────────────────────────────────────────────
    Fis_Goruntule = 300,
    Fis_Ekle = 301,
    Fis_Duzenle = 302,
    Fis_Sil = 303,
    Fis_Onayla = 304,
    Fis_Iptal = 305,

    // ── Fatura ──────────────────────────────────────────────
    Fatura_Goruntule = 400,
    Fatura_Ekle = 401,
    Fatura_Duzenle = 402,
    Fatura_Sil = 403,
    Fatura_Onayla = 404,

    // ── İrsaliye ────────────────────────────────────────────
    Irsaliye_Goruntule = 500,
    Irsaliye_Ekle = 501,
    Irsaliye_Duzenle = 502,
    Irsaliye_Sil = 503,

    // ── Kasa ────────────────────────────────────────────────
    Kasa_Goruntule = 600,
    Kasa_Ekle = 601,
    Kasa_Duzenle = 602,
    Kasa_Sil = 603,

    // ── Banka ───────────────────────────────────────────────
    Banka_Goruntule = 700,
    Banka_Ekle = 701,
    Banka_Duzenle = 702,
    Banka_Sil = 703,

    // ── Çek ─────────────────────────────────────────────────
    Cek_Goruntule = 800,
    Cek_Ekle = 801,
    Cek_Duzenle = 802,
    Cek_Sil = 803,

    // ── Senet ───────────────────────────────────────────────
    Senet_Goruntule = 900,
    Senet_Ekle = 901,
    Senet_Duzenle = 902,
    Senet_Sil = 903,

    // ── Personel ────────────────────────────────────────────
    Personel_Goruntule = 1000,
    Personel_Ekle = 1001,
    Personel_Duzenle = 1002,
    Personel_Sil = 1003,

    // ── Sipariş ─────────────────────────────────────────────
    Siparis_Goruntule = 1100,
    Siparis_Ekle = 1101,
    Siparis_Duzenle = 1102,
    Siparis_Sil = 1103,

    // ── Teklif ──────────────────────────────────────────────
    Teklif_Goruntule = 1200,
    Teklif_Ekle = 1201,
    Teklif_Duzenle = 1202,
    Teklif_Sil = 1203,

    // ── Taksit ──────────────────────────────────────────────
    Taksit_Goruntule = 1300,
    Taksit_Ekle = 1301,
    Taksit_Duzenle = 1302,
    Taksit_Sil = 1303,

    // ── Hesap Planı ─────────────────────────────────────────
    HesapPlani_Goruntule = 1400,
    HesapPlani_Ekle = 1401,
    HesapPlani_Duzenle = 1402,
    HesapPlani_Sil = 1403,

    // ── Raporlama ───────────────────────────────────────────
    Rapor_Goruntule = 1500,
    Rapor_DisaAktar = 1501,

    // ── Dönem Sonu ──────────────────────────────────────────
    DonemSonu_Baslat = 1600,
    DonemSonu_Onayla = 1601,

    // ── Sistem Yönetimi ─────────────────────────────────────
    Kullanici_Yonet = 1700,
    Rol_Yonet = 1800,
    Firma_Yonet = 1900,
    MaliDonem_Yonet = 2000,

    // ── Veritabanı Yönetimi ─────────────────────────────────
    Veritabani_Goruntule = 2100,
    Veritabani_YedekAl = 2101,
    Veritabani_GeriYukle = 2102,
    Veritabani_Arsivle = 2103,
    Veritabani_Sil = 2104,
    Veritabani_KaliciSil = 2105,

    // ── Log / Audit ─────────────────────────────────────────
    Log_Goruntule = 2200,
    Log_Sil = 2201,

    // ── AI Yardım Asistanı (Faz 6.92) ──────────────────────────
    AiAsistan_Kullan = 2300
}
