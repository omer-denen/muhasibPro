namespace MuhasibPro.Business.Contracts.UIServices
{
    /// <summary>Splash sonrası hedef — View, kararın karşılığını (View/ViewModel tipi) kendi eşler.</summary>
    public enum SplashTarget
    {
        SetupRequired,
        Login
    }

    /// <summary>DB hazırlık kararı (modelden gelen gerçek veri).</summary>
    public class SplashRouteDecision
    {
        public bool IsDatabaseReady { get; set; }
        public SplashTarget Target => IsDatabaseReady ? SplashTarget.Login : SplashTarget.SetupRequired;
    }

    /// <summary>Taşınmış-veri taraması sonucu — dialog gösterimi View'a aittir (Business dialog bilmez).</summary>
    public class TransferCheckResult
    {
        public string CurrentKurulumId { get; set; } = string.Empty;
        public string CurrentMachineId { get; set; } = string.Empty;
        public IReadOnlyList<string> Mismatches { get; set; } = Array.Empty<string>();
        public bool HasMismatches => Mismatches.Count > 0;
    }

    /// <summary>
    /// M1 splash yönlendirme — View'daki EF/DbContext sızıntısının taşındığı UI'sız servis.
    /// Best-effort: hiçbir metot fırlatmaz, arızada güvenli varsayılan döner.
    /// </summary>
    public interface ISplashRoutingService
    {
        /// <param name="startupDbReady">Startup DatabaseValidation adımının yazdığı tek kaynak; null ise servis DB'ye bakar.</param>
        Task<SplashRouteDecision> DecideRouteAsync(bool? startupDbReady);
        Task<TransferCheckResult> CheckTransferAsync();
    }
}
