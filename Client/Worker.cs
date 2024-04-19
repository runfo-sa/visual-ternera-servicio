using Client.Service;

namespace Client
{
    public class Worker(ConfigService config) : BackgroundService
    {
        private readonly ClientService _client = new(config);
        private static readonly Mutex s_mutPiQuatro = new(false, @"Local\PiQuatroMutx");
        private static readonly Mutex s_mutEtiquetas = new(false, @"Local\EtiquetasMutx");

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
                _client.SendEtiquetas().Wait(stoppingToken);
                s_mutEtiquetas.ReleaseMutex();

                // Si el tiempo intervalo no fue configurado se asiga a 3 horas por default.
                double interval = config.Data.App?.IntervaloMins ?? 180.0;
                await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
            }
        }

        private async Task CheckPiQuatro(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Calcula el tiempo que falta hasta las 2 de la mañana.
                DateTime midnight = DateTime.Today.AddDays(1).AddHours(2);
                double remaining = midnight.Subtract(DateTime.Now).TotalMinutes;
                await Task.Delay(TimeSpan.FromMinutes(remaining), stoppingToken);

                s_mutPiQuatro.WaitOne();
                _client.CheckPiQuatroAsync().Wait(stoppingToken);
                s_mutPiQuatro.ReleaseMutex();
            }
        }

        private async Task CheckUpdates(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Calcula el tiempo que falta hasta la media noche.
                DateTime midnight = DateTime.Today.AddDays(1);
                double remaining = midnight.Subtract(DateTime.Now).TotalMinutes;
                await Task.Delay(TimeSpan.FromMinutes(remaining), stoppingToken);

                s_mutEtiquetas.WaitOne();
                s_mutPiQuatro.WaitOne();

                _client.GetUpdate().Wait(stoppingToken);

                s_mutPiQuatro.ReleaseMutex();
                s_mutEtiquetas.ReleaseMutex();
            }
        }
    }
}