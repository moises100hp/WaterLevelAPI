using System.Security.Cryptography;
using System.Text;

namespace WaterLevelAPI.Service
{
    public static class PasswordHelper
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Senha é obrigatória.");

            using var hmac = new HMACSHA512();
            var salt = Convert.ToBase64String(hmac.Key);
            var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            return (hash, salt);
        }

        public static bool VerifyPassword(string password, string hash, string salt)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                using var hmac = new HMACSHA512(saltBytes);
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(computedHash),
                    Convert.FromBase64String(hash));
            }
            catch
            {
                return false;
            }
        }

        public static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var password = new char[10];

            for (var i = 0; i < password.Length; i++)
            {
                password[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }

            return new string(password);
        }
    }
}
