using System.Security.Cryptography;
using System.Text;
using UniApp.Api.Domain.Services;

namespace UniApp.Api.Infrastructure.Security;

/// <summary>
/// PBKDF2-SHA256 com salt aleatorio por senha (100.000 iteracoes).
/// Formato: $uniapp$pbkdf2-sha256$100000$base64(salt)$base64(hash)
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const string Prefix = "$uniapp$pbkdf2-sha256$";

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt, Iterations);
        return $"{Prefix}{Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string stored, string password)
    {
        if (string.IsNullOrEmpty(stored) || password is null)
            return false;

        if (!stored.StartsWith(Prefix, StringComparison.Ordinal))
        {
            // Compatibilidade com registros antigos gravados em texto claro.
            return FixedTimeEquals(Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(password));
        }

        try
        {
            var parts = stored.Split('$');
            // ["", "uniapp", "pbkdf2-sha256", "100000", salt, hash]
            if (parts.Length != 6 || !int.TryParse(parts[3], out var iterations))
                return false;

            var salt = Convert.FromBase64String(parts[4]);
            var expected = Convert.FromBase64String(parts[5]);
            var actual = Derive(password, salt, iterations);
            return FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Derive(string password, byte[] salt, int iterations)
        => Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            HashSize);

    private static bool FixedTimeEquals(byte[] left, byte[] right)
        => left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
}
