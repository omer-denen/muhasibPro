using MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Domain.Helpers;
using MuhasibPro.Domain.Utilities;

namespace MuhasibPro.Business.Services.DatabaseServices.UpdateDogrulama
{
    /// <summary>Faz 6.91-D: uygulama dosyası + sürüm doğrulaması (salt-okunur; Velopack işi değil).
    /// Kritik dosya eksikse ya da çalışan sürüm hedeflenenle uyuşmuyorsa güncelleme başarısız sayılır
    /// (Revizyon 3: "Uygulama güncellenemedi" — ön-sürüm/build eki yok sayılır).</summary>
    public class UygulamaDosyaDogrulayici : IUygulamaDosyaDogrulayici
    {
        internal static readonly string[] VarsayilanKritikDosyalar =
        {
            "MuhasibPro.exe",
            "MuhasibPro.dll",
            "MuhasibPro.Data.dll",
            "MuhasibPro.Domain.dll",
            "MuhasibPro.Business.dll",
            "e_sqlite3.dll"
        };

        private readonly string _baseDir;
        private readonly string[] _kritikDosyalar;

        public UygulamaDosyaDogrulayici() : this(AppContext.BaseDirectory, VarsayilanKritikDosyalar) { }

        /// <summary>Test edilebilirlik için dosya kökü/dosya listesi enjekte edilebilir (Kural 6).</summary>
        public UygulamaDosyaDogrulayici(string baseDir, string[] kritikDosyalar)
        {
            _baseDir = baseDir;
            _kritikDosyalar = kritikDosyalar;
        }

        public Task<DogrulamaAdimSonucu> DogrulaAsync(string? beklenenSurum, CancellationToken cancellationToken = default)
        {
            var eksikler = _kritikDosyalar
                .Where(dosya => !File.Exists(Path.Combine(_baseDir, dosya)))
                .ToList();

            if (eksikler.Count > 0)
            {
                return Task.FromResult(DogrulamaAdimSonucu.Block(
                    $"Kritik uygulama dosyaları eksik: {string.Join(", ", eksikler)}. Kurulumu (Setup) tekrarlayın."));
            }

            if (!string.IsNullOrWhiteSpace(beklenenSurum))
            {
                string calisan = ProcessInfoHelper.Version;
                if (!string.IsNullOrWhiteSpace(calisan)
                    && !SemanticVersion.IsEqual(calisan, beklenenSurum))
                {
                    return Task.FromResult(DogrulamaAdimSonucu.Fail(
                        $"Çalışan sürüm ({calisan}) hedeflenen sürümle ({beklenenSurum}) uyuşmuyor."));
                }
            }

            return Task.FromResult(DogrulamaAdimSonucu.Ok("Uygulama dosyaları ve sürümü doğrulandı."));
        }
    }
}
