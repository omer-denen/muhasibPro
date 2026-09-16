namespace MuhasibPro.Business.Contracts.UIServices
{
    /// <summary>Splash sonrası hedef — View, kararın karşılığını (View/ViewModel tipi) kendi eşler.</summary>
    public enum SplashTarget
    {
        /// <summary>DB yok — ilk kurulum gerekiyor (KurulumSplash otomatik oluşturur).</summary>
        FirstSetup,
        /// <summary>DB var ama migration/güncelleme gerekiyor (SistemDbYonetim ekranına yönlendir).</summary>
        MigrationRequired,
        /// <summary>Uygulama güncellendi — açılışta güncelleme sonrası doğrulama sagası çalışmalı (GuncellemeSonrasiView).</summary>
        PostUpdateVerification,
        /// <summary>DB hazır — doğrudan Login'e geç.</summary>
        Login
    }

    /// <summary>DB hazırlık kararı (modelden gelen gerçek veri).</summary>
    public class SplashRouteDecision
    {
        public bool IsDatabaseReady { get; set; }
        public bool IsDatabaseExists { get; set; }
        public bool HasPendingMigrations { get; set; }
        public int PendingMigrationCount { get; set; }
        /// <summary>Güncelleme sonrası doğrulama gerekli mi (Faz 6.91-D — kalıcı damgadan).</summary>
        public bool PostUpdateGerekli { get; set; }

        public SplashTarget Target
        {
            get
            {
                if (!IsDatabaseExists) return SplashTarget.FirstSetup;
                if (PostUpdateGerekli) return SplashTarget.PostUpdateVerification;
                if (!IsDatabaseReady || HasPendingMigrations) return SplashTarget.MigrationRequired;
                return SplashTarget.Login;
            }
        }

        /// <summary>Karar izi: neden bu hedef seçildi (Adım 0 tanısı — sayfa günlüğüne düşer).</summary>
        public string KararOzeti =>
            $"exists={IsDatabaseExists} ready={IsDatabaseReady} pending={PendingMigrationCount} postUpdate={PostUpdateGerekli} target={Target}";
    }

    /// <summary>Taşınmış-veri taraması sonucu — dialog gösterimi View'a aittir (Business dialog bilmez).
    /// <see cref="Mismatches"/> yalnız gerçek transferi (makine farklı) taşır; makinesi aynı olan
    /// kurulum-kimliği kayıpları <c>CheckTransferAsync</c> içinde sessizce onarılır.</summary>
    public class TransferCheckResult
    {
        public string CurrentKurulumId { get; set; } = string.Empty;
        public string CurrentMachineId { get; set; } = string.Empty;
        public IReadOnlyList<string> Mismatches { get; set; } = Array.Empty<string>();
        public bool HasMismatches => Mismatches.Count > 0;

        /// <summary>Sessizce onarılan (makinesi aynı) dönem sayısı — teşhis/log içindir.</summary>
        public int AlignedCount { get; set; }

        /// <summary>Onarımda dönem damgalarından geri alınan kurulum kimliği; onarım yoksa boş.</summary>
        public string AdoptedKurulumId { get; set; } = string.Empty;
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
