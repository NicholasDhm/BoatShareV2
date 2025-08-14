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

        public async Task<ReservationResponseDTO?> GetReservationByDateAndBoatIdAsync(DateTime date, int boatId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Boat)
                .Where(r => r.BoatId == boatId && r.StartTime.Date == date.Date && r.Status != "Cancelled")
                .OrderBy(r => r.CreatedAt) // Get the first reservation for this date
                .FirstOrDefaultAsync();

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
            // Get user to check quotas
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Check if user has sufficient quota for this reservation type
            if (!user.HasSufficientQuota(createReservationDto.ReservationType))
            {
                throw new InvalidOperationException($"Insufficient {createReservationDto.ReservationType} quota. Available: {GetUserQuotaForType(user, createReservationDto.ReservationType)}");
            }

            // Get boat to calculate cost
            var boat = await _context.Boats.FindAsync(createReservationDto.BoatId);
            if (boat == null)
                throw new ArgumentException("Boat not found");

            var duration = (createReservationDto.EndTime - createReservationDto.StartTime).TotalHours;
            var totalCost = (decimal)duration * boat.HourlyRate;

            // Determine initial status based on business rules
            var initialStatus = await DetermineReservationStatus(createReservationDto.StartTime, createReservationDto.BoatId, userId);

            // Deduct quota from user
            if (!user.DeductQuota(createReservationDto.ReservationType))
            {
                throw new InvalidOperationException($"Failed to deduct {createReservationDto.ReservationType} quota");
            }

            var reservation = new Reservation
            {
                UserId = userId,
                BoatId = createReservationDto.BoatId,
                StartTime = createReservationDto.StartTime,
                EndTime = createReservationDto.EndTime,
                ReservationType = createReservationDto.ReservationType,
                Status = initialStatus,
                Notes = createReservationDto.Notes ?? string.Empty,
                TotalCost = totalCost
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        /// <summary>
        /// Gets the current quota count for a specific reservation type
        /// </summary>
        private int GetUserQuotaForType(User user, string reservationType)
        {
            return reservationType switch
            {
                "Standard" => user.StandardQuota,
                "Substitution" => user.SubstitutionQuota,
                "Contingency" => user.ContingencyQuota,
                _ => 0
            };
        }

        /// <summary>
        /// Determines the initial status of a reservation based on business rules
        /// </summary>
        private async Task<string> DetermineReservationStatus(DateTime reservationDate, int boatId, int userId)
        {
            // Check if there's already a reservation for this date and boat
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.BoatId == boatId && 
                                         r.StartTime.Date == reservationDate.Date && 
                                         r.Status != "Cancelled");

            if (existingReservation == null)
            {
                // No existing reservation - check if we're in confirmation period (7 days before)
                var daysUntilReservation = (reservationDate.Date - DateTime.UtcNow.Date).TotalDays;
                
                if (daysUntilReservation <= 7)
                {
                    // Within confirmation period - set to Unconfirmed for the owner to confirm
                    return "Unconfirmed";
                }
                else
                {
                    // Outside confirmation period - set to Pending
                    return "Pending";
                }
            }
            else
            {
                // There's already a reservation - this becomes a substitution/queue reservation
                return "Pending";
            }
        }

        /// <summary>
        /// Updates reservation statuses based on current date and business rules
        /// </summary>
        public async Task UpdateReservationStatusesAsync()
        {
            var pendingReservations = await _context.Reservations
                .Where(r => r.Status == "Pending" && r.StartTime > DateTime.UtcNow)
                .ToListAsync();

            foreach (var reservation in pendingReservations)
            {
                var daysUntilReservation = (reservation.StartTime.Date - DateTime.UtcNow.Date).TotalDays;
                
                // Check if reservation owner and within confirmation period
                if (daysUntilReservation <= 7)
                {
                    // Check if this user is the "first" reservation (not in queue)
                    var isFirstReservation = await _context.Reservations
                        .Where(r => r.BoatId == reservation.BoatId && 
                                   r.StartTime.Date == reservation.StartTime.Date &&
                                   r.Status != "Cancelled")
                        .OrderBy(r => r.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (isFirstReservation?.ReservationId == reservation.ReservationId)
                    {
                        // This is the primary reservation and within confirmation period
                        reservation.Status = "Unconfirmed";
                        reservation.MarkAsUpdated();
                    }
                }
            }

            await _context.SaveChangesAsync();
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

            // Get user to restore quota
            var user = await _context.Users.FindAsync(reservation.UserId);
            if (user != null)
            {
                // Restore quota back to user
                user.RestoreQuota(reservation.ReservationType);
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
