using System.Reflection;

namespace MuhasibPro.Domain
{
    /// <summary>
    /// Derleme-zamanında gömülen güncelleme kaynağı (repo/feed adresi).
    /// Repo adresi sabit yazılmaz: CI'da <c>-p:GuncellemeFeedUrl=...</c>, yerelde
    /// <c>git config --get remote.origin.url</c> ile <c>AssemblyMetadata</c> olarak gömülür.
    /// Böylece farklı repo/fork'a taşınıldığında uygulama varsayılan olarak mevcut git'i kullanır.
    /// </summary>
    public static class AppGuncellemeBilgisi
    {
        public const string MetadataAnahtari = "GuncellemeFeedUrl";

        /// <summary>Ham (normalize edilmemiş) gömülü adres; yoksa boş.</summary>
        public static string HamAdres => Oku();

        /// <summary>Uygulamanın varsayılan güncelleme kaynağı (normalize edilmiş https URL).</summary>
        public static string VarsayilanFeedUrl => Normalize(HamAdres);

        /// <summary>Normalize sözleşmesinin örnekleri — birim testleri ve dev-mode öz-testi aynı kaynaktan beslenir.</summary>
        public static IReadOnlyList<NormalizeOrnegi> NormalizeOrnekleri { get; } = new[]
        {
            new NormalizeOrnegi("git@github.com:owner/repo.git", "https://github.com/owner/repo"),
            new NormalizeOrnegi("https://github.com/owner/repo.git", "https://github.com/owner/repo"),
            new NormalizeOrnegi("https://github.com/owner/repo/", "https://github.com/owner/repo"),
            new NormalizeOrnegi("ssh://git@github.com/owner/repo.git", "https://github.com/owner/repo"),
            new NormalizeOrnegi("  https://github.com/owner/repo.git  ", "https://github.com/owner/repo"),
            new NormalizeOrnegi("https://muhasibpro.com/download/latest", "https://muhasibpro.com/download/latest"),
            new NormalizeOrnegi("", ""),
        };

        /// <summary>Bir normalize örneği (ham giriş → beklenen çıkış).</summary>
        public readonly record struct NormalizeOrnegi(string Ham, string Beklenen);

        /// <summary>Normalize sözleşmesini örneklerle doğrular (dev-mode öz-testi; üretim akışına etkisi yok).</summary>
        public static (bool Basarili, int Gecen, int Toplam, IReadOnlyList<string> Detaylar) OzTest()
        {
            var detaylar = new List<string>();
            var gecen = 0;
            foreach (var ornek in NormalizeOrnekleri)
            {
                var sonuc = Normalize(ornek.Ham);
                var ok = string.Equals(sonuc, ornek.Beklenen, StringComparison.Ordinal);
                if (ok)
                    gecen++;
                detaylar.Add($"{(ok ? "OK" : "HATA")} '{ornek.Ham}' → '{sonuc}' (beklenen '{ornek.Beklenen}')");
            }
            return (gecen == NormalizeOrnekleri.Count, gecen, NormalizeOrnekleri.Count, detaylar);
        }

        /// <summary>
        /// Git remote biçimlerini Velopack/GitHub feed URL'sine çevirir:
        /// <c>git@github.com:owner/repo.git</c> → <c>https://github.com/owner/repo</c>;
        /// <c>ssh://git@host/owner/repo.git</c> → <c>https://host/owner/repo</c>;
        /// <c>https://host/owner/repo.git</c> → <c>https://host/owner/repo</c>.
        /// </summary>
        public static string Normalize(string adres)
        {
            if (string.IsNullOrWhiteSpace(adres))
                return string.Empty;

            var a = adres.Trim();

            if (a.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
            {
                // git@host:owner/repo(.git)
                var rest = a.Substring(4);
                var colon = rest.IndexOf(':');
                if (colon > 0)
                    a = $"https://{rest.Substring(0, colon)}/{rest.Substring(colon + 1)}";
            }
            else if (a.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
            {
                if (Uri.TryCreate(a, UriKind.Absolute, out var uri))
                    a = $"https://{uri.Host}{uri.AbsolutePath}";
            }

            if (a.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                a = a.Substring(0, a.Length - 4);

            return a.TrimEnd('/');
        }

        private static string Oku()
        {
            try
            {
                var entry = Assembly.GetEntryAssembly();
                var deger = Oku(entry);
                if (!string.IsNullOrWhiteSpace(deger))
                    return deger;
            }
            catch { /* entry assembly yok — devam */ }

            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var deger = Oku(asm);
                    if (!string.IsNullOrWhiteSpace(deger))
                        return deger;
                }
            }
            catch { /* yüklü assembly taranamadı — devam */ }

            return string.Empty;
        }

        private static string Oku(Assembly asm)
        {
            if (asm == null)
                return null;
            try
            {
                foreach (var meta in asm.GetCustomAttributes<AssemblyMetadataAttribute>())
                {
                    if (string.Equals(meta.Key, MetadataAnahtari, StringComparison.Ordinal))
                        return meta.Value;
                }
            }
            catch { /* reflection hatası — yoksay */ }
            return null;
        }
    }
}
