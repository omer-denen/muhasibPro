using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices
{
    /// <summary>
    /// Faz 6.78 Adım 3: Sistem.db geri-yükleme için tek-kapı analiz servisi.
    /// Yedek dosyasını ve mevcut Sistem.db'yi inceler, ortak restore çekirdeğinden hüküm üretir.
    /// </summary>
    public interface ISistemRestoreAnalizService
    {
        Task<SistemRestoreAnalizSonuc> AnalizEtAsync(string backupFileName);
    }
}
