using Tomlyn;
using Tomlyn.Model;

namespace Client.Service
{
    public class ConfigService
    {
        private const string DEFAULT_CONFIG = "[server]\r\nip = \"localhost\"\r\nport = \"5000\"\r\n\r\n[app]\r\n# Unidad en la que se busca la instalacion de PiQuatro\r\nunidad = \"C:\"\r\n# Hora del dia en la que se actualizara el servicio (formato 24hs)\r\nupdate_time = 0\r\n# Hora del dia en la que analiza por multiples instalaciones de PiQuatro (formato 24hs)\r\npiquatro_time = 2\r\n# Tiempo (en minutos) para enviar las etiquetas al servidor\r\nintervalo_mins = 180\r\n\r\n\r\n[auth]\r\nclave_publica = \"ABC123\"\r\nclave_privada = \"QWE987\"\r\nclave_descarga = \"ABC123\"\r\n";

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

    public class ConfigModel : ITomlMetadataProvider
    {
        public Server? Server { get; set; }
        public App? App { get; set; }
        public Auth? Auth { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }

    public class Server : ITomlMetadataProvider
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }

    public class App : ITomlMetadataProvider
    {
        public string Unidad { get; set; }
        public int UpdateTime { get; set; }
        public int PiquatroTime { get; set; }
        public int IntervaloMins { get; set; }
        public string? PiPath { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }

    public class Auth : ITomlMetadataProvider
    {
        public string ClavePublica { get; set; }
        public string ClavePrivada { get; set; }
        public string ClaveDescarga { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }
}