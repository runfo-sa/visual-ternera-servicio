using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public static class Scanner
    {
        public const string TEST_PATH = "C:\\soft\\PiQuatroRunfo\\Etiquetas";
        private static readonly HashAlgorithm hasher = SHA256.Create();

        public static Etiqueta[] GetEtiquetas(string path)
        {
            string[] files = Directory.GetFiles(path, "*.e01");
            Etiqueta[] etiquetas = new Etiqueta[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                var f = files[i];

                string name = Path.GetFileNameWithoutExtension(f).ToLower();
                string date = File.GetLastWriteTime(f).ToString();
                string hash = GetHashString(hasher.ComputeHash(File.ReadAllBytes(f)));
                etiquetas[i] = new Etiqueta(hash, date, name);
            }

            return etiquetas;
        }

        private static string GetHashString(byte[] bytes)
        {
            StringBuilder sb = new();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
