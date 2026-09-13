using Microsoft.Extensions.Logging;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.Common;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Business.Services.DatabaseServices.SistemDatabaseService
{
    /// <summary>
    /// Faz 6.78 Adım 3: Sistem restore tek-kapı analizi. Kural 7: eşik/yol modelden.
    /// Tenant'a özgü kimlik kıyası yoktur; sistem yedeği her zaman bağlamlı sayılır (KimlikliMi=true),
    /// kurulum/makine farkı "taşınmış yedek" onayına düşer. Hüküm ortak çekirdekten gelir (Kural 3/4).
    /// </summary>
    public class SistemRestoreAnalizService : ISistemRestoreAnalizService
    {
        private readonly IDatabaseBackupManager _backupManager;
        private readonly ISistemSnapshotReader _snapshotReader;
        private readonly IApplicationPaths _paths;
        private readonly ILogger<SistemRestoreAnalizService> _logger;

        public SistemRestoreAnalizService(
            IDatabaseBackupManager backupManager,
            ISistemSnapshotReader snapshotReader,
            IApplicationPaths paths,
            ILogger<SistemRestoreAnalizService> logger)
        {
            _backupManager = backupManager;
            _snapshotReader = snapshotReader;
            _paths = paths;
            _logger = logger;
        }

        public async Task<SistemRestoreAnalizSonuc> AnalizEtAsync(string backupFileName)
        {
            var sonuc = new SistemRestoreAnalizSonuc { YedekDosyaAdi = backupFileName ?? string.Empty };
            try
            {
                var backupDir = _paths.GetBackupFolderPath();

                // Katman 1 — yedek dosyası (salt-okunur): integrity + tablo + göç geçmişi
                sonuc.Dosya = await _backupManager.AnalyzeBackupFileAsync(backupDir, backupFileName);

                // Katman 2 — mevcut ↔ yedek farkı (salt-okunur anlık görüntüler)
                var backupPath = Path.Combine(backupDir, backupFileName ?? string.Empty);
                var yedekSnap = await _snapshotReader.ReadAsync(backupPath);
                var mevcutSnap = await _snapshotReader.ReadAsync(_paths.GetSistemDatabaseFilePath());

                if (!yedekSnap.OkunabildiMi)
                {
                    sonuc.Hukum = new RestoreVerdict
                    {
                        Kind = RestoreVerdictKind.Block,
                        Baslik = "Yedek okunamadı",
                        Aciklama = "Yedek dosyası geçerli bir Sistem veritabanı olarak açılamadı.",
                        KodBlokeNedeni = "Snapshot okunamadı"
                    };
                    return sonuc;
                }

                sonuc.Fark = FarkHesapla(mevcutSnap, yedekSnap);

                var girdi = new RestoreAnalizGirdisi
                {
                    Dosya = sonuc.Dosya,
                    Fark = sonuc.Fark,
                    KimlikliMi = true,
                    YedekVersion = yedekSnap.Surum,
                    MevcutVersion = mevcutSnap.Surum,
                    KurulumFarkli = IdFarkli(yedekSnap.KurulumId, mevcutSnap.KurulumId),
                    MakineFarkli = IdFarkli(yedekSnap.MachineGuid, mevcutSnap.MachineGuid)
                };

                sonuc.Hukum = RestoreAnalizDegerlendirici.Degerlendir(girdi);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Sistem restore analizi başarısız: {Yedek}", backupFileName);
                sonuc.Hukum = new RestoreVerdict
                {
                    Kind = RestoreVerdictKind.Block,
                    Baslik = "Analiz yapılamadı",
                    Aciklama = $"Yedek analiz edilemedi: {ex.Message}",
                    KodBlokeNedeni = ex.Message
                };
            }

            return sonuc;
        }

        /// <summary>Yedekten sonra açılan dönem/firma/kullanıcı kayıtları (restore'da kaybolur).</summary>
        private RestoreFarkOzeti FarkHesapla(SistemSnapshot mevcut, SistemSnapshot yedek)
        {
            var fark = new RestoreFarkOzeti();
            var yedekDonemler = yedek.MaliDonemler.Select(d => d.Id).ToHashSet();

            foreach (var donem in mevcut.MaliDonemler.Where(d => !yedekDonemler.Contains(d.Id)))
            {
                fark.KayipKayitlar.Add($"{donem.MaliYil} dönemi ({donem.DatabaseName ?? "dosya adı yok"})");
                if (!string.IsNullOrWhiteSpace(donem.DatabaseName) && _paths.TenantDatabaseFileExists(donem.DatabaseName))
                    fark.KayipTenantDosyalari.Add(donem.DatabaseName!);
            }

            int firmaKayip = mevcut.FirmaIdler.Count(id => !yedek.FirmaIdler.Contains(id));
            if (firmaKayip > 0)
                fark.KayipKayitlar.Add($"{firmaKayip} firma kaydı");

            int kullaniciKayip = mevcut.KullaniciIdler.Count(id => !yedek.KullaniciIdler.Contains(id));
            if (kullaniciKayip > 0)
                fark.KayipKayitlar.Add($"{kullaniciKayip} kullanıcı kaydı");

            return fark;
        }

        private static bool IdFarkli(string? yedekId, string? mevcutId)
            => !string.IsNullOrWhiteSpace(yedekId) && !string.IsNullOrWhiteSpace(mevcutId)
               && !string.Equals(yedekId, mevcutId, StringComparison.OrdinalIgnoreCase);
    }
}
