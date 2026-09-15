using MuhasibPro.Business.DTOModel.DevModel;

namespace MuhasibPro.Business.Contracts.SistemServices.DevServices
{
    /// <summary>
    /// Modül entegrasyon testleri (donanım POST): her modülün DI'da kayıtlı ve çalışır olduğunu doğrular.
    /// Yalnız geliştirme (dev-mode) panelinden çağrılır.
    /// </summary>
    public interface IModulTestCalistirici
    {
        Task<IReadOnlyList<ModulTestSonucu>> CalistirAsync();
    }
}
