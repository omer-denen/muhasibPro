namespace MuhasibPro.Business.DTOModel.SistemModel;

public class FirmaModel : ObservableObject
{
    public static FirmaModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };

    public string FirmaKodu { get; set; }
    public string KisaUnvani { get; set; }
    public string TamUnvani { get; set; }
    public string YetkiliKisi { get; set; }
    public string Il { get; set; }
    public string Ilce { get; set; }
    public string Adres { get; set; }
    public string PostaKodu { get; set; }
    public string Telefon1 { get; set; }
    public string Telefon2 { get; set; }
    public string VergiDairesi { get; set; }
    public string VergiNo { get; set; }
    public string TCNo { get; set; }
    public string Web { get; set; }
    public string Eposta { get; set; }
    public byte[] Logo { get; set; }
    public byte[] LogoOnizleme { get; set; }
    public object LogoSource { get; set; }
    public object LogoOnizlemeSource { get; set; }
    public string PBu1 { get; set; } = "TL";
    public string PBu2 { get; set; }


    public ICollection<MaliDonemModel> MaliDonemler { get; set; }

    //Model değişiklikleri
    public bool IsNew => Id <= 0;

    /// <summary>Monogram tile'ındaki numara ("F-0001" → "0001").</summary>
    public string FirmaNo
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FirmaKodu))
                return "-";
            var digits = new string(FirmaKodu.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? FirmaKodu : digits;
        }
    }

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(KisaUnvani))
                return "?";
            var parts = KisaUnvani.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                var s = parts[0].Trim();
                if (s.Length >= 2)
                    return $"{s[0]}{s[1]}".ToUpperInvariant();
                return $"{s[0]}".ToUpperInvariant();
            }
            return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
        }
    }

    public override void Merge(ObservableObject source)
    {
        if (source is FirmaModel model)
            Merge(model);
    }

    public void Merge(FirmaModel source)
    {
        if (source != null)
        {
            Id = source.Id;
            FirmaKodu = source.FirmaKodu;
            KisaUnvani = source.KisaUnvani;
            TamUnvani = source.TamUnvani;
            YetkiliKisi = source.YetkiliKisi;
            Il = source.Il;
            Ilce = source.Ilce;
            Adres = source.Adres;
            PostaKodu = source.PostaKodu;
            Telefon1 = source.Telefon1;
            Telefon2 = source.Telefon2;
            VergiDairesi = source.VergiDairesi;
            VergiNo = source.VergiNo;
            TCNo = source.TCNo;
            Web = source.Web;
            Eposta = source.Eposta;
            Logo = source.Logo;
            LogoSource = source.LogoSource;
            LogoOnizleme = source.LogoOnizleme;
            LogoOnizlemeSource = source.LogoOnizlemeSource;
            PBu1 = source.PBu1;
            PBu2 = source.PBu2;

            AktifMi = source.AktifMi;
            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;

        }
    }

    public override string ToString()
    {
        return IsEmpty ? "----" : KisaUnvani;
    }
}