using Tomlyn;

namespace Client.Model
{
    public class Config
    {
        public ConfigModel Data { get; set; }

        public Config()
        {
            string configDump = File.ReadAllText("C:\\Users\\Agustin.Marco\\Projects\\Apps\\C#\\visual_ternera\\Service\\Client\\config.toml");
            Data = Toml.ToModel<ConfigModel>(configDump);
        }

        public void Save()
        {
            string configDump = Toml.FromModel(Data);
            File.WriteAllText("C:\\Users\\Agustin.Marco\\Projects\\Apps\\C#\\visual_ternera\\Service\\Client\\config.toml", configDump);
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
