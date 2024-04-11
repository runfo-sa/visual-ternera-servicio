using Client.Model;
using Core;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Client
{
    public sealed class ClientService
    {
        private readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly HttpClient httpClient = new();
        private readonly string Ip = null!;
        private readonly bool okay = true;
        private readonly Config config = null!;

        public ClientService(Config config)
        {
            this.config = config;

            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    Ip = ip.ToString();
                    break;
                }
            }

            if (config.Data.App!.PiPath is null)
            {
                try
                {
                    config.Data.App.PiPath = FindPiQuatro(config.Data.App.Unidad);
                    config.Save();
                }
                catch (MultipleInstalls err)
                {
                    _ = SendMultipleInstalls();
                    ReportError(err.Message + "\n" + string.Join('\n', err.Paths));
                    okay = false;
                }
                catch (Exception err)
                {
                    ReportError(err.Message);
                    okay = false;
                }
            }
        }

        public async Task SendEtiquetas()
        {
            if (okay)
            {
                Etiqueta[] etiquetas = Scanner.GetEtiquetas(Scanner.TEST_PATH);
                await Post("/validarcliente", etiquetas);
            }
        }

        public async Task SendMultipleInstalls()
        {
            await Post("/multiplesinstalaciones");
        }

        private static void ReportError(string msg)
        {
            Console.Error.WriteLine($"{msg}");

            var commonpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(commonpath, "VisualTerneraService\\");
            var file = Logger.Log(path, msg);

            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select, \"{0}\"", file));
        }

        private async Task Post(string route, Etiqueta[]? etiquetas = null)
        {
            try
            {
                StringContent jsonBody = new(JsonSerializer.Serialize(
                    new Core.Client(Ip, etiquetas ?? []), jsonOptions),
                    Encoding.ASCII,
                    "application/json"
                );
                jsonBody.Headers.Add("request-key", "ABC123");

                string uri = string.Format("https://{0}:{1}{2}", config.Data.Server!.Ip, config.Data.Server!.Port, route);
                HttpResponseMessage response = await httpClient.PostAsync(uri, jsonBody);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                ReportError(err.Message);
            }
        }

        private static string FindPiQuatro(string unidad)
        {
            DateTime date = DateTime.Now;
            string[] files = Array.FindAll(
                Directory.GetFiles(unidad + "\\", "PiQuatro.exe", new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true,
                }),
                f => File.GetLastWriteTime(f) > date.AddYears(-1) && !f.Contains("test", StringComparison.CurrentCultureIgnoreCase)
            );

            if (files.Length == 0)
            {
                throw new NoInstallsFound();
            }
            else if (files.Length > 1)
            {
                throw new MultipleInstalls(files);
            }

            return files[0];
        }

        private class NoInstallsFound : Exception
        {
            public NoInstallsFound() : base("No se encontro ninguna instalación de PiQuatro") { }
        }

        private class MultipleInstalls(string[] paths) : Exception("Se encontraron mas de una instalación de PiQuatro")
        {
            public string[] Paths = paths;
        }
    }
}
