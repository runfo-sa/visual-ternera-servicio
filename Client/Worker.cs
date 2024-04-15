using Client.Service;

namespace Client
{
    public class Worker(ILogger<Worker> logger, ConfigService config) : BackgroundService
    {
        private readonly ClientService _client = new(config, logger);
        private static readonly Mutex s_mutEtiquetas = new(true);
        private static readonly Mutex s_mutPiQuatro = new(true);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Task> tasks = [
                Task.Run(() => CheckEtiquetas(stoppingToken), stoppingToken),
                Task.Run(() => CheckPiQuatro(stoppingToken), stoppingToken),
                Task.Run(() => CheckUpdates(stoppingToken), stoppingToken),
            ];

            foreach (var task in tasks)
            {
                await task;
            }
        }

        private async Task CheckEtiquetas(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                s_mutEtiquetas.WaitOne();
                // Ejecuta la recompilación de etiquetas
                await _client.SendEtiquetas();
                s_mutEtiquetas.ReleaseMutex();

                // Si el tiempo intervalo no fue configurado se asiga a 10 minutos por default
                double interval = config.Data.App?.IntervaloMins ?? 10.0;

                await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
            }
        }

        private async Task CheckPiQuatro(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                s_mutPiQuatro.WaitOne();
                // await _client.CheckPiQuatro()
                s_mutPiQuatro.ReleaseMutex();

                // Si el tiempo intervalo no fue configurado se asiga a 10 minutos por default
                double interval = config.Data.App?.IntervaloMins ?? 10.0;

                await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
            }
        }

        private async Task CheckUpdates(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                s_mutEtiquetas.WaitOne();
                s_mutPiQuatro.WaitOne();

                //await _client.GetUpdate();

                s_mutPiQuatro.ReleaseMutex();
                s_mutEtiquetas.ReleaseMutex();

                // Si el tiempo intervalo no fue configurado se asiga a 10 minutos por default
                double interval = config.Data.App?.IntervaloMins ?? 10.0;

                await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
            }
        }
    }
}