using boat_share.Abstract;

namespace boat_share.Services
{
    /// <summary>
    /// Background service that automatically runs reservation status updates
    /// Runs every hour to mark past reservations as Legacy and restore quotas
    /// </summary>
    public class ReservationCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ReservationCleanupBackgroundService> _logger;

        // Run every hour by default (configurable)
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public ReservationCleanupBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ReservationCleanupBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reservation Cleanup Background Service started. Running every {Interval}", _interval);

            // Run immediately on startup
            await ProcessReservationUpdates(stoppingToken);

            // Then run periodically
            using var timer = new PeriodicTimer(_interval);

            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessReservationUpdates(stoppingToken);
            }

            _logger.LogInformation("Reservation Cleanup Background Service stopped");
        }

        private async Task ProcessReservationUpdates(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogDebug("Starting reservation status update check at {Time}", DateTime.UtcNow);

                using var scope = _serviceScopeFactory.CreateScope();
                var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

                await reservationService.UpdateReservationStatusesAsync();

                _logger.LogDebug("Reservation status update completed successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                // Log error but don't crash the service - it will retry on next interval
                _logger.LogError(ex, "Error occurred while updating reservation statuses at {Time}", DateTime.UtcNow);
            }
        }
    }
}
