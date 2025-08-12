using boat_share.Models;
using boat_share.Services;
using Microsoft.AspNetCore.Mvc;

namespace boat_share.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoatsController : ControllerBase
    {
        private readonly BoatService _boatService;

        public BoatsController(BoatService boatService)
        {
            _boatService = boatService;
        }

        // GET: api/boats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Boat>>> GetBoats()
        {
            var boats = await _boatService.GetBoatsAsync();
            return Ok(boats);
        }

        // GET: api/boats/{boatId}
        [HttpGet("{boatId}")]
        public async Task<ActionResult<Boat>> GetBoatByBoatId(string boatId)
        {
            var boat = await _boatService.GetBoatByIdAsync(boatId);

            if (boat == null)
            {
                return NotFound($"Boat with ID {boatId} not found.");
            }

            return Ok(boat);
        }

		// POST: api/boats
		[HttpPost]
		public async Task<ActionResult<Boat>> AddBoat([FromBody] Boat newBoat)
		{
			if (newBoat == null)
			{
				return BadRequest("Invalid boat details.");
			}

			// Generate a new Guid if the BoatId is empty or null
			if (string.IsNullOrEmpty(newBoat.BoatId))
			{
				newBoat.BoatId = Guid.NewGuid().ToString();
			}

			await _boatService.AddBoatAsync(newBoat);
			return CreatedAtAction(nameof(GetBoatByBoatId), new { boatId = newBoat.BoatId }, newBoat);
		}

		// DELETE: api/boats/{boatId}
		[HttpDelete("{boatId}")]
        public async Task<IActionResult> DeleteBoat(string boatId)
        {
            var boat = await _boatService.GetBoatByIdAsync(boatId);

            if (boat == null)
            {
                return NotFound($"Boat with ID {boatId} not found.");
            }

            await _boatService.DeleteBoatAsync(boatId);
            return NoContent();
        }

        // PUT: api/boats/{boatId}
        [HttpPut("{boatId}")]
        public async Task<IActionResult> UpdateBoat(string boatId, [FromBody] Boat updatedBoat)
        {
            if (boatId != updatedBoat.BoatId)
            {
                return BadRequest("Boat ID mismatch.");
            }

            var boat = await _boatService.GetBoatByIdAsync(boatId);
            if (boat == null)
            {
                return NotFound($"Boat with ID {boatId} not found.");
            }

            await _boatService.UpdateBoatAsync(updatedBoat);
            return NoContent();
        }
    }
}
