using boat_share.Abstract;
using boat_share.Data;
using boat_share.DTOs;
using boat_share.Models;
using Microsoft.EntityFrameworkCore;

namespace boat_share.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserListDTO>> GetUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Boat)
                .Select(u => new UserListDTO
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Name = u.Name,
                    Role = u.Role,
                    BoatId = u.BoatId,
                    BoatName = u.Boat != null ? u.Boat.Name : null,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<UserInfoDTO?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Boat)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            return new UserInfoDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                BoatId = user.BoatId,
                BoatName = user.Boat?.Name,
                StandardQuota = user.StandardQuota,
                SubstitutionQuota = user.SubstitutionQuota,
                ContingencyQuota = user.ContingencyQuota,
                TotalQuotas = user.TotalQuotas,
                IsActive = user.IsActive
            };
        }

        public async Task<User?> CreateUserAsync(UserCreateDTO userCreateDto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == userCreateDto.Email))
            {
                return null; // Email already exists
            }

            // Check if boat exists
            if (!await _context.Boats.AnyAsync(b => b.BoatId == userCreateDto.BoatId))
            {
                return null; // Boat doesn't exist
            }

            var user = new User
            {
                Email = userCreateDto.Email,
                Name = userCreateDto.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password),
                Role = userCreateDto.Role,
                BoatId = userCreateDto.BoatId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<UserInfoDTO?> UpdateUserAsync(int userId, UserUpdateDTO userUpdateDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(userUpdateDto.Email))
            {
                // Check if new email already exists for another user
                if (await _context.Users.AnyAsync(u => u.Email == userUpdateDto.Email && u.UserId != userId))
                {
                    return null; // Email already exists
                }
                user.Email = userUpdateDto.Email;
            }

            if (!string.IsNullOrEmpty(userUpdateDto.Name))
                user.Name = userUpdateDto.Name;

            if (userUpdateDto.BoatId.HasValue)
            {
                // Check if boat exists
                if (!await _context.Boats.AnyAsync(b => b.BoatId == userUpdateDto.BoatId.Value))
                {
                    return null; // Boat doesn't exist
                }
                user.BoatId = userUpdateDto.BoatId.Value;
            }

            if (!string.IsNullOrEmpty(userUpdateDto.Role))
                user.Role = userUpdateDto.Role;

            if (userUpdateDto.StandardQuota.HasValue)
                user.StandardQuota = userUpdateDto.StandardQuota.Value;

            if (userUpdateDto.SubstitutionQuota.HasValue)
                user.SubstitutionQuota = userUpdateDto.SubstitutionQuota.Value;

            if (userUpdateDto.ContingencyQuota.HasValue)
                user.ContingencyQuota = userUpdateDto.ContingencyQuota.Value;

            if (userUpdateDto.IsActive.HasValue)
                user.IsActive = userUpdateDto.IsActive.Value;

            user.MarkAsUpdated();
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
