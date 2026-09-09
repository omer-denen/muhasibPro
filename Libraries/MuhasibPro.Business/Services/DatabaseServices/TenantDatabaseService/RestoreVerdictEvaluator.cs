using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities;
using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;

public enum RestoreVerdictKind
{
    Allow,
    Warning,
    RequireCode,
    Block
}

public class RestoreVerdict
{
    public RestoreVerdictKind Kind { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public bool KodGerekli => Kind == RestoreVerdictKind.RequireCode;
    public bool Engelli => Kind == RestoreVerdictKind.Block;
    public bool IsKimliksiz { get; set; }
    public string? KodBlokeNedeni { get; set; }
}

public static class RestoreVerdictEvaluator
{
    public static RestoreVerdict Evaluate(
        DatabaseBackupResult yedek,
        MaliDonemModel hedefDonem,
        string currentKurulumId,
        string currentMachineId,
        string? hedefVersion = null)
    {
        if (yedek == null)
            return new RestoreVerdict { Kind = RestoreVerdictKind.Block, Baslik = "Yedek yok", Aciklama = "Geçersiz yedek.", KodBlokeNedeni = "Yedek bulunamadı" };

        if (!yedek.IsKimlikli)
        {
            return new RestoreVerdict
            {
                Kind = RestoreVerdictKind.Warning,
                IsKimliksiz = true,
                Baslik = "Kimliksiz (eski) yedek",
                Aciklama = "Bu yedek kimlik damgası içermiyor (eski format). Ad eşleşmesi ile devam edilecek. Geri yükleme sonrası durum kontrolü önerilir."
            };
        }

        // Firma / Dönem uyuşmazlığı → kırmızı kilit (kod)
        if (yedek.KimlikFirmaId != null && hedefDonem.FirmaId != 0 && yedek.KimlikFirmaId != hedefDonem.FirmaId)
        {
            return new RestoreVerdict
            {
                Kind = RestoreVerdictKind.RequireCode,
                Baslik = "Firma uyuşmazlığı",
                Aciklama = $"Yedek farklı firmaya ait (FirmaId {yedek.KimlikFirmaId} → hedef {hedefDonem.FirmaId}). Devam etmek için tek-seferlik kod gerekli."
            };
        }
        if (yedek.KimlikMaliDonemId != null && hedefDonem.Id != 0 && yedek.KimlikMaliDonemId != hedefDonem.Id)
        {
            return new RestoreVerdict
            {
                Kind = RestoreVerdictKind.RequireCode,
                Baslik = "Mali dönem uyuşmazlığı",
                Aciklama = $"Yedek farklı döneme ait (Id {yedek.KimlikMaliDonemId} → hedef {hedefDonem.Id}). Devam etmek için tek-seferlik kod gerekli."
            };
        }
        if (!string.Equals(yedek.DatabaseName, hedefDonem.DatabaseName, StringComparison.OrdinalIgnoreCase))
        {
            // Ad zaten firma/dönem ile örtüşür ama yine de kırmızı
            return new RestoreVerdict
            {
                Kind = RestoreVerdictKind.RequireCode,
                Baslik = "Veritabanı adı uyuşmazlığı",
                Aciklama = $"Yedek '{yedek.DatabaseName}' hedef '{hedefDonem.DatabaseName}' ile eşleşmiyor. Kod gerekli."
            };
        }

        // Versiyon karşılaştırması → standart: eski → Warning+auto-migrate, yeni → Block
        if (!string.IsNullOrWhiteSpace(yedek.KimlikVersion) && !string.IsNullOrWhiteSpace(hedefVersion))
        {
            if (IsVersionGreater(yedek.KimlikVersion!, hedefVersion!))
            {
                return new RestoreVerdict
                {
                    Kind = RestoreVerdictKind.Block,
                    Baslik = "Sürüm uyumsuz — güncelleme gerekli",
                    Aciklama = $"Yedek sürümü ({yedek.KimlikVersion}) hedeften ({hedefVersion}) daha yeni. Geri yükleme engellendi — önce uygulamayı güncelleyin.",
                    KodBlokeNedeni = "Yedek daha yeni sürümden"
                };
            }
            if (IsVersionLess(yedek.KimlikVersion!, hedefVersion!))
            {
                return new RestoreVerdict
                {
                    Kind = RestoreVerdictKind.Warning,
                    Baslik = "Eski sürüm yedek",
                    Aciklama = $"Yedek sürümü ({yedek.KimlikVersion}) hedeften ({hedefVersion}) daha eski. Geri yüklendikten sonra otomatik güncellenecek — onay ile devam edebilirsiniz."
                };
            }
        }

        // Kurulum / makine farkı → amber onay (kod değil, sadece onay)
        bool kurulumFarkli = !string.IsNullOrWhiteSpace(yedek.KimlikKurulumId) && !string.IsNullOrWhiteSpace(currentKurulumId) && !string.Equals(yedek.KimlikKurulumId, currentKurulumId, StringComparison.OrdinalIgnoreCase);
        bool makineFarkli = !string.IsNullOrWhiteSpace(yedek.KimlikMakineId) && !string.IsNullOrWhiteSpace(currentMachineId) && !string.Equals(yedek.KimlikMakineId, currentMachineId, StringComparison.OrdinalIgnoreCase);

        if (kurulumFarkli || makineFarkli)
        {
            string detay = kurulumFarkli && makineFarkli ? "farklı kurulum ve makineden"
                : kurulumFarkli ? "farklı kurulumdan"
                : "farklı makineden";
            return new RestoreVerdict
            {
                Kind = RestoreVerdictKind.Warning,
                Baslik = "Taşınmış yedek",
                Aciklama = $"Yedek {detay} geliyor. Kurulum/Makine uyuşmazlığı — onay ile devam edebilirsiniz."
            };
        }

        // Rol kontrolü — artık enum admin sabit, blok yok; sadece bilgi
        // KimlikRol her zaman Yönetici, farklı admin bile aynı rol olduğu için engel yok.

        return new RestoreVerdict
        {
            Kind = RestoreVerdictKind.Allow,
            Baslik = "Geri yüklenebilir",
            Aciklama = "Yedek kimliği hedef ile uyumlu. Doğrudan geri yüklenebilir."
        };
    }

    private static bool IsVersionGreater(string a, string b) => SemanticVersion.IsGreater(a, b);

    private static bool IsVersionLess(string a, string b) => SemanticVersion.IsLess(a, b);
}
