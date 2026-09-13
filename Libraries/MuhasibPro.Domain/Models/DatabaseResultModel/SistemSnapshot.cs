namespace MuhasibPro.Domain.Models.DatabaseResultModel
{
    /// <summary>
    /// Faz 6.78 Adım 3: Sistem.db'nin (mevcut dosya veya yedek dosya) salt-okunur anlık görüntüsü.
    /// Yedek ↔ mevcut farkının (kayıp kayıt + sürüm + kurulum/makine kimliği) tek veri taşıyıcısı.
    /// </summary>
    public class SistemSnapshot
    {
        public bool OkunabildiMi { get; set; }
        public string? SonMigration { get; set; }

        /// <summary>Son migration'ın SemVer karşılığı (<c>DbSchemaVersions</c>).</summary>
        public string? Surum { get; set; }

        public string? KurulumId { get; set; }
        public string? MachineGuid { get; set; }

        public List<SistemMaliDonemSatiri> MaliDonemler { get; set; } = new();
        public List<long> FirmaIdler { get; set; } = new();
        public List<long> KullaniciIdler { get; set; } = new();
    }

    /// <summary>Anlık görüntüdeki tek bir MaliDonem satırı (yalnız fark için gereken alanlar).</summary>
    public class SistemMaliDonemSatiri
    {
        public long Id { get; set; }
        public long FirmaId { get; set; }
        public int MaliYil { get; set; }
        public string? DatabaseName { get; set; }
    }
}
