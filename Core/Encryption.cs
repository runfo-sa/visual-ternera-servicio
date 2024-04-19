using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public static class Encryption
    {
        // TODO! Deffinir las claves
        public const string PUBLIC_KEY = "ABC123";

        public const string DOWNLOAD_KEY = "ABC123";

        private const string PRIVATE_KEY = "QWERTY987";

        public static string EncryptKey(string publicKey = PUBLIC_KEY)
        {
            HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(PRIVATE_KEY));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(publicKey));
            return Convert.ToBase64String(hash);
        }
    }
}