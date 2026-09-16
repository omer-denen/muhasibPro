namespace MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D: bir doğrulama adımının sonucu (dosya / Sistem.db).
    /// <see cref="Bloklayici"/> true ise uygulama açılışı durur; <see cref="Uyari"/> yalnız bilgilendirir.</summary>
    public class DogrulamaAdimSonucu
    {
        public bool Basarili { get; set; }
        public bool Bloklayici { get; set; }
        public bool Uyari { get; set; }
        public string Mesaj { get; set; } = string.Empty;

        public static DogrulamaAdimSonucu Ok(string mesaj) => new() { Basarili = true, Mesaj = mesaj };

        public static DogrulamaAdimSonucu Warn(string mesaj) => new() { Basarili = true, Uyari = true, Mesaj = mesaj };

        public static DogrulamaAdimSonucu Block(string mesaj) => new() { Basarili = false, Bloklayici = true, Mesaj = mesaj };

        public static DogrulamaAdimSonucu Fail(string mesaj) => new() { Basarili = false, Mesaj = mesaj };
    }
}
