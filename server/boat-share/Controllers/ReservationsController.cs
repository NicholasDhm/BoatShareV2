using Amazon.DynamoDBv2.DataModel;
using boat_share.Models;
using boat_share.Services;
using boat_share.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace boat_share.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService _reservationService;
        private readonly DeleteAllPastReservationsUseCase _deleteAllPastReservationsUseCase;
        private readonly DeleteReservationUseCase _deleteReservationUseCase;
        private readonly ReservationDataService _reservationDataService;

        public ReservationsController(
            ReservationService reservationService,
            DeleteAllPastReservationsUseCase deleteAllPastReservationsUseCase,
            DeleteReservationUseCase deleteReservationUseCase,
            ReservationDataService reservationDataService
        )
        {
            _reservationService = reservationService;
            _deleteAllPastReservationsUseCase = deleteAllPastReservationsUseCase;
            _deleteReservationUseCase = deleteReservationUseCase;
            _reservationDataService = reservationDataService;
        }

        // POST: api/reservation/add
        [HttpPost("add")]
        public async Task<IActionResult> AddReservation([FromBody] ReservationDBO reservationDbo)
        {
            var result = await _reservationService.AddReservation(reservationDbo);

            if (result == "Reservation successfully created.")
            {
                return Ok(new { message = result });
            }
            else
            {
                return BadRequest(new { message = result });
            }
        }

        // POST: api/reservation/restore-quotas
        [HttpPost("restore-quotas")]
        public async Task<IActionResult> RestoreQuotas([FromBody] Reservation reservation)
        {
            await _reservationService.RestoreQuotas(reservation);
            return Ok(new { message = "Quotas restored if applicable." });
		}

        // PUT: api/reservation/confirm-reservation
        [HttpPut("confirm-reservation")]
        public async Task<IActionResult> ConfirmReservation([FromBody] Reservation reservation)
        {
            await _reservationService.ConfirmReservation(reservation);
            return Ok(new { message = "Reservation confirmed." });
        }

		// GET all reservations
		[HttpGet]
		public async Task<ActionResult<List<Reservation>>> GetAllReservations()
		{
			var reservations = await _reservationDataService.GetAllReservationsAsync();
			return Ok(reservations);
		}

		//GET Reservation by boat id
		[HttpGet("boat/{boatId}")]
		public async Task<ActionResult<List<Reservation>>> GetReservationsByBoatId(string boatId)
		{
			var reservations = await _reservationService.GetReservationsByBoatIdAsync(boatId);
			if (reservations == null)
			{
				return NotFound();
			}
			return Ok(reservations);
		}

		//GET Reservation by user id
		[HttpGet("user/{userId}")]
		public async Task<ActionResult<List<Reservation>>> GetReservationsByUserId(string userId)
		{
			var reservations = await _reservationService.GetReservationsByUserIdAsync(userId);
			
			return Ok(reservations);
		}

		// GET Reservation by date and boatId
		[HttpGet("by-date-and-boatId")]
		public async Task<IActionResult> GetReservationByDateAndBoatId(int day, int month, int year, string boatId)
		{
			var reservation = await _reservationService.GetReservationByDateAndBoatIdAsync(day, month, year, boatId);
			if (reservation == null)
			{
				return NotFound("No reservation found for the specified date.");
			}
			return Ok(reservation);
		}

		[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(string id)
        {
            await _deleteReservationUseCase.Execute(id);
            return NoContent();
        }

        [HttpGet("occupied/year/{year}")]
        public async Task<IActionResult> GetOccupiedDatesForYear(int year)
        {
            try
            {
                // Call the service to get occupied dates for the entire year
                var occupiedDates = await _reservationService.GetOccupiedDatesForYearAsync(year);

                // Return the dates as JSON
                return Ok(occupiedDates);
            }
            catch (Exception ex)
            {
                // Handle errors
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete-past")]
        public async Task<IActionResult> DeleteAllPastReservations()
        {
            await _deleteAllPastReservationsUseCase.Execute();
            return NoContent();
        }


		[HttpPut("update-reservations")]
		public async Task<IActionResult> UpdateAllReservations()
		{
			try
			{
				// Call the service to update all reservations
				await _reservationService.UpdateAllReservations();

				// Return success response
				return Ok("All reservations updated successfully.");
			}
			catch (Exception ex)
			{
				// In case of an error, return an error response
				return StatusCode(500, $"An error occurred while updating reservations: {ex.Message}");
			}
		}

	}
}
