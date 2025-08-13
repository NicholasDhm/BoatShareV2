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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserListDTO>>> GetUsers()
        {
            try
            {
                var users = await _userService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInfoDTO>> GetUser(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only access their own data unless they're admin
                if (currentUserRole != "Admin" && currentUserId != id)
                {
                    return Forbid("You can only access your own user data");
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new user (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserInfoDTO>> CreateUser(UserCreateDTO userCreateDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(userCreateDto);
                if (user == null)
                {
                    return BadRequest(new { message = "Failed to create user. Email may already exist or boat may not exist." });
                }

                var userInfo = await _userService.GetUserByIdAsync(user.UserId);
                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the user", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserInfoDTO>> UpdateUser(int id, UserUpdateDTO userUpdateDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only update their own data unless they're admin
                if (currentUserRole != "Admin" && currentUserId != id)
                {
                    return Forbid("You can only update your own user data");
                }

                // Non-admin users cannot change role or quotas
                if (currentUserRole != "Admin")
                {
                    userUpdateDto.Role = null;
                    userUpdateDto.StandardQuota = null;
                    userUpdateDto.SubstitutionQuota = null;
                    userUpdateDto.ContingencyQuota = null;
                    userUpdateDto.IsActive = null;
                }

                var updatedUser = await _userService.UpdateUserAsync(id, userUpdateDto);
                if (updatedUser == null)
                {
                    return NotFound(new { message = "User not found or update failed" });
                }

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user", error = ex.Message });
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
