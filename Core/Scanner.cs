using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public static class Scanner
    {
        public const string TEST_PATH = "C:\\soft\\PiQuatroRunfo\\Etiquetas";
        private static readonly HashAlgorithm hasher = SHA256.Create();

        public static List<Etiqueta> GetEtiquetas(string path)
        {
            string[] files = Directory.GetFiles(path);
            List<Etiqueta> etiquetas = new(files.Length);

            foreach (var f in files)
            {
                if (Path.GetExtension(f).Equals(".e01", StringComparison.CurrentCultureIgnoreCase))
                {
                    string name = Path.GetFileNameWithoutExtension(f);
                    string date = File.GetLastWriteTime(f).ToString();
                    string hash = GetHashString(hasher.ComputeHash(File.ReadAllBytes(f)));
                    etiquetas.Add(new Etiqueta(name, date, hash));
                }
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
