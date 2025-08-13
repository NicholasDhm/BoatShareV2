using boat_share.Abstract;
using boat_share.Data;
using boat_share.DTOs;
using boat_share.Models;
using Microsoft.EntityFrameworkCore;

namespace boat_share.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReservationResponseDTO>> GetReservationsAsync()
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Boat)
                .Select(r => new ReservationResponseDTO
                {
                    ReservationId = r.ReservationId,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.Name : null,
                    BoatId = r.BoatId,
                    BoatName = r.Boat != null ? r.Boat.Name : null,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    DurationHours = r.DurationHours,
                    TotalCost = r.TotalCost,
                    Status = r.Status,
                    ReservationType = r.ReservationType,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<List<ReservationResponseDTO>> GetReservationsByUserIdAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Boat)
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationResponseDTO
                {
                    ReservationId = r.ReservationId,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.Name : null,
                    BoatId = r.BoatId,
                    BoatName = r.Boat != null ? r.Boat.Name : null,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    DurationHours = r.DurationHours,
                    TotalCost = r.TotalCost,
                    Status = r.Status,
                    ReservationType = r.ReservationType,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<List<ReservationResponseDTO>> GetReservationsByBoatIdAsync(int boatId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Boat)
                .Where(r => r.BoatId == boatId)
                .Select(r => new ReservationResponseDTO
                {
                    ReservationId = r.ReservationId,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.Name : null,
                    BoatId = r.BoatId,
                    BoatName = r.Boat != null ? r.Boat.Name : null,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    DurationHours = r.DurationHours,
                    TotalCost = r.TotalCost,
                    Status = r.Status,
                    ReservationType = r.ReservationType,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<ReservationResponseDTO?> GetReservationByIdAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Boat)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) return null;

            return new ReservationResponseDTO
            {
                ReservationId = reservation.ReservationId,
                UserId = reservation.UserId,
                UserName = reservation.User?.Name,
                BoatId = reservation.BoatId,
                BoatName = reservation.Boat?.Name,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                DurationHours = reservation.DurationHours,
                TotalCost = reservation.TotalCost,
                Status = reservation.Status,
                ReservationType = reservation.ReservationType,
                Notes = reservation.Notes,
                CreatedAt = reservation.CreatedAt,
                UpdatedAt = reservation.UpdatedAt
            };
        }

        public async Task<Reservation> CreateReservationAsync(CreateReservationDTO createReservationDto, int userId)
        {
            // Get boat to calculate cost
            var boat = await _context.Boats.FindAsync(createReservationDto.BoatId);
            if (boat == null)
                throw new ArgumentException("Boat not found");

            var duration = (createReservationDto.EndTime - createReservationDto.StartTime).TotalHours;
            var totalCost = (decimal)duration * boat.HourlyRate;

            var reservation = new Reservation
            {
                UserId = userId,
                BoatId = createReservationDto.BoatId,
                StartTime = createReservationDto.StartTime,
                EndTime = createReservationDto.EndTime,
                ReservationType = createReservationDto.ReservationType,
                Status = "Pending",
                Notes = createReservationDto.Notes ?? string.Empty,
                TotalCost = totalCost
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<ReservationResponseDTO?> UpdateReservationAsync(int reservationId, ReservationDTO reservationDto)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return null;

            reservation.StartTime = reservationDto.StartTime;
            reservation.EndTime = reservationDto.EndTime;
            reservation.ReservationType = reservationDto.ReservationType;
            reservation.Status = reservationDto.Status;
            reservation.Notes = reservationDto.Notes;

            // Recalculate cost if time changed
            var boat = await _context.Boats.FindAsync(reservation.BoatId);
            if (boat != null)
            {
                var duration = (reservation.EndTime - reservation.StartTime).TotalHours;
                reservation.TotalCost = (decimal)duration * boat.HourlyRate;
            }

            reservation.MarkAsUpdated();
            await _context.SaveChangesAsync();

            return await GetReservationByIdAsync(reservationId);
        }

        public async Task<bool> DeleteReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return false;

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
