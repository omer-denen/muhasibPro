using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Business.Services.SistemServices.Authentication
{
    public class SurumOzellikService : ISurumOzellikService
    {
        private readonly ILisansService _lisans;

        public SurumOzellikService(ILisansService lisans)
        {
            _lisans = lisans;
        }

        public async Task<SurumHakkiDto> AiAsistanHakkiAsync()
        {
            try
            {
                var sonuc = await _lisans.GetLisansDurumuAsync();
                if (sonuc == null || !sonuc.Success || sonuc.Data == null)
                    return HakYok(LisansTuru.Deneme, false, "Lisans durumu okunamadı.");

                var durum = sonuc.Data;
                if (!durum.GecerliMi)
                    return HakYok(durum.Tur, false, "Geçerli lisans yok.");
                if (!SurumKatalogu.AiAsistanIcerirMi(durum.Tur))
                    return HakYok(durum.Tur, true, "AI Yardım Asistanı Profesyonel ve Kurumsal sürümlerdedir.");

                return new SurumHakkiDto
                {
                    Tur = durum.Tur,
                    GecerliMi = true,
                    HakVarMi = true,
                    Gerekce = string.Empty
                };
            }
            catch
            {
                return HakYok(LisansTuru.Deneme, false, "Lisans denetimi başarısız.");
            }
        }

        private static SurumHakkiDto HakYok(LisansTuru tur, bool gecerliMi, string gerekce) => new()
        {
            Tur = tur,
            GecerliMi = gecerliMi,
            HakVarMi = false,
            Gerekce = gerekce
        };
    }
}
