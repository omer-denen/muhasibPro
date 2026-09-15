namespace MuhasibPro.Business.DTOModel.DevModel
{
    /// <summary>
    /// Modül entegrasyon testi sonucu (donanım POST benzeri): modülün DI'da kayıtlı ve çalışır olduğunu gösterir.
    /// </summary>
    public class ModulTestSonucu
    {
        public string Modul { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public bool Basarili { get; set; }
        public string Mesaj { get; set; } = string.Empty;
        public string Detay { get; set; } = string.Empty;
        public string DurumMetni => Basarili ? "Başarılı" : "Başarısız";
    }
}
