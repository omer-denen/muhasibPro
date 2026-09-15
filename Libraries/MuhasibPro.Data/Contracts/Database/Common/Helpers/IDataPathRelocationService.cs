namespace MuhasibPro.Data.Contracts.Database.Common.Helpers
{
    /// <summary>
    /// Eski veri kökünden (<c>%LocalAppData%\MuhasibPro</c>) yeni köke (<c>%AppData%\MuhasibPro</c>)
    /// tek seferlik taşıma. Velopack uninstall app kökünü sildiği için veri app kökü dışına alınır (Faz 6.91-A).
    /// </summary>
    public interface IDataPathRelocationService
    {
        /// <summary>
        /// Gerekliyse taşımayı yapar. Kaynak yoksa veya taşıma zaten yapılmışsa <c>tasindi=false</c> döner.
        /// Taşıma başarısızsa eski veri <b>korunur</b> ve <c>basarili=false</c> döner (fail-closed).
        /// </summary>
        Task<(bool basarili, bool tasindi, string mesaj)> EnsureRelocatedAsync(CancellationToken cancellationToken = default);
    }
}
