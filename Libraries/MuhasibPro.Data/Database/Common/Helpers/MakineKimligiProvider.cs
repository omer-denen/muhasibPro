using Microsoft.Extensions.Logging;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.Database.Common.Helpers;

public class MakineKimligiProvider : IMakineKimligiProvider
{
    private const string FallbackKey = "FallbackMachineId";
    private readonly ILogger<MakineKimligiProvider> _logger;
    private readonly SistemDbContext _sistemDbContext;
    private string? _cachedMachineId;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public MakineKimligiProvider(ILogger<MakineKimligiProvider> logger, SistemDbContext sistemDbContext)
    {
        _logger = logger;
        _sistemDbContext = sistemDbContext;
    }

    public async Task<string> GetMachineIdAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedMachineId))
            return _cachedMachineId;

        await _gate.WaitAsync();
        try
        {
            if (!string.IsNullOrWhiteSpace(_cachedMachineId))
                return _cachedMachineId;

            var fromRegistry = OperatingSystem.IsWindows() ? TryReadMachineGuidFromRegistry() : null;
            if (!string.IsNullOrWhiteSpace(fromRegistry))
            {
                _cachedMachineId = fromRegistry;
                return _cachedMachineId;
            }

            _cachedMachineId = await GetOrCreateFallbackMachineIdAsync();
            return _cachedMachineId;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<string> GetOrCreateFallbackMachineIdAsync()
    {
        try
        {
            var existing = _sistemDbContext.GlobalAyarlar.FirstOrDefault(x => x.Anahtar == FallbackKey);
            if (existing != null && !string.IsNullOrWhiteSpace(existing.Deger))
                return existing.Deger;

            var newId = Guid.NewGuid().ToString("N");
            var entity = new GlobalAyarlar
            {
                Id = DateTime.UtcNow.Ticks,
                Anahtar = FallbackKey,
                Deger = newId,
                Aciklama = "MakineGuid okunamadiginda uretilen kalici yedek kimlik",
                KayitTarihi = DateTime.Now,
                KaydedenId = KullaniciSabitleri.SeedYoneticiId,
                AktifMi = true
            };
            _sistemDbContext.GlobalAyarlar.Add(entity);
            await _sistemDbContext.SaveChangesAsync();
            return newId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallback MachineId olusturulamadi, gecici GUID kullaniliyor");
            return Guid.NewGuid().ToString("N");
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private string? TryReadMachineGuidFromRegistry()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            var value = key?.GetValue("MachineGuid") as string;
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "MachineGuid registry okunamadi");
        }
        return null;
    }
}
