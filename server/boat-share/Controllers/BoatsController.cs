using boat_share.Abstract;
using boat_share.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boat_share.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoatsController : ControllerBase
    {
        private readonly IBoatService _boatService;

        public BoatsController(IBoatService boatService)
        {
            _boatService = boatService;
        }

        /// <summary>
        /// Get basic boat info for registration (public access)
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<List<object>>> GetPublicBoats()
        {
            try
            {
                var boats = await _boatService.GetBoatsAsync();
                // Return only minimal boat info for registration
                var publicBoats = boats
                    .Where(b => b.IsActive && (b.Capacity - b.AssignedUsersCount) > 0)
                    .Select(b => new
                    {
                        b.BoatId,
                        b.Name,
                        b.Capacity,
                        b.AssignedUsersCount
                    });
                return Ok(publicBoats);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving boats" });
            }
        }

        /// <summary>
        /// Get all boats (requires authentication)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<BoatDTO>>> GetBoats()
        {
            try
            {
                var boats = await _boatService.GetBoatsAsync();
                return Ok(boats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving boats", error = ex.Message });
            }
        }

        /// <summary>
        /// Get boat by ID (requires authentication)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BoatDTO>> GetBoat(int id)
        {
            try
            {
                var boat = await _boatService.GetBoatByIdAsync(id);
                if (boat == null)
                {
                    return NotFound(new { message = "Boat not found" });
                }

                return Ok(boat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the boat", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new boat (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BoatDTO>> CreateBoat(BoatCreateDTO boatCreateDto)
        {
            try
            {
                var boat = await _boatService.CreateBoatAsync(boatCreateDto);
                var boatDto = await _boatService.GetBoatByIdAsync(boat.BoatId);

                return CreatedAtAction(nameof(GetBoat), new { id = boat.BoatId }, boatDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the boat", error = ex.Message });
            }
        }

        /// <summary>
        /// Update boat (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BoatDTO>> UpdateBoat(int id, BoatUpdateDTO boatUpdateDto)
        {
            try
            {
                var updatedBoat = await _boatService.UpdateBoatAsync(id, boatUpdateDto);
                if (updatedBoat == null)
                {
                    return NotFound(new { message = "Boat not found" });
                }

                return Ok(updatedBoat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the boat", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete boat (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBoat(int id)
        {
            try
            {
                var success = await _boatService.DeleteBoatAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Boat not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the boat", error = ex.Message });
            }
        }

        /// <summary>
        /// Assign user to boat (Admin only)
        /// </summary>
        [HttpPost("{boatId}/assign/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignUserToBoat(int boatId, int userId)
        {
            try
            {
                var success = await _boatService.AssignUserToBoatAsync(boatId, userId);
                if (!success)
                {
                    return NotFound(new { message = "Boat or user not found" });
                }

                return Ok(new { message = "User successfully assigned to boat" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning user to boat", error = ex.Message });
            }
        }
    }
}
