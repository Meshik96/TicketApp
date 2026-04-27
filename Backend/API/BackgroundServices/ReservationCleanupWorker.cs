using Application.Interfaces.Persistence.Seats;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
public class ReservationCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private readonly ILogger<ReservationCleanupWorker> _logger;

    public ReservationCleanupWorker(IServiceProvider serviceProvider, ILogger<ReservationCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de limpieza iniciado correctamente.");
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var seatCommand = scope.ServiceProvider.GetRequiredService<ISeatCommands>();
                try
                {
                    await seatCommand.DeleteExpiredReservationsAsync();
                    _logger.LogInformation($"Revisión ejecutada a las: {DateTime.UtcNow}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fallo al intentar eliminar reservas expiradas.");
                }
            }
            // Espera 1 minuto antes de la siguiente revisión
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}