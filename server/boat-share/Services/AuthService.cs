using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using boat_share.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.HttpResults;

namespace boat_share.Services
{
    public class AuthService
    {
        private readonly DynamoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly BoatService _boatService;
        private readonly UserService _userService;

        public AuthService(IAmazonDynamoDB dynamoDBClient, IConfiguration configuration, BoatService boatService, UserService userService)
        {
            _context = new DynamoDBContext(dynamoDBClient);
            _configuration = configuration;
            _boatService = boatService;
            _userService = userService;

        }
        public async Task<User> RegisterUserAsync(UserDTO userDto)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password); // Hash the password

            var user = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = userDto.Email,
                Name = userDto.Name,
                Role = "Member",
                StandardQuota = 2,
                SubstitutionQuota = 2,
                ContingencyQuota = 1,
                BoatId = userDto.BoatId,
                PasswordHash = passwordHash
            };

            await _boatService.AssignUserToBoatAsync(user.BoatId, user.UserId);
			await _context.SaveAsync(user);
            return user;
        }

        public async Task<User> UpdateUserPassword(string userId, string newPassword)
        {
            var userToUpdate = await _userService.GetUserByIdAsync(userId);

            if (userToUpdate == null)
            {
                throw new UnauthorizedAccessException("User Not Found");
            }

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

			userToUpdate.PasswordHash = newPasswordHash;

            await _userService.UpdateUser(userToUpdate);

            return userToUpdate;
        }

        public async Task<(string, User)> LoginAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var token = GenerateJwtToken(user);
            return (token, user);
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user.UserId),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<User> GetUserByEmailAsync(string email)
        {
            var searchConditions = new List<ScanCondition>
            {
                new ScanCondition(nameof(User.Email), ScanOperator.Equal, email)
            };

            var searchResults = await _context.ScanAsync<User>(searchConditions).GetRemainingAsync();
            return searchResults.FirstOrDefault();
        }
    }
}
