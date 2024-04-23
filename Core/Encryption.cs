using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public static class Encryption
    {
        public static string EncryptKey(string publicKey, string privateKey)
        {
            HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(privateKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(publicKey));
            return Convert.ToBase64String(hash);
        }
    }
}