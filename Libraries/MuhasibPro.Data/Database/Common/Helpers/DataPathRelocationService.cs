using Microsoft.Extensions.Logging;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;

namespace MuhasibPro.Data.Database.Common.Helpers
{
    /// <inheritdoc />
    public sealed class DataPathRelocationService : IDataPathRelocationService
    {
        private readonly IApplicationPaths _paths;
        private readonly IEnvironmentDetector _environmentDetector;
        private readonly ILogger<DataPathRelocationService> _logger;

        public DataPathRelocationService(
            IApplicationPaths paths,
            IEnvironmentDetector environmentDetector,
            ILogger<DataPathRelocationService> logger)
        {
            _paths = paths;
            _environmentDetector = environmentDetector;
            _logger = logger;
        }

        public Task<(bool basarili, bool tasindi, string mesaj)> EnsureRelocatedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_environmentDetector.IsDevelopment())
                    return Task.FromResult((true, false, "Geliştirme modu — veri taşıma gerekmez."));

                var eskiKok = _paths.GetLegacyDataRootPath();
                if (string.IsNullOrWhiteSpace(eskiKok))
                    return Task.FromResult((true, false, "Eski veri kökü yok."));

                var yeniDb = Path.Combine(_paths.GetAppDataFolderPath(), DatabaseConstants.DATABASE_FOLDER);
                var eskiDb = Path.Combine(eskiKok, DatabaseConstants.DATABASE_FOLDER);
                var yeniSistem = Path.Combine(yeniDb, DatabaseConstants.SISTEM_DB_NAME);
                var eskiSistem = Path.Combine(eskiDb, DatabaseConstants.SISTEM_DB_NAME);

                if (_paths.IsSqliteDatabaseFileValid(yeniSistem))
                    return Task.FromResult((true, false, "Veri zaten yeni konumda."));

                if (!_paths.IsSqliteDatabaseFileValid(eskiSistem))
                    return Task.FromResult((true, false, "Taşınacak eski veri yok."));

                _logger.LogInformation("Veri taşıma başlıyor: {Eski} -> {Yeni}", eskiDb, yeniDb);

                // Yeni konumda geçerli bir Sistem.db yok — hedefi kaynakla bütün olarak doldur.
                TryDeleteDirectory(yeniDb);

                try
                {
                    Directory.Move(eskiDb, yeniDb);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Directory.Move başarısız — kopyala + doğrula yoluna geçiliyor");
                    CopyDirectory(eskiDb, yeniDb, cancellationToken);
                }

                if (!_paths.IsSqliteDatabaseFileValid(yeniSistem))
                {
                    _logger.LogError("Veri taşıma sonrası Sistem.db doğrulanamadı: {Yeni}", yeniSistem);
                    return Task.FromResult((false, false, "Taşıma sonrası Sistem.db doğrulanamadı — eski veri korundu."));
                }

                TryDeleteDirectory(eskiDb);
                _logger.LogInformation("Veri taşıma tamamlandı: {Yeni}", yeniDb);
                return Task.FromResult((true, true, "Veri yeni konuma taşındı."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veri taşıma hatası");
                return Task.FromResult((false, false, $"Veri taşıma hatası: {ex.Message}"));
            }
        }

        private static void CopyDirectory(string kaynak, string hedef, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(hedef);

            foreach (var dir in Directory.GetDirectories(kaynak, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Directory.CreateDirectory(Path.Combine(hedef, Path.GetRelativePath(kaynak, dir)));
            }

            foreach (var file in Directory.GetFiles(kaynak, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dest = Path.Combine(hedef, Path.GetRelativePath(kaynak, file));
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(file, dest, overwrite: true);
            }
        }

        private void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Klasör silinemedi (yoksayıldı): {Path}", path);
            }
        }
    }
}
