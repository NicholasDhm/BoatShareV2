using boat_share.Models;
using boat_share.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace boat_share.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> GetUserById(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
		}
        
        // PUT: api/user/{userId}
		[HttpPut("{userId}")]
		public async Task<IActionResult> UpdateUser(string userId, [FromBody] User user)
		{
			if (user == null || userId != user.UserId)
			{
				return BadRequest("User details are invalid or UserId mismatch.");
			}

			await _userService.UpdateUser(user);

            return NoContent();
		}


		[HttpPut("{userId}/add-quotas")]
        public async Task<IActionResult> AddQuotasBack(string userId, [FromBody] Reservation reservation)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Add quotas back
            await _userService.AddQuotasBack(user, reservation);

            return Ok(user); // Return the updated user or a success message
        }

        // GET: api/users/search/{partialName}
        [HttpGet("search/{partialName}")]
        public async Task<ActionResult<List<User>>> GetUsersByName(string partialName)
        {
            var users = await _userService.GetUsersByPartialNameAsync(partialName);
            if (users == null || users.Count == 0)
            {
                return NotFound();
            }
            return Ok(users);
        }

        // DELETE: api/users/{userId}
        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteUser(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            await _userService.DeleteUserAsync(userId);
            return NoContent();
		}
	}
}
