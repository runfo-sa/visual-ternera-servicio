using Core;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Client.Service
{
    /// <summary>
    /// Servicio principal, se encarga de reportar toda la información necesaria al Servidor.
    /// </summary>
    public sealed class ClientService
    {
        internal class HttpClientHandlerInsecure : HttpClientHandler
        {
            internal HttpClientHandlerInsecure()
            {
                ServerCertificateCustomValidationCallback = DangerousAcceptAnyServerCertificateValidator;
            }
        }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly string _ip = null!;
        private string[] _foundInstallations = null!;
        private readonly ConfigService _config = null!;
        private readonly HttpClient _client = new(new HttpClientHandlerInsecure());

        public ClientService(ConfigService config)
        {
            _config = config;

            // Busca la IPv4 de esta maquina
            _ip = Network.GetIpAddress();

            if (config.Data.App!.PiPath is null)
            {
                _ = CheckPiQuatroAsync();
            }
        }

        public async Task SendEtiquetas()
        {
            Etiqueta[] etiquetas = Scanner.GetEtiquetas(_config.Data.App!.PiPath!);
            await Post("/validarcliente", etiquetas);
        }

        public async Task SendMultipleInstalls()
        {
            await Post("/multiplesinstalaciones", msg: string.Join(Environment.NewLine, _foundInstallations));
        }

        public async Task SendNoInstalls()
        {
            await Post("/noinstalado");
        }

        /// <summary>
        /// Realiza una API Request del tipo POST al endpoint indicado.
        /// <br/>
        /// Las lista de etiquetas es opcional.
        /// </summary>
        private async Task Post(string route, Etiqueta[]? etiquetas = null, string? msg = null)
        {
            try
            {
                StringContent jsonBody = new(
                    JsonSerializer.Serialize(new Request(_ip, etiquetas, msg), _jsonOptions),
                    Encoding.ASCII,
                    "application/json"
                );

                jsonBody.Headers.Add("request-key", _config.Data.Auth!.ClavePublica);
                jsonBody.Headers.Add("request-hash", Encryption.EncryptKey(_config.Data.Auth!.ClavePublica, _config.Data.Auth!.ClavePrivada));

                string uri = $"http://{_config.Data.Server!.Ip}:{_config.Data.Server!.Port}{route}";
                HttpResponseMessage response = await _client.PostAsync(uri, jsonBody);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                Reporter.ReportError(err.Message);
            }
        }

        public async Task CheckPiQuatroAsync()
        {
            try
            {
                _config.Data.App!.PiPath = FindPiQuatro(_config.Data.App.Unidad);
                _config.Save();
            }
            catch (MultipleInstalls)
            {
                await SendMultipleInstalls();
                throw;
            }
            catch (NoInstallsFound)
            {
                await SendNoInstalls();
                throw;
            }
        }

        /// <summary>
        /// Busca la instalación de <b>PiQuatro</b> en la <paramref name="unidad"/> de disco especificada.
        /// </summary>
        /// <returns>Dirección donde PiQuatro guarda las etiquetas</returns>
        /// <exception cref="NoInstallsFound"/>
        /// <exception cref="MultipleInstalls"/>
        private string FindPiQuatro(string unidad)
        {
            DateTime date = DateTime.Now;
            _foundInstallations = Array.FindAll(
                Directory.GetFiles(unidad + "\\", "PiQuatro.exe", new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true,
                }),
                f => File.GetLastWriteTime(f) > date.AddYears(-1) && !f.Contains("test", StringComparison.CurrentCultureIgnoreCase)
            );

            if (_foundInstallations.Length == 0)
            {
                throw new NoInstallsFound();
            }
            else if (_foundInstallations.Length > 1)
            {
                throw new MultipleInstalls(_foundInstallations);
            }

            return Directory.GetParent(_foundInstallations[0])!.FullName + "\\Etiquetas";
        }

        private class NoInstallsFound() : Exception("No se encontro ninguna instalación de PiQuatro")
        { }

        private class MultipleInstalls(string[] paths) : Exception("Se encontraron mas de una instalación de PiQuatro")
        {
            public string[] Paths = paths;
        }

        public async Task GetUpdate()
        {
            try
            {
                string uri = $"http://{_config.Data.Server!.Ip}:{_config.Data.Server!.Port}/clienteversion";
                HttpResponseMessage response = await _client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                string serverHash = await response.Content.ReadAsStringAsync();
                string localHash = Scanner.GetHashString(SHA256.HashData(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "Client.exe")));

                if (localHash != serverHash.Trim('"'))
                {
                    uri = $"http://{_config.Data.Server!.Ip}:{_config.Data.Server!.Port}/instalador?key={_config.Data.Auth!.ClaveDescarga}";
                    response = _client.GetAsync(uri).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    string filename = response.Content.Headers.ContentDisposition!.FileName!;
                    string path = Path.Combine(Path.GetTempPath(), "VSTCTemp");
                    string filepath = Path.Combine(path, filename);
                    Directory.CreateDirectory(path);

                    using (var fs = new FileStream(filepath, FileMode.Create))
                    {
                        response.Content.CopyToAsync(fs).GetAwaiter().GetResult();
                    }

                    using var process = new Process();
                    process.StartInfo = new ProcessStartInfo("powershell.exe", $"-ExecutionPolicy Bypass -File \"{filepath}\"");
                    process.Start();

                    Environment.Exit(0);
                }
            }
            catch (Exception err)
            {
                Reporter.ReportError(err.Message);
            }
        }
    }
}