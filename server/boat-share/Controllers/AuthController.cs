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

        private bool VerifyPassword(string password, string hash)
        {
            // Use BCrypt for proper password verification
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key-that-is-at-least-32-characters-long"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: "BoatShare",
                audience: "BoatShare",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
