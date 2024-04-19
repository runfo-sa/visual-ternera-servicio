using System.Net.NetworkInformation;

namespace Core
{
    public static class Network
    {
        public static string GetIpAddress()
        {
            var ips = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .SelectMany(netif => netif.GetIPProperties().UnicastAddresses.Select(unicast => unicast.Address));

            var gateway = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Select(card =>
                        card
                         .GetIPProperties()
                         .GatewayAddresses
                         .FirstOrDefault(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                    .Where(g => g is not null)
                    .Select(g => g!.Address)
                    .ToArray()[0];

            string networkAddress = gateway.ToString()[..^(gateway.ToString().IndexOf('.'))];
            return ips.First(ip => ip.ToString().Contains(networkAddress)).ToString()!;
        }
    }
}