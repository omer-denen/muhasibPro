using System.Security.Cryptography;
using System.Text;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;

namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Faz 6.93: yardım bilgi tabanı orkestratörü (Singleton).
/// İçerik + (varsa) semantik indeks üzerinden hibrit arama; önbellek olduğu için iptal dışı fırlatmaz.</summary>
public sealed class YardimBilgiTabaniService : IYardimBilgiTabani
{
    private const string DbDosyaAdi = "AsistanBilgi.db";
    private const int GomuluTopluBoyut = 16;

    /// <summary>H4 ilgi eşiği: lexical en yüksek skor bunun altındaysa kalem ilgisiz sayılır (başlık=3/etiket=2/gövde=1).</summary>
    public const double LexicalEsik = 3;

    /// <summary>H4 ilgi eşiği: kosinüs benzerliği bunun altındaki vektör adayları elenir (0-1).</summary>
    public const double VektorEsik = 0.35;

    private readonly IYardimIcerikKaynagi _kaynak;
    private readonly IYardimVektorUretici _vektorUretici;
    private readonly IApplicationPaths _paths;
    private readonly IAiAsistanSettingsProvider _ayarlar;
    private readonly SemaphoreSlim _kapi = new(1, 1);

    private List<YardimKaydi> _kayitlar = new();
    private Dictionary<string, long> _idSozluk = new(StringComparer.Ordinal);
    private bool _vektorVarMi;
    private string _vektorModel = string.Empty;

    public YardimBilgiTabaniService(
        IYardimIcerikKaynagi kaynak,
        IYardimVektorUretici vektorUretici,
        IApplicationPaths paths,
        IAiAsistanSettingsProvider ayarlar)
    {
        _kaynak = kaynak ?? throw new ArgumentNullException(nameof(kaynak));
        _vektorUretici = vektorUretici ?? throw new ArgumentNullException(nameof(vektorUretici));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _ayarlar = ayarlar ?? throw new ArgumentNullException(nameof(ayarlar));
    }

    public async Task<YardimIndexDurumu> DurumGetirAsync(CancellationToken ct = default)
    {
        await YukleGuvencesiAsync(ct).ConfigureAwait(false);
        return new YardimIndexDurumu
        {
            HazirMi = _kayitlar.Count > 0,
            MaddeSayisi = _kayitlar.Count,
            VektorVarMi = _vektorVarMi,
            Asama = _kayitlar.Count == 0 ? "Hazır değil" : "Hazır",
            IlerlemeYuzde = _kayitlar.Count == 0 ? 0 : 100,
            Mesaj = _vektorVarMi ? _vektorModel : "semantik indeks yok"
        };
    }

    public async Task HazirlaAsync(bool modelIndirmeyeIzin = true, IProgress<YardimIndexDurumu>? ilerleme = null, CancellationToken ct = default)
    {
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await HazirlaIcAsync(modelIndirmeyeIzin, ilerleme, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ilerleme?.Report(new YardimIndexDurumu
            {
                HazirMi = false,
                MaddeSayisi = _kayitlar.Count,
                VektorVarMi = _vektorVarMi,
                Asama = "Hazırlanamadı",
                Mesaj = ex.Message
            });
        }
        finally
        {
            _kapi.Release();
        }
    }

    private async Task HazirlaIcAsync(bool modelIndirmeyeIzin, IProgress<YardimIndexDurumu>? ilerleme, CancellationToken ct)
    {
        Bildir(ilerleme, false, 0, "İçerik okunuyor", "İçerik okunuyor");
        var cozulmus = new List<YardimMarkdownCozumleyici.CozulmusMadde>();
        foreach (var ham in _kaynak.KaynaklariGetir())
        {
            ct.ThrowIfCancellationRequested();
            if (ham is null || string.IsNullOrWhiteSpace(ham.HamMetin))
                continue;
            cozulmus.AddRange(YardimMarkdownCozumleyici.Cozumle(ham.DosyaAdi, ham.HamMetin));
        }

        var depo = new YardimVektorDeposu(DbYolu());
        await depo.SemaGuvencesiAsync(ct).ConfigureAwait(false);

        Bildir(ilerleme, false, 10, "Dizin yazılıyor", "Dizin yazılıyor");
        var mevcut = await depo.MaddeleriGetirAsync(ct).ConfigureAwait(false);
        var mevcutSozluk = mevcut.ToDictionary(m => m.Anahtar, StringComparer.Ordinal);
        var idSozluk = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var madde in cozulmus)
        {
            ct.ThrowIfCancellationRequested();
            if (mevcutSozluk.TryGetValue(madde.Kayit.Anahtar, out var satir)
                && string.Equals(satir.IcerikHash, madde.IcerikHash, StringComparison.Ordinal))
            {
                idSozluk[madde.Kayit.Anahtar] = satir.Id;
                continue;
            }
            var id = await depo.MaddeYazAsync(
                madde.Kayit.Anahtar, madde.Kayit.Sayfa, madde.Kayit.Baslik,
                madde.Kayit.Icerik, string.Join(", ", madde.Kayit.Etiketler),
                madde.IcerikHash, ct).ConfigureAwait(false);
            idSozluk[madde.Kayit.Anahtar] = id;
        }
        await depo.OlmayanlariSilAsync(new HashSet<string>(idSozluk.Keys, StringComparer.Ordinal), ct).ConfigureAwait(false);

        var ayar = await _ayarlar.GetAsync().ConfigureAwait(false);
        var alias = ayar.GetEmbeddingModelAlias();
        var surum = SurumHesapla(cozulmus);

        bool vektorTamam = false;
        string vektorMesaj = "semantik indeks yok";
        try
        {
            var sakli = await depo.VektorleriGetirAsync(alias, ct).ConfigureAwait(false);
            var sakliIdler = new HashSet<long>(sakli.Select(v => v.MaddeId));
            var oncekiModel = await depo.MetaOkuAsync("VektorModel", ct).ConfigureAwait(false);
            bool modelDegisti = !string.Equals(oncekiModel, alias, StringComparison.Ordinal);
            var eksikler = cozulmus.Where(m =>
            {
                var id = idSozluk[m.Kayit.Anahtar];
                if (modelDegisti || !sakliIdler.Contains(id))
                    return true;
                return mevcutSozluk.TryGetValue(m.Kayit.Anahtar, out var satir)
                    && !string.Equals(satir.IcerikHash, m.IcerikHash, StringComparison.Ordinal);
            }).ToList();
            if (eksikler.Count == 0)
            {
                vektorTamam = sakli.Count > 0 || cozulmus.Count == 0;
                if (cozulmus.Count > 0 && sakli.Count == 0)
                    vektorMesaj = "semantik indeks yok";
            }
            else if (!modelIndirmeyeIzin && !await _vektorUretici.OnbellekteMiAsync(ct).ConfigureAwait(false))
            {
                // 6.91-G kapısı: indirme yasak ve model önbellekte değil → vektör adımı atlanır.
                vektorMesaj = "Embedding modeli önbellekte değil ve indirmeye izin verilmedi — lexical-only.";
            }
            else
            {
                // modelIndirmeyeIzin=true iken UretAsync indirip yükler (sözleşme); hatası dış catch'te lexical'e düşer.
                Bildir(ilerleme, false, 55, "Semantik indeks yazılıyor", "Semantik indeks yazılıyor");
                int boyut = await _vektorUretici.VektorBoyutuAsync(ct).ConfigureAwait(false);
                int uretilen = 0;
                foreach (var parca in eksikler.Chunk(GomuluTopluBoyut))
                {
                    ct.ThrowIfCancellationRequested();
                    var metinler = parca.Select(m => m.Kayit.Baslik + "\n" + m.Kayit.Icerik).ToList();
                    var vektorler = await _vektorUretici.UretAsync(metinler, null, ct).ConfigureAwait(false);
                    for (int i = 0; i < parca.Length && i < vektorler.Count; i++)
                    {
                        var v = vektorler[i] ?? [];
                        await depo.VektorYazAsync(
                            idSozluk[parca[i].Kayit.Anahtar], alias, v.Length,
                            YardimVektorDeposu.VektorPaketle(v), ct).ConfigureAwait(false);
                    }
                    uretilen += parca.Length;
                    double yuzde = 55 + 40.0 * uretilen / Math.Max(1, eksikler.Count);
                    Bildir(ilerleme, false, yuzde, "Semantik indeks yazılıyor", "Semantik indeks yazılıyor");
                }
                await depo.MetaYazAsync("VektorModel", alias, ct).ConfigureAwait(false);
                await depo.MetaYazAsync("VektorBoyut", boyut.ToString(), ct).ConfigureAwait(false);
                vektorTamam = true;
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            vektorTamam = false;
            vektorMesaj = ex.Message;
        }

        await depo.MetaYazAsync("IcerikSurumu", surum, ct).ConfigureAwait(false);
        await depo.MetaYazAsync("DizinTarihi", DateTime.UtcNow.ToString("o"), ct).ConfigureAwait(false);

        _kayitlar = cozulmus.Select(m => m.Kayit).ToList();
        _idSozluk = new Dictionary<string, long>(idSozluk, StringComparer.Ordinal);
        _vektorVarMi = vektorTamam;
        _vektorModel = vektorTamam ? alias : string.Empty;
        Bildir(ilerleme, true, 100, "Hazır", vektorTamam ? alias : vektorMesaj);
    }

    public async Task<IReadOnlyList<YardimAramaSonucu>> AraAsync(string soru, int enFazla, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(soru) || enFazla <= 0)
            return [];
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await YukleKilitliAsync(ct).ConfigureAwait(false);
            if (_kayitlar.Count == 0)
                return [];

            var lexical = YardimSkorlayici.KayitlariSirala(
                soru, _kayitlar.Select(k => (k.Anahtar, k.Baslik, k.Icerik, string.Join(", ", k.Etiketler))).ToList());
            var lexicalSira = lexical.Select(e => e.Anahtar).ToList();

            List<string>? vektorSira = null;
            if (_vektorVarMi)
            {
                try
                {
                    vektorSira = await VektorSiralaAsync(soru, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    vektorSira = null;
                }
            }

            // H4 ilgi eşiği: semantik indeks yoksa ve lexical skor zayıfsa yardım maddesi döndürme
            // (aksi halde alakasız maddeler prompt'a girer ve küçük model uydurmaya itilir).
            if ((vektorSira is null || vektorSira.Count == 0)
                && (lexical.Count == 0 || lexical[0].Skor < LexicalEsik))
                return [];

            IReadOnlyList<(string Anahtar, double Skor)> birlesik;
            string yontem;
            if (vektorSira is { Count: > 0 })
            {
                birlesik = RrfBirlestirici.Birlestir(lexicalSira, vektorSira);
                yontem = "hibrit";
            }
            else
            {
                birlesik = RrfBirlestirici.Birlestir(lexicalSira, null, 1, 0);
                yontem = "lexical";
            }

            var sozluk = _kayitlar.ToDictionary(k => k.Anahtar, StringComparer.Ordinal);
            var sonuc = new List<YardimAramaSonucu>();
            foreach (var (anahtar, skor) in birlesik.Take(enFazla))
            {
                ct.ThrowIfCancellationRequested();
                if (!sozluk.TryGetValue(anahtar, out var kayit))
                    continue;
                sonuc.Add(new YardimAramaSonucu
                {
                    Anahtar = kayit.Anahtar,
                    Sayfa = kayit.Sayfa,
                    Baslik = kayit.Baslik,
                    Icerik = kayit.Icerik,
                    Skor = skor,
                    Yontem = yontem
                });
            }
            return sonuc;
        }
        finally
        {
            _kapi.Release();
        }
    }

    public IReadOnlyList<YardimKaydi> TumKayitlar() => _kayitlar.ToList();

    private async Task<List<string>> VektorSiralaAsync(string soru, CancellationToken ct)
    {
        var ayar = await _ayarlar.GetAsync().ConfigureAwait(false);
        var alias = ayar.GetEmbeddingModelAlias();
        var uretilen = await _vektorUretici.UretAsync([soru], null, ct).ConfigureAwait(false);
        var sorgu = (uretilen.Count > 0 ? uretilen[0] : null) ?? [];
        if (sorgu.Length == 0)
            return [];
        var depo = new YardimVektorDeposu(DbYolu());
        var sakli = await depo.VektorleriGetirAsync(alias, ct).ConfigureAwait(false);
        var skorlar = new List<(string Anahtar, double Skor)>();
        var sozluk = _kayitlar.ToDictionary(k => k.Anahtar, StringComparer.Ordinal);
        var tersSozluk = _idSozluk.ToDictionary(e => e.Value, e => e.Key);
        foreach (var (maddeId, _, blob) in sakli)
        {
            ct.ThrowIfCancellationRequested();
            if (!tersSozluk.TryGetValue(maddeId, out var anahtar) || !sozluk.ContainsKey(anahtar))
                continue;
            var vektor = YardimVektorDeposu.VektorAc(blob, sorgu.Length);
            skorlar.Add((anahtar, Kosinus(sorgu, vektor)));
        }
        // H4 ilgi eşiği: düşük benzerlikli vektör adayları elenir (gürültüyü azaltır).
        return skorlar.Where(e => e.Skor >= VektorEsik)
            .OrderByDescending(e => e.Skor).Select(e => e.Anahtar).ToList();
    }

    internal static double Kosinus(float[] a, float[] b)
    {
        double nokta = 0, na = 0, nb = 0;
        int n = Math.Min(a.Length, b.Length);
        for (int i = 0; i < n; i++)
        {
            nokta += (double)a[i] * b[i];
            na += (double)a[i] * a[i];
            nb += (double)b[i] * b[i];
        }
        if (na <= 0 || nb <= 0)
            return 0;
        return nokta / (Math.Sqrt(na) * Math.Sqrt(nb));
    }

    internal static string SurumHesapla(IReadOnlyList<YardimMarkdownCozumleyici.CozulmusMadde> maddeler)
    {
        var metin = string.Join("\n", maddeler
            .OrderBy(m => m.Kayit.Anahtar, StringComparer.Ordinal)
            .Select(m => m.Kayit.Anahtar + "\n" + m.IcerikHash));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(metin)));
    }

    private async Task YukleGuvencesiAsync(CancellationToken ct)
    {
        await _kapi.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await YukleKilitliAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _kapi.Release();
        }
    }

    private async Task YukleKilitliAsync(CancellationToken ct)
    {
        if (_kayitlar.Count > 0)
            return;
        try
        {
            var depo = new YardimVektorDeposu(DbYolu());
            await depo.SemaGuvencesiAsync(ct).ConfigureAwait(false);
            var maddeler = await depo.MaddeleriGetirAsync(ct).ConfigureAwait(false);
            _kayitlar = maddeler.Select(m => new YardimKaydi
            {
                Anahtar = m.Anahtar,
                Sayfa = m.Sayfa,
                Baslik = m.Baslik,
                Icerik = m.Icerik,
                Etiketler = (m.Etiketler ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            }).ToList();
            _idSozluk = maddeler.ToDictionary(m => m.Anahtar, m => m.Id, StringComparer.Ordinal);
            var ayar = await _ayarlar.GetAsync().ConfigureAwait(false);
            var sakli = await depo.VektorleriGetirAsync(ayar.GetEmbeddingModelAlias(), ct).ConfigureAwait(false);
            var idler = new HashSet<long>(maddeler.Select(m => m.Id));
            _vektorVarMi = sakli.Count > 0 && sakli.All(v => idler.Contains(v.MaddeId));
            _vektorModel = _vektorVarMi ? ayar.GetEmbeddingModelAlias() : string.Empty;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            _kayitlar = new List<YardimKaydi>();
            _idSozluk = new Dictionary<string, long>(StringComparer.Ordinal);
            _vektorVarMi = false;
            _vektorModel = string.Empty;
        }
    }

    private string DbYolu() => Path.Combine(_paths.GetAppDataFolderPath(), DbDosyaAdi);

    private static void Bildir(IProgress<YardimIndexDurumu>? ilerleme, bool hazirMi, double yuzde, string asama, string mesaj) =>
        ilerleme?.Report(new YardimIndexDurumu
        {
            HazirMi = hazirMi,
            MaddeSayisi = 0,
            VektorVarMi = false,
            Asama = asama,
            IlerlemeYuzde = Math.Clamp(yuzde, 0, 100),
            Mesaj = mesaj
        });
}
