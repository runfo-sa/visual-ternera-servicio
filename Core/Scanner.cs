using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public static class Scanner
    {
        private static readonly HashAlgorithm hasher = SHA256.Create();

        /// <summary>
        /// Escanea <paramref name="path"/> en busca de archivos de etiquetas (con extension '.e01')
        /// </summary>
        /// <param name="path">Dirección a escanear</param>
        public static Etiqueta[] GetEtiquetas(string path)
        {
            string[] files = Directory.GetFiles(path, "*.e01");
            Etiqueta[] etiquetas = new Etiqueta[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                var f = files[i];

                string name = Path.GetFileNameWithoutExtension(f).ToLower();
                string hash = GetHashString(hasher.ComputeHash(File.ReadAllBytes(f)));

                etiquetas[i] = new Etiqueta(hash, name);
            }

            return etiquetas;
        }

        /// <summary>
        /// Convierte una array de <see cref="byte"/> en una <see cref="string"/> hexadecimal
        /// </summary>
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