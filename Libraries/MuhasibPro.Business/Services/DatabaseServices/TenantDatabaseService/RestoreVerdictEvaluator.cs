using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.Common;

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
    /// <summary>
    /// Faz 6.78 Adım 2: tenant hattı ortak restore çekirdeğine bağlandı.
    /// Tenant'a özgü kimlik kıyası (firma/dönem/ad) burada hesaplanır; hüküm ortak çekirdekte verilir.
    /// Davranış korunur: kimliksiz → Warning, kimlik uyuşmazlığı → RequireCode, sürüm/dosya/kurulum ortak.
    /// </summary>
    public static RestoreVerdict Evaluate(
        DatabaseBackupResult yedek,
        MaliDonemModel hedefDonem,
        string currentKurulumId,
        string currentMachineId,
        string? hedefVersion = null)
    {
        if (yedek == null)
            return new RestoreVerdict { Kind = RestoreVerdictKind.Block, Baslik = "Yedek yok", Aciklama = "Geçersiz yedek.", KodBlokeNedeni = "Yedek bulunamadı" };

        var girdi = new RestoreAnalizGirdisi
        {
            Dosya = new RestoreDosyaAnalizi
            {
                DosyaAdi = yedek.BackupFileName ?? yedek.DatabaseName,
                DosyaVarMi = true,
                IntegrityTamamMi = true,
                IntegrityMesaji = "ok"
            },
            Fark = new RestoreFarkOzeti(),
            KimlikliMi = yedek.IsKimlikli,
            YedekVersion = yedek.KimlikVersion,
            MevcutVersion = hedefVersion,
            KurulumFarkli = IdFarkli(yedek.KimlikKurulumId, currentKurulumId),
            MakineFarkli = IdFarkli(yedek.KimlikMakineId, currentMachineId)
        };

        var uyusmazlik = KimlikUyusmazligi(yedek, hedefDonem);
        if (uyusmazlik != null)
        {
            girdi.KimlikUyusmazlikBaslik = uyusmazlik.Value.Baslik;
            girdi.KimlikUyusmazlikAciklama = uyusmazlik.Value.Aciklama;
        }

        return RestoreAnalizDegerlendirici.Degerlendir(girdi);
    }

    private static bool IdFarkli(string? yedekId, string? currentId)
        => !string.IsNullOrWhiteSpace(yedekId) && !string.IsNullOrWhiteSpace(currentId)
           && !string.Equals(yedekId, currentId, StringComparison.OrdinalIgnoreCase);

    // Firma / Dönem / ad uyuşmazlığı → RequireCode (kırmızı kilit, kod)
    private static (string Baslik, string Aciklama)? KimlikUyusmazligi(DatabaseBackupResult yedek, MaliDonemModel hedefDonem)
    {
        if (yedek.KimlikFirmaId != null && hedefDonem.FirmaId != 0 && yedek.KimlikFirmaId != hedefDonem.FirmaId)
            return ("Firma uyuşmazlığı", $"Yedek farklı firmaya ait (FirmaId {yedek.KimlikFirmaId} → hedef {hedefDonem.FirmaId}). Devam etmek için tek-seferlik kod gerekli.");

        if (yedek.KimlikMaliDonemId != null && hedefDonem.Id != 0 && yedek.KimlikMaliDonemId != hedefDonem.Id)
            return ("Mali dönem uyuşmazlığı", $"Yedek farklı döneme ait (Id {yedek.KimlikMaliDonemId} → hedef {hedefDonem.Id}). Devam etmek için tek-seferlik kod gerekli.");

        if (!string.Equals(yedek.DatabaseName, hedefDonem.DatabaseName, StringComparison.OrdinalIgnoreCase))
            return ("Veritabanı adı uyuşmazlığı", $"Yedek '{yedek.DatabaseName}' hedef '{hedefDonem.DatabaseName}' ile eşleşmiyor. Kod gerekli.");

        return null;
    }
}
