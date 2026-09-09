using System.Security.Cryptography;

namespace MuhasibPro.Business.Infrastructure.Security;

/// <summary>Eski PBKDF2$ hash'leri için salt-okunur doğrulayıcı — yeni parolalar Identity hasher ile yazılır.</summary>
public static class LegacyPbkdf2Verifier
{
    private const int SaltSize = 16;

    public static bool Verify(string password, string hashedPassword)
    {
        try
        {
            var parts = hashedPassword.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2") return false;
            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var storedHash = Convert.FromBase64String(parts[3]);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(storedHash.Length);
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch { return false; }
    }
}
