using Microsoft.AspNetCore.Mvc;
using boat_share.Services;
using boat_share.Models;

namespace boat_share.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Password))
            {
                return BadRequest("Invalid request");
            }

            var result = await _authService.RegisterUserAsync(userDto);
            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                var (token, user) = await _authService.LoginAsync(loginModel.Email, loginModel.Password);
                return Ok(new { token, user });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

		[HttpPut("new-password")]
		public async Task<IActionResult> UpdateUserPassword([FromBody] UpdatePasswordModel model)
		{
			await _authService.UpdateUserPassword(model.UserId, model.NewPassword);

			return NoContent();
		}

	}

	public class LoginModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

	public class UpdatePasswordModel
	{
		public string UserId { get; set; }
		public string NewPassword { get; set; }
	}

}
