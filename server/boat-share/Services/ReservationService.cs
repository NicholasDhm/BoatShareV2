using boat_share.Abstract;
using boat_share.Data;
using boat_share.DTOs;
using boat_share.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace boat_share.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Centralized DTO mapping expression to eliminate code duplication
        /// </summary>
        private static Expression<Func<Reservation, ReservationResponseDTO>> MapToResponseDTO()
        {
            return r => new ReservationResponseDTO
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
            };
        }

        /// <summary>
        /// Get base query with includes for common use
        /// </summary>
        private IQueryable<Reservation> GetBaseReservationsQuery()
        {
            return _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Boat);
        }

        public async Task<List<ReservationResponseDTO>> GetReservationsAsync()
        {
            return await GetBaseReservationsQuery()
                .Select(MapToResponseDTO())
                .ToListAsync();
        }

        public async Task<List<ReservationResponseDTO>> GetReservationsByUserIdAsync(int userId)
        {
            return await GetBaseReservationsQuery()
                .Where(r => r.UserId == userId)
                .Select(MapToResponseDTO())
                .ToListAsync();
        }

        public async Task<List<ReservationResponseDTO>> GetReservationsByBoatIdAsync(int boatId)
        {
            // Filter out Legacy and Cancelled for calendar views
            return await GetBaseReservationsQuery()
                .Where(r => r.BoatId == boatId && r.Status != "Legacy" && r.Status != "Cancelled")
                .Select(MapToResponseDTO())
                .ToListAsync();
        }

        /// <summary>
        /// Get legacy (historical) reservations by user ID
        /// </summary>
        public async Task<List<ReservationResponseDTO>> GetLegacyReservationsByUserIdAsync(int userId)
        {
            return await GetBaseReservationsQuery()
                .Where(r => r.UserId == userId && r.Status == "Legacy")
                .OrderByDescending(r => r.StartTime)
                .Select(MapToResponseDTO())
                .ToListAsync();
        }

        /// <summary>
        /// Get legacy (historical) reservations by boat ID
        /// </summary>
        public async Task<List<ReservationResponseDTO>> GetLegacyReservationsByBoatIdAsync(int boatId)
        {
            return await GetBaseReservationsQuery()
                .Where(r => r.BoatId == boatId && r.Status == "Legacy")
                .OrderByDescending(r => r.StartTime)
                .Select(MapToResponseDTO())
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
            var reservation = await GetBaseReservationsQuery()
                .Where(r => r.BoatId == boatId &&
                           r.StartTime.Date == date.Date &&
                           r.Status != "Cancelled" &&
                           r.Status != "Legacy")  // Exclude Legacy from calendar lookups
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (reservation == null) return null;

            return MapToResponseDTO().Compile()(reservation);
        }

        public async Task<Reservation> CreateReservationAsync(CreateReservationDTO createReservationDto, int userId)
        {
            // Use transaction with pessimistic locking to prevent quota race conditions
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Lock user row for update (prevents concurrent quota modifications)
                var user = await _context.Users
                    .FromSqlRaw(@"SELECT * FROM ""Users"" WHERE ""UserId"" = {0} FOR UPDATE", userId)
                    .FirstOrDefaultAsync();

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

                // Determine initial status based on business rules (Contingency auto-confirms)
                var initialStatus = await DetermineReservationStatus(
                    createReservationDto.StartTime,
                    createReservationDto.BoatId,
                    userId,
                    createReservationDto.ReservationType);

                // Deduct quota from user (now safe from race conditions)
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
                await transaction.CommitAsync();

                return reservation;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
        private async Task<string> DetermineReservationStatus(DateTime reservationDate, int boatId, int userId, string reservationType)
        {
            // Contingency reservations always auto-confirm
            if (reservationType == "Contingency")
            {
                return "Confirmed";
            }

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
        /// Includes Legacy archival for past reservations
        /// </summary>
        public async Task UpdateReservationStatusesAsync()
        {
            var now = DateTime.UtcNow;

            // 1. Archive completed reservations to Legacy status and restore quotas
            var completedReservations = await _context.Reservations
                .Include(r => r.User)  // Include user to restore quota
                .Where(r => r.EndTime < now &&
                           r.Status != "Legacy" &&
                           r.Status != "Cancelled")
                .ToListAsync();

            foreach (var reservation in completedReservations)
            {
                reservation.Status = "Legacy";
                reservation.MarkAsUpdated();

                // Restore quota when reservation completes
                if (reservation.User != null)
                {
                    reservation.User.RestoreQuota(reservation.ReservationType);
                }
            }

            // 2. Update Pending → Unconfirmed (optimized to avoid N+1 queries)
            var pendingReservations = await _context.Reservations
                .Where(r => r.Status == "Pending" && r.StartTime > now)
                .ToListAsync();

            if (pendingReservations.Any())
            {
                // Pre-fetch first reservations for all boat/date combinations in ONE query
                var boatDatePairs = pendingReservations
                    .Select(r => new { r.BoatId, Date = r.StartTime.Date })
                    .Distinct()
                    .ToList();

                var firstReservationMap = await _context.Reservations
                    .Where(r => r.Status != "Cancelled" && r.Status != "Legacy")
                    .GroupBy(r => new { r.BoatId, Date = r.StartTime.Date })
                    .Select(g => new
                    {
                        g.Key.BoatId,
                        g.Key.Date,
                        FirstReservationId = g.OrderBy(r => r.CreatedAt).First().ReservationId
                    })
                    .ToDictionaryAsync(x => new { x.BoatId, x.Date }, x => x.FirstReservationId);

                foreach (var reservation in pendingReservations)
                {
                    var daysUntilReservation = (reservation.StartTime.Date - now.Date).TotalDays;

                    if (daysUntilReservation <= 7)
                    {
                        var key = new { reservation.BoatId, Date = reservation.StartTime.Date };

                        if (firstReservationMap.TryGetValue(key, out var firstId) &&
                            firstId == reservation.ReservationId)
                        {
                            reservation.Status = "Unconfirmed";
                            reservation.MarkAsUpdated();
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ReservationResponseDTO?> UpdateReservationAsync(int reservationId, ReservationDTO reservationDto)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return null;

            // Prevent modifications to Legacy reservations
            if (reservation.Status == "Legacy")
            {
                throw new InvalidOperationException("Cannot modify a Legacy reservation. Historical data is read-only.");
            }

            // Prevent manual transition TO Legacy status
            if (reservationDto.Status == "Legacy" && reservation.Status != "Legacy")
            {
                throw new InvalidOperationException("Cannot manually set reservation to Legacy status. Use automated archival process.");
            }

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

            // Prevent deletion of confirmed reservations
            if (reservation.Status == "Confirmed")
            {
                throw new InvalidOperationException("Cannot delete a confirmed reservation. Please cancel it first if allowed.");
            }

            // Prevent deletion of legacy reservations (historical data protection)
            if (reservation.Status == "Legacy")
            {
                throw new InvalidOperationException("Cannot delete a Legacy reservation. Historical records are immutable.");
            }

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
