using Core;
using System.Security.Cryptography;

namespace Server.Logic
{
    public class WatcherClient(string folder, string file) : Watcher(folder, file)
    {
        private static readonly HashAlgorithm hasher = SHA256.Create();
        private readonly string clientPath = Path.Combine(folder, file);

        public string ClientHash { get; private set; } =
            Scanner.GetHashString(hasher.ComputeHash(File.ReadAllBytes(Path.Combine(folder, file))));

        public override void UpdateList(Object sender, FileSystemEventArgs e)
        {
            ClientHash = Scanner.GetHashString(hasher.ComputeHash(File.ReadAllBytes(clientPath)));
        }
    }
}