using System.Security.Cryptography;

namespace eMarketing.Service.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string storedPassword);
    bool NeedsMigration(string storedPassword);
}

public sealed class PasswordService : IPasswordService
{
    private const string Prefix = "PBKDF2";
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string storedPassword)
    {
        if (string.IsNullOrWhiteSpace(storedPassword))
            return false;

        if (!storedPassword.StartsWith(Prefix + "$", StringComparison.Ordinal))
            return string.Equals(password?.Trim(), storedPassword.Trim(), StringComparison.Ordinal);

        string[] parts = storedPassword.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations))
            return false;

        byte[] salt = Convert.FromBase64String(parts[2]);
        byte[] expectedHash = Convert.FromBase64String(parts[3]);
        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    public bool NeedsMigration(string storedPassword)
    {
        return !string.IsNullOrWhiteSpace(storedPassword)
            && !storedPassword.StartsWith(Prefix + "$", StringComparison.Ordinal);
    }
}
