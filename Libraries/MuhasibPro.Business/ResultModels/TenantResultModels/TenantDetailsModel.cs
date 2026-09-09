using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Business.ResultModels.TenantResultModels
{
    public class TenantDetailsModel
    {
        public long MaliDonemId { get; set; }
        public long FirmaId { get; set; }
        public string FirmaKodu { get; set; }
        public string FirmaKisaUnvan { get; set; }
        public int MaliYil { get; set; }
        public long UserId { get; set; }
        public string DatabaseName { get; set; }

        /// <summary>Dönemin açık / kapalı / arşiv durumu (MaliDonem.Durum).</summary>
        public DonemDurum Durum { get; set; } = DonemDurum.Acik;

        public bool ArsivlendiMi { get; set; } = false;

        /// <summary>Tenant SQLite dosya boyutu (bayt).</summary>
        public long? DosyaBoyutu { get; set; }

        /// <summary>Son yedek tarihi.</summary>
        public DateTime? SonYedekTarihi { get; set; }

        /// <summary>Dönemin arşivlenmiş olup olmadığı (kartta Kapalı/Arşivlenmiş pill).</summary>
        public bool ArsivliMi => Durum == DonemDurum.Arsivlenmis || ArsivlendiMi;
    }
}
