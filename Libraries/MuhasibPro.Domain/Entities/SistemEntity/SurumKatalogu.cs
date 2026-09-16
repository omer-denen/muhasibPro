namespace MuhasibPro.Domain.Entities.SistemEntity;

/// <summary>
/// Sürüm kataloğu — hangi LisansTuru'nun hangi satılabilir özelliği içerdiği.
/// Ürün sabitidir (satış paketi tanımı), kullanıcı ayarı değildir; Permission/ModuleType emsali.
/// Karar (2026-09-16, kullanıcı): AI Yardım Asistanı Profesyonel + Kurumsal'dadır; Deneme'de açıktır (müşteri denesin).
/// </summary>
public static class SurumKatalogu
{
    public static bool AiAsistanIcerirMi(LisansTuru tur) =>
        tur is LisansTuru.Deneme or LisansTuru.Profesyonel or LisansTuru.Kurumsal;
}
