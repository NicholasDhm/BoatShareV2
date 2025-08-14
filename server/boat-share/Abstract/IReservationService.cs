using boat_share.Models;
using boat_share.DTOs;

namespace boat_share.Abstract
{
	public interface IReservationService
	{
		Task<List<ReservationResponseDTO>> GetReservationsAsync();
		Task<List<ReservationResponseDTO>> GetReservationsByUserIdAsync(int userId);
		Task<List<ReservationResponseDTO>> GetReservationsByBoatIdAsync(int boatId);
		Task<ReservationResponseDTO?> GetReservationByIdAsync(int reservationId);
		Task<ReservationResponseDTO?> GetReservationByDateAndBoatIdAsync(DateTime date, int boatId);
		Task<Reservation> CreateReservationAsync(CreateReservationDTO createReservationDto, int userId);
		Task<ReservationResponseDTO?> UpdateReservationAsync(int reservationId, ReservationDTO reservationDto);
		Task<bool> DeleteReservationAsync(int reservationId);
		Task UpdateReservationStatusesAsync();
	}
}
