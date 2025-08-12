using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using boat_share.Models;

namespace boat_share.Services
{
    public class ReservationService
    {
        private readonly IDynamoDBContext _context;

        private readonly UserService _userService;

        private readonly BoatService _boatService;

		private readonly ReservationDataService _reservationDataService;

        public ReservationService(
			IDynamoDBContext context,
			UserService userService,
			BoatService boatService,
			ReservationDataService reservationDataService
		)
        {
            _context = context;
            _userService = userService;
            _boatService = boatService;
			_reservationDataService = reservationDataService;
        }

        // Method to add a reservation based on ReservationDBO
        public async Task<string> AddReservation(ReservationDBO reservationDbo)
        {
            // Fetch the user based on reservationDbo.UserId
            var user = await _context.LoadAsync<User>(reservationDbo.UserId);

            // Handle quota deduction logic
            if (reservationDbo.Type == "Standard")
            {
                if (user.StandardQuota < 1)
                {
                    return "Insufficient quotas.";
                }

                user.StandardQuota -= 1;
            } else if (reservationDbo.Type == "Substitution")
			{
				if (user.SubstitutionQuota < 1)
				{
					return "Insufficient quotas.";
				}

				user.SubstitutionQuota -= 1;
			} else
			{
				if (user.ContingencyQuota < 1)
				{
					return "Insufficient quotas.";
				}

				user.ContingencyQuota -= 1;
			}
			await _userService.UpdateUser(user);

			var today = DateTime.Now;

			// Create a new Reservation object
			var reservation = new Reservation
			{
				ReservationId = Guid.NewGuid().ToString(), // Generate a new ReservationId
				UserId = reservationDbo.UserId,
				BoatId = reservationDbo.BoatId,
				Year = reservationDbo.Year,
				Month = reservationDbo.Month,
				Day = reservationDbo.Day,
				Type = reservationDbo.Type,
				Status = reservationDbo.Type == "Contingency" ? "Confirmed" : "Pending",
				CreatedAtIsoDate = today.ToString("MM/dd/yyyy h:mm:ss tt") // Custom format with AM/PM
			};

            await _reservationDataService.UpdateReservation(reservation);
            await CheckConfirmation(reservation);

            return "Reservation successfully created.";
        }

		public async Task CheckConfirmation(Reservation reservation)
		{
			if (reservation != null)
			{
				var today = DateTime.Now.Date;
				var reservationDate = new DateTime(reservation.Year, reservation.Month, reservation.Day);

				var reservationOnSameDay = await GetReservationByDateAndBoatIdAsync(reservation.Day, reservation.Month, reservation.Year, reservation.BoatId);

				if (reservationOnSameDay?.ReservationId == reservation.ReservationId || reservationOnSameDay == null)
				{
					// Calculate the difference in days between today and the reservation date
					var daysDifference = (reservationDate - today).TotalDays;

					// If the reservation is exactly 3 days away and it's not already confirmed
					if (daysDifference <= 3 && reservation.Status != "Confirmed")
					{
						reservation.Status = "Unconfirmed";
                        await _reservationDataService.UpdateReservation(reservation);
                    } else if (daysDifference <= 1 && reservation.Status == "Unconfirmed")
					{
						//TODO fix this, we need to delete
						//await DeleteReservationAsync(reservation.ReservationId);
					}
				} else
				{
					return;
				}
			}
		}

		public async Task ConfirmReservation(Reservation reservation)
        {
            if (reservation != null && reservation.Status == "Unconfirmed")
            {
                reservation.Status = "Confirmed";
				await _reservationDataService.UpdateReservation(reservation);
			}
		}

		public bool HasReservationExpired(Reservation reservation)
		{
			var today = DateTime.UtcNow.Date; // Get today's date (without time)
			var reservationDate = new DateTime(reservation.Year, reservation.Month, reservation.Day); // reservation date with time set to midnight (00:00:00)

			// Reservation is considered expired if today's date is greater than or equal to the reservation date + 1 day
			return today >= reservationDate.AddDays(1);
		}


		// Method to restore quotas if the reservation date has passed
		public async Task RestoreQuotas(Reservation reservation)
        {
            if (HasReservationExpired(reservation))
			{
				// Fetch the user associated with the reservation
				var user = await _userService.GetUserByIdAsync(reservation.UserId) ?? throw new Exception("User not found");

				// Add quotas back based on reservation type
				await _userService.AddQuotasBack(user, reservation);
			}
		}

        // Check if the date is available for the selected boat
        public async Task<bool> IsDateAvailableForBoat(string boatId, int year, int month, int day)
        {
            var reservations = await GetReservationsByBoatIdAsync(boatId);
            foreach (var res in reservations)
            {
                if (res.Year == year && res.Month == month && res.Day == day)
                {
                    return false; // Date already booked
                }
            }
            return true;
        }

        public async Task<List<Reservation>> GetOccupiedDatesForYearAsync(int year)
        {
            // Fetch all reservations from DynamoDB
            var scanCondition = new List<ScanCondition>
            {
                //new ScanCondition("BoatId", ScanOperator.Equal, boatId),
                new ScanCondition("Year", ScanOperator.Equal, year),
                //new ScanCondition("Month", ScanOperator.Equal, month),
            };
            var reservations = await _context.ScanAsync<Reservation>(scanCondition).GetRemainingAsync();
            return reservations;
        }

		// Get reservations by boat ID
		public async Task<List<Reservation>> GetReservationsByBoatIdAsync(string boatId)
		{
			var conditions = new List<ScanCondition>
			{
				new ScanCondition("BoatId", ScanOperator.Equal, boatId)
			};

			var reservations = await _reservationDataService.GetReservationsByConditionsAsync(conditions);
			return reservations ?? new List<Reservation>();
		}

		// Get reservation by user ID
		public async Task<List<Reservation>> GetReservationsByUserIdAsync(string userId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("UserId", ScanOperator.Equal, userId)
            };

			return await _context.ScanAsync<Reservation>(conditions).GetRemainingAsync();
		}

		public async Task<Reservation> GetReservationByDateAndBoatIdAsync(int day, int month, int year, string boatId)
		{
			// Query DynamoDB to find the reservation by date
			var conditions = new List<ScanCondition>
			{
				new ScanCondition("Day", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, day),
				new ScanCondition("Month", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, month),
				new ScanCondition("Year", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, year),
				new ScanCondition("BoatId", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, boatId)
			};

			var search = _context.ScanAsync<Reservation>(conditions);
			var results = await search.GetRemainingAsync();

			// Order by CreatedAtIsoDate to get the earliest created reservation
			var orderedResults = results
				.OrderBy(r => DateTime.ParseExact(r.CreatedAtIsoDate, "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)) // Parse with the custom format
				.ToList();


			// Return the first reservation, if any
			return orderedResults.FirstOrDefault();
		}

		public async Task<List<Reservation>> GetReservationsForDateAndBoatIdAsync(int day, int month, int year, string boatId)
		{
			// Query DynamoDB to find the reservation by date
			var conditions = new List<ScanCondition>
			{
				new ScanCondition("Day", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, day),
				new ScanCondition("Month", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, month),
				new ScanCondition("Year", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, year),
				new ScanCondition("BoatId", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, boatId)
			};

			var search = _context.ScanAsync<Reservation>(conditions);
			var results = await search.GetRemainingAsync();

			// Order by CreatedAtIsoDate to get the earliest created reservation
			return results
				.OrderBy(r => DateTime.ParseExact(r.CreatedAtIsoDate, "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)) // Parse with the custom format
				.ToList();
		}

		public async Task UpdateAllReservations()
		{
			// Fetch all reservations
			var reservations = await _reservationDataService.GetAllReservationsAsync();

			// Iterate through each reservation and check if it should be confirmed
			foreach (var res in reservations)
			{
				if (res != null)
				{
					// Call CheckConfirmation for each reservation
					await CheckConfirmation(res);
				}
			}
		}

	}
}
