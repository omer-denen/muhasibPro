using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;
using MuhasibPro.Business.Services.SistemServices.AiAsistan;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.93: YardimBilgiTabaniService davranış matriksi (temp-dir SQLite + sahte vektör üretici).</summary>
public class YardimBilgiTabaniTests : IDisposable
{
    private const string Icerik = """
        # Deneme

        ## Arşivleme nasıl yapılır?
        Dönem seçilip arşivlenir.
        Etiket: arşiv

        ## Yedekleme nasıl yapılır?
        Yedek alınır ve doğrulanır.
        Etiket: yedek
        """;

    private readonly string _kok;
    private bool _atildi;

    public YardimBilgiTabaniTests()
    {
        _kok = Path.Combine(Path.GetTempPath(), "YbTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_kok);
    }

    public void Dispose()
    {
        if (_atildi)
            return;
        _atildi = true;
        try { Directory.Delete(_kok, true); } catch { }
    }

    private sealed class SabitKaynak : IYardimIcerikKaynagi
    {
        public string Metin = Icerik;
        public IReadOnlyList<YardimHamKaynak> KaynaklariGetir() =>
            [new YardimHamKaynak { DosyaAdi = "deneme.md", HamMetin = Metin }];
    }

    private sealed class SahteVektor : IYardimVektorUretici
    {
        public string ModelAlias => "test-emb";
        public bool Onbellekte = true;
        public bool Patlasin;
        public int UretCagrisi;
        public Func<string, float[]>? VektorSec;
        public Task<int> VektorBoyutuAsync(CancellationToken ct = default) => Task.FromResult(3);
        public Task<bool> OnbellekteMiAsync(CancellationToken ct = default) => Task.FromResult(Onbellekte);
        public Task<IReadOnlyList<float[]>> UretAsync(IReadOnlyList<string> metinler, IProgress<double>? ilerleme = null, CancellationToken ct = default)
        {
            UretCagrisi++;
            if (Patlasin)
                throw new InvalidOperationException("model yok");
            return Task.FromResult<IReadOnlyList<float[]>>(metinler.Select(m => VektorSec?.Invoke(m) ?? [1f, 0f, 0f]).ToList());
        }
    }

    private static Mock<IApplicationPaths> Yollar(string kok)
    {
        var yollar = new Mock<IApplicationPaths>();
        yollar.Setup(y => y.GetAppDataFolderPath()).Returns(kok);
        return yollar;
    }

    private static Mock<IAiAsistanSettingsProvider> Ayarlar()
    {
        var ayarlar = new Mock<IAiAsistanSettingsProvider>();
        ayarlar.Setup(a => a.GetAsync()).ReturnsAsync(new AiAsistanSettings());
        return ayarlar;
    }

    private YardimBilgiTabaniService Servis(SabitKaynak kaynak, SahteVektor vektor) =>
        new(kaynak, vektor, Yollar(_kok).Object, Ayarlar().Object);

    [Fact]
    public async Task Hazirla_LexicalOnly_DizinKurar()
    {
        var svc = Servis(new SabitKaynak(), new SahteVektor { Onbellekte = false });

        await svc.HazirlaAsync();
        var durum = await svc.DurumGetirAsync();

        durum.HazirMi.Should().BeTrue();
        durum.MaddeSayisi.Should().Be(2);
        durum.VektorVarMi.Should().BeFalse();
        svc.TumKayitlar().Should().HaveCount(2);
    }

    [Fact]
    public async Task Ara_LexicalOnly_DogruMadde_YontemLexical()
    {
        var svc = Servis(new SabitKaynak(), new SahteVektor { Onbellekte = false });
        await svc.HazirlaAsync();

        var sonuc = await svc.AraAsync("arşivleme nasıl yapılır", 6);

        sonuc.Should().NotBeEmpty();
        sonuc[0].Anahtar.Should().Be("deneme#1");
        sonuc.All(s => s.Yontem == "lexical").Should().BeTrue();
    }

    [Fact]
    public async Task Hazirla_Vektorlu_HibritArama()
    {
        var vektor = new SahteVektor
        {
            VektorSec = m => m.Contains("Arşivleme") ? [1f, 0f, 0f] : [0f, 1f, 0f]
        };
        var svc = Servis(new SabitKaynak(), vektor);
        await svc.HazirlaAsync();
        (await svc.DurumGetirAsync()).VektorVarMi.Should().BeTrue();

        // Lexical'in bulamadığı soruyu vektör bulur → hibrit ilk sırada arşiv maddesini verir.
        vektor.VektorSec = _ => [1f, 0f, 0f];
        var sonuc = await svc.AraAsync("saklama politikası", 6);

        sonuc.Should().NotBeEmpty();
        sonuc[0].Anahtar.Should().Be("deneme#1");
        sonuc[0].Yontem.Should().Be("hibrit");
    }

    [Fact]
    public async Task Hazirla_ModelIndirmeyeIzinFalse_VektorAtlar()
    {
        var vektor = new SahteVektor();
        var svc = Servis(new SabitKaynak(), vektor);

        await svc.HazirlaAsync(modelIndirmeyeIzin: false);

        vektor.UretCagrisi.Should().Be(0);
        (await svc.DurumGetirAsync()).VektorVarMi.Should().BeFalse();
    }

    [Fact]
    public async Task Hazirla_VektorHatasi_LexicalDuser()
    {
        var svc = Servis(new SabitKaynak(), new SahteVektor { Patlasin = true });

        await svc.HazirlaAsync();
        var durum = await svc.DurumGetirAsync();

        durum.HazirMi.Should().BeTrue();
        durum.VektorVarMi.Should().BeFalse();
        var sonuc = await svc.AraAsync("arşivleme", 6);
        sonuc.Should().NotBeEmpty();
        sonuc[0].Yontem.Should().Be("lexical");
    }

    [Fact]
    public async Task Hazirla_DegismeyenIcerik_VektoruKorur()
    {
        var vektor = new SahteVektor();
        var kaynak = new SabitKaynak();
        var svc = Servis(kaynak, vektor);

        await svc.HazirlaAsync();
        int ilk = vektor.UretCagrisi;
        ilk.Should().BeGreaterThan(0);
        await svc.HazirlaAsync();

        vektor.UretCagrisi.Should().Be(ilk);
    }

    [Fact]
    public async Task Hazirla_DegisenMadde_YenidenGomulur()
    {
        var vektor = new SahteVektor();
        var kaynak = new SabitKaynak();
        var svc = Servis(kaynak, vektor);
        await svc.HazirlaAsync();
        int ilk = vektor.UretCagrisi;

        kaynak.Metin = Icerik.Replace("Yedek alınır ve doğrulanır.", "Yedek alınır, doğrulanır ve saklanır.");
        await svc.HazirlaAsync();

        vektor.UretCagrisi.Should().BeGreaterThan(ilk);
    }

    [Fact]
    public async Task Bos_Soru_Bos_Doner()
    {
        var svc = Servis(new SabitKaynak(), new SahteVektor { Onbellekte = false });
        await svc.HazirlaAsync();

        (await svc.AraAsync("  ", 6)).Should().BeEmpty();
        (await svc.AraAsync("arşiv", 0)).Should().BeEmpty();
    }

    [Fact]
    public async Task AramaSonucu_Prompta_Girer()
    {
        var svc = Servis(new SabitKaynak(), new SahteVektor { Onbellekte = false });
        await svc.HazirlaAsync();
        var sonuc = await svc.AraAsync("arşivleme nasıl yapılır", 6);

        var mesajlar = AsistanPromptKurucu.AramaSonuclariylaKur(
            sonuc, new AsistanSoruDto { Soru = "arşivleme nasıl yapılır" }, 4);

        mesajlar[0].Icerik.Should().Contain("Arşivleme nasıl yapılır?");
        var bos = AsistanPromptKurucu.AramaSonuclariylaKur(
            [], new AsistanSoruDto { Soru = "soru" }, 4);
        bos.Should().HaveCount(2);
    }
}
