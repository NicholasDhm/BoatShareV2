namespace boat_share.Services
{
	public class NextRunService
	{

		public static TimeSpan GetTimeToNextRun(DateTime now)
		{
			// Calculate the time until 23:59:59 of the current day
			//var nextRun = new DateTime(now.Year, now.Month, now.Day, 23, 59, 0, 0); // 23:59 today
			var nextRun = new DateTime(now.Year, now.Month, now.Day, 2, 59, 0, 0, DateTimeKind.Utc); // 23:59 today

            if (now > nextRun)
			{
				// If it's already past 23:59, schedule for tomorrow
				nextRun = nextRun.AddDays(1);
			}

			return nextRun - now;
		}
	}
}
