using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using FoundryLogLevel = Microsoft.AI.Foundry.Local.LogLevel;

namespace MuhasibPro.Services.AiAsistan;

/// <summary>
/// Faz 6.93: Foundry Local süreç-içi embedding üretici (Singleton).
/// Akış: yönetici → EP indir → katalog → embedding modeli indir/yükle → batch gömme.
/// Model indirilemezse FIRLATIR — çağıran (bilgi tabanı) lexical'e düşer.
/// </summary>
public sealed class FoundryYardimVektorUretici : IYardimVektorUretici
{
    private const string UygulamaAdi = "MuhasibPro";

    private readonly IAiAsistanSettingsProvider _ayarlar;
    private readonly ILogger<FoundryYardimVektorUretici>? _logger;
    private readonly SemaphoreSlim _kapi = new(1, 1);
    private bool _yoneticiOlustu;
    private string _hazirAlias = string.Empty;
    private int _boyut;

    public FoundryYardimVektorUretici(
        IAiAsistanSettingsProvider ayarlar,
        ILogger<FoundryYardimVektorUretici>? logger = null)
    {
        _ayarlar = ayarlar ?? throw new ArgumentNullException(nameof(ayarlar));
        _logger = logger;
    }

    public string ModelAlias => _hazirAlias;

    public async Task<int> VektorBoyutuAsync(CancellationToken ct = default)
    {
        if (_boyut > 0 && !string.IsNullOrEmpty(_hazirAlias))
            return _boyut;
        var prob = await UretAsync(["test"], null, ct).ConfigureAwait(false);
        if (prob.Count == 0 || prob[0].Length == 0)
            throw new InvalidOperationException("Embedding boyutu okunamadı.");
        return _boyut;
    }

    public async Task<bool> OnbellekteMiAsync(CancellationToken ct = default)
    {
        try
        {
            var ayar = await _ayarlar.GetAsync().ConfigureAwait(false);
            var alias = ayar.GetEmbeddingModelAlias();
            await _kapi.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                await YoneticiyiKurAsync().ConfigureAwait(false);
                var katalog = await FoundryLocalManager.Instance.GetCatalogAsync(ct).ConfigureAwait(false);
                var model = await katalog.GetModelAsync(alias, ct).ConfigureAwait(false);
                return model is not null && await model.IsCachedAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                _kapi.Release();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<float[]>> UretAsync(IReadOnlyList<string> metinler, IProgress<double>? ilerleme = null, CancellationToken ct = default)
    {
        if (metinler is null || metinler.Count == 0)
            return [];
        var ayar = await _ayarlar.GetAsync().ConfigureAwait(false);
        var alias = ayar.GetEmbeddingModelAlias();
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            ilerleme?.Report(0.05);
            await HazirlaIcAsync(alias, ct).ConfigureAwait(false);
            ilerleme?.Report(0.2);

            var katalog = await FoundryLocalManager.Instance.GetCatalogAsync(ct).ConfigureAwait(false);
            var model = await katalog.GetModelAsync(alias, ct).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Embedding modeli katalogda bulunamadı: '{alias}'.");
            var istemci = await model.GetEmbeddingClientAsync().ConfigureAwait(false);
            var yanit = await istemci.GenerateEmbeddingsAsync(metinler).ConfigureAwait(false);
            var sonuc = new List<float[]>(metinler.Count);
            for (int i = 0; i < metinler.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var gomulu = yanit.Data[i].Embedding;
                var vektor = new float[gomulu.Count];
                for (int j = 0; j < gomulu.Count; j++)
                    vektor[j] = (float)gomulu[j];
                sonuc.Add(vektor);
                ilerleme?.Report(0.2 + 0.8 * (i + 1) / metinler.Count);
            }
            if (sonuc.Count > 0)
                _boyut = sonuc[0].Length;
            return sonuc;
        }
        finally
        {
            _kapi.Release();
        }
    }

    /// <summary>Kapı çağırandadır; alias çözülmüş gelir.</summary>
    private async Task HazirlaIcAsync(string alias, CancellationToken ct)
    {
        if (_hazirAlias == alias && !string.IsNullOrEmpty(alias))
            return;

        ct.ThrowIfCancellationRequested();
        await YoneticiyiKurAsync().ConfigureAwait(false);

        var yonetici = FoundryLocalManager.Instance;
        var eps = yonetici.DiscoverEps();
        if (eps is { Length: > 0 })
            await yonetici.DownloadAndRegisterEpsAsync((_, _) => { }).ConfigureAwait(false);

        ct.ThrowIfCancellationRequested();
        var katalog = await yonetici.GetCatalogAsync(ct).ConfigureAwait(false);
        var model = await katalog.GetModelAsync(alias, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Embedding modeli katalogda bulunamadı: '{alias}'. Ayarlardan model adını kontrol edin.");

        await model.DownloadAsync(_ => { }).ConfigureAwait(false);

        ct.ThrowIfCancellationRequested();
        await model.LoadAsync().ConfigureAwait(false);

        _hazirAlias = alias;
        _logger?.LogInformation("Embedding modeli hazır: {Alias}", alias);
    }

    private async Task YoneticiyiKurAsync()
    {
        if (_yoneticiOlustu)
            return;
        await FoundryLocalManager.CreateAsync(
            new Configuration { AppName = UygulamaAdi, LogLevel = FoundryLogLevel.Information },
            NullLogger.Instance).ConfigureAwait(false);
        _yoneticiOlustu = true;
    }
}
