using System.Collections;
using System.Runtime.CompilerServices;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;
using FoundryLogLevel = Microsoft.AI.Foundry.Local.LogLevel;

namespace MuhasibPro.Services.AiAsistan;

/// <summary>
/// Faz 6.92: Foundry Local süreç-içi sohbet (Singleton).
/// Akış: Foundry örnekleri (REFERANSLAR Oturum 276) — yönetici → EP indir → katalog → model indir/yükle → streaming.
/// Sessiz catch yok: tüm hatalar anlamlı mesajla yukarı taşınır (VM hata bandında gösterir).
/// İstisna: ModelSilAsync yakalar-sonuç döndürür+loglar (sözleşme gereği, fail-closed).
/// </summary>
public sealed class FoundryAsistanSohbetService : IAsistanSohbetService
{
    private const string UygulamaAdi = "MuhasibPro";

    private readonly IAiAsistanSettingsProvider _ayarlar;
    private readonly IYardimBilgiTabani _bilgiTabani;
    private readonly ILogger<FoundryAsistanSohbetService>? _logger;
    private readonly SemaphoreSlim _kapi = new(1, 1);
    private bool _yoneticiOlustu;
    private bool _hazir;
    private string _hazirAlias = string.Empty;

    public FoundryAsistanSohbetService(
        IAiAsistanSettingsProvider ayarlar,
        IYardimBilgiTabani bilgiTabani,
        ILogger<FoundryAsistanSohbetService>? logger = null)
    {
        _ayarlar = ayarlar;
        _bilgiTabani = bilgiTabani;
        _logger = logger;
    }

    public Task<AsistanDurumDto> DurumuGetirAsync() =>
        Task.FromResult(new AsistanDurumDto
        {
            HazirMi = _hazir,
            Asama = _hazir ? "Hazır" : "Hazır değil",
            IlerlemeYuzde = _hazir ? 100 : 0,
            Mesaj = _hazir ? _hazirAlias : string.Empty
        });

    public async Task HazirlaAsync(IProgress<AsistanDurumDto>? ilerleme = null, CancellationToken ct = default)
    {
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var ayar = await _ayarlar.GetAsync().ConfigureAwait(false);
            await HazirlaIcAsync(ayar.GetModelAlias(), ilerleme, ct).ConfigureAwait(false);
        }
        finally
        {
            _kapi.Release();
        }
    }

    /// <summary>Kapı çağırandadır (HazirlaAsync/AliasDegisiminiUygulaAsync); alias çözülmüş gelir.</summary>
    private async Task HazirlaIcAsync(string alias, IProgress<AsistanDurumDto>? ilerleme, CancellationToken ct)
    {
        if (_hazir && _hazirAlias == alias)
        {
            Bildir(ilerleme, true, "Hazır", 100, alias);
            return;
        }
        if (_hazir)
            await ModeliKapatIcAsync(_hazirAlias).ConfigureAwait(false);

        ct.ThrowIfCancellationRequested();
        await YoneticiyiKurAsync().ConfigureAwait(false);

        var yonetici = FoundryLocalManager.Instance;
        Bildir(ilerleme, false, "Yürütücü hazırlanıyor", 2, alias);
        var eps = yonetici.DiscoverEps();
        if (eps is { Length: > 0 })
        {
            await yonetici.DownloadAndRegisterEpsAsync((ep, yuzde) =>
                Bildir(ilerleme, false, "Yürütücü indiriliyor", 2 + yuzde * 0.38, $"{alias} • {ep}"))
                .ConfigureAwait(false);
        }

        ct.ThrowIfCancellationRequested();
        Bildir(ilerleme, false, "Model listesi alınıyor", 42, alias);
        var katalog = await yonetici.GetCatalogAsync().ConfigureAwait(false);
        var model = await katalog.GetModelAsync(alias).ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Model katalogda bulunamadı: '{alias}'. Ayarlardan model adını kontrol edin.");

        Bildir(ilerleme, false, "Model indiriliyor", 45, alias);
        await model.DownloadAsync(yuzde =>
            Bildir(ilerleme, false, "Model indiriliyor", 45 + yuzde * 0.45, alias))
            .ConfigureAwait(false);

        ct.ThrowIfCancellationRequested();
        Bildir(ilerleme, false, "Model yükleniyor", 92, alias);
        await model.LoadAsync().ConfigureAwait(false);

        _hazirAlias = alias;
        _hazir = true;
        Bildir(ilerleme, true, "Hazır", 100, alias);
    }

    /// <summary>Foundry yöneticisini bir kez kurar (kapı çağırandadır).</summary>
    private async Task YoneticiyiKurAsync()
    {
        if (_yoneticiOlustu)
            return;
        await FoundryLocalManager.CreateAsync(
            new Configuration { AppName = UygulamaAdi, LogLevel = FoundryLogLevel.Information },
            NullLogger.Instance).ConfigureAwait(false);
        _yoneticiOlustu = true;
    }

    public async IAsyncEnumerable<string> SorStreamingAsync(
        AsistanSoruDto soru, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (soru is null)
            throw new ArgumentNullException(nameof(soru));
        if (string.IsNullOrWhiteSpace(soru.Soru))
            throw new ArgumentException("Soru boş olamaz.", nameof(soru));
        if (!_hazir)
            throw new InvalidOperationException("Asistan hazır değil. Önce HazirlaAsync çağırın.");

        var ayar = await _ayarlar.GetAsync().ConfigureAwait(false);
        var alias = ayar.GetModelAlias();
        if (alias != _hazirAlias)
            throw new InvalidOperationException("Model ayarı değişmiş. Önce HazirlaAsync ile yeniden hazırlayın.");

        var sonuclar = await _bilgiTabani.AraAsync(soru.Soru, ayar.GetEnFazlaMadde(), ct).ConfigureAwait(false);
        IReadOnlyList<AsistanPromptKurucu.SohbetMesaji> mesajlar =
            AsistanPromptKurucu.AramaSonuclariylaKur(sonuclar, soru, ayar.GetMaksGecmisTur());

        var katalog = await FoundryLocalManager.Instance.GetCatalogAsync().ConfigureAwait(false);
        var model = await katalog.GetModelAsync(alias).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Model katalogda bulunamadı: '{alias}'.");
        var istemci = await model.GetChatClientAsync().ConfigureAwait(false);
        var istek = mesajlar.Select(m => new ChatMessage { Role = m.Rol, Content = m.Icerik }).ToList();

        using var zamanAsimi = new CancellationTokenSource(TimeSpan.FromSeconds(ayar.GetSoruZamanAsimiSn()));
        using var bagli = CancellationTokenSource.CreateLinkedTokenSource(ct, zamanAsimi.Token);
        await foreach (var parca in istemci.CompleteChatStreamingAsync(istek, bagli.Token).ConfigureAwait(false))
        {
            string? metin = null;
            if (parca?.Choices is { Count: > 0 } secimler)
                metin = secimler[0]?.Message?.Content;
            if (!string.IsNullOrEmpty(metin))
                yield return metin;
        }
    }

    public async Task<IReadOnlyList<AsistanModelDto>> ModelleriGetirAsync(CancellationToken ct = default)
    {
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await YoneticiyiKurAsync().ConfigureAwait(false);
            var katalog = await FoundryLocalManager.Instance.GetCatalogAsync(ct).ConfigureAwait(false);
            var yukluler = await katalog.GetLoadedModelsAsync(ct).ConfigureAwait(false);
            var yukluAliaslar = yukluler.Select(m => m.Alias ?? string.Empty).ToList();
            var liste = await katalog.ListModelsAsync(ct).ConfigureAwait(false);
            var sonuc = new List<AsistanModelDto>(liste.Count);
            foreach (var m in liste)
            {
                ct.ThrowIfCancellationRequested();
                var alias = m.Alias ?? string.Empty;
                bool indirildi = await m.IsCachedAsync(ct).ConfigureAwait(false);
                long? boyut = null;
                if (indirildi)
                {
                    var yol = await m.GetPathAsync(ct).ConfigureAwait(false);
                    boyut = ModelKlasorOlcer.BaytToplaminiHesapla(yol);
                }
                var gosterim = m.Info?.DisplayName;
                sonuc.Add(new AsistanModelDto
                {
                    Alias = alias,
                    GosterimAdi = string.IsNullOrWhiteSpace(gosterim) ? alias : gosterim,
                    IndirildiMi = indirildi,
                    YukluMu = yukluAliaslar.Contains(alias, StringComparer.OrdinalIgnoreCase),
                    BoyutBayt = boyut
                });
            }
            return sonuc;
        }
        finally
        {
            _kapi.Release();
        }
    }

    public async Task<AsistanDiskKullanimiDto> DiskKullanimiAsync(CancellationToken ct = default)
    {
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await YoneticiyiKurAsync().ConfigureAwait(false);
            var katalog = await FoundryLocalManager.Instance.GetCatalogAsync(ct).ConfigureAwait(false);
            var indirilenler = await katalog.GetCachedModelsAsync(ct).ConfigureAwait(false);
            long toplam = 0;
            foreach (var m in indirilenler)
            {
                ct.ThrowIfCancellationRequested();
                var yol = await m.GetPathAsync(ct).ConfigureAwait(false);
                toplam += ModelKlasorOlcer.BaytToplaminiHesapla(yol);
            }
            return new AsistanDiskKullanimiDto { ToplamBayt = toplam, ModelSayisi = indirilenler.Count };
        }
        finally
        {
            _kapi.Release();
        }
    }

    public async Task<AsistanIslemSonucuDto> ModelSilAsync(string alias, CancellationToken ct = default)
    {
        var hedef = (alias ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(hedef))
            return new AsistanIslemSonucuDto { BasariliMi = false, Mesaj = "Model adı boş olamaz." };
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await YoneticiyiKurAsync().ConfigureAwait(false);
            var katalog = await FoundryLocalManager.Instance.GetCatalogAsync(ct).ConfigureAwait(false);
            var model = await katalog.GetModelAsync(hedef, ct).ConfigureAwait(false);
            if (model is null)
                return new AsistanIslemSonucuDto { BasariliMi = false, Mesaj = $"Model katalogda bulunamadı: '{hedef}'." };
            if (!await model.IsCachedAsync(ct).ConfigureAwait(false))
                return new AsistanIslemSonucuDto { BasariliMi = false, Mesaj = $"Model indirilmemiş: '{hedef}'." };
            if (_hazir && string.Equals(_hazirAlias, hedef, StringComparison.OrdinalIgnoreCase))
            {
                await ModeliKapatIcAsync(_hazirAlias).ConfigureAwait(false);
                _hazir = false;
                _hazirAlias = string.Empty;
            }
            await model.RemoveFromCacheAsync(ct).ConfigureAwait(false);
            _logger?.LogInformation("Model önbellekten silindi: {Alias}", hedef);
            return new AsistanIslemSonucuDto { BasariliMi = true, Mesaj = $"Model silindi: '{hedef}'." };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Model silinemedi: {Alias}", hedef);
            return new AsistanIslemSonucuDto { BasariliMi = false, Mesaj = ex.Message };
        }
        finally
        {
            _kapi.Release();
        }
    }

    public async Task AliasDegisiminiUygulaAsync(string yeniAlias, IProgress<AsistanDurumDto>? ilerleme = null, CancellationToken ct = default)
    {
        var hedef = (yeniAlias ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(hedef))
            throw new ArgumentException("Model adı boş olamaz.", nameof(yeniAlias));
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_hazir && _hazirAlias == hedef)
            {
                Bildir(ilerleme, true, "Hazır", 100, hedef);
                return;
            }
            if (_hazir)
                Bildir(ilerleme, false, "Eski model bırakılıyor", 2, _hazirAlias);
            await HazirlaIcAsync(hedef, ilerleme, ct).ConfigureAwait(false);
        }
        finally
        {
            _kapi.Release();
        }
    }

    public async Task KapatAsync()
    {
        await _kapi.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_hazir)
                await ModeliKapatIcAsync(_hazirAlias).ConfigureAwait(false);
        }
        finally
        {
            _hazir = false;
            _hazirAlias = string.Empty;
            _kapi.Release();
        }
    }

    private async Task ModeliKapatIcAsync(string alias)
    {
        if (!_yoneticiOlustu)
            return;
        var katalog = await FoundryLocalManager.Instance.GetCatalogAsync().ConfigureAwait(false);
        var model = await katalog.GetModelAsync(alias).ConfigureAwait(false);
        if (model is not null)
            await model.UnloadAsync().ConfigureAwait(false);
    }

    private static void Bildir(
        IProgress<AsistanDurumDto>? ilerleme, bool hazirMi, string asama, double yuzde, string mesaj) =>
        ilerleme?.Report(new AsistanDurumDto
        {
            HazirMi = hazirMi,
            Asama = asama,
            IlerlemeYuzde = Math.Clamp(yuzde, 0, 100),
            Mesaj = mesaj
        });
}
