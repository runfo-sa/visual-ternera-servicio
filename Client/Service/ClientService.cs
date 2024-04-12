using Core;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Client.Service
{
    /// <summary>
    /// Servicio principal, se encarga de reportar toda la información necesaria al Servidor.
    /// </summary>
    public sealed class ClientService
    {
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly ConfigService _config = null!;
        private readonly HttpClient _client = new();
        private readonly string _ip = null!;

        // Indica el estado de este cliente
        private readonly bool _error = false;
        private readonly string? _errorMsg;

        public ClientService(ConfigService config)
        {
            _config = config;

            // Busca la IPv4 de esta maquina
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    _ip = ip.ToString();
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
                    _error = true;
                    _errorMsg = err.Message + "\n" + string.Join('\n', err.Paths);
                }
                catch (Exception err)
                {
                    _error = true;
                    _errorMsg = err.Message;
                }
            }
        }

        public async Task SendEtiquetas()
        {
            if (_error)
            {
                throw new Exception(_errorMsg);
            }
            Etiqueta[] etiquetas = Scanner.GetEtiquetas(_config.Data.App!.PiPath!);
            await Post("/validarcliente", etiquetas);
        }

        public async Task SendMultipleInstalls()
        {
            await Post("/multiplesinstalaciones");
        }

        /// <summary>
        /// Realiza una API Request del tipo POST al endpoint indicado.
        /// <br/>
        /// Las lista de etiquetas es opcional.
        /// </summary>
        private async Task Post(string route, Etiqueta[]? etiquetas = null)
        {
            try
            {
                StringContent jsonBody = new(
                    JsonSerializer.Serialize(new Request(_ip, etiquetas), _jsonOptions),
                    Encoding.ASCII,
                    "application/json"
                );

                jsonBody.Headers.Add("request-key", "ABC123");
                jsonBody.Headers.Add("request-hash", Encryption.EncryptKey("ABC123"));

                string uri = string.Format(
                    "https://{0}:{1}{2}",
                    _config.Data.Server!.Ip,
                    _config.Data.Server!.Port,
                    route
                );

                HttpResponseMessage response = await _client.PostAsync(uri, jsonBody);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                Reporter.ReportError(err.Message, false);
            }
        }

        /// <summary>
        /// Busca la instalación de <b>PiQuatro</b> en la <paramref name="unidad"/> de disco especificada.
        /// </summary>
        /// <returns>Dirección donde PiQuatro guarda las etiquetas</returns>
        /// <exception cref="NoInstallsFound"/>
        /// <exception cref="MultipleInstalls"/>
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

            return Directory.GetParent(files[0])!.FullName + "\\Etiquetas";
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
