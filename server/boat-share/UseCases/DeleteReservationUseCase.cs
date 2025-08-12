using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using boat_share.Abstract;
using boat_share.Models;
using boat_share.Services;

namespace boat_share.UseCases
{
    public class DeleteReservationUseCase
    {
        private readonly ReservationDataService _reservationDataService;
        private readonly ReservationService _reservationService;
        private readonly UserService _userService;

        public DeleteReservationUseCase(
            ReservationDataService reservationDataService,
            ReservationService reservationService,
            UserService userService
        )
        {
            _reservationDataService = reservationDataService;
            _reservationService = reservationService;
            _userService = userService;
        }

        public async Task Execute(string reservationId)
        {
            var reservation = await _reservationDataService.GetReservationAsync(reservationId);

            // Fetch the user associated with the reservation
            var user = await _userService.GetUserByIdAsync(reservation.UserId) ?? throw new Exception("User not found");

            // Add quotas back based on reservation type
            await _userService.AddQuotasBack(user, reservation);

            // Check for any substitution reservations on the same day
            var hasSubstitutions = await CheckForSubstitutions(reservation);

            if (hasSubstitutions)
            {
                // Fetch all substitutions for the same date and update their status to 'Pending'
                await ConfirmSubstitutionsForDate(reservation);
            }

            await _reservationDataService.DeleteReservation(reservationId);
        }

        private async Task<bool> CheckForSubstitutions(Reservation reservation)
        {
            // Create a DynamoDB query to search for reservations with the same BoatId, Year, Month, Day, and Type = "Substitution"
            var searchConditions = new List<ScanCondition>
            {
                new ScanCondition("BoatId", ScanOperator.Equal, reservation.BoatId),
                new ScanCondition("Year", ScanOperator.Equal, reservation.Year),
                new ScanCondition("Month", ScanOperator.Equal, reservation.Month),
                new ScanCondition("Day", ScanOperator.Equal, reservation.Day),
                new ScanCondition("Type", ScanOperator.Equal, "Substitution")
            };

            var results = await _reservationDataService.GetReservationsByConditionsAsync(searchConditions);

            return results.Any();
        }

        // Method to confirm substitution reservations for the same boat and date
        private async Task ConfirmSubstitutionsForDate(Reservation reservation)
        {
            // Get all reservations for the same boat and date
            var reservationsForDate = await _reservationService.GetReservationsForDateAndBoatIdAsync(
                reservation.Day, reservation.Month, reservation.Year, reservation.BoatId);

            // Ensure that the list has at least two elements before accessing the second one
            if (reservationsForDate.Count > 1)
            {
                var firstSubstitutionInQueue = reservationsForDate[1];

                // If the substitution exists, update to pending
                if (firstSubstitutionInQueue != null)
                {
                    // Update the status of the substitution to 'Pending'
                    firstSubstitutionInQueue.Status = "Pending";
                    await _reservationDataService.UpdateReservation(reservation);
                    await _reservationService.CheckConfirmation(firstSubstitutionInQueue);
                }
            }
        }
    }
}
