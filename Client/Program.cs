using Client;
using Client.Service;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsService(options => options.ServiceName = "Visual Ternera - Controlador de Etiquetas");

            builder.Services.AddSingleton<ConfigService>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
        catch (Exception ex)
        {
            Reporter.ReportError(ex.Message);
            Environment.Exit(1);
        }
    }
}