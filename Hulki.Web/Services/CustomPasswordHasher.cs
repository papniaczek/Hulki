using System;
using System.Security.Cryptography;
using System.Text;

namespace Hulki.Web.Services;

/// <summary>
/// Ręczne hashowanie haseł – PBKDF2 z SHA-256, 100 000 iteracji, 32-bajtowa sól.
/// Nie używa ASP.NET Identity ani żadnego zewnętrznego NuGeta.
/// Format przechowywanego ciągu: base64(sól):base64(hash)
/// </summary>
public static class CustomPasswordHasher
{
    private const int SaltSize       = 32;   // bajty
    private const int HashSize        = 32;   // bajty (SHA-256 output)
    private const int Iterations      = 100_000;
    private const char Separator      = ':';

    /// <summary>
    /// Hashuje hasło i zwraca ciąg gotowy do zapisu w kolumnie PasswordHash.
    /// </summary>
    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Hasło nie może być puste.", nameof(password));

        // Generuj kryptograficznie bezpieczną sól
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        var hash = Pbkdf2(password, salt);

        return $"{Convert.ToBase64String(salt)}{Separator}{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Weryfikuje hasło względem przechowywanego hasha.
    /// Zwraca true jeśli hasło jest poprawne.
    /// </summary>
    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        var parts = storedHash.Split(Separator);
        if (parts.Length != 2) return false;

        try
        {
            var salt         = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);
            var actualHash   = Pbkdf2(password, salt);

            // Porównanie stałoczasowe – zapobiega atakom czasowym
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    // ─── prywatne ──────────────────────────────────────────────────────────

    private static byte[] Pbkdf2(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        return Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);
    }
}
