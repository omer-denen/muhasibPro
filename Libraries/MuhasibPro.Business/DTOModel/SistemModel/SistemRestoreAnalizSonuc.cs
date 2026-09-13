using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Business.DTOModel.SistemModel
{
    /// <summary>
    /// Faz 6.78 Adım 3: Sistem.db restore "tek kapı" analiz sonucu.
    /// Katman 1: yedek dosyası analizi (<see cref="Dosya"/>). Katman 2: mevcut↔yedek farkı (<see cref="Fark"/>).
    /// Hüküm ortak çekirdekten (<see cref="RestoreVerdict"/>) gelir; UI yalnız bunu gösterir.
    /// </summary>
    public class SistemRestoreAnalizSonuc
    {
        public string YedekDosyaAdi { get; set; } = string.Empty;

        /// <summary>Yedek dosyası salt-okunur analizi (integrity + tablo sayısı + göç geçmişi + boyut/tarih).</summary>
        public RestoreDosyaAnalizi Dosya { get; set; } = new();

        /// <summary>Yedek geri yüklenirse kaybolacak kayıtlar / karşılıksız tenant dosyaları.</summary>
        public RestoreFarkOzeti Fark { get; set; } = new();

        /// <summary>Ortak çekirdek hükmü: Allow / Warning / RequireCode / Block.</summary>
        public RestoreVerdict Hukum { get; set; } = new();

        /// <summary>Fark özeti tek satır (fark yoksa boş).</summary>
        public string FarkOzeti => Fark.KayipVarMi
            ? $"{Fark.ToplamKayip} kayıt kaybolur"
            : "Kayıp kayıt yok";

        /// <summary>Fark listesi tek metne indirgenmiş (ilk 5, tooltip/panel için).</summary>
        public string FarkDetayi
        {
            get
            {
                var tumu = Fark.KayipKayitlar.Concat(Fark.KayipTenantDosyalari).ToList();
                if (tumu.Count == 0) return string.Empty;
                var ilk = string.Join("\n• ", tumu.Take(5));
                return tumu.Count > 5 ? $"• {ilk}\n… +{tumu.Count - 5} daha" : $"• {ilk}";
            }
        }
    }
}
