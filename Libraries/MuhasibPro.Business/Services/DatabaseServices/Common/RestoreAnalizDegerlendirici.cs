using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities;

namespace MuhasibPro.Business.Services.DatabaseServices.Common
{
    /// <summary>
    /// Faz 6.78 Adım 1/2: ortak restore hüküm çekirdeği (saf fonksiyon, tenant+Sistem paylaşır).
    /// Hüküm türü mevcut <see cref="RestoreVerdictKind"/> reuse edilir — ikinci zarf açılmaz (Kural 3/4).
    /// Sıra: dosya-bütünlüğü → kimliksiz → kimlik-uyuşmazlığı → sürüm → kayıp-kayıt → kurulum/makine.
    /// (Tenant hattı sırası korunur: kimlik kıyası sürümden önce hükmeder.)
    /// </summary>
    public static class RestoreAnalizDegerlendirici
    {
        public static RestoreVerdict Degerlendir(RestoreAnalizGirdisi girdi)
        {
            if (girdi == null || !girdi.Dosya.DosyaVarMi)
                return new RestoreVerdict { Kind = RestoreVerdictKind.Block, Baslik = "Yedek dosyası yok", Aciklama = "Analiz edilecek yedek dosyası bulunamadı.", KodBlokeNedeni = "Dosya yok" };

            if (!girdi.Dosya.IntegrityTamamMi)
                return new RestoreVerdict
                {
                    Kind = RestoreVerdictKind.Block,
                    Baslik = "Yedek dosya bozuk",
                    Aciklama = $"Yedek dosyası bütünlük kontrolünden geçemedi ({girdi.Dosya.IntegrityMesaji}). Bozuk yedekten geri yükleme yapılamaz.",
                    KodBlokeNedeni = "integrity_check başarısız"
                };

            if (!girdi.KimlikliMi)
                return new RestoreVerdict
                {
                    Kind = RestoreVerdictKind.Warning,
                    IsKimliksiz = true,
                    Baslik = "Kimliksiz (eski) yedek",
                    Aciklama = "Bu yedek kimlik damgası içermiyor (eski format). Kayıp-kayıt taraması yapılamadı — geri yükleme sonrası durum kontrolü önerilir."
                };

            if (girdi.KimlikUyusmazMi)
                return new RestoreVerdict
                {
                    Kind = RestoreVerdictKind.RequireCode,
                    Baslik = girdi.KimlikUyusmazlikBaslik!,
                    Aciklama = girdi.KimlikUyusmazlikAciklama ?? string.Empty
                };

            if (!string.IsNullOrWhiteSpace(girdi.YedekVersion) && !string.IsNullOrWhiteSpace(girdi.MevcutVersion))
            {
                if (SemanticVersion.IsGreater(girdi.YedekVersion!, girdi.MevcutVersion!))
                    return new RestoreVerdict
                    {
                        Kind = RestoreVerdictKind.Block,
                        Baslik = "Sürüm uyumsuz — güncelleme gerekli",
                        Aciklama = $"Yedek sürümü ({girdi.YedekVersion}) mevcutten ({girdi.MevcutVersion}) daha yeni. Geri yükleme engellendi — önce uygulamayı güncelleyin.",
                        KodBlokeNedeni = "Yedek daha yeni sürümden"
                    };
                if (SemanticVersion.IsLess(girdi.YedekVersion!, girdi.MevcutVersion!))
                    return new RestoreVerdict
                    {
                        Kind = RestoreVerdictKind.Warning,
                        Baslik = "Eski sürüm yedek",
                        Aciklama = $"Yedek sürümü ({girdi.YedekVersion}) mevcutten ({girdi.MevcutVersion}) daha eski. Geri yüklendikten sonra otomatik güncellenecek."
                    };
            }

            if (girdi.Fark.KayipVarMi)
            {
                string ornek = string.Join(", ", girdi.Fark.KayipKayitlar.Concat(girdi.Fark.KayipTenantDosyalari).Take(3));
                return new RestoreVerdict
                {
                    Kind = RestoreVerdictKind.RequireCode,
                    Baslik = "Kayıp kayıt riski",
                    Aciklama = $"Bu yedek geri yüklenirse {girdi.Fark.ToplamKayip} kayıt kaybolur ({ornek}{(girdi.Fark.ToplamKayip > 3 ? ", …" : "")}). Devam etmek için tek-seferlik kod gerekli."
                };
            }

            if (girdi.KurulumFarkli || girdi.MakineFarkli)
            {
                string detay = girdi.KurulumFarkli && girdi.MakineFarkli ? "farklı kurulum ve makineden"
                    : girdi.KurulumFarkli ? "farklı kurulumdan" : "farklı makineden";
                return new RestoreVerdict
                {
                    Kind = RestoreVerdictKind.Warning,
                    Baslik = "Taşınmış yedek",
                    Aciklama = $"Yedek {detay} geliyor. Kurulum/Makine uyuşmazlığı — onay ile devam edebilirsiniz."
                };
            }

            return new RestoreVerdict
            {
                Kind = RestoreVerdictKind.Allow,
                Baslik = "Geri yüklenebilir",
                Aciklama = "Yedek dosyası sağlam, sürüm ve kayıtlar uyumlu. Doğrudan geri yüklenebilir."
            };
        }
    }
}
