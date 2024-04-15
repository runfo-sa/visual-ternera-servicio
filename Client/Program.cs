using Client;
using Client.Service;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

internal class Program
{
    private static void Main(string[] args)
    {
        ILogger _logger = LoggerFactory.Create(builder => builder.AddEventLog()).CreateLogger("Program");

        try
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsService(options => options.ServiceName = "Visual Ternera - Controlador de Etiquetas");

            LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

            builder.Services.AddSingleton<ConfigService>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            Reporter.ReportError(ex.Message);
            Environment.Exit(1);
        }
    }
}