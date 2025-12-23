using System.Security.Cryptography;

namespace ARBISTO_POS.Services
{
    public class PasswordHasher
    {
        // PBKDF2 with HMACSHA256
        public static void CreateHash(string password, out byte[] hash, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(16);
            hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        }

        public static bool Verify(string password, byte[] hash, byte[] salt)
        {
            var test = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(test, hash);
        }
    }
}
