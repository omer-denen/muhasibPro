using Microsoft.Data.Sqlite;

namespace MuhasibPro.Business.Services.SistemServices.AiAsistan;

/// <summary>Faz 6.93: AsistanBilgi.db ham erişimi (EF migration YOK, raw ADO).
/// Bu bir önbellektir; dosya bozuk/uyumsuzsa yeniden oluşturulur (kullanıcı verisi değil).</summary>
public sealed class YardimVektorDeposu
{
    public const string SemaSurumu = "1";

    public record DepoMadde(long Id, string Anahtar, string Sayfa, string Baslik, string Icerik, string Etiketler, string IcerikHash);

    private readonly string _dbYolu;

    public YardimVektorDeposu(string dbDosyaYolu)
    {
        _dbYolu = dbDosyaYolu ?? throw new ArgumentNullException(nameof(dbDosyaYolu));
    }

    /// <summary>Dosya/şema güvencesi: eksik/bozuk/uyumsuzsa dosya yeniden oluşturulur.</summary>
    public async Task SemaGuvencesiAsync(CancellationToken ct = default)
    {
        try
        {
            SQLitePCL.Batteries_V2.Init();
        }
        catch (InvalidOperationException)
        {
            // Zaten başlatılmış — devam.
        }
        var klasor = Path.GetDirectoryName(_dbYolu);
        if (!string.IsNullOrEmpty(klasor))
            Directory.CreateDirectory(klasor);

        if (!SemaUygunMu())
        {
            if (File.Exists(_dbYolu))
                File.Delete(_dbYolu);
            SemaOlustur();
        }
        await Task.CompletedTask.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
    }

    private bool SemaUygunMu()
    {
        try
        {
            if (!File.Exists(_dbYolu))
                return false;
            using var baglanti = BaglantiAc();
            var tablolar = new HashSet<string>(StringComparer.Ordinal);
            using (var cmd = baglanti.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                using var okuyucu = cmd.ExecuteReader();
                while (okuyucu.Read())
                    tablolar.Add(okuyucu.GetString(0));
            }
            if (!tablolar.Contains("Meta") || !tablolar.Contains("Madde") || !tablolar.Contains("Vektor"))
                return false;
            using (var cmd = baglanti.CreateCommand())
            {
                cmd.CommandText = "SELECT Deger FROM Meta WHERE Anahtar='SemaSurumu'";
                var deger = cmd.ExecuteScalar() as string;
                return string.Equals(deger, SemaSurumu, StringComparison.Ordinal);
            }
        }
        catch (SqliteException)
        {
            return false;
        }
    }

    private void SemaOlustur()
    {
        using var baglanti = BaglantiAc();
        using var cmd = baglanti.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE Meta (Anahtar TEXT PRIMARY KEY, Deger TEXT NOT NULL);
            CREATE TABLE Madde (
              Id INTEGER PRIMARY KEY,
              Anahtar TEXT NOT NULL,
              Sayfa TEXT NOT NULL,
              Baslik TEXT NOT NULL,
              Icerik TEXT NOT NULL,
              Etiketler TEXT NOT NULL DEFAULT '',
              IcerikHash TEXT NOT NULL
            );
            CREATE UNIQUE INDEX UX_Madde_Anahtar ON Madde(Anahtar);
            CREATE TABLE Vektor (
              MaddeId INTEGER NOT NULL REFERENCES Madde(Id) ON DELETE CASCADE,
              ModelAlias TEXT NOT NULL,
              Boyut INTEGER NOT NULL,
              Vektor BLOB NOT NULL,
              PRIMARY KEY (MaddeId, ModelAlias)
            );
            INSERT INTO Meta (Anahtar, Deger) VALUES ('SemaSurumu', '1');
            """;
        cmd.ExecuteNonQuery();
    }

    public async Task<IReadOnlyList<DepoMadde>> MaddeleriGetirAsync(CancellationToken ct = default)
    {
        var sonuc = new List<DepoMadde>();
        using var baglanti = BaglantiAc();
        using var cmd = baglanti.CreateCommand();
        cmd.CommandText = "SELECT Id, Anahtar, Sayfa, Baslik, Icerik, Etiketler, IcerikHash FROM Madde ORDER BY Id";
        using var okuyucu = cmd.ExecuteReader();
        while (okuyucu.Read())
        {
            ct.ThrowIfCancellationRequested();
            sonuc.Add(new DepoMadde(
                okuyucu.GetInt64(0), okuyucu.GetString(1), okuyucu.GetString(2),
                okuyucu.GetString(3), okuyucu.GetString(4), okuyucu.GetString(5), okuyucu.GetString(6)));
        }
        await Task.CompletedTask.ConfigureAwait(false);
        return sonuc;
    }

    /// <summary>Maddeyi Anahtar'a göre ekler/günceller; satır Id'sini döndürür.</summary>
    public async Task<long> MaddeYazAsync(string anahtar, string sayfa, string baslik, string icerik, string etiketler, string icerikHash, CancellationToken ct = default)
    {
        using var baglanti = BaglantiAc();
        using var cmd = baglanti.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Madde (Anahtar, Sayfa, Baslik, Icerik, Etiketler, IcerikHash)
            VALUES ($anahtar, $sayfa, $baslik, $icerik, $etiketler, $hash)
            ON CONFLICT(Anahtar) DO UPDATE SET Sayfa=$sayfa, Baslik=$baslik, Icerik=$icerik, Etiketler=$etiketler, IcerikHash=$hash;
            SELECT Id FROM Madde WHERE Anahtar=$anahtar;
            """;
        cmd.Parameters.AddWithValue("$anahtar", anahtar);
        cmd.Parameters.AddWithValue("$sayfa", sayfa);
        cmd.Parameters.AddWithValue("$baslik", baslik);
        cmd.Parameters.AddWithValue("$icerik", icerik);
        cmd.Parameters.AddWithValue("$etiketler", etiketler);
        cmd.Parameters.AddWithValue("$hash", icerikHash);
        var id = (long?)cmd.ExecuteScalar() ?? 0;
        await Task.CompletedTask.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
        return id;
    }

    /// <summary>Artık içerikte olmayan anahtarları (vektörleriyle) siler.</summary>
    public async Task OlmayanlariSilAsync(IReadOnlySet<string> gecerliAnahtarlar, CancellationToken ct = default)
    {
        using var baglanti = BaglantiAc();
        if (gecerliAnahtarlar is null || gecerliAnahtarlar.Count == 0)
        {
            using var temizle = baglanti.CreateCommand();
            temizle.CommandText = "DELETE FROM Vektor; DELETE FROM Madde;";
            temizle.ExecuteNonQuery();
        }
        else
        {
            var adlar = string.Join(", ", gecerliAnahtarlar.Select((_, i) => "$a" + i));
            using var cmd = baglanti.CreateCommand();
            cmd.CommandText = $"DELETE FROM Vektor WHERE MaddeId IN (SELECT Id FROM Madde WHERE Anahtar NOT IN ({adlar}));"
                + $" DELETE FROM Madde WHERE Anahtar NOT IN ({adlar});";
            int i = 0;
            foreach (var anahtar in gecerliAnahtarlar)
                cmd.Parameters.AddWithValue("$a" + i++, anahtar);
            cmd.ExecuteNonQuery();
        }
        await Task.CompletedTask.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
    }

    public async Task<IReadOnlyList<(long MaddeId, int Boyut, byte[] Vektor)>> VektorleriGetirAsync(string modelAlias, CancellationToken ct = default)
    {
        var sonuc = new List<(long, int, byte[])>();
        using var baglanti = BaglantiAc();
        using var cmd = baglanti.CreateCommand();
        cmd.CommandText = "SELECT MaddeId, Boyut, Vektor FROM Vektor WHERE ModelAlias=$alias";
        cmd.Parameters.AddWithValue("$alias", modelAlias ?? string.Empty);
        using var okuyucu = cmd.ExecuteReader();
        while (okuyucu.Read())
        {
            ct.ThrowIfCancellationRequested();
            sonuc.Add((okuyucu.GetInt64(0), okuyucu.GetInt32(1), (byte[])okuyucu.GetValue(2)));
        }
        await Task.CompletedTask.ConfigureAwait(false);
        return sonuc;
    }

    public async Task VektorYazAsync(long maddeId, string modelAlias, int boyut, byte[] vektor, CancellationToken ct = default)
    {
        using var baglanti = BaglantiAc();
        using var cmd = baglanti.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Vektor (MaddeId, ModelAlias, Boyut, Vektor)
            VALUES ($id, $alias, $boyut, $vektor)
            ON CONFLICT(MaddeId, ModelAlias) DO UPDATE SET Boyut=$boyut, Vektor=$vektor;
            """;
        cmd.Parameters.AddWithValue("$id", maddeId);
        cmd.Parameters.AddWithValue("$alias", modelAlias);
        cmd.Parameters.AddWithValue("$boyut", boyut);
        cmd.Parameters.AddWithValue("$vektor", vektor);
        cmd.ExecuteNonQuery();
        await Task.CompletedTask.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
    }

    public async Task<string?> MetaOkuAsync(string anahtar, CancellationToken ct = default)
    {
        using var baglanti = BaglantiAc();
        using var cmd = baglanti.CreateCommand();
        cmd.CommandText = "SELECT Deger FROM Meta WHERE Anahtar=$anahtar";
        cmd.Parameters.AddWithValue("$anahtar", anahtar);
        var deger = cmd.ExecuteScalar() as string;
        await Task.CompletedTask.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
        return deger;
    }

    public async Task MetaYazAsync(string anahtar, string deger, CancellationToken ct = default)
    {
        using var baglanti = BaglantiAc();
        using var cmd = baglanti.CreateCommand();
        cmd.CommandText = "INSERT INTO Meta (Anahtar, Deger) VALUES ($anahtar, $deger)"
            + " ON CONFLICT(Anahtar) DO UPDATE SET Deger=$deger;";
        cmd.Parameters.AddWithValue("$anahtar", anahtar);
        cmd.Parameters.AddWithValue("$deger", deger);
        cmd.ExecuteNonQuery();
        await Task.CompletedTask.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
    }

    private SqliteConnection BaglantiAc()
    {
        var baglanti = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _dbYolu,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString());
        baglanti.Open();
        using (var pragma = baglanti.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys=ON;";
            pragma.ExecuteNonQuery();
        }
        return baglanti;
    }

    /// <summary>float32 little-endian paketleme (sözleşme: Vektor BLOB).</summary>
    public static byte[] VektorPaketle(float[] vektor)
    {
        var bayt = new byte[vektor.Length * 4];
        for (int i = 0; i < vektor.Length; i++)
        {
            var parca = BitConverter.GetBytes(vektor[i]);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(parca);
            Buffer.BlockCopy(parca, 0, bayt, i * 4, 4);
        }
        return bayt;
    }

    /// <summary>float32 little-endian açma.</summary>
    public static float[] VektorAc(byte[] bayt, int boyut)
    {
        var vektor = new float[boyut];
        for (int i = 0; i < boyut && (i + 1) * 4 <= bayt.Length; i++)
        {
            var parca = new byte[4];
            Buffer.BlockCopy(bayt, i * 4, parca, 0, 4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(parca);
            vektor[i] = BitConverter.ToSingle(parca, 0);
        }
        return vektor;
    }
}
