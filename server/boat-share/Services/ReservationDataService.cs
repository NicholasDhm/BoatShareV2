using Amazon.DynamoDBv2.DataModel;
using boat_share.Models;

namespace boat_share.Services
{
    public class ReservationDataService
    {
        private readonly IDynamoDBContext _context;

        public ReservationDataService(IDynamoDBContext context)
        {
            _context = context;

        }

        public async Task<List<Reservation>> GetAllReservationsAsync()
        {
            return await _context.ScanAsync<Reservation>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task<List<Reservation>> GetReservationsByConditionsAsync(List<ScanCondition>  searchConditions)
        {
            var search = _context.ScanAsync<Reservation>(searchConditions);
            var result = await search.GetNextSetAsync();

            return result;
        }

        public async Task<Reservation> GetReservationAsync(string reservationId)
        {
            var reservation = await _context.LoadAsync<Reservation>(reservationId);

            if (reservation == null)
            {
                throw new Exception("Reservation not found");
            }
            return reservation;
        }

        public async Task DeleteReservation(string reservationId)
        {
            // Delete the reservation in DynamoDB
            await _context.DeleteAsync<Reservation>(reservationId);
        }

        public async Task UpdateReservation(Reservation reservation)
        {
            // Save the new reservation in DynamoDB
            await _context.SaveAsync(reservation);
        }
    }
}
