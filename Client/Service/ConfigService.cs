using Tomlyn;

namespace Client.Service
{
    public class ConfigService
    {
        private const string DEFAULT_CONFIG = "[server]\r\nip = \"localhost\"\r\nport = \"5000\"\r\n\r\n[app]\r\nintervalo_mins = 180\r\nunidad = \"C:\"";

        private readonly string _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Visual Ternera Service\\config.toml"
        );

        public ConfigModel Data { get; set; }

        public ConfigService()
        {
            if (Path.Exists(_configPath))
            {
                string configDump = File.ReadAllText(_configPath);
                Data = Toml.ToModel<ConfigModel>(configDump);
            }
            else
            {
                Data = Toml.ToModel<ConfigModel>(DEFAULT_CONFIG);
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
                File.WriteAllText(_configPath, DEFAULT_CONFIG);
            }
        }

        public void Save()
        {
            string configDump = Toml.FromModel(Data);
            File.WriteAllText(_configPath, configDump);
        }
    }

    public class ConfigModel
    {
        public Server? Server { get; set; }
        public App? App { get; set; }
    }

    public class Server
    {
        public string Ip { get; set; }
        public string Port { get; set; }
    }

    public class App
    {
        public int IntervaloMins { get; set; }
        public string Unidad { get; set; }
        public string? PiPath { get; set; }
    }
}