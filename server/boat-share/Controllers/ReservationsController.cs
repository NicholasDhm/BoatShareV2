using boat_share.Abstract;
using boat_share.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace boat_share.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>
        /// Get all reservations (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ReservationResponseDTO>>> GetReservations()
        {
            try
            {
                var reservations = await _reservationService.GetReservationsAsync();
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving reservations", error = ex.Message });
            }
        }

        /// <summary>
        /// Get reservations by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ReservationResponseDTO>>> GetReservationsByUserId(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only access their own reservations unless they're admin
                if (currentUserRole != "Admin" && currentUserId != userId)
                {
                    return Forbid("You can only access your own reservations");
                }

                var reservations = await _reservationService.GetReservationsByUserIdAsync(userId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user reservations", error = ex.Message });
            }
        }

        /// <summary>
        /// Get reservations by boat ID
        /// </summary>
        [HttpGet("boat/{boatId}")]
        public async Task<ActionResult<List<ReservationResponseDTO>>> GetReservationsByBoatId(int boatId)
        {
            try
            {
                var reservations = await _reservationService.GetReservationsByBoatIdAsync(boatId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving boat reservations", error = ex.Message });
            }
        }

        /// <summary>
        /// Get reservation by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationResponseDTO>> GetReservation(int id)
        {
            try
            {
                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null)
                {
                    return NotFound(new { message = "Reservation not found" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only access their own reservations unless they're admin
                if (currentUserRole != "Admin" && reservation.UserId != currentUserId)
                {
                    return Forbid("You can only access your own reservations");
                }

                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the reservation", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new reservation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReservationResponseDTO>> CreateReservation(CreateReservationDTO createReservationDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var reservation = await _reservationService.CreateReservationAsync(createReservationDto, currentUserId);
                var reservationResponse = await _reservationService.GetReservationByIdAsync(reservation.ReservationId);

                return CreatedAtAction(nameof(GetReservation), new { id = reservation.ReservationId }, reservationResponse);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the reservation", error = ex.Message });
            }
        }

        /// <summary>
        /// Update reservation
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ReservationResponseDTO>> UpdateReservation(int id, ReservationDTO reservationDto)
        {
            try
            {
                var existingReservation = await _reservationService.GetReservationByIdAsync(id);
                if (existingReservation == null)
                {
                    return NotFound(new { message = "Reservation not found" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only update their own reservations unless they're admin
                if (currentUserRole != "Admin" && existingReservation.UserId != currentUserId)
                {
                    return Forbid("You can only update your own reservations");
                }

                var updatedReservation = await _reservationService.UpdateReservationAsync(id, reservationDto);
                if (updatedReservation == null)
                {
                    return NotFound(new { message = "Reservation not found" });
                }

                return Ok(updatedReservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the reservation", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete reservation
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            try
            {
                var existingReservation = await _reservationService.GetReservationByIdAsync(id);
                if (existingReservation == null)
                {
                    return NotFound(new { message = "Reservation not found" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only delete their own reservations unless they're admin
                if (currentUserRole != "Admin" && existingReservation.UserId != currentUserId)
                {
                    return Forbid("You can only delete your own reservations");
                }

                var success = await _reservationService.DeleteReservationAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Reservation not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the reservation", error = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
        }
    }
}
