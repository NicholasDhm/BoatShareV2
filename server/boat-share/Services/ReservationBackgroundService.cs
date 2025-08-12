using NLog;
using boat_share.Services;
using boat_share.UseCases;

namespace boat_share.Api.Core.BackgroundReservation
{
	public class ReservationBackgroundService : BackgroundService
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private readonly int intervalToDelayStartupMs = 15 * 1000; // 15 seconds delay to allow system startup

		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ReservationBackgroundService(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// Start the task asynchronously
			Task.Run(async () =>
			{
				await ExecuteInternalAsync(stoppingToken);
			});
			return Task.CompletedTask;
		}

		private async Task ExecuteInternalAsync(CancellationToken stoppingToken)
		{
			Logger.Debug($"ReservationBackgroundService is starting");

			stoppingToken.Register(() =>
			{
				Logger.Debug($"ReservationBackgroundService is stopping");
			});

			// Delay the first run to allow system startup, database connection, etc.
			await Task.Delay(intervalToDelayStartupMs);

			while (!stoppingToken.IsCancellationRequested)
			{
				await ExecuteInternalLoopAsync(stoppingToken);
			}

			Logger.Debug($"ReservationBackgroundService stopped");
		}

		private async Task ExecuteInternalLoopAsync(CancellationToken stoppingToken)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var serviceProvider = scope.ServiceProvider;
				var reservationService = serviceProvider.GetService<ReservationService>();
				var deleteAllPastReservationsUseCase = serviceProvider.GetService<DeleteAllPastReservationsUseCase>();

                try
				{
					// Calculate the remaining time until 23:59 today
					var timeToNextRun = NextRunService.GetTimeToNextRun(DateTime.UtcNow);
					Logger.Debug($"Time to next run: {timeToNextRun}");

					// Wait until 23:59 of the current day
					await Task.Delay(timeToNextRun, stoppingToken);

					// Now it's 23:59 or shortly after, so we process reservations
					Logger.Debug($"ReservationBackgroundService processing reservations at {DateTime.UtcNow}");

					var thisRun = DateTime.UtcNow;
					Logger.Debug($"Processing reservations at: {thisRun}");

					await deleteAllPastReservationsUseCase!.Execute();

					await reservationService!.UpdateAllReservations();
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "Unexpected exception in ReservationBackgroundService");
				}
			}
		}
	}
}
