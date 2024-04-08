using Core;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Client
{
    public sealed class ClientService
    {
        private const string API_URI = "https://localhost:7164/validateClient";

        private readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly HttpClient httpClient = new();
        private readonly string Ip = null!;

        public ClientService()
        {
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    this.Ip = ip.ToString();
                    break;
                }
            }
        }

        public async Task SendEtiquetas()
        {
            List<Etiqueta> etiquetas = Scanner.GetEtiquetas(Scanner.TEST_PATH);
            StringContent jsonBody = new(JsonSerializer.Serialize(
                new Core.Client(Ip, etiquetas), jsonOptions),
                Encoding.ASCII,
                "application/json"
            );

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(API_URI, jsonBody);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                Console.Error.WriteLine($"{err}");
            }
        }
    }
}
