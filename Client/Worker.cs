using Client.Service;

namespace Client
{
    public class Worker(ILogger<Worker> logger, ConfigService config) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly ClientService _client = new(config);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Ejecuta la recompilación de etiquetas
                    await _client.SendEtiquetas();

                    // Si el tiempo intervalo no fue configurado se asiga a 10 minutos por default
                    double interval = config.Data.App?.IntervaloMins ?? 10.0;

                    await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                Reporter.ReportError(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
