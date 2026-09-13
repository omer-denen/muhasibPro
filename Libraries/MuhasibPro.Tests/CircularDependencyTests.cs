using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace MuhasibPro.Tests;

/// <summary>
/// DI circular-dependency bekçisi (Oturum 140 — AI-model hatası dersi).
/// Tek tek ctor gezmek yerine: gerçek HostBuilder kayıtları + constructor grafı
/// üzerinden döngü arar. Döngü bulunursa zinciri tam olarak raporlar.
/// </summary>
public class CircularDependencyTests
{
    private static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (File.Exists(Path.Combine(dir, "MuhasibPro.slnx")) || File.Exists(Path.Combine(dir, "MuhasibPro.sln"))) return dir;
            dir = Path.GetDirectoryName(dir)!;
        }
        return AppContext.BaseDirectory;
    }

    private static List<Assembly> TarananAssemblyler()
    {
        foreach (var ad in new[] { "MuhasibPro.Business", "MuhasibPro.Data", "MuhasibPro.ViewModels", "MuhasibPro.Domain" })
        {
            try { Assembly.Load(ad); } catch { /* yoksa atla */ }
        }
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("MuhasibPro.") == true
                && !a.GetName().Name!.EndsWith(".Tests"))
            .ToList();
    }

    /// <summary>Gerçek DI kayıt haritası: interface kısa-ad → impl kısa-ad.</summary>
    private static Dictionary<string, string> KayitHaritasi()
    {
        var harita = new Dictionary<string, string>(StringComparer.Ordinal);
        var kok = RepoRoot();
        var dosyalar = new List<string>();
        foreach (var klasor in new[]
        {
            Path.Combine(kok, "Libraries/MuhasibPro.Business/HostBuilder"),
            Path.Combine(kok, "MuhasibPro/HostBuilders")
        })
        {
            if (Directory.Exists(klasor))
                dosyalar.AddRange(Directory.EnumerateFiles(klasor, "*.cs"));
        }
        var kayitDeseni = new Regex(
            @"Add(?:Singleton|Scoped|Transient)\s*<\s*([\w\.]+)\s*,\s*([\w\.]+)\s*>",
            RegexOptions.Compiled);
        foreach (var dosya in dosyalar)
        {
            var icerik = File.ReadAllText(dosya);
            foreach (Match m in kayitDeseni.Matches(icerik))
                harita[KisaAd(m.Groups[1].Value)] = KisaAd(m.Groups[2].Value);
        }
        return harita;
    }

    private static string KisaAd(string tamAd) =>
        tamAd.Contains('.') ? tamAd[(tamAd.LastIndexOf('.') + 1)..] : tamAd;

    /// <summary>App katmanı (WinUI, reflection ile yüklenemez): kaynaktan ctor-kenarları.
    /// Sınıf kısa-ad → ctor parametre tip kısa-ad listesi.</summary>
    private static Dictionary<string, List<string>> AppKaynakKenarlari()
    {
        var sonuc = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var kok = RepoRoot();
        var klasorler = new[]
        {
            Path.Combine(kok, "MuhasibPro/Services"),
            Path.Combine(kok, "MuhasibPro/Helpers"),
            Path.Combine(kok, "MuhasibPro/HostBuilders")
        };
        var sinifDeseni = new Regex(@"(?:public\s+|internal\s+)?(?:sealed\s+|static\s+|abstract\s+)*class\s+(\w+)",
            RegexOptions.Compiled);
        var ctorDeseni = new Regex(@"public\s+(\w+)\s*\(((?:[^()]|\([^()]*\))*)\)",
            RegexOptions.Compiled | RegexOptions.Singleline);
        foreach (var klasor in klasorler)
        {
            if (!Directory.Exists(klasor)) continue;
            foreach (var dosya in Directory.EnumerateFiles(klasor, "*.cs", SearchOption.AllDirectories))
            {
                if (dosya.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")) continue;
                var icerik = File.ReadAllText(dosya);
                var siniflar = sinifDeseni.Matches(icerik).Select(m => m.Groups[1].Value).ToHashSet();
                if (siniflar.Count == 0) continue;
                // Greedy ctor = en çok parametreli (MS DI davranışı).
                string? enIyi = null;
                List<string>? enIyiParams = null;
                foreach (Match m in ctorDeseni.Matches(icerik))
                {
                    var ad = m.Groups[1].Value;
                    if (!siniflar.Contains(ad)) continue; // metoda değil, ctor'a bak
                    var paramlar = CtorParamTipleri(m.Groups[2].Value);
                    if (enIyiParams == null || paramlar.Count > enIyiParams.Count)
                    {
                        enIyi = ad;
                        enIyiParams = paramlar;
                    }
                }
                if (enIyi != null && enIyiParams != null)
                    sonuc[enIyi] = enIyiParams;
            }
        }
        return sonuc;
    }

    private static List<string> CtorParamTipleri(string paramBlogu)
    {
        var sonuc = new List<string>();
        if (string.IsNullOrWhiteSpace(paramBlogu)) return sonuc;
        foreach (var parca in UstDuzeyBol(paramBlogu, ','))
        {
            var p = Regex.Replace(parca.Trim(), @"\[.*?\]", string.Empty).Trim();
            var esit = p.IndexOf('=');
            if (esit >= 0) p = p[..esit].Trim();
            var bosluk = p.LastIndexOf(' ');
            if (bosluk < 0) continue;
            var tip = p[..bosluk].Trim().TrimEnd('?');
            if (tip.Length == 0 || tip.Contains('(')) continue;
            // Dış tip + generic argümanlar ayrı kenar adayıdır.
            foreach (Match m in Regex.Matches(tip, @"[A-Za-z_][\w\.]*"))
            {
                var kisa = KisaAd(m.Value);
                if (kisa is "string" or "int" or "long" or "bool" or "double" or "object"
                    or "IEnumerable" or "Func" or "Lazy" or "Action" or "Task"
                    or "ILogger" or "IOptions" or "IServiceProvider" or "IServiceScopeFactory")
                    continue;
                sonuc.Add(kisa);
            }
        }
        return sonuc.Distinct().ToList();
    }

    private static List<string> UstDuzeyBol(string s, char ayrac)
    {
        var parcalar = new List<string>();
        int derinlik = 0, baslangic = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if ("<([".Contains(s[i])) derinlik++;
            else if (">)]".Contains(s[i])) derinlik--;
            else if (s[i] == ayrac && derinlik == 0)
            {
                parcalar.Add(s[baslangic..i]);
                baslangic = i + 1;
            }
        }
        parcalar.Add(s[baslangic..]);
        return parcalar;
    }

    private static bool KenarDisi(Type t)
    {
        if (t == typeof(IServiceProvider) || t == typeof(IServiceScopeFactory)) return true;
        if (t.IsGenericType)
        {
            var g = t.GetGenericTypeDefinition();
            if (g.FullName?.StartsWith("System.Collections.Generic.IEnumerable") == true) return true;
            if (g.FullName?.StartsWith("Microsoft.Extensions.Logging.ILogger") == true) return true;
            if (g.FullName?.StartsWith("Microsoft.Extensions.Options") == true) return true;
            if (g.FullName?.StartsWith("System.Func") == true) return true;
            if (g.FullName?.StartsWith("System.Lazy") == true) return true;
        }
        if (t == typeof(string) || t.IsPrimitive || t.IsEnum) return true;
        return false;
    }

    private static ConstructorInfo? SecilenCtor(Type impl)
    {
        var ctorlar = impl.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        if (ctorlar.Length == 0) return null;
        // MS DI: [ActivatorUtilitiesConstructor] işaretli tek ctor kazanır.
        var isaretli = ctorlar.Where(c =>
            c.GetCustomAttributes().Any(a => a.GetType().FullName ==
                "Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructorAttribute")).ToList();
        if (isaretli.Count == 1) return isaretli[0];
        return ctorlar.OrderByDescending(c => c.GetParameters().Length).First();
    }

    [Fact]
    public void DI_Circular_Baglanti_Olmamali()
    {
        var assemblies = TarananAssemblyler();
        assemblies.Should().NotBeEmpty("Business/Data/ViewModels assembly'leri yüklenemedi");
        var harita = KayitHaritasi();

        var implar = assemblies
            .SelectMany(a => { try { return a.GetTypes(); } catch { return []; } })
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition
                && t.IsPublic && !t.Name.Contains('<'))
            .ToDictionary(t => t.FullName!, t => t);

        // App katmanı düğümleri önceden yüklenir (reflection kenarları da App impl'ına bağlanabilsin).
        var appKenarlari = AppKaynakKenarlari();
        string ImplCoz(string kisa)
        {
            if (harita.TryGetValue(kisa, out var hedefKisa))
            {
                var hedef = implar.Values.FirstOrDefault(t => t.Name == hedefKisa);
                if (hedef?.FullName != null) return hedef.FullName;
                if (appKenarlari.ContainsKey(hedefKisa)) return "App:" + hedefKisa;
                return string.Empty;
            }
            var dogrudan = implar.Values.FirstOrDefault(t => t.Name == kisa);
            if (dogrudan?.FullName != null) return dogrudan.FullName;
            if (appKenarlari.ContainsKey(kisa)) return "App:" + kisa;
            return string.Empty; // yaprak (kayıtsız sözleşme / framework)
        }

        // Kenar: impl → bağımlı olduğu impl'lar.
        var kenarlar = new Dictionary<string, List<string>>();
        foreach (var (tamAd, tip) in implar)
        {
            var liste = new List<string>();
            var ctor = SecilenCtor(tip);
            if (ctor != null)
            {
                foreach (var p in ctor.GetParameters())
                {
                    var pt = p.ParameterType;
                    if (KenarDisi(pt)) continue;
                    if (pt.IsGenericType && !pt.IsInterface && !pt.IsAbstract)
                    {
                        // Kapalı generic somut tip (örn. UnitOfWork<SistemDbContext>) — doğrudan kenar.
                        if (implar.ContainsKey(pt.FullName!)) liste.Add(pt.FullName!);
                        continue;
                    }
                    if (pt.IsInterface || pt.IsAbstract)
                    {
                        var kisa = KisaAd(pt.FullName ?? pt.Name);
                        var cozum = ImplCoz(kisa);
                        if (cozum.Length > 0)
                        {
                            liste.Add(cozum);
                        }
                        else
                        {
                            // Kayıtsız sözleşme: taranan impl'lar arasından adaylar (App-katmanı impl yoksa yaprak).
                            foreach (var aday in implar.Values.Where(t => pt.IsAssignableFrom(t)))
                                liste.Add(aday.FullName!);
                        }
                    }
                    else if (implar.ContainsKey(pt.FullName!))
                    {
                        liste.Add(pt.FullName!);
                    }
                }
            }
            kenarlar[tamAd] = liste.Distinct().Where(x => x != tamAd).ToList();
        }

        foreach (var (sinif, paramlar) in appKenarlari)
        {
            var dugum = "App:" + sinif;
            var liste = new List<string>();
            foreach (var p in paramlar)
            {
                var cozum = ImplCoz(p);
                if (cozum.Length > 0 && cozum != dugum) liste.Add(cozum);
            }
            kenarlar[dugum] = liste.Distinct().ToList();
        }

        // Kör-tarama guard'ı: evren boşsa test yanlış-yeşil verir.
        kenarlar.Count.Should().BeGreaterThan(100,
            "reflection evreni boş — assembly yükleme kör kaldı");
        appKenarlari.Count.Should().BeGreaterThan(10,
            "App kaynak taraması kör kaldı — MuhasibPro/Services ctor'ları okunamadı");

        // Derinlik-öncelikli döngü avı (renk: 0 yok, 1 yolda, 2 bitti).
        var durum = new Dictionary<string, int>();
        var donguler = new List<string>();
        void Gez(string dugum, Stack<string> yol)
        {
            durum[dugum] = 1;
            yol.Push(dugum);
            foreach (var komsu in kenarlar.GetValueOrDefault(dugum, []))
            {
                if (!kenarlar.ContainsKey(komsu)) continue;
                if (durum.GetValueOrDefault(komsu) == 1)
                {
                    var zincir = yol.Reverse()
                        .SkipWhile(x => x != komsu)
                        .Concat([komsu])
                        .Select(KisaAd);
                    donguler.Add(string.Join(" → ", zincir));
                }
                else if (durum.GetValueOrDefault(komsu) == 0)
                {
                    Gez(komsu, yol);
                }
            }
            yol.Pop();
            durum[dugum] = 2;
        }
        foreach (var dugum in kenarlar.Keys)
            if (durum.GetValueOrDefault(dugum) == 0)
                Gez(dugum, new Stack<string>());

        donguler.Distinct().Should().BeEmpty(
            "DI circular dependency var — zincir: " + string.Join(" | ", donguler.Distinct()));
    }
}
