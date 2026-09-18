namespace MuhasibPro.Business.Contracts.SistemServices.AiAsistan
{
    /// <summary>D2-D3 donanım uygunluk hükmü (Oturum 292): kilit yok, yalnız uyarı bandı verisi.</summary>
    public enum DonanimHukmu
    {
        Uygun,
        Sinirda,
        Yetersiz
    }

    /// <summary>Ölçülen donanım (saf okuma). Ram kullanılabilir bellektir (boş + geri alınabilir).</summary>
    public class DonanimBilgisiDto
    {
        public long KullanilabilirRamMb { get; set; }
        public long BosDiskMb { get; set; }
        public int IslemciCekirdek { get; set; }
    }

    /// <summary>D2: donanım ölçümü + D3 hüküm (saf okuma; fırlatmaz — bilinmeyen -1 döner).</summary>
    public interface IDonanimUygunlukService
    {
        DonanimBilgisiDto Olc();
        DonanimHukmu Degerlendir(DonanimBilgisiDto bilgi);
    }
}
