using Microsoft.EntityFrameworkCore;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Utilities.Responses;
using System.Collections.Concurrent;

namespace MuhasibPro.Business.Services.SistemServices.AppServices;

public class ModuleLicenseService : IModuleLicenseService
{
    private readonly SistemDbContext _sistemDbContext;
    private readonly ILogService _logService;
    private readonly IAuthenticationService _authenticationService;
    private static readonly ConcurrentDictionary<long, HashSet<ModuleType>> _firmaModulesCache = new();

    public ModuleLicenseService(SistemDbContext sistemDbContext, ILogService logService, IAuthenticationService authenticationService = null!)
    {
        _sistemDbContext = sistemDbContext;
        _logService = logService;
        _authenticationService = authenticationService;
    }

    public async Task<bool> IsModuleActiveAsync(long firmaId, ModuleType moduleType)
    {
        if (_firmaModulesCache.TryGetValue(firmaId, out var cachedSet))
        {
            return cachedSet.Contains(moduleType);
        }

        var activeModules = await LoadFirmaActiveModulesAsync(firmaId);
        _firmaModulesCache[firmaId] = activeModules;
        return activeModules.Contains(moduleType);
    }

    public async Task<ApiDataResponse<List<ModuleInfoDto>>> GetFirmaModulesAsync(long firmaId)
    {
        try
        {
            var activeSet = await LoadFirmaActiveModulesAsync(firmaId);
            _firmaModulesCache[firmaId] = activeSet;

            var allModules = new List<ModuleInfoDto>
            {
                new() { ModuleType = ModuleType.Cari, ModuleName = "Cari Hesap Yönetimi", Description = "Müşteri ve tedarikçi bakiye takibi, borç/alacak risk limitleri ve ekstreler.", IsActive = activeSet.Contains(ModuleType.Cari), IconKey = "People" },
                new() { ModuleType = ModuleType.Stok, ModuleName = "Stok & Depo Yönetimi", Description = "Barkodlu ürün takibi, kritik seviye uyarıları ve depo hareketleri.", IsActive = activeSet.Contains(ModuleType.Stok), IconKey = "Box" },
                new() { ModuleType = ModuleType.Fatura, ModuleName = "Fatura & İrsaliye Modülü", Description = "Alış/Satış faturaları, e-Arşiv/e-Fatura entegrasyonu, KDV ve tevkifat.", IsActive = activeSet.Contains(ModuleType.Fatura), IconKey = "Receipt" },
                new() { ModuleType = ModuleType.Kasa, ModuleName = "Kasa & Nakit Yönetimi", Description = "TL ve döviz kasaları, gün sonu devirleri ve nakit hareketleri.", IsActive = activeSet.Contains(ModuleType.Kasa), IconKey = "Money" },
                new() { ModuleType = ModuleType.Banka, ModuleName = "Banka Hesapları", Description = "Banka hesapları, IBAN takibi, havale, EFT ve POS hareketleri.", IsActive = activeSet.Contains(ModuleType.Banka), IconKey = "Bank" },
                new() { ModuleType = ModuleType.CekSenet, ModuleName = "Çek & Senet Portföyü", Description = "Kıymetli evrak, ciro etme, tahsilat ve karşılıksız çek takibi.", IsActive = activeSet.Contains(ModuleType.CekSenet), IconKey = "Document" },
                new() { ModuleType = ModuleType.Personel, ModuleName = "Personel & Bordro", Description = "Çalışan kartları, SGK/TCKN, net/brüt maaş tahakkuk ve ödemeleri.", IsActive = activeSet.Contains(ModuleType.Personel), IconKey = "Contact" },
                new() { ModuleType = ModuleType.SiparisTeklif, ModuleName = "Sipariş & Teklif Yönetimi", Description = "Fiyat teklifleri, müşteri siparişleri ve faturaya dönüştürme.", IsActive = activeSet.Contains(ModuleType.SiparisTeklif), IconKey = "Shop" },
                new() { ModuleType = ModuleType.Raporlama, ModuleName = "Raporlama & Dashboard", Description = "Finansal özetler, bilanço, gelir tablosu ve analiz grafikleri.", IsActive = activeSet.Contains(ModuleType.Raporlama), IconKey = "Chart" },
            };

            return new SuccessApiDataResponse<List<ModuleInfoDto>>(allModules, "Firma modül listesi başarıyla getirildi.");
        }
        catch (Exception ex)
        {
            await _logService.SistemLogService.SistemLogExceptionAsync(nameof(ModuleLicenseService), nameof(GetFirmaModulesAsync), ex);
            return new ErrorApiDataResponse<List<ModuleInfoDto>>(new List<ModuleInfoDto>(), $"Modüller alınırken hata: {ex.Message}");
        }
    }

    public async Task<ApiDataResponse<bool>> SetModuleStatusAsync(long firmaId, ModuleType moduleType, bool isActive)
    {
        try
        {
            var activeSet = await LoadFirmaActiveModulesAsync(firmaId);
            if (isActive) activeSet.Add(moduleType);
            else activeSet.Remove(moduleType);

            await SaveFirmaActiveModulesAsync(firmaId, activeSet);
            _firmaModulesCache[firmaId] = activeSet;

            return new SuccessApiDataResponse<bool>(true, $"{moduleType} modül durumu güncellendi: {(isActive ? "Aktif" : "Pasif")}");
        }
        catch (Exception ex)
        {
            return new ErrorApiDataResponse<bool>(false, $"Modül durumu güncellenemedi: {ex.Message}");
        }
    }

    public async Task<ApiDataResponse<bool>> UpdateActiveModulesAsync(long firmaId, IEnumerable<ModuleType> activeModules)
    {
        try
        {
            var newSet = new HashSet<ModuleType>(activeModules);
            await SaveFirmaActiveModulesAsync(firmaId, newSet);
            _firmaModulesCache[firmaId] = newSet;

            return new SuccessApiDataResponse<bool>(true, "Firma aktif modülleri başarıyla kaydedildi.");
        }
        catch (Exception ex)
        {
            return new ErrorApiDataResponse<bool>(false, $"Modüller kaydedilemedi: {ex.Message}");
        }
    }

    private async Task<HashSet<ModuleType>> LoadFirmaActiveModulesAsync(long firmaId)
    {
        var setting = await _sistemDbContext.GlobalAyarlar
            .FirstOrDefaultAsync(s => s.Anahtar == $"Firma_{firmaId}_ActiveModules");

        if (setting == null || string.IsNullOrWhiteSpace(setting.Deger))
        {
            // Default active modules: Cari, Fatura, Kasa, Banka, Raporlama
            return new HashSet<ModuleType>
            {
                ModuleType.Cari,
                ModuleType.Fatura,
                ModuleType.Kasa,
                ModuleType.Banka,
                ModuleType.Raporlama
            };
        }

        var parts = setting.Deger.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var set = new HashSet<ModuleType>();
        foreach (var p in parts)
        {
            if (System.Enum.TryParse<ModuleType>(p, out var mod))
            {
                set.Add(mod);
            }
        }
        return set;
    }

    private async Task SaveFirmaActiveModulesAsync(long firmaId, HashSet<ModuleType> modules)
    {
        var key = $"Firma_{firmaId}_ActiveModules";
        var val = string.Join(",", modules.Select(m => m.ToString()));

        var existing = await _sistemDbContext.GlobalAyarlar.FirstOrDefaultAsync(s => s.Anahtar == key);
        if (existing != null)
        {
            existing.Deger = val;
            existing.GuncellemeTarihi = DateTime.Now;
        }
        else
        {
            await _sistemDbContext.GlobalAyarlar.AddAsync(new GlobalAyarlar
            {
                Anahtar = key,
                Deger = val,
                Aciklama = $"Firma {firmaId} için aktif muhasebe modülleri listesi",
                KayitTarihi = DateTime.Now,
                KaydedenId = _authenticationService?.GetCurrentUserId > 0
                    ? _authenticationService.GetCurrentUserId
                    : KullaniciSabitleri.SeedYoneticiId,
                AktifMi = true
            });
        }
        await _sistemDbContext.SaveChangesAsync();
    }
}
