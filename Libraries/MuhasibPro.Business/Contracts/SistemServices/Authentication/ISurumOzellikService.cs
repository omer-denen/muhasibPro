using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Business.Contracts.SistemServices.Authentication
{
    /// <summary>AI asistan sürüm hakkı sonucu (kapı + kilit-UI metni tek çağrıda).</summary>
    public class SurumHakkiDto
    {
        public LisansTuru Tur { get; set; } = LisansTuru.Deneme;
        public bool GecerliMi { get; set; }
        public bool HakVarMi { get; set; }
        public string Gerekce { get; set; } = string.Empty;
    }

    /// <summary>Sürüm→özellik kapısı (Faz 6.92). Lisans türünü okur, SurumKatalogu'na göre hak söyler.
    /// Fail-closed: lisans okunamazsa hak YOK döner. Rol kapısı (Permission) ayrı eksendir, çağırandadır.</summary>
    public interface ISurumOzellikService
    {
        Task<SurumHakkiDto> AiAsistanHakkiAsync();
    }
}
