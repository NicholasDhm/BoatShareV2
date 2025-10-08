using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boat_share.Data;
using boat_share.DTOs;
using boat_share.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace boat_share.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { Message = "Auth API is working" });
        }

        [HttpPost("create-test-user")]
        public async Task<IActionResult> CreateTestUser()
        {
            try
            {
                // Check if test user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");
                if (existingUser != null)
                {
                    return Ok(new { Message = "Test user already exists" });
                }

                // Create a test boat first
                var testBoat = new Boat
                {
                    Name = "Test Boat",
                    Type = "Sailboat",
                    Description = "A test boat for development",
                    Location = "Test Harbor",
                    Capacity = 6,
                    HourlyRate = 50.00m,
                    IsActive = true
                };

                _context.Boats.Add(testBoat);
                await _context.SaveChangesAsync();

                // Create test user with hashed password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword("test");
                var testUser = new User
                {
                    Email = "test@test.com",
                    Name = "Test User",
                    Role = "Member",
                    PasswordHash = hashedPassword,
                    BoatId = testBoat.BoatId,
                    StandardQuota = 10,
                    SubstitutionQuota = 5,
                    ContingencyQuota = 3,
                    IsActive = true
                };

                _context.Users.Add(testUser);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Test user created successfully", Email = "test@test.com", Password = "test" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error creating test user", Error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if user with this email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "A user with this email already exists" });
                }

                // Verify boat exists
                var boat = await _context.Boats.FindAsync(userDto.BoatId);
                if (boat == null)
                {
                    return BadRequest(new { message = "Invalid boat ID" });
                }

                // Check boat capacity
                var assignedUsers = await _context.Users.CountAsync(u => u.BoatId == userDto.BoatId && u.IsActive);
                if (assignedUsers >= boat.Capacity)
                {
                    return BadRequest(new { message = "This boat has reached its capacity" });
                }

                // Create new user with default quotas
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
                var newUser = new User
                {
                    Email = userDto.Email,
                    Name = userDto.Name,
                    Role = userDto.Role,
                    PasswordHash = hashedPassword,
                    BoatId = userDto.BoatId,
                    StandardQuota = 10,
                    SubstitutionQuota = 5,
                    ContingencyQuota = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);

                // Update boat's assigned users count
                boat.AssignedUsersCount++;
                boat.MarkAsUpdated();

                await _context.SaveChangesAsync();

                // Generate JWT token for automatic login
                var token = GenerateJwtToken(newUser);

                // Return the same response format as login
                var response = new AuthResponseDTO
                {
                    Token = token,
                    User = new UserInfoDTO
                    {
                        UserId = newUser.UserId,
                        Email = newUser.Email,
                        Name = newUser.Name,
                        Role = newUser.Role,
                        BoatId = newUser.BoatId,
                        BoatName = boat.Name,
                        StandardQuota = newUser.StandardQuota,
                        SubstitutionQuota = newUser.SubstitutionQuota,
                        ContingencyQuota = newUser.ContingencyQuota,
                        TotalQuotas = newUser.TotalQuotas,
                        IsActive = newUser.IsActive
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Find user by email
                var user = await _context.Users
                    .Include(u => u.Boat)
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Verify password (simplified for now - in production use proper password hashing)
                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                // Create response
                var response = new AuthResponseDTO
                {
                    Token = token,
                    User = new UserInfoDTO
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        Name = user.Name,
                        Role = user.Role,
                        BoatId = user.BoatId,
                        BoatName = user.Boat?.Name,
                        StandardQuota = user.StandardQuota,
                        SubstitutionQuota = user.SubstitutionQuota,
                        ContingencyQuota = user.ContingencyQuota
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        [HttpPut("new-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO updatePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _context.Users.FindAsync(updatePasswordDto.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Hash the new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatePasswordDto.NewPassword);
                user.MarkAsUpdated();

                await _context.SaveChangesAsync();

                return Ok(new { message = "Password updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating password" });
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            // Use BCrypt for proper password verification
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT secret key not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "BoatShare",
                audience: _configuration["Jwt:Audience"] ?? "BoatShare",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Reduced from 1 day to 1 hour
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
