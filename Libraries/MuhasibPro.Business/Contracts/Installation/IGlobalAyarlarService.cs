namespace MuhasibPro.Business.Contracts.Installation;

public interface IGlobalAyarlarService
{
    Task<string?> GetAsync(string anahtar);
    Task SetAsync(string anahtar, string deger, string? aciklama = null);
    Task<string> GetOrCreateAsync(string anahtar, Func<string> factory, string? aciklama = null);
}
