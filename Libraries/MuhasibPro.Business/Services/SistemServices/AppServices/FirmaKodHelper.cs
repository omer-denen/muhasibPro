using System.Text;
using System.Text.RegularExpressions;

namespace MuhasibPro.Business.Services.SistemServices.AppServices
{
    /// <summary>
    /// Firma kodu desen yardımcısı — saf fonksiyonlar (DB/ayar bilmez).
    /// Desen dili: "X" = rakam, diğer karakterler aynen eşleşir ("F-XXXX" → F-0001 gibi).
    /// </summary>
    public static class FirmaKodHelper
    {
        public static bool IsValid(string? kod, string pattern)
        {
            if (string.IsNullOrWhiteSpace(kod) || string.IsNullOrWhiteSpace(pattern))
                return false;
            try
            {
                return Regex.IsMatch(kod.Trim(), ToRegex(pattern.Trim()), RegexOptions.CultureInvariant);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Desendeki X'leri sıra numarasıyla doldurur (taşan basamak korunur).</summary>
        public static string Generate(string pattern, int siraNo)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                pattern = "F-XXXX";
            pattern = pattern.Trim();
            int adet = 0;
            foreach (var c in pattern)
                if (c == 'X' || c == 'x')
                    adet++;
            if (adet == 0)
                return pattern;
            string rakam = Math.Max(0, siraNo).ToString().PadLeft(adet, '0');
            if (rakam.Length > adet)
                rakam = rakam.Substring(rakam.Length - adet);
            var sb = new StringBuilder(pattern.Length);
            int i = 0;
            foreach (var c in pattern)
            {
                if (c == 'X' || c == 'x')
                    sb.Append(rakam[i++]);
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        internal static string ToRegex(string pattern)
        {
            var sb = new StringBuilder("^");
            foreach (var c in pattern)
            {
                if (c == 'X' || c == 'x')
                    sb.Append(@"\d");
                else
                    sb.Append(Regex.Escape(c.ToString()));
            }
            sb.Append('$');
            return sb.ToString();
        }
    }
}
