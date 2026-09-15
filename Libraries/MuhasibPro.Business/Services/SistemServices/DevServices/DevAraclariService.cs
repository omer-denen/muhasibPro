using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.SistemServices.DevServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.DevModel;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Database.Common.Helpers;
using MuhasibPro.Domain.Enum;

namespace MuhasibPro.Business.Services.SistemServices.DevServices;

/// <summary>
/// Geliştirici araçları servisi (yalnız DEBUG görünürlüğünde çağrılır; Faz 6.82).
/// Tek cümle: mevcut kimlik/tenant/log servislerini dev araç aksiyonları için sarmalar ve DEV damgalı loglar.
/// </summary>
public class DevAraclariService : IDevAraclariService
{
    private const string DevKaynak = "DEV";

    private readonly IKurulumKayitService _kurulum;
    private readonly IMakineKimligiProvider _makine;
    private readonly ITenantVersionReader _versionReader;
    private readonly ITenantSQLiteDatabaseService _tenantDb;
    private readonly ISplashRoutingService _splash;
    private readonly ISistemLogService _log;
    private readonly ILogSeviyesiYoneticisi _logSeviyesi;
    private readonly IApplicationPaths _paths;

    public DevAraclariService(
        IKurulumKayitService kurulum,
        IMakineKimligiProvider makine,
        ITenantVersionReader versionReader,
        ITenantSQLiteDatabaseService tenantDb,
        ISplashRoutingService splash,
        ISistemLogService log,
        ILogSeviyesiYoneticisi logSeviyesi,
        IApplicationPaths paths)
    {
        _kurulum = kurulum;
        _makine = makine;
        _versionReader = versionReader;
        _tenantDb = tenantDb;
        _splash = splash;
        _log = log;
        _logSeviyesi = logSeviyesi;
        _paths = paths;
    }

    public async Task<DevAracDurumuModel> DurumOkuAsync()
    {
        var model = new DevAracDurumuModel();
        try
        {
            var kayit = await _kurulum.GetOrCreateAsync();
            model.KurulumId = kayit?.KurulumId ?? string.Empty;
            model.MachineGuid = kayit?.MachineGuid ?? string.Empty;
            try { model.MakineId = await _makine.GetMachineIdAsync(); } catch { /* makine kimliği okunamazsa boş kalır */ }

            model.UygulamaSemVer = DbSchemaVersions.CurrentSchemaVersion;
            model.LogKlasoru = LogDosyaYolu.Klasor;
            try { model.VeriKlasoru = _paths.GetDatabasesFolderPath(); } catch { /* yol okunamazsa boş */ }
            try { model.SistemDbYolu = _paths.GetSistemDatabaseFilePath(); } catch { /* yol okunamazsa boş */ }
            model.AyrintiliLog = _logSeviyesi.AyrintiliAktif;

            var damgalar = await _versionReader.GetStampsAsync();
            foreach (var d in damgalar)
            {
                model.TenantDamgalari.Add(new DevTenantDamgaModel
                {
                    DatabaseName = d.DatabaseName,
                    SemVer = string.IsNullOrWhiteSpace(d.SemVer) ? "-" : d.SemVer!,
                    KurulumId = d.KurulumId ?? string.Empty,
                    MakineId = d.MakineId ?? string.Empty,
                    KurulumEslesiyor = string.IsNullOrWhiteSpace(d.KurulumId)
                        || string.Equals(d.KurulumId, model.KurulumId, StringComparison.OrdinalIgnoreCase),
                    MakineEslesiyor = string.IsNullOrWhiteSpace(d.MakineId)
                        || string.Equals(d.MakineId, model.MakineId, StringComparison.OrdinalIgnoreCase)
                });
            }
        }
        catch (Exception ex)
        {
            await DevLogErrorAsync("durum", ex);
        }
        return model;
    }

    public async Task<DevAracSonucuModel> KimligiOnarAsync()
    {
        try
        {
            var kayit = await _kurulum.GetOrCreateAsync();
            string makineId = string.Empty;
            try { makineId = await _makine.GetMachineIdAsync(); } catch { /* makine yoksa eşleşme aranmaz */ }

            var mismatch = await _versionReader.ScanMismatchesAsync(kayit.KurulumId, makineId);
            var adlar = mismatch
                .Where(m => !m.MakineFarkli && !string.IsNullOrWhiteSpace(m.DatabaseName))
                .Select(m => m.DatabaseName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            int n = adlar.Count == 0 ? 0 : await _tenantDb.ReAlignTenantKurulumIdsAsync(adlar);
            string mesaj = n > 0
                ? $"{n} dönem damgası güncel kurulum kimliğine eşitlendi."
                : "Eşitlenecek dönem damgası bulunamadı (hepsi güncel veya makine farklı).";
            await DevLogInfoAsync("kimlik-onar", mesaj);
            return new DevAracSonucuModel { Basarili = true, Mesaj = mesaj };
        }
        catch (Exception ex)
        {
            await DevLogErrorAsync("kimlik-onar", ex);
            return new DevAracSonucuModel { Basarili = false, Mesaj = ex.Message };
        }
    }

    public async Task<DevAracSonucuModel> KimligiSifirlaAsync()
    {
        try
        {
            string yeniKimlik = Guid.NewGuid().ToString("N");
            await _kurulum.UpdateKurulumIdAsync(yeniKimlik);

            string makineId = string.Empty;
            try { makineId = await _makine.GetMachineIdAsync(); } catch { /* makine yoksa damga yine de yazılır */ }

            var damgalar = await _versionReader.GetStampsAsync();
            var adlar = damgalar
                .Where(d => !string.IsNullOrWhiteSpace(d.DatabaseName))
                .Where(d => string.IsNullOrWhiteSpace(d.MakineId)
                    || string.Equals(d.MakineId, makineId, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.DatabaseName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            int n = adlar.Count == 0 ? 0 : await _tenantDb.ReAlignTenantKurulumIdsAsync(adlar);
            string mesaj = $"Yeni kurulum kimliği üretildi ({DevAracDurumuModel.Kisalt(yeniKimlik)}); {n} dönem yeniden damgalandı.";
            await DevLogInfoAsync("kimlik-sifirla", mesaj);
            return new DevAracSonucuModel { Basarili = true, Mesaj = mesaj };
        }
        catch (Exception ex)
        {
            await DevLogErrorAsync("kimlik-sifirla", ex);
            return new DevAracSonucuModel { Basarili = false, Mesaj = ex.Message };
        }
    }

    public async Task<DevAracSonucuModel> TransferTaramasiAsync()
    {
        try
        {
            var sonuc = await _splash.CheckTransferAsync();
            string mesaj = sonuc.HasMismatches
                ? $"Taşınmış veri: {sonuc.Mismatches.Count} dönem bu kuruluma ait değil (makine/kurulum farklı)."
                : (sonuc.AlignedCount > 0
                    ? $"Fark yok; {sonuc.AlignedCount} dönem kimliği sessizce onarıldı."
                    : "Fark yok, tüm dönemler bu kuruluma ait.");
            await DevLogInfoAsync("transfer-tarama", mesaj);
            return new DevAracSonucuModel { Basarili = true, Mesaj = mesaj };
        }
        catch (Exception ex)
        {
            await DevLogErrorAsync("transfer-tarama", ex);
            return new DevAracSonucuModel { Basarili = false, Mesaj = ex.Message };
        }
    }

    public async Task<DevAracSonucuModel> AyrintiliLogAyarlaAsync(bool acik)
    {
        _logSeviyesi.AyrintiliAyarla(acik);
        string mesaj = acik ? "Ayrıntılı (Debug) log seviyesi açıldı." : "Ayrıntılı log kapatıldı (Information).";
        await DevLogInfoAsync("log-seviyesi", mesaj);
        return new DevAracSonucuModel { Basarili = true, Mesaj = mesaj };
    }

    private async Task DevLogInfoAsync(string action, string message)
    {
        try { await _log.WriteAsync(LogType.Bilgi, DevKaynak, action, message, string.Empty); }
        catch { /* log yazımı sessizce geçilir (aksiyon sonucu etkilenmez) */ }
    }

    private async Task DevLogErrorAsync(string action, Exception ex)
    {
        try { await _log.WriteAsync(LogType.Hata, DevKaynak, action, ex); }
        catch { /* log yazımı sessizce geçilir */ }
    }
}
