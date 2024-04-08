using Client;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddWindowsService(options => options.ServiceName = "Visual Ternera - Controlador");

        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

        builder.Services.AddSingleton<ClientService>();
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}