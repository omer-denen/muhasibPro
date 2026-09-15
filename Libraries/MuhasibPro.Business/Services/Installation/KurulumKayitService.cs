using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;

namespace MuhasibPro.Business.Services.Installation;

public class KurulumKayitService : IKurulumKayitService
{
    private const string KeyKurulumId = "KurulumId";
    private const string KeyKurulumMachineGuid = "KurulumMachineGuid";
    private const string KeyKurulumTarihi = "KurulumTarihi";

    private readonly ILocalSettingsService _localSettings;
    private readonly IGlobalAyarlarService _globalAyarlar;
    private readonly IMakineKimligiProvider _makineProvider;

    public KurulumKayitService(
        ILocalSettingsService localSettings,
        IGlobalAyarlarService globalAyarlar,
        IMakineKimligiProvider makineProvider)
    {
        _localSettings = localSettings;
        _globalAyarlar = globalAyarlar;
        _makineProvider = makineProvider;
    }

    public async Task<KurulumKayitModel?> GetAsync()
    {
        var id = await _localSettings.ReadSettingAsync<string>(KeyKurulumId);
        if (!string.IsNullOrWhiteSpace(id))
        {
            var mg = await _localSettings.ReadSettingAsync<string>(KeyKurulumMachineGuid) ?? string.Empty;
            var dtStr = await _localSettings.ReadSettingAsync<string>(KeyKurulumTarihi);
            DateTime.TryParse(dtStr, out var dt);
            return new KurulumKayitModel { KurulumId = id!, MachineGuid = mg, OlusturmaTarihi = dt };
        }

        var globalId = await _globalAyarlar.GetAsync(KeyKurulumId);
        if (!string.IsNullOrWhiteSpace(globalId))
        {
            var gm = await _globalAyarlar.GetAsync(KeyKurulumMachineGuid) ?? string.Empty;
            var gdtStr = await _globalAyarlar.GetAsync(KeyKurulumTarihi);
            DateTime.TryParse(gdtStr, out var gdt);
            return new KurulumKayitModel { KurulumId = globalId!, MachineGuid = gm, OlusturmaTarihi = gdt };
        }

        return null;
    }

    public async Task<KurulumKayitModel> GetOrCreateAsync()
    {
        var existing = await GetAsync();
        if (existing != null && !string.IsNullOrWhiteSpace(existing.KurulumId))
            return existing;

        var machineGuid = await _makineProvider.GetMachineIdAsync();
        var newId = Guid.NewGuid().ToString("N");
        var now = DateTime.Now;
        var model = new KurulumKayitModel { KurulumId = newId, MachineGuid = machineGuid, OlusturmaTarihi = now };

        await SaveAsync(model);
        return model;
    }

    public async Task EnsureAsync()
    {
        await GetOrCreateAsync();
    }

    /// <summary>Kimlik kaybı onarımı: aynı makinedeki dönem damgaları tek eski kurulum kimliğinde
    /// birleşiyorsa kimlik oradan geri alınır (eski kimlikli yedeklerle uyum korunur).</summary>
    public async Task UpdateKurulumIdAsync(string kurulumId)
    {
        if (string.IsNullOrWhiteSpace(kurulumId))
            return;

        var mevcut = await GetAsync();
        var machineGuid = !string.IsNullOrWhiteSpace(mevcut?.MachineGuid)
            ? mevcut!.MachineGuid
            : await _makineProvider.GetMachineIdAsync();

        var model = new KurulumKayitModel
        {
            KurulumId = kurulumId.Trim(),
            MachineGuid = machineGuid,
            OlusturmaTarihi = mevcut?.OlusturmaTarihi ?? DateTime.Now
        };

        await SaveAsync(model);
    }

    private async Task SaveAsync(KurulumKayitModel model)
    {
        await _localSettings.SaveSettingAsync(KeyKurulumId, model.KurulumId);
        await _localSettings.SaveSettingAsync(KeyKurulumMachineGuid, model.MachineGuid);
        await _localSettings.SaveSettingAsync(KeyKurulumTarihi, model.OlusturmaTarihi.ToString("O"));

        // Global ayna (best-effort)
        try
        {
            await _globalAyarlar.SetAsync(KeyKurulumId, model.KurulumId, "Kurulum kimligi");
            await _globalAyarlar.SetAsync(KeyKurulumMachineGuid, model.MachineGuid, "Kurulum anindaki MachineGuid");
            await _globalAyarlar.SetAsync(KeyKurulumTarihi, model.OlusturmaTarihi.ToString("O"), "Kurulum tarihi");
        }
        catch { /* best-effort */ }
    }
}
