using Tomlyn;

namespace Client.Service
{
    public class ConfigService
    {
        public ConfigModel Data { get; set; }

        public ConfigService()
        {
            string configDump = File.ReadAllText("config.toml");
            Data = Toml.ToModel<ConfigModel>(configDump);
        }

        public void Save()
        {
            string configDump = Toml.FromModel(Data);
            File.WriteAllText("config.toml", configDump);
        }
    }

    public class ConfigModel
    {
        public Server? Server { get; set; }
        public App? App { get; set; }
    }

    public class Server
    {
        public required string Ip { get; set; }
        public required string Port { get; set; }
    }

    public class App
    {
        public required int IntervaloMins { get; set; }
        public required string Unidad { get; set; }
        public string? PiPath { get; set; }
    }
}
